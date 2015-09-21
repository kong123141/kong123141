using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace EndifsCollections.Plugins
{
    class Udyr : PluginData
    {
        public Udyr()
        {
            LoadSpells();
            LoadMenus();
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
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
                combomenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("UseItemCombo", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
                harassmenu.AddItem(new MenuItem("UseRHarass", "Use R").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclear = new Menu("Farm", "Farm");
            {
                laneclear.AddItem(new MenuItem("UseWFarm", "Use W").SetValue(true));
                laneclear.AddItem(new MenuItem("UseRFarm", "Use R").SetValue(true));
                laneclear.AddItem(new MenuItem("RFarmValue", "R More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("FarmMana", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclear);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseRJFarm", "Use R").SetValue(true)); 
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("UseWMisc", "W Spellblock").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawW", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawE", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawR", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
                    
        }
        
        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(200, TargetSelector.DamageType.Physical);            
            var UseQ = config.Item("UseECombo").GetValue<bool>();            
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var UseR = config.Item("UseRCombo").GetValue<bool>();
            var CastItems = config.Item("UseItemCombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToPhysical(target)) return;
                try
                {
                    if (Orbwalking.InAutoAttackRange(target))
                    {
                        if (CastItems)
                        {
                            myUtility.UseItems(0, target);
                        }
                        if (UseE && E.IsReady() &&
                            (Stunnable(target) || !target.HasBuff("Stun") || myUtility.TickCount - LastE > 4500) && CurrentStance != "Bear")
                        {
                            if (myUtility.ImmuneToCC(target)) return;
                            E.Cast();
                        }
                        if (UseQ && Q.IsReady() &&
                            (!Stunnable(target) || target.HasBuff("Stun") || (myUtility.TickCount - LastE < 4500 && myUtility.TickCount - LastQ > 4500)) &&
                            (CurrentStance != "Tiger" || !target.HasBuff("udyrtigerstancebleed")))
                        {
                            Q.Cast();
                        }
                        if (UseR && R.IsReady() &&
                            target.HasBuff("udyrtigerstancebleed") &&
                            CurrentStance != "Phoenix" && !Active("Bear"))
                        {
                            if (myUtility.ImmuneToMagic(target)) return;
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
                catch
                {                    
                }
            }
        }
        private void Harass()
        {
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var UseW = config.Item("UseWHarass").GetValue<bool>();
            var UseE = config.Item("UseEHarass").GetValue<bool>();
            var UseR = config.Item("UseRHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(200, TargetSelector.DamageType.Physical);      
            if (UseE && E.IsReady() && Stunnable(target))
            {
                E.Cast();
            }
            else
            {
                if (UseQ && Q.IsReady()) Q.Cast();
                if (UseW && W.IsReady()) W.Cast();
                if (UseE && E.IsReady()) E.Cast();
                if (UseR && R.IsReady()) R.Cast();
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (!Player.IsWindingUp && myUtility.TickCount - LastAny > 5000)
            {
                var allMinions = MinionManager.GetMinions(Player.ServerPosition, 250);
                if (allMinions == null) return;
                if (config.Item("UseRFarm").GetValue<bool>() && CurrentStance != "Phoenix" && R.IsReady() && allMinions.Count > config.Item("RFarmValue").GetValue<Slider>().Value && !Player.UnderTurret(true))
                {
                    R.Cast();
                }
                else if (config.Item("UseWFarm").GetValue<bool>() && CurrentStance != "Turtle" && myUtility.PlayerHealthPercentage < 100 && W.IsReady())
                {
                    W.Cast();
                }
            }
            
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, 250, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myUtility.GetLargeMonsters(250).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null && (Orbwalking.InAutoAttackRange(mob) || Orbwalking.InAutoAttackRange(largemobs)))
            {
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp && CurrentStance != "Bear")
                {
                    if (largemobs != null && !largemobs.HasBuff("udyrbearstuncheck"))
                    {
                        E.Cast();
                    }
                    else 
                    {
                        if (!mob.HasBuff("udyrbearstuncheck")) E.Cast();
                    }
                }
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && !largemobs.HasBuff("udyrtigerbleed"))
                    {
                        Q.Cast();
                    }
                    else
                    {
                        if (!mob.HasBuff("udyrtigerbleed")) Q.Cast();
                    }
                }
                if (config.Item("UseRJFarm").GetValue<bool>() && R.IsReady() && !Player.IsWindingUp && CurrentStance != "Phoenix")
                {
                    if (largemobs != null && myUtility.TickCount - LastAny > 2000 && mobs.Count() > 1)
                    {
                        R.Cast();
                    }
                }
            }          
        }

        private int LastAny = 0;
        private int LastQ = 0;
        private int LastW = 0;
        private int LastE = 0;
        private int LastR = 0;
        private bool Active(string Stance)
        {
            if (Stance == "Phoenix" && Player.HasBuff("udyrphoenixactivation")) return true;
            if (Stance == "Bear" && Player.HasBuff("udyrbearactivation")) return true;
            if (Stance == "Turtle" && Player.HasBuff("udyrturtleactivation")) return true;
            if (Stance == "Tiger" && Player.HasBuff("udyrtigerpunch")) return true;

            if (Stance == null)
            {
                if (!(Player.HasBuff("udyrphoenixactivation") && Player.HasBuff("udyrbearactivation") && Player.HasBuff("udyrturtleactivation") && Player.HasBuff("udyrtigerpunch")))
                {
                    return false;
                }
            }
            return false;
        }
        private string CurrentStance
        {
            get
            {
                if (Player.HasBuff("UdyrPhoenixStance")) return "Phoenix";
                if (Player.HasBuff("UdyrBearStance")) return "Bear";
                if (Player.HasBuff("UdyrTurtleStance")) return "Turtle";
                return Player.HasBuff("UdyrTigerStance") ? "Tiger" : "None";
            }
        }
        private bool Stunnable(Obj_AI_Hero target)
        {
            return !target.HasBuff("udyrbearstuncheck");
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
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell) 
        {
            if (unit.IsMe)
            {
                if (spell.SData.Name.ToLower().Contains("udyr") && spell.SData.Name.ToLower().Contains("stance"))
                {
                    LastAny = myUtility.TickCount;
                }
                if (spell.SData.Name.ToLower() == "udyrtigerstance")
                {
                    LastQ = myUtility.TickCount;
                }
                if (spell.SData.Name.ToLower() == "udyrturtlestance")
                {
                    LastW = myUtility.TickCount;
                }
                if (spell.SData.Name.ToLower() == "udyrbearstance")
                {
                    LastE = myUtility.TickCount;
                }
                if (spell.SData.Name.ToLower() == "udyrphoenixstance")
                {
                    LastR = myUtility.TickCount;
                }
            }
            if (!unit.IsMe && unit.IsEnemy && unit.IsValid<Obj_AI_Hero>())
            {
                if (spell.Target == null || !spell.Target.IsValid || !spell.Target.IsMe)
                {
                    return;
                }
                if (!spell.SData.IsAutoAttack() && spell.Target.IsMe && E.IsReady())
                {
                    if (unit.IsChampion(unit.BaseSkinName))
                    {
                        if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                        {
                            if (config.Item("UseWCombo").GetValue<bool>() && !Active(null))
                            {
                                Utility.DelayAction.Add(myUtility.RandomDelay(0, 200), () => W.Cast());
                            }
                        }
                        if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass)
                        {
                            if (config.Item("UseWHarass").GetValue<bool>())
                            {
                                Utility.DelayAction.Add(myUtility.RandomDelay(0, 200), () => W.Cast());
                            }
                        }
                        if (config.Item("UseWMisc").GetValue<bool>() && !Active(null))
                        {
                            Utility.DelayAction.Add(myUtility.RandomDelay(0, 200), () => W.Cast());
                        }
                    }
                }
            }
        } 
    }
}