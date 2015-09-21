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
    class Kassadin : PluginData
    {
        public Kassadin()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, 200);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 500);

            Q.SetTargetted(0.5f, 1400f);
            //E.SetSkillshot(0.25f, 15f * 2 * (float)Math.PI / 180, 2000f, false, SkillshotType.SkillshotCone);
            E.SetSkillshot(0.25f, 80*(float)Math.PI / 90, 2000f, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(R.Instance.SData.SpellCastTime, 150f, float.MaxValue, false, SkillshotType.SkillshotCircle);        

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Kassadin.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Kassadin.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Kassadin.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Kassadin.Combo.R", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Kassadin.Combo.RType", "R").SetValue(new StringList(new[] { "Always", "E check" })));
                combomenu.AddItem(new MenuItem("EC.Kassadin.NoRValue", "Don't R if > enemy").SetValue(new Slider(1, 1, 5)));
                combomenu.AddItem(new MenuItem("EC.Kassadin.Combo.Dive", "Turret Dive").SetValue(false));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {               
                harassmenu.AddItem(new MenuItem("EC.Kassadin.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Kassadin.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Kassadin.Harass.E", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Kassadin.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Kassadin.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Kassadin.UseWLastHit", "(Last Hit) Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Kassadin.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Kassadin.Farm.E.Value", "E More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Kassadin.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Kassadin.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Kassadin.Jungle.W", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Kassadin.Jungle.E", "Use E").SetValue(true));
                config.AddSubMenu(junglemenu);
            }  
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Kassadin.Misc.Q", "Q Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Kassadin.Misc.E", "E Gapcloser").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Kassadin.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Kassadin.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Kassadin.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Kassadin.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Kassadin.Combo.Q").GetValue<bool>();
            var UseE = config.Item("EC.Kassadin.Combo.E").GetValue<bool>();
            var UseR = config.Item("EC.Kassadin.Combo.R").GetValue<bool>();           
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToMagic(Target)) return;                
                try
                {
                    if (UseR && R.IsReady())
                    {
                        if (Target.UnderTurret(true) && !config.Item("EC.Kassadin.Combo.Dive").GetValue<bool>()) return;
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= R.Range && (Target.ServerPosition.CountEnemiesInRange(R.Range) - 1) < config.Item("EC.Kassadin.NoRValue").GetValue<Slider>().Value)
                        {
                            Vector3 pos = Player.ServerPosition.Extend(Target.ServerPosition, Vector3.Distance(Player.ServerPosition, Target.ServerPosition));
                            if (R.Instance.ManaCost < 300f)
                            {                                
                                switch (config.Item("EC.Kassadin.Combo.RType").GetValue<StringList>().SelectedIndex)
                                {
                                    case 0:
                                        mySpellcast.LinearVector(pos, R, Target.BoundingRadius);
                                        break;
                                    case 1:
                                        if ((ECanCast || EBuffCount >= 3))
                                        {
                                            mySpellcast.LinearVector(pos, R, Target.BoundingRadius);
                                        }
                                        break;
                                }
                            }
                            if (R.Instance.ManaCost > 300f && R.IsKillable(Target) && Player.Mana > R.Instance.ManaCost)
                            {
                                mySpellcast.LinearVector(pos, R, Target.BoundingRadius);
                            }
                        }
                    }
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.Unit(Target, Q);
                    }
                    if (UseE && E.IsReady() && ECanCast && !Player.IsDashing() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.Linear(Target, E, HitChance.High);
                    }   
                }
                catch { }
            }
            
        }
        private void Harass()
        {
            var UseQ = config.Item("EC.Kassadin.Harass.Q").GetValue<bool>();
            var UseE = config.Item("EC.Kassadin.Harass.E").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady())
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    mySpellcast.Unit(target, Q);

                }
                if (UseE && E.IsReady() && ECanCast)
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= E.Range) E.Cast(target.ServerPosition);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("EC.Kassadin.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (config.Item("EC.Kassadin.Farm.Q").GetValue<bool>() && Q.IsReady())
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (minionQ == null) return;
                var siegeQ = myFarmManager.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                if (siegeQ != null && siegeQ.IsValidTarget())
                {
                    Q.CastOnUnit(siegeQ);
                }
                else
                {
                    var FurthestQ = minionQ.OrderByDescending(i => i.Distance(Player)).FirstOrDefault(x => Q.IsKillable(x));
                    if (FurthestQ != null && FurthestQ.IsValidTarget())
                    {
                        if (myUtility.IsFacing(Player, FurthestQ.ServerPosition, 60)) Q.CastOnUnit(FurthestQ);
                    }
                }                
            }
            if (config.Item("EC.Kassadin.Farm.E").GetValue<bool>() && E.IsReady() && ECanCast)
            {
                if (Player.UnderTurret(true)) return;
                var minionE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (minionE == null) return;
                var siegeE = myFarmManager.GetLargeMinions(E.Range).FirstOrDefault(x => E.IsKillable(x));
                if (siegeE != null && siegeE.IsValidTarget())
                {
                    E.Cast(siegeE.ServerPosition);
                }
                else if (minionE.Count() > 1)
                {
                    foreach (var x in minionE)
                    {
                        if (MinionManager.GetMinions(x.ServerPosition, 275).Count() > config.Item("EC.Kassadin.Farm.E.Value").GetValue<Slider>().Value)
                        {
                            if (x.IsValidTarget() && x.ServerPosition.IsValid()) E.Cast(x.ServerPosition);
                        }
                    }
                }
                else
                {
                    var SelectE = minionE.Where(x => E.IsKillable(x)).OrderByDescending(i => i.Distance(Player)).FirstOrDefault();
                    if (SelectE != null && SelectE.IsValidTarget())
                    {
                        E.Cast(SelectE.ServerPosition);
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
                if (config.Item("EC.Kassadin.Jungle.Q").GetValue<bool>() && Q.IsReady() && Q.IsInRange(mob))
                {
                    if (largemobs != null)
                    {
                        Q.CastOnUnit(largemobs);
                    }
                    else
                    {
                        Q.CastOnUnit(mob);
                    }
                }
                if (config.Item("EC.Kassadin.Jungle.E").GetValue<bool>() && E.IsReady() && ECanCast)
                {
                    if (largemobs != null)
                    {
                        E.Cast(largemobs);
                    }
                    else
                    {
                        if (myUtility.IsFacing(Player, mob.ServerPosition, 75)) E.Cast(mob);
                    }
                }
            }
        }
        private void LastHit()
        {
            if (myOrbwalker.IsWaiting() && !Player.IsWindingUp && config.Item("EC.Kassadin.UseWLastHit").GetValue<bool>() && W.IsReady())
            {
                W.Cast();
            }
        }
        
        private bool ECanCast
        {
            get { return Player.HasBuff("forcepulsecancast"); }
        }
        private int EBuffCount
        {
            get { return Player.Buffs.Count(x => x.Name == "forcepulsecounter"); }
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
                case myOrbwalker.OrbwalkingMode.Lasthit:
                    LastHit();
                    break;
            }            
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.E && Player.IsDashing())
            {
                args.Process = false;
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (sender.IsEnemy && config.Item("EC.Kassadin.Misc.Q").GetValue<bool>() && Q.IsReady())
            {
                if (myUtility.ImmuneToMagic(sender)) return;
                Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Unit(sender, Q));

            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "nullance") || (spell.SData.Name.ToLower() == "forcepulse") )
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("EC.Kassadin.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.Cast(gapcloser.End));
                }
            }
        }
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (config.Item("EC.Kassadin.Farm.W").GetValue<bool>() && W.IsReady())
            {
                var target = minion as Obj_AI_Base;
                if (target != null && 
                    W.IsKillable(target) &&
                    !Player.IsWindingUp &&
                    Vector3.Distance(Player.ServerPosition, target.ServerPosition) < 200)
                {
                    W.Cast();
                }
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.IsValidTarget() && args.Target.IsEnemy)
            {                
                if ((myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && config.Item("EC.Kassadin.Combo.W").GetValue<bool>() ||
                    myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass && config.Item("EC.Kassadin.Harass.W").GetValue<bool>()) &&                   
                    W.IsReady())
                {
                    W.Cast();
                }
            }            
            if (args.Target is Obj_AI_Minion)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear &&                    
                    config.Item("EC.Kassadin.Farm.W").GetValue<bool>() &&
                    !Player.IsWindingUp &&
                    W.IsReady())
                {
                    W.Cast();
                }
            }
            if (args.Target is Obj_AI_Minion && args.Target.Team == GameObjectTeam.Neutral)
            {
                if (!args.Target.Name.Contains("Mini") &&
                    config.Item("EC.Kassadin.Jungle.W").GetValue<bool>() &&
                    !Player.IsWindingUp &&
                    W.IsReady())
                {
                    W.Cast();
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
                    if (config.Item("EC.Kassadin.Combo.W").GetValue<bool>() &&
                        !Player.IsWindingUp &&
                        W.IsReady() && 
                        target.IsValidTarget()) W.Cast();
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Kassadin.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Kassadin.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Kassadin.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (config.Item("EC.Kassadin.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
