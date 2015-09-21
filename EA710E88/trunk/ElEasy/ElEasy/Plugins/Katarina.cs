namespace ElEasy.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Color = System.Drawing.Color;
    using ItemData = LeagueSharp.Common.Data.ItemData;

    public class Katarina : Standards
    {
        #region Static Fields

        private static readonly Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>
                                                                       {
                                                                           { Spells.Q, new Spell(SpellSlot.Q, 675) },
                                                                           { Spells.W, new Spell(SpellSlot.W, 375) },
                                                                           { Spells.E, new Spell(SpellSlot.E, 700) },
                                                                           { Spells.R, new Spell(SpellSlot.R, 550) }
                                                                       };

        private static bool isChanneling;

        private static long lastECast;

        private static int lastPlaced;

        private static Vector3 lastWardPos;

        private static float rStart;

        #endregion

        #region Public Methods and Operators

        public static void Load()
        {
            Ignite = Player.GetSpellSlot("summonerdot");
            spells[Spells.R].SetCharged("KatarinaR", "KatarinaR", 550, 550, 1.0f);

            Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnIssueOrder += Obj_AI_Hero_OnIssueOrder;
            Orbwalking.BeforeAttack += BeforeAttack;
            GameObject.OnCreate += GameObject_OnCreate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            // ReSharper disable once ObjectCreationAsStatement
            new AssassinManager();
        }

        #endregion

        #region Methods

        private static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe)
            {
                args.Process = !Player.HasBuff("KatarinaR");
            }
        }

        private static void CastE(Obj_AI_Base unit)
        {
            var playLegit = Menu.Item("ElEasy.Katarina.E.Legit").GetValue<bool>();
            var legitCastDelay = Menu.Item("ElEasy.Katarina.E.Delay").GetValue<Slider>().Value;

            if (playLegit)
            {
                if (Environment.TickCount > lastECast + legitCastDelay)
                {
                    spells[Spells.E].CastOnUnit(unit);
                    lastECast = Environment.TickCount;
                }
            }
            else
            {
                spells[Spells.E].CastOnUnit(unit);
                lastECast = Environment.TickCount;
            }
        }

        private static void DoWardJump()
        {
            if (Environment.TickCount <= lastPlaced + 3000 || !spells[Spells.E].IsReady())
            {
                return;
            }

            var cursorPos = Game.CursorPos;
            var myPos = Player.ServerPosition;
            var delta = cursorPos - myPos;

            delta.Normalize();

            var wardPosition = myPos + delta * (600 - 5);
            var invSlot = GetBestWardSlot();

            if (invSlot == null)
            {
                return;
            }

            Items.UseItem((int)invSlot.Id, wardPosition);
            lastWardPos = wardPosition;
            lastPlaced = Environment.TickCount;

            spells[Spells.E].Cast();
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!spells[Spells.E].IsReady() || !(sender is Obj_AI_Minion) || Environment.TickCount >= lastPlaced + 300)
            {
                return;
            }

            if (Environment.TickCount >= lastPlaced + 300)
            {
                return;
            }
            var ward = (Obj_AI_Minion)sender;

            if (ward.Name.ToLower().Contains("ward") && ward.Distance(lastWardPos) < 500)
            {
                spells[Spells.E].Cast(ward);
            }
        }

        private static InventorySlot GetBestWardSlot()
        {
            var slot = Items.GetWardSlot();
            if (slot == default(InventorySlot))
            {
                return null;
            }
            return slot;
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
                damage += Player.GetSpellDamage(enemy, SpellSlot.R) * 8;
            }

            if (KatarinaQ(enemy))
            {
                damage += Player.CalcDamage(
                    enemy,
                    Damage.DamageType.Magical,
                    Player.FlatMagicDamageMod * 0.15 + Player.Level * 15);
            }

            return (float)damage;
        }

        private static Obj_AI_Hero GetEnemy(
            float vDefaultRange = 0,
            TargetSelector.DamageType vDefaultDamageType = TargetSelector.DamageType.Magical)
        {
            if (Math.Abs(vDefaultRange) < 0.00001)
            {
                vDefaultRange = spells[Spells.R].Range;
            }

            if (!Menu.Item("AssassinActive").GetValue<bool>())
            {
                return TargetSelector.GetTarget(vDefaultRange, vDefaultDamageType);
            }

            var assassinRange = Menu.Item("AssassinSearchRange").GetValue<Slider>().Value;

            var vEnemy =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        enemy =>
                        enemy.Team != ObjectManager.Player.Team && !enemy.IsDead && enemy.IsVisible
                        && Menu.Item("Assassin" + enemy.ChampionName) != null
                        && Menu.Item("Assassin" + enemy.ChampionName).GetValue<bool>()
                        && ObjectManager.Player.Distance(enemy) < assassinRange);

            if (Menu.Item("AssassinSelectOption").GetValue<StringList>().SelectedIndex == 1)
            {
                vEnemy = (from vEn in vEnemy select vEn).OrderByDescending(vEn => vEn.MaxHealth);
            }

            var objAiHeroes = vEnemy as Obj_AI_Hero[] ?? vEnemy.ToArray();

            var t = !objAiHeroes.Any() ? TargetSelector.GetTarget(vDefaultRange, vDefaultDamageType) : objAiHeroes[0];

            return t;
        }

        private static float GetHealth(Obj_AI_Base target)
        {
            return target.Health;
        }

        private static bool HasRBuff()
        {
            return Player.HasBuff("KatarinaR") || Player.IsChannelingImportantSpell()
                   || Player.HasBuff("katarinarsound");
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
            Menu = new Menu("ElKatarina", "menu", true);

            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);

            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            Menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("Combo", "Combo");
            cMenu.AddItem(new MenuItem("ElEasy.Katarina.Combo.Q", "Use Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Katarina.Combo.W", "Use W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElEasy.Katarina.Combo.E", "Use E").SetValue(true));

            cMenu.SubMenu("E").AddItem(new MenuItem("ElEasy.Katarina.E.Legit", "Legit E").SetValue(false));
            cMenu.SubMenu("E")
                .AddItem(new MenuItem("ElEasy.Katarina.E.Delay", "E Delay").SetValue(new Slider(1000, 0, 2000)));

            cMenu.SubMenu("R").AddItem(new MenuItem("ElEasy.Katarina.Combo.R", "Use R").SetValue(true));
            cMenu.SubMenu("R")
                .AddItem(
                    new MenuItem("ElEasy.Katarina.Combo.Sort", "R:").SetValue(
                        new StringList(new[] { "Normal", "Smart" })));
            cMenu.SubMenu("R").AddItem(new MenuItem("ElEasy.Katarina.Combo.R.Force", "Force R").SetValue(false));
            cMenu.SubMenu("R")
                .AddItem(
                    new MenuItem("ElEasy.Katarina.Combo.R.Force.Count", "Force R when in range:").SetValue(
                        new Slider(3, 0, 5)));
            cMenu.AddItem(new MenuItem("ElEasy.Katarina.Combo.Ignite", "Use Ignite").SetValue(true));

            Menu.AddSubMenu(cMenu);

            var iMenu = new Menu("Items", "Items");
            iMenu.AddItem(new MenuItem("ElEasy.Katarina.Items.hextech", "Use Hextech Gunblade").SetValue(true));
            Menu.AddSubMenu(iMenu);

            var wMenu = new Menu("Wardjump", "Wardjump");
            wMenu.AddItem(
                new MenuItem("ElEasy.Katarina.Wardjump", "Wardjump key").SetValue(
                    new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
            Menu.AddSubMenu(wMenu);

            var hMenu = new Menu("Harass", "Harass");
            hMenu.AddItem(new MenuItem("ElEasy.Katarina.Harass.Q", "Use Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Katarina.Harass.W", "Use W").SetValue(true));
            hMenu.AddItem(new MenuItem("ElEasy.Katarina.Harass.E", "Use E").SetValue(true));

            hMenu.SubMenu("Harass")
                .SubMenu("AutoHarass settings")
                .AddItem(
                    new MenuItem("ElEasy.Katarina.AutoHarass.Activated", "Auto harass", true).SetValue(
                        new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle)));
            hMenu.SubMenu("Harass")
                .SubMenu("AutoHarass settings")
                .AddItem(new MenuItem("ElEasy.Katarina.AutoHarass.Q", "Use Q").SetValue(true));
            hMenu.SubMenu("Harass")
                .SubMenu("AutoHarass settings")
                .AddItem(new MenuItem("ElEasy.Katarina.AutoHarass.W", "Use W").SetValue(true));

            hMenu.SubMenu("Harass")
                .AddItem(
                    new MenuItem("ElEasy.Katarina.Harass.Mode", "Harass mode:").SetValue(
                        new StringList(new[] { "Q", "Q - W", "Q - E - W" })));

            Menu.AddSubMenu(hMenu);

            var ksMenu = new Menu("Killsteal", "Killsteal");
            ksMenu.AddItem(new MenuItem("ElEasy.Katarina.Killsteal", "Killsteal").SetValue(true));
            ksMenu.AddItem(new MenuItem("ElEasy.Katarina.Killsteal.R", "Killsteal with R").SetValue(true));

            Menu.AddSubMenu(ksMenu);

            var clearMenu = new Menu("Clear", "Clear");
            clearMenu.SubMenu("Lasthit").AddItem(new MenuItem("ElEasy.Katarina.Lasthit.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Lasthit").AddItem(new MenuItem("ElEasy.Katarina.Lasthit.W", "Use W").SetValue(true));
            clearMenu.SubMenu("Lasthit").AddItem(new MenuItem("ElEasy.Katarina.Lasthit.E", "Use E").SetValue(false));
            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Katarina.LaneClear.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Katarina.LaneClear.W", "Use W").SetValue(true));
            clearMenu.SubMenu("Laneclear").AddItem(new MenuItem("ElEasy.Katarina.LaneClear.E", "Use E").SetValue(false));
            clearMenu.SubMenu("Jungleclear")
                .AddItem(new MenuItem("ElEasy.Katarina.JungleClear.Q", "Use Q").SetValue(true));
            clearMenu.SubMenu("Jungleclear")
                .AddItem(new MenuItem("ElEasy.Katarina.JungleClear.W", "Use W").SetValue(true));
            clearMenu.SubMenu("Jungleclear")
                .AddItem(new MenuItem("ElEasy.Katarina.JungleClear.E", "Use E").SetValue(false));

            Menu.AddSubMenu(clearMenu);

            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("ElEasy.Katarina.Draw.off", "Turn drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElEasy.Katarina.Draw.Q", "Draw Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Katarina.Draw.W", "Draw W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Katarina.Draw.E", "Draw E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElEasy.Katarina.Draw.R", "Draw R").SetValue(new Circle()));

            var dmgAfterE = new MenuItem("ElEasy.Katarina.DrawComboDamage", "Draw combo damage").SetValue(true);
            var drawFill =
                new MenuItem("ElEasy.Katarina.DrawColour", "Fill colour", true).SetValue(
                    new Circle(true, Color.FromArgb(204, 204, 0, 0)));
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

            Menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = Menu.AddSubMenu(new Menu("Credits", "jQuery"));
            credits.AddItem(new MenuItem("ElEasy.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElEasy.Email", "info@zavox.nl"));

            Menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            Menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            Menu.AddToMainMenu();
        }


        private static bool KatarinaQ(Obj_AI_Base target)
        {
            return target.Buffs.Any(x => x.Name.Contains("katarinaqmark"));
        }

        private static void KillSteal()
        {
            foreach (var hero in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => x.IsValidTarget(1000) && !x.HasBuffOfType(BuffType.Invulnerability))
                    .OrderByDescending(GetComboDamage))
            {
                if (hero != null)
                {
                    var qDamage = spells[Spells.Q].GetDamage(hero);
                    var wDamage = spells[Spells.W].GetDamage(hero);
                    var eDamage = spells[Spells.E].GetDamage(hero);

                    var qMarkDamage = Player.CalcDamage(
                        hero,
                        Damage.DamageType.Magical,
                        Player.FlatMagicDamageMod * 0.15 + Player.Level * 15);

                    if (KatarinaQ(hero) && GetHealth(hero) - wDamage - qMarkDamage < 0 && spells[Spells.W].IsReady()
                        && Player.Distance(hero) < spells[Spells.W].Range)
                    {
                        spells[Spells.W].Cast();
                    }

                    if (GetHealth(hero) - qDamage < 0 && spells[Spells.Q].IsReady()
                        && Player.Distance(hero) < spells[Spells.Q].Range)
                    {
                        spells[Spells.Q].Cast(hero);
                    }

                    if (GetHealth(hero) - eDamage < 0 && spells[Spells.E].IsReady())
                    {
                        spells[Spells.E].Cast(hero);
                    }

                    if (GetHealth(hero) - eDamage - wDamage < 0 && spells[Spells.E].IsReady()
                        && spells[Spells.W].IsReady())
                    {
                        CastE(hero);
                        spells[Spells.W].Cast();
                    }

                    if (hero.Health - eDamage - wDamage - qDamage - IgniteDamage(hero) < 0 && spells[Spells.E].IsReady()
                        && spells[Spells.Q].IsReady() && spells[Spells.W].IsReady())
                    {
                        CastE(hero);
                        spells[Spells.Q].Cast(hero);
                        spells[Spells.W].Cast();
                        Player.Spellbook.CastSpell(Ignite, hero);
                    }

                    if (Player.Distance(hero.ServerPosition) <= spells[Spells.E].Range
                        && (Player.GetSpellDamage(hero, SpellSlot.R) * 5) > hero.Health + 20
                        && Menu.Item("ElEasy.Katarina.Killsteal.R").GetValue<bool>())
                    {
                        if (spells[Spells.R].IsReady())
                        {
                            Orbwalker.SetMovement(false);
                            Orbwalker.SetAttack(false);
                            spells[Spells.R].Cast();
                            return;
                        }
                    }
                }
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || args.SData.Name != "KatarinaR" || !Player.HasBuff("katarinarsound"))
            {
                return;
            }

            isChanneling = true;
            Orbwalker.SetMovement(false);
            Orbwalker.SetAttack(false);
            Utility.DelayAction.Add(1, () => isChanneling = false);
        }

        private static void Obj_AI_Hero_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (sender.IsMe && Environment.TickCount < rStart + 300)
            {
                args.Process = false;
            }
        }

        private static void OnAutoHarass()
        {
            var target = GetEnemy(spells[Spells.Q].Range);
            if (target == null || !target.IsValid || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                return;
            }

            var useQ = Menu.Item("ElEasy.Katarina.AutoHarass.Q").GetValue<bool>();
            var useW = Menu.Item("ElEasy.Katarina.AutoHarass.W").GetValue<bool>();

            if (useQ && spells[Spells.Q].IsReady() && target.IsValidTarget())
            {
                spells[Spells.Q].Cast(target);
            }

            if (useW && spells[Spells.W].IsReady() && target.IsValidTarget(spells[Spells.W].Range))
            {
                spells[Spells.W].Cast();
            }
        }

        private static void OnCombo()
        {
            var target = GetEnemy(spells[Spells.Q].Range);
            if (target == null || !target.IsValid)
            {
                return;
            }

            UseItems(target);

            var useQ = Menu.Item("ElEasy.Katarina.Combo.Q").GetValue<bool>();
            var useW = Menu.Item("ElEasy.Katarina.Combo.W").GetValue<bool>();
            var useE = Menu.Item("ElEasy.Katarina.Combo.E").GetValue<bool>();
            var useR = Menu.Item("ElEasy.Katarina.Combo.R").GetValue<bool>();
            var useI = Menu.Item("ElEasy.Katarina.Combo.Ignite").GetValue<bool>();
            var rSort = Menu.Item("ElEasy.Katarina.Combo.Sort").GetValue<StringList>();
            var forceR = Menu.Item("ElEasy.Katarina.Combo.R.Force").GetValue<bool>();
            var forceRCount = Menu.Item("ElEasy.Katarina.Combo.R.Force.Count").GetValue<Slider>().Value;

            var rdmg = spells[Spells.R].GetDamage(target, 1);

            if (useR && spells[Spells.R].IsReady() && spells[Spells.R].IsInRange(target) && !spells[Spells.Q].IsReady()
                && !spells[Spells.W].IsReady() && !spells[Spells.E].IsReady())
            {
                if (HeroManager.Enemies.Any(x => x.IsValidTarget(spells[Spells.R].Range))
                    && spells[Spells.R].Instance.Name == "KatarinaR")
                {
                    if (target.Health - rdmg < 0 && rSort.SelectedIndex == 1 && !spells[Spells.E].IsReady())
                    {
                        Orbwalker.SetMovement(false);
                        Orbwalker.SetAttack(false);
                        spells[Spells.R].Cast();
                        rStart = Environment.TickCount;
                    }
                    else if (rSort.SelectedIndex == 0 && !spells[Spells.E].IsReady()
                             || forceR && Player.CountEnemiesInRange(spells[Spells.R].Range) <= forceRCount)
                    {
                        Orbwalker.SetMovement(false);
                        Orbwalker.SetAttack(false);
                        spells[Spells.R].Cast();
                        rStart = Environment.TickCount;
                    }
                }
            }

            if (spells[Spells.R].Instance.Name != "KatarinaR")
            {
                return;
            }

            if (useQ && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(target))
            {
                spells[Spells.Q].Cast(target);
            }

            if (useE && spells[Spells.E].IsReady() && spells[Spells.E].IsInRange(target))
            {
                CastE(target);
            }

            if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(target))
            {
                spells[Spells.W].Cast();
                return;
            }

            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health && useI)
            {
                Player.Spellbook.CastSpell(Ignite, target);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            var drawOff = Menu.Item("ElEasy.Katarina.Draw.off").GetValue<bool>();
            var drawQ = Menu.Item("ElEasy.Katarina.Draw.Q").GetValue<Circle>();
            var drawW = Menu.Item("ElEasy.Katarina.Draw.W").GetValue<Circle>();
            var drawE = Menu.Item("ElEasy.Katarina.Draw.E").GetValue<Circle>();
            var drawR = Menu.Item("ElEasy.Katarina.Draw.R").GetValue<Circle>();

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

            if (drawR.Active)
            {
                if (spells[Spells.R].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.R].Range, Color.White);
                }
            }
        }

        private static void OnHarass()
        {
            var target = GetEnemy(spells[Spells.Q].Range);
            if (target == null || !target.IsValid)
            {
                return;
            }

            var useQ = Menu.Item("ElEasy.Katarina.Harass.Q").GetValue<bool>();
            var useW = Menu.Item("ElEasy.Katarina.Harass.W").GetValue<bool>();
            var useE = Menu.Item("ElEasy.Katarina.Harass.E").GetValue<bool>();
            var hMode = Menu.Item("ElEasy.Katarina.Harass.Mode").GetValue<StringList>().SelectedIndex;

            switch (hMode)
            {
                case 0:
                    if (useQ && spells[Spells.Q].IsReady())
                    {
                        spells[Spells.Q].CastOnUnit(target);
                    }
                    break;

                case 1:
                    if (useQ && useW)
                    {
                        if (spells[Spells.Q].IsReady())
                        {
                            spells[Spells.Q].Cast(target);
                        }

                        if (spells[Spells.W].IsInRange(target) && spells[Spells.W].IsReady())
                        {
                            spells[Spells.W].Cast();
                        }
                    }
                    break;

                case 2:
                    if (useQ && useW && useE)
                    {
                        if (spells[Spells.Q].IsReady())
                        {
                            spells[Spells.Q].Cast(target);
                        }

                        if (spells[Spells.E].IsReady())
                            //&& !target.UnderTurret(true) -- need to create a on/off for this
                        {
                            CastE(target);
                        }

                        if (spells[Spells.W].IsReady())
                        {
                            spells[Spells.W].Cast();
                        }
                    }
                    break;
            }
        }

        private static void OnJungleclear()
        {
            var useQ = Menu.Item("ElEasy.Katarina.JungleClear.Q").GetValue<bool>();
            var useW = Menu.Item("ElEasy.Katarina.JungleClear.W").GetValue<bool>();
            var useE = Menu.Item("ElEasy.Katarina.JungleClear.E").GetValue<bool>();

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
                spells[Spells.Q].Cast(minions[0]);
            }

            if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(minions[0]))
            {
                spells[Spells.W].Cast();
            }

            if (useE && spells[Spells.E].IsReady())
            {
                CastE(minions[0]);
            }
        }

        private static void OnLaneclear()
        {
            var useQ = Menu.Item("ElEasy.Katarina.LaneClear.Q").GetValue<bool>();
            var useW = Menu.Item("ElEasy.Katarina.LaneClear.W").GetValue<bool>();
            var useE = Menu.Item("ElEasy.Katarina.LaneClear.E").GetValue<bool>();

            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.Q].Range);
            if (minions.Count <= 0)
            {
                return;
            }

            if (spells[Spells.Q].IsReady() && useQ)
            {
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.Q].Range);
                {
                    foreach (var minion in
                        allMinions)
                    {
                        if (minion.IsValidTarget())
                        {
                            spells[Spells.Q].CastOnUnit(minion);
                            return;
                        }
                    }
                }
            }

            if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(minions[0]))
            {
                spells[Spells.W].Cast();
            }

            if (useE && spells[Spells.E].IsReady())
            {
                CastE(minions[0]);
            }
        }

        private static void OnLasthit()
        {
            var useQ = Menu.Item("ElEasy.Katarina.Lasthit.Q").GetValue<bool>();
            var useW = Menu.Item("ElEasy.Katarina.Lasthit.W").GetValue<bool>();
            var useE = Menu.Item("ElEasy.Katarina.Lasthit.E").GetValue<bool>();

            if (spells[Spells.Q].IsReady() && useQ)
            {
                var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.Q].Range);
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
                            spells[Spells.W].Cast();
                            return;
                        }
                    }
                }
            }

            if (spells[Spells.E].IsReady() && useE)
            {
                foreach (var minion in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            minion =>
                            minion.IsValidTarget() && minion.IsEnemy
                            && minion.Distance(Player.ServerPosition) < spells[Spells.E].Range))
                {
                    var edmg = spells[Spells.E].GetDamage(minion);

                    if (minion.Health - edmg <= 0 && minion.Distance(Player.ServerPosition) <= spells[Spells.E].Range)
                    {
                        CastE(minion);
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

            if (HasRBuff())
            {
                Orbwalker.SetAttack(false);
                Orbwalker.SetMovement(false);
            }
            else
            {
                Orbwalker.SetAttack(true);
                Orbwalker.SetMovement(true);
            }
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    OnCombo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    OnHarass();
                    break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    OnLaneclear();
                    OnJungleclear();
                    break;

                case Orbwalking.OrbwalkingMode.LastHit:
                    OnLasthit();
                    break;
            }

            if (Menu.Item("ElEasy.Katarina.Killsteal").GetValue<bool>())
            {
                KillSteal();
            }

            if (Menu.Item("ElEasy.Katarina.AutoHarass.Activated", true).GetValue<KeyBind>().Active)
            {
                OnAutoHarass();
            }

            if (Menu.Item("ElEasy.Katarina.Wardjump").GetValue<KeyBind>().Active)
            {
                DoWardJump();
            }

            var autor = new[] { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };
            int qOff = 0, wOff = 0, eOff = 0, rOff = 0;

            var qL = Player.Spellbook.GetSpell(SpellSlot.Q).Level + qOff;
            var wL = Player.Spellbook.GetSpell(SpellSlot.W).Level + wOff;
            var eL = Player.Spellbook.GetSpell(SpellSlot.E).Level + eOff;
            var rL = Player.Spellbook.GetSpell(SpellSlot.R).Level + rOff;
            if (qL + wL + eL + rL < ObjectManager.Player.Level)
            {
                var level = new[] { 0, 0, 0, 0 };
                for (var i = 0; i < ObjectManager.Player.Level; i++)
                {
                    level[autor[i] - 1] = level[autor[i] - 1] + 1;
                }
                if (qL < level[0])
                {
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                }
                if (wL < level[1])
                {
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                }
                if (eL < level[2])
                {
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                }
                if (rL < level[3])
                {
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
                }
            }
        }

        private static void UseItems(Obj_AI_Base target)
        {
            var useHextech = Menu.Item("ElEasy.Katarina.Items.hextech").GetValue<bool>();
            if (useHextech)
            {
                var cutlass = ItemData.Bilgewater_Cutlass.GetItem();
                var hextech = ItemData.Hextech_Gunblade.GetItem();

                if (cutlass.IsReady() && cutlass.IsOwned(Player) && cutlass.IsInRange(target))
                {
                    cutlass.Cast(target);
                }

                if (hextech.IsReady() && hextech.IsOwned(Player) && hextech.IsInRange(target))
                {
                    hextech.Cast(target);
                }
            }
        }

        #endregion
    }
}