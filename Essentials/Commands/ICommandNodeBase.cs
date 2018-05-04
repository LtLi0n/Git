using MUD_Server.Essentials.Framework;
using MUD_Server.Game.Entities.Players;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MUD_Server.Essentials.Commands
{
    public interface ICommandNodeBase
    {
        CommandNode Parent { get; }
        Description Description { get; }
        CommandNodeCollection Collection { get; }

        List<CommandNode> Connections { get; }
        IEnumerable<Command> Commands { get; }
        
        string Path { get; }
        string RegexFailedExplanation { get; }
        string Regex { get; }
        string Intro { get; }

        bool IsCinematic { get; }
        bool IsDestination { get; }
        bool AllowsBackwards { get; }
        bool DisableRefreshOnEnter { get; }

        int[] RegexGroups { get; }

        Func<SocketUser, Task<bool>> SwapFunc { get; }
        Action<SocketUser> OptionalForwardAction { get; }
        Action<SocketUser> OptionalBackwardsAction { get; }
    }
}
