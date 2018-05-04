using MUD_Server.Essentials.Framework;
using MUD_Server.Essentials.Storing;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MUD_Server.Essentials.Commands
{
    public abstract class CommandNodeCollection
    {
        public static WikiDictionary Wiki => WikiDictionary.Wiki;

        ///<summary>A collection to force hint/hightling all defined entries from description.</summary>
        private readonly IEnumerable<string> _forceMarkCollection;

        public CommandNode Entry { get; protected set; }

        protected CommandNodeCollection() => Entry = null;
        protected CommandNodeCollection(IEnumerable<string> forceMarkCollection) : this() => _forceMarkCollection = forceMarkCollection.OrderBy(x => x.Length);

        public void CreateConnection(CommandNode parent, CommandNode toInsert) => parent.Connections.Add(toInsert);
        public void CreateConnections(CommandNode parent, params CommandNode[] toInsert)
        {
            foreach (CommandNode cn in toInsert)
            {
                parent.Connections.Add(cn);
            }
        }

        private bool IsFinalCollectionVariable(string buffer)
        {
            int n = 0;

            string trigger = string.Empty;

            foreach (string mark in _forceMarkCollection)
            {
                if (mark.ToLower() == buffer.ToLower())
                {
                    n++;
                    trigger = mark;
                    break;
                }
            }

            foreach(string mark in _forceMarkCollection)
            {
                if(n > 0)
                {
                    if (mark.ToLower().Contains(buffer.ToLower()) && mark != trigger) return false;
                }
            }

            return n == 1;
        }

        private bool FollowsCollectionVariable(string buffer)
        {
            foreach (string mark in _forceMarkCollection)
            {
                if (mark.StartsWith(buffer)) return true;
            }

            return false;
        }

        ///<summary>Applies hints and markings to the description from ForceMarkCollection.</summary>
        public string ForceMarkDescription(CommandNode node, string description)
        {
            string output = description;

            if(_forceMarkCollection != null)
            {
                string buffer = string.Empty;
                output = string.Empty;
                //this is used to keep track of original colors
                string ongoingColor = Color.Parse("white").ToString();

                for(int i = 0; i < description.Length; i++)
                {
                    //is color code, detect and skip other characters
                    if (description[i] == '\x1b')
                    {
                        ongoingColor = description.Substring(i, 7);
                        output += ongoingColor;
                        i += 7;
                    }

                    buffer += description[i];
                    output += description[i];

                    if(IsFinalCollectionVariable(buffer))
                    {
                        //remove buffer data from output
                        output = output.Remove(output.Length - buffer.Length, buffer.Length);

                        //highlight : hint
                        output += node.Connections.Exists(x => Regex.IsMatch(buffer, x.Regex)) ? Color.Highlight(buffer) : Color.Hint(buffer);

                        output += ongoingColor;
                        buffer = string.Empty;
                    }
                    //if buffer does not contain any of the mark beginnings, reset
                    else if(!FollowsCollectionVariable(buffer))
                    {
                        buffer = string.Empty;
                    }
                }
            }

            return output;
        }
    }
}
