/*
 

                                                    __                        .-'''-.                             
       .-''-.                    .---.         ...-'  |`.                    '   _    \                           
     .' .-.  )         .--.      |   |         |      |  |  __  __   ___   /   /` '.   \   _..._       .          
    / .'  / /    .--./)|__|      |   |         ....   |  | |  |/  `.'   `..   |     \  ' .'     '.   .'|          
   (_/   / /    /.''\\ .--.-,.--.|   |           -|   |  | |   .-.  .-.   |   '      |  .   .-.   ..'  |          
        / /    | |  | ||  |  .-. |   |            |   |  | |  |  |  |  |  \    \     / /|  '   '  <    |          
       / /      \`-' / |  | |  | |   |      _  ...'   `--' |  |  |  |  |  |`.   ` ..' / |  |   |  ||   | ____     
      . '       /("'`  |  | |  | |   |    .' | |         |`|  |  |  |  |  |   '-...-'`  |  |   |  ||   | \ .'     
     / /    _.-'\ '---.|  | |  '-|   |   .   | ` --------\ |  |  |  |  |  |             |  |   |  ||   |/  .      
   .' '  _.'.-'' /'""'.|__| |    |   | .'.'| |//`---------'|__|  |__|  |__|             |  |   |  ||    /\  \     
  /  /.-'_.'    ||     || | |    '---.'.'.-'  /                                         |  |   |  ||   |  \  \    
 /    _.'       \'. __//  |_|        .'   \_.'                                          |  |   |  |'    \  \  \   
( _.-'           `'---'                                                                 '--'   '--'------'  '---' 

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace two_girls_one_monk
{
    class Laneclear
    {
        public static Obj_AI_Hero Player = ObjectManager.Player;

        public static void Clear()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, LeeSin.Q.Range).FirstOrDefault();
            if (minion == null || minion.Name.ToLower().Contains("ward")) return;
            if (LeeSinSharp.Config.Item("UseQClear").GetValue<bool>() && LeeSin.Q.IsReady())
            {
                useItems2(minion);
                if (LeeSin.Q.Instance.Name == "BlindMonkQOne")
                {
                    LeeSin.Q.Cast(minion, true);
                }
                else if ((minion.HasBuff("BlindMonkQOne", true) ||
                         minion.HasBuff("blindmonkqonechaos", true)) && (LeeSin.Q.IsKillable(minion, 1)) ||
                         Player.Distance(minion) > 500) LeeSin.Q.Cast();
            }
            if (LeeSinSharp.Config.Item("UseWClear").GetValue<bool>() && LeeSin.W.IsReady())
            {
                if ((LeeSin.W.Instance.Name == "BlindMonkWOne") && (Player.HealthPercent <= 75))
                {
                    LeeSin.W.Cast(Player, true);
                }
                else if ((Player.HasBuff("BlindMonkWOne", true) && (Player.HealthPercent <= 70)))
                    LeeSin.W.Cast();
            }
            if (LeeSinSharp.Config.Item("UseEClear").GetValue<bool>() && LeeSin.E.IsReady())
            {
                if (LeeSin.E.Instance.Name == "BlindMonkEOne" && minion.IsValidTarget(LeeSin.E.Range))
                {
                    LeeSin.E.Cast();
                }
                else if (minion.HasBuff("BlindMonkEOne", true) && (Player.Distance(minion) > 450) && HasEnergyFor(false, true, true, false))
                {
                    LeeSin.E.Cast();
                }
            }
        }
            public static void Jgclear()
            {
            var jngm = MinionManager.GetMinions(Player.ServerPosition, LeeSin.Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();
            if (jngm == null || jngm.Name.ToLower().Contains("ward")) return;
            if (LeeSinSharp.Config.Item("UseQClear").GetValue<bool>() && LeeSin.Q.IsReady())
            {
                useItems2(jngm);
                if (LeeSin.Q.Instance.Name == "BlindMonkQOne")
                {
                    LeeSin.Q.Cast(jngm, true);
                }
                else if ((jngm.HasBuff("BlindMonkQOne", true) ||
                         jngm.HasBuff("blindmonkqonechaos", true)) || Player.Distance(jngm) > 500 && !Player.HasBuff("BlindMonkFlurry", true)) 
                    LeeSin.Q.Cast();
            }
            if (LeeSinSharp.Config.Item("UseWClear").GetValue<bool>() && LeeSin.W.IsReady())
            {
                if ((LeeSin.W.Instance.Name == "BlindMonkWOne") && (Player.HealthPercent <= 80))
                {
                    LeeSin.W.Cast(Player, true);
                }
                else if ((Player.HasBuff("BlindMonkWOne", true) && (Player.HealthPercent <= 75)))
                    LeeSin.W.Cast();
            }
            if (LeeSinSharp.Config.Item("UseEClear").GetValue<bool>() && LeeSin.E.IsReady())
            {
                if (LeeSin.E.Instance.Name == "BlindMonkEOne" && jngm.IsValidTarget(LeeSin.E.Range))
                {
                    LeeSin.E.Cast();
                }
                else if (jngm.HasBuff("BlindMonkEOne", true) && (Player.Distance(jngm) > 450) && HasEnergyFor(false, true, true, false))
                {
                    LeeSin.E.Cast();
                }
            }
        }

            public static void useItems2(Obj_AI_Base enemy)
            {
            if ((Items.CanUseItem(3077) && Player.Distance(enemy) < 350))
            Items.UseItem(3077);
            if ((Items.CanUseItem(3074) && Player.Distance(enemy) < 350))
            Items.UseItem(3074);
            }
        
        
            static bool HasEnergyFor(bool Q, bool W, bool E, bool R)
            {
                float totalCost = 0;

                if (Q)
                    totalCost += Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
                if (W)
                    totalCost += Player.Spellbook.GetSpell(SpellSlot.W).ManaCost;
                if (E)
                    totalCost += Player.Spellbook.GetSpell(SpellSlot.E).ManaCost;
                if (R)
                    totalCost += Player.Spellbook.GetSpell(SpellSlot.R).ManaCost;

                if (Player.Mana >= totalCost)
                    return true;
                else
                    return false;
            }  
        }
    }
