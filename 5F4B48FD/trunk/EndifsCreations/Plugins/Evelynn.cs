using System;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.SummonerSpells;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
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
                custommenu.AddItem(new MenuItem("EC.Evelynn.UseRKey", "Key").SetValue(new KeyBind(Root.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("EC.Evelynn.UseRHitChecks", "Only if hits").SetValue(true));
                custommenu.AddItem(new MenuItem("EC.Evelynn.UseRDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("EC.Evelynn.UseRDrawDistance", "Draw Distance").SetValue(true));
                Root.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Evelynn.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Evelynn.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Evelynn.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Evelynn.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Evelynn.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Evelynn.Harass.E", "Use E").SetValue(true));
                Root.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Evelynn.Farm.Q", "Use Q").SetValue(true));                
                laneclearmenu.AddItem(new MenuItem("EC.Evelynn.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Evelynn.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                Root.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Evelynn.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Evelynn.Jungle.E", "Use E").SetValue(true));
                Root.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Evelynn.RPredHitchance", "R Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Evelynn.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Evelynn.Draw.E", "E").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);            

            var UseQ = Root.Item("EC.Evelynn.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Evelynn.Combo.W").GetValue<bool>();
            //var UseE = Root.Item("EC.Evelynn.Combo.E").GetValue<bool>();
            var CastItems = Root.Item("EC.Evelynn.Combo.Items").GetValue<bool>();
            if (UseQ && Q.IsReady())
            {
                mySpellcast.PointBlank(null, Q, Q.Range);
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;
                try
                {                   
                    if (UseW && W.IsReady())
                    {
                        if (Stealth && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) > DetectedRange) return;
                        if (Player.HasBuffOfType(BuffType.Slow) && !Stealth) W.Cast();
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) < DetectedRange) 
                        {
                            var dist = Vector3.Distance(Player.ServerPosition, Target.ServerPosition);
                            var msDif = Player.MoveSpeed - Target.MoveSpeed;
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
                    if (CastItems)
                    {
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= 450f)
                        {
                            myItemManager.UseItems(1, Target);
                        }
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) < 500f)
                        {
                            myItemManager.UseItems(3, null);
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
            var UseQ = Root.Item("EC.Evelynn.Harass.Q").GetValue<bool>();
            var UseE = Root.Item("EC.Evelynn.Harass.E").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget() && !myOrbwalker.Waiting && !Player.IsWindingUp)
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
            if (myUtility.PlayerManaPercentage < Root.Item("EC.Evelynn.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (Root.Item("EC.Evelynn.Farm.Q").GetValue<bool>() && Q.IsReady())
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
            if (Root.Item("EC.Evelynn.Farm.E").GetValue<bool>() && E.IsReady())
            {
                var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (allMinionsE == null) return;
                var siegeE = myFarmManager.GetLargeMinions(E.Range).FirstOrDefault(x => E.IsKillable(x));
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
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (Root.Item("EC.Evelynn.Jungle.Q").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
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
                if (Root.Item("EC.Evelynn.Jungle.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
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
            switch (Root.Item("EC.Evelynn.RPredHitchance").GetValue<StringList>().SelectedIndex)
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
            if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Custom && (Root.Item("EC.Evelynn.UseRHitChecks").GetValue<bool>()))
            {
                if (args.Slot == SpellSlot.R && myUtility.SpellHits(R).Item1 == 0)
                {
                    args.Process = false;
                }
            }
        }
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (Root.Item("EC.Evelynn.Farm.E").GetValue<bool>() && E.IsReady() &&
                myUtility.PlayerManaPercentage > Root.Item("EC.Evelynn.Farm.ManaPercent").GetValue<Slider>().Value)
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
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (Root.Item("EC.Evelynn.Combo.Items").GetValue<bool>())
                    {
                        myItemManager.UseItems(0, null);
                        myItemManager.UseItems(2, null);
                    }
                    if (Root.Item("EC.Evelynn.Combo.E").GetValue<bool>())
                    {
                        E.CastOnUnit(args.Target);
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Evelynn.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Evelynn.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (R.IsReady() && R.Level > 0)
            {
                if (Root.Item("EC.Evelynn.UseRDrawDistance").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 7);
                }
                if (Root.Item("EC.Evelynn.UseRDrawTarget").GetValue<bool>())
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