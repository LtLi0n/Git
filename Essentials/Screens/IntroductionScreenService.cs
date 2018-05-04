using MUD_Server.Essentials.Commands;

namespace MUD_Server.Essentials.Screens
{
    public class IntroductionScreenService : CommandNodeCollection
    {
        private const string path = @"data\scenes\introduction\";

        public IntroductionScreenService() :
            base(new string[]
            {
                "Node",
                "Node Manager",
                "The King",
                "The Island",
                "Convince Him",
                "Golmodoth",
                "Katra",
                "Holgard",
                "The Arrival",
                "Me",
                "The Test",
                "Barracks"
            })
        {
            //0
            Entry = new CommandNodeBuilder(this, path + "the_arrival").Cinematic().Build();
            {
                var Character_Backstory = new CommandNodeBuilder(this, Entry, "(?i)me", path + "backstory").Cinematic().Build();
                CreateConnection(Entry, Character_Backstory);
            }

            //0->
            var Arrival = new CommandNodeBuilder(this, Entry, "(?i)the arrival", path + "the_arrival_2").Cinematic().Build();
            CreateConnection(Entry, Arrival);
            {
                var Arrival_Katra = new CommandNodeBuilder(this, Arrival, "(?i)katra", Wiki[@"NPC:Clacton\katra"].Path).Cinematic().Build();
                var Arrival_Holgard = new CommandNodeBuilder(this, Arrival, "(?i)holgard", Wiki[@"NPC:Clacton\holgard"].Path).Cinematic().Build();
                var Arrival_Barracks = new CommandNodeBuilder(this, Arrival, "(?i)barracks", path + "barracks").Cinematic().Build();
                var Arrival_NodeManager = new CommandNodeBuilder(this, Arrival, "(?i)node manager", Wiki["System:node_manager"].Path).Cinematic().Build();

                //The King
                {
                    var TheKing = new CommandNodeBuilder(this, Arrival_NodeManager, "(?i)the king", Wiki["NPC:the_king"].Path).Cinematic().Build();
                    var Node = new CommandNodeBuilder(this, Arrival_NodeManager, "(?i)node", Wiki["System:node"].Path).Cinematic().Build();

                    CreateConnection(Arrival_NodeManager, TheKing);
                    CreateConnection(TheKing, Node);
                }
                CreateConnections(Arrival, Arrival_Katra, Arrival_Holgard, Arrival_Barracks, Arrival_NodeManager);
            }

            var TheTest = new CommandNodeBuilder(this, Arrival, "(?i)the test", path + "the_test").Cinematic().Build();
            CreateConnection(Arrival, TheTest);

            var TheTest_AskRats = new CommandNodeBuilder(this, TheTest, "(?i)cellar rats", Wiki["Monster:cellar_rat"].Path).Cinematic().Build();
            var TheIsland = new CommandNodeBuilder(this, TheTest, "(?i)the island", path + "the_island").Cinematic().Build();

            CreateConnections(TheTest, TheTest_AskRats, TheIsland);
        }
    }
}
