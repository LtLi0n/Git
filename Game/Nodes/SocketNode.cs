using DatabaseFormats.Nodes;
using DataBaseFormats;
using MUD_Server.Essentials.Commands;
using MUD_Server.Game.Entities.Players;
using MUD_Server.Game.Players;
using MUD_Server.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace MUD_Server.Game.Nodes
{
    public class SocketNode : Node
    {
        public IEnumerable<Command> Commands { get; }
        public List<SocketCharacter> Players { get; set; }

        public Dictionary<Position, SocketSubNode> SubNodes { get; }

        public SocketNode(string name, string description, int ID, IEnumerable<SocketSubNode> subNodes, IEnumerable<Command> commands, Server server) : base(name, description, ID, subNodes)
        {
            Commands = commands;

            SubNodes = new Dictionary<Position, SocketSubNode>();

            foreach(SocketSubNode subNode in subNodes) SubNodes.Add(subNode.Position, subNode);
        }
    }
}
