using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MUD_Server.Essentials.Framework;
using static MUD_Server.Essentials.Framework.Color;
using MUD_Server.Essentials.Storing;
using MUD_Server.Game;
using MUD_Server.Game.Entities.Players;
using MUD_Server.Essentials.Commands.CommandNodeCollections;

namespace MUD_Server.Network
{
    public class Server
    {
        public bool Listening { get; private set; }

        public delegate void MessageReceivedEventHandler(object sender, SocketMessageReceivedEventArgs e);
        public event MessageReceivedEventHandler OnMessageReceived;

        private readonly IPEndPoint _localEndPoint;
        private Socket _listener;

        private ColorCollection Colors => Color.Colors;

        private UserSQL UserDB => DB.UserDB;

        private DataBases DB { get; }

        private IEnumerable<string> whitelist;

        private GameWorld _world;

        public List<SocketUser> OnlineUsers { get; }

        public Server(int port, DataBases db)
        {
            OnlineUsers = new List<SocketUser>();

            DB = db;

            _localEndPoint = new IPEndPoint(IPAddress.Any, port);
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            whitelist = new string[]
            {
                "127.0.0.1", //home sweet home
                "192.168.1.254" //pi
            };
        }

        public Task Start(GameWorld world)
        {
            if (Listening) throw new Exception("Server is already in run state.");

            _world = world;

            _listener.Bind(_localEndPoint);
            _listener.Listen(100);
            Listening = true;

            new Thread(() => Listen()).Start();

            return Task.CompletedTask;
        }

        private async void Listen()
        {
            Console.WriteLine("Listening...");

            while (Listening)
            {
                Socket client = await _listener.AcceptAsync();

                new Thread((object o) => HandleSocket(client).GetAwaiter().GetResult()).Start();
            }
        }

        private async Task HandleSocket(Socket socket)
        {
            string ip = (socket.RemoteEndPoint as IPEndPoint).Address.ToString();

            //deny connection to unknown IPs
            if (!whitelist.Contains(ip))
            {
                Console.WriteLine($"[ {ip} ] denied.");
                await socket.SendMessageAsync($"{GetColor(ColorName.Red, ColorType.Intense)}You don't have access. Sorry :(");
                socket.Close();
                return;
            }

            SocketUser user = new SocketUser(socket);
            OnlineUsers.Add(user);

            Console.WriteLine($"{socket.RemoteEndPoint.ToString()} connected!");

            await user.SetActiveCommandNode(CommandNodeServiceCollection.LoginScreen.Entry, true, true);

            while(socket.IsConnected())
            {
                if(socket.Available > 0)
                {
                    byte[] buffer = new byte[socket.Available];
                    socket.Receive(buffer);
                    string msg = GetString(buffer);

                    string polishedMsg = msg.Trim('\n', '\r');

                    await user.HandleInput(polishedMsg, true, true);
                }

                await Task.Delay(1);
            }

            OnlineUsers.Remove(user);

            Console.WriteLine($"{socket.RemoteEndPoint.ToString()} disconnected!");
        }

        //ASCII
        private string GetString(byte[] data) => Encoding.ASCII.GetString(data);
        private byte[] ConvertString(string text) => Encoding.ASCII.GetBytes(text);
    }
}
