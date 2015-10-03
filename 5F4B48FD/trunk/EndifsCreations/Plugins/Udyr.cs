using System;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace EndifsCreations.Plugins
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
                combomenu.AddItem(new MenuItem("EC.Udyr.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Udyr.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Udyr.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Udyr.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Udyr.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Udyr.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Udyr.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Udyr.Harass.E", "Use E").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Udyr.UseRHarass", "Use R").SetValue(true));
                Root.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Udyr.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Udyr.UseRFarm", "Use R").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Udyr.RFarmValue", "R More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Udyr.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                Root.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Udyr.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Udyr.Jungle.W", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Udyr.Jungle.E", "Use E").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Udyr.UseRJFarm", "Use R").SetValue(true)); 
                Root.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Udyr.Misc.W", "W Shields").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Udyr.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Udyr.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Udyr.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Udyr.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
                    
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(300, TargetSelector.DamageType.Physical);

            var UseQ = Root.Item("EC.Udyr.Combo.E").GetValue<bool>();
            var UseE = Root.Item("EC.Udyr.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Udyr.Combo.R").GetValue<bool>();
            var CastItems = Root.Item("EC.Udyr.Combo.Items").GetValue<bool>();
            if (UseR && R.IsReady())
            {
                if (Target.IsValidTarget() && Orbwalking.InAutoAttackRange(Target) && (Target.HasBuff("udyrbearstuncheck") || Target.HasBuff("udyrtigerstancebleed")) && CurrentStance != "Phoenix")
                {
                    R.Cast();
                }
                else if (Player.CountEnemiesInRange(300) > 1 && CurrentStance != "Phoenix")
                {
                    R.Cast();
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;
                try
                {
                    if (!Orbwalking.InAutoAttackRange(Target) && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= 600)
                    {
                        if (UseE && E.IsReady() && Target.MoveSpeed > Player.MoveSpeed)
                        {
                            if (myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                            {
                                E.Cast();
                            }
                        }
                    }
                    if (Orbwalking.InAutoAttackRange(Target))
                    {
                       
                        if (UseE && E.IsReady() && !Target.HasBuff("udyrbearstuncheck") && CurrentStance != "Bear")                            
                        {
                            if (myUtility.ImmuneToCC(Target)) return;
                            if (UseQ && Q.IsReady() && !Target.HasBuff("udyrtigerstancebleed") && CurrentStance == "Tiger") return;
                            if (myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                            {
                                E.Cast();
                            }
                        }
                        if (UseQ && Q.IsReady() && !Target.HasBuff("udyrtigerstancebleed") && CurrentStance != "Tiger")                            
                        {
                            if (UseE && E.IsReady() && !Target.HasBuff("udyrbearstuncheck") && CurrentStance == "Bear" && !myUtility.ImmuneToCC(Target)) return;
                            if (myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                            {
                                Q.Cast();
                            }
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
                catch
                {
                }
            }
        }

        private void Harass()
        {
            var UseQ = Root.Item("EC.Udyr.Harass.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Udyr.Harass.W").GetValue<bool>();
            var UseE = Root.Item("EC.Udyr.Harass.E").GetValue<bool>();
            var UseR = Root.Item("EC.Udyr.UseRHarass").GetValue<bool>();
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
            if (myUtility.PlayerManaPercentage < Root.Item("EC.Udyr.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (!Player.IsWindingUp && myUtility.TickCount - LastSpell > 5000)
            {
                var allMinions = MinionManager.GetMinions(Player.ServerPosition, 250);
                if (allMinions == null) return;
                if (Root.Item("EC.Udyr.UseRFarm").GetValue<bool>() && CurrentStance != "Phoenix" && R.IsReady() && allMinions.Count > Root.Item("EC.Udyr.RFarmValue").GetValue<Slider>().Value && !Player.UnderTurret(true))
                {
                    R.Cast();
                }
                else if (Root.Item("EC.Udyr.Farm.W").GetValue<bool>() && CurrentStance != "Turtle" && myUtility.PlayerHealthPercentage < 100 && W.IsReady())
                {
                    W.Cast();
                }
            }
            
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, 250, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, 250).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null && (Orbwalking.InAutoAttackRange(mob) || Orbwalking.InAutoAttackRange(largemobs)))
            {
                if (Root.Item("EC.Udyr.Jungle.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp && CurrentStance != "Bear")
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
                if (Root.Item("EC.Udyr.Jungle.Q").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
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
                if (Root.Item("EC.Udyr.UseRJFarm").GetValue<bool>() && R.IsReady() && !Player.IsWindingUp && CurrentStance != "Phoenix")
                {
                    if (largemobs != null && myUtility.TickCount - LastSpell > 2000 && mobs.Count() > 1)
                    {
                        R.Cast();
                    }
                }
            }          
        }

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
                    LastSpell = myUtility.TickCount;
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
            if (unit is Obj_AI_Hero && unit.IsEnemy && !spell.SData.IsAutoAttack() && W.IsReady())
            {
                if ((myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Udyr.Combo.W").GetValue<bool>()) ||
                    (Root.Item("EC.Udyr.Misc.W").GetValue<bool>())
                    )
                {
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        var rectangle = new Geometry.Polygon.Rectangle(spell.Start, spell.End, Player.BoundingRadius);
                        if (rectangle.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 100))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast());
                        }
                    }
                    else if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Unit) || spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit)) && spell.Target != null && spell.Target.IsMe)
                    {
                        W.Cast();
                    }
                    else if (spell.End.Distance(Player.ServerPosition) <= 100)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast());
                    }
                }
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (Root.Item("EC.Udyr.Combo.Items").GetValue<bool>())
                    {
                        myItemManager.UseItems(0, null);
                        myItemManager.UseItems(2, null);
                    }
                }
            }
        }
    }
}