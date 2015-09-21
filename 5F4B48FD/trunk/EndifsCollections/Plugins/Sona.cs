using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Sona : PluginData
    {
        public Sona()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 850);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 1000);

            R.SetSkillshot(R.Instance.SData.SpellCastTime, R.Instance.SData.LineWidth, R.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotLine); 

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Cresendo", "Custom");
            {
                custommenu.AddItem(new MenuItem("UseFCKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("UseFCType", "R").SetValue(new StringList(new[] { "Cresendo", "Flash Cresendo" })));
                custommenu.AddItem(new MenuItem("UseFCDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("UseFCDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(false));                
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {               
                harassmenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclear = new Menu("Farm", "Farm");
            {
                laneclear.AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
                laneclear.AddItem(new MenuItem("FarmMana", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclear);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("RPredHitchance", "R Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("UseWMisc", "W Heals").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseW2Misc", "W Shields").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseRMisc", "R Interrupts").SetValue(false));
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
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var UseR = config.Item("UseRCombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToMagic(target)) return;
                try
                {
                    if (UseQ && Q.IsReady() && Q.IsInRange(target))
                    {                    
                        Q.Cast();
                    }
                    if (UseE && E.IsReady())
                    {
                        var dist = Vector3.Distance(Player.ServerPosition, target.ServerPosition);
                        var msDif = Player.MoveSpeed - target.MoveSpeed;
                        var reachIn = dist / msDif;
                        if (msDif < 0 && reachIn > 2)
                        {
                            E.Cast();
                        }
                        else if (msDif > 0 && reachIn > 3)
                        {
                           E.Cast();
                        }
                    }
                    if (UseR && R.IsReady())
                    {
                        if (myUtility.ImmuneToCC(target)) return;
                        RPredict(target);
                    }
                }
                catch { }
            }           
        }
        private void Harass()
        {
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady())
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    Q.Cast();
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (Player.UnderTurret(true)) return;
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range).Where(x => Q.IsKillable(x));
                if (minionQ.Any()) Q.Cast();
            }          
        }        
        private void Custom() 
        {
            if (R.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                switch (config.Item("UseFCType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        Obj_AI_Hero target; 
                        if (TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget())
                        {
                            target = TargetSelector.GetSelectedTarget();
                        }
                        else 
                        { 
                            target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                     Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range
                                     ).OrderByDescending(i => i.CountEnemiesInRange(R.Width)).FirstOrDefault();
                        }
                        if (target != null && target.IsValidTarget())
                        {
                            RPredict(target);
                        }
                        break;
                    case 1:
                        if (FlashSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready)
                        {
                            var FC = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range + 425f
                                ).OrderByDescending(i => i.CountEnemiesInRange(R.Width)).FirstOrDefault();
                            if (FC == null) return;
                            R.UpdateSourcePosition(Player.ServerPosition.Extend(FC.ServerPosition, 425f));
                            Player.Spellbook.CastSpell(FlashSlot, Player.ServerPosition.Extend(FC.ServerPosition, 425f));
                            R.Cast(FC.ServerPosition);
                        }
                        break;
                }
            }            
        }
        private void AutoHeal()
        {
            if (!config.Item("UseWMisc").GetValue<bool>() || Player.InFountain() || Player.InShop() || Player.HasBuff("Recall") || Player.IsWindingUp) return;
            if (myUtility.PlayerManaPercentage < 25) return;
            if (W.IsReady())
            {
                double wHeal = (10 + 20 * W.Level + .2 * Player.FlatMagicDamageMod) * (1 + (1 - (Player.Health / Player.MaxHealth)) / 2);
                if (Player.MaxHealth - Player.Health > wHeal * 2) W.Cast();
            }
        }

        private HitChance RHitChance
        {
            get
            {
                return GetRHitChance();
            }
        }
        private HitChance GetRHitChance()
        {
            switch (config.Item("RPredHitchance").GetValue<StringList>().SelectedIndex)
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
        private void RPredict(Obj_AI_Hero target)
        {
            PredictionOutput pred = R.GetPrediction(target);
            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= R.Range)
            {
                if (pred.Hitchance >= RHitChance)
                {
                    R.Cast(target.Position);
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
                    AutoHeal();
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
                case myOrbwalker.OrbwalkingMode.Custom:
                    Custom();
                    break;
            }            
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (!unit.IsMe && unit.IsEnemy && unit.IsValid<Obj_AI_Hero>())
            {
                if (spell.Target == null || !spell.Target.IsValid || !spell.Target.IsMe)
                {
                    return;
                }
                if (spell.Target.IsMe && W.IsReady())
                {
                    if (unit.IsChampion(unit.BaseSkinName))
                    {
                        if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                        {
                            if (config.Item("UseWCombo").GetValue<bool>())
                            {
                                 Utility.DelayAction.Add(myUtility.RandomDelay(0, 200), () => W.Cast());
                            }
                        }
                        if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass)
                        {
                            if (config.Item("UseWHarass").GetValue<bool>())
                            {
                                 Utility.DelayAction.Add(myUtility.RandomDelay(0, 200), () => W.Cast());
                            }
                        }
                        if (config.Item("UseW2Misc").GetValue<bool>())
                        {
                             Utility.DelayAction.Add(myUtility.RandomDelay(0, 200), () => W.Cast());
                        }
                    }
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("UseRMisc").GetValue<bool>() && R.IsReady())
            {
                if (sender.IsEnemy && args.DangerLevel == Interrupter2.DangerLevel.High)
                {
                    if (myUtility.ImmuneToMagic(sender) || myUtility.ImmuneToCC(sender)) return;
                    RPredict(sender);
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
            if (R.Level > 0 && R.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                switch (config.Item("UseFCType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        if (config.Item("UseFCDrawDistance").GetValue<bool>())
                        {
                            Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 7);
                        }
                        if (config.Item("UseFCDrawTarget").GetValue<bool>())
                        {
                            Obj_AI_Hero target;
                            if (TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget())
                            {
                                target = TargetSelector.GetSelectedTarget();
                            }
                            else
                            {
                                target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                         Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range
                                         ).OrderByDescending(i => i.CountEnemiesInRange(R.Width)).FirstOrDefault();
                            }
                            if (target != null && target.IsValidTarget())
                            {
                                PredictionOutput pred = R.GetPrediction(target);
                                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= R.Range)
                                {
                                    if (pred.Hitchance >= RHitChance)
                                    {
                                        Render.Circle.DrawCircle(target.Position, target.BoundingRadius, Color.Lime, 7);
                                    }
                                }
                            }
                        }
                        break;
                    case 1:
                        if (FlashSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready)
                        {
                            if (config.Item("UseFCDrawDistance").GetValue<bool>())
                            {
                                Render.Circle.DrawCircle(Player.Position, 425f, Color.Fuchsia, 7);
                                Render.Circle.DrawCircle(Player.Position, R.Range + 425f, Color.Fuchsia, 7);
                            }
                            if (config.Item("UseFCDrawTarget").GetValue<bool>())
                            {

                                var FC = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range + 425f
                                    ).OrderByDescending(i => i.CountEnemiesInRange(R.Width)).FirstOrDefault();
                                if (FC != null && FC.IsValidTarget())
                                {
                                    Render.Circle.DrawCircle(FC.Position, FC.BoundingRadius, Color.Lime, 7);
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}
