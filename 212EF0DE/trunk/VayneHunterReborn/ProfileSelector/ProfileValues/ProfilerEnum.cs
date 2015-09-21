using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VayneHunter_Reborn.ProfileSelector.ProfileValues
{
    internal enum MenuCategory
    {
        Combo, Harass, Misc, Farm, Cleanser
    }

    internal enum MajorCategories
    {
        Tumble, Condemn, General
    }

    internal enum MinorCategories
    {
        QLogic, SmartQ, NoAAStealth, NoQEnemies, DynamicQSafety, QSpam, QInRange, Mirin, WallTumble,
        CondemnMethod, PushDistance, Accuracy, ENextAuto, OnlyStunCurrent, AutoE, EKS, NoEAA, TrinketBush, EThird, LowLifePeel, CondemnTurret, CondemnFlag, NoETurret,
        AntiGP, Interrupt, AntiGPDelay, SpecialFocus, Reveal, DisableMovement, Permashow
        
    }

    internal enum ValueTypes
    {
        Boolean, Keybind, Slider, Stringlist
    }
}
