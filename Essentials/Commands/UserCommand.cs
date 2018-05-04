using System;
using System.Threading.Tasks;
using MUD_Server.Network;

namespace MUD_Server.Essentials.Commands
{
    public class UserCommand : Command
    {
        private Func<SocketMessageReceivedEventArgs, Task> _runFunc;

        public UserCommand(string trigger, Func<SocketMessageReceivedEventArgs, Task> runFunc) : base(trigger) => _runFunc = runFunc;
        public UserCommand(string trigger, string description, Func<SocketMessageReceivedEventArgs, Task> runFunc) : base(trigger)
        {
            Description = description;
            _runFunc = runFunc;
        }

        public override async Task ExecuteAsync(params object[] args) => await _runFunc((SocketMessageReceivedEventArgs)args[0]);
    }
}
