using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
{
    class Caitlyn : PluginData
    {
        public Caitlyn()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1250);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 950); //400 back
            R = new Spell(SpellSlot.R, 2000);

            Q.SetSkillshot(0.65f, 90f, 2200f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(1.5f, 1f, 1750f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 80f, 1600f, true, SkillshotType.SkillshotLine);         
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Caitlyn.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Caitlyn.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Caitlyn.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Caitlyn.Combo.R", "Use R").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {                
                miscmenu.AddItem(new MenuItem("EC.Caitlyn.Misc.W", "W Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Caitlyn.Misc.W2", "W Gapcloser").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Caitlyn.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Caitlyn.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Caitlyn.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Caitlyn.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            var UseQ = Root.Item("EC.Caitlyn.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Caitlyn.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Caitlyn.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Caitlyn.Combo.R").GetValue<bool>();
            
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                try
                {
                    if (UseQ && Q.IsReady())
                    {                                                
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) < 300) return;
                        mySpellcast.Linear(Target, Q, HitChance.High);
                    }
                    if (UseW && W.IsReady())
                    {
                        mySpellcast.CircularPrecise(Target, W, HitChance.Medium, W.Range);
                    }
                    if (UseE && E.IsReady())
                    {                        
                        PredictionOutput pred = E.GetPrediction(Target);
                        var test = pred.CastPosition.Extend(Player.ServerPosition, 400);
                        if (test.CountEnemiesInRange(600) > 0 || test.UnderTurret(true)) return;
                        mySpellcast.Linear(Target, E, HitChance.High, true);
                    }
                    if (UseR && R.IsReady())
                    {
                        if (Target.IsValidTarget() && !Orbwalking.InAutoAttackRange(Target) && R.IsKillable(Target))
                        {
                            mySpellcast.Unit(Target, R);
                        }
                    }
                }
                catch { }
            }
        }
        private void UpdateR()
        {
            if (R.Level > 0)
            {
                R.Range = 1500 + R.Level * 500;
            }
        }

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                myUtility.Reset();
                return;
            }
            UpdateR();
            switch (myOrbwalker.ActiveMode)
            {
                case myOrbwalker.OrbwalkingMode.None:
                    myUtility.Reset();
                    break;
                case myOrbwalker.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case myOrbwalker.OrbwalkingMode.LaneClear:
                    myFarmManager.LaneLinear(Q, Q.Range,true);
                    break;
            }            
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.R)
            {
                LastR = myUtility.TickCount;
                mySpellcast.Pause(1000 + Game.Ping);
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (Player.IsChannelingImportantSpell() || myUtility.TickCount - LastR <= 0.25f)
            {
                args.Process = false;
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Root.Item("EC.Caitlyn.Misc.W").GetValue<bool>() && W.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= W.Range)
                {
                    if (myUtility.ImmuneToCC(sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Circular(sender.ServerPosition, W, -10, 10, 10));
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Caitlyn.Misc.W2").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) <= W.Range)
                {
                    if (myUtility.ImmuneToMagic(gapcloser.Sender) || myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Circular(gapcloser.End, W, -10, 10, 10));                   
                }
            }
        }        
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Caitlyn.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Caitlyn.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Caitlyn.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Caitlyn.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
