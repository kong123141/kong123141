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
    class Aatrox : PluginData
    {
        public Aatrox()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 650); //225 radius, 75 epicenter
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R, 550);

            Q.SetSkillshot(0, 250, 2000, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.235f, 40, 1250, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Aatrox.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Aatrox.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Aatrox.Combo.R", "Use R").SetValue(true));
                //combomenu.AddItem(new MenuItem("EC.Aatrox.Combo.Dive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Aatrox.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Aatrox.Harass.E", "Use E").SetValue(true));
                Root.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Aatrox.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Aatrox.Farm.E", "Use E").SetValue(true));
                Root.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Aatrox.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Aatrox.Jungle.E", "Use E").SetValue(true));
                Root.AddSubMenu(junglemenu);
            }  
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Aatrox.Misc.W", "Auto W").SetValue(true));
                miscmenu.AddItem(new MenuItem("EC.Aatrox.Misc.W.Value", "+/- 10 from").SetValue(new Slider(50)));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Aatrox.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Aatrox.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Aatrox.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target =  myUtility.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            
            var UseQ = Root.Item("EC.Aatrox.Combo.Q").GetValue<bool>();
            var UseE = Root.Item("EC.Aatrox.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Aatrox.Combo.R").GetValue<bool>();
            var CastItems = Root.Item("EC.Aatrox.Combo.Items").GetValue<bool>();
            if (UseR && R.IsReady())
            {
                mySpellcast.PointBlank(null, R, 400, 1);
            }
            if (Target.IsValidTarget())
            {                
                if (myUtility.ImmuneToDeath(Target)) return;                
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        mySpellcast.CircularPrecise(Target, Q, HitChance.High, Q.Range, 250);
                    }
                    if (UseE && E.IsReady())
                    {
                        mySpellcast.Linear(Target, E, HitChance.High);
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
        private void Harass()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target != null)
            {                
                if (Root.Item("EC.Aatrox.Harass.E").GetValue<bool>() && E.IsReady())
                {
                    mySpellcast.Linear(target, E, HitChance.High);
                }
            }
        }
        private void LaneClear()
        {
            if (Root.Item("EC.Aatrox.Farm.Q").GetValue<bool>() && Q.IsReady())
            {
                myFarmManager.LaneCircular(Q, Q.Range, 225);
            }
            if (Root.Item("EC.Aatrox.Farm.E").GetValue<bool>() && E.IsReady())
            {
                myFarmManager.LaneLinear(E, E.Range);
            }
        }
        private void JungleClear()
        {
            if (Root.Item("EC.Aatrox.Jungle.Q").GetValue<bool>() && Q.IsReady())
            {
                myFarmManager.JungleCircular(Q, Q.Range, 225);
            }
            if (Root.Item("EC.Aatrox.Jungle.E").GetValue<bool>() && E.IsReady())
            {
                myFarmManager.JungleLinear(E, E.Range);
            }
        }

        private void SmartW()
        {
            if (Player.InFountain() || Player.InShop() || Player.HasBuff("Recall")) return;
            if (Root.Item("EC.Aatrox.Misc.W").GetValue<bool>())
            {
                var hpp = myUtility.PlayerHealthPercentage;
                var t = Root.Item("EC.Aatrox.Misc.W.Value").GetValue<Slider>().Value;
                if (Player.HasBuff("aatroxwlife")) //lifesteal
                {
                    if (hpp > (Math.Max(10, t - 10)))
                    {
                        W.Cast();
                    }
                }
                else if (Player.HasBuff("aatroxwpower")) //empowered
                {
                    if (hpp < (Math.Min(100, t - 10)))
                    {
                        W.Cast();
                    }
                }
            }
        }

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsZombie)
            {
                myUtility.Reset();
                return;
            }
            SmartW();
            switch (myOrbwalker.ActiveMode)
            {
                case myOrbwalker.OrbwalkingMode.None:
                    myUtility.Reset();
                    break;
                case myOrbwalker.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case myOrbwalker.OrbwalkingMode.Harass:
                    Harass();
                    break;
                case myOrbwalker.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                case myOrbwalker.OrbwalkingMode.JungleClear:
                    JungleClear();
                    break;
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                if ((myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Aatrox.Combo.Items").GetValue<bool>()) && Orbwalking.InAutoAttackRange(target))
                {
                    myItemManager.UseItems(2, null);                    
                }
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R && Root.Item("EC.Aatrox.Combo.Items").GetValue<bool>())
            {
                Utility.DelayAction.Add(myHumazier.ReactionDelay, () => myItemManager.UseGhostblade());
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Aatrox.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Aatrox.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Aatrox.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
