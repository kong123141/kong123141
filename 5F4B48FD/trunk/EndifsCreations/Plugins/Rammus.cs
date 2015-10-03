using System;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
{
    class Rammus : PluginData
    {
        public Rammus()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R, 300);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Rammus.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Rammus.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Rammus.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Rammus.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Rammus.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Rammus.Misc.W", "W Shields").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Rammus.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Rammus.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(375 + Player.BoundingRadius, TargetSelector.DamageType.Physical);

            var UseW = Root.Item("EC.Rammus.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Rammus.Combo.E").GetValue<bool>();
            var CastItems = Root.Item("EC.Rammus.Combo.Items").GetValue<bool>();
            var UseR = Root.Item("EC.Rammus.Combo.R").GetValue<bool>();
            if (UseE && E.IsReady())
            {
                if (Target.IsValidTarget() && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= E.Range)
                {
                    if (Player.HasBuff("DefensiveBallCurl"))
                    {
                        if (UseW && W.IsReady()) W.Cast();
                        E.CastOnUnit(Target);
                    }
                    else 
                    {                        
                        E.CastOnUnit(Target);
                    }
                }
                else
                {
                    var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !myUtility.ImmuneToMagic(x));
                    var etarget = EnemyList.Where(x => x.IsVisible &&
                                 Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= E.Range &&
                                 x != Target).OrderBy(i => i.BaseAttackDamage).ThenBy(i => i.Health).FirstOrDefault();
                    if (etarget != null && etarget.IsValidTarget())
                    {
                        if (Player.HasBuff("DefensiveBallCurl"))
                        {
                            E.CastOnUnit(etarget);
                        }
                        else
                        {
                            if (UseW && W.IsReady()) W.Cast();
                            E.CastOnUnit(etarget);
                        }
                    }
                }

            }
           
            if (Target.IsValidTarget())
            {
                if (UseR && R.IsReady())
                {
                    mySpellcast.PointBlank(Target, R, R.Range, 2);
                }
                if (CastItems)
                {
                    if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= 450f)
                    {
                        myItemManager.UseItems(1, Target);
                    }
                    if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) < 500f)
                    {
                        myItemManager.UseItems(3, null);
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
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {            
            if (unit is Obj_AI_Hero && unit.IsEnemy)
            {
                if (Root.Item("EC.Rammus.Misc.W").GetValue<bool>() || (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Rammus.Combo.W").GetValue<bool>()) && W.IsReady())
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
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Rammus.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Rammus.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
