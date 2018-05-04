using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using MUD_Server.Essentials.Framework;
using MUD_Server.Game.Entities.Players;
using MUD_Server.Essentials.Scripts;
using static MUD_Server.Essentials.Framework.DynamicDescription;

namespace MUD_Server.Essentials.Commands
{
    public class CommandNode : ICommandNodeBase
    {
        public CommandNode Parent { get; }
        public List<CommandNode> Connections { get; }
        public CommandNodeCollection Collection { get; }
        /// <summary>Text to display if input didn't match any connection regexes.</summary>
        public string RegexFailedExplanation { get; }
        public string Regex { get; }
        public string Intro { get; }
        public string Path { get; }
        public bool IsCinematic { get; }
        public bool IsDestination { get; }
        public bool AllowsBackwards { get; }
        public bool DisableRefreshOnEnter { get; }
        public IEnumerable<Command> Commands { get; }
        /// <summary>bool - can swap, runs before the node is swapped as an active node.</summary>
        public Func<SocketUser, Task<bool>> SwapFunc { get; }
        public Description Description { get; }
        /// <summary>Regex groups to store in user's cache upon successful transition. 0 - whole.</summary>
        public int[] RegexGroups { get; }
        /// <summary> 
        /// Is input stored.
        /// <para>If <see cref="RegexGroups"/> is defined.</para>
        /// </summary>
        public bool ParsableInput => RegexGroups != null;
        public Action<SocketUser> OptionalForwardAction { get; }
        public Action<SocketUser> OptionalBackwardsAction { get; }

        /// <summary>Parent base.</summary>
        protected CommandNode(CommandNodeCollection collection, IEnumerable<Command> commands)
        {
            Connections = new List<CommandNode>();
            AllowsBackwards = true;
            Commands = commands;
        }
        /// <summary>Child base.</summary>
        protected CommandNode(CommandNodeCollection collection, CommandNode parent, string regex, IEnumerable<Command> commands) : this(collection, commands)
        {
            Parent = parent;
            Regex = regex;
        }

        public async Task<string> GetDynamicDescription(SocketUser user, bool withSpeeds = false, bool filterCinematicColor = false)
        {
            //Cinematic strings "May" have TextSpeed tweaks inside. Terminal seeing them would be more confused than me without those comments.
            string displayStr = string.IsNullOrEmpty(Path) ? await Description.GetDynamicDisplayAsync(user) : (await ScriptText.Load(user, Path)).CinematicText;

            if (IsCinematic)
            {
                //remove hints and highlights from display string.
                if (filterCinematicColor)
                {
                    //Apply force hints/hightlights
                    displayStr = Collection.ForceMarkDescription(this, displayStr);

                    MatchCollection pointsOfInterestRaw = System.Text.RegularExpressions.Regex.Matches(displayStr, @"\x1b\[1;3[0-7]m([a-z|A-Z|0-9|\s])+");

                    foreach (Match pointOfInterestRaw in pointsOfInterestRaw)
                    {
                        string pointOfInterest = pointOfInterestRaw.Value.Substring(7);
                        Color highlight = Color.FromAnsi(pointOfInterestRaw.Value.Substring(0, 7));

                        string replacer = (user.CinematicsUsed.Skip(1).ToList().Exists(cin => System.Text.RegularExpressions.Regex.IsMatch(pointOfInterest, cin.Regex)) ? Color.Parse(highlight.ReadFormat.Split(':')[0] + ":intense") : highlight) + pointOfInterest;

                        displayStr = displayStr.Remove(pointOfInterestRaw.Index, pointOfInterestRaw.Length).Insert(pointOfInterestRaw.Index, replacer);
                    }
                }
                //remove speeds from display string.
                if (!withSpeeds)
                {
                    MatchCollection speeds = System.Text.RegularExpressions.Regex.Matches(displayStr, @"→\d+(↑|↥)");
                    foreach (Match m in speeds)
                    {
                        displayStr = displayStr.Remove(displayStr.IndexOf(m.Value), m.Value.Length);
                    }

                    MatchCollection clears = System.Text.RegularExpressions.Regex.Matches(displayStr, @"\x1b\[2J");
                    foreach (Match m in clears)
                    {
                        displayStr = displayStr.Remove(displayStr.IndexOf(m.Value), m.Value.Length);
                    }
                }
            }

            return displayStr;
        }

        public CommandNode(CommandNodeBuilder cnBase)
        {
            Parent = cnBase.Parent;
            Description = cnBase.Description;
            Collection = cnBase.Collection;

            Connections = cnBase.Connections;
            Commands = cnBase.Commands;

            Path = cnBase.Path;
            RegexFailedExplanation = cnBase.RegexFailedExplanation;
            Regex = cnBase.Regex;
            Intro = cnBase.Intro;

            IsCinematic = cnBase.IsCinematic;
            IsDestination = cnBase.IsDestination;
            AllowsBackwards = cnBase.AllowsBackwards;
            DisableRefreshOnEnter = cnBase.DisableRefreshOnEnter;

            RegexGroups = cnBase.RegexGroups;

            SwapFunc = cnBase.SwapFunc;
            OptionalForwardAction = cnBase.OptionalForwardAction;
            OptionalBackwardsAction = cnBase.OptionalBackwardsAction;
        }

        /// <summary>If input matches regex, returns trimmed input to just match regex.</summary>
        public IEnumerable<string> ParseInput(string input)
        {
            List<string> inputs = new List<string>();
            List<string> expectedInputs = new List<string>();

            foreach (Group group in System.Text.RegularExpressions.Regex.Match(input, Regex).Groups)
            {
                inputs.Add(group.Value);
            }

            int startCount = inputs.Count;

            foreach (int groupIndex in RegexGroups)
            {
                expectedInputs.Add(inputs[groupIndex]);
            }

            return expectedInputs;
        }


        /// <summary>Optional execution method when transition forward is successful.</summary>
        public void RunOptional_Forward(SocketUser user) => OptionalForwardAction?.Invoke(user);
        /// <summary>Optional execution method when transition backwards is allowed/supported(not parent).</summary>
        public void RunOptional_Backwards(SocketUser user)
        {
            if (AllowsBackwards) OptionalBackwardsAction?.Invoke(user);
            else throw new NotSupportedException("Backwards execution is not allowed because AllowsBackwards is set to false.");
        }

        /// <summary>Prints text by characters one by one applying delay of 'speed' for each display.</summary>
        public async Task DisplayDescriptionCinematic(SocketUser user)
        {
            user.CinematicsUsed.Add(this);

            try
            {
                string desc = await GetDynamicDescription(user, true, true);

                if (Intro != null) desc = Intro + desc;

                await new CinematicDisplay(desc).Render(user, true);
            }
            catch (Exception e)
            {
                await user.SendMessageAsync(e.Message);
            }
        }
    }
}