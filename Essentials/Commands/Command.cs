using MUD_Server.Essentials.Framework;
using MUD_Server.Network;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MUD_Server.Game.Entities.Players;

namespace MUD_Server.Essentials.Commands
{
    public abstract class Command
    {
        public string Trigger { get; }
        public string Description { get; protected set; }
        public bool IsHidden { get; private set; }

        public Command Hide() { IsHidden = true; return this; }

        public virtual Task ExecuteAsync(SocketUser caller, params object[] args) => throw new NotImplementedException();
        public abstract Task ExecuteAsync(params object[] args);

        public Command(string trigger) => Trigger = trigger;

        public static async Task ExecuteAsync(Command[] commands, SocketMessageReceivedEventArgs e)
        {
            bool found = false;

            foreach (Command c in commands)
            {
                if (c.Trigger == e.Content)
                {
                    await c.ExecuteAsync(e);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                await e.Character.User.Socket.SendMessageAsync(
                    $"\nDidn't quite catch that...\n" +
                    $"try {Color.Colors["bold[white]"]}help here {Color.Colors["white"]}for a command list specific to this SubNode.\n");
            }
        }

        ///<summary>ONLY FOR INTERNAL COMMANDS.</summary>
        public virtual Task<string> TranslateAsync(SocketUser user, IEnumerable<object> args) => throw new NotSupportedException("Only for InternalCommand");
        public virtual Task<string> TranslateAsync(IEnumerable<object> args) => throw new NotSupportedException("Only for InternalCommand");
    }
}
