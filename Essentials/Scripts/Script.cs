using System;
using System.Collections.Generic;

namespace MUD_Server.Essentials.Scripts
{
    public class Script
    {
        ///<summary>Provided comamnd args.</summary>
        public string[] Args { get; }
        ///<summary>Comamnd name to lookup when called.</summary>
        public string Call { get; }

        private readonly string _rawArgs;

        public string Raw => $"{Call}({_rawArgs})";

        public Script(string call, string rawArgs)
        {
            Call = call;
            Args = ExtractArguments(rawArgs);

            _rawArgs = rawArgs;
        }

        private string[] ExtractArguments(string rawArgs)
        {
            List<string> tmpArgs = new List<string>();

            string argBuffer = string.Empty;
            bool readingArg = false;
            //accept spaces, commas, all that stuff
            bool argWithinQuotes = false;

            foreach(char c in rawArgs)
            {
                if(!readingArg)
                {
                    //if it's spacing, ignore
                    if (c != ' ')
                    {
                        if (c == '\'') argWithinQuotes = true;
                        else argBuffer += c;

                        readingArg = true;
                    }
                }
                else
                {
                    //arg found, add to the list
                    if (argWithinQuotes && c == '\'' || !argWithinQuotes && c == ',')
                    {
                        if (argBuffer.Length == 0) throw new ArgumentNullException();

                        tmpArgs.Add(argBuffer);

                        argBuffer = string.Empty;
                        readingArg = false;
                        argWithinQuotes = false;
                    }
                    else if(argWithinQuotes || c != ' ') argBuffer += c;
                }
            }

            if(argBuffer != string.Empty) tmpArgs.Add(argBuffer);

            return tmpArgs.ToArray();
        }

        public override string ToString() => Raw;
    }
}
