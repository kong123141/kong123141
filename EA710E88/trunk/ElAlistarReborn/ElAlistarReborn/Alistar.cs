namespace ElAlistarReborn
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    internal enum Spells
    {
        Q,

        W,

        E,

        R
    }

    internal static class Alistar
    {
        #region Static Fields

        public static Vector3 InsecClickPos;

        public static Orbwalking.Orbwalker Orbwalker;

        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>
                                                             {
                                                                 { Spells.Q, new Spell(SpellSlot.Q, 365) },
                                                                 { Spells.W, new Spell(SpellSlot.W, 650) },
                                                                 { Spells.E, new Spell(SpellSlot.E, 575) },
                                                                 { Spells.R, new Spell(SpellSlot.R, 0) }
                                                             };

        private static SpellSlot flashSlot;

        private static SpellSlot ignite;

        private static Vector2 InsecLinePos;

        private static Vector3 insecPos;

        private static bool isNullInsecPos = true;

        #endregion

        #region Properties

        private static Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        #endregion

        #region Public Methods and Operators

        public static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.CharData.BaseSkinName != "Alistar")
            {
                return;
            }

            //float.MaxValue
            spells[Spells.W].SetTargetted(0.5f, 1.5f);

            Notifications.AddNotification("ElAlistarReborn by jQuery", 5000);
            ignite = Player.GetSpellSlot("summonerdot");
            flashSlot = Player.GetSpellSlot("summonerflash");

            ElAlistarMenu.Initialize();
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawings.OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        #endregion

        #region Methods

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var gapCloserActive = ElAlistarMenu.Menu.Item("ElAlistar.Interrupt").GetValue<bool>();

            if (gapCloserActive && spells[Spells.W].IsReady()
                && gapcloser.Sender.Distance(Player) < spells[Spells.W].Range)
            {
                spells[Spells.W].Cast(gapcloser.Sender);
            }

            if (gapCloserActive && !spells[Spells.W].IsReady() && spells[Spells.Q].IsReady()
                && gapcloser.Sender.Distance(Player) < spells[Spells.Q].Range)
            {
                spells[Spells.Q].Cast(gapcloser.Sender);
            }
        }

        private static List<Obj_AI_Hero> GetAllyHeroes(Obj_AI_Hero position, int range)
        {
            var temp = new List<Obj_AI_Hero>();

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsAlly && !hero.IsMe && hero.Distance(position) < range)
                {
                    temp.Add(hero);
                }
            }
            return temp;
        }

        private static List<Obj_AI_Hero> GetAllyInsec(List<Obj_AI_Hero> heroes)
        {
            var alliesAround = 0;
            var tempObject = new Obj_AI_Hero();
            foreach (var hero in heroes)
            {
                var localTemp = GetAllyHeroes(hero, 500).Count;
                if (localTemp > alliesAround)
                {
                    tempObject = hero;
                    alliesAround = localTemp;
                }
            }
            return GetAllyHeroes(tempObject, 500);
        }

        private static Vector3 GetInsecPos(Obj_AI_Hero target)
        {
            if (ElAlistarMenu.Menu.Item("ElAlistar.Combo.Click").GetValue<bool>())
            {
                InsecLinePos = Drawing.WorldToScreen(InsecClickPos);
                return V2E(InsecClickPos, target.Position, target.Distance(InsecClickPos) + 230).To3D();
            }

            if (isNullInsecPos)
            {
                isNullInsecPos = false;
                insecPos = Player.Position;
            }

            var turrets = (from tower in ObjectManager.Get<Obj_Turret>()
                           where
                               tower.IsAlly && !tower.IsDead && target.Distance(tower.Position) < 1500
                               && tower.Health > 0
                           select tower).ToList();

            if (GetAllyHeroes(target, 2000).Count > 0)
            {
                Console.WriteLine("xxxx");
                var insecPosition = InterceptionPoint(GetAllyInsec(GetAllyHeroes(target, 2000)));
                InsecLinePos = Drawing.WorldToScreen(insecPosition);

                return V2E(insecPosition, target.Position, target.Distance(insecPosition) + 350).To3D();
            }

            /*if (turrets.Any())
            {
                InsecLinePos = Drawing.WorldToScreen(turrets[0].Position);
                return V2E(turrets[0].Position, target.Position, target.Distance(turrets[0].Position) + 230).To3D();
            }*/

         
            return new Vector3();
        }

        private static float GetWDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (spells[Spells.W].IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);
            }

            return (float)damage;
        }

        private static void HealManager()
        {
            var useHeal = ElAlistarMenu.Menu.Item("ElAlistar.Heal.Activated").GetValue<bool>();
            var useHealAlly = ElAlistarMenu.Menu.Item("ElAlistar.Heal.Ally.Activated").GetValue<bool>();
            var playerMana = ElAlistarMenu.Menu.Item("ElAlistar.Heal.Player.Mana").GetValue<Slider>().Value;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                return;
            }

            if (Player.HasBuff("Recall") || Player.InFountain() || Player.Mana < playerMana
                || !spells[Spells.E].IsReady() || !useHeal)
            {
                return;
            }

            var playerHp = ElAlistarMenu.Menu.Item("ElAlistar.Heal.Player.HP").GetValue<Slider>().Value;
            var allyHp = ElAlistarMenu.Menu.Item("ElAlistar.Heal.Ally.HP").GetValue<Slider>().Value;

            //self heal
            if ((Player.Health / Player.MaxHealth) * 100 < playerHp)
            {
                spells[Spells.E].Cast(Player);
            }

            //ally
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && !h.IsMe))
            {
                if (useHealAlly && (hero.Health / hero.MaxHealth) * 100 <= allyHp && spells[Spells.E].IsInRange(hero))
                {
                    spells[Spells.E].Cast(Player);
                }
            }
        }

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        private static Vector3 InterceptionPoint(List<Obj_AI_Hero> heroes)
        {
            var result = new Vector3();
            foreach (var hero in heroes)
            {
                result += hero.Position;
            }
            result.X /= heroes.Count;
            result.Y /= heroes.Count;
            return result;
        }

        private static void Interrupter2_OnInterruptableTarget(
            Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel != Interrupter2.DangerLevel.High || sender.Distance(Player) > spells[Spells.Q].Range)
            {
                return;
            }

            if (sender.IsValidTarget(spells[Spells.Q].Range) && args.DangerLevel == Interrupter2.DangerLevel.High
                && spells[Spells.Q].IsReady())
            {
                spells[Spells.Q].Cast(sender);
            }

            if (sender.IsValidTarget(spells[Spells.W].Range) && args.DangerLevel == Interrupter2.DangerLevel.High
                && !spells[Spells.Q].IsReady() && spells[Spells.W].IsReady())
            {
                spells[Spells.W].Cast(sender);
            }
        }

        private static void OnCombo()
        {
            var target = TargetSelector.GetSelectedTarget();
            if (target == null || !target.IsValidTarget())
            {
                target = TargetSelector.GetTarget(spells[Spells.W].Range, TargetSelector.DamageType.Physical);
            }

            if (!target.IsValidTarget(spells[Spells.W].Range))
            {
                return;
            }

            var useQ = ElAlistarMenu.Menu.Item("ElAlistar.Combo.Q").GetValue<bool>();
            var useW = ElAlistarMenu.Menu.Item("ElAlistar.Combo.W").GetValue<bool>();
            var useR = ElAlistarMenu.Menu.Item("ElAlistar.Combo.R").GetValue<bool>();
            var useI = ElAlistarMenu.Menu.Item("ElAlistar.Combo.Ignite").GetValue<bool>();
            var enemiesInRange = ElAlistarMenu.Menu.Item("ElAlistar.Combo.Count.Enemies").GetValue<Slider>().Value;
            var rHealth = ElAlistarMenu.Menu.Item("ElAlistar.Combo.HP.Enemies").GetValue<Slider>().Value;

            var qmana = Player.Spellbook.GetSpell(SpellSlot.Q);
            var wmana = Player.Spellbook.GetSpell(SpellSlot.W);

            if (useQ && useW && spells[Spells.Q].IsReady() && spells[Spells.W].IsReady()
                && Player.Mana > qmana.ManaCost + wmana.ManaCost)
            {
                spells[Spells.W].Cast(target);
                //var comboTime = Math.Max(0, Player.Distance(target) - 500) * 10 / 25 + 25;
                var comboTime = Math.Max(0, Player.Distance(target) - 365) / 1.2f - 25;

                Utility.DelayAction.Add((int)comboTime, () => spells[Spells.Q].Cast());
            }

            if (useR && Player.CountEnemiesInRange(spells[Spells.W].Range) >= enemiesInRange
                && (Player.Health / Player.MaxHealth) * 100 >= rHealth)
            {
                spells[Spells.R].Cast(Player);
            }

            //Check if target is killable with W when Q is on CD
            if (spells[Spells.W].IsReady() && !spells[Spells.Q].IsReady() && spells[Spells.W].IsInRange(target)
                && GetWDamage(target) > target.Health)
            {
                spells[Spells.W].Cast(target);
            }

            // Ignite when killable
            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health && useI)
            {
                Player.Spellbook.CastSpell(ignite, target);
            }
        }

        private static void OnHarass()
        {
            var target = TargetSelector.GetSelectedTarget();
            if (target == null || !target.IsValidTarget())
            {
                target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Physical);
            }

            if (!target.IsValidTarget(spells[Spells.Q].Range))
            {
                return;
            }

            var useQ = ElAlistarMenu.Menu.Item("ElAlistar.Harass.Q").GetValue<bool>();

            if (useQ && spells[Spells.Q].IsReady())
            {
                spells[Spells.Q].Cast(target);
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    OnCombo();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    OnHarass();
                    break;
            }

            if (ElAlistarMenu.Menu.Item("ElAlistar.Combo.FlashQ").GetValue<KeyBind>().Active
                && spells[Spells.Q].IsReady())
            {
                Orbwalk(Game.CursorPos);

                var target = ElAlistarMenu.Menu.Item("ElAlistar.Combo.Click").GetValue<bool>()
                                 ? TargetSelector.GetSelectedTarget()
                                 : TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);

                if (!target.IsValidTarget())
                {
                    return;
                }

                Player.Spellbook.CastSpell(flashSlot, target.ServerPosition);
                Utility.DelayAction.Add(50, () => spells[Spells.Q].Cast());
            }

            /*if (ElAlistarMenu.Menu.Item("ElAlistar.Combo.Flash").GetValue<KeyBind>().Active
                && spells[Spells.W].IsReady()) //spells[Spells.Q].IsReady()
            {
                Orbwalk(Game.CursorPos);

                var target = ElAlistarMenu.Menu.Item("ElAlistar.Combo.Click").GetValue<bool>()
                                 ? TargetSelector.GetSelectedTarget()
                                 : TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);

                if (!target.IsValidTarget())
                {
                    return;
                }

                Player.Spellbook.CastSpell(flashSlot, GetInsecPos((Obj_AI_Hero)(target)));
                Utility.DelayAction.Add(50, () => spells[Spells.Q].Cast());
                //Utility.DelayAction.Add(50, () => Player.Spellbook.CastSpell(SpellSlot.W, GetInsecPos((Obj_AI_Hero)(target))));
                Utility.DelayAction.Add(550, () => spells[Spells.W].CastOnUnit(target));
            }*/

            HealManager();
        }

        private static void Orbwalk(Vector3 pos, Obj_AI_Hero target = null)
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, pos);
        }

        private static Vector2 V2E(Vector3 from, Vector3 direction, float distance)
        {
            return from.To2D() + distance * Vector3.Normalize(direction - from).To2D();
        }

        #endregion
    }
}