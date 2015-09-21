using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCreations.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
{
    class Ashe : PluginData
    {
        public Ashe()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1200);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            W.SetSkillshot(0.5f, 100, 902, true, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.5f, 100, 1600, false, SkillshotType.SkillshotLine);               
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Ashe.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Ashe.Combo.W", "Use W").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {                
                drawmenu.AddItem(new MenuItem("EC.Ashe.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Ashe.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            
            var UseW = config.Item("EC.Ashe.Combo.W").GetValue<bool>();            
            if (Target.IsValidTarget())
            {
                try
                {
                    if (UseW && W.IsReady())
                    {
                        mySpellcast.Linear(Target, W, HitChance.Medium);
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
                    if (config.Item("EC.Ashe.Combo.Q").GetValue<bool>() && Q.IsReady() && (Player.GetBuffCount("AsheQ") >= 5 || Player.HasBuff("asheqcastready")))
                    {
                        Q.Cast();
                    }
                }
            }
            if (args.Target is Obj_AI_Minion && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear)
                {               
                    if (Player.GetBuffCount("AsheQ") >= 5 || Player.HasBuff("asheqcastready"))
                    {
                        Q.Cast();
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Ashe.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Ashe.Draw.R").GetValue<bool>() && R.Level > 0 && R.IsReady())
            {
                var box = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(Game.CursorPos, R.Range), R.Width);
                var insidebox = HeroManager.Enemies.Where(x => box.IsInside(x) && x.IsValidTarget()).ToList();
                if (insidebox.Any())
                {
                    if (insidebox.Count() >= 4)
                    {
                        Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.Red, "Hits: " + insidebox.Count());
                    }
                    else
                    {
                        Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.Yellow, "Hits: " + insidebox.Count());
                    }
                }
                box.Draw(Color.Red);
            }
        }
    }
}
