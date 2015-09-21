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
    class Irelia : PluginData
    {
        public Irelia()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 425);
            R = new Spell(SpellSlot.R, 1000);

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
                combomenu.AddItem(new MenuItem("EC.Irelia.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Irelia.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Irelia.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Irelia.Combo.R", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Irelia.Combo.Dive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Irelia.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {               
                harassmenu.AddItem(new MenuItem("EC.Irelia.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Irelia.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Irelia.Harass.E", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Irelia.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Irelia.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Irelia.QFarmDelay", "Q Delay").SetValue(new Slider(500, 0, 1000)));
                laneclearmenu.AddItem(new MenuItem("EC.Irelia.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Irelia.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Irelia.Jungle.W", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Irelia.Jungle.E", "Use E").SetValue(true));
                config.AddSubMenu(junglemenu);
            }  
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Irelia.Misc.E", "E Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Irelia.Misc.E2", "E Gapcloser").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Irelia.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Irelia.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Irelia.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Physical, true);

            var UseQ = config.Item("EC.Irelia.Combo.Q").GetValue<bool>();
            var UseE = config.Item("EC.Irelia.Combo.E").GetValue<bool>();
            var UseR = config.Item("EC.Irelia.Combo.R").GetValue<bool>();
            var CastItems = config.Item("EC.Irelia.Combo.Items").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;
                if (CastItems) { myItemManager.UseItems(0, Target); }
                try
                {                    
                    if (UseQ && Q.IsReady() && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= Q.Range)
                    {
                        if (Target.UnderTurret(true) && !config.Item("EC.Irelia.Combo.Dive").GetValue<bool>()) return;
                        if (myUtility.ImmuneToMagic(Target)) return;
                        if (Q.IsKillable(Target)) Q.Cast(Target);
                        else if (!Orbwalking.InAutoAttackRange(Target) && myUtility.TickCount - LastQ > 2000)
                        {
                            var dist = Vector3.Distance(Player.ServerPosition, Target.ServerPosition);
                            var msDif = Player.MoveSpeed - Target.MoveSpeed;
                            var reachIn = dist / msDif;
                            if (msDif < 0)
                            {
                                Q.Cast(Target);
                            }
                            else if (reachIn > 1)
                            {
                                Q.Cast(Target);
                            }
                        }
                    }
                    if (UseE && E.IsReady() && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= E.Range)
                    {
                        if (myUtility.ImmuneToCC(Target) || myUtility.ImmuneToMagic(Target)) return;
                        if (myUtility.PlayerHealthPercentage < (Target.Health * 100 / Target.MaxHealth)) E.Cast(Target);
                        else
                        {
                            if (Target.MoveSpeed >= Player.MoveSpeed || 
                                (myUtility.IsFacing(Player, Target.ServerPosition) && !myUtility.IsFacing(Target, Player.ServerPosition)))
                            {
                                E.Cast(Target);
                            }
                        }
                    }
                    if (UseR && R.IsReady())
                    {
                        mySpellcast.Linear(Target, R, HitChance.High);
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
                catch { }
            }            
        }
        private void Harass()
        {
            var UseQ = config.Item("EC.Irelia.Harass.Q").GetValue<bool>();
            var UseW = config.Item("EC.Irelia.Harass.W").GetValue<bool>();
            var UseE = config.Item("EC.Irelia.Harass.E").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady() && Q.IsInRange(target))
                {
                    if (target.UnderTurret(true)) return;
                    Q.Cast(target);
                }
                if (UseW && W.IsReady() && Orbwalking.InAutoAttackRange(target))
                {
                    W.Cast();
                }
                if (UseE && E.IsReady() && E.IsInRange(target))
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    E.Cast(target);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("EC.Irelia.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (config.Item("EC.Irelia.Farm.Q").GetValue<bool>() && Q.IsReady() && myUtility.TickCount - LastQ > config.Item("EC.Irelia.QFarmDelay").GetValue<Slider>().Value && !Player.IsWindingUp)
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (minionQ == null) return;
                var siegeQ = myFarmManager.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                if (siegeQ != null && siegeQ.IsValidTarget())
                {
                    Q.Cast(siegeQ);
                }
                else
                {
                    var AnyQ = minionQ.Where(x => Q.IsKillable(x) &&(Player.GetAutoAttackDamage(x) < x.Health && !Orbwalking.InAutoAttackRange(x))).OrderBy(i => i.Distance(Player)).FirstOrDefault();
                    if (AnyQ != null && AnyQ.IsValidTarget())
                    {
                        if (myUtility.IsFacing(Player, AnyQ.ServerPosition)) Q.Cast(AnyQ);
                    }
                }                
            }
            if (config.Item("EC.Irelia.Farm.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
            {                
                var minionE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (minionE == null) return;
                var siegeE = myFarmManager.GetLargeMinions(E.Range).FirstOrDefault(x => E.IsKillable(x));
                if (siegeE != null && siegeE.IsValidTarget())
                {
                    E.Cast(siegeE);
                }
                else
                {
                    var AnyE = minionE.FirstOrDefault(x => E.IsKillable(x) && Player.GetAutoAttackDamage(x) < x.Health);
                    if (AnyE != null && AnyE.IsValidTarget())
                    {
                       E.Cast(AnyE);
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
                if (config.Item("EC.Irelia.Jungle.Q").GetValue<bool>() && Q.IsReady() && Q.IsInRange(mob))
                {
                    if (largemobs != null)
                    {
                        Q.Cast(largemobs);
                    }
                    else
                    {
                        Q.Cast(mob);
                    }
                }
                if (config.Item("EC.Irelia.Jungle.E").GetValue<bool>() && E.IsReady() && E.IsInRange(mob))
                {
                    if (largemobs != null)
                    {
                        E.Cast(largemobs);
                    }
                    else
                    {
                       E.Cast(mob);
                    }
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
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (config.Item("EC.Irelia.Combo.W").GetValue<bool>() && W.IsReady())
                    {
                        W.Cast();
                    }
                }
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (spell.SData.Name.ToLower() == "ireliagatotsu")
                    {
                        LastQ = myUtility.TickCount;
                    }
                }
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
            {
                if (args.Slot == SpellSlot.Q)
                {                   
                    if (LastTarget == null || !LastTarget.IsValidTarget())
                    {
                        LastTarget = (Obj_AI_Hero)args.Target;
                        args.Process = true;
                    }
                    if (LastTarget != (Obj_AI_Hero)args.Target)
                    {
                        if (myUtility.TickCount - LastQ < 2000 + myHumazier.SpellDelay)
                        {
                            args.Process = false;    
                        }
                        LastTarget = (Obj_AI_Hero)args.Target;
                        args.Process = true;
                    }
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("EC.Irelia.Misc.E").GetValue<bool>() && E.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.High)
            {
                if (Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= E.Range && myUtility.PlayerHealthPercentage < (sender.Health * 100 / sender.MaxHealth))
                {
                    W.Cast(sender);
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("EC.Irelia.Misc.E2").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;                    
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.Cast(gapcloser.Sender));
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Irelia.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Irelia.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (config.Item("EC.Irelia.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.White);
            }
        }
    }
}
