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
    class Jax : PluginData
    {
        public Jax()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {

            Q = new Spell(SpellSlot.Q, 700);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 190);
            R = new Spell(SpellSlot.R);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Jax.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Jax.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Jax.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Jax.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Jax.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Jax.Misc.E", "E Gapclosers").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Jax.Draw.Q", "Q").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            var UseQ = config.Item("EC.Jax.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Jax.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Jax.Combo.E").GetValue<bool>();
            var UseR = config.Item("EC.Jax.Combo.R").GetValue<bool>();
            var CastItems = config.Item("EC.Jax.Combo.Items").GetValue<bool>();
            if (UseE && E.IsReady() && !EActive)
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
                    if (myUtility.ImmuneToDeath(Target)) return;
                    if (CastItems) { myItemManager.UseItems(0, Target); }
                    if (UseQ && Q.IsReady() && !Orbwalking.InAutoAttackRange(Target))
                    {
                        if (UseE && E.IsReady() && !EActive && !myUtility.ImmuneToCC(Target))
                        {
                            E.Cast();
                        }
                        Q.CastOnUnit(Target);
                    }
                    if (UseW && W.IsReady())
                    {
                        if (Player.IsDashing())
                        {
                            W.Cast();
                        }
                    }
                    if (UseR && R.IsReady())
                    {
                        if (myUtility.PlayerHealthPercentage < 50 || Player.CountEnemiesInRange(200) > 1)
                        {
                            R.Cast();
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

        private static bool WActive
        {
            get { return ObjectManager.Player.HasBuff("JaxEmpowerTwo"); }
        }
        private static bool EActive
        {
            get { return ObjectManager.Player.HasBuff("JaxCounterStrike"); }
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
                    if (config.Item("EC.Jax.Combo.W").GetValue<bool>() && W.IsReady() && Orbwalking.InAutoAttackRange(target) && !WActive)
                    {
                        W.Cast();
                    }
                }
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (spell.SData.Name.ToLower() == "jaxleapstrike")
                    {
                        LastQ = myUtility.TickCount;
                    }
                }
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
            {
                if (args.Slot == SpellSlot.Q)
                {
                    if (LastTarget == null || !LastTarget.IsValidTarget())
                    {
                        LastTarget = (Obj_AI_Hero)args.Target;
                        args.Process = true;
                    }
                    if (LastTarget != (Obj_AI_Hero)args.Target)
                    {
                        if (myUtility.TickCount - LastQ < 2000 + myHumazier.SpellDelay)
                        {
                            args.Process = false;
                        }
                        LastTarget = (Obj_AI_Hero)args.Target;
                        args.Process = true;
                    }
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("EC.Jax.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.Cast());
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Jax.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
        }
    }
}
