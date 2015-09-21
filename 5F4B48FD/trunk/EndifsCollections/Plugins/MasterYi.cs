using System;
using System.Linq;
using EndifsCollections.Controller;
using EndifsCollections.SummonerSpells;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
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
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("TurretDive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("UseItemCombo", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclear = new Menu("Farm", "Farm");
            {
                laneclear.AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(false));
                laneclear.AddItem(new MenuItem("QFarmType", "Q").SetValue(new StringList(new[] { "Any", "Furthest" })));
                laneclear.AddItem(new MenuItem("FarmMana", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclear);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true)); 
                config.AddSubMenu(junglemenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(Q.Range * 1.5f, TargetSelector.DamageType.Physical);
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseR = config.Item("UseRCombo").GetValue<bool>();
            var CastItems = config.Item("UseItemCombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToPhysical(target)) return;                
                if (CastItems) { myUtility.UseItems(0, target); }
                if (mySmiter.CanSmiteChampions(target)) mySmiter.Smites(target);
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        if (target.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                        if (Q.IsKillable(target)) Q.Cast(target);
                        else if (!Orbwalking.InAutoAttackRange(target))
                        {
                            var dist = Vector3.Distance(Player.ServerPosition, target.ServerPosition);
                            var msDif = Player.MoveSpeed - target.MoveSpeed;
                            var reachIn = dist / msDif;
                            if (msDif < 0)
                            {
                                Q.Cast(target);
                            }
                            else if (reachIn > 1)
                            {
                                Q.Cast(target);
                            }
                        }
                        else if (target.CountEnemiesInRange(Q.Range) >= 1)
                        {
                            Q.Cast(target);
                        }                       
                    }
                    if (UseR && R.IsReady() && R.IsInRange(target) && !Player.IsWindingUp)
                    {
                        var dist = Vector3.Distance(Player.ServerPosition, target.ServerPosition);
                        var msDif = Player.MoveSpeed - target.MoveSpeed;
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
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= 450f)
                        {
                            myUtility.UseItems(1, target);
                        }
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < 500f)
                        {
                            myUtility.UseItems(3, null);
                        }
                    }
                }
                catch { }
            }
            
        }
        private void Harass()
        {
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
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
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            var minions = MinionManager.GetMinions(Player.ServerPosition, Player.AttackRange * 2, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None);
            if (minions.Count >= 3 && !myOrbwalker.IsWaiting() && !Player.IsWindingUp)
            {
                myUtility.UseItems(2, null);
            }
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (allMinionsQ == null) return;
                var siegeQ = myUtility.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                if (siegeQ != null && siegeQ.IsValidTarget())
                {
                    Q.CastOnUnit(siegeQ);
                }
                else
                {
                    switch (config.Item("QFarmType").GetValue<StringList>().SelectedIndex)
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
            var largemobs = myUtility.GetLargeMonsters(Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob == null) return;
            if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady())
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
                    config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() &&
                    !args.Target.Name.Contains("Mini") &&
                    !Player.IsWindingUp &&
                    Orbwalking.InAutoAttackRange(args.Target))
                {
                    myUtility.UseItems(2, null);
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
                        myUtility.UseItems(2, null);
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("DrawQ").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.White);
            }
        }
    }
}
