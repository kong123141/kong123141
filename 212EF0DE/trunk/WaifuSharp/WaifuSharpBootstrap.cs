using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;

namespace WaifuSharp
{
    class WaifuSharpBootstrap
    {
        //Suggestions:
        //
        //Random anime girl sometimes saying notice me senpai
        //http://i.imgur.com/l1G8yAm.png
        //http://i.imgur.com/StoWRWQ.png
        //http://i.imgur.com/vQmPWdd.png
        //http://i.imgur.com/9wa0NW3.png
        //http://i.imgur.com/y7a5s8C.png
        //http://i.imgur.com/qnpt7hO.png
        //Waifu# - Asuna & Blacky powered by Fukas/OutRageOusMe
        internal static void OnLoad()
        {
            WaifuSharp.Menu = new Menu("Waifu#", "waifusharp", true);

            WaifuSelector.WaifuSelector.OnLoad();
            Levelmanager.LevelManager.Onload();
            WaifuSharp.OnLoad();
        }
    }
}
