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
    class Draven : PluginData
    {
        public Draven()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1050);
            R = new Spell(SpellSlot.R);

            E.SetSkillshot(0.25f, 130, 1400, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.4f, 160, 2000, true, SkillshotType.SkillshotLine);
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Draven.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Draven.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Draven.Combo.E", "Use E").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Draven.Misc.E", "E Gapclosers").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Draven.Misc.E2", "E Interrupts").SetValue(false));
                
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Draven.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Draven.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Draven.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Draven.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            var UseE = Root.Item("EC.Draven.Combo.E").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                try
                {
                    if (UseE && E.IsReady() && !myUtility.ImmuneToCC(Target))
                    {
                        mySpellcast.Linear(Target, E, HitChance.High);
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
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (Root.Item("EC.Draven.Combo.Q").GetValue<bool>() && Q.IsReady())
                    {
                        Q.Cast();
                    }
                    if (Root.Item("EC.Draven.Combo.W").GetValue<bool>() && W.IsReady() && Player.HasBuff("dravenspinningattack") && !Player.HasBuff("dravenfurybuff"))
                    {
                        W.Cast();
                    }
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Draven.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.PointVector(gapcloser.End,E));                    
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Root.Item("EC.Draven.Misc.E2").GetValue<bool>() && E.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Linear(sender, Q, HitChance.Medium));
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Draven.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Draven.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Draven.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Draven.Draw.R").GetValue<bool>() && R.Level > 0 && R.IsReady())
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
