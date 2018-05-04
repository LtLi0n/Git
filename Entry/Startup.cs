using DatabaseFormats.Nodes;
using MUD_Server.Essentials.Commands.CommandNodeCollections;
using MUD_Server.Essentials.Framework;
using MUD_Server.Essentials.Scripts;
using MUD_Server.Essentials.Storing;
using MUD_Server.Game;
using MUD_Server.Game.Entities.NPCs;
using MUD_Server.Network;
using System;
using System.Threading.Tasks;

namespace MUD_Server.Entry
{
    public class Startup
    {
        public const string Version = "0.0.7a";

        private Server _server;
        private GameWorld _world;
        private DataBases DB { get; }

        public Startup(string[] args)
        {
            Color.InitCollection();
            WikiDictionary.Init(@"data\wiki\");

            DB = new DataBases();

            _server = new Server(9999, DB);
            _world = new GameWorld(_server, DB);
        }

        public async Task RunAsync()
        {
            await DB.Open();

            await ScriptService.Init();
            await CommandNodeServiceCollection.Init(DB, _server);

            await _server.Start(_world);

            await Task.Delay(-1);
        }
    }
}
