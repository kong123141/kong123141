﻿// <copyright file="MenuCustomizer.cs" company="LeagueSharp">
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

namespace LeagueSharp.SDK.Core.UI.IMenu.Customizer
{
    using SharpDX;
    using SharpDX.Direct3D9;
    using Skins;
    using Values;

    /// <summary>
    ///     This menu allows the user to modify several properties in <see cref="MenuSettings" />.
    /// </summary>
    public sealed class MenuCustomizer : Menu
    {
        #region Static Fields

        /// <summary>
        ///     An instance of this MenuCustomizer
        /// </summary>
        public static MenuCustomizer Instance;

        #endregion

        #region Constructors and Destructors

        private MenuCustomizer(Menu parentMenu)
            : base("menucustomizer", "菜单设置", false, string.Empty)
        {
            parentMenu.Add(this);
            this.BuildCustomizer();
            this.BuildOptions();

            this.ApplyChanges();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Holds the background color
        /// </summary>
        public MenuColor BackgroundColor { get; private set; }

        /// <summary>
        ///     Holds the container height
        /// </summary>
        public MenuSlider ContainerHeight { get; private set; }

        /// <summary>
        ///     Holds the font height
        /// </summary>
        public MenuSlider FontHeight { get; private set; }

        /// <summary>
        ///     True if dragging of the menu is enabled.
        /// </summary>
        public MenuBool LockPosition { get; private set; }

        /// <summary>
        ///     Holds the X position of the menu.
        /// </summary>
        public MenuSlider PositionX { get; private set; }

        /// <summary>
        ///     Holds the Y position of the menu.
        /// </summary>
        public MenuSlider PositionY { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Creates a new instance of MenuCustomizer
        /// </summary>
        /// <param name="menu">The menu that will hold this MenuCustomizer</param>
        public static void Initialize(Menu menu)
        {
            if (Instance == null)
            {
                Instance = new MenuCustomizer(menu);
            }
        }

        #endregion

        #region Methods

        private void AddContainer(Menu menu)
        {
            this.ContainerHeight =
                menu.Add(new MenuSlider("containerheight", "项目高度", MenuSettings.ContainerHeight, 15, 50));
            this.FontHeight =
                menu.Add(new MenuSlider("fontheight", "字体大小", MenuSettings.Font.Description.Height, 10, 30));
            this.BackgroundColor =
                menu.Add(new MenuColor("backgroundcolor", "背景颜色", MenuSettings.RootContainerColor));
        }

        private void AddPosition(Menu menu)
        {
            this.PositionX =
                menu.Add(new MenuSlider("x", "横坐标", (int)MenuSettings.Position.X, 0, Drawing.Width));
            this.PositionY =
                menu.Add(new MenuSlider("y", "纵坐标", (int)MenuSettings.Position.Y, 0, Drawing.Height));
        }

        private void ApplyChanges()
        {
            MenuSettings.Position = new Vector2(this.PositionX.Value, this.PositionY.Value);
            MenuSettings.ContainerHeight = this.ContainerHeight.Value;
            var oldFont = MenuSettings.Font;
            MenuSettings.Font = new Font(
                Drawing.Direct3DDevice,
                this.FontHeight.Value,
                0,
                FontWeight.DoNotCare,
                0,
                false,
                FontCharacterSet.Default,
                FontPrecision.Raster,
                FontQuality.Antialiased,
                FontPitchAndFamily.DontCare | FontPitchAndFamily.Decorative,
                "Tahoma");
            oldFont.Dispose();
            MenuSettings.RootContainerColor = this.BackgroundColor.Color;
            MenuManager.Instance.ResetWidth();
        }

        private void BuildCustomizer()
        {
            var customizeMenu = this.Add(new Menu("customize", "Customize"));
            this.AddPosition(customizeMenu);
            this.AddContainer(customizeMenu);
            customizeMenu.Add(new MenuButton("apply", "Apply Changes", "Apply") { Action = this.ApplyChanges });
            customizeMenu.Add(
                new MenuButton("reset", "Reset Customization", "Reset")
                    {
                        Action = () =>
                            {
                                customizeMenu.RestoreDefault();
                                this.ApplyChanges();
                            }
                    });
        }

        private void BuildOptions()
        {
            this.LockPosition = this.Add(new MenuBool("lock", "Lock Position"));
        }

        #endregion
    }
}