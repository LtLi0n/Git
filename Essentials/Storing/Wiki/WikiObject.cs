
namespace MUD_Server.Essentials.Storing
{
    public class WikiObject
    {
        public WikiObjectType Type { get; }
        public string Description { get; }
        public string Path { get; }
        public string Name { get; }

        public WikiObject(WikiObjectType type, string name, string description, string root)
        {
            Type = type;
            Description = description;
            Name = name;

            Path = root + name;
        }
    }
}
