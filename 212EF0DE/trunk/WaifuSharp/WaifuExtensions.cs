using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;

namespace WaifuSharp
{
    static class WaifuExtensions
    {
            public static string GetLast(this string source, int tail_length)
            {
                if (tail_length >= source.Length)
                    return source;
                return source.Substring(source.Length - tail_length);
            }
    }
}
