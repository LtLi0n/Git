using MUD_Server.Game.Entities.Players;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MUD_Server.Essentials.Framework
{
    public class CinematicDisplay
    {
        private readonly string _renderInfo;

        public CinematicDisplay(string renderInfo) => _renderInfo = renderInfo;

        public async Task Render(SocketUser user, bool clearScreen)
        {
            user.IsInCinematic = true;
            
            int speed = TextSpeed.NORMAL.Speed;

            ArgumentException ansi_e = new ArgumentException("Ansi code was not in a valid format");
            ArgumentException speed_e = new ArgumentException("Speed code was not in a valid format");

            Color color = Color.Parse("white");
            string ansi = string.Empty;
            string speedCode = string.Empty;
            string speedNumBuff = string.Empty;

            //clear user screen
            if(clearScreen) await user.ClearScreen();

            foreach (char c in _renderInfo)
            {
                //skip if user entered 's'
                if (user.RecentInput == "s")
                {
                    await user.SendMessageAsync(await user.ActiveCommandNode.GetDynamicDescription(user, false, true), true);
                    break;
                }
                if (ansi == string.Empty && speedCode == string.Empty)
                {
                    if (c == '\x1b') ansi += c;
                    else if (c == '→') speedCode += c;
                    else
                    {
                        await user.SendMessageAsync($"{color}{c}", false, false, false);
                        await Task.Delay(speed);
                    }
                }
                //I sense ansi code
                else if (ansi != string.Empty)
                {
                    ansi += c;

                    var colorMatch = System.Text.RegularExpressions.Regex.Match(ansi, @"\x1b\[\d;\d{2}m");
                    var escapeMatch = System.Text.RegularExpressions.Regex.Match(ansi, @"\x1b\[2J");

                    if (colorMatch.Success || escapeMatch.Success)
                    {
                        if (colorMatch.Success) color = Color.FromAnsi(ansi);
                        else await user.SendMessageAsync(ansi); //clear screen

                        ansi = string.Empty;
                    }
                }
                //I sense custom speed code
                else if (speedCode != string.Empty)
                {
                    if (c == '↥' || c == '↑')
                    {
                        speed = int.Parse(speedNumBuff);
                        speedNumBuff = string.Empty;
                        speedCode = string.Empty;

                        if (c == '↥') await Task.Delay(speed);
                    }
                    else speedNumBuff += c;
                }
            }

            user.IsInCinematic = false;

            await user.SendMessageAsync(Color.Parse("white"), newLine: false, forceColorAtEnd: false);
        }
    }
}
