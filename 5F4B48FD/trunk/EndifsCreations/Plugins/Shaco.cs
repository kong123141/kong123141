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
    class Shaco : PluginData
    {
        public Shaco()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 400);
            W = new Spell(SpellSlot.W, 425); 
            E = new Spell(SpellSlot.E, 625);
            R = new Spell(SpellSlot.R, 250);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Shaco.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Shaco.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Shaco.Combo.E", "Use E").SetValue(true));
                //combomenu.AddItem(new MenuItem("EC.Shaco.Combo.R", "Use R").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Shaco.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Shaco.Draw.E", "E").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            var UseW = Root.Item("EC.Shaco.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Shaco.Combo.E").GetValue<bool>();

            if (RPet)
            {
                if (Target.IsValidTarget() && Orbwalking.InAutoAttackRange(Target))
                {
                    PetWalker(Target);
                }
                else
                {
                    var pettarget = HeroManager.Enemies.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= 1500).OrderByDescending(i => myRePriority.ResortDB(i.ChampionName)).ThenBy(u => u.Health).FirstOrDefault();
                    if (pettarget != null)
                    {
                        PetWalker(pettarget);
                    }
                }
            }

            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {
                    if (myUtility.ImmuneToMagic(Target)) return;
                    if (UseW && W.IsReady())
                    {
                        if (myUtility.MovementDisabled(Target))
                        {
                            Vector3 pos = myUtility.RandomPos(0, 25, 20, Player.ServerPosition.Extend(Target.ServerPosition, Vector3.Distance(Player.ServerPosition, Target.ServerPosition)));
                            W.Cast(pos);
                        }
                    }
                    if (UseE && E.IsReady())
                    {
                        if (E.IsKillable(Target))
                        {
                            E.CastOnUnit(Target);
                        }
                        else if (myUtility.MovementDisabled(Target))
                        {
                            E.CastOnUnit(Target);
                        }
                        else if (!myUtility.IsFacing(Target, Player.ServerPosition, 180))
                        {
                            E.CastOnUnit(Target);
                        }
                        E.CastOnUnit(Target);
                    }                   
                }
                catch { }
            }
        }

        private static bool RPet
        {
            get { return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name == "hallucinateguide"; }
        }

        private Obj_AI_Minion GetPet()
        {
            Obj_AI_Minion Pet = null;
            foreach (var unit in ObjectManager.Get<Obj_AI_Minion>().Where(x => !x.IsMe && x.Name == Player.Name))
            {
                Pet = unit;
            }
            return Pet;
        }
        private int PetLastOrder;
        private void PetWalker(Obj_AI_Base target)
        {
            if (RPet)
            {
                Obj_AI_Base Pet = GetPet();
                if (myUtility.TickCount > PetLastOrder + 250)
                {
                    if (target != null && !Pet.IsWindingUp)
                    {
                        R.Cast(target);
                    }
                    else
                    {
                        R.Cast(Game.CursorPos);
                    }
                    PetLastOrder = myUtility.TickCount;
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
            if (unit is Obj_AI_Hero && unit.IsEnemy && !spell.SData.IsAutoAttack() && Q.IsReady())
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Shaco.Combo.Q").GetValue<bool>())
                {
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        var box = new Geometry.Polygon.Rectangle(spell.Start, spell.End, Player.BoundingRadius);
                        if (box.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 100))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => Q.Cast(Player.ServerPosition.Extend(spell.End,Q.Range)));
                        }
                    }
                    else if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Unit) || spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit)) && spell.Target != null && spell.Target.IsMe)
                    {
                        Q.Cast(Player.ServerPosition.Extend(spell.End, Q.Range));
                    }
                    else if (spell.End.Distance(Player.ServerPosition) <= 100)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => Q.Cast(Player.ServerPosition.Extend(spell.End, Q.Range)));
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Shaco.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Shaco.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
        }
       
    }
}
