using MUD_Server.Game.Nodes;
using MUD_Server.Network;

using MUD_Server.Essentials.Framework;
using static MUD_Server.Essentials.Framework.Color;
using MUD_Server.Essentials.Commands;
using System.Collections.Generic;
using MUD_Server.Essentials.Storing;
using MUD_Server.Game.Entities.Players;
using DataBaseFormats;
using DatabaseFormats.Nodes;

namespace MUD_Server.Game
{
    public class GameWorld
    {
        public SocketNode[] Nodes { get; }

        private Server _server;
        public List<SocketCharacter> Players { get; set; }

        private ColorCollection Colors => Color.Colors;

        public GameWorld(Server server, DataBases databases)
        {
            _server = server;
            Players = new List<SocketCharacter>();

            Command[] subNodeCommands = new Command[]
            {
                new UserCommand("ping", async e => await e.SendMessageAsync($"{Color.Colors["yellow"]}pong!", true)),
                new UserCommand("look", async e =>
                {
                    await e.SendMessageAsync(
                        $"{Colors["white"]}You look to your feet and see some numbers on the road?\n" +
                        $"{Colors["bold[green]"]}{e.SubNode(this).Position}\n\n" +
                        $"{Colors["white"]}...Looks like they indicate your location.", true);
                }),
            };

            SocketSubNode subNodeEntry = new SocketSubNode("middle", "middle entry sub node", new Position(0, 0, 0), NodeEntry.None, subNodeCommands, server);

            Nodes = new SocketNode[]
            {
                new SocketNode("Entry", "Test Node", 0, new SocketSubNode[]{ subNodeEntry }, null, server)
            };
        }
    }
}
