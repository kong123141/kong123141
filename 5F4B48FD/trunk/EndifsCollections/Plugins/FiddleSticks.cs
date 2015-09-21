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
    class FiddleSticks : PluginData
    {
        public FiddleSticks()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 575);
            W = new Spell(SpellSlot.W, 575);
            E = new Spell(SpellSlot.E, 750);
            R = new Spell(SpellSlot.R, 800);

            R.SetSkillshot(R.Instance.SData.SpellCastTime, 600f, R.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotCircle);

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
                laneclear.AddItem(new MenuItem("UseWFarm", "Use W").SetValue(true));
                laneclear.AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
                laneclear.AddItem(new MenuItem("WFarmValue", "W HP <").SetValue(new Slider(75)));
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
                miscmenu.AddItem(new MenuItem("UseQ2Misc", "Q Gapcloser").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawW", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawE", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }

        }
        
        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (myUtility.ImmuneToMagic(target)) return;
                if (mySmiter.CanSmiteChampions(target)) mySmiter.Smites(target);       
                try
                {
                    if (UseQ && Q.IsReady() && !myUtility.ImmuneToCC(target))
                    {
                        Q.CastOnUnit(target);
                    }
                    if (UseW && W.IsReady() && myUtility.TickCount - LastSpell > 300)
                    {
                        if (UseQ && Q.CanCast(target) && Q.IsInRange(target) && !myUtility.ImmuneToCC(target)) return;
                        if (UseE && E.CanCast(target) && E.IsInRange(target)) return;
                        W.CastOnUnit(target);
                    }
                    if (UseE && E.IsReady())
                    {
                        E.CastOnUnit(target);
                    }
                }
                catch { }
            }           
        }
        private void Harass()
        {
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var UseW = config.Item("UseWHarass").GetValue<bool>();
            var UseE = config.Item("UseEHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);            
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady())
                {
                    Q.CastOnUnit(target);
                }
                if (UseW && W.IsReady() && myUtility.TickCount - LastSpell > 300)
                {
                    if (UseQ && Q.CanCast(target) && Q.IsInRange(target)) return;
                    if (UseE && E.CanCast(target) && E.IsInRange(target)) return;
                    W.CastOnUnit(target);
                }
                if (UseE && E.IsReady())
                {
                    E.CastOnUnit(target);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (config.Item("UseWFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp && myUtility.PlayerHealthPercentage < config.Item("WFarmValue").GetValue<Slider>().Value && !myOrbwalker.IsWaiting())
            {
                var minionW = MinionManager.GetMinions(Player.ServerPosition, W.Range);
                if (minionW == null) return;
                var siegew = myUtility.GetLargeMinions(W.Range).FirstOrDefault();
                var meleeW = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.Melee, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
                var mW = meleeW.Where(x => x.Health >= (x.MaxHealth * 2 / 3)).OrderByDescending(i => i.Health).FirstOrDefault();
                if (siegew != null && siegew.IsValidTarget())
                {
                    W.CastOnUnit(siegew);
                }
                else if (mW != null && mW.IsValidTarget())
                {
                    W.CastOnUnit(mW);
                }
                else
                {
                    var anyW = minionW.OrderByDescending(i => i.Health).FirstOrDefault();
                    if (anyW != null && mW.IsValidTarget())
                    {
                        W.CastOnUnit(mW);
                    }
                }
            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
            {
                if (Player.UnderTurret(true)) return;
                var minionE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (minionE == null) return;
                var FurthestE = minionE.OrderByDescending(i => i.Distance(Player)).ToList();
                foreach (var x in FurthestE)
                {
                    if (E.IsInRange(x) && HeroManager.Enemies.Any(z => Vector3.Distance(z.ServerPosition, x.ServerPosition) < E.Instance.SData.BounceRadius))
                    {
                        E.CastOnUnit(x);
                    }
                    else if (E.IsInRange(x) && MinionManager.GetMinions(x.ServerPosition, 200).Count() > 1)
                    {
                        E.CastOnUnit(x);
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
                if (largemobs != null && mySmiter.CanSmiteMonster) mySmiter.Smites(largemobs);
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady())
                {
                    if (largemobs != null)
                    {
                        Q.CastOnUnit(largemobs);
                    }
                }
                if (config.Item("UseWJFarm").GetValue<bool>() && W.IsReady())
                {
                    if (largemobs != null)
                    {
                        W.CastOnUnit(largemobs);
                    }
                    else
                    {
                        W.CastOnUnit(mob);
                    }
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady())
                {
                    if (largemobs != null)
                    {
                        E.CastOnUnit(largemobs);
                    }
                    else
                    {
                        E.CastOnUnit(mob);
                    } 
                }
            }
        }

        private int LastSpell;

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
                case myOrbwalker.OrbwalkingMode.JungleClear:
                    JungleClear();
                    break;
            }            
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (myOrbwalker.Active())
            {
                if ((args.Slot == SpellSlot.Q) || (args.Slot == SpellSlot.E))
                {
                    if (Player.HasBuff("Drain") || Player.HasBuff("fearmonger_marker") || (Player.IsChannelingImportantSpell() || Player.IsCastingInterruptableSpell()))
                    {
                        args.Process = false;
                    }
                }
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                 if ((spell.SData.Name.ToLower() == "terrify") || (spell.SData.Name.ToLower() == "fiddlesticksdarkwind"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("UseQMisc").GetValue<bool>() && Q.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= Q.Range)
                {
                    if (myUtility.ImmuneToCC(sender) || myUtility.ImmuneToMagic(sender)) return;
                    Q.CastOnUnit(sender);
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("UseQ2Misc").GetValue<bool>() && Q.IsReady())
            {
                if (Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) <= Q.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;
                    Q.CastOnUnit(gapcloser.Sender);
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
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
