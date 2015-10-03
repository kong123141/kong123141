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
    class Swain : PluginData
    {
        public Swain()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 625);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 625);
            R = new Spell(SpellSlot.R, 700);

            Q.SetTargetted(0.5f, float.MaxValue);
            W.SetSkillshot(0.5f, 275, 1250, false, SkillshotType.SkillshotCircle);
            E.SetTargetted(0.5f, 1400);
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Swain.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Swain.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Swain.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Swain.Combo.R", "Use R").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Swain.Misc.W", "W Gapclosers").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Swain.Misc.W2", "W Interrupts").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Swain.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Swain.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Swain.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Swain.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = Root.Item("EC.Swain.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Swain.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Swain.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Swain.Combo.R").GetValue<bool>();

            if (UseR)
            {
                if (R.IsReady() && Player.Spellbook.GetSpell(SpellSlot.R).ToggleState == 1 && Player.CountEnemiesInRange(625) > 0)
                {
                    R.Cast();
                }
                else if (R.IsReady() && Player.Spellbook.GetSpell(SpellSlot.R).ToggleState != 1 && Player.CountEnemiesInRange(625) <= 0)
                {
                    var minions = MinionManager.GetMinions(Player.ServerPosition, R.Range);
                    if (minions == null) R.Cast();
                    else if (myUtility.PlayerManaPercentage < 35 || myUtility.PlayerHealthPercentage > 75)
                    {
                        R.Cast();
                    }
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.Unit(Target, Q);
                    }
                    if (UseW && W.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.CircularAoe(Target, W, HitChance.High, W.Range, 275);
                    }
                    if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.Unit(Target, E);
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
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "swaindecrepify") || (spell.SData.Name.ToLower() == "swainshadowgrasp") || (spell.SData.Name.ToLower() == "swaintorment"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Swain.Misc.W").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= W.Range + (W.Width/2))
                {
                    if (myUtility.ImmuneToMagic(gapcloser.Sender) || myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Vector3 pos = myUtility.RandomPos(1, 25, 25, gapcloser.End);
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast(pos));
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Root.Item("EC.Swain.Misc.W2").GetValue<bool>() && W.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= W.Range + (W.Width / 2))
                {
                    if ((myUtility.ImmuneToMagic(sender) || myUtility.ImmuneToCC(sender))) return;
                    PredictionOutput pred = W.GetPrediction(sender);
                    if (Vector3.Distance(Player.ServerPosition, pred.CastPosition) <= W.Range)
                    {
                        if (pred.Hitchance >= HitChance.High)
                        {
                            Vector3 pos = myUtility.RandomPos(1, 25, 25, Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition)));
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast(pos));
                        }
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Swain.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Swain.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Swain.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Swain.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.Yellow, "Hits: " + Player.CountEnemiesInRange(R.Range));
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
