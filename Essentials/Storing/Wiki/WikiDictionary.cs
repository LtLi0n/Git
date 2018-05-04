using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MUD_Server.Essentials.Storing
{
    ///<summary>This object does not store any WikiObjects, it just reads or writes them to local storage.</summary>
    public class WikiDictionary : Dictionary<string, WikiObject>
    {
        public static WikiDictionary Wiki { get; private set; }
        public static void Init(string path) => Wiki = new WikiDictionary(path);

        private readonly string _root;

        public WikiDictionary(string root) => _root = root;

        ///<summary>Returns wiki object from root directory</summary>
        /// <param name="key">Path to the wiki object. 'NPC:the_king', 'System:node_manager'.</param>
        /// <returns></returns>
        public new WikiObject this[string key] => ReadWikiObject(_root + key).GetAwaiter().GetResult();

        private async Task<WikiObject> ReadWikiObject(string pathWithType)
        {
            string typeStr = pathWithType.Substring(0, pathWithType.IndexOf(':'));

            string path = pathWithType.Substring(typeStr.Length + 1) + ".gosf";

            return await ReadWikiObject(pathWithType.Substring(typeStr.Length + 1) + ".gosf", (WikiObjectType)Enum.Parse(typeof(WikiObjectType), Path.GetFileName(typeStr)));
        }

        ///<summary>Read from disk.</summary>
        private async Task<WikiObject> ReadWikiObject(string path, WikiObjectType type) => new WikiObject(type, Path.GetFileNameWithoutExtension(path), await File.ReadAllTextAsync(_root + Enum.GetName(typeof(WikiObjectType), type) + '\\' + path), _root);

        //path from _root
        public async Task Write(WikiObject wObject, string name) => await File.WriteAllTextAsync($@"{_root}\{Enum.GetName(typeof(WikiObjectType), wObject.Type)}\{name}.gosf", wObject.Description);
    }
}
