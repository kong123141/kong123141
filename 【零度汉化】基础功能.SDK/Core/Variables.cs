﻿// <copyright file="Variables.cs" company="LeagueSharp">
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

namespace LeagueSharp.SDK.Core
{
    using System;

    using LeagueSharp.SDK.Core.UI.IMenu;
    using LeagueSharp.SDK.Core.Wrappers.Orbwalking;
    using LeagueSharp.SDK.Core.Wrappers.TargetSelector;

    /// <summary>
    ///     Class that contains helpful variables.
    /// </summary>
    public class Variables
    {
        #region Static Fields

        /// <summary>
        ///     The league version.
        /// </summary>
        public static readonly Version GameVersion = new Version(Game.Version);

        /// <summary>
        ///     The kit version.
        /// </summary>
        public static readonly Version KitVersion = typeof(Bootstrap).Assembly.GetName().Version;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the Orbwalker instance.
        /// </summary>
        /// <value>
        ///     The orbwalker.
        /// </value>
        public static Orbwalker Orbwalker { get; internal set; }

        /// <summary>
        ///     Gets the TargetSelector instance.
        /// </summary>
        /// <value>
        ///     The targetselector.
        /// </value>
        public static TargetSelector TargetSelector { get; internal set; }

        /// <summary>
        ///     Gets the Safe TickCount.
        /// </summary>
        public static int TickCount => (int)(Game.ClockTime * 1000);

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the LeagueSharp menu.
        /// </summary>
        /// <value>
        ///     The LeagueSharp menu.
        /// </value>
        internal static Menu LeagueSharpMenu { get; set; }

        #endregion
    }
}