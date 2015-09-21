namespace ElEasy.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Color = System.Drawing.Color;

    public class Nasus : Standards
    {
        #region Static Fields

        public static int Sheen = 3057, Iceborn = 3025;

        private static readonly Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>
                                                                       {
                                                                           {
                                                                               Spells.Q,
                                                                               new Spell(
                                                                               SpellSlot.Q,
                                                                               Player.AttackRange + 50)
                                                                           },
                                                                           { Spells.W, new Spell(SpellSlot.W, 600) },
                                                                           { Spells.E, new Spell(SpellSlot.E, 650) },
                                                                           { Spells.R, new Spell(SpellSlot.R) }
                                                                       };

        #endregion

        #region Public Methods and Operators

        public static void Load()
        {
            Ignite = Player.GetSpellSlot("summonerdot");
            spells[Spells.E].SetSkillshot(
                spells[Spells.E].Instance.SData.SpellCastTime,
                spells[Spells.E].Instance.SData.LineWidth,
                spells[Spells.E].Instance.SData.MissileSpeed,
                false,
                SkillshotType.SkillshotCircle);

            Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        #endregion

        #region Methods

        private static void AutoLastHit()
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo
                || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed || Player.IsRecalling())
            {
                return;
            }

            var minions = MinionManager.GetMinions(
                Player.Position,
                spells[Spells.E].Range,
                MinionTypes.All,
                MinionTeam.Enemy,
                MinionOrderTypes.MaxHealth);

            foreach (var minion in minions)
            {
                if (GetBonusDmg(minion) > minion.Health
                    && Vector3.Distance(ObjectManager.Player.ServerPosition, minion.Position) < Player.AttackRange + 50
                    && spells[Spells.Q].IsReady())
                {
                    Orbwalker.SetAttack(false);
                    //spells[Spells.Q].Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                    Orbwalker.SetAttack(true);
                    break;
                }
            }
        }

        private static double GetBonusDmg(Obj_AI_Base target)
        {
            double dmgItem = 0;
            if (Items.HasItem(Sheen) && (Items.CanUseItem(Sheen) || Player.HasBuff("sheen"))
                && Player.BaseAttackDamage > dmgItem)
            {
                dmgItem = Player.GetAutoAttackDamage(target);
            }

            if (Items.HasItem(Iceborn) && (Items.CanUseItem(Iceborn) || Player.HasBuff("itemfrozenfist"))
                && Player.BaseAttackDamage * 1.25 > dmgItem)
            {
                dmgItem = Player.GetAutoAttackDamage(target) * 1.25;
            }

            return spells[Spells.Q].GetDamage(target) + Player.GetAutoAttackDamage(target) + dmgItem;
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
            Menu = new Menu("ElNasus", "menu", true);

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            Menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElEasy.Nasus.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Nasus.Combo.W", "Use W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Nasus.Combo.E", "Use E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Nasus.Combo.R", "Use R").SetValue(true));
            cMenu.AddItem(
                new MenuItem("ElEasy.Nasus.Combo.Count.R", "Minimum champions in range for R").SetValue(
                    new Slider(2, 1, 5)));
            cMenu.AddItem(new MenuItem("ElEasy.Nasus.Combo.HP", "Minimum HP for R").SetValue(new Slider(55)));
            cMenu.AddItem(new MenuItem("ElEasy.Nasus.Combo.Ignite", "Use Ignite").SetValue(true));

            Menu.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElEasy.Nasus.Harass.E", "Use E").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Nasus.Harass.Player.Mana", "Minimum Mana").SetValue(new Slider(55)));

            Menu.AddSubMenu(hMenu);

            var clearMenu = new Menu("Clear", "Clear");
            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Nasus.LaneClear.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Nasus.LaneClear.E", "Use E").SetValue(true));
            clearMenu.SubMenu("Jungleclear").AddItem(new MenuItem("ElEasy.Nasus.JungleClear.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Jungleclear").AddItem(new MenuItem("ElEasy.Nasus.JungleClear.E", "Use E").SetValue(true));

            Menu.AddSubMenu(clearMenu);

            var settingsMenu = new Menu("Lasthit", "Lasthit");
            settingsMenu.AddItem(
                new MenuItem("ElEasy.Nasus.Lasthit.Activated", "Auto Lasthit").SetValue(
                    new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle)));
            Menu.AddSubMenu(settingsMenu);

            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("ElEasy.Nasus.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElEasy.Nasus.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Nasus.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Nasus.Draw.Text", "Draw text").SetValue(true));
            miscMenu.AddItem(new MenuItem("ElEasy.Nasus.Draw.MinionHelper", "Draw killable minions").SetValue(true));

            Menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = Menu.AddSubMenu(new Menu("Credits", "jQuery"));
            credits.AddItem(new MenuItem("ElEasy.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElEasy.Email", "info@zavox.nl"));

            Menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            Menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            Menu.AddToMainMenu();
        }

        private static void Jungleclear()
        {
            var useQ = Menu.Item("ElEasy.Nasus.JungleClear.Q").GetValue<bool>();
            var useE = Menu.Item("ElEasy.Nasus.JungleClear.E").GetValue<bool>();

            var minions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition,
                spells[Spells.E].Range,
                MinionTypes.All,
                MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);

            if (minions.Count <= 0)
            {
                return;
            }

            if (useQ && spells[Spells.Q].IsReady())
            {
                if (minions.Find(x => x.Health >= spells[Spells.Q].GetDamage(x) && x.IsValidTarget()) != null)
                {
                    spells[Spells.Q].Cast();
                }
            }

            if (useE && spells[Spells.E].IsReady())
            {
                if (minions.Count > 1)
                {
                    var farmLocation = spells[Spells.E].GetCircularFarmLocation(minions);
                    spells[Spells.E].Cast(farmLocation.Position);
                }
            }
        }

        private static void Laneclear()
        {
            var useQ = Menu.Item("ElEasy.Nasus.LaneClear.Q").GetValue<bool>();
            var useE = Menu.Item("ElEasy.Nasus.LaneClear.E").GetValue<bool>();

            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.E].Range);
            if (minions.Count <= 0)
            {
                return;
            }

            /*  if (useQ && spells[Spells.Q].IsReady())
            {
                if (minions.Find(x => x.Health >= spells[Spells.Q].GetDamage(x) && x.IsValidTarget()) != null)
                {
                    spells[Spells.Q].Cast();
                }
            }*/

            if (spells[Spells.Q].IsReady() && useQ)
            {
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.E].Range);
                {
                    foreach (var minion in
                        allMinions.Where(
                            minion => minion.Health <= ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q)))
                    {
                        if (minion.IsValidTarget())
                        {
                            spells[Spells.Q].CastOnUnit(minion);
                            return;
                        }
                    }
                }
            }

            if (useE && spells[Spells.E].IsReady())
            {
                if (minions.Count > 1)
                {
                    var farmLocation = spells[Spells.E].GetCircularFarmLocation(minions);
                    spells[Spells.E].Cast(farmLocation.Position);
                }
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!args.SData.Name.ToLower().Contains("attack") || !sender.IsMe)
            {
                return;
            }

            var unit = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(args.Target.NetworkId);
            if ((GetBonusDmg(unit) > unit.Health))
            {
                spells[Spells.Q].Cast();
            }
        }

        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.W].Range, TargetSelector.DamageType.Physical);
            var eTarget = TargetSelector.GetTarget(
                spells[Spells.E].Range + spells[Spells.E].Width,
                TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            var useQ = Menu.Item("ElEasy.Nasus.Combo.Q").GetValue<bool>();
            var useW = Menu.Item("ElEasy.Nasus.Combo.W").GetValue<bool>();
            var useE = Menu.Item("ElEasy.Nasus.Combo.E").GetValue<bool>();
            var useR = Menu.Item("ElEasy.Nasus.Combo.R").GetValue<bool>();
            var useI = Menu.Item("ElEasy.Nasus.Combo.Ignite").GetValue<bool>();
            var countEnemies = Menu.Item("ElEasy.Nasus.Combo.Count.R").GetValue<Slider>().Value;
            var playerHp = Menu.Item("ElEasy.Nasus.Combo.HP").GetValue<Slider>().Value;

            if (useQ && spells[Spells.Q].IsReady())
            {
                spells[Spells.Q].Cast();
            }

            if (useW && spells[Spells.W].IsReady())
            {
                spells[Spells.W].Cast(target);
            }

            if (useE && spells[Spells.E].IsReady() && eTarget.IsValidTarget() && spells[Spells.E].IsInRange(eTarget))
            {
                var pred = spells[Spells.E].GetPrediction(eTarget).Hitchance;
                if (pred >= HitChance.High)
                {
                    spells[Spells.E].Cast(eTarget);
                }
            }

            if (useR && spells[Spells.R].IsReady() && Player.CountEnemiesInRange(spells[Spells.W].Range) >= countEnemies
                || (Player.Health / Player.MaxHealth) * 100 <= playerHp)
            {
                spells[Spells.R].CastOnUnit(Player);
            }

            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health && useI)
            {
                Player.Spellbook.CastSpell(Ignite, target);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            var drawOff = Menu.Item("ElEasy.Nasus.Draw.off").GetValue<bool>();
            var drawW = Menu.Item("ElEasy.Nasus.Draw.W").GetValue<Circle>();
            var drawE = Menu.Item("ElEasy.Nasus.Draw.E").GetValue<Circle>();
            var drawText = Menu.Item("ElEasy.Nasus.Draw.Text").GetValue<bool>();
            var rBool = Menu.Item("ElEasy.Nasus.Lasthit.Activated").GetValue<KeyBind>().Active;
            var helper = Menu.Item("ElEasy.Nasus.Draw.MinionHelper").GetValue<bool>();

            var playerPos = Drawing.WorldToScreen(ObjectManager.Player.Position);

            if (drawOff)
            {
                return;
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

            if (drawText)
            {
                Drawing.DrawText(
                    playerPos.X - 70,
                    playerPos.Y + 40,
                    (rBool ? Color.Green : Color.Red),
                    "{0}",
                    (rBool ? "Auto lasthit enabled" : "Auto lasthit disabled"));
            }

            if (helper)
            {
                var minions = MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition,
                    spells[Spells.E].Range,
                    MinionTypes.All,
                    MinionTeam.NotAlly);
                foreach (var minion in minions)
                {
                    if (minion != null)
                    {
                        if ((GetBonusDmg(minion) > minion.Health))
                        {
                            Render.Circle.DrawCircle(minion.ServerPosition, minion.BoundingRadius, Color.Black);
                        }
                    }
                }
            }
        }

        private static void OnHarass()
        {
            var eTarget = TargetSelector.GetTarget(
                spells[Spells.E].Range + spells[Spells.E].Width,
                TargetSelector.DamageType.Magical);
            if (eTarget == null || !eTarget.IsValid)
            {
                return;
            }

            var useE = Menu.Item("ElEasy.Nasus.Harass.E").GetValue<bool>();

            if (useE && spells[Spells.E].IsReady() && eTarget.IsValidTarget() && spells[Spells.E].IsInRange(eTarget))
            {
                var pred = spells[Spells.E].GetPrediction(eTarget).Hitchance;
                if (pred >= HitChance.High)
                {
                    spells[Spells.E].Cast(eTarget);
                }
            }
        }

        private static void OnLastHit()
        {
            var minions = MinionManager.GetMinions(
                Player.Position,
                spells[Spells.E].Range,
                MinionTypes.All,
                MinionTeam.Enemy,
                MinionOrderTypes.MaxHealth);

            foreach (var minion in minions)
            {
                if (GetBonusDmg(minion) > minion.Health && spells[Spells.Q].IsReady())
                {
                    /*Orbwalker.SetAttack(false);
                    spells[Spells.Q].Cast();*/
                    Orbwalker.SetAttack(false);
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                    Orbwalker.SetAttack(true);
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

                case Orbwalking.OrbwalkingMode.LaneClear:
                    Laneclear();
                    Jungleclear();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    OnHarass();
                    break;

                case Orbwalking.OrbwalkingMode.LastHit:
                    OnLastHit();
                    break;
            }

            var active = Menu.Item("ElEasy.Nasus.Lasthit.Activated").GetValue<KeyBind>().Active;
            if (active)
            {
                AutoLastHit();
            }
        }

        #endregion
    }
}