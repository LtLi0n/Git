using MUD_Server.Game.Entities.Players;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MUD_Server.Essentials.Scripts
{
    public class ScriptText
    {
        public string Raw { get; }
        ///<summary>String stripped from script calls.</summary>
        public string Naked { get; }
        ///<summary>int - string location at which script is expected to be executed.</summary>
        public IEnumerable<(int, Script)> Scripts { get; }

        public string CinematicText { get; }

        private ScriptText(SocketUser user, string raw)
        {
            Raw = raw;

            var scrapped = ScrapScripts();
            Scripts = scrapped.Item1;
            Naked = scrapped.Item2;

            CinematicText = CreateCinematicText(user);
        }

        ///<summary>item1 - scrapped scripts, item2 - string without scripts.</summary>
        private (IEnumerable<(int, Script)>, string) ScrapScripts()
        {
            List<(int, Script)> scripts = new List<(int, Script)>();

            //\[\w+\(((\w+(, )?)+)\w+\)\] - old
            //\[\w+\(([^,]+, )?+[^,)]+\)\] - new
            //\[\w+\((((('?)\w+('?)(,( )?)?)+)('?)\w+('?))?\)\] abomination but works
            //\[\w+\(([^\)]+)?\)\] - toReplace

            MatchCollection matches = Regex.Matches(Raw, @"\[\w+\(([^\)]+)?\)\]");

            string naked = Raw;
            int nakedOffset = 0;

            foreach(Match m in matches)
            {
                naked = naked.Remove(m.Index - nakedOffset, m.Value.Length);
                nakedOffset += m.Value.Length;

                string scriptValue = m.Value;

                string argGroup = m.Groups.Count > 1 ? m.Groups[1].Value : string.Empty;

                scripts.Add((m.Index, new Script(Regex.Match(scriptValue, @"\w+").Value, argGroup)));
            }

            return (scripts, naked);
        }

        ///<summary>Load a script file from path.</summary>
        public static async Task<ScriptText> Load(string path)
        {
            if (File.Exists(path)) return new ScriptText(null, await File.ReadAllTextAsync(path));
            else throw new FileNotFoundException($"File '{path}' doesn't exist.");
        }

        ///<summary>Load a script file from path.</summary>
        public static async Task<ScriptText> Load(SocketUser user, string path)
        {
            if (File.Exists(path)) return new ScriptText(user, await File.ReadAllTextAsync(path));
            else throw new FileNotFoundException($"File '{path}' doesn't exist.");
        }

        ///<summary>Creates string with formats supported by the server for a cinematic display.</summary>
        private string CreateCinematicText(SocketUser user)
        {
            string cinematicText = Raw;
            int offset = 0;

            foreach (var script in Scripts)
            {
                string rawScript = script.Item2.ToString();
                cinematicText = cinematicText.Remove(script.Item1 + offset, rawScript.Length + 2); //+2 for [] surrounding borders

                string translatedScript = user == null ? ScriptService.TranslateScript(script.Item2) : ScriptService.TranslateScript(script.Item2, user);
                cinematicText = cinematicText.Insert(script.Item1 + offset, translatedScript);

                offset += (translatedScript.Length - (rawScript.Length + 2));
            }

            return cinematicText;
        }
    }
}
