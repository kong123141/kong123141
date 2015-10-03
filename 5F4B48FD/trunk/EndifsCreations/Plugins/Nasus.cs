using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
{
    class Nasus : PluginData
    {
        public Nasus()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 150);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 175);

            E.SetSkillshot(0.5f, 400f, 450f, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {            
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Nasus.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Nasus.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Nasus.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Nasus.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Nasus.Combo.Dive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Nasus.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Nasus.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Nasus.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Nasus.Harass.E", "Use E").SetValue(true));
                Root.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Nasus.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Nasus.UseQLastHit", "(Last Hit) Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Nasus.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Nasus.Farm.E.Value", "E More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Nasus.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                Root.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Nasus.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Nasus.Jungle.E", "Use E").SetValue(true)); 
                Root.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Nasus.Misc.Q", "Q Turrets").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Nasus.Misc.W", "W Gapcloser").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Nasus.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Nasus.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Nasus.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range + 400, TargetSelector.DamageType.Physical);

            //var UseQ = Root.Item("EC.Nasus.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Nasus.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Nasus.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Nasus.Combo.R").GetValue<bool>();
            var CastItems = Root.Item("EC.Nasus.Combo.Items").GetValue<bool>();
            if (UseR && R.IsReady())
            {
                if (Player.CountEnemiesInRange(300) > 1) R.Cast();
                else if (Target.IsValidTarget() && Orbwalking.InAutoAttackRange(Target) && myUtility.PlayerHealthPercentage < 50) R.Cast();
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;                
                if (CastItems) { myItemManager.UseItems(0, Target); }
                try
                {
                    if (UseW && W.IsReady() && W.IsInRange(Target))
                    {
                        W.CastOnUnit(Target);
                    }
                    if (UseE && E.IsReady())
                    {
                        mySpellcast.CircularAoe(Target, E, HitChance.High, E.Range, 400);
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
            var UseW = Root.Item("EC.Nasus.Harass.W").GetValue<bool>();
            var UseE = Root.Item("EC.Nasus.Harass.E").GetValue<bool>();
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                if (UseW && W.IsReady() && W.IsInRange(target))
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    W.CastOnUnit(target);
                }
                if (UseE && E.IsReady())
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    mySpellcast.CircularAoe(target, E, HitChance.High, E.Range, 400);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < Root.Item("EC.Nasus.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (Root.Item("EC.Nasus.Farm.Q").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp && myOrbwalker.Waiting)
            {
                Q.Cast();
            }
            if (Root.Item("EC.Nasus.Farm.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
            {
                if (Player.UnderTurret(true)) return;
                var minionE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (minionE == null) return;
                var epred = E.GetCircularFarmLocation(minionE);
                if (epred.MinionsHit > Root.Item("EC.Nasus.Farm.E.Value").GetValue<Slider>().Value)
                {
                    E.Cast(epred.Position);
                }
            } 
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, E.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null && !Player.IsWindingUp)
            {
                if (Root.Item("EC.Nasus.Jungle.Q").GetValue<bool>() && Q.IsReady() && Orbwalking.InAutoAttackRange(mob))
                {
                    if (largemobs != null && Q.IsKillable(largemobs))
                    {
                        Q.Cast();
                    }
                    else if (Q.IsKillable(mob))
                    {
                        Q.Cast();
                    }
                }               
            }
            if (Root.Item("EC.Nasus.Jungle.E").GetValue<bool>() && E.IsReady())
            {
                myFarmManager.JungleCircular(E, E.Range, 400);
            }
        }
        private void LastHit()
        {
            if (myOrbwalker.Waiting && !Player.IsWindingUp && Root.Item("EC.Nasus.UseQLastHit").GetValue<bool>() && Q.IsReady())
            {
                Q.Cast();
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
                    //Harass();
                    Target = myUtility.GetTarget(E.Range + 400, TargetSelector.DamageType.Magical);
                    if (Target.IsValidTarget())
                    {
                        if (E.IsReady())
                        {
                            mySpellcast.PointVector(Target.Position, E, Target.BoundingRadius);
                        }
                    }
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
                case myOrbwalker.OrbwalkingMode.Lasthit:
                    LastHit();
                    break;
            }
        }
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (Root.Item("EC.Nasus.Farm.Q").GetValue<bool>() && Q.IsReady() && (myUtility.PlayerManaPercentage > Root.Item("EC.Nasus.Farm.ManaPercent").GetValue<Slider>().Value))
            {
                var target = minion as Obj_AI_Base;
                if (target != null &&
                    Q.IsKillable(target) &&
                    Orbwalking.InAutoAttackRange(target))
                {
                    Q.Cast();
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Nasus.Misc.W").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) <= W.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.CastOnUnit(gapcloser.Sender));                    
                }
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {                    
                    if (Root.Item("EC.Nasus.Combo.Q").GetValue<bool>() && Q.IsReady())
                    {
                        Q.Cast();
                    }
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
                    if (Root.Item("EC.Nasus.Combo.Items").GetValue<bool>() && Orbwalking.InAutoAttackRange(target))
                    {
                        myItemManager.UseItems(2, null);
                    }
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear)
                {
                    if (target is Obj_AI_Minion && target.Team == GameObjectTeam.Neutral && !target.Name.Contains("Mini") &&
                        Orbwalking.InAutoAttackRange(target))
                    {
                        myItemManager.UseItems(2, null);
                    }
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear)
                {
                    if (target is Obj_AI_Turret && target.Team != Player.Team &&
                        Root.Item("EC.Nasus.Misc.Q").GetValue<bool>() &&
                        Orbwalking.InAutoAttackRange(target))
                    {
                        Q.Cast();
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Nasus.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Nasus.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Nasus.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.White);
            }
        }
    }
}
