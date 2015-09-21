using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
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
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
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
                laneclear.AddItem(new MenuItem("UseWFarm", "Use W").SetValue(true));
                laneclear.AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
                laneclear.AddItem(new MenuItem("EFarmValue", "E More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("FarmMana", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclear);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("UseEMisc", "E Gapcloser").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
        }
        
        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(W.Range * 2, TargetSelector.DamageType.Physical);      
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
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
                    if (UseQ && Q.IsReady() && !Player.IsWindingUp)
                    {
                        var dist = Vector3.Distance(Player.ServerPosition, target.ServerPosition);
                        var msDif = Player.MoveSpeed - target.MoveSpeed;
                        var reachIn = dist / msDif;
                        if (msDif < 0 && reachIn >= 3)
                        {
                            Q.Cast();
                        }
                        else if (msDif > 0)
                        {
                            if (myUtility.ImmuneToCC(target)) return;
                            Q.Cast();
                        }
                    }
                    if (UseW && W.IsReady() && !Player.HasBuff("VolibearQ") && !Player.IsWindingUp)
                    {
                        W.Cast(target);
                    }
                    if (UseE && E.IsReady() && !Player.HasBuff("VolibearQ") && !Player.IsWindingUp)
                    {
                        if (myUtility.ImmuneToCC(target) || myUtility.ImmuneToMagic(target)) return;
                        if (Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) < 400f)
                        {
                            E.Cast();
                        }
                    }
                    if (UseR && R.IsReady() && Player.HasBuff("volibearwparticle") && !Player.HasBuff("VolibearQ"))
                    {
                        if (myUtility.ImmuneToMagic(target)) return;
                        if (target.CountEnemiesInRange(300f) >= 1) R.Cast();
                    }
                    if (CastItems)
                    {
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
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical, false);
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var UseW = config.Item("UseWHarass").GetValue<bool>();
            var UseE = config.Item("UseEHarass").GetValue<bool>();
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
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (config.Item("UseWFarm").GetValue<bool>() && !Player.IsWindingUp && Player.HasBuff("volibearwparticle"))
            {
                var minionW = MinionManager.GetMinions(Player.ServerPosition, W.Range);
                if (minionW == null) return;
                var siegew = myUtility.GetLargeMinions(W.Range).FirstOrDefault(x => W.IsKillable(x));
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
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady())
            {
                var minionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None);
                if (minionsE.Count > config.Item("EFarmValue").GetValue<Slider>().Value && myOrbwalker.IsWaiting() && !Player.IsWindingUp)
                {
                    E.Cast();
                }
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myUtility.GetLargeMonsters(E.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null && !Player.IsWindingUp)
            {
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady())
                {
                    if (largemobs != null && Orbwalking.InAutoAttackRange(largemobs))
                    {
                        Q.Cast();
                    }
                }
                if (config.Item("UseWJFarm").GetValue<bool>() && W.IsReady())
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
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady())
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
            if (config.Item("UseWFarm").GetValue<bool>() && W.IsReady() && (myUtility.PlayerManaPercentage > config.Item("FarmMana").GetValue<Slider>().Value))
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
            if (config.Item("UseEMisc").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) < E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    E.Cast();
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
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
