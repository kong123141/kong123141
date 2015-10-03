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
    class Syndra : PluginData
    {
        public Syndra()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 800);
            W = new Spell(SpellSlot.W, 925); //950
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 675);

            E2 = new Spell(SpellSlot.E, 1100);
            R2 = new Spell(SpellSlot.R, 750);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellList.Add(R2);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Syndra.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Syndra.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Syndra.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Syndra.Combo.R", "Use R").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Syndra.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Syndra.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Syndra.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Syndra.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(E2.Range, TargetSelector.DamageType.Magical);

            var UseQ = Root.Item("EC.Syndra.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Syndra.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Syndra.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Syndra.Combo.R").GetValue<bool>();
           
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        if (!Player.HasBuff("syndrawtooltip"))
                        {
                            mySpellcast.CircularPrecise(Target, Q, HitChance.High, Q.Range, 75);
                        }
                    }
                    if (UseW && W.IsReady())
                    {
                        if (UseQ && Q.CanCast(Target)) return;
                        if (!Player.HasBuff("syndrawtooltip") && AllSpheres.Any())
                        {
                            foreach (var balls in AllSpheres.OrderBy(x => x.NetworkId))
                            {
                                W.Cast(balls);
                                return;
                            }
                        }
                        else if (myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                        {
                            mySpellcast.PointVector(Target.Position, W, Target.BoundingRadius);
                        }
                    }
                    if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        var box = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(Target.ServerPosition, E.Range), 50);
                        if (AllSpheres.Any(x => box.IsInside(x)))
                        {
                            mySpellcast.PointVector(box.End.To3D(), E, E2.Range);
                        }
                    }
                    if (UseR && R.IsReady() && R.Instance.Ammo >= 5)
                    {
                        R.Cast(Target);
                    }
                }
                catch { }
            }
        }

        private static List<Obj_AI_Minion> DarkSphere = new List<Obj_AI_Minion>();
        private static List<Obj_AI_Minion> AllSpheres
        {
            get { return DarkSphere.Where(s => s.IsValid && !s.IsMoving).ToList(); }
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
                    if (Target.IsValidTarget() && Q.IsReady())
                    {
                        mySpellcast.CircularPrecise(Target, Q, HitChance.High, Q.Range, 75);
                    }
                    break;  
            }
        }        
        protected override void OnCreate(GameObject sender, EventArgs args)
        {            
            if (sender is Obj_AI_Minion  && sender.Name.Contains("Seed"))
            {
                DarkSphere.Add((Obj_AI_Minion)sender);
            }
        }
        protected override void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender is Obj_AI_Minion && sender.Name.Contains("Seed"))
            {
                DarkSphere.RemoveAll(s => s.NetworkId == sender.NetworkId);
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "syndraq") || (spell.SData.Name.ToLower() == "syndraw") || (spell.SData.Name.ToLower() == "syndrawcast") || (spell.SData.Name.ToLower() == "syndrae"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Syndra.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Syndra.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Syndra.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
                if (AllSpheres.Count > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, E2.Range, Color.Cyan);
                }
            }
            if (Root.Item("EC.Syndra.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
