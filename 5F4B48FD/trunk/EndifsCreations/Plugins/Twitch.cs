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
    class Twitch : PluginData
    {
        public Twitch()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 1200);
            R = new Spell(SpellSlot.R, 850);
            
            W.SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Twitch.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Twitch.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Twitch.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Twitch.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Twitch.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Twitch.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Twitch.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Twitch.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            var UseW = Root.Item("EC.Twitch.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Twitch.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Twitch.Combo.R").GetValue<bool>();
            if (UseE && E.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && x.HasBuff("twitchdeadlyvenom"));                
                foreach (var x in EnemyList.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= E.Range))
                {
                    E.Cast();
                }
            }            
            if (Target.IsValidTarget())
            {
                try
                {
                    if (UseW && W.IsReady())
                    {
                        mySpellcast.CircularAoe(Target,W,HitChance.High, W.Range, 120);
                    }
                    if (UseR && R.IsReady())
                    {
                        Geometry.Polygon box = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(Target.ServerPosition, R.Range), R.Width);
                        var EnemyList = HeroManager.Enemies.Where(x => x != Target && x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToDeath(x));
                        if (EnemyList.Count(x => box.IsInside(x)) > 0)
                        {
                            R.Cast();
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
                    if (Root.Item("EC.Twitch.Combo.Q").GetValue<bool>() && Q.IsReady())
                    {
                        Q.Cast();
                    }
                }
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
            {
                if (args.Slot == SpellSlot.R && Root.Item("EC.Twitch.Combo.Items").GetValue<bool>())
                {
                    myItemManager.UseGhostblade();
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
   
            if (Root.Item("EC.Twitch.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Twitch.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Twitch.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
