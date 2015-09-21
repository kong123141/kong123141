using LeagueSharp.Common;
using System;
using System.Reflection;

namespace Talon.Helper
{
    internal class Updater
    {
        private static readonly System.Version Version = Assembly.GetExecutingAssembly().GetName().Version;
        public static void Init(string path)
        {
            try
            {
                var data = new BetterWebClient(null).DownloadString("https://raw.github.com/" + path + "/Properties/AssemblyInfo.cs");
                foreach (var line in data.Split('\n'))
                {
                    if (line.StartsWith("//"))
                    {
                        continue;
                    }

                    if (line.StartsWith("[assembly: AssemblyVersion"))
                    {
                        var serverVersion = new System.Version(line.Substring(28, (line.Length - 4) - 28 + 1));
                        if (serverVersion > Version)
                        {
                            Config.ShowNotification(
                                "Update avalible: " + Version + " => " + serverVersion, System.Drawing.Color.Red, 10000);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Config.ShowNotification("No Update avalible: "+Version, System.Drawing.Color.GreenYellow, 5000);
        }
    }
}
