using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Aatrox : PluginData
    {
        public Aatrox()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R, 550);

            Q.SetSkillshot(0, 250, 2000, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.235f, 40, 1250, false, SkillshotType.SkillshotLine);

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
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("TurretDive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("MustHavePassive", "Have Passive (Turret Dive)").SetValue(false));
                combomenu.AddItem(new MenuItem("UseItemCombo", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }

            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclear = new Menu("Farm", "Farm");
            {
                laneclear.AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
                laneclear.AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
                laneclear.AddItem(new MenuItem("QFarmValue", "Q More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("EFarmValue", "E More Than").SetValue(new Slider(1, 1, 5)));
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
                miscmenu.AddItem(new MenuItem("MiscW", "Auto W").SetValue(true));
                miscmenu.AddItem(new MenuItem("MiscWPower", "Power when % >").SetValue(new Slider(50)));
                miscmenu.AddItem(new MenuItem("MiscWLife", "Life when % <").SetValue(new Slider(40)));
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
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            var UseQ = config.Item("UseQCombo").GetValue<bool>();
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
                        QPredict(target);
                    }
                    if (UseE && E.IsReady() && !Player.IsWindingUp)
                    {                        
                        E.CastIfHitchanceEquals(target, HitChance.High);
                    }
                    if (UseR && R.IsReady() && !Player.IsWindingUp)
                    {
                        if (Player.ServerPosition.CountEnemiesInRange(500f) > 1)
                        {
                            R.Cast();
                        }
                        else
                        {
                            if (myUtility.HitsToKill(target) >= 5 || Vector3.Distance(Player.ServerPosition, target.ServerPosition) > 150f)
                            {                                
                                R.Cast();
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
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target != null)
            {
                if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                if (config.Item("UseEHarass").GetValue<bool>() && E.IsReady() && E.IsInRange(target) && !Player.IsWindingUp)
                {
                    E.CastIfHitchanceEquals(target, HitChance.High);
                }
            }
        }
        private void LaneClear()
        {
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                var MinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width);
                var QCircular = Q.GetCircularFarmLocation(MinionsQ);
                if (QCircular.MinionsHit > config.Item("QFarmValue").GetValue<Slider>().Value && !Player.IsWindingUp && !myOrbwalker.IsWaiting())
                {
                    if (QCircular.Position.To3D().Extend(Player.ServerPosition, 20f).UnderTurret(true)) return;
                    if (Vector3.Distance(QCircular.Position.To3D(), Player.ServerPosition) < Player.AttackRange) return;
                    Q.Cast(QCircular.Position.To3D().Extend(Player.ServerPosition, 20f));
                }
            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady())
            {
                var MinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                var ELine = E.GetLineFarmLocation(MinionsE);
                if (ELine.MinionsHit > config.Item("EFarmValue").GetValue<Slider>().Value && !Player.IsWindingUp && !myOrbwalker.IsWaiting())
                {
                    if (Player.UnderTurret(true)) return;
                    if (myUtility.IsFacing(Player, ELine.Position.To3D())) E.Cast(ELine.Position);
                }
            }
        }
        private void JungleClear()
        {
            var largemobs = myUtility.GetLargeMonsters(Q.Range).FirstOrDefault();
            if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
            {
                if (largemobs != null)
                {
                    Q.Cast(largemobs.ServerPosition.Extend(Player.ServerPosition, 40f));
                }
                var MobsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                var QCircular = Q.GetCircularFarmLocation(MobsQ);
                if (QCircular.MinionsHit > 0)
                {
                    Q.Cast(QCircular.Position.To3D().Extend(Player.ServerPosition, 50f));
                }
            }
            if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
            {
                if (largemobs != null)
                {
                    E.Cast(largemobs.ServerPosition);
                }
                var MobsE = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                var ELine = E.GetLineFarmLocation(MobsE);
                if (ELine.MinionsHit > 0)
                {
                    E.Cast(ELine.Position);
                }
            }
        }

        private void SmartW()
        {
            if (Player.InFountain() || Player.InShop() || Player.HasBuff("Recall")) return;
            if (myUtility.PlayerHealthPercentage > config.Item("MiscWPower").GetValue<Slider>().Value)
            {
                if (Player.HasBuff("aatroxwlife")) W.Cast();
            }
            if (myUtility.PlayerHealthPercentage < config.Item("MiscWLife").GetValue<Slider>().Value)
            {
                if (Player.HasBuff("aatroxwpower")) W.Cast();
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
        private void QPredict(Obj_AI_Hero target)
        {
            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range)
            {
                if (myUtility.MovementImpaired(target))
                {
                    Q.Cast(target.ServerPosition);
                }
                PredictionOutput pred = Q.GetPrediction(target);
                if (pred.CollisionObjects.Count == 0)
                {
                    if (target.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                    var test1 = Prediction.GetPrediction(target, Q.Instance.SData.SpellCastTime).CastPosition;
                    float movement = target.MoveSpeed * 100 / 1000;
                    if (target.Distance(test1) > movement)
                    {
                        Q.Cast(target.ServerPosition.Extend(test1, Q.Instance.SData.SpellCastTime * target.MoveSpeed));
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
        }

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsZombie)
            {
                myUtility.Reset();
                return;
            }            
            if (config.Item("MiscW").GetValue<bool>())
            {
                SmartW();
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
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe)
            {
                if (!Player.IsWindingUp &&
                    (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && config.Item("UseItemCombo").GetValue<bool>()) &&
                    Orbwalking.InAutoAttackRange(target))
                {
                    myUtility.UseItems(2, null);                    
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
