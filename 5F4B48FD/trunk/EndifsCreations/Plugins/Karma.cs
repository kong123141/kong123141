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
    class Karma : PluginData
    {
        public Karma()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 950); 
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R);

           // Q2 = new Spell(SpellSlot.Q, 950); //250 rad

            Q.SetSkillshot(0.25f, 90f, 1700f, true, SkillshotType.SkillshotLine);
            W.SetTargetted(0.25f, float.MaxValue);
            E.SetTargetted(0.25f, float.MaxValue);
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Karma.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Karma.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Karma.Combo.E", "Use E").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Karma.Misc.W", "W Gapclosers").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Karma.Misc.E", "E Shields").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Karma.UseESupport", "E Supports").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Karma.Draw.Q", "Q").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Karma.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Karma.Combo.W").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        PredictionOutput pred = Q.GetPrediction(Target);
                        if (pred.CollisionObjects.Count == 0 && Vector3.Distance(Player.ServerPosition, pred.CastPosition) <= Q.Range)
                        {
                            if (pred.Hitchance >= HitChance.High)
                            {
                                if ((pred.CastPosition.CountEnemiesInRange(250) > 1 ||  pred.Hitchance == HitChance.VeryHigh || myUtility.MovementDisabled(Target)) && R.IsReady())
                                {
                                    R.Cast();
                                }
                                mySpellcast.Linear(Target, Q, HitChance.High); 
                            }
                        }
                    }
                    if (UseW && W.IsReady())
                    {
                        if (myUtility.MovementDisabled(Target))
                        {
                            if (myUtility.PlayerHealthPercentage < 50 && R.IsReady())
                            {
                                R.Cast();                                
                            }
                            mySpellcast.Unit(Target, W);
                        }                        
                        if (myUtility.ImmuneToCC(Target)) return;
                        if (myUtility.PlayerHealthPercentage < 50 && R.IsReady())
                        {
                            R.Cast();                            
                        }
                        mySpellcast.Unit(Target, W);
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
            if (unit is Obj_AI_Hero && unit.IsAlly && !unit.IsMe)
            {
                if (config.Item("EC.Karma.UseESupport").GetValue<bool>())
                {
                    if (spell.Target is Obj_AI_Hero && spell.SData.IsAutoAttack() && spell.Target.IsEnemy)
                    {
                        mySpellcast.Unit((Obj_AI_Hero)unit, E);
                    }
                }
            }
            if (unit is Obj_AI_Hero && unit.IsEnemy)
            {
                if (config.Item("EC.Karma.Misc.E").GetValue<bool>() || (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && config.Item("EC.Karma.Combo.E").GetValue<bool>()) && E.IsReady())
                {
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        var box = new Geometry.Polygon.Rectangle(spell.Start, spell.End, Player.BoundingRadius);
                        if (box.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 100))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () =>
                            {
                                if (Player.CountAlliesInRange(600) > 0 && R.IsReady())
                                {
                                    R.Cast();
                                    mySpellcast.Unit(null, E);
                                }
                                else
                                {
                                    mySpellcast.Unit(null, E);
                                }
                            });
                        }
                    }
                    else if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Unit) || spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit)) && spell.Target != null && spell.Target.IsMe)
                    {
                        mySpellcast.Unit(null, E);
                    }
                    else if (spell.End.Distance(Player.ServerPosition) <= 100)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Unit(null, E));
                    }
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("EC.Karma.Misc.W").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= W.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;
                    if (myUtility.PlayerHealthPercentage < 50 && R.IsReady())
                    {
                        R.Cast();
                    }
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Unit(gapcloser.Sender, E));                   
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Karma.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {                
                if (Player.HasBuff("KarmaMantra"))
                {
                    Vector3 vec = Player.ServerPosition.Extend(Game.CursorPos, Q.Range);
                    Render.Circle.DrawCircle(vec, 250, Color.Cyan);
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Color.Cyan);
                }
                else
                {
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
                }
            }
        }
    }
}
//KarmaMantra