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
    class Nami : PluginData
    {
        public Nami()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 875);
            W = new Spell(SpellSlot.W, 725);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 2750);

            Q.SetSkillshot(1f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 260f, 850f, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Tidal Wave", "Custom");
            {
                custommenu.AddItem(new MenuItem("EC.Nami.UseRKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                //custommenu.AddItem(new MenuItem("EC.Nami.UseRDrawTarget", "Draw Target").SetValue(true));
                //custommenu.AddItem(new MenuItem("EC.Nami.UseRDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Nami.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Nami.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Nami.Combo.E", "Use E").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Nami.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Nami.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Nami.Harass.E", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Nami.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Nami.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Nami.Farm.Q.Value", "Q More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Nami.Farm.E.Value", "E More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Nami.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Nami.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Nami.Jungle.E", "Use E").SetValue(true));
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Nami.QPredHitchance", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("EC.Nami.Misc.Q", "Q Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Nami.Misc.Q2", "Q Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Nami.UseWSupport", "W Supports").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Nami.UseESupport", "E Supports").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Nami.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Nami.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Nami.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Nami.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            
            var UseQ = config.Item("EC.Nami.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Nami.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Nami.Combo.E").GetValue<bool>();
            if (UseW && W.IsReady() && !Target.IsValidTarget())
            {
                var WAllies = HeroManager.Allies.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= W.Range);
                foreach (var wbounce in WAllies)
                {
                    if (W.IsInRange(wbounce))
                    {
                        if (HeroManager.Enemies.Any(x => Vector3.Distance(wbounce.ServerPosition, x.ServerPosition) <= W.Range))
                        {
                            W.Cast(wbounce);
                        }
                        else
                        {
                            if (config.Item("EC.Nami.UseWSupport").GetValue<bool>() && wbounce.Health < 50)
                            {
                                W.Cast(wbounce);
                            }
                        }
                    }
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToMagic(Target)) return;
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        if (myUtility.ImmuneToCC(Target)) return;
                        mySpellcast.CircularPrecise(Target, Q, HitChance.High, 0, 0, 0);
                    }
                    if (UseW && W.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        if (Vector3.Distance(Target.ServerPosition, Player.ServerPosition) <= W.Range)
                        {
                            W.Cast(Target);
                        }
                    }
                    if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        if (UseQ && Q.IsReady()) return;
                        if (UseW && W.IsReady()) return;
                        if (Orbwalking.InAutoAttackRange(Target) && Player.CountEnemiesInRange(E.Range) > 0)
                        {
                            E.CastOnUnit(Player);
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
            var UseQ = config.Item("EC.Nami.Harass.Q").GetValue<bool>();
            var UseW = config.Item("EC.Nami.Harass.W").GetValue<bool>();
            var UseE = config.Item("EC.Nami.Harass.E").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady())
                {
                    if (myUtility.ImmuneToCC(target) || (Player.UnderTurret(true) && target.UnderTurret(true))) return;
                    if (myUtility.IsFacing(Player, target.ServerPosition, 60))
                    {
                        mySpellcast.CircularPrecise(target, Q, HitChance.High,0,0,0);
                    }
                }
                if (UseW && W.IsReady())
                {
                    if (Vector3.Distance(target.ServerPosition, Player.ServerPosition) < W.Range)
                    {
                        if (target.UnderTurret(true) && Player.UnderTurret(true)) return;
                        if (myUtility.IsFacing(Player, target.ServerPosition, 60))
                        {
                            W.Cast(target);
                        } 
                    }
                    else
                    {
                        var WAllies = HeroManager.Allies.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= W.Range);
                        foreach (var wbounce in WAllies)
                        {
                            if (W.IsInRange(wbounce) &&
                                HeroManager.Enemies.Any(
                                x =>
                                    Vector3.Distance(wbounce.ServerPosition, x.ServerPosition) < W.Range))
                            {
                                if (Player.UnderTurret(true)) return;
                                W.Cast(wbounce);
                            }
                        }
                    }
                }
                if (UseE && E.IsReady())
                {
                    if (myUtility.IsFacing(Player, target.ServerPosition) && Orbwalking.InAutoAttackRange(target))
                    {
                        E.CastOnUnit(Player);
                    }
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("EC.Nami.Farm.ManaPercent").GetValue<Slider>().Value) return;
            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
            if (allMinionsQ == null) return;
            if (config.Item("EC.Nami.Farm.Q").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
            {                                
                foreach (var x in allMinionsQ)
                {
                    if (Q.IsInRange(x) && MinionManager.GetMinions(x.ServerPosition, Q.Width).Count() > config.Item("EC.Nami.Farm.Q.Value").GetValue<Slider>().Value)
                    {
                        if (myUtility.IsFacing(Player, x.ServerPosition)) Q.CastOnUnit(x);
                    }
                }
            }
            if (config.Item("EC.Nami.Farm.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
            {
                if (allMinionsQ.Count() > config.Item("EC.Nami.Farm.E.Value").GetValue<Slider>().Value)
                {
                    E.CastOnUnit(Player);
                }
            }            
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null && !Player.IsWindingUp)
            {
                if (config.Item("EC.Nami.Jungle.Q").GetValue<bool>() && Q.IsReady())
                {
                    if (largemobs != null)
                    {
                        Q.Cast(largemobs.ServerPosition);
                    }
                    else
                    {
                        if (myUtility.IsFacing(Player, mob.ServerPosition)) Q.Cast(mob.ServerPosition);
                    }
                }
                if (config.Item("EC.Nami.Jungle.E").GetValue<bool>() && E.IsReady())
                {
                    if (largemobs != null)
                    {
                        E.CastOnUnit(Player);
                    }                   
                }
            }
        }
        private void Custom()
        {
            if (R.IsReady())
            {
                mySpellcast.LinearBox(R, HitChance.High, 5);               
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
            switch (config.Item("EC.Nami.QPredHitchance").GetValue<StringList>().SelectedIndex)
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
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {                
                if ((spell.SData.Name.ToLower() == "namiq") || (spell.SData.Name.ToLower() == "namiw") || (spell.SData.Name.ToLower() == "namie"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
            if (unit is Obj_AI_Hero && unit.IsAlly && !unit.IsMe)
            {
                if (config.Item("EC.Nami.UseESupport").GetValue<bool>())
                {
                    if (spell.Target is Obj_AI_Hero && spell.SData.IsAutoAttack() && spell.Target.IsEnemy && Vector3.Distance(Player.ServerPosition, unit.ServerPosition) <= E.Range)
                    {
                        E.Cast(unit);
                    }
                }
            }
        }
        protected override void OnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid<MissileClient>())
            {
                return;
            }
            if (!config.Item("EC.Nami.UseESupport").GetValue<bool>()) return;
            var missile = (MissileClient)sender;
            if (!missile.SpellCaster.IsValid<Obj_AI_Hero>() || !missile.SpellCaster.IsAlly || missile.SpellCaster.IsMe ||
                missile.SpellCaster.IsMelee())
            {
                return;
            }
            if (!missile.Target.IsValid<Obj_AI_Hero>() || !missile.Target.IsEnemy)
            {
                return;
            }
            var caster = (Obj_AI_Hero)missile.SpellCaster;
            if (E.IsReady() && E.IsInRange(caster))
            {
                E.CastOnUnit(caster);
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("EC.Nami.Misc.Q").GetValue<bool>() && Q.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= Q.Range)
                {
                    if (myUtility.ImmuneToMagic(sender) || myUtility.ImmuneToCC(sender)) return;
                    Q.Cast(sender.Position);
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("EC.Nami.Misc.Q2").GetValue<bool>() && Q.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= Q.Range)
                {
                    if (myUtility.ImmuneToMagic(gapcloser.Sender) || myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Vector3 pos = myUtility.RandomPos(1, 20, 25, gapcloser.End);
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => Q.Cast(pos));                    
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Nami.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Nami.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Nami.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (config.Item("EC.Nami.Draw.R").GetValue<bool>() && R.Level > 0 && R.IsReady())
            {
                var wtc = Drawing.WorldToScreen(Game.CursorPos);
                var box = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(Game.CursorPos, R.Range), R.Width);
                var insidebox = HeroManager.Enemies.Where(x => box.IsInside(x) && x.IsValidTarget()).ToList();
                if (insidebox.Any())
                {
                    if (insidebox.Count() >= 4)
                    {
                        Drawing.DrawText(wtc.X + 10, wtc.Y - 15, Color.Red, "Hits: " + insidebox.Count());
                    }
                    else
                    {
                        Drawing.DrawText(wtc.X + 10, wtc.Y - 15, Color.Yellow, "Hits: " + insidebox.Count());
                    }
                }
                box.Draw(Color.Red);
            }     
        }
    }
}
