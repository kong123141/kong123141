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

            Q.SetSkillshot(0.5f, 60, 1200f, true, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Flash Flay", "Custom");
            {
                custommenu.AddItem(new MenuItem("EC.Thresh.UseFFKey", "Key").SetValue(new KeyBind(Root.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("EC.Thresh.UseFFDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("EC.Thresh.UseFFDrawDistance", "Draw Distance").SetValue(true));
                Root.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Thresh.Combo.Q", "Use Q").SetValue(true));                
                combomenu.AddItem(new MenuItem("EC.Thresh.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Thresh.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Thresh.Combo.R", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Thresh.Combo.Q2", "Use Q Leap").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Thresh.Combo.WLogic", "W").SetValue(new StringList(new[] { "after Hooked", "after Leaped", "Ignores Q state" })));
                combomenu.AddItem(new MenuItem("EC.Thresh.Combo.WType", "W").SetValue(new StringList(new[] { "Path Block", "Ally", "Self" })));                
                combomenu.AddItem(new MenuItem("EC.Thresh.Combo.EType", "E").SetValue(new StringList(new[] { "Inwards", "Outwards" })));
                combomenu.AddItem(new MenuItem("EC.Thresh.RComboValue", "R More Than").SetValue(new Slider(1, 1, 5)));
                combomenu.AddItem(new MenuItem("EC.Thresh.Combo.Dive", "Turret Dive").SetValue(false));
                Root.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {               
                harassmenu.AddItem(new MenuItem("EC.Thresh.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Thresh.Harass.E", "Use E").SetValue(true));
                Root.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Thresh.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Thresh.Farm.E.Value", "E More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Thresh.Farm.EType", "E").SetValue(new StringList(new[] { "Inwards", "Outwards" })));
                laneclearmenu.AddItem(new MenuItem("EC.Thresh.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                Root.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Thresh.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Thresh.Jungle.E", "Use E").SetValue(true));
                Root.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Thresh.QPredLogic", "Q Logic").SetValue(new StringList(new[] { "Default", "Custom", "Predict Movement" })));
                miscmenu.AddItem(new MenuItem("EC.Thresh.QPredHitchance", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("EC.Thresh.Misc.Q", "Q Interrupts").SetValue(true));
                miscmenu.AddItem(new MenuItem("EC.Thresh.Misc.E", "E Interrupts").SetValue(true));
                miscmenu.AddItem(new MenuItem("EC.Thresh.Misc.E2", "E Gapcloser").SetValue(true));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Thresh.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Thresh.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Thresh.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Thresh.Draw.R", "R").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Thresh.Draw.QPred", "Q Prediction").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical, true);

            var UseQ = Root.Item("EC.Thresh.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Thresh.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Thresh.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Thresh.Combo.R").GetValue<bool>();
            var UseQ2 = Root.Item("EC.Thresh.Combo.Q2").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToMagic(Target)) return;
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - QTick > QTime && !QLeap)
                    {
                        mySpellcast.Hook(Target, Q);
                    }
                    if (UseQ2 && Target == LastQTarget && (myUtility.TickCount - QTick > 2000 && myUtility.TickCount - QTick < 3000) && Target.HasBuff("ThreshQ") && !Target.HasBuff("threshqfakeknockup"))
                    {
                        if (Target.UnderTurret(true) && !Root.Item("EC.Thresh.Combo.Dive").GetValue<bool>()) return;
                        Q.Cast();
                    }
                    if (UseW && W.IsReady() && !Player.IsWindingUp)
                    {
                        switch (Root.Item("EC.Thresh.Combo.WType").GetValue<StringList>().SelectedIndex)
                        {
                            case 0:
                                if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= 900)
                                {
                                    switch (Root.Item("EC.Thresh.Combo.WLogic").GetValue<StringList>().SelectedIndex)
                                    {
                                        case 0:
                                            if (Target.HasBuff("ThreshQ"))
                                            {
                                                W.Cast(Player.ServerPosition.Extend(Target.ServerPosition, 125f));
                                            }
                                            break;
                                        case 1:
                                            if (Target == LastQTarget && myUtility.TickCount - QTick < 3000 && Target.HasBuff("ThreshQ") && !Target.HasBuff("threshqfakeknockup"))
                                            {
                                                W.Cast(Player.ServerPosition.Extend(Target.ServerPosition, 125f));
                                            }
                                            break;
                                        case 2:
                                            W.Cast(Player.ServerPosition.Extend(Target.ServerPosition, 125f));
                                            break;
                                    }
                                }
                                break;
                            case 1:
                                var Ally = HeroManager.Allies.Where(x => !x.IsDead && !x.IsMe && Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= 1125).OrderByDescending(i => i.Distance(Player)).FirstOrDefault();
                                if (Ally != null)
                                {
                                    switch (Root.Item("EC.Thresh.Combo.WLogic").GetValue<StringList>().SelectedIndex)
                                    {
                                        case 0:
                                            if (Target.HasBuff("ThreshQ"))
                                            {
                                                W.Cast(Player.ServerPosition.Extend(Ally.ServerPosition, 100f));
                                            }
                                            break;
                                        case 1:
                                            if (Target == LastQTarget && myUtility.TickCount - QTick < 3000 && Target.HasBuff("ThreshQ") && !Target.HasBuff("threshqfakeknockup"))
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
                                switch (Root.Item("EC.Thresh.Combo.WLogic").GetValue<StringList>().SelectedIndex)
                                {

                                    case 0:
                                        if (Target.HasBuff("ThreshQ"))
                                        {
                                            W.Cast(Player.ServerPosition.Extend(Target.ServerPosition, (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) / 2)));
                                        }
                                        break;
                                    case 1:
                                        if (Target == LastQTarget && myUtility.TickCount - QTick < 3000 && Target.HasBuff("ThreshQ") && !Target.HasBuff("threshqfakeknockup"))
                                        {
                                            W.Cast(Player.ServerPosition.Extend(Target.ServerPosition, (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) / 2)));
                                        }
                                        break;
                                    case 2:
                                        W.Cast(Player.ServerPosition.Extend(Target.ServerPosition, (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) / 2)));
                                        break;
                                }
                                break;
                        }
                    }
                    if (UseE && E.IsReady() && E.IsInRange(Target) && !Player.IsDashing() && !Player.IsWindingUp && !Target.HasBuff("ThreshQ") && (myUtility.TickCount - QTick > 2000))
                    {
                        switch (Root.Item("EC.Thresh.Combo.EType").GetValue<StringList>().SelectedIndex)
                        {
                            case 0:
                                if (myUtility.TickCount - JumpTime < (1000 + Game.Ping))
                                {
                                    E.Cast(JumpStart);
                                }
                                else
                                {
                                    E.Cast(EInwards(Player, Target.Position));
                                }
                                break;
                            case 1:
                                if (myUtility.TickCount - JumpTime < (1000 + Game.Ping))
                                {
                                    E.Cast(JumpEnd);
                                }
                                else
                                {
                                    E.Cast(Target.Position);
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
                        else if (ValidTargets.Count() > Root.Item("EC.Thresh.RComboValue").GetValue<Slider>().Value)
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
            var UseQ = Root.Item("EC.Thresh.Harass.Q").GetValue<bool>();
            var UseE = Root.Item("EC.Thresh.Harass.E").GetValue<bool>();
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
            if (myUtility.PlayerManaPercentage < Root.Item("EC.Thresh.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (Player.UnderTurret(true)) return;
            if (Root.Item("EC.Thresh.Farm.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp && myOrbwalker.Waiting)
            {
                var minionE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (minionE == null) return;
                foreach (var x in minionE)
                {
                    if (MinionManager.GetMinions(x.ServerPosition, 200f).Count() > Root.Item("EC.Thresh.Farm.E.Value").GetValue<Slider>().Value)
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
                            switch (Root.Item("EC.Thresh.Farm.EType").GetValue<StringList>().SelectedIndex)
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
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, Q.Range).FirstOrDefault();
            if (Root.Item("EC.Thresh.Jungle.Q").GetValue<bool>() && Q.IsReady() && myUtility.TickCount - QTick >= QTime && !Player.IsWindingUp && !QLeap)
            {
                if (largemobs != null && Q.IsInRange(largemobs) && Q.IsKillable(largemobs) && !Player.IsWindingUp)
                {
                    Q.Cast(largemobs.ServerPosition);
                }
            }
            if (Root.Item("EC.Thresh.Jungle.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
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
            if (pred.CollisionObjects.Count == 0 && Vector3.Distance(Player.ServerPosition, pred.CastPosition) <= Q.Range)
            {
                switch (Root.Item("EC.Thresh.QPredLogic").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        if (pred.Hitchance >= QHitChance)
                        {
                            Q.Cast(pred.CastPosition);
                        }
                        break;
                    case 1:
                        if (pred.Hitchance >= QHitChance)
                        {
                            Q.Cast(Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition)));
                        }                       
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
            switch (Root.Item("EC.Thresh.QPredHitchance").GetValue<StringList>().SelectedIndex)
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
            if (Root.Item("EC.Thresh.Farm.E").GetValue<bool>() && E.IsReady() && (myUtility.PlayerManaPercentage > Root.Item("EC.Thresh.Farm.ManaPercent").GetValue<Slider>().Value))
            {
                var target = minion as Obj_AI_Base;
                if (target != null &&
                    E.IsKillable(target) &&
                    !Player.IsWindingUp &&
                    target.CharData.BaseSkinName.Contains("Siege"))
                {
                    E.Cast(EInwards(Player, target.Position));
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Root.Item("EC.Thresh.Misc.Q").GetValue<bool>() && Q.IsReady())
            {
                if (args.DangerLevel == Interrupter2.DangerLevel.High)
                {
                    if (myUtility.ImmuneToCC(sender)) return;
                    QPrediction(sender);
                }
            }
            if (Root.Item("EC.Thresh.Misc.E").GetValue<bool>() && E.IsReady())
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
            if (Root.Item("EC.Thresh.Misc.E2").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.Cast(gapcloser.End)) ;
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Thresh.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Thresh.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Thresh.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Thresh.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.White);
            }
            if (FlashSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready && E.IsReady())
            {
                if (Root.Item("EC.Thresh.UseFFDrawDistance").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, 425f, Color.Fuchsia, 7);
                    Render.Circle.DrawCircle(Player.Position, E.Range + 425f, Color.Fuchsia, 7);
                }
                if (Root.Item("EC.Thresh.UseFFDrawTarget").GetValue<bool>())
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
            if (Root.Item("EC.Thresh.Draw.QPred").GetValue<bool>())
            {
                Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (Target.IsValidTarget(Q.Range))
                {
                    PredictionOutput Pred = Q.GetPrediction(Target);
                    var box = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(Target.ServerPosition, (Q.Range - Target.BoundingRadius - Q.Width)), Q.Width);
                    if (Pred.Hitchance >= HitChance.High && Pred.CollisionObjects.Count == 0 && box.Points.Any(point => Vector3.Distance(point.To3D(), Target.Position) <= Q.Width + Target.BoundingRadius))
                    {
                        box.Draw(Color.Red);
                    }
                    else
                    {
                        box.Draw(Color.White);
                    }
                }
            }
        }
    }
}
