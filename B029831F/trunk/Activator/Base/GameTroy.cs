﻿#region Copyright © 2015 Kurisu Solutions
// All rights are reserved. Transmission or reproduction in part or whole,
// any form or by any means, mechanical, electronical or otherwise, is prohibited
// without the prior written consent of the copyright owner.
// 
// Document:	activator/gametroy.cs
// Date:		01/07/2015
// Author:		Robin Kurisu
#endregion

using System.Collections.Generic;
using LeagueSharp;

namespace Activator.Base
{
    public class Gametroy
    {
        public int Damage;
        public bool Included;
        public string Name;
        public GameObject Obj;
        public Obj_AI_Hero Owner;
        public SpellSlot Slot;
        public int Start;

        public Gametroy(
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

        public static List<Gametroy> Troys = new List<Gametroy>(); 

        static Gametroy()
        {
            
        }
    }
}