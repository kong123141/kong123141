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
    class Kayle : PluginData
    {
        public Kayle()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {

            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 525); //150 splash
            R = new Spell(SpellSlot.R, 900);

            W.SetTargetted(float.MaxValue, float.MaxValue);
            R.SetTargetted(float.MaxValue, float.MaxValue);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Kayle.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Kayle.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Kayle.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Kayle.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Kayle.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Kayle.Misc.Q", "Q Gapclosers").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Kayle.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Kayle.Draw.E", "E").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = Root.Item("EC.Kayle.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Kayle.Combo.W").GetValue<bool>();      
            var UseE = Root.Item("EC.Kayle.Combo.E").GetValue<bool>();
            
            var CastItems = Root.Item("EC.Kayle.Combo.Items").GetValue<bool>();
            if (UseW && W.IsReady())
            {
                if (myUtility.PlayerHealthPercentage < 70)
                {
                    mySpellcast.Unit(null, W);
                }
                else
                {
                    if (myUtility.PlayerManaPercentage < 50) return;
                    var Allies = HeroManager.Allies.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= W.Range).OrderBy(i => i.Health);
                    foreach (var heal in Allies.Where(x => x.Health < x.MaxHealth))
                    {
                        mySpellcast.Unit(heal, W);
                    }
                }
            }
            if (UseE && E.IsReady())
            {
                if (Player.CountEnemiesInRange(E.Range) > 0)
                {
                    E.Cast();
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {
                    if (myUtility.ImmuneToMagic(Target)) return;
                    if (CastItems) { myItemManager.UseItems(0, Target); }
                    if (UseQ && Q.IsReady())
                    {
                        if (!Orbwalking.InAutoAttackRange(Target))
                        {
                            Q.Cast(Target);
                        }
                        else if (Q.IsKillable(Target))
                        {
                            Q.Cast(Target);
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
        protected override void ProcessDamageBuffer(Obj_AI_Base sender, Obj_AI_Hero target, SpellData spell, float damage, myDamageBuffer.DamageTriggers type)
        {
            if (sender != null && target.IsMe)
            {
                switch (type)
                {
                    case myDamageBuffer.DamageTriggers.TonsOfDamage:
                        if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Kayle.Combo.W").GetValue<bool>() && W.IsReady())
                        {
                            mySpellcast.Unit(null, W);
                        }
                        break;
                    case myDamageBuffer.DamageTriggers.Killable:
                        if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Kayle.Combo.R").GetValue<bool>() && R.IsReady())
                        {
                            mySpellcast.Unit(null, R);
                        }
                        break;
                }
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit is Obj_AI_Hero && unit.IsEnemy && !spell.SData.IsAutoAttack() && W.IsReady() && myUtility.PlayerHealthPercentage < 75)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Kayle.Combo.W").GetValue<bool>())
                {
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        var box = new Geometry.Polygon.Rectangle(spell.Start, spell.End, Player.BoundingRadius);
                        if (box.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 100))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast(Player));
                        }
                    }
                    else if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Unit) || spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit)) && spell.Target != null && spell.Target.IsMe)
                    {
                        W.Cast(Player);
                    }
                    else if (spell.End.Distance(Player.ServerPosition) <= 100)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast(Player));
                    }
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Kayle.Misc.Q").GetValue<bool>() && Q.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= Q.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => Q.Cast(gapcloser.Sender));
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Kayle.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Kayle.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
        }
    }
}
