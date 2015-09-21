using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
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
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 175);

            E.SetSkillshot(E.Instance.SData.SpellCastTime, E.Instance.SData.LineWidth, E.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotCircle);

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
                laneclear.AddItem(new MenuItem("UseQLastHit", "(Last Hit) Use Q").SetValue(true));
                laneclear.AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
                laneclear.AddItem(new MenuItem("EFarmValue", "E More Than").SetValue(new Slider(1, 1, 5)));
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
                miscmenu.AddItem(new MenuItem("UseQMisc", "Q Turrets").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseWMisc", "W Gapcloser").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawW", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawE", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawR", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
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
                    if (UseQ && Q.IsReady() && Orbwalking.InAutoAttackRange(target))
                    {
                        if (myUtility.ImmuneToMagic(target)) return;
                        Q.Cast();
                    }
                    if (UseW && W.IsReady() && W.IsInRange(target))
                    {
                        if (myUtility.ImmuneToMagic(target)) return;
                        W.CastOnUnit(target);
                    }
                    if (UseE && E.IsReady())
                    {
                        if (Player.UnderTurret(true) && target.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                        EPredict(target);
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
                catch { }
            }
            
        }
        private void Harass()
        {
            var UseW = config.Item("UseWHarass").GetValue<bool>();
            var UseE = config.Item("UseEHarass").GetValue<bool>();
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
                    EPredict(target);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp && myOrbwalker.IsWaiting())
            {
                Q.Cast();
            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
            {
                if (Player.UnderTurret(true)) return;
                var minionE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (minionE == null) return;
                var epred = E.GetCircularFarmLocation(minionE);
                if (epred.MinionsHit > config.Item("EFarmValue").GetValue<Slider>().Value)
                {
                    E.Cast(epred.Position);
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
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && Orbwalking.InAutoAttackRange(mob))
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
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady())
                {
                    List<Obj_AI_Base> MobsE = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                    MinionManager.FarmLocation ECircular = E.GetCircularFarmLocation(MobsE);
                    if (ECircular.MinionsHit > 0)
                    {
                        E.Cast(ECircular.Position.To3D().Shorten(Player.ServerPosition, 50f));
                    }
                }
            }
        }
        private void LastHit()
        {
            if (myOrbwalker.IsWaiting() && !Player.IsWindingUp && config.Item("UseQLastHit").GetValue<bool>() && Q.IsReady())
            {
                Q.Cast();
            }
        }
      
        private void EPredict(Obj_AI_Base target)
        {
            var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where champ.IsValidTarget(E.Range) && target != champ select champ).ToList();
            if (nearChamps.Count > 0)
            {
                var closeToPrediction = new List<Obj_AI_Hero>();
                foreach (var enemy in nearChamps)
                {
                    PredictionOutput prediction = E.GetPrediction(enemy);
                    if (prediction.Hitchance >= HitChance.Medium && Vector3.Distance(Player.ServerPosition, enemy.ServerPosition) < 400f)
                    {
                        closeToPrediction.Add(enemy);
                    }
                }
                if (closeToPrediction.Count == 0)
                {
                    PredictionOutput pred = E.GetPrediction(target);
                    if (pred.Hitchance >= HitChance.High && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < E.Range)
                    {
                        E.Cast(pred.CastPosition.Extend(Player.ServerPosition, -100f));
                    }
                }
                else if (closeToPrediction.Count > 0)
                {
                    E.CastIfWillHit(target, closeToPrediction.Count, false);
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
                case myOrbwalker.OrbwalkingMode.Lasthit:
                    LastHit();
                    break;
            }
        }
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && (myUtility.PlayerManaPercentage > config.Item("FarmMana").GetValue<Slider>().Value))
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
            if (config.Item("UseWMisc").GetValue<bool>() && W.IsReady())
            {
                if (Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) <= W.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    W.CastOnUnit(gapcloser.Sender);
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
                    if (!Player.IsWindingUp && config.Item("UseItemCombo").GetValue<bool>() && Orbwalking.InAutoAttackRange(target))
                    {
                        myUtility.UseItems(2, null);
                    }
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear)
                {
                    if (target is Obj_AI_Minion && target.Team == GameObjectTeam.Neutral && !target.Name.Contains("Mini") &&
                        !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                    {
                        myUtility.UseItems(2, null);
                    }
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear)
                {
                    if (target is Obj_AI_Turret && target.Team != Player.Team &&
                        config.Item("UseQMisc").GetValue<bool>() &&
                        !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                    {
                        Q.Cast();
                    }
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
