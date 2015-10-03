using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCreations.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
{
    class Tristana : PluginData
    {
        public Tristana()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 543);
            R = new Spell(SpellSlot.R, 543);

            W.SetSkillshot(0.25f, 150, 1200, false, SkillshotType.SkillshotCircle);
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Tristana.Combo.Q", "Use Q").SetValue(true));
                //combomenu.AddItem(new MenuItem("EC.Tristana.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Tristana.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Tristana.Combo.R", "Use R").SetValue(true));
                Root.AddSubMenu(combomenu);
            }            
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Tristana.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Tristana.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Tristana.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Tristana.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            var UseR = Root.Item("EC.Tristana.Combo.R").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                try
                {
                    if (UseR && R.IsReady())
                    {
                        if (Target.CountEnemiesInRange(200) > 1)
                        {
                            mySpellcast.Unit(Target, R);
                        }
                        else if (R.IsKillable(Target))
                        {
                            mySpellcast.Unit(Target, R);
                        }                        
                    }
                }
                catch { }
            }
        }
        private void UpdateER()
        {
            if (E.Level > 0)
            {
                E.Range = 543 + (7 * Player.Level);
            }
            if (R.Level > 0)
            {
                R.Range = 543 + (7 * Player.Level);
            }
        }

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                myUtility.Reset();
                return;
            }
            UpdateER();
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
                    if (Root.Item("EC.Tristana.Combo.Q").GetValue<bool>() && Q.IsReady())
                    {
                        Q.Cast();
                    }
                    if (Root.Item("EC.Tristana.Combo.E").GetValue<bool>() && E.IsReady())
                    {
                        mySpellcast.Unit((Obj_AI_Hero)args.Target, E);
                    }
                }
            }
            if (args.Target is Obj_AI_Turret && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear && E.IsInRange(args.Target))
                {
                    E.CastOnUnit(args.Target);
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Tristana.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Tristana.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Tristana.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Tristana.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
