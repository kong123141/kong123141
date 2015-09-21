namespace ElEasy.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    public class Sona : Standards
    {
        #region Static Fields

        private static readonly Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>
                                                                       {
                                                                           { Spells.Q, new Spell(SpellSlot.Q, 850) },
                                                                           { Spells.W, new Spell(SpellSlot.W, 1000) },
                                                                           { Spells.E, new Spell(SpellSlot.E, 350) },
                                                                           { Spells.R, new Spell(SpellSlot.R, 1000) }
                                                                       };

        #endregion

        #region Public Methods and Operators

        public static void Load()
        {
            Ignite = Player.GetSpellSlot("summonerdot");

            spells[Spells.R].SetSkillshot(0.5f, 125, 3000f, false, SkillshotType.SkillshotLine);

            Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        #endregion

        #region Methods

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var gapCloserActive = Menu.Item("ElEasy.Sona.GapCloser.Activated").GetValue<bool>();

            if (gapCloserActive && spells[Spells.R].IsReady()
                && gapcloser.Sender.Distance(Player) < spells[Spells.R].Range)
            {
                spells[Spells.R].Cast(gapcloser.Sender);
            }
        }

        private static void AutoHarass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid)
            {
                return;
            }
            var active = Menu.Item("ElEasy.Sona.Autoharass.Activated").GetValue<KeyBind>().Active;

            if (active && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                spells[Spells.Q].Cast(target);
            }
        }

        private static void HealManager()
        {
            var useHeal = Menu.Item("ElEasy.Sona.Heal.Activated").GetValue<bool>();
            var playerMana = Menu.Item("ElEasy.Sona.Heal.Player.Mana").GetValue<Slider>().Value;
            var playerHp = Menu.Item("ElEasy.Sona.Heal.Player.HP").GetValue<Slider>().Value;
            var allyHp = Menu.Item("ElEasy.Sona.Heal.Ally.HP").GetValue<Slider>().Value;

            if (Player.IsRecalling() || Player.InFountain() || !useHeal || Player.ManaPercent < playerMana
                || !spells[Spells.W].IsReady())
            {
                return;
            }

            //self heal
            if ((Player.Health / Player.MaxHealth) * 100 <= playerHp)
            {
                spells[Spells.W].CastOnUnit(Player);
            }

            //ally
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && !h.IsMe))
            {
                if ((hero.Health / hero.MaxHealth) * 100 <= allyHp && spells[Spells.W].IsInRange(hero))
                {
                    spells[Spells.W].CastOnUnit(Player);
                }
            }
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
            Menu = new Menu("ElSona", "menu", true);

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            Menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElEasy.Sona.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Sona.Combo.W", "Use W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Sona.Combo.E", "Use E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Sona.Combo.R", "Use R").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Sona.Combo.Count.R", "Minimum hit by R").SetValue(new Slider(2, 1, 5)));
            cMenu.AddItem(new MenuItem("ElEasy.Sona.Combo.Ignite", "Use Ignite").SetValue(true));

            Menu.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElEasy.Sona.Harass.Q", "Use Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Sona.Harass.Player.Mana", "Minimum Mana").SetValue(new Slider(55)));
            hMenu.SubMenu("Auto harass")
                .AddItem(
                    new MenuItem("ElEasy.Sona.Autoharass.Activated", "Autoharass").SetValue(
                        new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle)));

            Menu.AddSubMenu(hMenu);

            var healMenu = new Menu("Heal", "Heal");
            healMenu.AddItem(new MenuItem("ElEasy.Sona.Heal.Activated", "Heal").SetValue(true));
            healMenu.AddItem(new MenuItem("ElEasy.Sona.Heal.Player.HP", "Player HP").SetValue(new Slider(55)));
            healMenu.AddItem(new MenuItem("ElEasy.Sona.Heal.Ally.HP", "Ally HP").SetValue(new Slider(55)));
            healMenu.AddItem(new MenuItem("ElEasy.Sona.Heal.Player.Mana", "Minimum Mana").SetValue(new Slider(55)));

            Menu.AddSubMenu(healMenu);

            var interruptMenu = new Menu("Settings", "Settings");
            interruptMenu.AddItem(new MenuItem("ElEasy.Sona.Interrupt.Activated", "Interrupt spells").SetValue(true));
            interruptMenu.AddItem(new MenuItem("ElEasy.SonaGapCloser.Activated", "Anti gapcloser").SetValue(true));

            Menu.AddSubMenu(interruptMenu);

            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("ElEasy.Sona.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElEasy.Sona.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Sona.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Sona.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Sona.Draw.R", "Draw R").SetValue(new Circle()));

            Menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = Menu.AddSubMenu(new Menu("Credits", "jQuery"));
            credits.AddItem(new MenuItem("ElEasy.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElEasy.Email", "info@zavox.nl"));

            Menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            Menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            Menu.AddToMainMenu();
        }

        private static void Interrupter2_OnInterruptableTarget(
            Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel != Interrupter2.DangerLevel.High || sender.Distance(Player) > spells[Spells.R].Range)
            {
                return;
            }

            if (sender.IsValidTarget(spells[Spells.R].Range) && args.DangerLevel == Interrupter2.DangerLevel.High
                && spells[Spells.R].IsReady())
            {
                spells[Spells.R].Cast(sender.Position);
            }
        }

        private static void OnCombo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            var rTarget = TargetSelector.GetTarget(spells[Spells.R].Range, TargetSelector.DamageType.Magical);

            if (target == null || !target.IsValid)
            {
                return;
            }

            var useQ = Menu.Item("ElEasy.Sona.Combo.Q").GetValue<bool>();
            var useW = Menu.Item("ElEasy.Sona.Combo.W").GetValue<bool>();
            var useE = Menu.Item("ElEasy.Sona.Combo.E").GetValue<bool>();
            var useR = Menu.Item("ElEasy.Sona.Combo.R").GetValue<bool>();
            var useI = Menu.Item("ElEasy.Sona.Combo.Ignite").GetValue<bool>();
            var hitByR = Menu.Item("ElEasy.Sona.Combo.Count.R").GetValue<Slider>().Value;

            if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                spells[Spells.Q].Cast(target);
            }

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
            {
                spells[Spells.E].CastOnUnit(Player);
            }

            if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(target))
            {
                spells[Spells.W].CastOnUnit(Player);
            }

            if (useR && spells[Spells.R].IsReady() && spells[Spells.R].IsInRange(rTarget))
            {
                spells[Spells.R].CastIfWillHit(rTarget, hitByR);

                /*
                var pred = spells[Spells.R].GetPrediction(rTarget).Hitchance;
                if(pred >= HitChance.High)
                    //spells[Spells.R].CastIfWillHit(rTarget, hitByR);
              */
            }

            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health && useI)
            {
                Player.Spellbook.CastSpell(Ignite, target);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            var drawOff = Menu.Item("ElEasy.Sona.Draw.off").GetValue<bool>();
            var drawQ = Menu.Item("ElEasy.Sona.Draw.Q").GetValue<Circle>();
            var drawW = Menu.Item("ElEasy.Sona.Draw.W").GetValue<Circle>();
            var drawE = Menu.Item("ElEasy.Sona.Draw.E").GetValue<Circle>();
            var drawR = Menu.Item("ElEasy.Sona.Draw.R").GetValue<Circle>();

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

            if (drawR.Active)
            {
                if (spells[Spells.W].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.R].Range, Color.White);
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

            var useQ = Menu.Item("ElEasy.Sona.Harass.Q").GetValue<bool>();
            var playerMana = Menu.Item("ElEasy.Sona.Harass.Player.Mana").GetValue<Slider>().Value;

            if (Player.Mana < playerMana)
            {
                return;
            }

            if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                spells[Spells.Q].Cast(target);
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

                case Orbwalking.OrbwalkingMode.Mixed:
                    OnHarass();
                    break;
            }

            HealManager();
            AutoHarass();
        }

        #endregion
    }
}