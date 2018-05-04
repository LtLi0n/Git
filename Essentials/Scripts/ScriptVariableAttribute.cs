using System;

namespace MUD_Server.Essentials.Scripts
{
    ///<summary>Exposes variable to script texts.</summary>
    public class ScriptVariableAttribute : Attribute
    {
        public string Recognizer { get; }
        ///<summary>Ignores variable naming, focuses on defined recognizer instead.</summary>
        public ScriptVariableAttribute(string recognizer) => Recognizer = recognizer;
        public ScriptVariableAttribute() => Recognizer = string.Empty;
    }
}
