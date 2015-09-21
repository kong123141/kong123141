using LeagueSharp;
using VHR_SDK.Utility;

namespace VHR_SDK
{
    using LeagueSharp.SDK.Core;

    class Program
    {
        static void Main(string[] args)
        {
            LeagueSharp.SDK.Core.Events.Load.OnLoad += (sender, eventArgs) =>
            {
                if (ObjectManager.Player.ChampionName != "Vayne")
                {
                    VHRDebug.WriteInfo("[VHR] You are not playing Vayne!");
                    return;
                }

                Bootstrap.Init(args);
                VHRBootstrap.Init();
            };
        }
    }
}
