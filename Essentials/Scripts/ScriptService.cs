using MUD_Server.Essentials.Commands;
using System;
using System.Threading.Tasks;
using System.Linq;
using MUD_Server.Essentials.Framework;
using MUD_Server.Game.Entities.Players;

namespace MUD_Server.Essentials.Scripts
{
    public static class ScriptService
    {
        private static ScriptCommand[] _scriptFormats;

        public static Task Init()
        {
            _scriptFormats = new ScriptCommand[]
            {
                //Changes speed at which the text is displayed.
                new ScriptCommand<string, Optional<string>>("Speed", e =>
                {
                    int speed = 0;

                    bool simulate = false;
                    string simulationRelease = string.Empty;
                    bool alreadyHolding = false;

                    if(e.Arg1 == "NORMAL") speed = TextSpeed.NORMAL.Speed;
                    else if(e.Arg1 == "HOLD")
                    {
                        //default speed
                        if (e.Arg2 == null)
                        {
                            speed = 1000;
                            simulate = true;
                            simulationRelease = TextSpeed.NORMAL.ToString();
                            alreadyHolding = true;
                        }
                        else speed = int.Parse(e.Arg2.Value);
                    }
                    else speed = int.Parse(e.Arg1);

                    //if optional param exists and is equal to HOLD, textspeed is simulated.
                    if(e.Arg2 != null && !alreadyHolding)
                    {
                        if (e.Arg2 == "HOLD")
                        {
                            speed = 1000;
                            simulate = true;
                            simulationRelease = TextSpeed.NORMAL.ToString();
                        }
                    }

                    return new TextSpeed(speed, simulate).ToString() + simulationRelease;
                }),
                //Changes color of the text
                new ScriptCommand<string, Optional<string>>("Color", e =>
                {
                    string color = e.Arg1;
                    string modifier = string.Empty;
                    if(e.Arg2 != null) modifier = ':' + e.Arg2;
                    else modifier = string.Empty;

                    return Color.Parse(color + modifier);
                }),
                new ScriptCommand<string, string>("Variable", e =>
                {
                    if(e.Arg1.ToUpper() == "CHARACTER") return e.Character[e.Arg2];
                    return "[Variable parent unrecognized]";
                }),
                new ScriptCommand<string>("Highlight", e => Color.Highlight(e.Arg)),
                new ScriptCommand<string>("Hint", e => Color.Hint(e.Arg))
            };

            return Task.CompletedTask;
        }

        ///<summary>Executes script and translates it into a renderable format.</summary>
        public static string TranslateScript(Script script)
        {
            var scriptHandler = _scriptFormats.ToList().Find(x => x.Trigger == script.Call);

            //Try to handle the script
            if (scriptHandler != null) return scriptHandler.Translate(script.Args);

            throw new ArgumentException("Supported Script with this call was not found.");
        }

        ///<summary>Executes script and translates it into a renderable format.
        ///<para><paramref name="user"/> provides variable extraction option from the user.</para>
        ///</summary>
        public static string TranslateScript(Script script, SocketUser user)
        {
            var scriptHandler = _scriptFormats.ToList().Find(x => x.Trigger == script.Call);

            //Try to handle the script
            if (scriptHandler != null) return scriptHandler.Translate(user, script.Args);

            throw new ArgumentException("Supported Script with this call was not found.");
        }
    }
}
