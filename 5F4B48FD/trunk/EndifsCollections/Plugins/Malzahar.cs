using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCollections.Controller;
using EndifsCollections.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Malzahar : PluginData
    {
        public Malzahar()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 700);

            Q.SetSkillshot(0.5f, 100, Q.Instance.SData.SpellCastTime, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.5f, 240, 20, false, SkillshotType.SkillshotCircle);     

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Nether Grasp", "Custom");
            {
                custommenu.AddItem(new MenuItem("UseRKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("UseRType", "R").SetValue(new StringList(new[] { "Default", "Flash in" })));
                custommenu.AddItem(new MenuItem("UseRDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("UseRDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("UseEComboType", "E Voidlings check").SetValue(true));
                combomenu.AddItem(new MenuItem("UseRComboType", "R").SetValue(new StringList(new[] { "Always", "Killable", "Voidlings" })));
                combomenu.AddItem(new MenuItem("NoRValue", "Don't R if > enemy").SetValue(new Slider(1, 1, 5)));
                
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
                laneclear.AddItem(new MenuItem("QFarmValue", "Q More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("WFarmValue", "W More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("EFarmType", "E").SetValue(new StringList(new[] { "Any", "Voidlings" })));
                laneclear.AddItem(new MenuItem("EFarmValue", "Voidlings >").SetValue(new Slider(0, 0, 3)));
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
                miscmenu.AddItem(new MenuItem("QPredHitchance", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("UseQMisc", "Q Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseEMisc", "E Gapcloser").SetValue(false));
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
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var UseR = config.Item("UseRCombo").GetValue<bool>();
            var UseEVoidlings = config.Item("UseEComboType").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToMagic(target)) return;
                try
                {
                    if (UseQ && Q.IsReady() && !Player.IsWindingUp)
                    {                        
                        QPredict(target);
                    }
                    if (UseW && W.IsReady() && !Player.IsWindingUp)
                    {
                        WPredict(target);
                    }
                    if (UseE && E.IsReady() && !Player.IsWindingUp)
                    {
                        if (UseEVoidlings)
                        {
                            if (NextCastSummons || VoidlingsTotal >= 1)
                            {
                                E.CastOnUnit(target);
                            }
                        }
                        else
                        {
                            E.CastOnUnit(target);
                        }
                    }
                    if (UseR && R.IsReady() && !Player.IsWindingUp)
                    {
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= R.Range && (Player.ServerPosition.CountEnemiesInRange(R.Range) - 1) < config.Item("NoRValue").GetValue<Slider>().Value)
                        {                            
                            switch (config.Item("UseRComboType").GetValue<StringList>().SelectedIndex)
                            {
                                case 0:
                                    R.CastOnUnit(target);
                                    break;
                                case 1:
                                    if (R.IsKillable(target)) R.CastOnUnit(target);
                                    break;
                                case 2:
                                    if (NextCastSummons || VoidlingsTotal >= 1) R.CastOnUnit(target);
                                    break;
                            }
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
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady() && Q.IsInRange(target))
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    Q.Cast(target.ServerPosition);

                }
                if (UseW && W.IsReady() && W.IsInRange(target))
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    WPredict(target);
                }
                if (UseE && E.IsReady() && E.IsInRange(target))
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    if (NextCastSummons || VoidlingsTotal >= 1)
                    {
                        E.CastOnUnit(target);
                    }
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (Player.UnderTurret(true)) return;
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
            {               
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (minionQ == null) return;
                var qpred = Q.GetCircularFarmLocation(minionQ);
                if (qpred.MinionsHit > config.Item("QFarmValue").GetValue<Slider>().Value)
                {
                    if (myUtility.IsFacing(Player, qpred.Position.To3D())) Q.Cast(qpred.Position);
                }
            }
            if (config.Item("UseWFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
            {                
                var minionW = MinionManager.GetMinions(Player.ServerPosition, W.Range);
                if (minionW == null) return;
                var wpred = W.GetCircularFarmLocation(minionW);
                if (wpred.MinionsHit > config.Item("WFarmValue").GetValue<Slider>().Value)
                {
                    W.Cast(wpred.Position);
                }
            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
            {                
                var minionE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (minionE == null) return;
                var siegeE = myUtility.GetLargeMinions(E.Range).FirstOrDefault(x => E.IsKillable(x));
                if (siegeE != null && siegeE.IsValidTarget())
                {
                    E.CastOnUnit(siegeE);
                }
                else
                {
                    switch (config.Item("EFarmType").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            var targets = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && x.IsTargetable && !myUtility.ImmuneToMagic(x) && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < (E.Range + 50)).ToList();
                            var test1 = targets.FirstOrDefault();
                            if (test1 != null)
                            {
                                var NearestE = minionE.OrderBy(x => Vector3.Distance(x.ServerPosition, test1.ServerPosition)).FirstOrDefault();
                                if (NearestE != null && NearestE.IsValidTarget())
                                {
                                    E.CastOnUnit(NearestE);
                                }
                            }
                            else
                            {
                                var SelectE = minionE.OrderBy(i => i.Health).FirstOrDefault(x => E.IsKillable(x) || (Player.GetAutoAttackDamage(x) + E.GetDamage(x) > x.Health));
                                if (SelectE != null && SelectE.IsValidTarget())
                                {
                                    E.CastOnUnit(SelectE);
                                }
                            }
                            break;
                        case 1:
                            if ((NextCastSummons && (VoidlingsTotal + 1 >= config.Item("EFarmValue").GetValue<Slider>().Value)) ||
                                VoidlingsTotal > config.Item("EFarmValue").GetValue<Slider>().Value)
                            {
                                var LowestE = minionE.OrderBy(i => i.Health).FirstOrDefault(x => E.IsKillable(x) || (Player.BaseAttackDamage + E.GetDamage(x) > x.Health));
                                if (LowestE != null && LowestE.IsValidTarget())
                                {
                                    E.CastOnUnit(LowestE);
                                }
                            }
                            break;
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
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && Q.IsInRange(mob) && !Player.IsWindingUp)
                {
                    if (largemobs != null)
                    {
                        Q.Cast(largemobs.ServerPosition);
                    }
                    else
                    {
                        var mobQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.Health);
                        if (mobQ == null) return;
                        var qpred = Q.GetCircularFarmLocation(mobQ);
                        if (qpred.MinionsHit > 0)
                        {
                            Q.Cast(qpred.Position);
                        }
                    }
                }
                if (config.Item("UseWJFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null)
                    {
                        W.Cast(largemobs.ServerPosition);
                    }
                    else
                    {
                        var mobW = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.Health);
                        if (mobW == null) return;
                        var wpred = W.GetCircularFarmLocation(mobW);
                        if (wpred.MinionsHit > 0)
                        {
                            W.Cast(wpred.Position);
                        }
                    }
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && E.IsInRange(mob) && !Player.IsWindingUp)
                {
                    if (largemobs != null)
                    {
                        E.CastOnUnit(largemobs);
                    }
                    else
                    {
                        E.CastOnUnit(mob);
                    }
                }
            }
        }
        private void Custom()
        {
            if (R.IsReady())
            {
                Obj_AI_Hero target;
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                switch (config.Item("UseRType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        if (TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() && 
                            Vector3.Distance(Player.ServerPosition, TargetSelector.GetSelectedTarget().ServerPosition) <= R.Range)
                        {
                            target = TargetSelector.GetSelectedTarget();
                        }
                        else
                        {
                            target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range)
                                .OrderByDescending(i => myRePriority.ResortDB(i.ChampionName))
                                .ThenBy(i => i.Health)
                                .FirstOrDefault();
                        }
                        if (target != null && target.IsValidTarget())
                        {
                            R.CastOnUnit(target);
                        }
                        break;
                    case 1:
                        if (FlashSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready)
                        {                            
                            if (TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() &&
                                Vector3.Distance(Player.ServerPosition, TargetSelector.GetSelectedTarget().ServerPosition) <= R.Range + 425f)
                            {
                                target = TargetSelector.GetSelectedTarget();
                            }
                            else
                            {
                                target = EnemyList.Where(x => !x.InFountain() && x.IsVisible && 
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) > 425f && 
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range + 425f)
                                    .OrderByDescending(i => myRePriority.ResortDB(i.ChampionName))
                                    .ThenBy(i => i.Health)
                                    .FirstOrDefault();
                            }
                            if (target != null && target.IsValidTarget())
                            {
                                R.UpdateSourcePosition(Player.ServerPosition.Extend(target.ServerPosition, 425f));
                                Player.Spellbook.CastSpell(FlashSlot, Player.ServerPosition.Extend(target.ServerPosition, 425f));
                                R.CastOnUnit(target);
                            }
                        }
                        break;
                }                
            }
        }

        private bool NextCastSummons
        {
            get { return Player.HasBuff("alzaharsummonvoidling"); }
        }
        private int VoidlingsTotal
        {
            get { return ObjectManager.Get<Obj_AI_Minion>().Count(minion => minion.IsValid && minion.IsAlly && minion.BaseSkinName.Contains("voidling")); }
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
                var test1 = Prediction.GetPrediction(target, Q.Instance.SData.SpellCastTime).CastPosition;
                float movement = target.MoveSpeed * 100 / 1000;
                if (target.Distance(test1) > movement) Q.Cast(target.ServerPosition.Extend(test1, Q.Instance.SData.SpellCastTime * target.MoveSpeed));
                else
                {
                    if (pred.Hitchance >= QHitChance) Q.Cast(pred.CastPosition);
                }
            }
        }        
        private void WPredict(Obj_AI_Base target)
        {
            var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where champ.IsValidTarget(W.Range) && target != champ select champ).ToList();
            if (nearChamps.Count > 0)
            {
                var closeToPrediction = new List<Obj_AI_Hero>();
                foreach (var enemy in nearChamps)
                {
                    PredictionOutput prediction = W.GetPrediction(enemy);
                    if (prediction.Hitchance >= HitChance.Medium && Vector3.Distance(Player.ServerPosition, enemy.ServerPosition) < 400f)
                    {
                        closeToPrediction.Add(enemy);
                    }
                }
                if (closeToPrediction.Count == 0)
                {
                    PredictionOutput pred = W.GetPrediction(target);
                    if (pred.Hitchance >= HitChance.High && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < W.Range)
                    {
                        W.Cast(pred.CastPosition.Extend(Player.ServerPosition, -100f));
                    }
                }
                else if (closeToPrediction.Count > 0)
                {
                    W.CastIfWillHit(target, closeToPrediction.Count, false);
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
        protected override void OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (Player.IsChannelingImportantSpell() || Player.IsCastingInterruptableSpell()) args.Process = false;
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("UseQMisc").GetValue<bool>() && Q.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) < Q.Range)
                {
                    if (myUtility.ImmuneToMagic(sender) || myUtility.ImmuneToCC(sender)) return;
                    PredictionOutput pred = Q.GetPrediction(sender);
                    var test1 = Prediction.GetPrediction(sender, Q.Instance.SData.SpellCastTime).CastPosition;
                    float movement = sender.MoveSpeed * 100 / 1000;
                    if (sender.Distance(test1) > movement) Q.Cast(sender.ServerPosition.Extend(test1, Q.Instance.SData.SpellCastTime * sender.MoveSpeed));
                    else
                    {
                        if (pred.Hitchance >= HitChance.High) Q.Cast(pred.CastPosition);
                    }
                }
            }
        }
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (E.IsReady() && config.Item("UseEFarm").GetValue<bool>() && 
                config.Item("EFarmType").GetValue<StringList>().SelectedIndex == 0 &&
                myUtility.PlayerManaPercentage > config.Item("FarmMana").GetValue<Slider>().Value)
            {
                var target = minion as Obj_AI_Base;
                if (target != null && 
                    E.IsKillable(target) && Player.GetAutoAttackDamage(target) < target.Health &&
                    !Player.IsWindingUp && 
                    Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= E.Range)
                {
                    E.CastOnUnit(target);
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
            if (R.Level > 0 && R.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                switch (config.Item("UseRType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                       if (config.Item("UseRDrawDistance").GetValue<bool>())
                        {
                            Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 7);
                        }
                        if (config.Item("UseRDrawTarget").GetValue<bool>())
                        {
                            var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range)
                                .OrderByDescending(i => myRePriority.ResortDB(i.ChampionName))
                                .ThenBy(i => i.Health)
                                .FirstOrDefault();
                            if (target != null && target.IsValidTarget())
                            {
                                Render.Circle.DrawCircle(target.Position, target.BoundingRadius, Color.Lime, 7);
                            }
                        }
                        break;
                    case 1:
                        if (FlashSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready)
                        {
                            if (config.Item("UseRDrawDistance").GetValue<bool>())
                            {
                                Render.Circle.DrawCircle(Player.Position, 425f, Color.Fuchsia, 7);
                                Render.Circle.DrawCircle(Player.Position, R.Range + 425f, Color.Fuchsia, 7);
                            }
                            if (config.Item("UseRDrawTarget").GetValue<bool>())
                            {
                                var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) > 425f &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range + 425f)
                                    .OrderByDescending(i => myRePriority.ResortDB(i.ChampionName))
                                    .ThenBy(i => i.Health)
                                    .FirstOrDefault();
                                if (target != null && target.IsValidTarget())
                                {
                                    Render.Circle.DrawCircle(target.Position, target.BoundingRadius, Color.Lime, 7);
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}
