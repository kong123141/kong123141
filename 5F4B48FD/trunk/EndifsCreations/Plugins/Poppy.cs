using System;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
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
                combomenu.AddItem(new MenuItem("EC.Poppy.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Poppy.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Poppy.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Poppy.Combo.R", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Poppy.Combo.Dive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Poppy.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Poppy.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Poppy.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Poppy.Harass.E", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Poppy.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Poppy.Farm.E", "Use E").SetValue(true)); 
                laneclearmenu.AddItem(new MenuItem("EC.Poppy.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Poppy.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Poppy.Jungle.E", "Use E").SetValue(true)); 
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Poppy.Misc.E", "E Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Poppy.Misc.E2", "E Gapcloser").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Poppy.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Poppy.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            var UseE = config.Item("EC.Poppy.Combo.E").GetValue<bool>();
            var UseR = config.Item("EC.Poppy.Combo.R").GetValue<bool>();
            var CastItems = config.Item("EC.Poppy.Combo.Items").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;
                if (CastItems) { myItemManager.UseItems(0, Target); }
                try
                {
                    if (UseE && E.IsReady())
                    {
                        //EPredict(Target);
                        if (E.IsKillable(Target))
                        {
                            E.CastOnUnit(Target);
                        }
                        else 
                        {
                            var box = new Geometry.Polygon.Rectangle(Target.ServerPosition, Player.ServerPosition.Extend(Target.ServerPosition, Vector3.Distance(Player.ServerPosition, Target.ServerPosition) + 300 - Target.BoundingRadius), Target.BoundingRadius); //reduced
                            if (box.Points.Any(point => myUtility.PointCollides(point.To3D())))
                            {
                                E.CastOnUnit(Target);
                            }
                        }
                    }
                    if (UseR && R.IsReady() && Orbwalking.InAutoAttackRange(Target) && Player.CountEnemiesInRange(R.Range) > 1)
                    {
                        var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !myUtility.ImmuneToMagic(x));
                        var rtarget = EnemyList.Where(x => x.IsVisible &&
                                     Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range &&
                                     x != Target).OrderBy(i => i.BaseAttackDamage).ThenBy(i => i.BaseAbilityDamage).FirstOrDefault();
                        if (rtarget != null && rtarget.IsValidTarget())
                        {
                            R.CastOnUnit(rtarget);
                        }
                    }
                    if (CastItems)
                    {
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= 450f)
                        {
                            myItemManager.UseItems(1, Target);
                        }
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) < 500f)
                        {
                            myItemManager.UseItems(3, null);
                        }
                    }
                }
                catch { }
            }            
        }
        private void Harass()
        {
            var UseE = config.Item("EC.Poppy.Harass.E").GetValue<bool>();
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget() )
            {
                if (UseE && E.IsReady())
                {
                    //EPredict(target);
                    E.CastOnUnit(target);
                }               
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("EC.Poppy.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (config.Item("EC.Poppy.Farm.E").GetValue<bool>() && E.IsReady())
            {
                var minionE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (minionE == null) return;
                var siegeE = myFarmManager.GetLargeMinions(E.Range).FirstOrDefault(x => E.IsKillable(x));
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
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, E.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob == null) return;
            if (config.Item("EC.Poppy.Jungle.E").GetValue<bool>() && E.IsReady() && E.IsInRange(mob))
            {
                if (largemobs != null && Player.ServerPosition.Extend(largemobs.ServerPosition, 300).IsWall())
                {
                   E.CastOnUnit(largemobs);
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
                        if (Player.ServerPosition.Extend(target.ServerPosition, 300).UnderTurret(true) && !config.Item("EC.Poppy.Combo.Dive").GetValue<bool>()) return;
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
            if (config.Item("EC.Poppy.Misc.E2").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.CastOnUnit(gapcloser.Sender));
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (sender.IsEnemy)
            {
                if (myUtility.ImmuneToMagic(sender) || myUtility.ImmuneToCC(sender)) return;
                if (config.Item("EC.Poppy.Misc.E").GetValue<bool>() && E.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.High)
                {
                    if (Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= E.Range)
                    {
                        
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.CastOnUnit(sender));
                    }
                }
            }
        }
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (config.Item("EC.Poppy.Farm.Q").GetValue<bool>() && Q.IsReady())
            {
                if (myUtility.PlayerManaPercentage < config.Item("EC.Poppy.Farm.ManaPercent").GetValue<Slider>().Value) return;
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
                    if (config.Item("EC.Poppy.Combo.Q").GetValue<bool>() &&
                        !Player.IsWindingUp &&
                        Q.IsReady() &&
                        target.IsValidTarget()) Q.Cast();
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass)
                {
                    if (config.Item("EC.Poppy.Harass.Q").GetValue<bool>() &&
                        !Player.IsWindingUp &&
                        Q.IsReady() &&
                        target.IsValidTarget()) Q.Cast();
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear)
                {
                    if (target is Obj_AI_Minion && target.Team == GameObjectTeam.Neutral && !target.Name.Contains("Mini") &&
                        !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                    {
                        if (Q.IsReady() && config.Item("EC.Poppy.Jungle.Q").GetValue<bool>()) Q.Cast();
                    }
                }
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (config.Item("EC.Poppy.Combo.W").GetValue<bool>() && W.IsReady())
                    {
                        W.Cast();
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Poppy.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
                Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (Target.IsValidTarget())
                {
                    var box = new Geometry.Polygon.Rectangle(Target.ServerPosition, Player.ServerPosition.Extend(Target.ServerPosition, Vector3.Distance(Player.ServerPosition, Target.ServerPosition) + 300 - Target.BoundingRadius), Target.BoundingRadius);
                    if (box.Points.Any(point => myUtility.PointCollides(point.To3D())))
                    {
                        box.Draw(Color.Red, 4);
                    }
                    else
                    {
                        box.Draw(Color.White, 4);
                    }
                }
            }
            if (config.Item("EC.Poppy.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
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