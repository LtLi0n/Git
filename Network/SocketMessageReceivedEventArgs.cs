using MUD_Server.Game;
using MUD_Server.Game.Entities.Players;
using MUD_Server.Game.Nodes;

using System.Threading.Tasks;

namespace MUD_Server.Network
{
    public class SocketMessageReceivedEventArgs
    {
        public SocketUser User { get; }
        public string Content { get; }
        public SocketCharacter Character => User.OnlineCharacter;
        public SocketSubNode SubNode(GameWorld world) => Character.SubNode(world);

        public string Call { get; }
        public string[] Args { get; }
        public string ArgsUnparsed { get; }

        public string GetArg(int index) => Args.Length > index ? Args[index] : string.Empty;

        public SocketMessageReceivedEventArgs(SocketUser user, string content)
        {
            User = user;
            Content = content;

            Call = content.Split(' ')[0];
            Args = (content.Length > Call.Length) ? content.Substring(Call.Length + 1, content.Length - Call.Length - 1).Split(' ') : new string[0];
            ArgsUnparsed = (content.Length > Call.Length) ? content.Substring(Call.Length + 1, content.Length - Call.Length - 1) : string.Empty;

        }

        public async Task SendMessageAsync(object value, bool clearScreen = false, bool newLine = true, bool forceColorAtEnd = true) => await User.SendMessageAsync(value, clearScreen, newLine, forceColorAtEnd);
    }
}
