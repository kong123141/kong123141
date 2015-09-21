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
    class Sion : PluginData
    {
        public Sion()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {

            Q = new Spell(SpellSlot.Q, 300); //600 max, 2second charge, 1 second charge = knocks up
            W = new Spell(SpellSlot.W, 325);
            E = new Spell(SpellSlot.E, 725); 
            R = new Spell(SpellSlot.R);

            E2 = new Spell(SpellSlot.E, 1500); 

            Q.SetSkillshot(0.6f, 100f, float.MaxValue, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 80f, 1800, false, SkillshotType.SkillshotLine);

            E2.SetSkillshot(0.25f, 80f, 1800, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellList.Add(E2);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Sion.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Sion.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Sion.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Sion.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Sion.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Sion.Misc.W", "W Shields").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Sion.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Sion.Draw.E", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            var UseQ = config.Item("EC.Sion.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Sion.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Sion.Combo.E").GetValue<bool>();
            var CastItems = config.Item("EC.Sion.Combo.Items").GetValue<bool>();
            if (UseQ)
            {
                if (Q.IsCharging && myUtility.TickCount - LastQ > 1000)
                {
                    Q.Cast();
                }
            }
            if (UseW && W.IsReady())
            {
                if (Player.CountEnemiesInRange(500) > 0 && myUtility.TickCount - LastW > 3000 && Player.HasBuff("sionwshieldstacks"))
                {
                    W.Cast();
                }
            }
            if (UseE && E.IsReady())
            {
                if (!Target.IsValidTarget())
                {
                    mySpellcast.Extension(null, E, E.Range, E2.Range);
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {
                    if (myUtility.ImmuneToDeath(Target)) return;
                    if (UseQ && Q.IsReady())
                    {
                        if (Q.IsInRange(Target) && myUtility.MovementDisabled(Target))
                        {
                            Q.Cast(Target.ServerPosition);
                        }
                    }
                    if (UseE && E.IsReady())
                    {
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= E.Range)
                        {
                            mySpellcast.Linear(Target, E, HitChance.High);
                        }
                        else if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) > 725 && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= 1500)
                        {
                            mySpellcast.Extension(Target, E, E.Range, E2.Range);
                        }
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
            if (Player.IsZombie)
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable);
                foreach (var q in EnemyList.OrderBy(i => i.Distance(Player)))
                {
                    Q.Cast();
                    if (!Player.IsWindingUp && myUtility.TickCount - LastOrder > 1000 )
                    {
                        Player.IssueOrder(GameObjectOrder.AttackUnit, q);
                        LastOrder = myUtility.TickCount;
                    }
                }
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
                if (spell.SData.Name.ToLower() == "sionq")
                {
                    LastQ = myUtility.TickCount;
                }
                if (spell.SData.Name.ToLower() == "sionw")
                {
                    LastW = myUtility.TickCount;
                }
            }
            if (unit is Obj_AI_Hero && unit.IsEnemy)
            {
                if ((config.Item("EC.Sion.Misc.W").GetValue<bool>() || (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && config.Item("EC.Sion.Combo.W").GetValue<bool>())) && W.IsReady())
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
            if (config.Item("EC.Sion.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Sion.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
                Render.Circle.DrawCircle(Player.Position, E2.Range, Color.Cyan);
            }            
        }
    }
}
