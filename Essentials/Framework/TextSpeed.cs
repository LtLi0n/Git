using System;
using System.Collections.Generic;
using System.Text;

namespace MUD_Server.Essentials.Framework
{
    public class TextSpeed
    {
        public int Speed { get; }
        private readonly bool _simulate;

        public TextSpeed(int speed, bool simulate)
        {
            Speed = speed;
            _simulate = simulate;
        }

        public static TextSpeed NORMAL = new TextSpeed(25, false);
        //public static TextSpeed NORMAL = new TextSpeed(0, false);

        public static TextSpeed HOLD_500MS = new TextSpeed(500, true);
        public static TextSpeed HOLD_1S = new TextSpeed(1000, true);

        public static TextSpeed Create(int speed, bool simulate = false) => new TextSpeed(speed, simulate);

        public override string ToString() => "→" + Speed + (_simulate ? '↥' : '↑');
    }
}
