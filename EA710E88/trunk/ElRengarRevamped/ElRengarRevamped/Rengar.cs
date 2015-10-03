namespace ElRengarRevamped
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Color = System.Drawing.Color;

    public enum Spells
    {
        Q,

        W,

        E,

        R
    }

    internal class Rengar : Standards
    {
        #region Static Fields

        public static bool justDoIt;

        public static int LastAutoAttack, Lastrengarq;

        #endregion

        #region Public Methods and Operators

        public static void OnLoad(EventArgs args)
        {
            if (Player.ChampionName != "Rengar")
            {
                return;
            }

            try
            {
                Youmuu = new Items.Item(3142, 0f);

                Ignite = Player.GetSpellSlot("summonerdot");
                Notifications.AddNotification(string.Format("ElRengarRevamped by jQuery v{0}", ScriptVersion), 6000);
                Game.PrintChat(
                    "[00:00] <font color='#f9eb0b'>HEEEEEEY!</font> Use ElUtilitySuite for optimal results! xo jQuery");
                spells[Spells.E].SetSkillshot(0.25f, 70f, 1500f, true, SkillshotType.SkillshotLine);

                MenuInit.Initialize();
                Game.OnUpdate += OnUpdate;
                Drawing.OnDraw += OnDraw;
                CustomEvents.Unit.OnDash += OnDash;
                Drawing.OnEndScene += OnDrawEndScene;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
                Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;
                Orbwalking.AfterAttack += AfterAttack;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Methods

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (Ferocity == 5 && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (unit.IsMe && spells[Spells.Q].IsReady() && target is Obj_AI_Hero && target.IsValidTarget())
                {
                    spells[Spells.Q].Cast();
                    if (RengarQ || RengarE)
                    {
                        Orbwalking.ResetAutoAttackTimer();
                        Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                        Console.WriteLine("Orbwalker Reset in AfterAttack");
                    }
                }
            }
        }

        private static void Heal()
        {
            if (Player.IsRecalling() || Player.InFountain() || Player.Mana <= 4 || RengarR)
            {
                return;
            }

            if (IsActive("Heal.AutoHeal")
                && (Player.Health / Player.MaxHealth) * 100 <= MenuInit.Menu.Item("Heal.HP").GetValue<Slider>().Value
                && spells[Spells.W].IsReady())
            {
                spells[Spells.W].Cast();
                // Console.WriteLine("Casted heal 5 ferocity HP: " + Player.Health );
            }
        }

        private static void OnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            var target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);
            if (!target.IsValidTarget())
            {
                return;
            }

            if (sender.IsMe && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (spells[Spells.Q].IsReady() && target.Distance(Player) < Player.AttackRange && HasPassive
                   && Player.IsDashing() && Ferocity == 5 && IsListActive("Combo.Prio").SelectedIndex == 2)
                {
                    spells[Spells.Q].Cast();
                }
                else if (IsListActive("Combo.Prio").SelectedIndex == 0 && spells[Spells.E].IsReady() && Ferocity == 5
                         && Player.IsDashing())
                {
                    spells[Spells.E].Cast(target);
                }

                if (justDoIt)
                {
                    if (spells[Spells.E].IsReady() && Player.IsDashing())
                    {
                        spells[Spells.E].Cast(target);
                        Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                        Console.WriteLine("here");
                    }

                    if (!spells[Spells.E].IsReady() && spells[Spells.Q].IsReady() && Player.IsDashing())
                    {
                        spells[Spells.Q].Cast();
                        Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                        Console.WriteLine("here 2");
                    }
                }

                SendTime = TickCount;
                Orbwalker.SetMovement(false);

                switch (IsListActive("Combo.Prio").SelectedIndex)
                {
                    case 0:
                        if (spells[Spells.E].IsReady() && spells[Spells.E].CanCast(target) && Ferocity == 5)
                        {
                            spells[Spells.E].Cast(target);
                            Console.WriteLine("E Cast on OnDash" + Player.Mana);
                        }
                        break;

                   case 1:
                        if (IsActive("Beta.Cast.Q") && RengarR)
                        {
                            spells[Spells.E].Cast(target);
                            Console.WriteLine("BETA CAST E ON DASH");
                            return;
                        }

                        spells[Spells.Q].Cast();
                        if (target.IsValidTarget())
                        {
                            if (Player.Distance(target) < spells[Spells.Q].Range + 100)
                            {
                                Utility.DelayAction.Add(
                                    50,
                                    () =>
                                        {
                                            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition)
                                                < spells[Spells.W].Range)
                                            {
                                                spells[Spells.W].Cast();
                                            }

                                            spells[Spells.E].Cast(target.ServerPosition);
                                            UseHydra();
                                            Console.WriteLine("Casted Q Prio");
                                        });
                            }
                        }
                        break;
                }

               /*if (spells[Spells.W].IsReady() && !HasPassive)
                {
                    Utility.DelayAction.Add(100, () => spells[Spells.W].Cast());
                }
                if (spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
                {
                    spells[Spells.E].Cast(target);
                }
                */
                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < spells[Spells.W].Range)
                {
                    UseHydra();
                }

                // Broscience?
                Utility.DelayAction.Add(400, () => Orbwalker.SetMovement(true));
            }
        }

        private static void OnDraw(EventArgs args)
        {
            var drawW = MenuInit.Menu.Item("Misc.Drawings.W").GetValue<Circle>();
            var drawE = MenuInit.Menu.Item("Misc.Drawings.E").GetValue<Circle>();

            var drawSearchRange = MenuInit.Menu.Item("Beta.Search.Range").GetValue<Circle>();
            var searchrange = MenuInit.Menu.Item("Beta.searchrange").GetValue<Slider>().Value;

            var drawsearchrangeQ = MenuInit.Menu.Item("Beta.Search.QCastRange").GetValue<Circle>();
            var searchrangeQCastRange = MenuInit.Menu.Item("Beta.searchrange.Q").GetValue<Slider>().Value;

            if (IsActive("Misc.Drawings.Off"))
            {
                return;
            }

            if (IsActive("Beta.Cast.Q"))
            {
                if (drawSearchRange.Active && spells[Spells.R].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, searchrange, Color.Orange);
                }

                if (drawsearchrangeQ.Active && spells[Spells.R].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, searchrangeQCastRange, Color.Orange);
                }
            }

            if (drawW.Active)
            {
                if (spells[Spells.W].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.W].Range, Color.White);
                }
            }

            if (drawE.Active)
            {
                if (spells[Spells.E].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.E].Range, Color.White);
                }
            }

            if (IsActive("Misc.Drawings.Prioritized"))
            {
                switch (IsListActive("Combo.Prio").SelectedIndex)
                {
                    case 0:
                        Drawing.DrawText(
                            Drawing.Width * 0.70f,
                            Drawing.Height * 0.95f,
                            Color.Yellow,
                            "Prioritized spell: E");
                        break;
                    case 1:
                        Drawing.DrawText(
                            Drawing.Width * 0.70f,
                            Drawing.Height * 0.95f,
                            Color.White,
                            "Prioritized spell: W");
                        break;
                    case 2:
                        Drawing.DrawText(
                            Drawing.Width * 0.70f,
                            Drawing.Height * 0.95f,
                            Color.White,
                            "Prioritized spell: Q");
                        break;
                }
            }
        }

        private static void OnDrawEndScene(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            if (IsActive("Misc.Drawings.Minimap") && spells[Spells.R].Level > 0)
            {
                Utility.DrawCircle(ObjectManager.Player.Position, spells[Spells.R].Range, Color.White, 1, 23, true);
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (RengarQ || RengarE)
                {
                    Orbwalking.ResetAutoAttackTimer();
                }

                if (args.SData.Name == "RengarR")
                {
                    if (Items.CanUseItem(3142))
                    {
                        Utility.DelayAction.Add(1500, () => Items.UseItem(3142));
                    }
                }

                if (args.SData.Name.Contains("Attack") && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                {
                    //Tiamat Hydra cast after AA  - Credits to Kurisu 
                    Utility.DelayAction.Add(
                        50 + (int)(Player.AttackDelay * 100) + Game.Ping / 2 + 10,
                        delegate
                            {
                                if (Items.CanUseItem(3077))
                                {
                                    Items.UseItem(3077);
                                }
                                if (Items.CanUseItem(3074))
                                {
                                    Items.UseItem(3074);
                                }
                            });
                }

                if (args.SData.IsAutoAttack())
                {
                    LastAutoAttack = Utils.GameTimeTickCount;
                }

                if (RengarQ)
                {
                    Lastrengarq = Environment.TickCount;
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
            try
            {

                //Console.WriteLine(IsListActive("Combo.Prio").SelectedIndex);
                SwitchCombo();
                SmiteCombo();
                Heal();
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        ActiveModes.Combo();
                        break;

                    case Orbwalking.OrbwalkingMode.LaneClear:
                        ActiveModes.Laneclear();
                        ActiveModes.Jungleclear();
                        break;

                    case Orbwalking.OrbwalkingMode.Mixed:
                        ActiveModes.Harass();
                        break;
                }

                if (IsActive("Beta.Cast.Q") && IsListActive("Combo.Prio").SelectedIndex == 2)
                {
                    if (IsActive("Beta.Cast.Youmuu") && !Items.HasItem(3142))
                    {
                        return;
                    }

                    var searchrange = MenuInit.Menu.Item("Beta.searchrange").GetValue<Slider>().Value;
                    var explode =
                        ObjectManager.Get<Obj_AI_Hero>()
                            .FirstOrDefault(h => h.IsEnemy && h.IsValidTarget(searchrange, false));
                    if (!explode.IsValidTarget())
                    {
                        return;
                    }

                    var qdelay = MenuInit.Menu.Item("Beta.Cast.Q.Delay").GetValue<Slider>().Value;
                    //var minimumFerocity = MenuInit.Menu.Item("Beta.Ferocity").GetValue<Slider>().Value;
                    var qcastRange = MenuInit.Menu.Item("Beta.searchrange.Q").GetValue<Slider>().Value;

                    if (explode.Distance(Player.ServerPosition) <= qcastRange && Ferocity == 5 && RengarR)
                    {
                        Utility.DelayAction.Add(qdelay, () => spells[Spells.Q].Cast());
                        //Player.IssueOrder(GameObjectOrder.AttackUnit, explode);
                        justDoIt = true;
                        //Console.WriteLine("RengarLogs: CASTED Q WHILE R ");
                    }
                }
               
                //Console.WriteLine(GetEnemy());
                spells[Spells.R].Range = 1000 + spells[Spells.R].Level * 1000;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void OrbwalkingBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.IsValidTarget())
            {
                if (Ferocity <= 4 && Orbwalking.InAutoAttackRange(args.Target) && IsListActive("Combo.Prio").SelectedIndex == 2)
                {
                    spells[Spells.Q].Cast();
                }

                if (Ferocity == 5 && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo
                    && Orbwalking.InAutoAttackRange(args.Target) && IsListActive("Combo.Prio").SelectedIndex == 2)
                {
                    spells[Spells.Q].Cast();
                }

                if (Ferocity == 5 && HasPassive && spells[Spells.Q].IsReady()
                    && IsListActive("Combo.Prio").SelectedIndex == 2)
                {
                    //Console.WriteLine("Before jump");
                    spells[Spells.Q].Cast();
                }
            }
        }

        private static void SwitchCombo()
        {
            var switchTime = Utils.GameTimeTickCount - LastSwitch;
            if (MenuInit.Menu.Item("Combo.Switch").GetValue<KeyBind>().Active && switchTime >= 350)
            {
                switch (IsListActive("Combo.Prio").SelectedIndex)
                {
                    case 0:
                        MenuInit.Menu.Item("Combo.Prio").SetValue(new StringList(new[] { "E", "W", "Q" }, 2));
                        Console.WriteLine("yeee");
                        LastSwitch = Utils.GameTimeTickCount;
                        break;
                    case 1:
                        MenuInit.Menu.Item("Combo.Prio").SetValue(new StringList(new[] { "E", "W", "Q" }, 0));
                        LastSwitch = Utils.GameTimeTickCount;
                        break;

                    default:
                        MenuInit.Menu.Item("Combo.Prio").SetValue(new StringList(new[] { "E", "W", "Q" }, 0));
                        LastSwitch = Utils.GameTimeTickCount;
                        break;
                }
            }
        }

        #endregion
    }
}