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
    class Chogath : PluginData
    {
        public Chogath()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 900); //175 rad
            W = new Spell(SpellSlot.W, 700); //60deg
            E = new Spell(SpellSlot.E, 500);
            R = new Spell(SpellSlot.R, 175);

            Q.SetSkillshot(0.75f, 200f, 450f, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f, 60 * (float)Math.PI / 90, 2000f, false, SkillshotType.SkillshotCone);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Feast", "Custom");
            {
                custommenu.AddItem(new MenuItem("EC.Chogath.UseAutoR", "Auto").SetValue(true));
                Root.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Chogath.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Chogath.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Chogath.Combo.E", "Use E").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Chogath.Misc.Q", "Q Gapclosers").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Chogath.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Chogath.Draw.W", "W").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        private void Custom()
        {
            var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToMagic(x) && R.IsKillable(x));
            foreach (var x in EnemyList)
            {
                R.CastOnUnit(x);
            }
        }
        private void Combo()
        {   
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = Root.Item("EC.Chogath.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Chogath.Combo.W").GetValue<bool>();            
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {
                    if (myUtility.ImmuneToMagic(Target)) return;
                    if (UseQ && Q.IsReady())
                    {             
                        mySpellcast.CircularAoe(Target, Q, HitChance.High, Q.Range, 200);
                    }
                    if (UseW && W.IsReady())
                    {
                        W.CastIfHitchanceEquals(Target, HitChance.High);
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
            if (Root.Item("EC.Chogath.UseAutoR").GetValue<bool>())
            {
                Custom();
            }
            switch (myOrbwalker.ActiveMode)
            {
                case myOrbwalker.OrbwalkingMode.None:
                    myUtility.Reset();
                    break;
                case myOrbwalker.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case myOrbwalker.OrbwalkingMode.LaneClear:
                    {
                        if (R.IsReady() && Player.GetBuffCount("Feast") < 6)
                        {
                            var minionR = MinionManager.GetMinions(Player.ServerPosition, R.Range);
                            if (minionR == null) return;
                            var siegeR = myFarmManager.GetLargeMinions(R.Range).FirstOrDefault(x => R.IsKillable(x));
                            if (siegeR != null && siegeR.IsValidTarget())
                            {
                                R.CastOnUnit(siegeR);
                            }
                            else
                            {
                                foreach (var AnyR in minionR.Where(x => R.IsKillable(x)))
                                {
                                     R.CastOnUnit(AnyR);
                                }                     
                            }
                        }
                    }
                    break;
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Chogath.Misc.Q").GetValue<bool>() && Q.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= Q.Range + (Q.Width/2))
                {
                    if (myUtility.ImmuneToMagic(gapcloser.Sender) || myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Vector3 pos = myUtility.RandomPos(1, 25, 25, gapcloser.End);
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => Q.Cast(pos));                   
                }
            }
        }        
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Chogath.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Chogath.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
        }
    }
}
