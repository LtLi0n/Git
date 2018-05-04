using DataBaseFormats.Entities.Players;
using MUD_Server.Essentials.Commands;
using MUD_Server.Essentials.Framework;
using MUD_Server.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MUD_Server.Game.Entities.Players
{
    public class SocketUser
    {
        public string Username { get; set; }
        public string Email { get; set; }

        public Socket Socket { get; }

        public SocketCharacter OnlineCharacter { get; set; }

        public CommandNode ActiveCommandNode { get; private set; }
        private bool _commandNodeRecentRollback;
        public List<CommandNode> CinematicsUsed { get; }
        public bool IsInCinematic { get; set; }

        public IEnumerable<Character> Characters { get; set; }

        public List<string> Cache { get; private set; } 

        public string RecentInput { get; private set; }

        public SocketUser(Socket socket)
        {
            Characters = Enumerable.Empty<Character>();
            Socket = socket;

            Cache = new List<string>();

            CinematicsUsed = new List<CommandNode>();
        }

        public SocketUser(Socket socket, string username, string email)
        {
            Characters = Enumerable.Empty<Character>();

            Socket = socket;
            Username = username;
            Email = email;
        }

        public async Task SetActiveCommandNode(CommandNode cNode, bool showDescription = true, bool clearScreen = false, string reason = null)
        {
            if (reason != null)
            {
                _commandNodeRecentRollback = true;
                await Socket.SendMessageAsync(reason, clearScreen);
            }

            if (showDescription && cNode != null) await Socket.SendMessageAsync(await cNode.GetDynamicDescription(this), reason == null ? clearScreen : false, newLine: false);

            ActiveCommandNode = cNode;
        }

        public async Task HandleInput(string input, bool showNext = false, bool clearLast = false)
        {
            RecentInput = input;

            if (IsInCinematic) return;

            bool isCommand = false;

            if (ActiveCommandNode != null)
            {
                if (input.ToLower() == "back" && ActiveCommandNode.Parent != null && ActiveCommandNode.AllowsBackwards)
                {
                    //if input was provided
                    if (ActiveCommandNode.ParsableInput)
                    {
                        Cache.RemoveAt(Cache.Count - 1);
                    }
                    ActiveCommandNode.RunOptional_Backwards(this);
                    ActiveCommandNode = ActiveCommandNode.Parent;
                }
                else
                {
                    if(ActiveCommandNode.Commands != null)
                    {
                        string call = RecentInput.Split(' ')[0];

                        Command cmd = ActiveCommandNode.Commands.ToList().Find(x => x.Trigger.ToLower() == call.ToLower());

                        if(cmd != null)
                        {
                            await cmd.ExecuteAsync(new SocketMessageReceivedEventArgs(this, RecentInput.Substring(call.Length)));
                            isCommand = true;
                        }
                    }

                    if(!isCommand)
                    {
                        CommandNode cNode = ActiveCommandNode.Connections.Find(x => Regex.IsMatch(input, x.Regex));

                        if (cNode != null)
                        {
                            bool canSwap = true;

                            //Swap func is defined, run and see if user can switch to this node
                            if(cNode.SwapFunc != null)
                            {
                                canSwap = false;
                                if (await cNode.SwapFunc(this))
                                {
                                    canSwap = true;
                                }
                            }
                            //Proceed to swap
                            if (canSwap)
                            {
                                ActiveCommandNode = cNode;

                                if (ActiveCommandNode.ParsableInput)
                                {
                                    //Add input to the cache.
                                    Cache = new List<string>(Cache.Concat(ActiveCommandNode.ParseInput(input)));
                                }

                                ActiveCommandNode.RunOptional_Forward(this);

                                if (ActiveCommandNode == null) return;
                            }
                        }
                        else
                        {
                            //Display why user failed
                            if (ActiveCommandNode.RegexFailedExplanation != null)
                            {
                                await Socket.SendMessageAsync(ActiveCommandNode.RegexFailedExplanation + $"{Color.Parse("white")}\n\n" + await ActiveCommandNode.GetDynamicDescription(this), true, false);
                                return;
                            }
                        }
                    }
                }

                //If Active Node was recently set back, show reason message.
                if (_commandNodeRecentRollback)
                {
                    _commandNodeRecentRollback = false;
                }
                else
                {
                    if (clearLast && !isCommand)
                    {
                        //If refresh on enter disabled, refresh screen for cinematic one time display else refresh all the time.
                        if (!ActiveCommandNode.DisableRefreshOnEnter && !IsInCinematic)
                        {
                            await Socket.ClearScreen();
                        }
                    }
                    if (showNext)
                    {
                        if (ActiveCommandNode.IsCinematic && !CinematicsUsed.Contains(ActiveCommandNode))
                        {
                            new Thread(() => ActiveCommandNode.DisplayDescriptionCinematic(this).GetAwaiter().GetResult()).Start();
                        }
                        else
                        {
                            string desc;

                            try
                            {
                                desc = await ActiveCommandNode.GetDynamicDescription(this, filterCinematicColor: true);
                                await SendMessageAsync(desc, newLine: false);
                            }
                            catch (Exception e) { await SendMessageAsync(e.Message); }
                            
                        }
                    }

                    if (ActiveCommandNode.IsDestination)
                    {
                        ActiveCommandNode = null;
                    }
                }
            }
        }

        public async Task SendMessageAsync(object value, bool clearScreen = false, bool newLine = true, bool forceColorAtEnd = true) => await Socket.SendMessageAsync(value, clearScreen, newLine, forceColorAtEnd);

        //\x1b[2J doesn't work :(
        public async Task ClearScreen(bool simulate = false, int simulateCount = 100) =>
            await Task.Run(() => Socket.Send(Encoding.ASCII.GetBytes(simulate ?
               new string(new char[simulateCount]).Replace('\0', '\n') :
               "\x1b[2J")));
    }
}
