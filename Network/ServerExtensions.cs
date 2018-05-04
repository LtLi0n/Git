using MUD_Server.Essentials.Framework;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MUD_Server.Network
{
    public static class ServerExtensions
    {
        public static async Task SendMessageAsync(this Socket socket, object value, bool clearScreen = false, bool newLine = true, bool forceColorAtEnd = true) =>
            await  Task.Run(async () => 
            {
                if (clearScreen) await socket.ClearScreen();

                socket.Send(Encoding.ASCII.GetBytes(value + (newLine ? "\n" : string.Empty) + (forceColorAtEnd ? Color.Colors["white"].ToString() : string.Empty)));
            });

        //\x1b[2J doesn't work :(
        public static async Task ClearScreen(this Socket socket, bool simulate = false, int simulateCount = 100) =>
            await Task.Run( () => socket.Send(Encoding.ASCII.GetBytes(simulate ? 
                new string(new char[simulateCount]).Replace('\0','\n') :
                "\x1b[2J")));

        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }
    }
}
