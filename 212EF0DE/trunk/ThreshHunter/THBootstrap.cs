using LeagueSharp.Common;
using ThreshHunter.Utility;

namespace ThreshHunter
{
    internal class THBootstrap
    {
        public static void Init()
        {
            Thresh.RootMenu = new Menu("Thresh - Soul Hunter", "dz191.thresh" , true);

            MenuGenerator.Init(Thresh.RootMenu);
            Thresh.Init();
        }
    }
}
