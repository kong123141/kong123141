using System;
using System.Linq;
using EndifsCollections.Controller;
using EndifsCollections.SummonerSpells;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Evelynn : PluginData
    {
        public Evelynn()
        {
            LoadSpells();
            LoadMenus();
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 500);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 275);
            R = new Spell(SpellSlot.R, 600);

            R.SetSkillshot(R.Instance.SData.SpellCastTime, 250f, R.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Agony's Embrace", "Custom");
            {
                custommenu.AddItem(new MenuItem("UseRKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("UseRHitChecks", "Only if hits").SetValue(true));
                custommenu.AddItem(new MenuItem("UseRDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("UseRDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("UseItemCombo", "Use Items").SetValue(true));
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
                laneclear.AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));                
                laneclear.AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
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
                miscmenu.AddItem(new MenuItem("RPredHitchance", "R Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawE", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);            
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var CastItems = config.Item("UseItemCombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToPhysical(target)) return;
                if (!Stealth && mySmiter.CanSmiteChampions(target)) mySmiter.Smites(target);
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range)
                        {
                            if (myUtility.ImmuneToMagic(target)) return;
                            Q.Cast();
                        }                       
                    }
                    if (UseW && W.IsReady())
                    {
                        if (Stealth && Vector3.Distance(Player.ServerPosition, target.ServerPosition) > DetectedRange) return;
                        if (Player.HasBuffOfType(BuffType.Slow) && !Stealth) W.Cast();
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < DetectedRange) 
                        {
                            var dist = Vector3.Distance(Player.ServerPosition, target.ServerPosition);
                            var msDif = Player.MoveSpeed - target.MoveSpeed;
                            var reachIn = dist / msDif;
                            if (msDif < 0 && reachIn > 2)
                            {
                                W.Cast();
                            }
                            else if (msDif > 0 && reachIn > 3)
                            {
                                W.Cast();
                            }
                        }
                    }
                    if (UseE && E.IsReady())
                    {
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < E.Range)
                        {
                            E.CastOnUnit(target);
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
                catch
                {
                }
            }
        }
        private void Harass()
        {
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var UseE = config.Item("UseEHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget() && !myOrbwalker.IsWaiting() && !Player.IsWindingUp)
            {
                if (UseQ && Q.IsReady())
                {
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range)
                    {
                        Q.Cast();
                    }
                }
                if (UseE && E.IsReady())
                {
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < E.Range)
                    {
                        E.CastOnUnit(target);
                    }
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                if (Player.UnderTurret(true)) return;
                var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (allMinionsQ == null) return;
                var QKillable = allMinionsQ.Where(x => Q.IsKillable(x)).OrderBy(i => i.Distance(Player));
                if (QKillable.Any())
                {
                    Q.Cast();
                }
            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady())
            {
                var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (allMinionsE == null) return;
                var siegeE = myUtility.GetLargeMinions(E.Range).FirstOrDefault(x => E.IsKillable(x));
                if (siegeE != null && siegeE.IsValidTarget())
                {
                    E.CastOnUnit(siegeE);
                }
                else
                {
                    var selectE = allMinionsE.Where(x => E.IsKillable(x) && Player.BaseAttackDamage < x.Health).OrderByDescending(i => i.Health).FirstOrDefault();
                    if (selectE != null && selectE.IsValidTarget())
                    {
                        if (myUtility.IsFacing(Player, selectE.ServerPosition, 60)) E.CastOnUnit(selectE);
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
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < Q.Range)
                    {
                        Q.Cast();
                    }
                    if (Vector3.Distance(Player.ServerPosition, mob.ServerPosition) < Q.Range)
                    {
                        Q.Cast();
                    }
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) <= E.Range)
                    {
                        E.CastOnUnit(largemobs);
                    }
                    if (Vector3.Distance(Player.ServerPosition, mob.ServerPosition) <= E.Range)
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
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToMagic(x));
                if (TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget())
                {
                    target = TargetSelector.GetSelectedTarget();
                }
                else
                {
                    target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                             Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range)
                             .OrderByDescending(i => i.CountEnemiesInRange(250))
                             .FirstOrDefault();
                }
                if (target != null && target.IsValidTarget())
                {
                    PredictionOutput pred = R.GetPrediction(target);
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= R.Range)
                    {
                        if (pred.Hitchance >= RHitChance)
                        {
                            R.Cast(pred.CastPosition);
                        }
                    }
                }
            }
           
        }

        private bool Stealth
        {
            get { return Player.HasBuff("evelynnstealthmarker"); }             
        }
        private int DetectedRange
        {
            get { return 600; }
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
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Custom && (config.Item("UseRHitChecks").GetValue<bool>()))
            {
                if (args.Slot == SpellSlot.R && myUtility.SpellHits(R).Item1 == 0)
                {
                    args.Process = false;
                }
            }
        }
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() &&
                myUtility.PlayerManaPercentage > config.Item("FarmMana").GetValue<Slider>().Value)
            {
                var target = minion as Obj_AI_Base;
                if (target != null &&
                    E.IsKillable(target) && Player.GetAutoAttackDamage(target) < target.Health && 
                    !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                {
                    if (myUtility.IsFacing(Player, target.ServerPosition, 60)) E.CastOnUnit(target);
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
            if (R.IsReady() && R.Level > 0)
            {
                if (config.Item("UseRDrawDistance").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 7);
                }
                if (config.Item("UseRDrawTarget").GetValue<bool>())
                {
                    if (R.IsReady())
                    {
                        var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                        var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                             Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range)
                             .OrderByDescending(i => i.CountEnemiesInRange(250))
                             .FirstOrDefault();
                        if (target != null && target.IsValidTarget())
                        {
                            PredictionOutput pred = R.GetPrediction(target);
                            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= R.Range)
                            {
                                var num = target.CountEnemiesInRange(250);
                                Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.White, "Hits: " + num);
                                if (pred.Hitchance >= RHitChance)
                                {
                                    Render.Circle.DrawCircle(pred.CastPosition, R.Width, Color.Lime, 7);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}