using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Fiora : PluginData
    {
        public Fiora()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 400);
           
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
                combomenu.AddItem(new MenuItem("UseRComboType", "R").SetValue(new StringList(new[] { "Killable", "Multi", "Always" })));
                combomenu.AddItem(new MenuItem("UseRComboValue", "(Multi) R targets >").SetValue(new Slider(2, 2, 5)));
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
                laneclear.AddItem(new MenuItem("UseEFarm", "Use E").SetValue(false));
                laneclear.AddItem(new MenuItem("FarmMana", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclear);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true)); 
                config.AddSubMenu(junglemenu);
            }  
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("UseWMisc", "W Attack Block").SetValue(false));                
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawR", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }

        }

        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseR = config.Item("UseRCombo").GetValue<bool>();
            var CastItems = config.Item("UseItemCombo").GetValue<bool>(); 
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToPhysical(target)) return;                
                if (CastItems) { myUtility.UseItems(0, target); }
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        if (target.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                        if (!QBuff() && !EBuff() && !Player.IsWindingUp)
                        {
                            Q.Cast(target);
                        }
                        else if (QBuff() && !Player.IsWindingUp)
                        {
                            Q.Cast(target);                            
                        }
                        if (Orbwalking.InAutoAttackRange(target) && !Player.IsWindingUp) Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                    }
                    if (UseR && R.IsReady() && R.IsInRange(target) && !QBuff() && !Player.IsWindingUp)
                    {
                        switch (config.Item("UseRComboType").GetValue<StringList>().SelectedIndex)
                        {
                            case 0:
                                if (R.IsKillable(target) && !E.IsReady()) R.Cast(target);
                                break;
                            case 1:
                                if (target.CountEnemiesInRange(R.Range) > config.Item("UseRComboValue").GetValue<Slider>().Value)
                                    R.Cast(target);
                                break;
                            case 2:
                                R.Cast(target);
                                break;
                        }
                    }
                }
                catch { }
            }
            
        }
        private void Harass()
        {
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var target = LockedTargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget() && UseQ && Q.IsReady() && Q.IsInRange(target))
            {
                if (!target.UnderTurret(true))
                {
                    if (!QBuff() && !Player.IsWindingUp)
                    {
                        Q.Cast(target);
                        return;
                    }
                    if (QBuff() && myUtility.TickCount - LastCast > 500 && !Player.IsWindingUp)
                    {
                        Q.Cast(target);
                    }
                }
                else
                {
                    var SelectQ = MinionManager.GetMinions(target.ServerPosition, Q.Range).OrderByDescending(i => i.Distance(target)).FirstOrDefault(x => !x.UnderTurret(true));
                    if (!QBuff() && SelectQ != null && SelectQ.IsValidTarget())
                    {
                        Q.Cast(target);
                    }
                    if (QBuff() && SelectQ != null && SelectQ.IsValidTarget())
                    {
                        Q.Cast(SelectQ);
                    }
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
                    Q.Cast(siegeQ);
                }
                else
                {
                    foreach (var minionq in allMinionsQ)
                    {
                        if (minionq.Health < Player.GetSpellDamage(minionq, SpellSlot.Q) && !Player.IsWindingUp && !Orbwalking.InAutoAttackRange(minionq) && !minionq.UnderTurret(true))
                        {
                            if (!QBuff())
                            {
                                Q.Cast(minionq);
                            }
                            if (QBuff() && myUtility.TickCount - LastCast > 1000)
                            {
                                Q.Cast(minionq);
                            }
                        }
                    }
                }               
            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady())
            {
                var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, Player.AttackRange * 2, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None).Count();
                if (allMinionsE >= 3 && !Player.IsWindingUp)
                {
                    E.Cast();
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
                    if (Q.IsKillable(largemobs))
                    {
                        Q.Cast(largemobs);
                    }
                    if (!QBuff())
                    {
                        Q.Cast(largemobs);
                    }
                    if (QBuff())
                    {
                        Q.Cast(largemobs);
                    }
                }
                else
                {
                    if (!QBuff())
                    {
                        Q.Cast(mob);
                    }
                    else if (QBuff() && myUtility.TickCount - LastCast > 1000)
                    {
                        Q.Cast(mob);
                    }
                }
            }
            
        }

        private bool QBuff()
        {
            return Player.HasBuff("FioraQCD");
        }
        private bool EBuff()
        {
            return Player.HasBuff("FioraFlurry");
        }
        private static int LastCast = myUtility.TickCount;

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
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit != null && !unit.IsMe && spell.Target.IsMe && unit.IsChampion(unit.BaseSkinName))
            {
                if (myOrbwalker.ActiveMode != myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (spell.SData.Name.Contains("Attack") && config.Item("UseWMisc").GetValue<bool>())
                    {
                        if (Player.UnderTurret(true)) return;
                        Utility.DelayAction.Add(400, () => W.Cast());
                    }
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (spell.SData.Name.Contains("Attack") && !Player.IsWindingUp && config.Item("UseWCombo").GetValue<bool>())
                    {
                        W.Cast();
                    }
                }
            }
            if (unit != null && unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo || myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass)
                {
                    if (spell.SData.Name.ToLower() == "fioraq" && !Player.IsWindingUp)
                    {
                        Player.IssueOrder(GameObjectOrder.AttackUnit, spell.Target);
                    }
                    if (spell.SData.Name.ToLower() == "fioraflurry" && !Player.IsWindingUp)
                    {
                        Player.IssueOrder(GameObjectOrder.AttackUnit, spell.Target);
                    }
                }
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.Q)
            {
                LastCast = myUtility.TickCount;
            }
        }            
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Minion && args.Target.Team == GameObjectTeam.Neutral)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear &&
                    config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !EBuff() &&
                    !args.Target.Name.Contains("Mini") &&
                    !Player.IsWindingUp &&
                    Orbwalking.InAutoAttackRange(args.Target))
                {
                    myUtility.UseItems(2, null);
                    E.Cast();
                }
            }        
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (E.IsReady() && !EBuff() && !(Q.IsReady() && QBuff()) && !Player.IsWindingUp && config.Item("UseECombo").GetValue<bool>() && Orbwalking.InAutoAttackRange(target))
                    {
                        E.Cast();
                    }
                    if (!(Q.IsReady() && QBuff() && E.IsReady()) && !Player.IsWindingUp && config.Item("UseItemCombo").GetValue<bool>() && Orbwalking.InAutoAttackRange(target))
                    {
                        myUtility.UseItems(2, null);
                    }
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear)
                {
                    if (target is Obj_AI_Minion && target.Team == GameObjectTeam.Neutral && !target.Name.Contains("Mini") &&
                        config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !EBuff() && !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                    {
                        myUtility.UseItems(2, null);
                        E.Cast();
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
            if (config.Item("DrawR").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, Color.White);
            }
        }
    }
}
