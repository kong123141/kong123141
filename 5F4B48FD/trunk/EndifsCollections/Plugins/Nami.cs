using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Nami : PluginData
    {
        public Nami()
        {
            LoadSpells();
            LoadMenus();
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 875);
            W = new Spell(SpellSlot.W, 725);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 2750);

            Q.SetSkillshot(1f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 260f, 850f, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Tidal Wave", "Custom");
            {
                custommenu.AddItem(new MenuItem("UseRKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("UseRDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("UseRDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
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
                laneclear.AddItem(new MenuItem("QFarmValue", "Q More Than").SetValue(new Slider(1, 1, 5)));
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
                miscmenu.AddItem(new MenuItem("QPredHitchance", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("UseQMisc", "Q Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseQ2Misc", "Q Gapcloser").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawW", "W").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToMagic(target)) return;
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        if (myUtility.ImmuneToCC(target)) return;
                        QPrediction(target);
                    }
                    if (UseW && W.IsReady())
                    {
                        if (Vector3.Distance(target.ServerPosition, Player.ServerPosition) <= W.Range)
                        {
                            W.Cast(target);
                        }
                        else
                        {
                            var WAllies = HeroManager.Allies.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= W.Range);
                            foreach (var wbounce in WAllies)
                            {
                                if (W.IsInRange(wbounce) &&
                                    HeroManager.Enemies.Any(
                                    x =>
                                        Vector3.Distance(wbounce.ServerPosition, x.ServerPosition) <= W.Range))
                                {
                                    W.Cast(wbounce);
                                }
                            }
                        }
                    }
                    if (UseE && E.IsReady())
                    {
                        if (Orbwalking.InAutoAttackRange(target))
                        {
                            E.CastOnUnit(Player);
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
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady())
                {
                    if (myUtility.ImmuneToCC(target) || (Player.UnderTurret(true) && target.UnderTurret(true))) return;
                    if (myUtility.IsFacing(Player, target.ServerPosition, 60))
                    {
                        QPrediction(target);
                    }
                }
                if (UseW && W.IsReady())
                {
                    if (Vector3.Distance(target.ServerPosition, Player.ServerPosition) < W.Range)
                    {
                        if (target.UnderTurret(true) && Player.UnderTurret(true)) return;
                        if (myUtility.IsFacing(Player, target.ServerPosition, 60))
                        {
                            W.Cast(target);
                        } 
                    }
                    else
                    {
                        var WAllies = HeroManager.Allies.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= W.Range);
                        foreach (var wbounce in WAllies)
                        {
                            if (W.IsInRange(wbounce) &&
                                HeroManager.Enemies.Any(
                                x =>
                                    Vector3.Distance(wbounce.ServerPosition, x.ServerPosition) < W.Range))
                            {
                                if (Player.UnderTurret(true)) return;
                                W.Cast(wbounce);
                            }
                        }
                    }
                }
                if (UseE && E.IsReady())
                {
                    if (myUtility.IsFacing(Player, target.ServerPosition) && Orbwalking.InAutoAttackRange(target))
                    {
                        E.CastOnUnit(Player);
                    }
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
            if (allMinionsQ == null) return;
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
            {                                
                foreach (var x in allMinionsQ)
                {
                    if (Q.IsInRange(x) && MinionManager.GetMinions(x.ServerPosition, Q.Width).Count() > config.Item("QFarmValue").GetValue<Slider>().Value)
                    {
                        if (myUtility.IsFacing(Player, x.ServerPosition)) Q.CastOnUnit(x);
                    }
                }
            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
            {
                if (allMinionsQ.Count() > config.Item("EFarmValue").GetValue<Slider>().Value)
                {
                    E.CastOnUnit(Player);
                }
            }            
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myUtility.GetLargeMonsters(Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null && !Player.IsWindingUp)
            {
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady())
                {
                    if (largemobs != null)
                    {
                        Q.Cast(largemobs.ServerPosition);
                    }
                    else
                    {
                        if (myUtility.IsFacing(Player, mob.ServerPosition)) Q.Cast(mob.ServerPosition);
                    }
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady())
                {
                    if (largemobs != null)
                    {
                        E.CastOnUnit(Player);
                    }                   
                }
            }
        }
        private void Custom()
        {
            if (R.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? 
                    TargetSelector.GetSelectedTarget() : 
                    EnemyList.Where(x => !x.InFountain() && x.IsVisible && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < 1000).OrderByDescending(i => i.CountEnemiesInRange(500)).FirstOrDefault();
                if (target != null && target.IsValidTarget())
                {
                    //Vector3 pos;
                    PredictionOutput pred = R.GetPrediction(target);
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < R.Range)
                    {
                        /*
                        var test1 = Prediction.GetPrediction(target, R.Instance.SData.MissileSpeed).CastPosition;
                        float movement = target.MoveSpeed * 100 / 1000;
                        if (target.Distance(test1) > movement)
                        {
                            pos = target.ServerPosition.Extend(Player.ServerPosition.Extend(test1, R.Instance.SData.MissileSpeed * target.MoveSpeed), target.BoundingRadius);
                            R.Cast(pos);
                        }
                        else
                        {*/
                            if (pred.Hitchance >= HitChance.High)
                            {
                                R.Cast(pred.CastPosition);
                            }
                        //}
                    }
                }
            }           
        }

        private HitChance QHitChance
        {
            get
            {
                return GetQHitChance();
            }
        }
        private HitChance GetQHitChance()
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
        private void QPrediction(Obj_AI_Hero target)
        {            
            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= Q.Range)
            {
                if (myUtility.MovementImpaired(target))
                {
                    Q.Cast(target.ServerPosition);
                }
                PredictionOutput pred = Q.GetPrediction(target);
                var test1 = Prediction.GetPrediction(target, Q.Instance.SData.MissileSpeed).CastPosition;
                float movement = target.MoveSpeed * 100 / 1000;
                if (target.Distance(test1) > movement)
                {
                    Q.Cast(target.ServerPosition.Extend(test1, Q.Instance.SData.MissileSpeed * target.MoveSpeed));
                }
                else
                {
                    if (pred.Hitchance >= QHitChance) Q.Cast(pred.CastPosition);
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
                case myOrbwalker.OrbwalkingMode.Custom:
                    Custom();                   
                    break;    
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("UseQMisc").GetValue<bool>() && Q.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= Q.Range)
                {
                    if (myUtility.ImmuneToMagic(sender) || myUtility.ImmuneToCC(sender)) return;
                    Q.Cast(sender.Position);
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("UseQ2Misc").GetValue<bool>() && Q.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= Q.Range)
                {
                    if (myUtility.ImmuneToMagic(gapcloser.Sender) || myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Q.Cast(gapcloser.End);
                }
            }
        }
        protected override void OnEndScene(EventArgs args)
        {
            if (ObjectManager.Player.IsDead) return;
            if (R.Level > 0)
            {
                if (config.Item("UseRDrawDistance").GetValue<bool>())
                {
                    Utility.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 1, 30, true);
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
            if (R.Level > 0 && R.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ?
                    TargetSelector.GetSelectedTarget() :
                    EnemyList.Where(x => !x.InFountain() && x.IsVisible && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < 1000).OrderByDescending(i => i.CountEnemiesInRange(500)).FirstOrDefault();
                if (target != null && target.IsValidTarget())
                {
                    //Vector3 pos;
                    PredictionOutput pred = R.GetPrediction(target);
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= R.Range)
                    {
                        /*
                        var test1 = Prediction.GetPrediction(target, R.Instance.SData.MissileSpeed).CastPosition;
                        float movement = target.MoveSpeed * 100 / 1000;
                        if (target.Distance(test1) > movement)
                        {
                            pos = target.ServerPosition.Extend(Player.ServerPosition.Extend(test1, R.Instance.SData.MissileSpeed * target.MoveSpeed), target.BoundingRadius);
                            Render.Circle.DrawCircle(pos, R.Width, Color.Lime, 7);
                        }
                        else
                        {*/
                            if (pred.Hitchance >= HitChance.High)
                            {
                                Render.Circle.DrawCircle(pred.CastPosition, R.Width, Color.Lime, 7);
                            }
                        //}
                    }
                }
            }     
        }
    }
}
