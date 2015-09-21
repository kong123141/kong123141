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
    class Diana : PluginData
    {
        public Diana()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 830);
            W = new Spell(SpellSlot.W, 200);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 825);
           
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
                combomenu.AddItem(new MenuItem("UseR2Combo", "Use R (Second)").SetValue(true));
                combomenu.AddItem(new MenuItem("ComboType", "Combo Type").SetValue(new StringList(new[] { "Standard", "Full Combo" })));
                combomenu.AddItem(new MenuItem("ComboStyle", "(Full Combo) Style").SetValue(new StringList(new[] { "Q-R-W-E", "R-Q-W-E" })));
                combomenu.AddItem(new MenuItem("TurretDive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("UseItemCombo", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("QHarassType", "Q").SetValue(new StringList(new[] { "Single", "something" })));
                harassmenu.AddItem(new MenuItem("QHarassValue", "(something)").SetValue(new Slider(1, 1, 5)));
                config.AddSubMenu(harassmenu);
            }
            var laneclear = new Menu("Farm", "Farm");
            {
                laneclear.AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(false));
                laneclear.AddItem(new MenuItem("UseWFarm", "Use W").SetValue(false));
                laneclear.AddItem(new MenuItem("UseRFarm", "Use R").SetValue(false));
                laneclear.AddItem(new MenuItem("QFarmValue", "Q More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("RFarmType", "R").SetValue(new StringList(new[] { "Any (Q buff)", "Siege", "Siege (Q buff)" })));
                laneclear.AddItem(new MenuItem("FarmMana", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclear);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseRJFarm", "Use R").SetValue(true));
                junglemenu.AddItem(new MenuItem("RJFarmType", "R").SetValue(new StringList(new[] { "Any (Q buff)", "Large", "Large (Q buff)", "Secure" })));
                config.AddSubMenu(junglemenu);
            }  
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("QPredHitchance", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("UseWMisc", "W Block").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseEMisc", "E Interrupts").SetValue(false));
                config.AddSubMenu(miscmenu);
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
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var UseR = config.Item("UseRCombo").GetValue<bool>();
            var UseR2 = config.Item("UseR2Combo").GetValue<bool>();            
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToMagic(target)) return;                
                try
                {
                    switch (config.Item("ComboType").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            if (UseQ && UseW && UseE && UseR)
                            {
                                switch (config.Item("ComboStyle").GetValue<StringList>().SelectedIndex)
                                {
                                    case 0:
                                        if (Q.IsReady())
                                        {
                                            Q.CastIfHitchanceEquals(target, QHitChance); //QPredict(target);
                                        }
                                        if (R.IsReady() && (QCasted || target.HasBuff("dianamoonlight")) && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < R.Range)
                                        {
                                            if (target.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                                            if (target.UnderTurret(true) && config.Item("TurretDive").GetValue<bool>() && myUtility.PlayerHealthPercentage < 25) return;
                                            R.Cast(target);
                                        }
                                        if (W.IsReady() && Orbwalking.InAutoAttackRange(target) && !R.IsReady())
                                        {
                                            W.Cast();
                                        }
                                        if (E.IsReady() && Orbwalking.InAutoAttackRange(target))
                                        {
                                            E.Cast();
                                        }
                                        if (UseR2 && R.IsReady() && !Q.IsReady() && R.IsInRange(target))
                                        {
                                            R.Cast(target);
                                        }
                                        break;
                                    case 1:
                                        if (Q.IsReady() && R.IsReady() && Q.IsInRange(target))
                                        {
                                            if (Q.GetPrediction(target).Hitchance >= QHitChance)
                                            {
                                                R.Cast(target);
                                                Q.CastIfHitchanceEquals(target, QHitChance);
                                            }
                                        }
                                        if (W.IsReady() && Orbwalking.InAutoAttackRange(target) && !R.IsReady())
                                        {
                                            W.Cast();
                                        }
                                        if (E.IsReady() && Orbwalking.InAutoAttackRange(target))
                                        {
                                            E.Cast();
                                        }
                                        if (UseR2 && R.IsReady() && !Q.IsReady() && R.IsInRange(target))
                                        {
                                            R.Cast(target);
                                        }
                                        break;
                                }
                            }
                            break;
                        case 1:

                            if (UseQ && Q.IsReady())
                            {
                                QPredict(target);
                            }
                            if (UseW && W.IsReady() && W.IsInRange(target))
                            {
                                W.Cast();
                            }
                            if (UseE && E.IsReady() && E.IsInRange(target))
                            {
                                E.Cast();
                            }
                            if (R.IsReady())
                            {
                                if (target.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                                if (target.UnderTurret(true) && config.Item("TurretDive").GetValue<bool>() && myUtility.PlayerHealthPercentage < 25) return;
                                R.Cast(target);
                            }
                            break;
                    }
                }
                catch { }
            }
            
        }
        private void Harass()
        {
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            if (!Player.UnderTurret(true))
            {
                if (UseQ && Q.IsReady())
                {
                    switch (config.Item("QHarassType").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                            PredictionOutput pred = Q.GetPrediction(target);
                            if (pred.Hitchance >= QHitChance)
                            {
                                Q.Cast(target.ServerPosition);
                            }
                            break;
                        case 1:
                            var tuple = GetQArcHero();
                            if (tuple.Item1 >= config.Item("QHarassValue").GetValue<Slider>().Value)
                            {
                                Q.Cast(tuple.Item2);
                            }
                            break;
                    }
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;            
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                var tuple = GetQArc();
                if (tuple.Item1 > config.Item("QFarmValue").GetValue<Slider>().Value)
                {
                    Q.Cast(tuple.Item2);
                }
            }
            if (config.Item("UseWFarm").GetValue<bool>() && W.IsReady())
            {
                var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, Player.AttackRange);
                if (allMinionsW.Count >= 3)
                {
                    W.Cast();
                }
            }
            if (config.Item("UseRFarm").GetValue<bool>() && R.IsReady())
            {
                switch (config.Item("RFarmType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        var minionR = MinionManager.GetMinions(Player.ServerPosition, R.Range).Where(x => !x.UnderTurret(true) && x.HasBuff("dianamoonlight") && R.IsKillable(x)).OrderByDescending(i => i.Distance(Player)).FirstOrDefault();
                        R.CastOnUnit(minionR);
                        break;
                    case 1:
                        var siegeQ = myUtility.GetLargeMinions(R.Range).FirstOrDefault(x => !x.UnderTurret(true) && R.IsKillable(x));
                        R.CastOnUnit(siegeQ);
                        break;
                    case 2:
                        var buffedsiegeQ = myUtility.GetLargeMinions(R.Range).FirstOrDefault(x => !x.UnderTurret(true) && R.IsKillable(x) && x.HasBuff("dianamoonlight")); 
                        R.CastOnUnit(buffedsiegeQ);
                        break;
                }
            }            
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myUtility.GetLargeMonsters(R.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob == null) return;

            if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady())
            {
                if (largemobs != null)
                {
                    Q.Cast(largemobs.ServerPosition);
                }
                var mobQ = Q.GetCircularFarmLocation(mobs, Q.Width);
                if (mobQ.MinionsHit > 0)
                {
                    Q.Cast(mobQ.Position);
                }
            }
            if (config.Item("UseWJFarm").GetValue<bool>() && W.IsReady() && (W.IsInRange(largemobs) || W.IsInRange(mob)))
            {
                W.Cast();
            }
            if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && (E.IsInRange(largemobs) || E.IsInRange(mob)))
            {
                E.Cast();
            }
            if (config.Item("UseRJFarm").GetValue<bool>() && R.IsReady())
            {
                switch (config.Item("RJFarmType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        //Any w/ buff
                        var MobQBuff = mobs.FirstOrDefault(x => x.HasBuff("dianamoonlight"));
                        if (MobQBuff.IsValidTarget())
                        {
                            R.CastOnUnit(MobQBuff);
                        }
                        break;
                    case 1:
                        //Large
                        if (largemobs != null)
                        {
                            R.CastOnUnit(largemobs);
                        }
                        break;
                    case 2:
                        //Large w/buff
                        if (largemobs != null)
                        {
                            if (largemobs.HasBuff("dianamoonlight"))
                            {
                                R.CastOnUnit(largemobs);
                            }
                        }
                        break;
                    case 3:
                        //Secure
                        if (largemobs != null)
                        {
                            if (largemobs.HasBuff("dianamoonlight") && R.IsKillable(largemobs))
                            {
                                R.CastOnUnit(largemobs);
                            }
                            else if (!largemobs.HasBuff("dianamoonlight") && R.IsKillable(largemobs))
                            {
                                R.CastOnUnit(largemobs);
                            }
                        }
                        break;
                }
            }
        }

        private bool QCasted;
        private static Obj_SpellMissile QMissile;
        private static HitChance QHitChance
        {
            get
            {
                return GetQHitChance();
            }
        }
        private static HitChance GetQHitChance()
        {
            switch (config.Item("QPredHitchance").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                default:
                    return HitChance.Medium;
            }
        }
        private void QPredict(Obj_AI_Base target)
        {
            PredictionOutput pred = Q.GetPrediction(target);
            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range)
            {
                var test1 = Prediction.GetPrediction(target, Q.Instance.SData.MissileSpeed).CastPosition;
                float movement = target.MoveSpeed * 100 / 1000;
                if (target.Distance(test1) > movement)
                {
                    Q.Cast(target.ServerPosition.Extend(test1, Q.Instance.SData.MissileSpeed * target.MoveSpeed));
                }
                else
                {
                    if (pred.Hitchance >= QHitChance)
                    {
                        Q.Cast(pred.CastPosition);
                    }
                }
            }
        }

        private int GetQHits(Obj_AI_Base target)
        {
            var laneMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
            return laneMinions.Count(minion => Vector3.Distance(minion.ServerPosition, target.ServerPosition) <= 200);
        }
        private int GetQHitsHero(Obj_AI_Base target)
        {
            var GetEnemysHit = ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && !enemy.IsDead && enemy.IsValidTarget()).ToList();
            return GetEnemysHit.Count(enemy => Vector3.Distance(enemy.ServerPosition, target.ServerPosition) <= 200);
        }
        private Tuple<int, Vector3> GetQArc()
        {
            Tuple<int, Vector3> bestSoFar = Tuple.Create(0, Player.Position);
            var laneMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
            laneMinions.Reverse();
            foreach (var minion in laneMinions)
            {
                var hitCount = GetQHits(minion);
                if (hitCount > bestSoFar.Item1)
                {
                    bestSoFar = Tuple.Create(hitCount, minion.ServerPosition);
                }
            }
            return bestSoFar;
        }
        private Tuple<int, Vector3> GetQArcHero()
        {
            List<Obj_AI_Hero> AllEnemy = ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && !enemy.IsDead && enemy.IsValidTarget()).Reverse().ToList();
            Tuple<int, Vector3> bestSoFar = Tuple.Create(0, Player.ServerPosition);
            foreach (var enemy in AllEnemy)
            {
                var hitCount = GetQHitsHero(enemy);
                if (hitCount > bestSoFar.Item1)
                {
                    bestSoFar = Tuple.Create(hitCount, enemy.ServerPosition);
                }
            }
            return bestSoFar;
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
        protected override void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender is Obj_SpellMissile && sender.IsValid)
            {
                var missile = (Obj_SpellMissile)sender;
                if (missile.SpellCaster.Name == Player.Name && (missile.SData.Name == "dianaarcthrow"))
                {
                    QMissile = missile;
                    QCasted = true;
                }
            }
        }
        protected override void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender is Obj_SpellMissile && sender.IsValid)
            {
                var missile = (Obj_SpellMissile)sender;
                if (missile.SpellCaster.Name == Player.Name && (missile.SData.Name == "dianaarcthrow"))
                {
                    QMissile = null;
                    QCasted = false;
                }
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit != null && !unit.IsMe && spell.Target.IsMe && unit.IsChampion(unit.BaseSkinName) && !spell.SData.IsAutoAttack())
            {
                if (config.Item("UseWMisc").GetValue<bool>() && W.IsReady())
                {
                    Utility.DelayAction.Add(100, () => W.Cast());
                }
            }
            if (unit != null && unit.IsMe)
            {
                if (spell.SData.Name.ToLower() == "dianateleport")
                {
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && config.Item("UseWCombo").GetValue<bool>() && W.IsReady())
                    {
                        W.Cast();
                    }
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("UseEMisc").GetValue<bool>() && E.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) < E.Range)
                {
                    if (myUtility.ImmuneToCC(sender)) return;
                    E.Cast();
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
