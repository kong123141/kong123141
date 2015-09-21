﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace MasterOfInsec.Combos
{
    static class Combo
    {
        public static HitChance HitchanceCheck(int i)
        {
            switch (i)
            {
                case 1:
                    return HitChance.Low;
                case 2:
                    return HitChance.Medium;
                case 3:
                    return HitChance.High;
                case 4:
                    return HitChance.VeryHigh;

            }
            return HitChance.Low;
        }
        public static void Do()
        {

            var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Physical);
            if (target != null)
            {
                if (Program.Q.IsReady() && Program.GetBool("comboQ") && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name == "BlindMonkQOne")
                {
                    Program.Q.CastIfHitchanceEquals(target, HitchanceCheck(Program.menu.Item("seth").GetValue<Slider>().Value)); // Continue like that
                }
                else if (Program.Q.IsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name == "BlindMonkQOne")
                {
                    if (Program.GetBool("csmiteq"))
                    {

                        List<Obj_AI_Base> minions = new List<Obj_AI_Base>();
                        foreach (Obj_AI_Base m in ObjectManager.Get<Obj_AI_Base>())
                        {
                            if (Geometry.Intersection(Program.Player.Position.To2D(), target.Position.To2D(), m.Position.To2D(), m.Position.To2D()).Intersects)
                            {
                                minions.Add(m);
                            }
                        }
                        if (minions.Count == 1)
                        {
                            if (Program.Smite.IsReady())
                            {
                                ObjectManager.Player.Spellbook.CastSpell(Program.Smite, minions[0]);
                                
                                Program.Q.Cast(target.Position);
                            }

                        }

                    }
                }
             if (Program.GetBool("comboQ") && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name == "blindmonkqtwo")
             {
                 Program.Q.Cast();
             }
                //work 
                #region work
                if (Program.E.IsReady() && Program.GetBool("comboE") && Program.E.IsInRange(target))
                {
                    Program.E.Cast();
                    if (Items.CanUseItem(3077) && Program.Player.Distance(target.Position) < 350)
                        Items.UseItem(3077);
                    if (Items.CanUseItem(3074) && Program.Player.Distance(target.Position) < 350)
                        Items.UseItem(3074);
                    if (Items.CanUseItem(3142) && Program.Player.Distance(target.Position) < 350)
                        Items.UseItem(3142);
                }
                if (Program.W.IsReady() && Program.GetBool("comboW"))
                {
                    if (ObjectManager.Player.HealthPercent <= Program.menu.Item("Set W life %").GetValue<Slider>().Value)
                    {
                        Program.W.Cast(Program.Player);
                    }
                }
                //can kill
                if (Program.E.IsReady() && Program.E.IsKillable(target)) // si la e mata
                {
                    Program.E.Cast(target);
                }
                else if (Program.R.IsReady() && Program.GetBool("comboR") && Program.R.IsKillable(target)) // si solo la r mata
                {
                    Program.R.Cast(target);
                }
                else if (Program.Ignite.IsReady() && Program.R.IsReady() && Program.menu.Item("IgniteR").GetValue<bool>()) // si ignite R mata
                {
                    double DamageRIgnite = ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) + Program.Player.GetSpellDamage(target, SpellSlot.R);
                    if (target.Health - DamageRIgnite <= 0)
                    {
                        Program.R.Cast(target);
                        ObjectManager.Player.Spellbook.CastSpell(Program.Ignite, target);
                    }
                }
                //end can kill
                if (Program.Ignite.IsReady()) // ignite cuando esta bajo
                {
                    if (target.Health - ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) <= 0)
                        ObjectManager.Player.Spellbook.CastSpell(Program.Ignite, target);
                }
                #endregion
            
        }
        }
    }
}
