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
    class Gragas : PluginData
    {
        public Gragas()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 775);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 675);
            R = new Spell(SpellSlot.R, 1100);

            Q.SetSkillshot(0.3f, 110f, 1000f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.0f, 50, 1000, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.3f, 700, 1000, false, SkillshotType.SkillshotCircle);      
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Gragas.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Gragas.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Gragas.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Gragas.Combo.R", "Use R").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Gragas.Misc.W", "W Misc").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Gragas.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Gragas.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Gragas.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range + 150, TargetSelector.DamageType.Magical);

            var UseQ = Root.Item("EC.Gragas.Combo.Q").GetValue<bool>();
            var UseE = Root.Item("EC.Gragas.Combo.E").GetValue<bool>();
            if (UseQ && Q.IsReady() && Barrel != null && Q.Instance.SData.Name.Equals("gragasqtoggle"))
            {
                if (Target.IsValidTarget() && Vector3.Distance(Target.Position, Barrel.Position) <= 300)
                {
                    if (myUtility.MovementDisabled(Target))
                    {
                        Q.Cast();
                    }
                    else if (Q.IsKillable(Target))
                    {
                        Q.Cast();
                    }
                    else
                    {
                        Q.Cast();
                    }
                }
                else
                {
                    if (Barrel.Position.CountEnemiesInRange(300) > 0)
                    {
                        Q.Cast();
                    }
                }

            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        if (Barrel == null && !Q.Instance.SData.Name.Equals("gragasqtoggle"))
                        {                                                        
                            PredictionOutput pred = Q.GetPrediction(Target);
                            if (myUtility.MovementDisabled(Target))
                            {
                                Q.Cast(Player.ServerPosition.Extend(Target.ServerPosition, Vector3.Distance(Player.ServerPosition, Target.ServerPosition)));
                            }
                            if (Vector3.Distance(Player.ServerPosition, pred.CastPosition) <= Q.Range + 150)
                            {
                                if (pred.Hitchance >= HitChance.High)
                                {
                                    Q.Cast(Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition)));                                    
                                }
                            }
                        }
                    }
                    if (UseE && E.IsReady())
                    {
                        if (UseQ && Q.IsReady()) return;
                        PredictionOutput pred = E.GetPrediction(Target);
                        if (Orbwalking.InAutoAttackRange(Target))
                        {
                            E.Cast(Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, Target.ServerPosition) + Target.BoundingRadius));
                        }
                        if (pred.CollisionObjects.Count == 0 && myUtility.MovementDisabled(Target))
                        {
                            E.Cast(Player.ServerPosition.Extend(Target.ServerPosition, Vector3.Distance(Player.ServerPosition, Target.ServerPosition)));
                        }
                        if (Vector3.Distance(Player.ServerPosition, pred.CastPosition) <= E.Range - Target.BoundingRadius)
                        {
                            if (pred.Hitchance >= HitChance.High)
                            {
                                E.Cast(Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition) + Target.BoundingRadius));
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private GameObject Barrel;       

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
        protected override void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Gragas"))
            {
                if (sender.Name.Contains("_Q_Ally"))
                {
                    Barrel = sender;
                }
                if (sender.Name.Contains("_Q_End"))
                {
                                     
                }
            }
        }
        protected override void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Gragas"))
            {
                if (sender.Name.Contains("_Q_Ally"))
                {
                    //Barrel = null;
                }
                if (sender.Name.Contains("_Q_End"))
                {
                    Barrel = null;
                }
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if (spell.SData.Name.ToLower() == "gragasq")
                {
                    LastQ = myUtility.TickCount;   
                }
            }
            if (unit is Obj_AI_Hero && unit.IsEnemy)
            {
                if (Root.Item("EC.Gragas.Misc.W").GetValue<bool>() || (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Gragas.Combo.W").GetValue<bool>()) && E.IsReady())
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
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Gragas.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
                if (Barrel != null)
                {
                    Render.Circle.DrawCircle(Barrel.Position, 300, Color.White);    
                }
            }
        }
    }
}
