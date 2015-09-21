namespace ElEasy.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using ItemData = LeagueSharp.Common.Data.ItemData;

    public class Ryze : Standards
    {
        #region Static Fields

        private static readonly Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>
                                                                       {
                                                                           { Spells.Q, new Spell(SpellSlot.Q, 900) },
                                                                           { Spells.W, new Spell(SpellSlot.W, 600) },
                                                                           { Spells.E, new Spell(SpellSlot.E, 600) },
                                                                           { Spells.R, new Spell(SpellSlot.R) }
                                                                       };

        #endregion

        #region Public Methods and Operators

        public static void Load()
        {
            Ignite = Player.GetSpellSlot("summonerdot");
            spells[Spells.Q].SetSkillshot(
                spells[Spells.Q].Instance.SData.SpellCastTime,
                spells[Spells.Q].Instance.SData.LineWidth,
                spells[Spells.Q].Instance.SData.MissileSpeed,
                true,
                SkillshotType.SkillshotLine);

            Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        #endregion

        #region Methods

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var gapCloserActive = Menu.Item("ElEasy.Ryze.Interrupt.Activated").GetValue<bool>();

            if (gapCloserActive && spells[Spells.W].IsReady()
                && gapcloser.Sender.Distance(Player) < spells[Spells.W].Range)
            {
                spells[Spells.W].CastOnUnit(gapcloser.Sender);
            }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (spells[Spells.Q].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (spells[Spells.W].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);
            }

            if (spells[Spells.E].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);
            }

            if (spells[Spells.R].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
            }

            return (float)damage;
        }

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        private static void Initialize()
        {
            Menu = new Menu("ElRyze", "menu", true);

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            Menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElEasy.Ryze.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Ryze.Combo.W", "Use W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Ryze.Combo.E", "Use E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Ryze.Combo.R", "Use R").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Ryze.Combo.R.HP", "Use R when HP").SetValue(new Slider(100)));
            cMenu.AddItem(new MenuItem("ElEasy.Ryze.Combo.Ignite", "Use Ignite").SetValue(true));

            Menu.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElEasy.Ryze.Harass.Q", "Use Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Ryze.Harass.W", "Use W").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Ryze.Harass.E", "Use E").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Ryze.Harass.Player.Mana", "Minimum Mana").SetValue(new Slider(55)));

            hMenu.SubMenu("Harass")
                .SubMenu("AutoHarass settings")
                .AddItem(
                    new MenuItem("ElEasy.Ryze.AutoHarass.Activated", "Auto harass", true).SetValue(
                        new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle)));
            hMenu.SubMenu("Harass")
                .SubMenu("AutoHarass settings")
                .AddItem(new MenuItem("ElEasy.Ryze.AutoHarass.Q", "Use Q").SetValue(true));
            hMenu.SubMenu("Harass")
                .SubMenu("AutoHarass settings")
                .AddItem(new MenuItem("ElEasy.Ryze.AutoHarass.W", "Use W").SetValue(true));

            hMenu.SubMenu("Harass")
                .SubMenu("AutoHarass settings")
                .AddItem(new MenuItem("ElEasy.Ryze.AutoHarass.E", "Use E").SetValue(true));

            hMenu.SubMenu("Harass")
                .SubMenu("AutoHarass settings")
                .AddItem(new MenuItem("ElEasy.Ryze.AutoHarass.Mana", "Minimum mana").SetValue(new Slider(55)));

            Menu.AddSubMenu(hMenu);

            var clearMenu = new Menu("Clear", "Clear");
            clearMenu.SubMenu("Lasthit").AddItem(new MenuItem("ElEasy.Ryze.Lasthit.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Lasthit").AddItem(new MenuItem("ElEasy.Ryze.Lasthit.W", "Use E").SetValue(true));
            clearMenu.SubMenu("Lasthit").AddItem(new MenuItem("ElEasy.Ryze.Lasthit.E", "Use W").SetValue(true));

            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Ryze.LaneClear.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Ryze.LaneClear.W", "Use W").SetValue(true));
            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Ryze.LaneClear.E", "Use E").SetValue(true));
            clearMenu.SubMenu("Jungleclear").AddItem(new MenuItem("ElEasy.Ryze.JungleClear.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Jungleclear").AddItem(new MenuItem("ElEasy.Ryze.JungleClear.W", "Use W").SetValue(true));
            clearMenu.SubMenu("Jungleclear").AddItem(new MenuItem("ElEasy.Ryze.JungleClear.E", "Use E").SetValue(true));
            clearMenu.AddItem(
                new MenuItem("ElEasy.Ryze.Clear.Player.Mana", "Minimum Mana for clear").SetValue(new Slider(55)));

            Menu.AddSubMenu(clearMenu);

            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("ElEasy.Ryze.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElEasy.Ryze.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Ryze.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Ryze.Draw.E", "Draw E").SetValue(new Circle()));

            var dmgAfterE = new MenuItem("ElEasy.Ryze.DrawComboDamage", "Draw combo damage").SetValue(true);
            var drawFill =
                new MenuItem("ElEasy.Ryze.DrawColour", "Fill colour", true).SetValue(
                    new Circle(true, Color.FromArgb(0xcc, 0xcc, 0x0, 0x0)));
            miscMenu.AddItem(drawFill);
            miscMenu.AddItem(dmgAfterE);

            DrawDamage.DamageToUnit = GetComboDamage;
            DrawDamage.Enabled = dmgAfterE.GetValue<bool>();
            DrawDamage.Fill = drawFill.GetValue<Circle>().Active;
            DrawDamage.FillColor = drawFill.GetValue<Circle>().Color;

            dmgAfterE.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                    {
                        DrawDamage.Enabled = eventArgs.GetNewValue<bool>();
                    };

            drawFill.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    DrawDamage.Fill = eventArgs.GetNewValue<Circle>().Active;
                    DrawDamage.FillColor = eventArgs.GetNewValue<Circle>().Color;
                };

            miscMenu.AddItem(new MenuItem("ElEasy.Ryze.GapCloser.Activated", "Anti gapcloser").SetValue(true));
            miscMenu.AddItem(new MenuItem("ElEasy.Ryze.AA", "Don't use AA in combo").SetValue(false));

            Menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = Menu.AddSubMenu(new Menu("Credits", "jQuery"));
            credits.AddItem(new MenuItem("ElEasy.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElEasy.Email", "info@zavox.nl"));

            Menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            Menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            Menu.AddToMainMenu();
        }

        private static void OnAutoHarass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            var useQ = Menu.Item("ElEasy.Ryze.AutoHarass.Q").GetValue<bool>();
            var useW = Menu.Item("ElEasy.Ryze.AutoHarass.W").GetValue<bool>();
            var useE = Menu.Item("ElEasy.Ryze.AutoHarass.E").GetValue<bool>();
            var mana = Menu.Item("ElEasy.Ryze.AutoHarass.Mana").GetValue<Slider>().Value;

            if (Player.Mana < mana)
            {
                return;
            }

            if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                var pred = spells[Spells.Q].GetPrediction(target);
                if (pred.Hitchance >= HitChance.High && pred.CollisionObjects.Count == 0)
                {
                    spells[Spells.Q].Cast(target);
                }
            }

            if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(target))
            {
                spells[Spells.W].CastOnUnit(target);
            }

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
            {
                spells[Spells.E].CastOnUnit(target);
            }
        }

        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.W].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            var useQ = Menu.Item("ElEasy.Ryze.Combo.Q").GetValue<bool>();
            var useW = Menu.Item("ElEasy.Ryze.Combo.W").GetValue<bool>();
            var useE = Menu.Item("ElEasy.Ryze.Combo.E").GetValue<bool>();
            var useR = Menu.Item("ElEasy.Ryze.Combo.R").GetValue<bool>();
            var rHp = Menu.Item("ElEasy.Ryze.Combo.R.HP").GetValue<Slider>().Value;
            var useI = Menu.Item("ElEasy.Ryze.Combo.Ignite").GetValue<bool>();

            if (Player.Buffs.Count(buf => buf.Name == "RyzePassiveStack") <= 2)
            {
                if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
                {
                    var pred = spells[Spells.Q].GetPrediction(target);
                    if (pred.Hitchance >= HitChance.High && pred.CollisionObjects.Count == 0)
                    {
                        spells[Spells.Q].Cast(target);
                    }
                }

                if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
                {
                    spells[Spells.E].CastOnUnit(target);
                }
                if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(target))
                {
                    spells[Spells.W].CastOnUnit(target);
                }

                if (useR && spells[Spells.R].IsReady() && Player.HealthPercent <= rHp)
                {
                    spells[Spells.R].Cast(Player);
                }
            }
            else if (Player.Buffs.Count(buf => buf.Name == "RyzePassiveStack") == 3)
            {
                if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(target))
                {
                    spells[Spells.W].CastOnUnit(target);
                }

                if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
                {
                    var pred = spells[Spells.Q].GetPrediction(target);
                    if (pred.Hitchance >= HitChance.High && pred.CollisionObjects.Count == 0)
                    {
                        spells[Spells.Q].Cast(target);
                    }
                }

                if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
                {
                    spells[Spells.E].CastOnUnit(target);
                }

                if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
                {
                    var pred = spells[Spells.Q].GetPrediction(target);
                    if (pred.Hitchance >= HitChance.High && pred.CollisionObjects.Count == 0)
                    {
                        spells[Spells.Q].Cast(target);
                    }
                }

                if (useR && spells[Spells.R].IsReady() && Player.HealthPercent <= rHp)
                {
                    spells[Spells.R].Cast(Player);
                }
            }
            else if (Player.Buffs.Count(buf => buf.Name == "RyzePassiveStack") == 4)
            {
                if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(target))
                {
                    spells[Spells.W].CastOnUnit(target);
                }

                if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
                {
                    var pred = spells[Spells.Q].GetPrediction(target);
                    if (pred.Hitchance >= HitChance.High && pred.CollisionObjects.Count == 0)
                    {
                        spells[Spells.Q].Cast(target);
                    }
                }

                if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
                {
                    spells[Spells.E].CastOnUnit(target);
                }
            }

            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health && useI)
            {
                Player.Spellbook.CastSpell(Ignite, target);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            var drawOff = Menu.Item("ElEasy.Ryze.Draw.off").GetValue<bool>();
            var drawQ = Menu.Item("ElEasy.Ryze.Draw.Q").GetValue<Circle>();
            var drawW = Menu.Item("ElEasy.Ryze.Draw.W").GetValue<Circle>();
            var drawE = Menu.Item("ElEasy.Ryze.Draw.E").GetValue<Circle>();

            if (drawOff)
            {
                return;
            }

            if (drawQ.Active)
            {
                if (spells[Spells.Q].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.Q].Range, Color.White);
                }
            }

            if (drawE.Active)
            {
                if (spells[Spells.E].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.E].Range, Color.White);
                }
            }

            if (drawW.Active)
            {
                if (spells[Spells.W].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.W].Range, Color.White);
                }
            }
        }

        private static void OnHarass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            var useQ = Menu.Item("ElEasy.Ryze.Harass.Q").GetValue<bool>();
            var useW = Menu.Item("ElEasy.Ryze.Harass.W").GetValue<bool>();
            var useE = Menu.Item("ElEasy.Ryze.Harass.E").GetValue<bool>();
            var mana = Menu.Item("ElEasy.Ryze.Harass.Player.Mana").GetValue<Slider>().Value;

            if (Player.Mana < mana)
            {
                return;
            }

            if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                var pred = spells[Spells.Q].GetPrediction(target);
                if (pred.Hitchance >= HitChance.High && pred.CollisionObjects.Count == 0)
                {
                    spells[Spells.Q].Cast(target);
                }
            }

            if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(target))
            {
                spells[Spells.W].CastOnUnit(target);
            }

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
            {
                spells[Spells.E].CastOnUnit(target);
            }
        }

        private static void OnJungleclear()
        {
            var useQ = Menu.Item("ElEasy.Ryze.JungleClear.Q").GetValue<bool>();
            var useW = Menu.Item("ElEasy.Ryze.JungleClear.W").GetValue<bool>();
            var useE = Menu.Item("ElEasy.Ryze.JungleClear.E").GetValue<bool>();
            var mana = Menu.Item("ElEasy.Ryze.Clear.Player.Mana").GetValue<Slider>().Value;

            if (Player.Mana < mana)
            {
                return;
            }

            var minions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition,
                spells[Spells.Q].Range,
                MinionTypes.All,
                MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);
            if (minions.Count <= 0)
            {
                return;
            }

            if (useQ && spells[Spells.Q].IsReady())
            {
                spells[Spells.Q].Cast(minions[0]);
            }

            if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(minions[0]))
            {
                spells[Spells.W].Cast(minions[0]);
            }

            if (useE && spells[Spells.E].IsReady())
            {
                spells[Spells.E].Cast(minions[0]);
            }
        }

        private static void OnLaneclear()
        {
            var useQ = Menu.Item("ElEasy.Ryze.LaneClear.Q").GetValue<bool>();
            var useW = Menu.Item("ElEasy.Ryze.LaneClear.W").GetValue<bool>();
            var useE = Menu.Item("ElEasy.Ryze.LaneClear.E").GetValue<bool>();
            var mana = Menu.Item("ElEasy.Ryze.Clear.Player.Mana").GetValue<Slider>().Value;

            if (Player.Mana < mana)
            {
                return;
            }

            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.W].Range);
            if (minions.Count <= 0)
            {
                return;
            }

            if (useW && spells[Spells.W].IsReady())
            {
                spells[Spells.W].CastOnUnit(minions[0]);
            }

            if (useQ && spells[Spells.Q].IsReady())
            {
                var qtarget =
                    minions.Where(
                        x =>
                        x.Distance(Player) < spells[Spells.Q].Range
                        && spells[Spells.Q].GetPrediction(x).Hitchance >= HitChance.High
                        && (x.Health < Player.GetSpellDamage(x, SpellSlot.Q)
                            && !(x.Health < Player.GetAutoAttackDamage(x))))
                        .OrderByDescending(x => x.Health)
                        .FirstOrDefault();
                if (HealthPrediction.GetHealthPrediction(qtarget, (int)0.25)
                    <= Player.GetSpellDamage(qtarget, SpellSlot.Q))
                {
                    spells[Spells.Q].Cast(qtarget);
                }
            }

            if (useE && spells[Spells.E].IsReady())
            {
                spells[Spells.E].Cast(minions[0]);
            }
        }

        private static void OnLasthit()
        {
            var useQ = Menu.Item("ElEasy.Ryze.Lasthit.Q").GetValue<bool>();
            var useW = Menu.Item("ElEasy.Ryze.Lasthit.W").GetValue<bool>();
            var useE = Menu.Item("ElEasy.Ryze.Lasthit.E").GetValue<bool>();

            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.W].Range);
            if (minions.Count <= 0)
            {
                return;
            }

            if (spells[Spells.Q].IsReady() && useQ)
            {
                var qtarget =
                    minions.Where(
                        x =>
                        x.Distance(Player) < spells[Spells.Q].Range
                        && spells[Spells.Q].GetPrediction(x).Hitchance >= HitChance.High
                        && (x.Health < Player.GetSpellDamage(x, SpellSlot.Q)
                            && !(x.Health < Player.GetAutoAttackDamage(x))))
                        .OrderByDescending(x => x.Health)
                        .FirstOrDefault();
                if (HealthPrediction.GetHealthPrediction(qtarget, (int)0.25)
                    <= Player.GetSpellDamage(qtarget, SpellSlot.Q))
                {
                    spells[Spells.Q].Cast(qtarget);
                }
            }

            if (spells[Spells.W].IsReady() && useW)
            {
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.W].Range);
                {
                    foreach (var minion in
                        allMinions.Where(
                            minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.W)))
                    {
                        if (minion.IsValidTarget())
                        {
                            spells[Spells.W].CastOnUnit(minion);
                            return;
                        }
                    }
                }
            }

            if (spells[Spells.E].IsReady() && useE)
            {
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.E].Range);
                {
                    foreach (var minion in
                        allMinions.Where(
                            minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.E)))
                    {
                        if (minion.IsValidTarget())
                        {
                            spells[Spells.E].CastOnUnit(minion);
                            return;
                        }
                    }
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    OnCombo();
                    break;

                case Orbwalking.OrbwalkingMode.LastHit:
                    OnLasthit();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    OnLaneclear();
                    OnJungleclear();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    OnHarass();
                    break;
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (Player.Buffs.Count(buf => buf.Name == "Muramana") == 0)
                {
                    var muramana = ItemData.Muramana.GetItem();
                    if (muramana.IsOwned(Player))
                    {
                        muramana.Cast();
                    }
                }
            }
            else
            {
                if (Player.Buffs.Count(buf => buf.Name == "Muramana") != 0)
                {
                    var muramana = ItemData.Muramana.GetItem();
                    if (muramana.IsOwned(Player))
                    {
                        muramana.Cast();
                    }
                }
            }

            var autoHarass = Menu.Item("ElEasy.Ryze.AutoHarass.Activated", true).GetValue<KeyBind>().Active;
            if (autoHarass)
            {
                OnAutoHarass();
            }
        }

        private static void OrbwalkingBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            var autoattack = Menu.Item("ElEasy.Ryze.AA").GetValue<bool>();
            if (autoattack && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                args.Process = false;
            }
            else
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                {
                    args.Process =
                        !(spells[Spells.Q].IsReady() || spells[Spells.W].IsReady() || spells[Spells.E].IsReady()
                          || Player.Distance(args.Target) >= 1000);
                }
            }
        }

        #endregion
    }
}