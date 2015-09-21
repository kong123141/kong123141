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
    class Kennen : PluginData
    {
        public Kennen()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1050);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 100); 
            R = new Spell(SpellSlot.R, 550);

            Q.SetSkillshot(0.25f, 50, 1700, true, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Kennen.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Kennen.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Kennen.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Kennen.Combo.R", "Use R").SetValue(true));
                config.AddSubMenu(combomenu);
            }            
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Kennen.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Kennen.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Kennen.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Kennen.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Kennen.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Kennen.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Kennen.Combo.E").GetValue<bool>();
            var UseR = config.Item("EC.Kennen.Combo.R").GetValue<bool>();
            if (UseW && W.IsReady())
            {
                if (Player.HasBuff("KennenShurikenStorm") && Player.CountEnemiesInRange(R.Range) >= 4)
                {
                    W.Cast();
                }
            }
            if (UseE && E.IsReady())
            {
                if (Player.HasBuff("KennenShurikenStorm"))
                {
                    E.Cast();
                }
            }
            if (UseR && R.IsReady())
            {
                if (Player.CountEnemiesInRange(R.Range) >= 4)
                {
                    R.Cast();
                }
            }
            if (Target.IsValidTarget())
            {
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.Linear(Target, Q, HitChance.High, true);
                    }
                    if (UseW && W.IsReady() && W.IsInRange(Target))
                    {
                        if (W.IsKillable(Target))
                        {
                            W.Cast();
                        }
                        else if (Target.GetBuffCount("kennenmarkofstorm") >= 1 && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                        {
                            W.Cast();
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
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "kennenshurikenhurlmissile1") || (spell.SData.Name.ToLower() == "kennenbringthelight") || (spell.SData.Name.ToLower() == "kennenlightningrush"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Kennen.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Kennen.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Kennen.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (config.Item("EC.Kennen.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.Yellow, "Hits: " + Player.CountEnemiesInRange(R.Range));
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
