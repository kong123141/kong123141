﻿// <copyright file="SpellType.cs" company="LeagueSharp">
//    Copyright (c) 2015 LeagueSharp.
// 
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
// 
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
// 
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see http://www.gnu.org/licenses/
// </copyright>

namespace LeagueSharp.SDK.Core.Enumerations
{
    /// <summary>
    ///     <c>SpellType</c> enumeration
    /// </summary>
    public enum SpellType
    {
        /// <summary>
        ///     The spell is a Circle Skillshot
        /// </summary>
        SkillshotCircle,

        /// <summary>
        ///     The spell is a Line Skillshot
        /// </summary>
        SkillshotLine,

        /// <summary>
        ///     The spell is a Line Skillshot that creates a Missile
        /// </summary>
        SkillshotMissileLine,

        /// <summary>
        ///     The spell is a Cone Skillshot
        /// </summary>
        SkillshotCone,

        /// <summary>
        ///     The spell is a Cone Skillshot that leaves a Missile
        /// </summary>
        SkillshotMissileCone,

        /// <summary>
        ///     The spell is a Ring Skillshot (Veigar E)
        /// </summary>
        SkillshotRing,

        /// <summary>
        ///     The spell is an Arc Skillshot (Diana Q)
        /// </summary>
        SkillshotArc,

        /// <summary>
        ///     The spell is Targeted
        /// </summary>
        Targeted,

        /// <summary>
        ///     The spell can be toggled on/off
        /// </summary>
        Toggled,

        /// <summary>
        ///     The spell can be activated, after which it lasts for a while (Vayne R, Olaf R)
        /// </summary>
        Activated,

        /// <summary>
        ///     The spell does nothing else but contain a passive (Vayne W, Mini Gnar W)
        /// </summary>
        Passive
    }
}