using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using ItemData = LeagueSharp.Common.Data.ItemData;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
{
    class Varus : PluginData
    {
        public Varus()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 925); //1625 max charge
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 925);
            R = new Spell(SpellSlot.R, 1100);

            Q.SetSkillshot(.25f, 70f, 1650f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(.50f, 250f, 1400f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(.25f, 120f, 1950f, false, SkillshotType.SkillshotLine);

            Q.SetCharged("VarusQ", "VarusQ", 575, 1625, 1.2f);       
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Varus.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Varus.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Varus.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Varus.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                laneclearmenu.AddItem(new MenuItem("EC.Varus.Farm.Q", "Use Q").SetValue(true));
                config.AddSubMenu(laneclearmenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Varus.Muramana", "Muramana").SetValue(new Slider(50)));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Varus.Draw.Q", "Q").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(1600, TargetSelector.DamageType.Physical);

            var UseQ = config.Item("EC.Varus.Combo.Q").GetValue<bool>();
            var UseE = config.Item("EC.Varus.Combo.E").GetValue<bool>();
            var CastItems = config.Item("EC.Varus.Combo.Items").GetValue<bool>();
            if (UseQ && Q.IsReady() && Q.IsCharging)
            {
                if (Target != null && Target.IsValidTarget())
                {
                    if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) < 925 && Target.Buffs.Count(x => x.Name == "varuswdebuff") >= 3)
                    {
                        mySpellcast.Charge(
                        Target,
                        Q,
                        mySpellcast.ChargeState.Release,
                        HitChance.VeryHigh,
                        myUtility.MovementDisabled(Target),
                        1625f,
                        Target.BoundingRadius
                        );
                    }
                    else if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) >= 925 && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= 1625)
                    {
                        mySpellcast.Charge(
                        Target,
                        Q,
                        mySpellcast.ChargeState.Release,
                        HitChance.VeryHigh,
                        myUtility.MovementDisabled(Target),
                        1625f,
                        Target.BoundingRadius
                        );
                    }
                }
                if ((!Target.IsValidTarget() || Target == null))
                {
                    mySpellcast.Charge(
                        null,
                        Q,
                        mySpellcast.ChargeState.Discharge,                       
                        HitChance.OutOfRange,
                        false,
                        0,
                        0,
                        5500);
                }
            }
            if (Target != null && Target.IsValidTarget())
            {                
                try
                {
                    if (myUtility.ImmuneToDeath(Target)) return;
                    if (CastItems) { myItemManager.UseItems(0, Target); }
                    if (UseQ && Q.IsReady() && !Q.IsCharging)
                    {                        
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) < 925 && Target.Buffs.Count(x => x.Name == "varuswdebuff") >= 3)
                        {
                            mySpellcast.Charge(Target, Q, mySpellcast.ChargeState.Start);
                        }
                        else if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) >= 925 && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= 1625)
                        {
                            mySpellcast.Charge(Target, Q, mySpellcast.ChargeState.Start);
                        }
                    
                    }
                    if (UseE && E.IsReady())
                    {
                        mySpellcast.CircularAoe(Target, E, HitChance.High);
                    }
                }
                catch { }
            }
        }
        private void Custom()
        {
            var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToMagic(x) && !myUtility.ImmuneToDeath(x));

            Target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ?
                TargetSelector.GetSelectedTarget() :
                EnemyList.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= 1625).OrderBy(i => i.Health).ThenByDescending(z => myRePriority.ResortDB(z.ChampionName)).FirstOrDefault();//.OrderByDescending(z => myRePriority.ResortDB(z.ChampionName)).ThenBy(i => i.Health).FirstOrDefault();

            if (Target == null || Vector3.Distance(Player.ServerPosition,Target.ServerPosition) > 1625) return;
            if (!Q.IsCharging)
            {
                Q.StartCharging();
            }
            else if (Q.IsCharging)
            {
                PredictionOutput pred = Q.GetPrediction(Target);
                if (pred.Hitchance >= HitChance.High)
                {
                    if (Vector3.Distance(Player.ServerPosition, pred.CastPosition) <= Q.ChargedMaxRange)
                    {
                        Q.Cast(Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition) + Target.BoundingRadius));
                    }
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
                    if (Player.HasBuff("Muramana") || (myUtility.PlayerManaPercentage < config.Item("EC.Varus.Muramana").GetValue<Slider>().Value))
                    {
                        if (Items.HasItem(3042) && Items.CanUseItem(3042)) Items.UseItem(3042);
                    }
                    break;
                case myOrbwalker.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case myOrbwalker.OrbwalkingMode.LaneClear:
                    if (config.Item("EC.Varus.Farm.Q").GetValue<bool>())
                    {
                        var minion = MinionManager.GetMinions(Player.ServerPosition, Q.ChargedMaxRange);
                        if (minion.Count() >= 3)
                        {
                            if (Q.IsCharging)
                            {
                                if (Q.Range >= Q.ChargedMaxRange)
                                {
                                    myFarmManager.LaneLinear(Q, Q.Range, minion.Count() >= 6);
                                }
                            }
                            if (myUtility.EnoughMana(config.Item("EC.Varus.Farm.ManaPercent").GetValue<Slider>().Value))
                            {
                                Q.StartCharging();
                            }
                        }
                    }
                    break;
                case myOrbwalker.OrbwalkingMode.Custom:
                    Custom();
                    break;

                    
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
            {
                if (args.Slot == SpellSlot.R)
                {
                    if (ItemData.Muramana.GetItem().IsReady() && !Player.HasBuff("Muramana") && myUtility.PlayerManaPercentage > config.Item("EC.Varus.Muramana").GetValue<Slider>().Value)
                    {
                        if (Items.HasItem(3042) && Items.CanUseItem(3042)) Items.UseItem(3042);
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
                    if (ItemData.Muramana.GetItem().IsReady() && !Player.HasBuff("Muramana") && myUtility.PlayerManaPercentage > config.Item("EC.Varus.Muramana").GetValue<Slider>().Value)
                    {
                        if (Items.HasItem(3042) && Items.CanUseItem(3042)) Items.UseItem(3042);
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Varus.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                if (Player.HasBuff("Muramana"))
                {
                    Render.Circle.DrawCircle(Player.Position, 925, Color.Cyan);
                    Render.Circle.DrawCircle(Player.Position, 1625, Color.Cyan);
                }
                else
                {
                    Render.Circle.DrawCircle(Player.Position, 925, Color.White);
                    Render.Circle.DrawCircle(Player.Position, 1625, Color.White);
                }
            }
        }
    }
}
