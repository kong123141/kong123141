using EndifsCreations.Plugins;
using LeagueSharp;

namespace EndifsCreations
{
    class PluginLoader
    {
        public PluginLoader()
        {
            switch (ObjectManager.Player.ChampionName.ToLower())
            {
                case "aatrox":
                    new Aatrox();
                    break;
                case "ahri":
                    new Ahri();
                    break;
                case "akali":
                    new Akali();
                    break;
                case "alistar":
                    new Alistar();
                    break;
                case "amumu":
                    new Amumu();
                    break;
                case "anivia":
                    new Anivia();
                    break;
                case "annie":
                    new Annie();
                    break;
                case "ashe":
                    new Ashe();
                    break;
                case "azir":
                    new Azir();
                    break;/*
                case "bard":
                    new Bard();
                    break;*/
                case "blitzcrank":
                    new Blitzcrank();
                    break;/*
                case "brand":
                    new Brand();
                    break;*/
                case "braum":
                    new Braum();
                    break;
                case "caitlyn":
                    new Caitlyn();
                    break;
                case "cassiopeia":
                    new Cassiopeia();
                    break;
                case "chogath":
                    new Chogath();
                    break;
                case "corki":
                    new Corki();
                    break;/*
                case "darius":
                    new Darius();
                    break;*/
                case "diana":
                    new Diana();
                    break;
                case "drmundo":
                    new DrMundo();
                    break;
                case "draven":
                    new Draven();
                    break;/*
                case "ekko":
                    new Ekko();
                    break;
                case "elise":
                    new Elise();
                    break;*/
                case "evelynn":
                    new Evelynn();
                    break;
                case "ezreal":
                    new Ezreal();
                    break;
                case "fiddlesticks":
                    new FiddleSticks();
                    break;
                case "fiora":
                    new Fiora();
                    break;/*
                case "fizz":
                    new Fizz();
                    break;*/
                case "galio":
                    new Galio();
                    break;
                case "gangplank":
                    new Gangplank();
                    break;
                case "garen":
                    new Garen();
                    break;                
                case "gnar": 
                    new Gnar();
                    break;                                
                case "gragas":
                    new Gragas();
                    break;/*
                case "graves":
                    new Graves();
                    break;
                case "hecarim":
                    new Hecarim();
                    break;*/
                case "heimerdinger":
                    new Heimerdinger();
                    break;
                case "irelia":
                    new Irelia();
                    break;
                case "janna":
                    new Janna();
                    break;
                case "jarvaniv":
                    new JarvanIV();
                    break;
                case "jax":
                    new Jax();
                    break;                
                case "jayce":
                    new Jayce();
                    break;                                
                case "jinx":
                    new Jinx();
                    break;/*
                case "kalista":
                    new Kalista();
                    break;*/
                case "karma":
                    new Karma();
                    break;
                case "karthus":
                    new Karthus();
                    break;                
                case "kassadin":
                    new Kassadin();
                    break;/*
                case "katarina":
                    new Katarina();
                    break;*/
                case "kayle":
                    new Kayle();
                    break;
                case "kennen":
                    new Kennen();
                    break;
                case "khazix":
                    new Khazix();
                    break;
                case "kogmaw":
                    new KogMaw();
                    break;/*
                case "leblanc":
                    new LeBlanc();
                    break;
                case "leesin":
                    new LeeSin();
                    break;*/
                case "leona":
                    new Leona();
                    break;
                case "lissandra":
                    new Lissandra();
                    break;
                case "lucian":
                    new Lucian();
                    break;/*
                case "lulu":
                    new LuLu();
                    break;*/
                case "lux":
                    new Lux();
                    break;
                case "malphite":
                    new Malphite();
                    break;
                case "malzahar":
                    new Malzahar();
                    break;
                case "maokai":
                    new Maokai();
                    break;
                case "masteryi":
                    new MasterYi();
                    break;
                case "missfortune":
                    new MissFortune();
                    break;
                case "mordekaiser":
                    new Mordekaiser();
                    break;
                case "morgana":
                    new Morgana();
                    break;
                case "nami":
                    new Nami();
                    break;
                case "nasus":
                    new Nasus();
                    break;/*
                case "nautilus":
                    new Nautilus();
                    break;*/
                case "nidalee":
                    new Nidalee();
                    break;                 
                case "nocturne":
                    new Nocturne();
                    break;
                case "nunu":
                    new Nunu();
                    break;
                case "olaf":
                    new Olaf();
                    break;/*
                case "orianna":
                    new Orianna();
                    break;*/
                case "pantheon":
                    new Pantheon();
                    break;
                case "poppy":
                    new Poppy();
                    break;/*
                case "quinn":
                    new Quinn();
                    break;*/                
                case "rammus":
                    new Rammus();
                    break;/*
                case "reksai":
                    new RekSai();
                    break;*/
                case "renekton":
                    new Renekton();
                    break;
                case "rengar":
                    new Rengar();
                    break;/*
                 case "riven":
                    new Riven();
                    break;
                 case "rumble":
                    new Rumble();
                    break;
                 case "ryze":
                    new Ryze();
                    break;*/
                case "sejuani":
                    new Sejuani();
                    break;                
                case "shaco":
                    new Shaco();
                    break;
                case "shen":
                    new Shen();
                    break;
                case "shyvana":
                    new Shyvana();
                    break;
                case "singed":
                    new Singed();
                    break;              
                case "sion":
                    new Sion();
                    break;                 
                case "sivir":
                    new Sivir();
                    break;
                case "skarner":
                    new Skarner();
                    break;
                case "sona":
                    new Sona();
                    break;
                case "soraka":
                    new Soraka();
                    break;                
                case "swain":
                    new Swain();
                    break;
                case "syndra":
                    new Syndra();
                    break; /*         
                case "tahmkench":
                    new TahmKench();
                    break;           
                case "talon":
                    new Talon();
                    break;*/
                case "taric":
                    new Taric();
                    break;
                case "teemo":
                    new Teemo();
                    break;
                case "thresh":
                    new Thresh();
                    break;                
                case "tristana":
                    new Tristana();
                    break; 
                case "trundle":
                    new Trundle();
                    break;
                case "tryndamere":
                    new Tryndamere();
                    break;
                case "twistedfate":
                    new TwistedFate();
                    break;                 
                case "twitch":
                    new Twitch();
                    break;
                case "udyr":
                    new Udyr();
                    break;
                case "urgot":
                    new Urgot();
                    break;
                case "varus":
                    new Varus();
                    break;
                case "vayne":
                    new Vayne();
                    break; 
                case "veigar":
                    new Veigar();
                    break;
                case "velkoz":
                    new Velkoz();
                    break;
                case "vi":
                    new Vi();
                    break;                
                case "viktor":
                    new Viktor();
                    break;                
                case "vladimir":
                    new Vladimir();
                    break;
                case "volibear":
                    new Volibear();
                    break;
                case "warwick":
                    new Warwick();
                    break;
                case "monkeyking":
                    new MonkeyKing();
                    break;/*
                case "xerath":
                    new Xerath();
                    break;*/
                case "xinzhao":
                    new XinZhao();
                    break;
                case "yasuo":
                    new Yasuo();
                    break;/*
                case "yorick":
                    new Yorick();
                    break;
                case "zac":
                    new Zac();
                    break;
                case "zed":
                    new Zed();
                    break;*/
                case "ziggs":
                    new Ziggs();
                    break;
                case "zilean":
                    new Zilean();
                    break;
                case "zyra":
                    new Zyra();
                    break;
            }
        }
    }
}