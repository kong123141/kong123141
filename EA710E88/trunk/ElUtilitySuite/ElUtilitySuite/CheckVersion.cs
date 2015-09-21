namespace ElUtilitySuite
{
    using System;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using LeagueSharp;

    internal class CheckVersion
    {
        #region Static Fields

        private static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;

        #endregion

        #region Public Methods and Operators

        public static void Init()
        {
            try
            {
                var request =
                    WebRequest.Create(
                        "https://github.com/AlterEgojQuery/ElBundle/blob/master/ElUtilitySuite/ElUtilitySuite/Properties/AssemblyInfo.cs");
                var response = request.GetResponse();
                var data = response.GetResponseStream();
                string version = null;
                if (data != null)
                {
                    using (var sr = new StreamReader(data))
                    {
                        version = sr.ReadToEnd();
                    }
                }
                const string Pattern = @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}";
                if (version != null)
                {
                    var serverVersion = new Version(new Regex(Pattern).Match(version).Groups[0].Value);

                    if (serverVersion > Version)
                    {
                        Game.PrintChat(
                            "<font color='#cc0000'>ElUtilitySuite</font> There is a new version available, please recompile.");
                    }

                    if (serverVersion == Version)
                    {
                        Game.PrintChat("<font color='#0dd629'>ElUtilitySuite</font> Your version is up-to-date, nice!");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
    }
}