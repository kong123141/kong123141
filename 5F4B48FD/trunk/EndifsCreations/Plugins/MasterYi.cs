using System;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.SummonerSpells;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
{
    class MasterYi : PluginData
    {
        public MasterYi()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);
           
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.MasterYi.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.MasterYi.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.MasterYi.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.MasterYi.Combo.R", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.MasterYi.Combo.Dive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.MasterYi.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.MasterYi.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.MasterYi.Harass.E", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.MasterYi.Farm.Q", "Use Q").SetValue(false));
                laneclearmenu.AddItem(new MenuItem("EC.MasterYi.QFarmType", "Q").SetValue(new StringList(new[] { "Any", "Furthest" })));
                laneclearmenu.AddItem(new MenuItem("EC.MasterYi.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.MasterYi.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.MasterYi.Jungle.E", "Use E").SetValue(true)); 
                config.AddSubMenu(junglemenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.MasterYi.Draw.Q", "Q").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range * 2, TargetSelector.DamageType.Physical);

            var UseQ = config.Item("EC.MasterYi.Combo.Q").GetValue<bool>();
            var UseR = config.Item("EC.MasterYi.Combo.R").GetValue<bool>();
            var CastItems = config.Item("EC.MasterYi.Combo.Items").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;                                
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        if (Target.UnderTurret(true) && !config.Item("EC.MasterYi.Combo.Dive").GetValue<bool>()) return;
                        if (Q.IsKillable(Target)) mySpellcast.Unit(Target, Q);
                        else if (!Orbwalking.InAutoAttackRange(Target))
                        {
                            var dist = Vector3.Distance(Player.ServerPosition, Target.ServerPosition);
                            var msDif = Player.MoveSpeed - Target.MoveSpeed;
                            var reachIn = dist / msDif;
                            if (msDif < 0)
                            {
                                mySpellcast.Unit(Target, Q);
                            }
                            else if (reachIn > 1)
                            {
                                mySpellcast.Unit(Target, Q);
                            }
                        }
                        else if (Target.CountEnemiesInRange(Q.Range) >= 1)
                        {
                            mySpellcast.Unit(Target, Q);
                        }                       
                    }
                    if (UseR && R.IsReady() && R.IsInRange(Target))
                    {
                        var dist = Vector3.Distance(Player.ServerPosition, Target.ServerPosition);
                        var msDif = Player.MoveSpeed - Target.MoveSpeed;
                        var reachIn = dist / msDif;
                        if (msDif < 0 && reachIn > 2)
                        {
                            R.Cast();
                        }
                        else if (msDif > 0)
                        {
                            R.Cast();
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
        private void Harass()
        {
            var UseQ = config.Item("EC.MasterYi.Harass.Q").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget() )
            {
                if (UseQ && Q.IsReady() && Q.IsInRange(target) && !target.UnderTurret(true) && !Player.IsWindingUp)
                {
                    Q.CastOnUnit(target);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("EC.MasterYi.Farm.ManaPercent").GetValue<Slider>().Value) return;
            var minions = MinionManager.GetMinions(Player.ServerPosition, Player.AttackRange * 2, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None);
            if (minions.Count >= 3 && !myOrbwalker.IsWaiting() && !Player.IsWindingUp)
            {
                myItemManager.UseItems(2, null);
            }
            if (config.Item("EC.MasterYi.Farm.Q").GetValue<bool>() && Q.IsReady())
            {
                var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (allMinionsQ == null) return;
                var siegeQ = myFarmManager.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                if (siegeQ != null && siegeQ.IsValidTarget())
                {
                    Q.CastOnUnit(siegeQ);
                }
                else
                {
                    switch (config.Item("EC.MasterYi.QFarmType").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            var AnyQ = allMinionsQ.OrderByDescending(i => i.Distance(Player)).FirstOrDefault(x => !x.UnderTurret(true));
                            if (AnyQ != null && AnyQ.IsValidTarget())
                            {
                                Q.CastOnUnit(AnyQ);
                            }
                            break;
                        case 1:
                            var FurthestQ = allMinionsQ.OrderByDescending(i => i.Distance(Player)).Where(x => !x.UnderTurret(true)).ToList();
                            foreach (var x in FurthestQ)
                            {
                                if (Q.IsInRange(x) && MinionManager.GetMinions(x.ServerPosition, 300).Count() > 2)
                                {
                                    if (!Orbwalking.InAutoAttackRange(x)) Q.CastOnUnit(x);
                                }
                            }
                            break;
                    }
                }
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob == null) return;
            if (config.Item("EC.MasterYi.Jungle.Q").GetValue<bool>() && Q.IsReady())
            {
                if (largemobs != null)
                {
                    Q.CastOnUnit(largemobs);
                }
                else
                {
                    Q.CastOnUnit(mob);
                }
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
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Minion && args.Target.Team == GameObjectTeam.Neutral)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear &&
                    config.Item("EC.MasterYi.Jungle.E").GetValue<bool>() && E.IsReady() &&
                    !args.Target.Name.Contains("Mini") &&
                    !Player.IsWindingUp &&
                    Orbwalking.InAutoAttackRange(args.Target))
                {
                    myItemManager.UseItems(2, null);
                }
            }
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (config.Item("EC.MasterYi.Combo.E").GetValue<bool>() && E.IsReady())
                    {
                        E.Cast();
                    }
                }
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear)
                {
                    if (target is Obj_AI_Minion && target.Team == GameObjectTeam.Neutral && !target.Name.Contains("Mini") &&
                        !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                    {
                        myItemManager.UseItems(2, null);
                    }
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (config.Item("EC.MasterYi.Combo.Items").GetValue<bool>() && Orbwalking.InAutoAttackRange(target))
                    {
                        myItemManager.UseItems(2, null);
                    }
                }
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R && config.Item("EC.MasterYi.Combo.Items").GetValue<bool>())
            {
                Utility.DelayAction.Add(myHumazier.ReactionDelay, () => myItemManager.UseGhostblade());
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.MasterYi.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
        }
    }
}
