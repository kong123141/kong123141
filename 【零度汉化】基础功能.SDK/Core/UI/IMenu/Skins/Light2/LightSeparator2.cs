﻿// <copyright file="LightSeparator2.cs" company="LeagueSharp">
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

namespace LeagueSharp.SDK.Core.UI.IMenu.Skins.Light2
{
    using Enumerations;
    using LeagueSharp.SDK.Core.UI.IMenu.Skins.Light;
    using Math;
    using SharpDX;
    using Values;

    /// <summary>
    ///     Implements <see cref="ADrawable{MenuSeperator}" /> as a default skin.
    /// </summary>
    public class LightSeparator2 : LightSeparator
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="LightSeparator2" /> class.
        /// </summary>
        /// <param name="component">
        ///     The menu component
        /// </param>
        public LightSeparator2(MenuSeparator component)
            : base(component)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Draw a <see cref="MenuSeparator" />
        /// </summary>
        public override void Draw()
        {
            var centerY = LightUtilities.GetContainerRectangle(this.Component)
                .GetCenteredText(
                    null,
                    MenuSettings.Font,
                    this.Component.DisplayName,
                    CenteredFlags.VerticalCenter | CenteredFlags.HorizontalCenter);

            LightMenuSettings.FontCaption.DrawText(
                MenuManager.Instance.Sprite,
                this.Component.DisplayName.ToUpper(),
                (int)centerY.X,
                (int)centerY.Y,
                new ColorBGRA(40, 40, 40, 255));
        }

        #endregion
    }
}