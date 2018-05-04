using MUD_Server.Essentials.Framework;
using MUD_Server.Game.Entities.Players;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MUD_Server.Essentials.Framework.DynamicDescription;

namespace MUD_Server.Essentials.Commands
{
    public class CommandNodeBuilder : ICommandNodeBase
    {
        public CommandNode Parent { get; set; }
        public Description Description { get; private set; }

        public List<CommandNode> Connections { get; }
        public CommandNodeCollection Collection { get; }
        public IEnumerable<Command> Commands { get; }
        public int[] RegexGroups { get; private set; }

        public string Regex { get; }
        public string RegexFailedExplanation { get; private set; }
        public string Intro { get; private set; }
        public string Path { get; private set; }

        public bool IsCinematic { get; private set; }
        public bool IsDestination { get; private set; }
        public bool AllowsBackwards { get; private set; }
        public bool DisableRefreshOnEnter { get; private set; }

        public Func<SocketUser, Task<bool>> SwapFunc { get; private set; }
        public Action<SocketUser> OptionalForwardAction { get; private set; }
        public Action<SocketUser> OptionalBackwardsAction { get; private set; }

        public CommandNode Build() => new CommandNode(this);

        ///<summary>Parent base constructor.</summary>
        private CommandNodeBuilder(CommandNodeCollection collection, IEnumerable<Command> commands)
        {
            Connections = new List<CommandNode>();
            AllowsBackwards = true;
            Collection = collection;
            Commands = commands;
        }

        #region Constructors

        ///<summary>Parent runtime description constructor.</summary>
        public CommandNodeBuilder(CommandNodeCollection collection, Description description, IEnumerable<Command> commands = null) : this(collection, commands) => Description = description;
        ///<summary>Parent runtime dynamic description constructor.</summary>
        public CommandNodeBuilder(CommandNodeCollection collection, DescriptionFunction descFunc, IEnumerable<Command> commands = null) : this(collection, commands) => Description = new DynamicDescription(descFunc);
        ///<summary>Parent local description constructor.</summary>
        ///<param path="path">Path to .gosf extension. Extension does not need to be specified.</param>
        public CommandNodeBuilder(CommandNodeCollection collection, string path, IEnumerable<Command> commands = null) : this(collection, commands) => Path = path + ".gosf";

        ///<summary>Child base constructor.</summary>
        private CommandNodeBuilder(CommandNodeCollection collection, CommandNode parent, string regex, IEnumerable<Command> commands) : this(collection, commands)
        {
            Parent = parent;
            Regex = regex;
        }
        ///<summary>Child runtime description constructor.</summary>
        public CommandNodeBuilder(CommandNodeCollection collection, CommandNode parent, string regex, Description description, IEnumerable<Command> commands = null) : this(collection, parent, regex, commands) => Description = description;
        ///<summary>Child runtime dynamic description constructor.</summary>
        public CommandNodeBuilder(CommandNodeCollection collection, CommandNode parent, string regex, DescriptionFunction descFunc, IEnumerable<Command> commands = null) : this(collection, parent, regex, commands) => Description = new DynamicDescription(descFunc);
        ///<summary>Child local description constructor.</summary>
        ///<param path="path">Path to .gosf extension. Extension does not need to be specified.</param>
        public CommandNodeBuilder(CommandNodeCollection collection, CommandNode parent, string regex, string path, IEnumerable<Command> commands = null) : this(collection, parent, regex, commands) => Path = path + ".gosf";

        #endregion

        #region With-Addings

        public CommandNodeBuilder WithSwapCheck(Func<SocketUser, Task<bool>> swapCheckFunc)
        {
            SwapFunc = swapCheckFunc;
            return this;
        }
        public CommandNodeBuilder WithOptionalExecution(Action<SocketUser> optionalAction)
        {
            OptionalForwardAction = optionalAction;
            return this;
        }
        public CommandNodeBuilder WithBackwardsExecution(Action<SocketUser> backwardsAction)
        {
            OptionalBackwardsAction = backwardsAction;
            return this;
        }
        public CommandNodeBuilder WithInput(params int[] groups)
        {
            RegexGroups = groups;
            return this;
        }
        ///<summary>Text to display if input didn't match any connection regexes.</summary>
        public CommandNodeBuilder WithRegexExplanation(string regexExplanation)
        {
            RegexFailedExplanation = regexExplanation;
            return this;
        }
        public CommandNodeBuilder WithIntro(string intro)
        {
            Intro = intro;
            return this;
        }

        public CommandNodeBuilder Cinematic()
        {
            IsCinematic = true;
            return this;
        }
        public CommandNodeBuilder Destination()
        {
            IsDestination = true;
            return this;
        }

        public CommandNodeBuilder DisableBackwards()
        {
            AllowsBackwards = false;
            return this;
        }
        public CommandNodeBuilder DisableEnterRefresh()
        {
            DisableRefreshOnEnter = true;
            return this;
        }

        #endregion
    }
}
