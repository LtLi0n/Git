using MUD_Server.DevTools;
using MUD_Server.Essentials.Screens;
using MUD_Server.Essentials.Storing;
using MUD_Server.Network;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MUD_Server.Essentials.Commands.CommandNodeCollections
{
    public static class CommandNodeServiceCollection
    {
        private static IEnumerable<CommandNodeCollection> CommandNodeService { get; set; }

        public static LoginScreenService LoginScreen { get; private set; }
        public static IntroductionScreenService IntroductionScreen { get; private set; }

        public static Task Init(DataBases databases, Server server)
        {
            LoginScreen = new LoginScreenService(databases.UserDB, server);
            IntroductionScreen = new IntroductionScreenService();

            CommandNodeService = new CommandNodeCollection[]
            {
                LoginScreen, IntroductionScreen
            };

            return Task.CompletedTask;
        }
    }
}
