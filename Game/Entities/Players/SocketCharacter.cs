using MUD_Server.Game.Nodes;
using System;
using System.Net.Sockets;
using MUD_Server.Network;
using DataBaseFormats.Entities.Players;
using DatabaseFormats.Interfaces.Entities;
using DataBaseFormats;
using MUD_Server.Essentials.Scripts;

namespace MUD_Server.Game.Entities.Players
{
    public class SocketCharacter : ICharacter
    {
        ///<summary>Character Variable Library</summary>
        private readonly static CharacterDictionary cDictionary = new CharacterDictionary();
        public string this [object variable] => cDictionary[this, variable];

        public SocketSubNode SubNode(GameWorld world) => world.Nodes[NodeID].SubNodes[Location];

        public SocketUser User { get; set; }
        public Socket Socket => User.Socket;

        [ScriptVariable] public string Name { get; }
        public Position Location { get; set; }
        public int NodeID { get; set; }

        [ScriptVariable] public Race Race { get; }
        [ScriptVariable] public Class Class { get; }
        [ScriptVariable] public Zodiac Zodiac { get; }
        [ScriptVariable] public Gender Gender { get; }

        public long Xp { get; set; }
        public long XpCap => Level * 100;
        [ScriptVariable]public int Level { get; set; }
        public int HP { get; set; }
        public int MaxHP => Level * 10;

        public DateTime CreatedAt { get; }
        public TimeSpan Played { get; set; }
        public bool IsNew { get; set; }

        public SocketCharacter(SocketUser socketUser, ICharacter character)
        {
            User = socketUser;

            Name = character.Name;
            Location = character.Location;
            NodeID = character.NodeID;

            Class = character.Class;
            Zodiac = character.Zodiac;
            Gender = character.Gender;
            Race = character.Race;

            Xp = character.Xp;
            Level = character.Level;
            HP = character.HP;

            CreatedAt = character.CreatedAt;
            Played = character.Played;
            IsNew = character.IsNew;
        }

        public void OnMessageReceived(object sender, SocketMessageReceivedEventArgs e)
        {
            Console.WriteLine(e.Content);
        }
    }
}
