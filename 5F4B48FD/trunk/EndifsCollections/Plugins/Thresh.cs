using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Thresh : PluginData
    {
        public Thresh()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 400);
            R = new Spell(SpellSlot.R, 400);

            Q.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed, true, SkillshotType.SkillshotLine);         

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Flash Flay", "Custom");
            {
                custommenu.AddItem(new MenuItem("UseFFKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("UseFFDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("UseFFDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));                
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("UseQ2Combo", "Use Q Leap").SetValue(false));
                combomenu.AddItem(new MenuItem("UseWComboLogic", "W").SetValue(new StringList(new[] { "after Hooked", "after Leaped", "Ignores Q state" })));
                combomenu.AddItem(new MenuItem("UseWComboType", "W").SetValue(new StringList(new[] { "Path Block", "Ally", "Self" })));                
                combomenu.AddItem(new MenuItem("UseEComboType", "E").SetValue(new StringList(new[] { "Inwards", "Outwards" })));
                combomenu.AddItem(new MenuItem("RComboValue", "R More Than").SetValue(new Slider(1, 1, 5)));
                combomenu.AddItem(new MenuItem("TurretDive", "Turret Dive").SetValue(false));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {               
                harassmenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclear = new Menu("Farm", "Farm");
            {
                laneclear.AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
                laneclear.AddItem(new MenuItem("EFarmValue", "E More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("UseEFarmType", "E").SetValue(new StringList(new[] { "Inwards", "Outwards" })));
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
                miscmenu.AddItem(new MenuItem("QPredLogic", "Q Logic").SetValue(new StringList(new[] { "Winding Up", "Within Bounding Box", "Predict Movement" })));
                miscmenu.AddItem(new MenuItem("QPredHitchance", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("UseQMisc", "Q Interrupts").SetValue(true));
                miscmenu.AddItem(new MenuItem("UseEMisc", "E Interrupts").SetValue(true));
                miscmenu.AddItem(new MenuItem("UseE2Misc", "E Gapcloser").SetValue(true));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawW", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawE", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawR", "R").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawQPred", "Q Prediction").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var UseR = config.Item("UseRCombo").GetValue<bool>();
            var UseQ2 = config.Item("UseQ2Combo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToMagic(target)) return;
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - QTick > QTime && !Player.IsWindingUp && !QLeap)
                    {
                        QPrediction(target);
                    }
                    if (UseQ2 && target == LastQTarget && (myUtility.TickCount - QTick > 2000 && myUtility.TickCount - QTick < 3000) && target.HasBuff("ThreshQ") && !target.HasBuff("threshqfakeknockup"))
                    {
                        if (target.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                        Q.Cast();
                    }
                    if (UseW && W.IsReady() && !Player.IsWindingUp)
                    {
                        switch (config.Item("UseWComboType").GetValue<StringList>().SelectedIndex)
                        {
                            case 0:
                                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= 900)
                                {
                                    switch (config.Item("UseWComboLogic").GetValue<StringList>().SelectedIndex)
                                    {
                                        case 0:
                                            if (target.HasBuff("ThreshQ"))
                                            {
                                                W.Cast(Player.ServerPosition.Extend(target.ServerPosition, 125f));
                                            }
                                            break;
                                        case 1:
                                            if (target == LastQTarget && myUtility.TickCount - QTick < 3000 && target.HasBuff("ThreshQ") && !target.HasBuff("threshqfakeknockup"))
                                            {
                                                W.Cast(Player.ServerPosition.Extend(target.ServerPosition, 125f));
                                            }
                                            break;
                                        case 2:
                                            W.Cast(Player.ServerPosition.Extend(target.ServerPosition, 125f));
                                            break;
                                    }
                                }
                                break;
                            case 1:
                                var Ally = HeroManager.Allies.Where(x => !x.IsDead && !x.IsMe && Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= 1125).OrderByDescending(i => i.Distance(Player)).FirstOrDefault();
                                if (Ally != null)
                                {
                                    switch (config.Item("UseWComboLogic").GetValue<StringList>().SelectedIndex)
                                    {
                                        case 0:
                                            if (target.HasBuff("ThreshQ"))
                                            {
                                                W.Cast(Player.ServerPosition.Extend(Ally.ServerPosition, 100f));
                                            }
                                            break;
                                        case 1:
                                            if (target == LastQTarget && myUtility.TickCount - QTick < 3000 && target.HasBuff("ThreshQ") && !target.HasBuff("threshqfakeknockup"))
                                            {
                                                W.Cast(Player.ServerPosition.Extend(Ally.ServerPosition, 100f));
                                            }
                                            break;
                                        case 2:
                                            W.Cast(Player.ServerPosition.Extend(Ally.ServerPosition, 100f));
                                            break;
                                    }
                                }
                                break;
                            case 2:
                                switch (config.Item("UseWComboLogic").GetValue<StringList>().SelectedIndex)
                                {

                                    case 0:
                                        if (target.HasBuff("ThreshQ"))
                                        {
                                            W.Cast(Player.ServerPosition.Extend(target.ServerPosition, (Vector3.Distance(Player.ServerPosition, target.ServerPosition) / 2)));
                                        }
                                        break;
                                    case 1:
                                        if (target == LastQTarget && myUtility.TickCount - QTick < 3000 && target.HasBuff("ThreshQ") && !target.HasBuff("threshqfakeknockup"))
                                        {
                                            W.Cast(Player.ServerPosition.Extend(target.ServerPosition, (Vector3.Distance(Player.ServerPosition, target.ServerPosition) / 2)));
                                        }
                                        break;
                                    case 2:
                                        W.Cast(Player.ServerPosition.Extend(target.ServerPosition, (Vector3.Distance(Player.ServerPosition, target.ServerPosition) / 2)));
                                        break;
                                }
                                break;
                        }
                    }
                    if (UseE && E.IsReady() && E.IsInRange(target) && !Player.IsDashing() && !Player.IsWindingUp && !target.HasBuff("ThreshQ") && (myUtility.TickCount - QTick > 2000))
                    {
                        switch (config.Item("UseEComboType").GetValue<StringList>().SelectedIndex)
                        {
                            case 0:
                                if (myUtility.TickCount - JumpTime < (1000 + Game.Ping))
                                {
                                    E.Cast(JumpStart);
                                }
                                else
                                {
                                    E.Cast(EInwards(Player, target.Position));
                                }
                                break;
                            case 1:
                                if (myUtility.TickCount - JumpTime < (1000 + Game.Ping))
                                {
                                    E.Cast(JumpEnd);
                                }
                                else
                                {
                                    E.Cast(target.Position);
                                }
                                break;
                        }
                    }
                    if (UseR && R.IsReady() && !Player.IsWindingUp)
                    {
                        var EnemyList = HeroManager.AllHeroes.Where(x => x.IsValidTarget() && x.IsEnemy && !x.IsDead && !x.IsZombie && !x.IsInvulnerable);
                        var ValidTargets = EnemyList.Where(x => !x.InFountain() && x.IsVisible && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range).ToList();
                        if (ValidTargets.Count() == 1 && ValidTargets[0] != null)
                        {
                            if (ValidTargets[0].HealthPercent < 50) R.Cast();
                        }
                        else if (ValidTargets.Count() > config.Item("RComboValue").GetValue<Slider>().Value)
                        {
                            R.Cast();
                        }
                    }
                }
                catch { }
            }           
        }
        private void Harass()
        {
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var UseE = config.Item("UseEHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - QTick >= QTime && !Player.IsWindingUp && !QLeap && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range - 50f)
                    {
                        QPrediction(target);
                    }
                    if (UseE && E.IsReady() && E.IsInRange(target) && !Player.IsDashing() && !Player.IsWindingUp)
                    {
                        E.Cast(target.Position);
                    }
                }
                catch { }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (Player.UnderTurret(true)) return;
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp && myOrbwalker.IsWaiting())
            {
                var minionE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (minionE == null) return;
                foreach (var x in minionE)
                {
                    if (MinionManager.GetMinions(x.ServerPosition, 200f).Count() > config.Item("EFarmValue").GetValue<Slider>().Value)
                    {
                        if (x.ServerPosition.UnderTurret(true))
                        {
                            E.Cast(EInwards(Player, x.ServerPosition));
                        }
                        else if (x.ServerPosition.UnderTurret(false))
                        {
                            E.Cast(x.ServerPosition);
                        }
                        else
                        {
                            switch (config.Item("UseEFarmType").GetValue<StringList>().SelectedIndex)
                            {
                                case 0:
                                    E.Cast(EInwards(Player, x.ServerPosition));
                                    break;
                                case 1:
                                    E.Cast(x.ServerPosition);
                                    break;
                            }
                        }
                    }
                }   
            }       
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();
            var largemobs = myUtility.GetLargeMonsters(Q.Range).FirstOrDefault();
            if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && myUtility.TickCount - QTick >= QTime && !Player.IsWindingUp && !QLeap)
            {
                if (largemobs != null && Q.IsInRange(largemobs) && Q.IsKillable(largemobs) && !Player.IsWindingUp)
                {
                    Q.Cast(largemobs.ServerPosition);
                }
            }
            if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
            {
                if (largemobs != null && E.IsInRange(largemobs))
                {
                    E.Cast(largemobs.ServerPosition);
                }
                else
                {
                    if (mobs != null && mobs.IsValidTarget() && E.IsInRange(mobs))
                        E.Cast(EInwards(Player, mobs.ServerPosition));
                }
            }
        }
        private void Custom()
        {
            if (FlashSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready && E.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                var FF = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) > E.Range &&
                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) < E.Range + 425f
                    ).OrderBy(i => i.Health).FirstOrDefault();
                if (FF == null) return;
                E.UpdateSourcePosition(Player.ServerPosition.Extend(FF.ServerPosition, 425f));
                Player.Spellbook.CastSpell(FlashSlot, Player.ServerPosition.Extend(FF.ServerPosition, 425f));
                E.Cast(EInwards(Player, FF.ServerPosition));               
            }
        }

        private Vector3 JumpStart, JumpEnd;
        private int? JumpTime;
        private const int QTime = 3000;
        private int QTick;
        private static Obj_AI_Hero LastQTarget { get; set; }
        
        private bool QLeap
        {
            get { return Q.Instance.Name == "threshqleap"; }
        }
        private void QReset()
        {
            if (LastQTarget != null)
            {
                if (myUtility.TickCount - QTick > QTime)
                {
                    LastQTarget = null;
                }
            }
        }
        private void QPrediction(Obj_AI_Hero target)
        {
            PredictionOutput pred = Q.GetPrediction(target);
            if (pred.CollisionObjects.Count == 0 && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range)
            {
                switch (config.Item("QPredLogic").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        if (pred.Hitchance >= QHitChance) Q.Cast(pred.CastPosition);
                        break;
                    case 1:
                        var vc1 = new Vector3(
                            target.ServerPosition.X + ((pred.UnitPosition.X - target.ServerPosition.X) / 2),
                            target.ServerPosition.Y + ((pred.UnitPosition.Y - target.ServerPosition.Y) / 2),
                            target.ServerPosition.Z);
                        if (Vector3.Distance(vc1, target.ServerPosition) < target.BoundingRadius) Q.Cast(vc1);
                        break;
                    case 2:
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
                        break;

                }
                LastQTarget = target;
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
        private Vector3 EInwards(Obj_AI_Hero source, Vector3 target)
        {
            return new Vector3(source.Position.X + (source.Position.X - target.X), source.Position.Y + (source.Position.Y - target.Y), source.Position.Z);
        }

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                myUtility.Reset();
                return;
            }
            QReset();
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
        protected override void OnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            if (sender.IsMe)
            {
                JumpStart = args.StartPos.To3D();
                JumpEnd = args.EndPos.To3D();
                //JumpTime = myUtility.TickCount;
                JumpTime = args.EndTick;
                
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.E && Player.IsDashing())
            {
                args.Process = false;
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if (spell.SData.Name.ToLower() == "threshq")
                {
                    QTick = myUtility.TickCount;
                }
            }
        }
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && (myUtility.PlayerManaPercentage > config.Item("FarmMana").GetValue<Slider>().Value))
            {
                var target = minion as Obj_AI_Base;
                if (target != null &&
                    E.IsKillable(target) &&
                    !Player.IsWindingUp &&
                    target.BaseSkinName.Contains("Siege"))
                {
                    E.Cast(EInwards(Player, target.Position));
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("UseQMisc").GetValue<bool>() && Q.IsReady())
            {
                if (args.DangerLevel == Interrupter2.DangerLevel.High)
                {
                    if (myUtility.ImmuneToCC(sender)) return;
                    QPrediction(sender);
                }
            }
            if (config.Item("UseEMisc").GetValue<bool>() && E.IsReady())
            {
                if (Vector3.Distance(Player.ServerPosition, sender.ServerPosition) < E.Range)
                {
                    if (myUtility.ImmuneToCC(sender)) return;
                    E.Cast(sender.ServerPosition);
                }
            } 
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("UseE2Misc").GetValue<bool>() && E.IsReady())
            {
                if (Vector3.Distance(Player.ServerPosition, gapcloser.End) < E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    E.Cast(gapcloser.End);
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
            if (FlashSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready && E.IsReady())
            {
                if (config.Item("UseFFDrawDistance").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, 425f, Color.Fuchsia, 7);
                    Render.Circle.DrawCircle(Player.Position, E.Range + 425f, Color.Fuchsia, 7);
                }
                if (config.Item("UseFFDrawTarget").GetValue<bool>())
                {
                    var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                    var FF = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) > E.Range &&
                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) < E.Range + 425f
                    ).OrderBy(i => i.Health).FirstOrDefault();
                    if (FF != null && FF.IsValidTarget())
                    {
                        Render.Circle.DrawCircle(FF.ServerPosition, FF.BoundingRadius, Color.Lime, 7);
                    }
                }
            }
            if (config.Item("DrawQPred").GetValue<bool>())
            {
                var target = TargetSelector.GetSelectedTarget();
                if (target == null) return;
                PredictionOutput pred = Q.GetPrediction(target);
                if (pred.CollisionObjects.Count == 0 && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range)
                {
                    switch (config.Item("QPredLogic").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            if (pred.Hitchance >= QHitChance && target.IsWindingUp)
                            {
                                Render.Circle.DrawCircle(pred.CastPosition, E.Width, Color.Fuchsia, 7);
                            }
                            break;
                        case 1:
                            var vc1 = new Vector3(
                                target.ServerPosition.X + ((pred.UnitPosition.X - target.ServerPosition.X) / 2),
                                target.ServerPosition.Y + ((pred.UnitPosition.Y - target.ServerPosition.Y) / 2),
                                target.ServerPosition.Z);
                            if (Vector3.Distance(vc1, target.ServerPosition) < target.BoundingRadius)
                            {
                                Render.Circle.DrawCircle(vc1, E.Width, Color.Fuchsia, 7);
                            }
                            break;
                        case 2:
                            var test1 = Prediction.GetPrediction(target, Q.Instance.SData.MissileSpeed).CastPosition;
                            float movement = target.MoveSpeed * 100 / 1000;
                            if (target.Distance(test1) > movement)
                            {
                                var pos = target.ServerPosition.Extend(test1, Q.Instance.SData.MissileSpeed * target.MoveSpeed);
                                Render.Circle.DrawCircle(pos, E.Width, Color.Fuchsia, 7);
                            }
                            else
                            {
                                if (pred.Hitchance >= QHitChance)
                                {
                                    Render.Circle.DrawCircle(pred.CastPosition, E.Width, Color.Fuchsia, 7);
                                }
                            }
                            break;
                    }
                }
            }
        }
    }
}
