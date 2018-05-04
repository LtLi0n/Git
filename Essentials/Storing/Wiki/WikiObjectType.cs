using System;
using System.Collections.Generic;
using System.Text;

namespace MUD_Server.Essentials.Storing
{
    public enum WikiObjectType : byte
    {
        NPC,
        Monster,
        Item,
        System,
        Lore,
        None = byte.MaxValue
    }
}
