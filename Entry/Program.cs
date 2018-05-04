using System;
using System.Threading.Tasks;

namespace MUD_Server.Entry
{
    class Program
    {
        static async Task Main(string[] args) => await new Startup(args).RunAsync();
    }
}
