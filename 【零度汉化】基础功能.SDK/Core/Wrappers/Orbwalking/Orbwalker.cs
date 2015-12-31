// <copyright file="Orbwalker.cs" company="LeagueSharp">
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

namespace LeagueSharp.SDK.Core.Wrappers.Orbwalking
{
    using System;
    using System.Linq;
    using System.Windows.Forms;

    using LeagueSharp.SDK.Core.Enumerations;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.Extensions.SharpDX;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;
    using LeagueSharp.SDK.Core.Utils;
    using LeagueSharp.SDK.Core.Wrappers.Damages;

    using SharpDX;

    using Color = System.Drawing.Color;
    using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

    /// <summary>
    ///     The <c>Orbwalker</c> system.
    /// </summary>
    public sealed class Orbwalker : Base<OrbwalkingMode, AttackableUnit>
    {
        #region Fields

        /// <summary>
        ///     The <c>orbwalker</c> menu.
        /// </summary>
        internal readonly Menu Menu = new Menu("orbwalker", "走砍设置");

        /// <summary>
        ///     The <see cref="Selector" /> class.
        /// </summary>
        internal readonly Selector Selector;

        private readonly Random random = new Random(DateTime.Now.Millisecond);

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Orbwalker" /> class.
        /// </summary>
        /// <param name="menu">The menu.</param>
        internal Orbwalker(Menu menu)
        {
            var drawing = new Menu("drawings", "显示设置");
            drawing.Add(new MenuBool("drawAARange", "自己AA范围", true));
            drawing.Add(new MenuBool("drawAARangeEnemy", "敌人AA范围"));
            drawing.Add(new MenuBool("drawExtraHoldPosition", "显示不动区域"));
            drawing.Add(new MenuBool("drawKillableMinion", "显示可击杀小兵(线圈)"));
            drawing.Add(new MenuBool("drawKillableMinionFade", "显示可击杀小兵(淡出淡入效果)"));
            this.Menu.Add(drawing);

            var advanced = new Menu("advanced", "激活设置");

            advanced.Add(new MenuSeparator("separatorMovement", "移动"));
            advanced.Add(new MenuBool("movementRandomize", "随机位置移动", true));
            advanced.Add(new MenuSlider("movementExtraHold", "额外不动区域设置", 0, 0, 250));
            advanced.Add(new MenuSlider("movementMaximumDistance", "最小距离", 1500, 500, 1500));

            advanced.Add(new MenuSeparator("separatorDelay", "延迟设置"));
            advanced.Add(new MenuSlider("delayMovement", "移动延迟", 0, 0, 500));
            advanced.Add(new MenuSlider("delayWindup", "走砍后摇", 80, 0, 200));
            advanced.Add(new MenuSlider("delayFarm", "打钱延迟", 30, 0, 200));

            advanced.Add(new MenuSeparator("separatorPrioritization", "优先权"));
            advanced.Add(new MenuBool("prioritizeFarm", "骚扰时优先补刀", true));
            advanced.Add(new MenuBool("prioritizeMinions", "优先打残血兵"));
            advanced.Add(new MenuBool("prioritizeSmallJungle", "优先打小野怪"));
            advanced.Add(new MenuBool("prioritizeWards", "优先打眼睛"));
            advanced.Add(new MenuBool("prioritizeSpecialMinions", "优先打大车 大野怪"));

            advanced.Add(new MenuSeparator("separatorAttack", "攻击设置"));
            advanced.Add(new MenuBool("attackWards", "眼位"));
            advanced.Add(new MenuBool("attackBarrels", "船长桶"));
            advanced.Add(new MenuBool("attackClones", "植物"));
            advanced.Add(new MenuBool("attackSpecialMinions", "大车 大野怪", true));

            advanced.Add(new MenuSeparator("separatorMisc", "杂项设置"));
            advanced.Add(new MenuBool("miscMissile", "启动 碰撞检测", true));
            advanced.Add(new MenuBool("miscAttackSpeed", "攻速超过2.5关闭移动", true));

            this.Menu.Add(advanced);

            this.Menu.Add(new MenuSeparator("separatorKeys", "按键设置"));
            this.Menu.Add(new MenuKeyBind("lasthitKey", "补刀按键", Keys.X, KeyBindType.Press));
            this.Menu.Add(new MenuKeyBind("laneclearKey", "清线按键", Keys.V, KeyBindType.Press));
            this.Menu.Add(new MenuKeyBind("hybridKey", "骚扰按键", Keys.C, KeyBindType.Press));
            this.Menu.Add(new MenuKeyBind("comboKey", "连招按键", Keys.Space, KeyBindType.Press));
            this.Menu.Add(new MenuBool("enabledOption", "启用SDK走砍", true));

            this.Menu.MenuValueChanged += (sender, args) =>
                {
                    var keyBind = sender as MenuKeyBind;
                    if (keyBind != null)
                    {
                        var modeName = keyBind.Name.Substring(0, keyBind.Name.IndexOf("Key", StringComparison.Ordinal));
                        OrbwalkingMode mode;
                        this.ActiveMode = Enum.TryParse(modeName, true, out mode)
                                              ? keyBind.Active
                                                    ? mode
                                                    : mode == this.ActiveMode
                                                          ? this.Menu["lasthitKey"].GetValue<MenuKeyBind>().Active
                                                                ? OrbwalkingMode.LastHit
                                                                : this.Menu["laneclearKey"].GetValue<MenuKeyBind>()
                                                                      .Active
                                                                      ? OrbwalkingMode.LaneClear
                                                                      : this.Menu["hybridKey"].GetValue<MenuKeyBind>()
                                                                            .Active
                                                                            ? OrbwalkingMode.Hybrid
                                                                            : this.Menu["comboKey"]
                                                                                  .GetValue<MenuKeyBind>().Active
                                                                                  ? OrbwalkingMode.Combo
                                                                                  : OrbwalkingMode.None
                                                          : this.ActiveMode
                                              : this.ActiveMode;
                    }

                    var boolean = sender as MenuBool;
                    if (boolean != null)
                    {
                        if (boolean.Name.Equals("enabledOption"))
                        {
                            this.Enabled = boolean.Value;
                        }
                    }
                };

            menu.Add(this.Menu);
            this.Selector = new Selector(this);
            this.Enabled = this.Menu["enabledOption"].GetValue<MenuBool>().Value;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Block Attack & Move methods until tick
        /// </summary>
        public int BlockOrdersUntilTick { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="Base{TK, T}" /> is enabled.
        /// </summary>
        /// <value>
        ///     <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                if (base.Enabled != value)
                {
                    if (value)
                    {
                        Drawing.OnDraw += this.OnDrawingDraw;
                    }
                    else
                    {
                        Drawing.OnDraw -= this.OnDrawingDraw;
                    }
                }
                base.Enabled = value;
                if (this.Menu != null)
                {
                    this.Menu["enabledOption"].GetValue<MenuBool>().Value = value;
                }
            }
        }

        /// <summary>
        ///     Forces the orbwalker to select the set target if valid and in range.
        /// </summary>
        public AttackableUnit ForceTarget
        {
            get
            {
                return this.Selector.ForceTarget;
            }
            set
            {
                this.Selector.ForceTarget = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Issues the attack order.
        /// </summary>
        /// <param name="target">
        ///     The target to attack.
        /// </param>
        public override void Attack(AttackableUnit target)
        {
            if (this.BlockOrdersUntilTick - Variables.TickCount > 0)
            {
                return;
            }

            var gTarget = target ?? this.Selector.GetTarget(this.ActiveMode);
            if (gTarget.IsValidTarget())
            {
                var eventArgs = new OrbwalkingActionArgs
                                    {
                                        Target = gTarget, Position = gTarget.Position, Process = true,
                                        Type = OrbwalkingType.BeforeAttack
                                    };
                this.InvokeAction(eventArgs);

                if (eventArgs.Process)
                {
                    if (GameObjects.Player.CanCancelAutoAttack())
                    {
                        this.MissileLaunched = false;
                    }
                    if (GameObjects.Player.IssueOrder(GameObjectOrder.AttackUnit, gTarget))
                    {
                        this.LastAutoAttackCommandTick = Variables.TickCount;
                        this.LastTarget = gTarget;
                    }
                    this.BlockOrdersUntilTick = Variables.TickCount + 70 + Math.Min(60, Game.Ping);
                }
            }
        }

        /// <summary>
        ///     Indicates whether the orbwalker can issue attacking.
        /// </summary>
        /// <param name="extraWindup">
        ///     The extra windup.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public override bool CanAttack(float extraWindup)
        {
            var extraAttackDelay = 0f;
            if (GameObjects.Player.ChampionName.Equals("Graves"))
            {
                if (!GameObjects.Player.HasBuff("GravesBasicAttackAmmo1"))
                {
                    return false;
                }
                var attackDelay = GameObjects.Player.AttackDelay * 1000f;
                extraAttackDelay = attackDelay * 1.0740296828f - 716.2381256175f - attackDelay;
            }
            return base.CanAttack(extraWindup + extraAttackDelay);
        }

        /// <summary>
        ///     Indicates whether the orbwalker can issue moving.
        /// </summary>
        /// <param name="extraWindup">
        ///     The extra windup.
        /// </param>
        /// <param name="disableMissileCheck">
        ///     If set to <c>true</c> [disable missile check].
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public override bool CanMove(float extraWindup, bool disableMissileCheck)
        {
            var localExtraWindup = 0;
            if (GameObjects.Player.ChampionName.Equals("Rengar")
                && (GameObjects.Player.HasBuff("rengarqbase") || GameObjects.Player.HasBuff("rengarqemp")))
            {
                localExtraWindup = 200;
            }
            return
                base.CanMove(
                    extraWindup + localExtraWindup + this.Menu["advanced"]["delayWindup"].GetValue<MenuSlider>().Value,
                    disableMissileCheck || !this.Menu["advanced"]["miscMissile"].GetValue<MenuBool>().Value);
        }

        /// <summary>
        ///     Gets the target.
        /// </summary>
        /// <returns>
        ///     Returns the target filtered by the selector and/or by the target selector if available.
        /// </returns>
        public override AttackableUnit GetTarget()
        {
            return this.Selector.GetTarget(this.ActiveMode);
        }

        /// <summary>
        ///     Issue the move order.
        /// </summary>
        /// <param name="position">The position.</param>
        public override void Move(Vector3 position)
        {
            if (this.BlockOrdersUntilTick - Variables.TickCount > 0)
            {
                return;
            }

            if (Variables.TickCount - this.LastMovementOrderTick
                < this.Menu["advanced"]["delayMovement"].GetValue<MenuSlider>().Value)
            {
                return;
            }

            if (this.Menu["advanced"]["miscAttackSpeed"].GetValue<MenuBool>().Value
                && (GameObjects.Player.AttackDelay < 1 / 2.6f) && this.TotalAutoAttacks % 3 != 0
                && !this.CanMove(500, true))
            {
                return;
            }

            if (position.Distance(GameObjects.Player.Position)
                < GameObjects.Player.BoundingRadius
                + this.Menu["advanced"]["movementExtraHold"].GetValue<MenuSlider>().Value)
            {
                if (GameObjects.Player.Path.Length > 0)
                {
                    var eventStopArgs = new OrbwalkingActionArgs
                                            {
                                                Position = GameObjects.Player.ServerPosition, Process = true,
                                                Type = OrbwalkingType.StopMovement
                                            };
                    this.InvokeAction(eventStopArgs);
                    if (eventStopArgs.Process)
                    {
                        GameObjects.Player.IssueOrder(GameObjectOrder.Stop, eventStopArgs.Position);
                        this.LastMovementOrderTick = Variables.TickCount - 70;
                    }
                }
                return;
            }

            if (position.Distance(GameObjects.Player.ServerPosition) < GameObjects.Player.BoundingRadius)
            {
                position = GameObjects.Player.ServerPosition.Extend(
                    position,
                    GameObjects.Player.BoundingRadius + this.random.Next(0, 51));
            }

            var maximumDistance = this.Menu["advanced"]["movementMaximumDistance"].GetValue<MenuSlider>().Value;
            if (position.Distance(GameObjects.Player.ServerPosition) > maximumDistance)
            {
                position = GameObjects.Player.ServerPosition.Extend(
                    position,
                    maximumDistance + 25 - this.random.Next(0, 51));
            }

            if (this.Menu["advanced"]["movementRandomize"].GetValue<MenuBool>().Value
                && GameObjects.Player.Distance(position) > 350f)
            {
                var rAngle = 2D * Math.PI * this.random.NextDouble();
                var radius = GameObjects.Player.BoundingRadius / 2f;
                var x = (float)(position.X + radius * Math.Cos(rAngle));
                var y = (float)(position.Y + radius * Math.Sin(rAngle));
                position = new Vector3(x, y, NavMesh.GetHeightForPosition(x, y));
            }

            var angle = 0f;
            var currentPath = GameObjects.Player.GetWaypoints();
            if (currentPath.Count > 1 && currentPath.PathLength() > 100)
            {
                var movePath = GameObjects.Player.GetPath(position);
                if (movePath.Length > 1)
                {
                    var v1 = currentPath[1] - currentPath[0];
                    var v2 = movePath[1] - movePath[0];
                    angle = v1.AngleBetween(v2);
                    var distance = movePath.Last().DistanceSquared(currentPath.Last());
                    if ((angle < 10 && distance < 500 * 500) || distance < 50 * 50)
                    {
                        return;
                    }
                }
            }

            if (Variables.TickCount - this.LastMovementOrderTick < 70 + Math.Min(60, Game.Ping) && angle < 60)
            {
                return;
            }

            if (angle >= 60 && Variables.TickCount - this.LastMovementOrderTick < 60)
            {
                return;
            }

            var eventArgs = new OrbwalkingActionArgs
                                { Position = position, Process = true, Type = OrbwalkingType.Movement };
            this.InvokeAction(eventArgs);

            if (eventArgs.Process)
            {
                GameObjects.Player.IssueOrder(GameObjectOrder.MoveTo, eventArgs.Position);
                this.LastMovementOrderTick = Variables.TickCount;
            }
        }

        /// <summary>
        ///     Indicates whether the depended process should wait before executing.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public override bool ShouldWait()
        {
            return this.Selector.ShouldWait();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     OnDraw event.
        /// </summary>
        /// <param name="args">
        ///     The event data
        /// </param>
        private void OnDrawingDraw(EventArgs args)
        {
            if (GameObjects.Player == null || !GameObjects.Player.IsValid || GameObjects.Player.IsDead)
            {
                return;
            }

            if (GameObjects.Player.Position.IsValid())
            {
                if (this.Menu["drawings"]["drawAARange"].GetValue<MenuBool>().Value
                    && GameObjects.Player.Position.IsOnScreen(GameObjects.Player.GetRealAutoAttackRange()))
                {
                    Drawing.DrawCircle(
                        GameObjects.Player.Position,
                        GameObjects.Player.GetRealAutoAttackRange(),
                        Color.Blue);
                }

                if (this.Menu["drawings"]["drawExtraHoldPosition"].GetValue<MenuBool>().Value
                    && GameObjects.Player.Position.IsOnScreen())
                {
                    Drawing.DrawCircle(
                        GameObjects.Player.Position,
                        GameObjects.Player.BoundingRadius
                        + this.Menu["advanced"]["movementExtraHold"].GetValue<MenuSlider>().Value,
                        Color.Purple);
                }
            }

            if (this.Menu["drawings"]["drawAARangeEnemy"].GetValue<MenuBool>().Value)
            {
                foreach (var enemy in
                    GameObjects.EnemyHeroes.Where(
                        e => e.IsValidTarget() && e.Position.IsOnScreen(e.GetRealAutoAttackRange(GameObjects.Player))))
                {
                    Drawing.DrawCircle(enemy.Position, enemy.GetRealAutoAttackRange(GameObjects.Player), Color.Blue);
                }
            }

            if (this.Menu["drawings"]["drawKillableMinion"].GetValue<MenuBool>().Value)
            {
                if (this.Menu["drawings"]["drawKillableMinionFade"].GetValue<MenuBool>().Value)
                {
                    var minions =
                        this.Selector.GetEnemyMinions(GameObjects.Player.GetRealAutoAttackRange() * 2f)
                            .Where(
                                m =>
                                m.Position.IsOnScreen() && m.Health < GameObjects.Player.GetAutoAttackDamage(m) * 2f);
                    foreach (var minion in minions)
                    {
                        var value = 255 - (minion.Health * 2);
                        value = value > 255 ? 255 : value < 0 ? 0 : value;

                        Drawing.DrawCircle(
                            minion.Position,
                            minion.BoundingRadius * 2f,
                            Color.FromArgb(255, 0, 255, (byte)(255 - value)));
                    }
                }
                else
                {
                    var minions =
                        this.Selector.GetEnemyMinions(GameObjects.Player.GetRealAutoAttackRange() * 2f)
                            .Where(m => m.Position.IsOnScreen() && m.Health < GameObjects.Player.GetAutoAttackDamage(m));
                    foreach (var minion in minions)
                    {
                        Drawing.DrawCircle(minion.Position, minion.BoundingRadius * 2f, Color.FromArgb(255, 0, 255, 0));
                    }
                }
            }
        }

        #endregion
    }
}