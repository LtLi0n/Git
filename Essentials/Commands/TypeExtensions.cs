using System;
using System.Collections.Generic;
using System.Text;

namespace MUD_Server.Essentials.Commands
{
    public static class TypeExtensions
    {
        public static object GetDefault(Type type) => typeof(TypeExtensions).GetMethod("GetDefaultGeneric").MakeGenericMethod(type).Invoke(null, null);
        
        public static T GetDefaultGeneric<T>() => default;
    }
}
