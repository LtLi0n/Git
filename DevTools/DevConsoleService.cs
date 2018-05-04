using MUD_Server.Essentials.Commands;
using MUD_Server.Essentials.Framework;
using MUD_Server.Essentials.Storing;
using MUD_Server.Game.Entities.Players;
using MUD_Server.Network;
using MUD_Server.Essentials.Scripts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace MUD_Server.DevTools
{
    public static class DevConsoleService
    {
        private const string Version = "0.2.1a";

        private static WikiDictionary Wiki => WikiDictionary.Wiki;

        private static IEnumerable<Command> CreateCommands(UserSQL userDB, Server server)
        {
            string wikiTypesDisplay = string.Empty;
            {
                Array wikiTypes = Enum.GetValues(typeof(WikiObjectType));

                for (int i = 0; i < wikiTypes.Length - 1; i++) wikiTypesDisplay += Enum.GetName(typeof(WikiObjectType), wikiTypes.GetValue(i)) + ", ";
                wikiTypesDisplay = wikiTypesDisplay.Remove(wikiTypesDisplay.Length - 2, 2);
            }

            List<Command> commands = new List<Command>
            {
                new UserCommand("SQL",
                $"{Color.Parse("white")}Runs direct sql code.\n" +
                $"{Color.Parse("yellow")}[command]", async e =>
                {
                    await userDB.ExecuteAsync(e.ArgsUnparsed);
                    Console.WriteLine($"SQL EXECUTED BY {e.User.Username}: {e.ArgsUnparsed}");
                }),
                new UserCommand("Process",
                $"{Color.Parse("white")}shows server statistics.\n" +
                $"-{Color.Parse("cyan")}kill {Color.Parse("white")}- shuts down the server remotely", async e =>
                {
                    if (e.GetArg(0) == "kill") Environment.Exit(0);
                    Process current = Process.GetCurrentProcess();

                    await e.SendMessageAsync(
                        $"Memory: {current.WorkingSet64}\n" +
                        $"Running for: {(DateTime.Now - current.StartTime).ToString()}"
                        );
                }),
                new UserCommand("SendMessage",
                $"{Color.Parse("white")}Sends user a message\n" +
                $"{Color.Parse("yellow")}[user] [msg]", async e =>
                {
                    var user = server.OnlineUsers.Find(x => x.Username == e.Args[0]);
                    if(user == null) await e.SendMessageAsync($"{e.Args[0]} is not online.");
                    else await user.SendMessageAsync($"{Color.Parse("red:intense")}{e.ArgsUnparsed.Substring(e.Args[0].Length + 1, e.ArgsUnparsed.Length - e.Args[0].Length - 1)}");
                }),
                new UserCommand("Online",
                $"{Color.Parse("white")}Displays all online users.\n" +
                $"-{Color.Parse("cyan")}characters {Color.Parse("white")}- shows character names near user [user<->character]", async e =>
                {
                    if(e.Args.Length == 0)
                    {
                        string toReturn = string.Empty;

                        foreach(SocketUser u in server.OnlineUsers)
                        {
                            if(!string.IsNullOrEmpty(u.Username)) toReturn += u.Username + '-';
                            toReturn += u.Socket.RemoteEndPoint.ToString() + '\n';
                        }
                        toReturn = toReturn.Remove(toReturn.Length - 1, 1);

                        await e.SendMessageAsync(toReturn);
                    }
                }),
                new UserCommand("CD",
                $"{Color.Parse("yellow")}[PATH]",
                e =>
                {
                    if(e.ArgsUnparsed == "..")
                    {
                        //check if user isn't at entry already
                        if(e.User.Cache[0] != @".\")
                        {
                            //trim and replace so the path doesn't end up looking like 'data\wiki/NPC\blahblah'
                            string path = e.User.Cache[0].Trim('\\').Trim('/').Replace('/', '\\');

                            string[] pathDirs = path.Split('\\');

                            string currentDir = pathDirs.Last();
                            e.User.Cache[0] = path.Remove(path.Length - currentDir.Length - 1, currentDir.Length + 1);
                        }
                    }
                    else if(Directory.Exists(e.User.Cache[0] + '\\' + e.ArgsUnparsed)) e.User.Cache[0] += '\\' + e.ArgsUnparsed.Trim('\\').Trim('/').Replace('/', '\\');

                    return Task.CompletedTask;
                }),
                new UserCommand("DIR", "Displays directory contents.", async e =>
                {
                    string[] dirs = Directory.GetDirectories(e.User.Cache[0]);
                    string[] files = Directory.GetFiles(e.User.Cache[0]);

                    string toReturn = string.Empty;

                    foreach(string dir in dirs) toReturn += dir.Split('\\').Last() + '\n';
                    foreach(string file in files) toReturn += file.Split('\\').Last() + '\n';
                    if(toReturn.Length > 0) toReturn = toReturn.Remove(toReturn.Length - 1, 1);

                    await e.SendMessageAsync(toReturn);
                }),
                new UserCommand("READ",
                $"Displays .txt or .bin file contents\n" +
                $"{Color.Parse("green")}-st {Color.Parse("white")}- show as ScriptText format.\n" +
                $"{Color.Parse("green")}-cin {Color.Parse("white")}- show as cinematic.", async e =>
                {
                    string rawUnparsed = e.ArgsUnparsed;
                    bool asScriptText = false;

                    bool asCinematic = false;

                    if(e.Args[0] == "-st" || e.Args[0] == "-cin")
                    {
                        rawUnparsed = rawUnparsed.Remove(0, e.Args[0].Length + 1);
                        asScriptText = true;
                        if(e.Args[0] == "-cin") asCinematic = true;
                    }

                    string path = e.User.Cache[0] + '\\' + rawUnparsed;

                    string content = string.Empty;

                    if(File.Exists(path))
                    {
                        var sText = await ScriptText.Load(path);

                        if(!asCinematic)
                        {
                            await e.SendMessageAsync(asScriptText ? sText.Raw : sText.Naked);
                        }
                        else
                        {
                            await new CinematicDisplay(sText.CinematicText + '\n').Render(e.User, false);
                        }
                    }
                    else await e.SendMessageAsync("File doesn't exist.");
                })
            };

            Command help = new UserCommand("help", async e =>
            {
                if (e.Args.Length == 0)
                {
                    string helpDisplay = string.Empty;

                    foreach (Command c in commands) helpDisplay += c.Trigger + '\n';

                    helpDisplay = helpDisplay.Remove(helpDisplay.Length - 1, 1);

                    await e.User.SendMessageAsync(helpDisplay);
                }
                else
                {
                    Command target = commands.Find(x => x.Trigger.ToLower() == e.Args[0].ToLower());
                    if (target != null) await e.SendMessageAsync(target.Description);
                    else await e.SendMessageAsync("Such command does not exist.");
                }
            });

            return commands.Append(help);
        }

        public static void Bind(CommandNode parent, CommandNodeCollection service, UserSQL userDB, Server server)
        {
            CommandNode refNode;

            //Dev Console Entry
            {
                var enterDevConsole = new CommandNodeBuilder(service, parent, "(?i)terminal", 
                    x => Task.FromResult($"\n{Color.Parse("green:intense")}{"system" + (x.Cache[0] == @".\" ? string.Empty : $"\\{x.Cache[0].Remove(0, 3)}")}>"),
                    CreateCommands(userDB, server))
                    .Cinematic()
                    .WithIntro($"{TextSpeed.Create(20)}{Color.Parse("green:intense")}Golmodoth Online Control Terminal [Version {Version}]\n")
                    .DisableEnterRefresh()
                    .WithSwapCheck(async e =>
                    {
                        if (e.Username != "LtLi0n")
                        {
                            await e.SendMessageAsync("Not for mortals, sorry.\n");

                            return false;
                        }
                        else
                        {
                            e.Cache.Clear();
                            e.Cache.Add(@".\");
                            await e.ClearScreen();
                            return true;
                        }
                    })
                    .WithBackwardsExecution(e => { e.CinematicsUsed.Clear(); e.Cache.Clear(); }).Build();

                service.CreateConnection(parent, enterDevConsole);
                refNode = enterDevConsole;
            }
        }
    }
}
