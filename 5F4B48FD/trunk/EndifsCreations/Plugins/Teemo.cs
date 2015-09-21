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
    class Teemo : PluginData
    {
        public Teemo()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 580);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 300);            

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Teemo.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Teemo.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Teemo.Combo.R", "Use R").SetValue(false));                
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {               
                harassmenu.AddItem(new MenuItem("EC.Teemo.Harass.Q", "Use Q").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Teemo.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Teemo.UseRFarm", "Use R").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Teemo.RFarmValue", "R More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Teemo.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Teemo.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Teemo.UseRJFarm", "Use R").SetValue(true));
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Teemo.UseRMisc", "R Gapcloser").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Teemo.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Teemo.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Teemo.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Teemo.Combo.W").GetValue<bool>();
            var UseR = config.Item("EC.Teemo.Combo.R").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        mySpellcast.Unit(Target, Q);
                    }
                    if (UseR && R.IsReady())
                    {
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= R.Range)
                        {
                            mySpellcast.LinearVector(Target.ServerPosition, R, Vector3.Distance(Player.ServerPosition, Target.ServerPosition));
                        }
                        else
                        {
                            var box = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(Target.ServerPosition, R.Range), 50);
                            if (AllNoxiousTraps.Any(x => box.IsInside(x)))
                            {
                                mySpellcast.LinearVector(AllNoxiousTraps.Where(x => box.IsInside(x)).Select(p => p.ServerPosition).LastOrDefault(), R);
                            }
                        }
                    }
                }
                catch { }
            }           
        }
        private void Harass()
        {
            var UseQ = config.Item("EC.Teemo.Harass.Q").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady())
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    Q.Cast(target);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("EC.Teemo.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (Player.UnderTurret(true)) return;
            if (config.Item("EC.Teemo.Farm.Q").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp && myOrbwalker.IsWaiting())
            {
                var siegeQ = myFarmManager.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                if (siegeQ != null && siegeQ.IsValidTarget())
                {
                    Q.Cast(siegeQ);
                }
                else
                {
                    var minionQ = MinionManager.GetMinions(Player.AttackRange).FirstOrDefault(x => Q.IsKillable(x));
                    if (minionQ != null && minionQ.IsValidTarget())
                    {
                        Q.Cast(minionQ);
                    }
                }
            }
            if (config.Item("EC.Teemo.UseRFarm").GetValue<bool>() && R.IsReady() && !Player.IsWindingUp)
            {
                var minionR = MinionManager.GetMinions(Player.ServerPosition, R.Range);
                if (minionR == null) return;
                var rpred = R.GetCircularFarmLocation(minionR);
                if (rpred.MinionsHit > config.Item("EC.Teemo.RFarmValue").GetValue<Slider>().Value)
                {
                    R.Cast(rpred.Position);
                }
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null && !Player.IsWindingUp)
            {
                if (config.Item("EC.Teemo.Jungle.Q").GetValue<bool>() && Q.IsReady() && Orbwalking.InAutoAttackRange(mob))
                {
                    if (largemobs != null)
                    {
                        Q.Cast(largemobs);
                    }
                    else 
                    {
                        Q.Cast(mob);
                    }
                }
                if (config.Item("EC.Teemo.UseRJFarm").GetValue<bool>() && R.IsReady())
                {
                    if (largemobs != null)
                    {
                        R.Cast(largemobs.ServerPosition);
                    }
                    var RCircular = R.GetCircularFarmLocation(mobs);
                    if (RCircular.MinionsHit > 0)
                    {
                        R.Cast(RCircular.Position);
                    }
                }
            }
        }
        private void UpdateR()
        {
            if (R.Level > 0)
            {
                R.Range = R.Level * 300;
            }
        }

        private static List<Obj_AI_Minion> NoxiousTrap = new List<Obj_AI_Minion>();
        private static List<Obj_AI_Minion> AllNoxiousTraps
        {
            get { return NoxiousTrap.Where(s => s.IsValid && !s.IsMoving).ToList(); }
        }

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                myUtility.Reset();
                return;
            }
            UpdateR();
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
                case myOrbwalker.OrbwalkingMode.Hybrid:
                    LaneClear();
                    Harass();
                    break;
                case myOrbwalker.OrbwalkingMode.JungleClear:
                    JungleClear();
                    break;
            }            
        }

        protected override void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender is Obj_AI_Minion && sender.Name == "Noxious Trap")
            {
                NoxiousTrap.Add((Obj_AI_Minion)sender);
            }
        }
        protected override void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender is Obj_AI_Minion && sender.Name == "Noxious Trap")
            {
                NoxiousTrap.RemoveAll(s => s.NetworkId == sender.NetworkId);
            }
        }

        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {                   
                    if (config.Item("EC.Teemo.Combo.Q").GetValue<bool>() && Q.IsReady())
                    {
                        Q.Cast(args.Target);
                    }
                    if (config.Item("EC.Teemo.Combo.W").GetValue<bool>() && W.IsReady())
                    {
                        W.Cast();
                    }
                }
            }
        }
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (config.Item("EC.Teemo.Farm.Q").GetValue<bool>() && Q.IsReady() && (myUtility.PlayerManaPercentage > config.Item("EC.Teemo.Farm.ManaPercent").GetValue<Slider>().Value))
            {
                var target = minion as Obj_AI_Base;
                if (target != null &&
                    Q.IsKillable(target) &&
                    Orbwalking.InAutoAttackRange(target))
                {
                    Q.Cast(target);
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("EC.Teemo.UseRMisc").GetValue<bool>() && R.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= R.Range)
                {
                    if (myUtility.ImmuneToMagic(gapcloser.Sender) || myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Vector3 pos = myUtility.RandomPos(1, 25, 25, gapcloser.End);
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => R.Cast(pos));
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Teemo.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Teemo.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.White);
            }
        }
    }
}
