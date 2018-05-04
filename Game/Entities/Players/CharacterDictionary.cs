using MUD_Server.Essentials.Framework;
using MUD_Server.Essentials.Scripts;
using System;

namespace MUD_Server.Game.Entities.Players
{
    public class CharacterDictionary
    {
        public string this[SocketCharacter character, object variable]
        {
            get
            {
                if (character == null) return "<USER_NULL_ERROR>";

                string variableStr = variable.ToString().ToUpper();

                var properties = character.GetType().GetProperties();

                //Search for asked property
                foreach(var prop in properties)
                {
                    bool isTargetProperty = false;

                    object[] attributes = prop.GetCustomAttributes(false);

                    foreach(var attribute in attributes)
                    {
                        if (attribute is ScriptVariableAttribute scriptAttribute)
                        {
                            if(scriptAttribute.Recognizer != string.Empty)
                            {
                                if(scriptAttribute.Recognizer.ToUpper() == variableStr) isTargetProperty = true;
                            }
                            else
                            {
                                if (prop.Name.ToUpper() == variableStr) isTargetProperty = true;
                            }
                            break;
                        }
                    }

                    if(isTargetProperty)
                    {
                        return prop.GetValue(character).ToString();
                    }
                }

                return $"{Color.Parse("red:intense")}<Variable {Color.Parse("blue:intense")}[{variableStr}] {Color.Parse("red:intense")}not recognized>{Color.Parse("white")}";
            }
        }
    }
}
