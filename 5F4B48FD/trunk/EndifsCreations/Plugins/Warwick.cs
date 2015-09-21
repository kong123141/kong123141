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
    class Warwick : PluginData
    {
        public Warwick()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 400);
            W = new Spell(SpellSlot.W, 1250);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 700);            


            Q.SetTargetted(0.5f, 1500f);
            E.SetSkillshot(0f, 50f, 1600f, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Warwick.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Warwick.UseQAuto", "Use Q (Auto)").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Warwick.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Warwick.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Warwick.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Warwick.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Warwick.UseQAuto", "Auto Q").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Warwick.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Warwick.Draw.E", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Warwick.Combo.Q").GetValue<bool>();
            var UseR = config.Item("EC.Warwick.Combo.R").GetValue<bool>();
            var CastItems = config.Item("EC.Warwick.Combo.Items").GetValue<bool>();           
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {
                    if (myUtility.ImmuneToDeath(Target)) return;
                    if (UseQ && Q.IsReady())
                    {
                        Q.CastOnUnit(Target);
                    }
                    if (UseR && R.IsReady() && myUtility.PlayerHealthPercentage > 70)
                    {
                        if (myUtility.ImmuneToMagic(Target) || myUtility.ImmuneToCC(Target)) return;
                        if (myRePriority.ResortDB(Target.ChampionName) >= 4)
                        {
                            R.Cast(Target);
                        }
                        else if (Target.HealthPercent < 33)
                        {
                            R.Cast(Target);
                        }
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
                    if (config.Item("EC.Warwick.UseQAuto").GetValue<bool>() && Q.IsReady())
                    {
                        var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.IsTargetable && !myUtility.ImmuneToMagic(x));
                        var qt = EnemyList.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= Q.Range).OrderBy(i => i.Health).FirstOrDefault();
                        if (qt != null)
                        {
                            Q.CastOnUnit(qt);
                        }
                    }
                    myUtility.Reset();
                    break;
                case myOrbwalker.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case myOrbwalker.OrbwalkingMode.LaneClear:
                    {
                        if (Q.IsReady())
                        {
                            var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                            if (minionQ == null) return;
                            var siegeQ = myFarmManager.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                            if (siegeQ != null && siegeQ.IsValidTarget())
                            {
                                Q.CastOnUnit(siegeQ);
                            }
                            else
                            {
                                var AnyQ = minionQ.FirstOrDefault(x => Q.IsKillable(x));
                                if (AnyQ != null && AnyQ.IsValidTarget())
                                {
                                    Q.CastOnUnit(AnyQ);
                                }
                            }
                        }
                    }
                    break;
            } 
        }
        protected override void OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (myOrbwalker.Active())
            {
                if (Player.IsChannelingImportantSpell() || Player.IsCastingInterruptableSpell()) args.Process = false;
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (config.Item("EC.Warwick.Combo.W").GetValue<bool>() && W.IsReady())
                    {
                        W.Cast();
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Warwick.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Warwick.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.White);
            }
        }
    }
}
