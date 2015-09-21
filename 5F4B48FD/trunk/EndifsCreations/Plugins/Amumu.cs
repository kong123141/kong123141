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
    class Amumu  : PluginData
    {
        public Amumu()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {

            Q = new Spell(SpellSlot.Q, 1080);
            W = new Spell(SpellSlot.W, 300);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 550);

            Q.SetSkillshot(.25f, 90, 2000, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0f, W.Range, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(.5f, E.Range, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(.25f, R.Range, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Amumu.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Amumu.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Amumu.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Amumu.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Amumu.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Amumu.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Amumu.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Amumu.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Amumu.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Amumu.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Amumu.Combo.E").GetValue<bool>();
            var UseR = config.Item("EC.Amumu.Combo.R").GetValue<bool>();

            var CastItems = config.Item("EC.Amumu.Combo.Items").GetValue<bool>();

            if (UseW && W.IsReady())
            {
                mySpellcast.Toggle(null, W, SpellSlot.W, 0, 300);                
            }
            if (UseE && E.IsReady())
            {
                if (Player.CountEnemiesInRange(E.Range) > 0)
                {
                    E.Cast();
                }
            }
            if (UseR && R.IsReady())
            {
                if (Player.CountEnemiesInRange(R.Range) >= 4)
                {
                    R.Cast();
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {
                    if (myUtility.ImmuneToMagic(Target)) return;
                    if (UseQ && Q.IsReady() && !Orbwalking.InAutoAttackRange(Target))
                    {
                        if (myUtility.ImmuneToCC(Target)) return;
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
            if (config.Item("EC.Amumu.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Amumu.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (config.Item("EC.Amumu.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.Yellow, "Hits: " + Player.CountEnemiesInRange(R.Range));
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
