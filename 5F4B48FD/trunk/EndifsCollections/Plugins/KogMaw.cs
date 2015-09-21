using System;
using System.Linq;
using EndifsCollections.Controller;
using EndifsCollections.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class KogMaw : PluginData
    {
        public KogMaw()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W, 630);
            E = new Spell(SpellSlot.E, 1280);
            R = new Spell(SpellSlot.R, 1200);

            Q.SetSkillshot(0.25f, 70f, 1650f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.6f*2, 100f, float.MaxValue, false, SkillshotType.SkillshotCircle);                 
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Living Artillery ", "Custom");
            {
                custommenu.AddItem(new MenuItem("UseRKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T                
                custommenu.AddItem(new MenuItem("UseRConserve", "Conserve Mana").SetValue(true));
                custommenu.AddItem(new MenuItem("UseRDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("UseRDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("UseRComboBool", "(R) Conserve Mana").SetValue(true));
                combomenu.AddItem(new MenuItem("UseRComboValue", "Up to x stacks").SetValue(new Slider(1, 1, 10)));
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
                laneclear.AddItem(new MenuItem("UseRFarm", "Use R").SetValue(true));
                laneclear.AddItem(new MenuItem("EFarmValue", "E More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("RFarmValue", "R More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("FarmMana", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclear);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true));          
                junglemenu.AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseRJFarm", "Use R").SetValue(true));
                config.AddSubMenu(junglemenu);
            }  
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("KPredHitchance", "Spells Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawW", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawE", "E").SetValue(true));
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
            var Conserve = config.Item("UseRComboBool").GetValue<bool>();
            if (UseR) Custom(Conserve, target);
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToPhysical(target)) return;
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        QPredict(target);
                    }
                    if (UseW && W.IsReady() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= WRange)
                    {
                        W.Cast();
                    }
                    if (UseE && E.IsReady())
                    {
                        EPredict(target);
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
                if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                if (UseQ && Q.IsReady())
                {
                    QPredict(target);
                }
                if (UseW && W.IsReady() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= WRange)
                {
                    W.Cast();
                }
                if (UseE && E.IsReady())
                {
                    EPredict(target);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;            
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp && !myOrbwalker.IsWaiting())
            {
                if (Player.UnderTurret(true)) return;
                var minionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (minionsQ == null) return;
                var selectQ = minionsQ.Where(x => Q.IsKillable(x)).OrderBy(i => i.Distance(Player)).FirstOrDefault();
                if (selectQ != null && !Player.UnderTurret(true))
                {
                    Q.Cast(selectQ.Position);
                }

            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady())
            {
                if (Player.UnderTurret(true)) return;
                var MinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                var ELine = E.GetLineFarmLocation(MinionsE);
                if (ELine.Position.IsValid() && Vector3.Distance(Player.ServerPosition, ELine.Position.To3D()) > Player.AttackRange)
                {
                    if (ELine.MinionsHit > config.Item("EFarmValue").GetValue<Slider>().Value)
                    {
                        if (myUtility.IsFacing(Player, ELine.Position.To3D())) E.Cast(ELine.Position);
                    }
                }
            }
            if (config.Item("UseRFarm").GetValue<bool>() && R.IsReady() && !Player.IsWindingUp && R.Instance.ManaCost <= 40)
            {
                if (Player.UnderTurret(true)) return;
                var minionR = MinionManager.GetMinions(Player.ServerPosition, RRange);
                if (minionR == null) return;
                var rpred = R.GetCircularFarmLocation(minionR);
                if (rpred.MinionsHit > config.Item("RFarmValue").GetValue<Slider>().Value)
                {
                    R.Cast(rpred.Position);
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
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady())
                {
                    if (largemobs != null)
                    {
                        Q.Cast(largemobs.ServerPosition);
                    }
                    if (myUtility.IsFacing(Player, mob.ServerPosition, 70)) Q.Cast(mob);
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady())
                {
                    if (largemobs != null)
                    {
                        E.Cast(largemobs.ServerPosition);
                    }
                    var ELine = E.GetLineFarmLocation(mobs);
                    if (ELine.MinionsHit > 0)
                    {
                        if (myUtility.IsFacing(Player, ELine.Position.To3D(), 70)) E.Cast(ELine.Position);
                    }
                }
                if (config.Item("UseRJFarm").GetValue<bool>() && R.IsReady() && R.Instance.ManaCost <= 40)
                {
                    var MobsR = MinionManager.GetMinions(Player.ServerPosition, RRange, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                    MinionManager.FarmLocation RCircular = R.GetCircularFarmLocation(MobsR);
                    if (RCircular.MinionsHit > 0)
                    {
                        R.Cast(RCircular.Position.To3D().Shorten(Player.ServerPosition, 10f));
                    }
                }
            }
        }
        private void Custom(bool conserve, Obj_AI_Hero selected)
        {
            if (R.IsReady())
            {
                Obj_AI_Hero target;
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && conserve && R.Instance.ManaCost > (40 * config.Item("UseRComboValue").GetValue<Slider>().Value)) return;
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Custom && config.Item("UseRConserve").GetValue<bool>() && R.Instance.ManaCost > 40) return;
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToMagic(x) && !myUtility.ImmuneToPhysical(x));
                if (selected != null && selected.IsValid<Obj_AI_Hero>()) target = selected;
                else
                {
                    target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? 
                        TargetSelector.GetSelectedTarget() :
                        EnemyList.Where(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= RRange).OrderByDescending(z => myRePriority.ResortDB(z.ChampionName)).ThenBy(i => i.Health).FirstOrDefault();
                }
                if (target == null) return;
                PredictionOutput pred = R.GetPrediction(target);
                Vector3 pos;
                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= RRange)
                {
                    
                    var test1 = Prediction.GetPrediction(target, 1.2f).CastPosition;
                    float movement = target.MoveSpeed * 100 / 1000;
                    if (target.Distance(test1) > movement)
                    {                        
                        if (myUtility.IsFacing(target, Player.ServerPosition))
                        {
                            pos = target.ServerPosition.Extend(Player.ServerPosition.Extend(test1, target.MoveSpeed), 100);
                            R.Cast(pos);
                        }
                        if (!myUtility.IsFacing(target, Player.ServerPosition))
                        {
                            pos = target.ServerPosition.Extend(Player.ServerPosition.Extend(test1, target.MoveSpeed), -100);
                            R.Cast(pos);
                        }                        
                    }
                    else
                    {
                        if (pred.Hitchance >= KHitChance)
                        {
                            pos = myUtility.RandomPos(1, 10, 10, pred.CastPosition);
                            R.Cast(pos);
                        }
                    }
                }
            }
        }

        private void QPredict(Obj_AI_Hero target)
        {
            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range)
            {
                PredictionOutput pred = Q.GetPrediction(target);
                if (pred.CollisionObjects.Count == 0)
                {
                    var test1 = Prediction.GetPrediction(target, Q.Instance.SData.MissileSpeed).CastPosition;
                    float movement = target.MoveSpeed * 100 / 1000;
                    if (target.Distance(test1) > movement) W.Cast(target.ServerPosition.Extend(test1, W.Instance.SData.MissileSpeed * target.MoveSpeed));
                    else
                    {
                        if (pred.Hitchance >= KHitChance) Q.Cast(pred.CastPosition);
                    }
                }
            }
        }
        private void EPredict(Obj_AI_Hero target)
        {
            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < E.Range)
            {
                if (myUtility.MovementImpaired(target))
                {
                    E.Cast(target.ServerPosition);
                }
                PredictionOutput pred = E.GetPrediction(target);
                var test1 = Prediction.GetPrediction(target, E.Instance.SData.SpellCastTime).CastPosition;
                float movement = target.MoveSpeed * 100 / 1000;
                if (target.Distance(test1) > movement) Q.Cast(target.ServerPosition.Extend(test1, E.Instance.SData.SpellCastTime * target.MoveSpeed));
                else
                {
                    if (pred.Hitchance >= KHitChance) E.Cast(pred.CastPosition);
                }
            }
        }
        private float WRange
        {
            get
            {
                return W.Level > 0 ? (W.Level * 20) + 110 + Player.AttackRange : 630;
            }
        }
        private float RRange
        {
            get
            {
                return R.Level > 0 ? 900 + (R.Level * 300) : 1200;
            }
        }
       
        private HitChance KHitChance
        {
            get
            {
                return GetKHitChance();
            }
        }
        private HitChance GetKHitChance()
        {
            switch (config.Item("KPredHitchance").GetValue<StringList>().SelectedIndex)
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

        protected override void OnUpdate(EventArgs args)
        {
            R.Range = RRange;
            W.Range = WRange;
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
                    Custom(false, null);
                    break;
            }            
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Minion)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear &&
                    config.Item("UseWFarm").GetValue<bool>() &&
                    !Player.IsWindingUp &&
                    W.IsReady())
                {
                    W.Cast();
                }
            }
            if (args.Target is Obj_AI_Minion && args.Target.Team == GameObjectTeam.Neutral)
            {
                if (!args.Target.Name.Contains("Mini") &&
                    config.Item("UseWJFarm").GetValue<bool>() &&
                    !Player.IsWindingUp &&
                    W.IsReady())
                {
                    W.Cast();
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
                Render.Circle.DrawCircle(ObjectManager.Player.Position, WRange, Color.White);
            }
            if (config.Item("DrawE").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.White);
            }
            if (R.Level > 0)
            {
                if (config.Item("UseRDrawDistance").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, RRange, Color.Fuchsia, 7);
                }
                if (config.Item("UseRDrawTarget").GetValue<bool>())
                {
                    var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToMagic(x) && !myUtility.ImmuneToPhysical(x));
                    var target = EnemyList.Where(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= RRange).OrderByDescending(z => myRePriority.ResortDB(z.ChampionName)).ThenBy(i => i.Health).FirstOrDefault();
                    if (target == null) return;
                    PredictionOutput pred = R.GetPrediction(target);
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= RRange)
                    {
                        var test1 = Prediction.GetPrediction(target, 1.2f).CastPosition;
                        float movement = target.MoveSpeed * 100 / 1000;
                        if (target.Distance(test1) > movement)
                        {
                            var pos = target.ServerPosition.Extend(Player.ServerPosition.Extend(test1, 1.2f * target.MoveSpeed), 100);                                                        
                            if (myUtility.IsFacing(target, Player.ServerPosition))
                            {
                                pos = target.ServerPosition.Extend(Player.ServerPosition.Extend(test1, 1.2f * target.MoveSpeed), 100);
                                Render.Circle.DrawCircle(pos, target.BoundingRadius, Color.Lime, 7);
                            }
                            if (!myUtility.IsFacing(target, Player.ServerPosition))
                            {
                                pos = target.ServerPosition.Extend(Player.ServerPosition.Extend(test1, 1.2f * target.MoveSpeed), -100);
                                Render.Circle.DrawCircle(pos, target.BoundingRadius, Color.Lime, 7);
                            } 
                        }
                        else
                        {
                            if (pred.Hitchance >= KHitChance)
                            {
                                Render.Circle.DrawCircle(pred.CastPosition, target.BoundingRadius, Color.Red, 7);
                            }
                        }
                    }
                }
            }
        }
    }
}
