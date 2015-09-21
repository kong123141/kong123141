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
    class Lissandra : PluginData
    {
        public Lissandra()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 725); 
            W = new Spell(SpellSlot.W, 450);
            E = new Spell(SpellSlot.E, 1050);
            R = new Spell(SpellSlot.R, 550);

            Q2 = new Spell(SpellSlot.Q, 825); 

            Q.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileFixedTravelTime, false, SkillshotType.SkillshotLine);            
            W.SetSkillshot(0.2f, 175f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 110, 850, false, SkillshotType.SkillshotLine);
            R.SetTargetted(0.4f, float.MaxValue);

            Q2.SetSkillshot(Q2.Instance.SData.SpellCastTime, Q2.Instance.SData.LineWidth, Q2.Instance.SData.MissileFixedTravelTime, false, SkillshotType.SkillshotCone);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellList.Add(Q2);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Frozen Tomb", "Custom");
            {
                custommenu.AddItem(new MenuItem("EC.Lissandra.UseRKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("EC.Lissandra.UseRType", "R").SetValue(new StringList(new[] { "Self", "Target" })));
                custommenu.AddItem(new MenuItem("EC.Lissandra.UseRDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("EC.Lissandra.UseRDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Lissandra.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Lissandra.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Lissandra.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Lissandra.Combo.E2", "Use E Second Cast").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Lissandra.Combo.R", "Use R (On dying)").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Lissandra.Combo.Dive", "Turret Dive").SetValue(false));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Lissandra.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Lissandra.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Lissandra.Harass.E", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Lissandra.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Lissandra.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Lissandra.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Lissandra.Farm.W.Value", "W More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Lissandra.Farm.E.Value", "E More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Lissandra.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Lissandra.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Lissandra.Jungle.W", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Lissandra.Jungle.E", "Use E").SetValue(true));
                config.AddSubMenu(junglemenu);
            }  
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Lissandra.QPredHitchance", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));                
                miscmenu.AddItem(new MenuItem("EC.Lissandra.Misc.W", "W Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Lissandra.UseRMisc", "R Interrupts").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Lissandra.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Lissandra.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Lissandra.Draw.E", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Lissandra.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Lissandra.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Lissandra.Combo.E").GetValue<bool>();
            var UseE2 = config.Item("EC.Lissandra.Combo.E2").GetValue<bool>();
            if (UseQ && Q.IsReady())
            {
                if (!Target.IsValidTarget())
                {
                    mySpellcast.Extension(null, Q, Q.Range, Q2.Range, true, true, true);
                }
            }
            if (UseW && W.IsReady())
            {
                if (Player.CountEnemiesInRange(W.Range) > 0)
                {
                    W.Cast();
                }
            }
            if (UseE && UseE2 && E.IsReady())
            {
                if (Target.IsValidTarget())
                {
                    if (myUtility.TickCount - LastE >= (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) / 0.7f))
                    {
                        if (Target.UnderTurret(true) && !config.Item("EC.Lissandra.Combo.Dive").GetValue<bool>()) return;
                        E.Cast();
                    }
                }
                return;
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToMagic(Target)) return;
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= Q.Range)
                        {
                            mySpellcast.Linear(Target, Q, HitChance.High);
                        }
                        else if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) > Q.Range && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= Q2.Range)
                        {
                            mySpellcast.Extension(Target, Q, Q.Range, Q2.Range, true, true, true);
                        }
                    }
                    if (UseE && E.IsReady() && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= E.Range)                    
                    {
                        if (myUtility.TickCount - LastE > 2500)
                        {
                            E.Cast(Target.ServerPosition);
                        }
                    }
                }
                catch { }
            }           
        }
        private void Harass()
        {
            var UseQ = config.Item("EC.Lissandra.Harass.Q").GetValue<bool>();
            var UseW = config.Item("EC.Lissandra.Harass.W").GetValue<bool>();
            var UseE = config.Item("EC.Lissandra.Harass.E").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                if (UseQ && Q.IsReady() && !Player.IsWindingUp)
                {
                    mySpellcast.Linear(target, Q2, HitChance.High);
                }
                if (UseW && W.IsReady())
                {
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < W.Range)
                    {
                        W.Cast();
                    }
                }
                if (UseE && E.IsReady() && !Player.IsWindingUp && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < E.Range && 
                    myUtility.TickCount - LastE > 1800)
                {
                    E.Cast(target.ServerPosition);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("EC.Lissandra.Farm.ManaPercent").GetValue<Slider>().Value)
            {
                if (!Passive) return;
            }
            if (Player.UnderTurret(true)) return;            
            if (config.Item("EC.Lissandra.Farm.Q").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (minionQ == null) return;
                var siegeQ = myFarmManager.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                if (siegeQ != null && siegeQ.IsValidTarget())
                {
                    Q.Cast(siegeQ.ServerPosition);
                }
                else
                {
                    var FurthestQ = minionQ.OrderByDescending(i => i.Distance(Player)).FirstOrDefault(x => Q.IsKillable(x));
                    if (FurthestQ != null && FurthestQ.IsValidTarget())
                    {
                        if (myUtility.IsFacing(Player, FurthestQ.ServerPosition, 60)) Q.Cast(FurthestQ.ServerPosition);
                    }
                }
            }
            if (config.Item("EC.Lissandra.Farm.W").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
            {
                var minionW = MinionManager.GetMinions(Player.ServerPosition, W.Range);
                if (minionW == null) return;
                if (minionW.Count() > config.Item("EC.Lissandra.Farm.W.Value").GetValue<Slider>().Value)
                {
                   W.Cast();
                }
            }
            if (config.Item("EC.Lissandra.Farm.E").GetValue<bool>() && E.IsReady() && myUtility.TickCount - LastE > 1800)
            {
                var minionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                var ELine = E.GetLineFarmLocation(minionsE);
                if (ELine.MinionsHit > config.Item("EC.Lissandra.Farm.E.Value").GetValue<Slider>().Value)
                {
                    if (myUtility.IsFacing(Player, ELine.Position.To3D())) E.Cast(ELine.Position);
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
                if (config.Item("EC.Lissandra.Jungle.Q").GetValue<bool>() && Q.IsReady() && Q.IsInRange(mob) && !Player.IsWindingUp)
                {
                    if (largemobs != null)
                    {
                        Q.Cast(largemobs.ServerPosition);
                    }
                    else
                    {
                        Q.Cast(mob.Position);
                    }
                }
                if (config.Item("EC.Lissandra.Jungle.W").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < W.Range)
                    {
                        W.Cast();
                    }
                    if (Vector3.Distance(Player.ServerPosition, mob.ServerPosition) < W.Range) W.Cast();
                }
                if (config.Item("EC.Lissandra.Jungle.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp && myUtility.TickCount - LastE > 1800)
                {
                    if (largemobs != null)
                    {
                        E.Cast(largemobs.ServerPosition);
                    }
                    else
                    {
                        E.Cast(mob.ServerPosition);
                    }
                }
            }
        }
        private void Custom()
        {
            Target = myUtility.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (Target.IsValidTarget() && !myUtility.ImmuneToMagic(Target) && !myUtility.ImmuneToCC(Target))
            {
                mySpellcast.Unit(Target, R);
            }
        }

        private bool Passive
        {
            get
            {
                return Player.HasBuff("lissandrapassiveready");
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
            switch (config.Item("EC.Lissandra.QPredHitchance").GetValue<StringList>().SelectedIndex)
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
        protected override void ProcessDamageBuffer(Obj_AI_Base sender, Obj_AI_Hero target, SpellData spell, myCustomEvents.DamageTriggerType type)
        {
            if (sender != null && target.IsMe)
            {
                switch (type)
                {
                    case myCustomEvents.DamageTriggerType.Killable:
                        if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo &&
                            config.Item("EC.Lissandra.Combo.R").GetValue<bool>() &&
                            R.IsReady())
                        {
                            R.Cast();
                        }
                        break;
                    case myCustomEvents.DamageTriggerType.TonsOfDamage:
                        break;
                }
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if (spell.SData.Name.ToLower() == "lissandraemissile")
                {
                    LastE = myUtility.TickCount;
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("EC.Lissandra.UseRMisc").GetValue<bool>() && R.IsReady())
            {
                if (Vector3.Distance(Player.ServerPosition,sender.ServerPosition) <= R.Range && args.DangerLevel == Interrupter2.DangerLevel.High)
                {
                    if (myUtility.ImmuneToCC(sender)) return;
                    R.CastOnUnit(sender);
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("EC.Lissandra.Misc.W").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= W.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast());
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Lissandra.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Lissandra.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Lissandra.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (R.Level > 0 && R.IsReady())
            {
                switch (config.Item("EC.Lissandra.UseRType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                       if (config.Item("EC.Lissandra.UseRDrawDistance").GetValue<bool>())
                        {
                            Render.Circle.DrawCircle(Player.Position, E.Range + 225, Color.Fuchsia, 7);
                        }
                        if (config.Item("EC.Lissandra.UseRDrawTarget").GetValue<bool>())
                        {
                            var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                            var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                         Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= E.Range + 225
                                         ).OrderByDescending(i => i.CountEnemiesInRange(550)).FirstOrDefault();                            
                            if (target != null && target.IsValidTarget())
                            {
                                var num = EnemyList.Count(x => Vector3.Distance(target.ServerPosition, x.ServerPosition) <= 550);
                                Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.White, "Hits: " + num);
                                Render.Circle.DrawCircle(target.ServerPosition, target.BoundingRadius, Color.Lime, 7);                               
                            }
                        }
                        break;
                    case 1:
                       if (config.Item("EC.Lissandra.UseRDrawDistance").GetValue<bool>())
                        {
                            Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 7);
                        }
                       if (config.Item("EC.Lissandra.UseRDrawTarget").GetValue<bool>())
                       {
                           var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                           var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                        Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range
                                        ).OrderByDescending(i => i.CountEnemiesInRange(550)).FirstOrDefault();
                           if (target != null && target.IsValidTarget())
                           {
                               var num = EnemyList.Count(x => Vector3.Distance(target.ServerPosition, x.ServerPosition) < 550);
                               Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.White, "Hits: " + num);
                               Render.Circle.DrawCircle(target.ServerPosition, target.BoundingRadius, Color.Lime, 7);
                           }
                       }
                        break;
                }
            }
        }
    }
}
  