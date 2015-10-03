﻿#region Copyright © 2015 Kurisu Solutions
// All rights are reserved. Transmission or reproduction in part or whole,
// any form or by any means, mechanical, electronical or otherwise, is prohibited
// without the prior written consent of the copyright owner.
// 
// Document:	Base/GameTroy.cs
// Date:		22/09/2015
// Author:		Robin Kurisu
#endregion

using LeagueSharp;
using System.Collections.Generic;

namespace Activator.Base
{
    public class GameTroy
    {
        public int Damage;
        public bool Included;
        public string Name;
        public GameObject Obj;
        public Obj_AI_Hero Owner;
        public SpellSlot Slot;
        public int Start;

        public GameTroy(
            Obj_AI_Hero owner, 
            SpellSlot slot, 
            string name, 
            int start, 
            bool inculded, 
            int incdmg = 0,
            GameObject obj = null)
        {
            Owner = owner;
            Slot = slot;
            Start = start;
            Name = name;
            Obj = obj;
            Included = inculded;
            Damage = incdmg;
        }

        public static List<GameTroy> Troys = new List<GameTroy>(); 

        static GameTroy()
        {
            
        }
    }
}
