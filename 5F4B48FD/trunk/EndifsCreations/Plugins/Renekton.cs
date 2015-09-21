using System;
using System.Linq;
using EndifsCreations.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
{
    class Renekton : PluginData
    {
        public Renekton()
        {
            LoadSpells();
            LoadMenus();
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 300);
            W = new Spell(SpellSlot.W, Player.AttackRange + 50);
            E = new Spell(SpellSlot.E, 450);
            R = new Spell(SpellSlot.R, 175);
            
            E.SetSkillshot(0.5f, E.Instance.SData.LineWidth, float.MaxValue, true, SkillshotType.SkillshotLine);
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);                       
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Renekton.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Renekton.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Renekton.Combo.E", "Use E (Slice)").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Renekton.Combo.E2", "Use E (Dice)").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Renekton.Combo.R", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Renekton.Combo.Dive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Renekton.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Renekton.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Renekton.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Renekton.Harass.E", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Renekton.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Renekton.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Renekton.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Renekton.Farm.Q.Value", "Q More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Renekton.Farm.E.Value", "E More Than").SetValue(new Slider(1, 1, 5)));
                config.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Renekton.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Renekton.Jungle.W", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Renekton.Jungle.E", "Use E").SetValue(true));
                config.AddSubMenu(junglemenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Renekton.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Renekton.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Renekton.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Renekton.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(W.Range + E.Range, TargetSelector.DamageType.Physical);  

            var UseQ = config.Item("EC.Renekton.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Renekton.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Renekton.Combo.E").GetValue<bool>();
            var UseE2 = config.Item("EC.Renekton.Combo.E2").GetValue<bool>();       
            var UseR = config.Item("EC.Renekton.Combo.R").GetValue<bool>();
            var CastItems = config.Item("EC.Renekton.Combo.Items").GetValue<bool>();
            if (UseR && R.IsReady())
            {
                if (Player.CountEnemiesInRange(R.Range) > 1) R.Cast();
                else if (Target.IsValidTarget() && Orbwalking.InAutoAttackRange(Target) && myUtility.PlayerHealthPercentage < 50) R.Cast();
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;
                if (CastItems) { myItemManager.UseItems(0, Target); }
                try
                {
                    if (UseW && UseE && W.IsReady() && E.IsReady())
                    {
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) > Q.Range &&
                            Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= W.Range + E.Range)
                        {
                            if (Target.UnderTurret(true) && !config.Item("EC.Renekton.Combo.Dive").GetValue<bool>()) return;
                            W.Cast();
                            E.Cast(Target.ServerPosition);
                        }
                    }
                    if (UseQ && Q.IsReady() && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= Q.Range)
                    {
                        Q.Cast();
                    }
                    if (UseW && W.IsReady() && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= W.Range)
                    {
                        W.Cast();
                    } 
                    if (UseE && E.IsReady() && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= E.Range)
                    {
                        if (Target.UnderTurret(true) && !config.Item("EC.Renekton.Combo.Dive").GetValue<bool>()) return;
                        if (!CanDice)
                        {
                            E.Cast(Target.ServerPosition);
                        }
                        if (UseE2 && CanDice)
                        {
                            E.Cast(Target.ServerPosition);
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
                catch
                {
                }
            }
        }
        private void Harass()
        {
            var UseQ = config.Item("EC.Renekton.Harass.Q").GetValue<bool>();
            var UseW = config.Item("EC.Renekton.Harass.W").GetValue<bool>();
            var UseE = config.Item("EC.Renekton.Harass.E").GetValue<bool>();
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (target != null && target.IsValidTarget())
            {
                if (UseQ && Q.IsReady() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range)
                {
                    Q.Cast();
                }
                if (UseW && W.IsReady() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < W.Range)
                {
                    W.Cast();
                }
                if (UseE && E.IsReady() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < E.Range)
                {
                    E.Cast(target.ServerPosition);
                }
            }
        }
        private void LaneClear()
        {
            if (config.Item("EC.Renekton.Farm.Q").GetValue<bool>() && Q.IsReady() && (!myOrbwalker.IsWaiting() || Fury) && !Player.IsWindingUp)
            {
                var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (allMinionsQ == null) return;
                var siegeQ = myFarmManager.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x) && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < Q.Range);
                if (siegeQ != null && siegeQ.IsValidTarget())
                {
                    Q.Cast();
                }
                if (allMinionsQ.Count > config.Item("EC.Renekton.Farm.Q.Value").GetValue<Slider>().Value)
                {
                    if (Player.UnderTurret(true)) return;
                    Q.Cast();
                }
            }
            if (config.Item("EC.Renekton.Farm.E").GetValue<bool>() && E.IsReady() && (!myOrbwalker.IsWaiting() || !Fury) && !Player.IsWindingUp)
            {
                var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (allMinionsE == null) return;
                if (CanDice)
                {
                    var SelectE = allMinionsE.FirstOrDefault(x => E.IsKillable(x) && !x.UnderTurret(true));
                    if (SelectE != null && SelectE.IsValidTarget())
                    {
                        E.Cast(SelectE.ServerPosition);
                    }
                }
                var ELine = E.GetLineFarmLocation(allMinionsE, E.Width);
                if (ELine.Position.IsValid() && !ELine.Position.To3D().UnderTurret(true))
                {
                    if (ELine.MinionsHit > config.Item("EC.Renekton.Farm.E.Value").GetValue<Slider>().Value)
                    {
                        E.Cast(ELine.Position);
                    }
                }
            }
        }        
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range + E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, Q.Range + E.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (config.Item("EC.Renekton.Jungle.Q").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < Q.Range)
                    {
                        Q.Cast();
                    }
                    if (Vector3.Distance(Player.ServerPosition, mob.ServerPosition) < Q.Range)
                    {
                        Q.Cast();
                    }
                }
                if (config.Item("EC.Renekton.Jungle.W").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && Orbwalking.InAutoAttackRange(largemobs))
                    {
                        W.Cast();
                    }
                    W.Cast();
                }
                if (config.Item("EC.Renekton.Jungle.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < E.Range)
                    {
                       E.Cast(largemobs.ServerPosition);
                    }
                    if (Vector3.Distance(Player.ServerPosition, mob.ServerPosition) < E.Range)
                    {
                        E.Cast(mob.ServerPosition);
                    }
                }
            }
        }

        private bool Fury
        {
            get { return Player.HasBuff("renektonrageready"); }
        }
        private bool WBuff
        {
            get { return Player.HasBuff("renektonpreexecute"); }
        }
        private bool CanDice
        {
            get { return Player.HasBuff("renektonsliceanddicedelay"); }
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
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (config.Item("EC.Renekton.Farm.W").GetValue<bool>() && W.IsReady() && !Fury)
            {
                var target = minion as Obj_AI_Base;
                if (target != null &&
                    W.IsKillable(target) &&
                    Orbwalking.InAutoAttackRange(target))
                {
                    W.Cast();
                }
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Minion && args.Target.Team == GameObjectTeam.Neutral)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear &&
                    !args.Target.Name.Contains("Mini") &&
                    !Player.IsWindingUp &&
                    Orbwalking.InAutoAttackRange(args.Target))
                {
                    myItemManager.UseItems(2, null);
                }
            } 
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe )
            {
                if (target is Obj_AI_Hero)
                {
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                    {
                        if (config.Item("EC.Renekton.Combo.Items").GetValue<bool>())
                        {
                            myItemManager.UseItems(2, null);
                        }
                        if (config.Item("EC.Renekton.Combo.W").GetValue<bool>() && Fury && W.IsReady() && !WBuff)
                        {
                           W.Cast();
                        }
                    }
                }
                if (target is Obj_AI_Minion && target.IsValidTarget())
                {
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear)
                    {
                        if (config.Item("EC.Renekton.Farm.W").GetValue<bool>())
                        {
                            if (W.IsKillable((Obj_AI_Minion)target))
                            {
                                W.Cast();
                            }
                        }
                    }
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear)
                    {
                        if (config.Item("EC.Renekton.Jungle.W").GetValue<bool>())
                        {
                            if (W.IsKillable((Obj_AI_Minion)target))
                            {
                                W.Cast();
                            }
                        }
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Renekton.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Renekton.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Renekton.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (config.Item("EC.Renekton.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}