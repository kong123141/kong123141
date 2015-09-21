using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Poppy : PluginData
    {
        public Poppy()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E,525);
            R = new Spell(SpellSlot.R,900);
           
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("TurretDive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("UseItemCombo", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclear = new Menu("Farm", "Farm");
            {
                laneclear.AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
                laneclear.AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true)); 
                laneclear.AddItem(new MenuItem("FarmMana", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclear);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true)); 
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("UseEMisc", "E Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseE2Misc", "E Gapcloser").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawE", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawR", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var UseR = config.Item("UseRCombo").GetValue<bool>();
            var CastItems = config.Item("UseItemCombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToPhysical(target)) return;
                if (CastItems) { myUtility.UseItems(0, target); }
                try
                {
                    if (UseE && E.IsReady())
                    {
                        EPredict(target);
                    }
                    if (UseW && W.IsReady() && Orbwalking.InAutoAttackRange(target))
                    {
                        W.Cast();
                    }
                    if (UseR && R.IsReady() && Orbwalking.InAutoAttackRange(target))
                    {
                        BestR();//R.CastOnUnit(target);
                    }
                    if (CastItems)
                    {
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= 450f)
                        {
                            myUtility.UseItems(1, target);
                        }
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < 500f)
                        {
                            myUtility.UseItems(3, null);
                        }
                    }
                }
                catch { }
            }            
        }
        private void Harass()
        {
            var UseE = config.Item("UseEHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget() )
            {
                if (UseE && E.IsReady())
                {
                    EPredict(target);
                }               
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady())
            {
                var minionE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (minionE == null) return;
                var siegeE = myUtility.GetLargeMinions(E.Range).FirstOrDefault(x => E.IsKillable(x));
                if (siegeE != null && siegeE.IsValidTarget())
                {
                    E.CastOnUnit(siegeE);
                }
                else
                {
                    var AnyE = minionE.OrderBy(i => i.Distance(Player)).FirstOrDefault(x => E.IsKillable(x));
                    if (AnyE != null && AnyE.IsValidTarget())
                    {
                        if (myUtility.IsFacing(Player, AnyE.ServerPosition, 60)) E.CastOnUnit(AnyE);
                    }
                }
            }    
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myUtility.GetLargeMonsters(E.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob == null) return;
            if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && E.IsInRange(mob))
            {
                if (largemobs != null && Player.ServerPosition.Extend(largemobs.ServerPosition, 300).IsWall())
                {
                   E.CastOnUnit(largemobs);
                }
            }            
        }
        private void BestR()
        {
            if (R.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !myUtility.ImmuneToMagic(x));
                var target = EnemyList.Where(x => x.IsVisible &&
                             Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range
                             ).OrderBy(i => i.BaseAttackDamage).ThenBy(i => i.BaseAbilityDamage).FirstOrDefault();
                if (target != null && target.IsValidTarget())
                {
                    R.CastOnUnit(target);
                }
            }
        }
        private void EPredict(Obj_AI_Hero target)
        {
            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < E.Range)
            {

                if (Player.ServerPosition.Extend(target.ServerPosition, 300).IsWall())
                {
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass)
                    {
                        if (Player.ServerPosition.Extend(target.ServerPosition, 300).UnderTurret(true)) return;
                        E.CastOnUnit(target);
                    }
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                    {
                        if (Player.ServerPosition.Extend(target.ServerPosition, 300).UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                        E.CastOnUnit(target);
                    }
                }
            }
        }

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                myUtility.Reset();
                return;
            }
            switch (myOrbwalker.ActiveMode)
            {
                case myOrbwalker.OrbwalkingMode.None:
                    myUtility.Reset();
                    break;
                case myOrbwalker.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case myOrbwalker.OrbwalkingMode.Harass:
                    Harass();
                    break;
                case myOrbwalker.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                case myOrbwalker.OrbwalkingMode.Hybrid:
                    LaneClear();
                    Harass();
                    break;
                case myOrbwalker.OrbwalkingMode.JungleClear:
                    JungleClear();
                    break;
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("UseE2Misc").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) < E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    E.CastOnUnit(gapcloser.Sender);
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (sender.IsEnemy)
            {
                if (myUtility.ImmuneToMagic(sender) || myUtility.ImmuneToCC(sender)) return;
                if (config.Item("UseEMisc").GetValue<bool>() && E.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.High)
                {
                    if (Vector3.Distance(Player.ServerPosition, sender.ServerPosition) < E.Range)
                    {
                        E.CastOnUnit(sender);
                    }
                }
            }
        }
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
                var target = minion as Obj_AI_Base;
                if (target != null &&
                    Q.IsKillable(target) &&
                    !Player.IsWindingUp &&
                    Vector3.Distance(Player.ServerPosition, target.ServerPosition) < 300)
                {
                    Q.Cast();
                }
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (config.Item("UseQCombo").GetValue<bool>() &&
                        !Player.IsWindingUp &&
                        Q.IsReady() &&
                        target.IsValidTarget()) Q.Cast();
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass)
                {
                    if (config.Item("UseQHarass").GetValue<bool>() &&
                        !Player.IsWindingUp &&
                        Q.IsReady() &&
                        target.IsValidTarget()) Q.Cast();
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear)
                {
                    if (target is Obj_AI_Minion && target.Team == GameObjectTeam.Neutral && !target.Name.Contains("Mini") &&
                        !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                    {
                        if (Q.IsReady() && config.Item("UseQJFarm").GetValue<bool>()) Q.Cast();
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("DrawE").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.White);
            }
            if (config.Item("DrawR").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, Color.Fuchsia);
                if (R.IsReady())
                {
                    var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !myUtility.ImmuneToMagic(x));
                    var target = EnemyList.Where(x => x.IsVisible &&
                                 Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range
                                 ).OrderBy(i => i.BaseAttackDamage).ThenBy(i => i.BaseAbilityDamage).FirstOrDefault();
                    if (target != null && target.IsValidTarget())
                    {
                        Render.Circle.DrawCircle(target.Position, target.BoundingRadius, Color.Lime);
                    }
                }
            }
        }
    }
}