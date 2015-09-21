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
    class Alistar : PluginData
    {
        public Alistar()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 365);
            //W = new Spell(SpellSlot.W, 650);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 575);
            R = new Spell(SpellSlot.R);

            W.SetTargetted(0.5f, float.MaxValue);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            myDamageIndicator.DamageToUnit = GetDamage;
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Alistar.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Alistar.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Alistar.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Alistar.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Alistar.Combo.Dive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Alistar.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Alistar.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Alistar.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Alistar.Harass.E", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Alistar.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Alistar.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Alistar.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Alistar.Farm.Q.Value", "Q More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Alistar.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Alistar.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Alistar.Jungle.W", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Alistar.Jungle.E", "Use E").SetValue(true));
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Alistar.Misc.Q", "Q Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Alistar.Misc.W", "W Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Alistar.Misc.Q2", "Q Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Alistar.Misc.W2", "W Interrupts").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Alistar.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Alistar.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Alistar.Draw.E", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(W.Range, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Alistar.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Alistar.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Alistar.Combo.E").GetValue<bool>();
            var UseR = config.Item("EC.Alistar.Combo.R").GetValue<bool>();
            var CastItems = config.Item("EC.Alistar.Combo.Items").GetValue<bool>();
            if (UseE && E.IsReady())
            {
                if (myUtility.PlayerHealthPercentage < 75)
                {
                    E.Cast();
                }
                else
                {
                    var Allies = HeroManager.Allies.Where(
                        x =>
                            Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= E.Range &&
                            x.HealthPercent < 75)
                            .OrderBy(i => i.Health);
                    if (Allies.Any())
                    {
                        E.Cast();
                    }
                }
            }
            if (UseR && R.IsReady())
            {
                if (Player.CountEnemiesInRange(200) > 1 && myUtility.MovementDisabled(Player)) R.Cast();
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;                               
                if (CastItems) { myItemManager.UseItems(0, Target); }
                try
                {
                    if (UseQ && UseW && Player.Mana > (Q.Instance.ManaCost + W.Instance.ManaCost))
                    {
                        if (Target.UnderTurret(true) && !config.Item("EC.Alistar.Combo.Dive").GetValue<bool>()) return;
                        if (myUtility.ImmuneToCC(Target) || myUtility.ImmuneToMagic(Target)) return;
                        if (W.IsReady())
                        {
                            W.Cast(Target);
                        }
                        if (Q.IsReady())
                        {
                            if (Player.IsDashing())
                            {
                                var x = (int)(Vector3.Distance(Player.ServerPosition, Target.ServerPosition) / 3.5f);                              
                                Utility.DelayAction.Add(x, () =>
                                {
                                    Q.Cast();
                                });
                            }
                            else
                            {
                                if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= Q.Range * 2 / 3)
                                {

                                    Q.Cast();
                                }
                            }
                        }
                    }
                    else
                    {
                        if (UseQ && Q.IsReady())
                        {
                            if (myUtility.ImmuneToCC(Target) || myUtility.ImmuneToMagic(Target)) return;
                            if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= Q.Range * 2/3) Q.Cast();
                        }
                        if (UseW && W.IsReady())
                        {
                            if (Target.UnderTurret(true) && !config.Item("EC.Alistar.Combo.Dive").GetValue<bool>()) return;
                            if (myUtility.ImmuneToCC(Target) || myUtility.ImmuneToMagic(Target)) return;
                            W.Cast(Target);
                        }
                    }
                    
                    if (CastItems)
                    {
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
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical, false);            
            var UseQ = config.Item("EC.Alistar.Harass.Q").GetValue<bool>();
            var UseW = config.Item("EC.Alistar.Harass.W").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady())
                {
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= Q.Range) Q.Cast();
                }
                if (UseW && W.IsReady())
                {
                    W.Cast(target);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("EC.Alistar.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (config.Item("EC.Alistar.Farm.Q").GetValue<bool>() && Q.IsReady())
            {
                if (Player.UnderTurret(true)) return;
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (minionQ == null) return;
                var qpred = Q.GetCircularFarmLocation(minionQ);
                if (qpred.MinionsHit > config.Item("EC.Alistar.Farm.Q.Value").GetValue<Slider>().Value)
                {
                    if (Vector3.Distance(Player.ServerPosition,qpred.Position.To3D()) <= Q.Range)
                    {
                        Q.Cast();
                    }
                }
            }
            if (config.Item("EC.Alistar.Farm.W").GetValue<bool>() && W.IsReady())
            {
                var siegeW = myFarmManager.GetLargeMinions(W.Range).FirstOrDefault(x => !Orbwalking.InAutoAttackRange(x) && W.IsKillable(x));
                if (siegeW != null && siegeW.IsValidTarget())
                {
                    W.Cast(siegeW);
                }
            }
            if (config.Item("EC.Alistar.Farm.E").GetValue<bool>() && E.IsReady())
            {                
                var minionE = MinionManager.GetMinions(Player.ServerPosition, E.Range).Where(x => x.HealthPercent < 50).ToList();
                if (minionE.Any())
                {
                    if (minionE.Count() >= 3) E.Cast();
                }

            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (config.Item("EC.Alistar.Jungle.Q").GetValue<bool>() && Q.IsReady())
                {
                    if (largemobs != null && largemobs.IsValidTarget() && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < Q.Range)
                    {
                        Q.Cast();
                    }
                }
                if (config.Item("EC.Alistar.Jungle.W").GetValue<bool>() && W.IsReady())
                {
                    if (largemobs != null && largemobs.IsValidTarget() && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) <= W.Range)
                    {
                        if (W.IsKillable(largemobs)) W.Cast(largemobs);
                    }
                }
                if (config.Item("EC.Alistar.Jungle.E").GetValue<bool>() && E.IsReady())
                {
                    if (mobs.Count() > 1 || myUtility.PlayerHealthPercentage < 75)
                    {
                        E.Cast();
                    }
                }
            }            
        }
        
        private float GetDamage(Obj_AI_Hero target)
        {
            var damage = 0d;
            if (Q.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.Q);
            }
            if (W.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.W);
            }
            return (float)damage;
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
            if (config.Item("EC.Alistar.Misc.Q").GetValue<bool>() && Q.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= Q.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => Q.Cast());
                }
            }
            if (config.Item("EC.Alistar.Misc.W").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) <= W.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast(gapcloser.Sender));    
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (sender.IsEnemy)
            {
                if (myUtility.ImmuneToMagic(sender) || myUtility.ImmuneToCC(sender)) return;
                if (config.Item("EC.Alistar.Misc.Q2").GetValue<bool>() && Q.IsReady())
                {
                    if (Vector3.Distance(Player.ServerPosition, sender.ServerPosition) < Q.Range)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => Q.Cast());                        
                    }
                }
                if (config.Item("EC.Alistar.Misc.W2").GetValue<bool>() && W.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.High)
                {
                    if (Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= W.Range)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast(sender));                        
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Alistar.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Alistar.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Alistar.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
        }
    }
}
