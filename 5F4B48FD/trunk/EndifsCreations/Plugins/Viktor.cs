﻿using System;
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
    class Viktor : PluginData
    {
        public Viktor()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 525); //500 beam path
            R = new Spell(SpellSlot.R, 700);

            Q.SetTargetted(0.25f, 2000);
            W.SetSkillshot(0.5f, 300, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0, 80, 1200, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.25f, 450f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Viktor.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Viktor.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Viktor.Combo.E", "Use E").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Viktor.Misc.W", "W Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Viktor.Misc.W2", "W Gapcloers").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Viktor.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Viktor.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Viktor.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Viktor.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(1025f, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Viktor.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Viktor.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Viktor.Combo.E").GetValue<bool>();           
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                try
                {
                    if (UseQ && Q.IsReady() && Q.IsInRange(Target))
                    {
                        Q.CastOnUnit(Target);
                    }
                    if (UseW && W.IsReady())
                    {
                        mySpellcast.CircularAoe(Target, W, HitChance.High);
                    }
                    if (UseE && E.IsReady())
                    {
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= E.Range && Vector3.Distance(Game.CursorPos, Target.ServerPosition) <= E.Range)
                        {
                            E.Cast(Player.ServerPosition.Extend(Game.CursorPos, Vector3.Distance(Player.ServerPosition, Game.CursorPos)), Target.ServerPosition);
                        }
                        else
                        {
                            E.Cast(Player.ServerPosition.Extend(Target.ServerPosition, E.Range), Target.ServerPosition);
                        }
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
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("EC.Viktor.Misc.W").GetValue<bool>() && W.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= W.Range + (W.Width/2))
                {
                    if (myUtility.ImmuneToMagic(sender) || myUtility.ImmuneToCC(sender)) return;                    
                    Vector3 pos = myUtility.RandomPos(1, 25, 25, Player.ServerPosition.Extend(sender.ServerPosition, Vector3.Distance(Player.ServerPosition, sender.ServerPosition)));
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast(pos));
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("EC.Viktor.Misc.W2").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= W.Range + (W.Width / 2))
                {
                    if (myUtility.ImmuneToMagic(gapcloser.Sender) || myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Vector3 pos = myUtility.RandomPos(1, 25, 25, gapcloser.End);
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast(pos));
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Viktor.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Viktor.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Viktor.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (config.Item("EC.Viktor.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
