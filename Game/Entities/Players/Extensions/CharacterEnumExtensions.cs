using DataBaseFormats.Entities.Players;
using System;
using static MUD_Server.Essentials.Framework.Color;

namespace MUD_Server.Game.Players.Entities.Extensions
{
    public static class CharacterEnumExtensions
    {
        public static string GetDescription(this Race race)
        {
            switch(race)
            {
                case Race.Human:
                    return
                        $"Humans function in huge societies.\n" +
                        $"They usually live in castles or outskirts and as such are very social.";
                case Race.Dwarf:
                    return 
                        $"Dwarves live underground. There, they feel much safer because of the tunnels they dug which intruders can't comprehend.\n" +
                        $"Basically ants.";
                case Race.Elf:
                    return
                        $"Elves most commonly live in forested areas.\n" +
                        $"Their excellent sense of sight make them {Parse("white:intense")}better archers and hunters{Parse("white")}.";
            }

            throw new NotImplementedException();
        }

        public static string GetDescription(this Class classValue)
        {
            switch (classValue)
            {
                case Class.Warrior:
                    return $"Fury warrior btw.";
                case Class.Ranger:
                    return $"Pew pew from bew bew.";
                case Class.Mage:
                    return $"Skadie skadi, you are on fire teehee.";
            }

            throw new NotImplementedException();
        }

        public static string GetDescription(this Zodiac zodiac)
        {
            switch (zodiac)
            {
                case Zodiac.Aquarius: return "Deep, imaginative, original and uncompromising.";
                case Zodiac.Aries: return "Eager, dynamic, quick and competitive.";
                case Zodiac.Cancer: return "Intuitive, sentimental, compassionate and protective.";
                case Zodiac.Capricorn: return "Serious, independent, disciplined and tenacious.";
                case Zodiac.Gemini: return "Versatile, expressive, curious and kind.";
                case Zodiac.Leo: return "Dramatic, outgoing, fiery and self-assured.";
                case Zodiac.Libra: return "Social, fair-minded, diplomatic and gracious.";
                case Zodiac.Pisces: return "Affectionate, emphatetic, wise and artistic.";
                case Zodiac.Sagittarius: return "Extroverted, optimistic, funny and generous.";
                case Zodiac.Scorpio: return "Passionate, stubborn, resourceful and brave.";
                case Zodiac.Taurus: return "Strong, dependable, sensual and creative.";
                case Zodiac.Virgo: return "Practical, loyal, gentle and analytical.";
            }

            throw new NotImplementedException();
        }
    }
}
