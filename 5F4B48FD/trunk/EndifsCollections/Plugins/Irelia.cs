using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Irelia : PluginData
    {
        public Irelia()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 425);
            R = new Spell(SpellSlot.R, 1000);

            R.SetSkillshot(R.Instance.SData.SpellCastTime, 150f, float.MaxValue, false, SkillshotType.SkillshotCircle);        

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
                laneclear.AddItem(new MenuItem("QFarmDelay", "Q Delay").SetValue(new Slider(500, 0, 1000)));
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
                miscmenu.AddItem(new MenuItem("UseEMisc", "E Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseE2Misc", "E Gapcloser").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawE", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawR", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
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
                    if (UseQ && Q.IsReady() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= Q.Range)
                    {
                        if (target.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                        if (myUtility.ImmuneToMagic(target)) return;
                        if (Q.IsKillable(target)) Q.Cast(target);
                        else if (!Orbwalking.InAutoAttackRange(target))
                        {
                            var dist = Vector3.Distance(Player.ServerPosition, target.ServerPosition);
                            var msDif = Player.MoveSpeed - target.MoveSpeed;
                            var reachIn = dist / msDif;
                            if (msDif < 0)
                            {
                                Q.Cast(target);
                            }
                            else if (reachIn > 1)
                            {
                                Q.Cast(target);
                            }
                        }
                    }
                    if (UseW && W.IsReady() && Orbwalking.InAutoAttackRange(target))
                    {
                        W.Cast();
                    }
                    if (UseE && E.IsReady() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= E.Range)
                    {
                        if (myUtility.ImmuneToCC(target) || myUtility.ImmuneToMagic(target)) return;
                        if (myUtility.PlayerHealthPercentage < (target.Health * 100 / target.MaxHealth)) E.Cast(target);
                        else
                        {
                            if (target.MoveSpeed >= Player.MoveSpeed || 
                                (myUtility.IsFacing(Player, target.ServerPosition) && !myUtility.IsFacing(target, Player.ServerPosition)))
                            {
                                E.Cast(target);
                            }                            
                        }
                    }
                    if (UseR && R.IsReady())
                    {
                        if (myUtility.MovementImpaired(target))
                        {
                            R.Cast(target.Position);
                        }
                        else
                        {
                            Vector3 pos;
                            PredictionOutput pred = R.GetPrediction(target);
                            var test1 = Prediction.GetPrediction(target, R.Instance.SData.MissileSpeed).CastPosition;
                            float movement = target.MoveSpeed * 100 / 1000;
                            if (target.Distance(test1) > movement)
                            {
                                pos = target.ServerPosition.Extend(test1, R.Instance.SData.MissileSpeed * target.MoveSpeed);
                                R.Cast(pos);
                            }
                            else
                            {
                                if (pred.Hitchance >= HitChance.High)
                                {
                                    R.Cast(pred.CastPosition);
                                }
                            }
                        }
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
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var UseW = config.Item("UseWHarass").GetValue<bool>();
            var UseE = config.Item("UseEHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady() && Q.IsInRange(target))
                {
                    if (target.UnderTurret(true)) return;
                    Q.Cast(target);
                }
                if (UseW && W.IsReady() && Orbwalking.InAutoAttackRange(target))
                {
                    W.Cast();
                }
                if (UseE && E.IsReady() && E.IsInRange(target))
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    E.Cast(target);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && myUtility.TickCount - LastQ > config.Item("QFarmDelay").GetValue<Slider>().Value && !Player.IsWindingUp)
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (minionQ == null) return;
                var siegeQ = myUtility.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                if (siegeQ != null && siegeQ.IsValidTarget())
                {
                    Q.Cast(siegeQ);
                }
                else
                {
                    var AnyQ = minionQ.Where(x => Q.IsKillable(x) &&(Player.GetAutoAttackDamage(x) < x.Health && !Orbwalking.InAutoAttackRange(x))).OrderBy(i => i.Distance(Player)).FirstOrDefault();
                    if (AnyQ != null && AnyQ.IsValidTarget())
                    {
                        if (myUtility.IsFacing(Player, AnyQ.ServerPosition)) Q.Cast(AnyQ);
                    }
                }                
            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
            {                
                var minionE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (minionE == null) return;
                var siegeE = myUtility.GetLargeMinions(E.Range).FirstOrDefault(x => E.IsKillable(x));
                if (siegeE != null && siegeE.IsValidTarget())
                {
                    E.Cast(siegeE);
                }
                else
                {
                    var AnyE = minionE.FirstOrDefault(x => E.IsKillable(x) && Player.GetAutoAttackDamage(x) < x.Health);
                    if (AnyE != null && AnyE.IsValidTarget())
                    {
                       E.Cast(AnyE);
                    }
                }
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myUtility.GetLargeMonsters(Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && Q.IsInRange(mob))
                {
                    if (largemobs != null)
                    {
                        Q.Cast(largemobs);
                    }
                    else
                    {
                        Q.Cast(mob);
                    }
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && E.IsInRange(mob))
                {
                    if (largemobs != null)
                    {
                        E.Cast(largemobs);
                    }
                    else
                    {
                       E.Cast(mob);
                    }
                }
            }
        }

        private int LastQ;

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
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if (spell.SData.Name.ToLower() == "ireliagatotsu" && myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear)
                {
                    LastQ = myUtility.TickCount;
                }            
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("UseEMisc").GetValue<bool>() && E.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.High)
            {
                if (Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= E.Range && myUtility.PlayerHealthPercentage < (sender.Health * 100 / sender.MaxHealth))
                {
                    W.Cast(sender);
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("UseE2Misc").GetValue<bool>() && E.IsReady())
            {
                if (Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) <= E.Range && myUtility.PlayerHealthPercentage < (gapcloser.Sender.Health * 100 / gapcloser.Sender.MaxHealth))
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    E.Cast(gapcloser.Sender.ServerPosition);
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
