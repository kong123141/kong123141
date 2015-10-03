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
                custommenu.AddItem(new MenuItem("EC.KogMaw.UseRKey", "Key").SetValue(new KeyBind(Root.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T                
                custommenu.AddItem(new MenuItem("EC.KogMaw.UseRConserve", "Conserve Mana").SetValue(true));
                custommenu.AddItem(new MenuItem("EC.KogMaw.UseRConserveValue", "Up to x stacks").SetValue(new Slider(1, 1, 10)));
                custommenu.AddItem(new MenuItem("EC.KogMaw.UseRDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("EC.KogMaw.UseRDrawDistance", "Draw Distance").SetValue(true));
                Root.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.KogMaw.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.KogMaw.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.KogMaw.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.KogMaw.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.KogMaw.Combo.RBool", "(R) Conserve Mana").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.KogMaw.Combo.RValue", "Up to x stacks").SetValue(new Slider(1, 1, 10)));
                Root.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {               
                harassmenu.AddItem(new MenuItem("EC.KogMaw.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.KogMaw.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.KogMaw.Harass.E", "Use E").SetValue(true));
                Root.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.KogMaw.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.KogMaw.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.KogMaw.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.KogMaw.UseRFarm", "Use R").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.KogMaw.Farm.E.Value", "E More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.KogMaw.RFarmValue", "R More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.KogMaw.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                Root.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.KogMaw.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.KogMaw.Jungle.W", "Use W").SetValue(true));          
                junglemenu.AddItem(new MenuItem("EC.KogMaw.Jungle.E", "Use E").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.KogMaw.UseRJFarm", "Use R").SetValue(true));
                Root.AddSubMenu(junglemenu);
            }  
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.KogMaw.KPredHitchance", "Spells Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.KogMaw.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.KogMaw.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.KogMaw.Draw.E", "E").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            var UseQ = Root.Item("EC.KogMaw.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.KogMaw.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.KogMaw.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.KogMaw.Combo.R").GetValue<bool>();
            var Conserve = Root.Item("EC.KogMaw.Combo.RBool").GetValue<bool>();
            if (UseR) Custom(Conserve, Target);
            if (UseW && W.IsReady() && Player.CountEnemiesInRange(W.Range) >  0)
            {
                W.Cast();
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        mySpellcast.Linear(Target, Q, HitChance.High, true);
                    }                    
                    if (UseE && E.IsReady())
                    {
                        mySpellcast.LinearRandomized(Target, E, HitChance.High, false, true);
                    }
                }
                catch { }
            }
        }
        private void Harass()
        {
            var UseQ = Root.Item("EC.KogMaw.Harass.Q").GetValue<bool>();
            var UseW = Root.Item("EC.KogMaw.Harass.W").GetValue<bool>();
            var UseE = Root.Item("EC.KogMaw.Harass.E").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget())
            {
                if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                if (UseQ && Q.IsReady())
                {
                    mySpellcast.Linear(target, Q, HitChance.High, true);
                }
                if (UseW && W.IsReady() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= W.Range)
                {
                    W.Cast();
                }
                if (UseE && E.IsReady())
                {
                    mySpellcast.LinearRandomized(target, E, HitChance.High, false, true);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < Root.Item("EC.KogMaw.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (Root.Item("EC.KogMaw.Farm.Q").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp && !myOrbwalker.Waiting)
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
            if (Root.Item("EC.KogMaw.Farm.E").GetValue<bool>() && E.IsReady())
            {
                if (Player.UnderTurret(true)) return;
                var MinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                var ELine = E.GetLineFarmLocation(MinionsE);
                if (ELine.Position.IsValid() && Vector3.Distance(Player.ServerPosition, ELine.Position.To3D()) > Player.AttackRange)
                {
                    if (ELine.MinionsHit > Root.Item("EC.KogMaw.Farm.E.Value").GetValue<Slider>().Value)
                    {
                        if (myUtility.IsFacing(Player, ELine.Position.To3D())) E.Cast(ELine.Position);
                    }
                }
            }
            if (Root.Item("EC.KogMaw.UseRFarm").GetValue<bool>() && R.IsReady() && !Player.IsWindingUp && R.Instance.ManaCost <= 40)
            {
                if (Player.UnderTurret(true)) return;
                var minionR = MinionManager.GetMinions(Player.ServerPosition, R.Range);
                if (minionR == null) return;
                var rpred = R.GetCircularFarmLocation(minionR);
                if (rpred.MinionsHit > Root.Item("EC.KogMaw.RFarmValue").GetValue<Slider>().Value)
                {
                    R.Cast(rpred.Position);
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
                if (Root.Item("EC.KogMaw.Jungle.Q").GetValue<bool>() && Q.IsReady())
                {
                    if (largemobs != null)
                    {
                        Q.Cast(largemobs.ServerPosition);
                    }
                    if (myUtility.IsFacing(Player, mob.ServerPosition, 70)) Q.Cast(mob);
                }
                if (Root.Item("EC.KogMaw.Jungle.E").GetValue<bool>() && E.IsReady())
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
                if (Root.Item("EC.KogMaw.UseRJFarm").GetValue<bool>() && R.IsReady() && R.Instance.ManaCost <= 40)
                {
                    var MobsR = MinionManager.GetMinions(Player.ServerPosition, R.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
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
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && conserve && R.Instance.ManaCost > (40 * Root.Item("EC.KogMaw.Combo.RValue").GetValue<Slider>().Value)) return;
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Custom && Root.Item("EC.KogMaw.UseRConserve").GetValue<bool>() && R.Instance.ManaCost > (40 * Root.Item("EC.KogMaw.UseRConserveValue").GetValue<Slider>().Value)) return;
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToMagic(x) && !myUtility.ImmuneToDeath(x));
                if (selected != null && selected.IsValid<Obj_AI_Hero>())
                {
                    target = selected;
                }
                else
                {
                    target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ?
                        TargetSelector.GetSelectedTarget() :
                        EnemyList.Where(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range).OrderByDescending(z => myRePriority.ResortDB(z.ChampionName)).ThenBy(i => i.Health).FirstOrDefault();
                }
                if (target == null) return;
                mySpellcast.CircularPrecise(target, R, KHitChance, R.Range, 100);
            }
        }
     
        private void UpdateW()
        {
            if (W.Level > 0)
            {
                W.Range = 500 + 110 + (W.Level * 20);
            }
        }
        private void UpdateR()
        {
            if (R.Level > 0)
            {
               R.Range = 900 + (R.Level * 300);
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
            switch (Root.Item("EC.KogMaw.KPredHitchance").GetValue<StringList>().SelectedIndex)
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
            UpdateW();
            UpdateR();
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
                    Root.Item("EC.KogMaw.Farm.W").GetValue<bool>() &&
                    !Player.IsWindingUp &&
                    W.IsReady())
                {
                    W.Cast();
                }
            }
            if (args.Target is Obj_AI_Minion && args.Target.Team == GameObjectTeam.Neutral)
            {
                if (!args.Target.Name.Contains("Mini") &&
                    Root.Item("EC.KogMaw.Jungle.W").GetValue<bool>() &&
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
            if (Root.Item("EC.KogMaw.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.KogMaw.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.KogMaw.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (R.Level > 0)
            {
                if (Root.Item("EC.KogMaw.UseRDrawDistance").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 7);
                }
                if (Root.Item("EC.KogMaw.UseRDrawTarget").GetValue<bool>())
                {
                    var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToMagic(x) && !myUtility.ImmuneToDeath(x));
                    var target = EnemyList.Where(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range).OrderByDescending(z => myRePriority.ResortDB(z.ChampionName)).ThenBy(i => i.Health).FirstOrDefault();
                    if (target == null) return;
                    PredictionOutput pred = R.GetPrediction(target);
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= R.Range)
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
