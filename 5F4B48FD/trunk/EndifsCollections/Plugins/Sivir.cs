using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Sivir : PluginData
    {
        public Sivir()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1250);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

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
                laneclear.AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(false));
                laneclear.AddItem(new MenuItem("UseWFarm", "Use W").SetValue(false));
                laneclear.AddItem(new MenuItem("QFarmValue", "Q More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("WFarmValue", "W More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("FarmMana", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclear);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true)); 
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("QPredHitchance", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("UseEMisc", "E Spellblock").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseR = config.Item("UseRCombo").GetValue<bool>();
            var CastItems = config.Item("UseItemCombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToPhysical(target)) return;                
                if (CastItems) { myUtility.UseItems(0, target); }
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        QPredict(target);
                    }
                    if (UseR && R.IsReady())
                    {
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range &&
                            Vector3.Distance(Player.ServerPosition, target.ServerPosition) > Player.AttackRange)
                        {
                            var dist = Vector3.Distance(Player.ServerPosition, target.ServerPosition);
                            var msDif = Player.MoveSpeed - target.MoveSpeed;
                            var reachIn = dist / msDif;
                            if (msDif < 0 && reachIn > 3)
                            {
                                R.Cast();
                            }
                            else if (msDif > 0 && reachIn > 4)
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
                    }
                }
                catch { }
            }
            
        }
        private void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var UseQ = config.Item("UseQHarass").GetValue<bool>();            
            if (target.IsValidTarget() && !Player.IsWindingUp)
            {
                if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                if (UseQ && Q.IsReady() && Q.IsInRange(target))
                {
                    if (myUtility.IsFacing(Player, target.ServerPosition, 60)) QPredict(target);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (Player.UnderTurret(true)) return;
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                var MinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                var QLine = Q.GetLineFarmLocation(MinionsQ);
                if (QLine.Position.IsValid() && QLine.MinionsHit > config.Item("QFarmValue").GetValue<Slider>().Value)
                {
                    if (myUtility.IsFacing(Player, QLine.Position.To3D())) Q.Cast(QLine.Position);
                }
            }
            if (config.Item("UseWFarm").GetValue<bool>() && W.IsReady())
            {
                var MinionsW = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (MinionsW == null) return;
                if (MinionsW.Count > config.Item("WFarmValue").GetValue<Slider>().Value)
                {                   
                    W.Cast();
                }
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myUtility.GetLargeMonsters(Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob == null) return;
            if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady())
            {
                if (largemobs != null)
                {
                    Q.Cast(largemobs.ServerPosition);
                }
                var QLine = Q.GetLineFarmLocation(mobs);
                if (QLine.MinionsHit > 0)
                {
                    Q.Cast(QLine.Position);
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
        private void QPredict(Obj_AI_Hero target)
        {
            if (myUtility.MovementImpaired(target))
            {
                Q.Cast(target.ServerPosition);
            }
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
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (!unit.IsMe && unit.IsEnemy)
            {
                if (!spell.SData.IsAutoAttack() && spell.Target.IsMe && E.IsReady())
                {
                    if (unit.IsChampion(unit.BaseSkinName))
                    {
                        if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                        {
                            if (config.Item("UseECombo").GetValue<bool>())
                            {
                                Utility.DelayAction.Add(myUtility.RandomDelay(0, 200), () => E.Cast());
                            }
                        }
                        if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass)
                        {
                            if (config.Item("UseEHarass").GetValue<bool>())
                            {
                                Utility.DelayAction.Add(myUtility.RandomDelay(0, 200), () => E.Cast());
                            }
                        }
                        if (config.Item("UseEMisc").GetValue<bool>())
                        {
                            Utility.DelayAction.Add(myUtility.RandomDelay(0, 200), () => E.Cast());
                        }
                    }
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
                    if (config.Item("UseWCombo").GetValue<bool>() &&
                        !Player.IsWindingUp &&
                        W.IsReady() &&
                        target.IsValidTarget() && Orbwalking.InAutoAttackRange(target)) W.Cast();
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass)
                {
                    if (config.Item("UseWHarass").GetValue<bool>() &&
                        !Player.IsWindingUp &&
                        W.IsReady() &&
                        target.IsValidTarget() && Orbwalking.InAutoAttackRange(target)) W.Cast();
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear)
                {
                    if (target is Obj_AI_Minion && target.Team == GameObjectTeam.Neutral && !target.Name.Contains("Mini") &&
                        !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                    {
                        if (W.IsReady() && config.Item("UseWJFarm").GetValue<bool>()) W.Cast();
                    }
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
        }
    }
}
