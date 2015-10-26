using LeagueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 花边_花式多合一.Core
{
    class Huabian
    {
        public static Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }
    }
}
