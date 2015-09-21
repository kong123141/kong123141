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
    class Volibear : PluginData
    {
        public Volibear()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 400);
            E = new Spell(SpellSlot.E, 425);
            R = new Spell(SpellSlot.R, 300);
      
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Volibear.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Volibear.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Volibear.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Volibear.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Volibear.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Volibear.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Volibear.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Volibear.Harass.E", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Volibear.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Volibear.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Volibear.Farm.E.Value", "E More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Volibear.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Volibear.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Volibear.Jungle.W", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Volibear.Jungle.E", "Use E").SetValue(true));
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Volibear.Misc.E", "E Gapcloser").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(W.Range * 2, TargetSelector.DamageType.Physical);      

            var UseQ = config.Item("EC.Volibear.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Volibear.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Volibear.Combo.E").GetValue<bool>();
            var UseR = config.Item("EC.Volibear.Combo.R").GetValue<bool>();
            var CastItems = config.Item("EC.Volibear.Combo.Items").GetValue<bool>();
            if (UseE && E.IsReady())
            {
                if (Player.CountEnemiesInRange(400) > 0)
                {
                    E.Cast();
                }
            }
            if (UseR && R.IsReady() && !Player.HasBuff("VolibearQ"))
            {
                if (Player.CountEnemiesInRange(400) > 1 && Player.Spellbook.IsAutoAttacking)
                {
                    R.Cast();
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;                
                if (CastItems) { myItemManager.UseItems(0, Target); }
                try
                {
                    if (UseQ && Q.IsReady() && !Player.IsWindingUp)
                    {
                        var dist = Vector3.Distance(Player.ServerPosition, Target.ServerPosition);
                        var msDif = Player.MoveSpeed - Target.MoveSpeed;
                        var reachIn = dist / msDif;
                        if (msDif < 0 && reachIn >= 3)
                        {
                            Q.Cast();
                        }
                        else if (msDif > 0)
                        {
                            if (myUtility.ImmuneToCC(Target)) return;
                            Q.Cast();
                        }
                    }
                    if (UseW && W.IsReady() && !Player.HasBuff("VolibearQ"))
                    {
                        W.Cast(Target);
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
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical, false);
            var UseQ = config.Item("EC.Volibear.Harass.Q").GetValue<bool>();
            var UseW = config.Item("EC.Volibear.Harass.W").GetValue<bool>();
            var UseE = config.Item("EC.Volibear.Harass.E").GetValue<bool>();
            if (target.IsValidTarget() && !myOrbwalker.IsWaiting() && !Player.IsWindingUp)
            {
                if (UseQ && Q.IsReady() && !target.UnderTurret(true))
                {
                    var dist = Vector3.Distance(Player.ServerPosition, target.ServerPosition);
                    var msDif = Player.MoveSpeed - target.MoveSpeed;
                    var reachIn = dist / msDif;
                    if (msDif < 0 && reachIn > 0)
                    {
                        Q.Cast();
                    }
                    else if (msDif > 0 && !Orbwalking.InAutoAttackRange(target))
                    {
                        Q.Cast();
                    }
                }
                if (UseW && W.IsReady() && !Player.HasBuff("VolibearQ"))
                {
                    W.Cast(target);
                }
                if (UseE && E.IsReady() && !Player.HasBuff("VolibearQ"))
                {
                    if (Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) < 300f)
                    {
                        E.Cast();
                    }
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("EC.Volibear.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (config.Item("EC.Volibear.Farm.W").GetValue<bool>() && !Player.IsWindingUp && Player.HasBuff("volibearwparticle"))
            {
                var minionW = MinionManager.GetMinions(Player.ServerPosition, W.Range);
                if (minionW == null) return;
                var siegew = myFarmManager.GetLargeMinions(W.Range).FirstOrDefault(x => W.IsKillable(x));
                var meleeW = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
                var mW = meleeW.Where(x => W.IsKillable(x) && Player.GetAutoAttackDamage(x) < x.Health).OrderByDescending(i => i.Health).FirstOrDefault();
                if (siegew != null && siegew.IsValidTarget())
                {
                    W.Cast(siegew);
                }
                else if (mW != null && mW.IsValidTarget())
                {
                    W.Cast(mW);
                }
                else
                {
                    var anyW = minionW.OrderByDescending(i => i.Health).FirstOrDefault(x => W.IsKillable(x));
                    if (anyW != null && mW.IsValidTarget())
                    {
                        W.Cast(mW);
                    }
                }
            }
            if (config.Item("EC.Volibear.Farm.E").GetValue<bool>() && E.IsReady())
            {
                var minionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None);
                if (minionsE.Count > config.Item("EC.Volibear.Farm.E.Value").GetValue<Slider>().Value && myOrbwalker.IsWaiting() && !Player.IsWindingUp)
                {
                    E.Cast();
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
                if (config.Item("EC.Volibear.Jungle.Q").GetValue<bool>() && Q.IsReady())
                {
                    if (largemobs != null && Orbwalking.InAutoAttackRange(largemobs))
                    {
                        Q.Cast();
                    }
                }
                if (config.Item("EC.Volibear.Jungle.W").GetValue<bool>() && W.IsReady())
                {
                    if (largemobs != null && W.IsKillable(largemobs) && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < W.Range)
                    {
                        W.Cast(largemobs);
                    }
                    else if (W.IsKillable(mob))
                    {
                        W.Cast(mob);
                    }
                }
                if (config.Item("EC.Volibear.Jungle.E").GetValue<bool>() && E.IsReady())
                {
                    if (largemobs != null && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < E.Range)
                    {
                        E.Cast();
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
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (config.Item("EC.Volibear.Farm.W").GetValue<bool>() && W.IsReady() && (myUtility.PlayerManaPercentage > config.Item("EC.Volibear.Farm.ManaPercent").GetValue<Slider>().Value))
            {
                var target = minion as Obj_AI_Base;
                if (target != null &&
                    W.IsKillable(target) &&
                    Orbwalking.InAutoAttackRange(target))
                {
                    W.Cast(target);
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("EC.Volibear.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.Cast());
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Volibear.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Volibear.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (config.Item("EC.Volibear.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.White);
            }
        }
    }
}
