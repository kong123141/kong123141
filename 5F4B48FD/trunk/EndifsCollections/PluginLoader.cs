using LeagueSharp;

namespace EndifsCollections
{
    class PluginLoader
    {
        public PluginLoader()
        {
            switch (ObjectManager.Player.ChampionName.ToLower())
            {
                case "aatrox":
                    new Plugins.Aatrox();
                    break;
                case "alistar":
                    new Plugins.Alistar();
                    break;
                case "diana":
                    new Plugins.Diana();
                    break;
                /* Outdated
                case "fiora":
                    new Plugins.Fiora();
                    break;
                */
                case "khazix":
                    new Plugins.Khazix();
                    break;
                case "kassadin":
                    new Plugins.Kassadin();
                    break;
                case "malzahar":
                    new Plugins.Malzahar();
                    break;
                case "masteryi":
                    new Plugins.MasterYi();
                    break;
                case "nocturne":
                    new Plugins.Nocturne();
                    break;
                case "pantheon":
                    new Plugins.Pantheon();
                    break;
                case "nasus":
                    new Plugins.Nasus();
                    break;
                case "sejuani":
                    new Plugins.Sejuani();
                    break;
                case "fiddlesticks":
                    new Plugins.FiddleSticks();
                    break;
                case "sona":
                    new Plugins.Sona();
                    break;
                case "teemo":
                    new Plugins.Teemo();
                    break;
                case "thresh":
                    new Plugins.Thresh();
                    break;
                case "tryndamere":
                    new Plugins.Tryndamere();
                    break;
                case "urgot":
                    new Plugins.Urgot();
                    break;
                case "vi":
                    new Plugins.Vi();
                    break;
                case "volibear":
                    new Plugins.Volibear();
                    break;
                case "leona":
                    new Plugins.Leona();
                    break;
                case "rengar":
                    new Plugins.Rengar();
                    break;
                case "shyvana":
                    new Plugins.Shyvana();
                    break;
                case "vladimir":
                    new Plugins.Vladimir();
                    break;
                case "evelynn":
                    new Plugins.Evelynn();
                    break;
                case "nami":
                    new Plugins.Nami();
                    break;
                case "morgana":
                    new Plugins.Morgana();
                    break;
                case "annie":
                    new Plugins.Annie();
                    break;
                case "lissandra":
                    new Plugins.Lissandra();
                    break;
                case "poppy":
                    new Plugins.Poppy();
                    break;
                case "garen":
                    new Plugins.Garen();
                    break;
              case "sivir":
                    new Plugins.Sivir();
                    break;
              case "udyr":
                    new Plugins.Udyr();
                    break;
              case "kogmaw":
                    new Plugins.KogMaw();
                    break;
              case "renekton":
                    new Plugins.Renekton();
                    break;
              case "irelia":
                    new Plugins.Irelia();
                    break;
            }
        }
    }
}
