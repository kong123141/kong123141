using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
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
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E (Slice)").SetValue(true));
                combomenu.AddItem(new MenuItem("UseE2Combo", "Use E (Dice)").SetValue(true));
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
                laneclear.AddItem(new MenuItem("UseWFarm", "Use W").SetValue(true));
                laneclear.AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
                laneclear.AddItem(new MenuItem("QFarmValue", "Q More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("EFarmValue", "E More Than").SetValue(new Slider(1, 1, 5)));
                config.AddSubMenu(laneclear);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
                config.AddSubMenu(junglemenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawW", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawE", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawR", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(W.Range + E.Range, TargetSelector.DamageType.Physical);  
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var UseE2 = config.Item("UseE2Combo").GetValue<bool>();       
            var UseR = config.Item("UseRCombo").GetValue<bool>();
            var CastItems = config.Item("UseItemCombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToPhysical(target)) return;
                if (CastItems) { myUtility.UseItems(0, target); }
                try
                {
                    if (UseW && UseE && W.IsReady() && E.IsReady())
                    {
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) > Q.Range &&
                            Vector3.Distance(Player.ServerPosition, target.ServerPosition) < W.Range + E.Range)
                        {
                            if (target.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                            W.Cast();
                            E.Cast(target.ServerPosition);
                        }
                    }
                    if (UseQ && Q.IsReady() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range)
                    {
                        if (Orbwalking.InAutoAttackRange(target) && WBuff) return;
                        Q.Cast();
                    }
                    if (UseW && W.IsReady() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < W.Range)
                    {
                        W.Cast();
                    } 
                    if (UseE && E.IsReady() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= E.Range)
                    {
                        if (target.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                        if (!CanDice)
                        {
                            if (Orbwalking.InAutoAttackRange(target) && WBuff) return;
                            E.Cast(target.ServerPosition);
                        }
                        if (UseE2 && CanDice)
                        {
                            if (Orbwalking.InAutoAttackRange(target) && WBuff) return;
                            E.Cast(target.ServerPosition);
                        }
                    }
                    if (UseR && R.IsReady())
                    {
                        if (Player.CountEnemiesInRange(R.Range * 1.75f) > 1) R.Cast();
                        else if (myUtility.PlayerHealthPercentage < 50) R.Cast();
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
                catch
                {
                }
            }
        }
        private void Harass()
        {
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var UseW = config.Item("UseWHarass").GetValue<bool>();
            var UseE = config.Item("UseEHarass").GetValue<bool>();
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
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && (!myOrbwalker.IsWaiting() || Fury) && !Player.IsWindingUp)
            {
                var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (allMinionsQ == null) return;
                var siegeQ = myUtility.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x) && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < Q.Range);
                if (siegeQ != null && siegeQ.IsValidTarget())
                {
                    Q.Cast();
                }
                if (allMinionsQ.Count > config.Item("QFarmValue").GetValue<Slider>().Value)
                {
                    if (Player.UnderTurret(true)) return;
                    Q.Cast();
                }
            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && (!myOrbwalker.IsWaiting() || !Fury) && !Player.IsWindingUp)
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
                    if (ELine.MinionsHit > config.Item("EFarmValue").GetValue<Slider>().Value)
                    {
                        E.Cast(ELine.Position);
                    }
                }
            }
        }        
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range + E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myUtility.GetLargeMonsters(Q.Range + E.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
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
                if (config.Item("UseWJFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && Orbwalking.InAutoAttackRange(largemobs))
                    {
                        W.Cast();
                    }
                    W.Cast();
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
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
            if (config.Item("UseWFarm").GetValue<bool>() && W.IsReady() && !Fury)
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
                    myUtility.UseItems(2, null);
                }
            } 
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe && W.IsReady() && !WBuff)
            {
                if (target is Obj_AI_Hero && target.IsValidTarget())
                {
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                    {
                        if (config.Item("UseItemCombo").GetValue<bool>() && !Player.IsWindingUp)
                        {
                            myUtility.UseItems(2, null);
                        }
                        if (config.Item("UseWCombo").GetValue<bool>() && !Player.IsWindingUp && Fury)
                        {
                           W.Cast();
                        }
                    }
                }
                if (target is Obj_AI_Minion && target.IsValidTarget())
                {
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear)
                    {
                        if (config.Item("UseWFarm").GetValue<bool>())
                        {
                            if (W.IsKillable((Obj_AI_Minion)target))
                            {
                                W.Cast();
                            }
                        }
                    }
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear)
                    {
                        if (config.Item("UseWJFarm").GetValue<bool>())
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
            if (config.Item("DrawQ").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.White);
            }
            if (config.Item("DrawW").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Color.White);
            }
            if (config.Item("DrawE").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.White);
            }
            if (config.Item("DrawR").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, Color.White);
            }
        }
    }
}