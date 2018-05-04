using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace MUD_Server.Essentials.Framework
{
    public enum ColorName { Black, Red, Green, Yellow, Blue, Purple, Cyan, White };
    public enum ColorType{ None, Bold, Intense, Underline };

    public class Color
    {
        public static string QuestionString => $"{GetColor(ColorName.White)}[ {GetColor(ColorName.Green, ColorType.Intense)}Y{GetColor(ColorName.White)}/{GetColor(ColorName.Red, ColorType.Intense)}N {GetColor(ColorName.White)}]{GetColor(ColorName.White, ColorType.Intense)}?";

        public static ColorCollection Colors { get; private set; }

        public static Color GetColor(ColorName color, ColorType colorType = ColorType.None)
        {
            string colorName = Enum.GetName(typeof(ColorName), color).ToLower();

            if (colorType == ColorType.Bold) return Colors[$"{colorName}:bold"];
            if (colorType == ColorType.Intense) return Colors[$"{colorName}:intense"];
            if (colorType == ColorType.Underline) return Colors[$"{colorName}:underline"];

            return Colors[colorName];
        }

        public static Color Parse(string color) => Colors[color.ToLower()];
        public static Color FromAnsi(string fullColorAnsi) => Colors.First(x => x.Value.ToString() == fullColorAnsi).Value;

        public static string Highlight(object pointOfInterest, bool speaking = false) => Colors["green:bold"] + pointOfInterest.ToString() + (speaking ? Colors["white:intense"] : Colors["white"]);
        ///<summary>Show user that this is a point of interest. If it's bold, it means the user still hadn't explored it yet.</summary>
        public static string Hint(object pointOfInterest, bool speaking = false) => Colors["cyan:bold"] + pointOfInterest.ToString() + (speaking ? Colors["white:intense"] : Colors["white"]);

        public string ReadFormat => Colors.First(x => x.Value.ToString() == this.ToString()).Key;

        public static void InitCollection() => Colors = new ColorCollection();

        public class ColorCollection : Dictionary<string, Color>
        {
            public ColorCollection()
            {
                //https://gist.github.com/chrisopedia/8754917

                //Regular colors
                Add("black", new Color("30m", 0));
                Add("red", new Color("31m", 0));
                Add("green", new Color("32m", 0));
                Add("yellow", new Color("33m", 0));
                Add("blue", new Color("34m", 0));
                Add("purple", new Color("35m", 0));
                Add("cyan", new Color("36m", 0));
                Add("white", new Color("37m", 0));

                //Bold colors
                Add("black:bold", new Color("30m", 1));
                Add("red:bold", new Color("31m", 1));
                Add("green:bold", new Color("32m", 1));
                Add("yellow:bold", new Color("33m", 1));
                Add("blue:bold", new Color("34m", 1));
                Add("purple:bold", new Color("35m", 1));
                Add("cyan:bold", new Color("36m", 1));
                Add("white:bold", new Color("37m", 1));

                //Intense colors
                Add("black:intense", new Color("90m", 0));
                Add("red:intense", new Color("91m", 0));
                Add("green:intense", new Color("92m", 0));
                Add("yellow:intense", new Color("93m", 0));
                Add("blue:intense", new Color("94m", 0));
                Add("purple:intense", new Color("95m", 0));
                Add("cyan:intense", new Color("96m", 0));
                Add("white:intense", new Color("97m", 0));

                //Underline colors
                Add("black:underline", new Color("30m", 4));
                Add("red:underline", new Color("31m", 4));
                Add("green:underline", new Color("32m", 4));
                Add("yellow:underline", new Color("33m", 4));
                Add("blue:underline", new Color("34m", 4));
                Add("purple:underline", new Color("35m", 4));
                Add("cyan:underline", new Color("36m", 4));
                Add("white:underline", new Color("37m", 4));
            }
        }

        private readonly string _ansiCode;
        private readonly byte _mode;

        private Color(string ansiCode, byte mode)
        {
            _ansiCode = ansiCode;
            _mode = mode;
        }

        public override string ToString() => $"\x1b[{_mode};{_ansiCode}";
    }
}
