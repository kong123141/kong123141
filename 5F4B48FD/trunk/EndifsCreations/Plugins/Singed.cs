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
    class Singed : PluginData
    {
        public Singed()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 125);
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
                combomenu.AddItem(new MenuItem("EC.Singed.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Singed.Combo.E", "Use E").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Singed.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Singed.Draw.E", "E").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(W.Range, TargetSelector.DamageType.Magical);

            var UseW = Root.Item("EC.Singed.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Singed.Combo.E").GetValue<bool>();

            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {     
                    if (UseW && W.IsReady())
                    {
                        mySpellcast.CircularAoe(Target, W, HitChance.High, W.Range, W.Instance.SData.CastRadius);
                    }
                    if (UseE && E.IsReady())
                    {
                        E.CastOnUnit(Target);
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
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;

            if (Root.Item("EC.Singed.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
        }
    }
}