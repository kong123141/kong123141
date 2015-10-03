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
    class Lux : PluginData
    {
        public Lux()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1075);
            R = new Spell(SpellSlot.R, 3300);

            Q.SetSkillshot(0.25f, float.MaxValue, 1300f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, float.MaxValue, 1200f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, float.MaxValue, 1300f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1f, 190f, float.MaxValue, false, SkillshotType.SkillshotLine);         
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Lux.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Lux.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Lux.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Lux.Combo.R", "Use R").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Lux.Misc.Q", "Q Gapclosers").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Lux.Misc.W", "W Shields").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Lux.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Lux.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Lux.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = Root.Item("EC.Lux.Combo.Q").GetValue<bool>();
            var UseE = Root.Item("EC.Lux.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Lux.Combo.R").GetValue<bool>();
            if (UseR && R.IsReady())
            {
                if (Target.IsValidTarget() && myUtility.MovementDisabled(Target))
                {
                    if (R.IsKillable(Target) && !myUtility.ImmuneToMagic(Target))
                    {
                        R.Cast(Target.Position);
                    }
                }
                else
                {
                    mySpellcast.LinearBox(R, HitChance.High, 3);
                }
            }
            if (UseE && E.IsReady() && LucentSingularity != null)
            {
                if (Target.IsValidTarget() && Vector3.Distance(Target.Position, LucentSingularity.Position) <= 300)
                {
                    if (myUtility.MovementDisabled(Target))
                    {
                        E.Cast();
                    }
                    if (E.IsKillable(Target))
                    {
                        E.Cast();
                    }
                }
                else if (LucentSingularity.Position.CountEnemiesInRange(300) > 0)
                {
                    E.Cast();
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastE > myHumazier.SpellDelay)
                    {
                        mySpellcast.Linear(Target, Q, HitChance.High, true, 1);
                    }
                    if (UseE && E.IsReady() && myUtility.TickCount - LastQ > myHumazier.SpellDelay)
                    {
                        if (LucentSingularity == null)
                        {                                                        
                            PredictionOutput pred = E.GetPrediction(Target);
                            if (myUtility.MovementDisabled(Target))
                            {
                                E.Cast(Player.ServerPosition.Extend(Target.ServerPosition, Vector3.Distance(Player.ServerPosition, Target.ServerPosition)));
                            }
                            if (UseQ && Q.IsReady()) return;
                            if (Vector3.Distance(Player.ServerPosition, pred.CastPosition) <= E.Range + 150)
                            {
                                if (pred.Hitchance >= HitChance.High)
                                {
                                    E.Cast(Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition)));                                    
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        }
        private void Harass()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            
            if (Target.IsValidTarget())
            {
                try
                {
                    if (Q.IsReady())
                    {
                        mySpellcast.Linear(Target, Q, HitChance.High, true, 1);
                    }
                }
                catch { }
            }
        }
        
        private GameObject LucentSingularity;  

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
                    if (Q.IsReady())
                    {
                        myFarmManager.LaneLinear(Q, Q.Range, false, true, 1);
                    }
                    break;
            }
        }   
        protected override void OnCreate(GameObject sender, EventArgs args)
        {            
            if (sender.Name.Contains("Lux") && sender.Name.Contains("_E"))
            {
                LucentSingularity = sender;
            }
        }
        protected override void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Lux") && sender.Name.Contains("_E"))
            {
                LucentSingularity = null;
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if (spell.SData.Name.ToLower() == "luxlightbinding")
                {
                    LastQ = myUtility.TickCount;
                }
                if (spell.SData.Name.ToLower() == "luxlightstrikekugel")
                {
                    LastE = myUtility.TickCount;
                }
            }
            if (unit is Obj_AI_Hero && unit.IsEnemy)
            {
                if (Root.Item("EC.Lux.Misc.W").GetValue<bool>() || (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Lux.Combo.W").GetValue<bool>()) && W.IsReady())
                {
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        var box = new Geometry.Polygon.Rectangle(spell.Start, spell.End, Player.BoundingRadius);
                        if (box.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 100))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast(Player.ServerPosition));                            
                        }
                    }
                    else if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Unit) || spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit)) && spell.Target != null && spell.Target.IsMe)
                    {
                        W.Cast(Player.ServerPosition);
                    }
                    else if (spell.End.Distance(Player.ServerPosition) <= 100)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast(Player.ServerPosition));
                    }
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Lux.Misc.Q").GetValue<bool>() && Q.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) <= Q.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;                   
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () =>  mySpellcast.Linear(gapcloser.Sender, Q, HitChance.High, true, 1));
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Lux.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Lux.Draw.R").GetValue<bool>() && R.Level > 0 && R.IsReady())
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
