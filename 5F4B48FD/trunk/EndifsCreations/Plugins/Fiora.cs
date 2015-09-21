#region Todo
    //      Riposte allow/ignore list
#endregion Todo

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
    class Fiora : PluginData
    {
        public Fiora()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 400);
            W = new Spell(SpellSlot.W, 750);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);
            W.SetSkillshot(0.75f, 80, 2000, false, SkillshotType.SkillshotLine);
           
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Fiora.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Fiora.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Fiora.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Fiora.Combo.R", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Fiora.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Fiora.Misc.W", "W Spell Block").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Fiora.Draw.Q", "Q").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            var UseQ = config.Item("EC.Fiora.Combo.Q").GetValue<bool>();
            var UseR = config.Item("EC.Fiora.Combo.R").GetValue<bool>();
            var CastItems = config.Item("EC.Fiora.Combo.Items").GetValue<bool>(); 
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        Vector3 vec = Player.ServerPosition.Extend(Game.CursorPos, Q.Range);
                        if (Vector3.Distance(vec, Target.ServerPosition) <= 200)
                        {
                            Q.Cast(vec);
                        }
                    }
                    if (UseR && R.IsReady())
                    {
                        if (Target.HealthPercent <= 33)
                        {
                            R.Cast(Target);
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
        //Only AOE with CC or sheits. no dots like morgana
        private static readonly string[] IgnoreSpellBlock =
        {
            "tormentedsoil",
        };
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
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R && config.Item("EC.Fiora.Combo.Items").GetValue<bool>())
            {
                Utility.DelayAction.Add(myHumazier.ReactionDelay, () => myItemManager.UseGhostblade());
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit is Obj_AI_Hero && unit.IsEnemy && !spell.SData.IsAutoAttack() && W.IsReady())
            {
                if ((myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && config.Item("EC.Fiora.Combo.W").GetValue<bool>()) || (config.Item("EC.Fiora.Misc.W").GetValue<bool>()))
                {
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        var box = new Geometry.Polygon.Rectangle(spell.Start, spell.End, Player.BoundingRadius);
                        if (box.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 100))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast(spell.Start));
                        }
                    }
                    else if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Unit) || spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit)) && spell.Target != null && spell.Target.IsMe)
                    {
                        W.Cast(spell.Start);
                    }
                    else if (spell.SData.TargettingType.Equals(SpellDataTargetType.LocationAoe) && !IgnoreSpellBlock.Contains(spell.SData.Name.ToLower()) && spell.End.Distance(Player.ServerPosition) <= 100)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast(spell.Start));
                    }
                }
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (config.Item("EC.Fiora.Combo.E").GetValue<bool>() && E.IsReady())
                    {
                        E.Cast();
                    }
                }
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                if ((myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && config.Item("EC.Fiora.Combo.Items").GetValue<bool>()) && Orbwalking.InAutoAttackRange(target))
                {
                    myItemManager.UseItems(2, null);
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Fiora.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
        }
    }
}
