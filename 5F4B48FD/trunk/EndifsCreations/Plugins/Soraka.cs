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
    class Soraka : PluginData
    {
        public Soraka()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 950);
            W = new Spell(SpellSlot.W, 550);
            E = new Spell(SpellSlot.E, 925);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.5f, 210, 1100, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 70f, 1750, false, SkillshotType.SkillshotCircle);   
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Soraka.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Soraka.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Soraka.Combo.E", "Use E").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Soraka.Misc.E", "E Gapclosers").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Soraka.Misc.E2", "E Interrupts").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Soraka.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Soraka.Draw.E", "E").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range + 200, TargetSelector.DamageType.Magical);

            var UseQ = Root.Item("EC.Soraka.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Soraka.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Soraka.Combo.E").GetValue<bool>();
            if (UseW && W.IsReady())
            {
                var Allies = HeroManager.Allies.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= W.Range).OrderBy(i => i.Health);
                foreach (var heal in Allies.Where(x => x.HealthPercent < 50))
                {
                    W.Cast(heal);
                }
            }
            if (Target.IsValidTarget())
            {
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastE > myHumazier.SpellDelay)
                    {
                        mySpellcast.CircularAoe(Target, Q, HitChance.High, Q.Range, 210);                   
                    }
                    if (UseE && E.IsReady() && myUtility.TickCount - LastQ > myHumazier.SpellDelay)
                    {
                        mySpellcast.CircularAoe(Target, E, HitChance.High, E.Range, 70);
                    }
                }
                catch { }
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
                    Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                    if (Target.IsValidTarget() && Q.IsReady())
                    {
                        mySpellcast.CircularAoe(Target, Q, HitChance.High, Q.Range, 210);
                    }
                    break;
                case myOrbwalker.OrbwalkingMode.LaneClear:
                    if (myUtility.EnoughMana(33))
                    {
                        myFarmManager.LaneCircular(Q, Q.Range, 210);
                    }
                    break;
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if (spell.SData.Name.ToLower() == "sorakaq")
                {
                    LastQ = myUtility.TickCount;
                }
                if (spell.SData.Name.ToLower() == "sorakae")
                {
                    LastE = myUtility.TickCount;
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Soraka.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range + (E.Width/2))
                {
                    if (myUtility.ImmuneToMagic(gapcloser.Sender) || myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Vector3 pos = myUtility.RandomPos(1, 25, 25, gapcloser.End);
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.Cast(pos));
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Root.Item("EC.Soraka.Misc.E2").GetValue<bool>() && E.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(sender)) return;
                    PredictionOutput pred = E.GetPrediction(sender);
                    if (Vector3.Distance(Player.ServerPosition, pred.CastPosition) <= E.Range)
                    {
                        if (pred.Hitchance >= HitChance.High)
                        {
                            Vector3 pos = myUtility.RandomPos(1, 25, 25, Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition)));
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.Cast(pos));
                        }
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Soraka.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Soraka.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
        }
    }
}
