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
    class Braum : PluginData
    {
        public Braum()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W, 650);
            E = new Spell(SpellSlot.E, 0);
            R = new Spell(SpellSlot.R, 1200);

            Q.SetSkillshot(0.25f, 60f, 1700f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.5f, 115f, 1400f, false, SkillshotType.SkillshotLine);                   
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Braum.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Braum.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Braum.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Braum.Combo.R", "Use R").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Braum.Misc.Q", "Q Gapclosers").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Braum.Misc.W", "W Shields").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Braum.Misc.E", "E Blocks").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Braum.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Braum.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Braum.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Braum.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Braum.Combo.W").GetValue<bool>();
            var UseR = config.Item("EC.Braum.Combo.R").GetValue<bool>();
            if (UseW && W.IsReady() && Player.CountEnemiesInRange(200) > 0)
            {
                W.Cast(Player);
            }
            if (UseR && R.IsReady())
            {
                mySpellcast.LinearBox(R, HitChance.High, 3);
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                       mySpellcast.Linear(Target, Q, HitChance.High, true);
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
            if (unit is Obj_AI_Hero && unit.IsEnemy)
            {
                if (config.Item("EC.Braum.Misc.E").GetValue<bool>() || (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && config.Item("EC.Braum.Combo.E").GetValue<bool>()) && E.IsReady())
                {
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        var box = new Geometry.Polygon.Rectangle(spell.Start, spell.End, Player.BoundingRadius);
                        if (box.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 100))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.Cast(spell.Start));      
                        }
                    }
                    else if (spell.End.Distance(Player.ServerPosition) <= 100)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.Cast(spell.Start));                      
                    }
                }
                if (config.Item("EC.Braum.Misc.W").GetValue<bool>() || (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && config.Item("EC.Braum.Combo.W").GetValue<bool>()) && W.IsReady())
                {
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        var box = new Geometry.Polygon.Rectangle(spell.Start, spell.End, Player.BoundingRadius);
                        if (box.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 100))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast());
                        }
                    }
                    else if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Unit) || spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit)) && spell.Target != null && spell.Target.IsMe)
                    {
                        W.Cast();
                    }
                    else if (spell.End.Distance(Player.ServerPosition) <= 100)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast());
                    }
                    else if (spell.SData.IsAutoAttack() && spell.Target != null && spell.Target.IsMe)
                    {
                        W.Cast();
                    }
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("EC.Braum.Misc.Q").GetValue<bool>() && Q.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= Q.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Linear(gapcloser.Sender, Q, HitChance.High, true));
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Braum.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Braum.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Braum.Draw.R").GetValue<bool>() && R.Level > 0 && R.IsReady())
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
