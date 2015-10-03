using System;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.SummonerSpells;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
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
                combomenu.AddItem(new MenuItem("EC.FiddleSticks.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.FiddleSticks.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.FiddleSticks.Combo.E", "Use E").SetValue(true));                
                Root.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {               
                harassmenu.AddItem(new MenuItem("EC.FiddleSticks.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.FiddleSticks.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.FiddleSticks.Harass.E", "Use E").SetValue(true));
                Root.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.FiddleSticks.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.FiddleSticks.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.FiddleSticks.Farm.W.Value", "W HP <").SetValue(new Slider(75)));
                laneclearmenu.AddItem(new MenuItem("EC.FiddleSticks.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                Root.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.FiddleSticks.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.FiddleSticks.Jungle.W", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.FiddleSticks.Jungle.E", "Use E").SetValue(true));
                Root.AddSubMenu(junglemenu);
            }  
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.FiddleSticks.Misc.Q", "Q Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.FiddleSticks.Misc.Q2", "Q Gapcloser").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.FiddleSticks.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.FiddleSticks.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.FiddleSticks.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.FiddleSticks.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }

        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            
            var UseQ = Root.Item("EC.FiddleSticks.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.FiddleSticks.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.FiddleSticks.Combo.E").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                if (myUtility.ImmuneToMagic(Target)) return;
                try
                {
                    if (UseQ && Q.IsReady() && !myUtility.ImmuneToCC(Target))
                    {
                        mySpellcast.Unit(Target, Q);
                    }
                    if (UseW && W.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.Unit(Target, W);
                    }
                    if (UseE && E.IsReady())
                    {
                        mySpellcast.Unit(Target, E);
                    }
                }
                catch { }
            }           
        }
        private void Harass()
        {
            var UseQ = Root.Item("EC.FiddleSticks.Harass.Q").GetValue<bool>();
            var UseW = Root.Item("EC.FiddleSticks.Harass.W").GetValue<bool>();
            var UseE = Root.Item("EC.FiddleSticks.Harass.E").GetValue<bool>();
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);            
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady())
                {
                    mySpellcast.Unit(target, Q);
                }
                if (UseW && W.IsReady())
                {
                    mySpellcast.Unit(target, W);
                }
                if (UseE && E.IsReady())
                {
                    mySpellcast.Unit(target, E);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < Root.Item("EC.FiddleSticks.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (Root.Item("EC.FiddleSticks.Farm.W").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp && myUtility.PlayerHealthPercentage < Root.Item("EC.FiddleSticks.Farm.W.Value").GetValue<Slider>().Value && !myOrbwalker.Waiting)
            {
                var minionW = MinionManager.GetMinions(Player.ServerPosition, W.Range);
                if (minionW == null) return;
                var siegew = myFarmManager.GetLargeMinions(W.Range).FirstOrDefault();
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
            if (Root.Item("EC.FiddleSticks.Farm.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
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
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (Root.Item("EC.FiddleSticks.Jungle.Q").GetValue<bool>() && Q.IsReady())
                {
                    if (largemobs != null)
                    {
                        Q.CastOnUnit(largemobs);
                    }
                }
                if (Root.Item("EC.FiddleSticks.Jungle.W").GetValue<bool>() && W.IsReady())
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
                if (Root.Item("EC.FiddleSticks.Jungle.E").GetValue<bool>() && E.IsReady())
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
        private void Custom()
        {
            if (R.IsReady())
            {
                var tomouse = Player.ServerPosition.Extend(Game.CursorPos, Vector3.Distance(Player.ServerPosition, Game.CursorPos));
                var tomax = Player.ServerPosition.Extend(Game.CursorPos, R.Range);
                var newvec = Vector3.Distance(Player.ServerPosition, tomouse) >= Vector3.Distance(Player.ServerPosition, tomax) ? tomax : tomouse;
                if (newvec.CountEnemiesInRange(500) > 0)
                {
                    R.Cast(newvec);
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
                case myOrbwalker.OrbwalkingMode.Custom:
                    Custom();
                    break;
            }            
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe)
            {
                if (args.Slot == SpellSlot.Q || args.Slot == SpellSlot.E)
                {
                    if (myUtility.TickCount - LastW < 200) args.Process = false;
                }
                if (args.Slot == SpellSlot.W)
                {
                    LastW = myUtility.TickCount;
                    mySpellcast.Pause(2500 + Game.Ping);
                }
                if (args.Slot == SpellSlot.R)
                {
                    LastR = myUtility.TickCount;
                    mySpellcast.Pause(1500 + Game.Ping);
                }
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (Player.IsChannelingImportantSpell() || myUtility.TickCount - LastR <= 0.5f || myUtility.TickCount - LastW <= 0.5f)
            {
                args.Process = false;
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
            if (Root.Item("EC.FiddleSticks.Misc.Q").GetValue<bool>() && Q.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= Q.Range)
                {
                    if (myUtility.ImmuneToCC(sender) || myUtility.ImmuneToMagic(sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Unit(sender, Q));
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.FiddleSticks.Misc.Q2").GetValue<bool>() && Q.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= Q.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Unit(gapcloser.Sender, Q));
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {                      
            if (Player.IsDead) return;
            if (Root.Item("EC.FiddleSticks.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.FiddleSticks.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.FiddleSticks.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.FiddleSticks.Draw.R").GetValue<bool>())
            {
                var tomouse = Player.ServerPosition.Extend(Game.CursorPos, Vector3.Distance(Player.ServerPosition, Game.CursorPos));
                var tomax = Player.ServerPosition.Extend(Game.CursorPos, R.Range);
                var newvec = Vector3.Distance(Player.ServerPosition, tomouse) >= Vector3.Distance(Player.ServerPosition, tomax) ? tomax : tomouse;
                var wts = Drawing.WorldToScreen(newvec);
                var wtf = Drawing.WorldToScreen(Player.ServerPosition);
                Drawing.DrawLine(wtf, wts, 2, Color.GhostWhite);
                Render.Circle.DrawCircle(newvec, 500, Color.GhostWhite, 2);
                Drawing.DrawText(wts.X - 20, wts.Y - 50, Color.Yellow, "Hits: " + newvec.CountEnemiesInRange(500));
            }
        }
    }
}
