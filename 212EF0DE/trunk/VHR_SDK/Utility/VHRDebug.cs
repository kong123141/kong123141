using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Utils;

namespace VHR_SDK.Utility
{
    class VHRDebug
    {
        public static void WriteDebug(object info)
        {
            Logging.Write()(LogLevel.Debug, info);
        }

        public static void WriteInfo(object info)
        {
            Logging.Write()(LogLevel.Info, info);
        }

        public static void WriteError(object error)
        {
            Logging.Write()(LogLevel.Error, error);
        }

        public static void WriteFatal(object fatal)
        {
            Logging.Write()(LogLevel.Fatal, fatal);
        }
    }
}
