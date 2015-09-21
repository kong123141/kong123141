﻿#region Copyright © 2015 Kurisu Solutions
// All rights are reserved. Transmission or reproduction in part or whole,
// any form or by any means, mechanical, electronical or otherwise, is prohibited
// without the prior written consent of the copyright owner.
// 
// Document:	activator/enumerators.cs
// Date:		01/07/2015
// Author:		Robin Kurisu
#endregion

using System.Collections.Generic;
using Activator.Items;
using Activator.Spells;
using Activator.Summoners;

namespace Activator.Base
{
    public class Lists
    {
        public static List<CoreItem> Items = new List<CoreItem>();
        public static List<CoreItem> BoughtItems = new List<CoreItem>();
        public static List<CoreSpell> Spells = new List<CoreSpell>();
        public static List<CoreSum> Summoners = new List<CoreSum>();
    }

    public enum HitType
    {
        None,
        AutoAttack,
        MinionAttack,
        TurretAttack,
        Spell,
        Danger,
        Ultimate,
        CrowdControl,
        Stealth,
        ForceExhaust
    }

    public enum MapType
    {        
        Common = 0,
        SummonersRift = 1,
        CrystalScar = 2,
        TwistedTreeline = 3,
        HowlingAbyss = 4
    }

    public enum MenuType
    {
        Zhonyas,
        Stealth,
        Cleanse,
        SlowRemoval,
        SpellShield,
        ActiveCheck,
        SelfCount,
        SelfMuchHP,
        SelfLowHP,
        SelfLowMP,
        SelfMinMP,
        SelfMinHP,
        EnemyLowHP
    }
}
