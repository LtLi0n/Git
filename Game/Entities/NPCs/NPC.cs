using MUD_Server.Essentials.Framework;
using MUD_Server.Essentials.Scripts;
using System.Threading.Tasks;

namespace MUD_Server.Game.Entities.NPCs
{
    public class NPC
    {
        public string Name { get; }
        public string LastName { get; }
        public Description Description => string.IsNullOrEmpty(_path) ? _desc : new Description(ScriptText.Load(_path + ".gosf").GetAwaiter().GetResult().CinematicText);
        public Color Color { get; }

        private readonly string _path;
        private readonly Description _desc;

        private NPC(string name, string lastname, Color color)
        {
            Name = name;
            LastName = lastname;
            Color = color;
        }
        private NPC(string name, string lastname, Description description, Color color) : this(name, lastname, color) => _desc = description;
        private NPC(string name, string lastname, string descPath, Color color) : this(name, lastname, color) => _path = descPath;

        public static string Say(string text) => $"\"{Color.Parse("white:intense")}{text}{Color.Parse("white")}\"";

        public override string ToString() => Name + (LastName != null ? $" {LastName}" : string.Empty);

        public static class Clacton
        {
            private const string root = @"data\wiki\NPC\Clacton\";

            public static NPC Katra { get; }
            public static NPC Holgard { get; }

            static Clacton()
            {
                Katra = new NPC("Katra", null, root + "katra", Color.Parse("red:bold"));
                Holgard = new NPC("Holgard", null, root + "holgard", Color.Parse("blue:bold"));
            }
        }

        public static NPC Golmodoth = new NPC("Golmodoth", null,
            $"Golmodoth is a demon summoned by ", Color.Parse("red:bold"));


        public static NPC Gierzuk_Engia = new NPC("Gierzuk", "Engia",
            $"Gierzuk Engia is a powerful Dark Elf living inside the island's volcano.\n" +
            $"He once was a good friend of Holgard but after ", Color.Parse("cyan:bold"));

        public static NPC King_William_IV = new NPC("William", "The Forth", string.Empty, Color.Parse("red:bold"));
    }
}
