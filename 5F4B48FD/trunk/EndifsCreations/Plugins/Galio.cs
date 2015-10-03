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
    class Galio : PluginData
    {
        public Galio()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 850);//940
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 1180);
            R = new Spell(SpellSlot.R, 540);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Galio.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Galio.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Galio.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Galio.Combo.R", "Use R").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Galio.Misc.W", "W Shields").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Galio.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Galio.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Galio.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Galio.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            var UseQ = Root.Item("EC.Galio.Combo.Q").GetValue<bool>();
            var UseE = Root.Item("EC.Galio.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Galio.Combo.R").GetValue<bool>();
            if (UseR && R.IsReady())
            {
                mySpellcast.PointBlank(null, R, R.Range, 3);
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.Linear(Target, Q, HitChance.High);
                    }
                    if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
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
                case myOrbwalker.OrbwalkingMode.Harass:
                    Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                    if (Target.IsValidTarget() && Q.IsReady())
                    {
                        mySpellcast.Linear(Target, Q, HitChance.High);
                    }
                    break;
            }            
        }

        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "galioresolutesmite") || (spell.SData.Name.ToLower() == "galiobulwark") || (spell.SData.Name.ToLower() == "galiorighteousgust"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
            if (unit is Obj_AI_Hero && unit.IsEnemy && !spell.SData.IsAutoAttack() && E.IsReady())
            {
                if (Root.Item("EC.Galio.Misc.W").GetValue<bool>() || (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Galio.Combo.W").GetValue<bool>()))                     
                {
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        var box = new Geometry.Polygon.Rectangle(spell.Start, spell.End, Player.BoundingRadius);
                        if (box.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 100))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.CastOnUnit(Player));
                        }
                    }
                    else if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Unit) || spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit)) && spell.Target != null && spell.Target.IsMe)
                    {
                        W.CastOnUnit(Player);
                    }
                    else if (spell.End.Distance(Player.ServerPosition) <= 100)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.CastOnUnit(Player));
                    }
                }
            }   
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.R) //Player.HasBuff("GalioIdolOfDurand")
            {
                if (!Player.HasBuff("GalioBulwark") && W.IsReady() && Root.Item("EC.Galio.Combo.W").GetValue<bool>())
                {
                    W.CastOnUnit(Player);
                }
                LastR = myUtility.TickCount;                
                mySpellcast.Pause(1000 + Game.Ping);
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (Player.IsChannelingImportantSpell() || myUtility.TickCount - LastR <= 0.5f)
            {
                args.Process = false;
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Galio.Draw.W").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Galio.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Galio.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Galio.Draw.R").GetValue<bool>() && R.Level > 0)
            {        
                var color = Player.CountEnemiesInRange(R.Range) >= 4 ? Color.Red : Color.Yellow;
                Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, color, "Hits: " + Player.CountEnemiesInRange(R.Range));
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
