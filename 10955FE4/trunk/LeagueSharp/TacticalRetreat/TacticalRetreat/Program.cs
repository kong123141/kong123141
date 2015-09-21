using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TacticalRetreat.Plugins;


namespace TacticalRetreat
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        public static void OnGameLoad(EventArgs args)
        {
            try
            {
                switch (ObjectManager.Player.ChampionName)
                {
                    case "Aatrox":
                        Aatrox.Load();
                        break;
                    case "Ahri":
                        Ahri.Load();
                        break;
                    case "Akali":
                        Akali.Load();
                        break;
                    case "Alistar":
                        Alistar.Load();
                        break;
                    case "Amumu":
                        Amumu.Load();
                        break;
                    case "Anivia":
                        Anivia.Load();
                        break;
                    case "Annie":
                        Annie.Load();
                        break;
                    case "Ashe":
                        Ashe.Load();
                        break;
                    case "Azir":
                        Azir.Load();
                        break;
                    case "Bard":
                        Bard.Load();
                        break;
                    case "Blitzcrank":
                        Blitzcrank.Load();
                        break;
                    case "Brand":
                        Brand.Load();
                        break;
                    case "Braum":
                        Braum.Load();
                        break;
                    case "Caitlyn":
                        Caitlyn.Load();
                        break;
                    case "Cassiopeia":
                        Cassiopeia.Load();
                        break;
                    case "Chogath":
                        ChoGath.Load();
                        break;
                    case "Corki":
                        Corki.Load();
                        break;
                    case "Darius":
                        Darius.Load();
                        break;
                    case "Diana":
                        Diana.Load();
                        break;
                    case "Draven":
                        Draven.Load();
                        break;
                    case "Ekko":
                        Ekko.Load();
                        break;
                    case "Elise":
                        Elise.Load();
                        break;
                    case "Evelynn":
                        Evelynn.Load();
                        break;
                    case "Ezreal":
                        Ezreal.Load();
                        break;
                    case "Fiddlesticks":
                        Fiddlesticks.Load();
                        break;
                    case "Fiora":
                        Fiora.Load();
                        break;
                    case "Fizz":
                        Fizz.Load();
                        break;
                    case "Galio":
                        Galio.Load();
                        break;
                    case "Gangplank":
                        Gangplank.Load();
                        break;
                    case "Garen":
                        Garen.Load();
                        break;
                    case "Gnar":
                        Gnar.Load();
                        break;
                    case "Gragas":
                        Gragas.Load();
                        break;
                    case "Graves":
                        Graves.Load();
                        break;
                    case "Hecarim":
                        Hecarim.Load();
                        break;
                    case "Heimerdinger":
                        Heimerdinger.Load();
                        break;
                    case "Irelia":
                        Irelia.Load();
                        break;
                    case "Janna":
                        Janna.Load();
                        break;
                    case "JarvanIV":
                        JarvanIV.Load();
                        break;
                    case "Jax":
                        Jax.Load();
                        break;
                    case "Jayce":
                        Jayce.Load();
                        break;
                    case "Jinx":
                        Jinx.Load();
                        break;
                    case "Kalista":
                        Kalista.Load();
                        break;
                    case "Karma":
                        Karma.Load();
                        break;
                    case "Karthus":
                        Karthus.Load();
                        break;
                    case "Kassadin":
                        Kassadin.Load();
                        break;
                    case "Katarina":
                        Katarina.Load();
                        break;
                    case "Kayle":
                        Kayle.Load();
                        break;
                    case "Kennen":
                        Kennen.Load();
                        break;
                    case "Khazix":
                        KhaZix.Load();
                        break;
                    case "KogMaw":
                        KogMaw.Load();
                        break;
                    case "LeBlanc":
                        LeBlanc.Load();
                        break;
                    case "LeeSin":
                        LeeSin.Load();
                        break;
                    case "Leona":
                        Leona.Load();
                        break;
                    case "Lissandra":
                        Lissandra.Load();
                        break;
                    case "Lucian":
                        Lucian.Load();
                        break;
                    case "Lulu":
                        Lulu.Load();
                        break;
                    case "Lux":
                        Lux.Load();
                        break;
                    case "Malphite":
                        Malphite.Load();
                        break;
                    case "Malzahar":
                        Malzahar.Load();
                        break;  
                    case "Maokai":
                        Maokai.Load();
                        break;
                    case "MasterYi":
                        MasterYi.Load();
                        break;
                    case "MissFortune":
                        MissFortune.Load();
                        break;
                    case "Mordekaiser":
                        Mordekaiser.Load();
                        break;
                    case "Morgana":
                        Morgana.Load();
                        break;
                    case "DrMundo":
                        Mundo.Load();
                        break;
                    case "Nami":
                        Nami.Load();
                        break;
                    case "Nasus":
                        Nasus.Load();
                        break;
                    case "Nautilus":
                        Nautilus.Load();
                        break;
                    case "Nidalee":
                        Nidalee.Load();
                        break;
                    case "Nocturne":
                        Nocturne.Load();
                        break;
                    case "Nunu":
                        Nunu.Load();
                        break;
                    case "Olaf":
                        Olaf.Load();
                        break;
                    case "Orianna":
                        Orianna.Load();
                        break;
                    case "Pantheon":
                        Pantheon.Load();
                        break;  
                    case "Poppy":
                        Poppy.Load();
                        break;
                    case "Quinn":
                        Quinn.Load();
                        break;
                    case "Rammus":
                        Rammus.Load();
                        break;
                    case "Reksai":
                        RekSai.Load();
                        break;
                    case "Renekton":
                        Renekton.Load();
                        break;
                    case "Rengar":
                        Rengar.Load();
                        break;
                    case "Riven":
                        Riven.Load();
                        break;
                    case "Rumble":
                        Rumble.Load();
                        break;
                    case "Ryze":
                        Ryze.Load();
                        break;
                    case "Sejuani":
                        Sejuani.Load();
                        break;
                    case "Shaco":
                        Shaco.Load();
                        break;
                    case "Shen":
                        Shen.Load();
                        break;
                    case "Shyvana":
                        Shyvana.Load();
                        break;
                    case "Singed":
                        Singed.Load();
                        break;
                    case "Sion":
                        Sion.Load();
                        break;
                    case "Sivir":
                        Sivir.Load();
                        break;
                    case "Skarner":
                        Skarner.Load();
                        break;
                    case "Sona":
                        Sona.Load();
                        break;
                    case "Soraka":
                        Soraka.Load();
                        break;
                    case "Swain":
                        Swain.Load();
                        break;
                    case "Syndra":
                        Syndra.Load();
                        break;
                    case "Talon":
                        Talon.Load();
                        break;
                    case "Taric":
                        Taric.Load();
                        break; 
                    case "Teemo":
                        Teemo.Load();
                        break;
                    case "Thresh":
                        Thresh.Load();
                        break;
                    case "Tristana":
                        Tristana.Load();
                        break;
                    case "Trundle":
                        Trundle.Load();
                        break;  
                    case "Tryndamere":
                        Tryndamere.Load();
                        break;
                    case "TwistedFate":
                        TwistedFate.Load();
                        break;
                    case "Twitch":
                        Twitch.Load();
                        break;
                    case "Udyr":
                        Udyr.Load();
                        break;
                    case "Urgot":
                        Urgot.Load();
                        break;
                    case "Varus":
                        Varus.Load();
                        break;
                    case "Vayne":
                        Vayne.Load();
                        break;
                    case "Veigar":
                        Veigar.Load();
                        break;
                    case "Velkoz":
                        VelKoz.Load();
                        break;
                    case "Vi":
                        Vi.Load();
                        break;
                    case "Viktor":
                        Viktor.Load();
                        break;
                    case "Vladimir":
                        Vladimir.Load();
                        break;
                    case "Volibear":
                        Volibear.Load();
                        break;
                    case "Warwick":
                        Warwick.Load();
                        break;
                    case "MonkeyKing":
                        Wukong.Load();
                        break;
                    case "Xerath":
                        Xerath.Load();
                        break;
                    case "XinZhao":
                        XinZhao.Load();
                        break;
                    case "Yasuo":
                        Yasuo.Load();
                        break;
                    case "Yorick":
                        Yorick.Load();
                        break;
                    case "Zac":
                        Zac.Load();
                        break;
                    case "Zed":
                        Zed.Load();
                        break;
                    case "Ziggs":
                        Ziggs.Load();
                        break;
                    case "Zilean":
                        Zilean.Load();
                        break;
                    case "Zyra":
                        Zyra.Load();
                        break;





                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
