using MUD_Server.Game.Entities.Players;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MUD_Server.Essentials.Framework
{
    public class DynamicDescription : Description
    {
        public delegate Task<string> DescriptionFunction(SocketUser user);

        private readonly DescriptionFunction _descFunc;

        public override async Task<string> GetDynamicDisplayAsync(SocketUser user) => await _descFunc.Invoke(user);

        public DynamicDescription(DescriptionFunction func) => _descFunc = func;
    }
}
