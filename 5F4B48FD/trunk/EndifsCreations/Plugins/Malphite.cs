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
    class Malphite : PluginData
    {
        public Malphite()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {

            Q = new Spell(SpellSlot.Q, 625);
            W = new Spell(SpellSlot.W, 125);
            E = new Spell(SpellSlot.E, 375);
            R = new Spell(SpellSlot.R, 1000);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Malphite.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Malphite.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Malphite.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Malphite.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Malphite.Misc.Q", "Q Gapclosers").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Malphite.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Malphite.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Malphite.Combo.Q").GetValue<bool>();
            var UseE = config.Item("EC.Malphite.Combo.E").GetValue<bool>();
            var CastItems = config.Item("EC.Malphite.Combo.Items").GetValue<bool>();
            if (UseE && E.IsReady())
            {
                if (Player.CountEnemiesInRange(300) > 0)
                {
                    E.Cast();
                }
            }
            if (Target.IsValidTarget())
            {
                try
                {
                    if (myUtility.ImmuneToDeath(Target)) return;
                    if (UseQ && Q.IsReady())
                    {
                        mySpellcast.Unit(Target, Q);
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
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (config.Item("EC.Malphite.Combo.W").GetValue<bool>() && W.IsReady() && Orbwalking.InAutoAttackRange(target))
                    {
                        W.Cast();
                    }
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("EC.Malphite.Misc.Q").GetValue<bool>() && Q.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= Q.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;                    
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Unit(gapcloser.Sender, Q));
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Malphite.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Malphite.Draw.R").GetValue<bool>() && R.Level > 0 && R.IsReady())
            {
                var tomouse = Player.ServerPosition.Extend(Game.CursorPos, Vector3.Distance(Player.ServerPosition, Game.CursorPos));
                var tomax = Player.ServerPosition.Extend(Game.CursorPos, R.Range);
                var newvec = Vector3.Distance(Player.ServerPosition, tomouse) >= Vector3.Distance(Player.ServerPosition, tomax) ? tomax : tomouse;
                var wts = Drawing.WorldToScreen(newvec);
                var wtf = Drawing.WorldToScreen(Player.ServerPosition);
                Drawing.DrawLine(wtf, wts, 2, Color.GhostWhite);
                Render.Circle.DrawCircle(newvec, 300, Color.GhostWhite, 2);
                Drawing.DrawText(wts.X - 20, wts.Y - 50, Color.Yellow, "Hits: " + newvec.CountEnemiesInRange(300));

            }
        }
    }
}
