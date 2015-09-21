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
    class XinZhao : PluginData
    {
        public XinZhao()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 480);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.XinZhao.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.XinZhao.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.XinZhao.Combo.E", "Use E").SetValue(true));
                //combomenu.AddItem(new MenuItem("EC.XinZhao.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.XinZhao.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.XinZhao.Draw.E", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Physical, true);

            var UseE = config.Item("EC.XinZhao.Combo.E").GetValue<bool>();
            var CastItems = config.Item("EC.XinZhao.Combo.Items").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {
                    if (myUtility.ImmuneToDeath(Target)) return;
                    if (CastItems) { myItemManager.UseItems(0, Target); }
                    if (UseE && E.IsReady())
                    {
                        E.CastOnUnit(Target);
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
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (config.Item("EC.XinZhao.Combo.W").GetValue<bool>() && W.IsReady())
                    {
                        W.Cast();
                    }
                    if (config.Item("EC.XinZhao.Combo.Q").GetValue<bool>() && Q.IsReady())
                    {
                        Q.Cast();
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
                    if (spell.SData.Name.ToLower() == "xinzhaosweep")
                    {
                        LastE = myUtility.TickCount;
                    }
                }
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
            {
                if (args.Slot == SpellSlot.E)
                {
                    if (myUtility.TickCount - LastE > 2000 + myHumazier.SpellDelay)
                    {
                        LastTarget = null;
                    }
                    if (LastTarget == null || !LastTarget.IsValidTarget())
                    {
                        LastTarget = (Obj_AI_Hero)args.Target;
                        args.Process = true;
                    }
                    if (LastTarget != (Obj_AI_Hero)args.Target)
                    {
                        if (myUtility.TickCount - LastE < 2000 + myHumazier.SpellDelay)
                        {
                            args.Process = false;
                        }
                        LastTarget = (Obj_AI_Hero)args.Target;
                        args.Process = true;
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.XinZhao.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
        }
    }
}
