using MUD_Server.Game.Entities.Players;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MUD_Server.Essentials.Framework
{
    public class Description
    {
        public string Display { get; }
        public virtual Task<string> GetDynamicDisplayAsync(SocketUser user) => Task.FromResult(Display);

        public Description(string staticDisplay) => Display = staticDisplay;
        protected Description() { }

        public static readonly Description Empty = new Description(string.Empty);
    }
}
