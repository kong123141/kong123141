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
    class Maokai : PluginData
    {
        public Maokai()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 525);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 475);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Maokai.Combo.Q", "Use Q").SetValue(true));
                //combomenu.AddItem(new MenuItem("EC.Maokai.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Maokai.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Maokai.Combo.R", "Use R").SetValue(true));
                //combomenu.AddItem(new MenuItem("EC.Maokai.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Maokai.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Maokai.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Maokai.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Maokai.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            var UseQ = Root.Item("EC.Maokai.Combo.Q").GetValue<bool>();
            //var UseW = Root.Item("EC.Maokai.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Maokai.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Maokai.Combo.R").GetValue<bool>(); 

            if (UseR && R.IsReady())
            {
                //TODO test toggle
                if (Player.CountEnemiesInRange(R.Range) >= 4)
                {
                    R.Cast();
                }
            }
            if (Target.IsValidTarget())
            {        
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        mySpellcast.Linear(Target, Q, HitChance.High);
                    }
                    if (UseE && E.IsReady())
                    {
                        mySpellcast.CircularPrecise(Target, E, HitChance.Medium, E.Range);
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
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
            {
                if (args.Slot == SpellSlot.R)
                {
                    if (Player.HasBuff("MaokaiDrain3"))
                    {
                        args.Process = false;
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Maokai.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Maokai.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Maokai.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Maokai.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.Yellow, "Hits: " + Player.CountEnemiesInRange(R.Range));
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
