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
    class DrMundo : PluginData
    {
        public DrMundo()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {

            Q = new Spell(SpellSlot.Q, 1000); //1050
            W = new Spell(SpellSlot.W, 325);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.25f, 60, 2000, true, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.DrMundo.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.DrMundo.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.DrMundo.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.DrMundo.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.DrMundo.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.DrMundo.Draw.Q", "Q").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            var UseQ = config.Item("EC.DrMundo.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.DrMundo.Combo.W").GetValue<bool>();
            var UseR = config.Item("EC.DrMundo.Combo.R").GetValue<bool>();
            var CastItems = config.Item("EC.DrMundo.Combo.Items").GetValue<bool>();
            if (UseW && W.IsReady())
            {             
                mySpellcast.Toggle(null, W, SpellSlot.W, 0, 400);
            }
            if (UseR && R.IsReady())
            {
                if (Player.Spellbook.IsAutoAttacking && Player.CountEnemiesInRange(400) > 0 && myUtility.PlayerHealthPercentage <= 50)
                {
                    R.Cast();
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
                        if (myUtility.ImmuneToMagic(Target)) return;
                        mySpellcast.Linear(Target, Q, HitChance.High, true);
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

        private static bool WActive
        {
            get { return ObjectManager.Player.HasBuff("BurningAgony"); }
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
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (config.Item("EC.DrMundo.Combo.E").GetValue<bool>() && E.IsReady() && Orbwalking.InAutoAttackRange(target))
                    {
                        E.Cast();
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.DrMundo.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
        }
    }
}
