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
    class Diana : PluginData
    {
        public Diana()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 830);
            W = new Spell(SpellSlot.W, 200);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 825);
           
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Diana.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Diana.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Diana.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Diana.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Diana.Combo.R2", "Use R (Second)").SetValue(true));                
                combomenu.AddItem(new MenuItem("EC.Diana.Combo.Dive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Diana.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Diana.Misc.W", "W Shields").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Diana.Misc.E", "E Interrupts").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Diana.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Diana.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Diana.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Diana.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            var UseQ = config.Item("EC.Diana.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Diana.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Diana.Combo.E").GetValue<bool>();
            var UseR = config.Item("EC.Diana.Combo.R").GetValue<bool>();
            var UseR2 = config.Item("EC.Diana.Combo.R2").GetValue<bool>();

            Target = myUtility.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (!Player.IsDashing())
            {
                if (UseW && W.IsReady())
                {
                    if (Player.CountEnemiesInRange(W.Range) > 0)
                    {
                        W.Cast();
                    }
                }
                if (UseE && E.IsReady())
                {
                    if (Player.CountEnemiesInRange(E.Range) > 0)
                    {
                        E.Cast();
                    }
                }
            }
            if (LastTarget == null && (Target != null && Target.IsValidTarget()))
            {
                LastTarget = Target;
            }
            if (LastTarget != null)
            {
                if (UseQ && Q.IsReady())
                {
                    if (Player.IsDashing())
                    {
                        Q.Cast(Player.GetDashInfo().EndPos.To3D());
                    }
                    else
                    {
                        mySpellcast.LinearVector(LastTarget.ServerPosition, Q, LastTarget.BoundingRadius);
                    }
                }
                if (UseR && R.IsReady() && myUtility.TickCount - LastR > 1000)
                {
                    if (LastTarget.HasBuff("dianamoonlight"))
                    {
                        mySpellcast.Unit(LastTarget, R);
                    }
                    else if (CresentStrike != null && Vector3.Distance(LastTarget.Position, CresentStrike.Position) <= Target.BoundingRadius)
                    {
                        mySpellcast.Unit(LastTarget, R);
                    }
                }
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
        private GameObject CresentStrike;
        protected override void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Diana") && sender.Name.Contains("_Q_End"))
            {
                CresentStrike = sender;
            }
        }
        protected override void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Diana") && sender.Name.Contains("_Q_End"))
            {
                CresentStrike = null;
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "dianaarc") || (spell.SData.Name.ToLower() == "dianaorbs") || (spell.SData.Name.ToLower() == "dianavortex"))
                {
                    LastSpell = myUtility.TickCount;
                }
                if (spell.SData.Name.ToLower() == "dianateleport")
                {
                    LastR = myUtility.TickCount;
                }
            }
            if (unit is Obj_AI_Hero && unit.IsEnemy && !spell.SData.IsAutoAttack() && W.IsReady())
            {
                if ((config.Item("EC.Diana.Misc.W").GetValue<bool>()))
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
        protected override void OnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
            {
                if (sender.IsMe)
                {
                    Utility.DelayAction.Add(50, () =>
                    {
                        W.Cast();
                        E.Cast();
                        myItemManager.UseItems(2, null);
                    }
                    );
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("EC.Diana.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(sender)) return;
                    E.Cast();
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Diana.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Diana.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Diana.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (config.Item("EC.Diana.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
