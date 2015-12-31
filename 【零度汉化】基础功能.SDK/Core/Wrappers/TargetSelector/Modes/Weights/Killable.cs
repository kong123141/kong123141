﻿// <copyright file="Killable.cs" company="LeagueSharp">
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

namespace LeagueSharp.SDK.Core.Wrappers.TargetSelector.Modes.Weights
{
    using LeagueSharp.SDK.Core.Wrappers.Damages;

    /// <summary>
    ///     AA Killable
    /// </summary>
    public class Killable : IWeightItem
    {
        #region Public Properties

        public int DefaultWeight => 20;

        public string DisplayName => "AA Killable";

        public bool Inverted => false;

        public string Name => "aa-killable";

        #endregion

        #region Public Methods and Operators

        public float GetValue(Obj_AI_Hero hero) => hero.Health < GameObjects.Player.GetAutoAttackDamage(hero) ? 1 : 0;

        #endregion
    }
}