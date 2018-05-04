using MUD_Server.Essentials.Commands;
using MUD_Server.Essentials.Framework;
using MUD_Server.Game.Players;
using MUD_Server.Network;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using static MUD_Server.Essentials.Framework.Color;
using System.Threading.Tasks;
using MUD_Server.Game.Entities.Players;
using DatabaseFormats.Nodes;
using DataBaseFormats;

namespace MUD_Server.Game.Nodes
{
    public class SocketSubNode : SubNode
    {
        private ColorCollection Colors => Color.Colors;

        public Command[] Commands { get; }

        private Func<SocketCharacter, Task> _onPlayerArrival;

        public SocketSubNode(
            string name,
            string description,
            Position pos,
            NodeEntry entries,
            IEnumerable<Command> commands,
            Server server,
            Func<SocketCharacter, Task> onPlayerArrival = null)
            : base(name, description, pos, entries)
        {
            Commands = commands.Append(
                new UserCommand("help here", async e =>
                {
                    string toReturn = $"Current list of subnode {Colors["bold[yellow]"]}{name} {Colors["white"]}at {Colors["bold[yellow]"]}{pos} {Colors["white"]}Commands:\n";

                    foreach (Command c in commands) toReturn += $" -{c.Trigger}\n";

                    await e.SendMessageAsync(toReturn, true);

                })
            ).ToArray();

            if (commands != null)
            {
                server.OnMessageReceived += async (object sender, SocketMessageReceivedEventArgs e) => await Command.ExecuteAsync(Commands, e);
            }

            _onPlayerArrival = onPlayerArrival;
        }

        public async Task OnArrival(SocketCharacter player)
        {
            await player.Socket.SendMessageAsync(Description);

            await _onPlayerArrival?.Invoke(player);
        }
    }
}
