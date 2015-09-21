using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
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
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 500);

            Q.SetTargetted(0.5f, 1400f);
            //E.SetSkillshot(0.25f, 15f * 2 * (float)Math.PI / 180, 2000f, false, SkillshotType.SkillshotCone);
            E.SetSkillshot(0.25f, 80*(float)Math.PI / 180, 2000f, false, SkillshotType.SkillshotCone);
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
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("UseRComboType", "R").SetValue(new StringList(new[] { "Always", "E check" })));
                combomenu.AddItem(new MenuItem("NoRValue", "Don't R if > enemy").SetValue(new Slider(1, 1, 5)));
                combomenu.AddItem(new MenuItem("TurretDive", "Turret Dive").SetValue(false));
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
                laneclear.AddItem(new MenuItem("UseWLastHit", "(Last Hit) Use W").SetValue(true));
                laneclear.AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
                laneclear.AddItem(new MenuItem("EFarmValue", "E More Than").SetValue(new Slider(1, 1, 5)));
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
                miscmenu.AddItem(new MenuItem("UseQMisc", "Q Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseEMisc", "E Gapcloser").SetValue(false));
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
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var UseR = config.Item("UseRCombo").GetValue<bool>();           
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToMagic(target)) return;
                
                try
                {
                    if (UseR && R.IsReady())
                    {
                        if (target.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= R.Range && (target.ServerPosition.CountEnemiesInRange(R.Range) - 1) < config.Item("NoRValue").GetValue<Slider>().Value)
                        {
                            if (R.Instance.ManaCost < 300f)
                            {
                                switch (config.Item("UseRComboType").GetValue<StringList>().SelectedIndex)
                                {
                                    case 0:
                                        R.Cast(target.ServerPosition);
                                        break;
                                    case 1:
                                        if ((ECanCast || EBuffCount >= 3))
                                        {
                                            R.Cast(target.ServerPosition);
                                        }
                                        break;
                                }
                            }
                            if (R.Instance.ManaCost > 300f && R.IsKillable(target) && Player.Mana > R.Instance.ManaCost)
                            {
                                R.Cast(target.ServerPosition);
                            }
                        }
                    }
                    if (UseQ && Q.IsReady() && Q.IsInRange(target))
                    {
                        Q.CastOnUnit(target);
                    }
                    if (UseE && E.IsReady() && ECanCast && !Player.IsDashing())
                    {
                        E.CastIfHitchanceEquals(target, HitChance.High);
                    }   
                }
                catch { }
            }
            
        }
        private void Harass()
        {
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var UseE = config.Item("UseEHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady() && Q.IsInRange(target))
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    Q.CastOnUnit(target);

                }
                if (UseE && E.IsReady() && ECanCast)
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < E.Range) E.Cast(target.ServerPosition);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (minionQ == null) return;
                var siegeQ = myUtility.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
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
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && ECanCast)
            {
                if (Player.UnderTurret(true)) return;
                var minionE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (minionE == null) return;
                var siegeE = myUtility.GetLargeMinions(E.Range).FirstOrDefault(x => E.IsKillable(x));
                if (siegeE != null && siegeE.IsValidTarget())
                {
                    E.Cast(siegeE.ServerPosition);
                }
                else if (minionE.Count() > 1)
                {
                    foreach (var x in minionE)
                    {
                        if (MinionManager.GetMinions(x.ServerPosition, 275).Count() > config.Item("EFarmValue").GetValue<Slider>().Value)
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
            var largemobs = myUtility.GetLargeMonsters(Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && Q.IsInRange(mob))
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
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && ECanCast)
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
            if (myOrbwalker.IsWaiting() && !Player.IsWindingUp && config.Item("UseWLastHit").GetValue<bool>() && W.IsReady())
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
            if (config.Item("UseQMisc").GetValue<bool>() && Q.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) < Q.Range)
                {
                    if (myUtility.ImmuneToMagic(sender)) return;
                    Q.CastOnUnit(sender);
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("UseEMisc").GetValue<bool>() && E.IsReady())
            {
                if (Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) < E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    E.Cast(gapcloser.Sender.ServerPosition);
                }
            }
        }
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (config.Item("UseWFarm").GetValue<bool>() && W.IsReady())
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
                if ((myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && config.Item("UseWCombo").GetValue<bool>() ||
                    myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass && config.Item("UseWHarass").GetValue<bool>()) &&
                    !Player.IsWindingUp &&
                    W.IsReady())
                {
                    W.Cast();
                }
            }            
            if (args.Target is Obj_AI_Minion)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear &&                    
                    config.Item("UseWFarm").GetValue<bool>() &&
                    !Player.IsWindingUp &&
                    W.IsReady())
                {
                    W.Cast();
                }
            }
            if (args.Target is Obj_AI_Minion && args.Target.Team == GameObjectTeam.Neutral)
            {
                if (!args.Target.Name.Contains("Mini") &&
                    config.Item("UseWJFarm").GetValue<bool>() &&
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
                    if (config.Item("UseWCombo").GetValue<bool>() &&
                        !Player.IsWindingUp &&
                        W.IsReady() && 
                        target.IsValidTarget()) W.Cast();
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
        }
    }
}
