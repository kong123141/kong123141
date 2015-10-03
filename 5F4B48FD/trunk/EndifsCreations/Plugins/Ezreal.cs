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
    class Ezreal : PluginData
    {
        public Ezreal()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1150);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 475); //750
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.25f, 40f, 2000f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 60f, 1600f, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Ezreal.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Ezreal.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Ezreal.Combo.E", "Use E").SetValue(true));
                //combomenu.AddItem(new MenuItem("EC.Ezreal.Combo.R", "Use R").SetValue(true));
                Root.AddSubMenu(combomenu);
            }            
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Ezreal.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Ezreal.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Ezreal.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Ezreal.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(W.Range, TargetSelector.DamageType.Magical, true);

            var UseQ = Root.Item("EC.Ezreal.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Ezreal.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Ezreal.Combo.E").GetValue<bool>();
            //var UseR = Root.Item("EC.Ezreal.Combo.R").GetValue<bool>();

            if (Target.IsValidTarget())
            {
                try
                {
                    if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        Vector3 vec = Player.ServerPosition.Extend(Game.CursorPos, E.Range);
                        if (Q.IsReady())
                        {
                            Q.UpdateSourcePosition(vec);
                        }
                        if (W.IsReady())
                        {
                            W.UpdateSourcePosition(vec);
                        }
                        if (myUtility.EnoughHealth(33) && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) > Vector3.Distance(vec, Target.ServerPosition))
                        {
                            E.Cast(vec);
                        }
                        else if (!myUtility.EnoughHealth(33) && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) < Vector3.Distance(vec, Target.ServerPosition))
                        {
                            E.Cast(vec);
                        }
                    }

                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.Linear(Target, Q, HitChance.High, true);
                    }

                    if (UseW && W.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.Linear(Target, W, HitChance.High);
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
                    if (Target.IsValidTarget())
                    {
                        if (Q.IsReady() && Q.IsInRange(Target))
                        {
                            mySpellcast.Linear(Target, Q, HitChance.High, true);
                        }
                        else if (W.IsReady() && W.IsInRange(Target))
                        {
                            mySpellcast.PointVector(Target.Position, W, Target.BoundingRadius);
                        }
                    }
                    break;
            }            
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "ezrealmysticshot") || (spell.SData.Name.ToLower() == "ezrealessenceflux") || (spell.SData.Name.ToLower() == "ezrealarcaneshift"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Ezreal.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Ezreal.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Ezreal.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Ezreal.Draw.R").GetValue<bool>() && R.Level > 0 && R.IsReady())
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
