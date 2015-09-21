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
    class Anivia : PluginData
    {
        public Anivia()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1075); //75
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 625);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Anivia.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Anivia.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Anivia.Combo.E", "Use E").SetValue(true));
                //combomenu.AddItem(new MenuItem("EC.Anivia.Combo.R", "Use R").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Anivia.Misc.W", "W Interrupts").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Anivia.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Anivia.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Anivia.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Anivia.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Anivia.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Anivia.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Anivia.Combo.E").GetValue<bool>();
            //var UseR = config.Item("EC.Anivia.Combo.R").GetValue<bool>();
            if (UseQ && Q.IsReady())
            {
                if (FlashFrost != null)
                {
                    if (FlashFrost.Position.CountEnemiesInRange(75) > 0)
                    {
                        Q.Cast();
                    }
                }
            }
            if (Target.IsValidTarget())
            {
                try
                {
                    if (myUtility.ImmuneToDeath(Target)) return;
                    if (UseQ && Q.IsReady())
                    {
                        if (FlashFrost == null)
                        {
                            mySpellcast.Linear(Target, Q, HitChance.High);
                        }
                    }
                    if (UseW && W.IsReady())
                    {
                        mySpellcast.Wall(Target, W, HitChance.High, myUtility.PlayerHealthPercentage > Target.HealthPercent);                                         
                    }
                    if (UseE && E.IsReady())
                    {
                        if (Target.HasBuff("chilled") || E.IsKillable(Target) || myUtility.MovementDisabled(Target))
                        {
                            mySpellcast.Unit(Target, E);
                        }
                    }
                }
                catch { }
            }
        }

        private GameObject FlashFrost;

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
                case myOrbwalker.OrbwalkingMode.Harass:
                    Target = myUtility.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                    if (Target.IsValidTarget())
                    {
                        mySpellcast.Wall(Target, W, HitChance.High);
                    }
                    break;
            }
        }
        protected override void OnCreate(GameObject sender, EventArgs args)
        {           
            if (sender.Name.Contains("_FlashFrost_Player"))
            {
                FlashFrost = sender;
            }
        }
        protected override void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("_FlashFrost_Player"))
            {
                FlashFrost = null;
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("EC.Anivia.Misc.W").GetValue<bool>() && W.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= W.Range)
                {
                    if (myUtility.ImmuneToCC(sender)) return;
                    mySpellcast.Wall(sender, W, HitChance.High, myUtility.PlayerHealthPercentage > sender.HealthPercent);
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Anivia.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Anivia.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Anivia.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (config.Item("EC.Anivia.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
