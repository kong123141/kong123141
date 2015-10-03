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
    class Janna : PluginData
    {
        public Janna()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 550);

            Q.SetSkillshot(0.25f, 120f, 900f, false, SkillshotType.SkillshotLine);         
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                //combomenu.AddItem(new MenuItem("EC.Janna.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Janna.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Janna.Combo.E", "Use E").SetValue(true));
                Root.AddSubMenu(combomenu);
            }            
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Janna.Misc.W", "W Gapclosers").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Janna.Misc.E", "E Shields").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Janna.UseESupport", "E Supports").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Janna.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Janna.Draw.W", "W").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var UseW = Root.Item("EC.Janna.Combo.W").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                try
                {
                    if (UseW && W.IsReady() && !myUtility.ImmuneToCC(Target))
                    {
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
            if (unit.IsMe)
            {                
                if (spell.SData.Name.ToLower() == "howlinggale")
                {
                    LastQ = myUtility.TickCount;
                }
                if (spell.SData.Name == "reapthewhirlwind")
                {
                    LastR = myUtility.TickCount;
                }
            }
            if (unit is Obj_AI_Hero && unit.IsAlly && !unit.IsMe)
            {
                if (Root.Item("EC.Janna.UseESupport").GetValue<bool>())
                {                    
                    if (spell.Target is Obj_AI_Hero && spell.SData.IsAutoAttack() && spell.Target.IsEnemy)
                    {
                        mySpellcast.Unit((Obj_AI_Hero)unit, E);
                    }
                }
            }
            if (unit is Obj_AI_Hero && unit.IsEnemy)
            {
                if (Root.Item("EC.Janna.Misc.E").GetValue<bool>() || (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Janna.Combo.E").GetValue<bool>()) && E.IsReady())
                {
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        var box = new Geometry.Polygon.Rectangle(spell.Start, spell.End, Player.BoundingRadius);
                        if (box.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 100))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Unit(null, E));
                        }
                    }
                    else if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Unit) || spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit)) && spell.Target != null && spell.Target.IsMe)
                    {
                        mySpellcast.Unit(null, E);
                    }
                    else if (spell.End.Distance(Player.ServerPosition) <= 100)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () =>  mySpellcast.Unit(null, E));
                    }
                }
            }
        }

        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.R)
            {
                LastR = myUtility.TickCount;
                mySpellcast.Pause(3000 + Game.Ping);
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (Player.IsChannelingImportantSpell() || myUtility.TickCount - LastR <= 0.5f)
            {
                args.Process = false;
            }
        }
        protected override void OnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid<MissileClient>())
            {
                return;
            }
            if (Root.Item("EC.Janna.UseESupport").GetValue<bool>())
            {
                var missile = (MissileClient)sender;
                if (!missile.SpellCaster.IsValid<Obj_AI_Hero>() || !missile.SpellCaster.IsAlly || missile.SpellCaster.IsMe ||
                    missile.SpellCaster.IsMelee())
                {
                    return;
                }
                if (!missile.Target.IsValid<Obj_AI_Hero>() || !missile.Target.IsEnemy)
                {
                    return;
                }
                var caster = (Obj_AI_Hero)missile.SpellCaster;
                if (E.IsReady())
                {
                    mySpellcast.Unit(caster, E);
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Janna.Misc.W").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= W.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;                    
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Unit(gapcloser.Sender, W));
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Janna.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Janna.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
        }
    }
}
