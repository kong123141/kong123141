using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;
using LeagueSharp;

namespace 花边_花式多合一
{
    class CheckVersion
    {
        public static System.Version Version;

        //Update by h3h3
        internal static void Game_OnGameLoad(EventArgs args)
        {
            Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        // updater by h3h3
                        using (var c = new WebClient())
                        {
                            var rawVersion =
                                c.DownloadString(
                                    "https://github.com/CHA2172886/NewFlowers/blob/master/CheckVersion/DuoHeYi.cs");

                            var match =
                                new Regex(
                                    @"\[assembly\: AssemblyVersion\(""(\d{1,})\.(\d{1,})\.(\d{1,})\.(\d{1,})""\)\]")
                                    .Match(rawVersion);

                            Version = Assembly.GetExecutingAssembly().GetName().Version;

                            if (match.Success)
                            {
                                var gitVersion =
                                    new Version(
                                        string.Format(
                                            "{0}.{1}.{2}.{3}",
                                            match.Groups[1],
                                            match.Groups[2],
                                            match.Groups[3],
                                            match.Groups[4]));

                                if (gitVersion != Version)
                                {
                                    Game.PrintChat("<font color=\"#FF0000\">鑺辫竟-鑺卞紡澶氬悎涓€</font> - 宸茬粡鏇存柊,璇蜂笅杞芥柊鐗堟湰!");
                                }
                            }
                        }
                    }

                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
        }
    }
}
