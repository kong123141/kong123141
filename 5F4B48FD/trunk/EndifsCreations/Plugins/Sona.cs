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

            myDamageIndicator.DamageToUnit = GetDamage;
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Cresendo", "Custom");
            {
                custommenu.AddItem(new MenuItem("EC.Sona.UseFCKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("EC.Sona.UseFCType", "R").SetValue(new StringList(new[] { "Cresendo", "Flash Cresendo" })));
                custommenu.AddItem(new MenuItem("EC.Sona.UseFCDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("EC.Sona.UseFCDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Sona.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Sona.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Sona.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Sona.Combo.R", "Use R").SetValue(false));                
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {               
                harassmenu.AddItem(new MenuItem("EC.Sona.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Sona.Harass.W", "Use W").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Sona.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Sona.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclearmenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Sona.RPredHitchance", "R Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("EC.Sona.Misc.W", "W Heals").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Sona.Misc.W2", "W Shields").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Sona.UseRMisc", "R Interrupts").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Sona.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Sona.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Sona.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Sona.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Sona.Combo.Q").GetValue<bool>();            
            var UseE = config.Item("EC.Sona.Combo.E").GetValue<bool>();
            var UseR = config.Item("EC.Sona.Combo.R").GetValue<bool>();
            if (UseQ && Q.IsReady())
            {
                if (Target.IsValidTarget() && Q.IsInRange(Target))
                {
                    if (myUtility.ImmuneToMagic(Target)) return;
                    Q.Cast();
                }
                else
                {
                    if (Player.CountEnemiesInRange(800) > 0)
                    {
                        Q.Cast();
                    }
                }
            }
            if (UseR && R.IsReady())
            {
                mySpellcast.LinearBox(R, HitChance.High, 5);
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                try
                {                   
                    if (UseE && E.IsReady())
                    {
                        var dist = Vector3.Distance(Player.ServerPosition, Target.ServerPosition);
                        var msDif = Player.MoveSpeed - Target.MoveSpeed;
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
                    
                }
                catch { }
            }           
        }
        private void Harass()
        {
            var UseQ = config.Item("EC.Sona.Harass.Q").GetValue<bool>();
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
            if (myUtility.PlayerManaPercentage < config.Item("EC.Sona.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (Player.UnderTurret(true)) return;
            if (config.Item("EC.Sona.Farm.Q").GetValue<bool>() && Q.IsReady())
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
                switch (config.Item("EC.Sona.UseFCType").GetValue<StringList>().SelectedIndex)
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
            if (!config.Item("EC.Sona.Misc.W").GetValue<bool>() || Player.InFountain() || Player.InShop() || Player.HasBuff("Recall") || Player.IsWindingUp) return;
            if (myUtility.PlayerManaPercentage < 25) return;
            if (W.IsReady())
            {
                double wHeal = (10 + 20 * W.Level + .2 * Player.FlatMagicDamageMod) * (1 + (1 - (Player.Health / Player.MaxHealth)) / 2);
                if (Player.MaxHealth - Player.Health > wHeal * 2) W.Cast();
            }
        }

        private float GetDamage(Obj_AI_Hero target)
        {
            var damage = 0d;
            if (Q.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.Q);
            }
            if (R.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.R);
            }
            return (float)damage;
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
            switch (config.Item("EC.Sona.RPredHitchance").GetValue<StringList>().SelectedIndex)
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
            if (unit is Obj_AI_Hero && unit.IsEnemy && !spell.SData.IsAutoAttack() && W.IsReady())
            {
                if ((myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && config.Item("EC.Sona.Combo.W").GetValue<bool>()) ||
                    (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass && config.Item("EC.Sona.Harass.W").GetValue<bool>()) ||
                    (config.Item("EC.Sona.Misc.W2").GetValue<bool>())
                    )
                {
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        var box = new Geometry.Polygon.Rectangle(spell.Start, spell.End, Player.BoundingRadius);
                        if (box.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 100))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast());
                        }
                    }
                    else if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Unit) || spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit)) && spell.Target != null && spell.Target.IsMe)
                    {
                        W.Cast();
                    }
                    else if (spell.End.Distance(Player.ServerPosition) <= 100)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast());
                    }
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("EC.Sona.UseRMisc").GetValue<bool>() && R.IsReady())
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
            if (config.Item("EC.Sona.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Sona.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Sona.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (config.Item("EC.Sona.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.White);
            }
            if (R.Level > 0 && R.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                switch (config.Item("EC.Sona.UseFCType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        if (config.Item("EC.Sona.UseFCDrawDistance").GetValue<bool>())
                        {
                            Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 7);
                        }
                        if (config.Item("EC.Sona.UseFCDrawTarget").GetValue<bool>())
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
                            if (config.Item("EC.Sona.UseFCDrawDistance").GetValue<bool>())
                            {
                                Render.Circle.DrawCircle(Player.Position, 425f, Color.Fuchsia, 7);
                                Render.Circle.DrawCircle(Player.Position, R.Range + 425f, Color.Fuchsia, 7);
                            }
                            if (config.Item("EC.Sona.UseFCDrawTarget").GetValue<bool>())
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
