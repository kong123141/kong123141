using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;


//这个惩戒是ELSmite 我是提取过来 并且精简掉里面的英雄插件功能 
//因为我只需要一个单独秒惩戒..而EL是最好的选择
//由于尊重原作者 我在此表示敬意 并且保留作者的信息 不删减!


namespace 花边_花式多合一.Core
{
    class AutoSmite
    {
        public static Obj_AI_Minion minion;
        public static Spell smite;
        private static readonly string[] BuffsThatActuallyMakeSenseToSmite =
            {
                "SRU_Red", "SRU_Blue", "SRU_Dragon",
                "SRU_Baron", "SRU_Gromp", "SRU_Murkwolf",
                "SRU_Razorbeak", "SRU_Krug", "Sru_Crab",
                "TT_Spiderboss", "TTNGolem", "TTNWolf",
                "TTNWraith"
            };

        private static SpellDataInst slot1;
        private static SpellDataInst slot2;
        private static SpellSlot smiteSlot;
        public static bool IsSummonersRift
        {
            get
            {
                return Game.MapId == GameMapId.SummonersRift;
            }
        }
        public static bool IsTwistedTreeline
        {
            get
            {
                return Game.MapId == GameMapId.TwistedTreeline;
            }
        }

        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if (!InitializeMenu.Menu.Item("AutoSmite").GetValue<bool>()) return;
                slot1 = Huabian.Player.Spellbook.GetSpell(SpellSlot.Summoner1);
                slot2 = Huabian.Player.Spellbook.GetSpell(SpellSlot.Summoner2);
                var smiteNames = new[]
                                     {
                                         "s5_summonersmiteplayerganker", "itemsmiteaoe", "s5_summonersmitequick",
                                         "s5_summonersmiteduel", "summonersmite"
                                     };

                if (smiteNames.Contains(slot1.Name))
                {
                    smite = new Spell(SpellSlot.Summoner1, 550f);
                    smiteSlot = SpellSlot.Summoner1;
                }
                else if (smiteNames.Contains(slot2.Name))
                {
                    smite = new Spell(SpellSlot.Summoner2, 550f);
                    smiteSlot = SpellSlot.Summoner2;
                }
                else
                {
                    Console.WriteLine("You don't have smite faggot");
                    return;
                }

                Game.OnUpdate += Game_OnUpdate;
            }
            catch (Exception ex)
            {
                Console.WriteLine("AutoSmite error occurred: '{0}'", ex);
            }

        }

        public static double SmiteDamage()
        {
            var damage = new[]
                             {
                                 20 * Huabian.Player.Level + 370, 30 * Huabian.Player.Level + 330, 40 * + Huabian.Player.Level + 240,
                                 50 * Huabian.Player.Level + 100
                             };

            return Huabian.Player.Spellbook.CanUseSpell(smite.Slot) == SpellState.Ready ? damage.Max() : 0;
        }
        private static void JungleSmite()
        {
            if (!InitializeMenu.Menu.Item("ElSmite.Activated").GetValue<KeyBind>().Active)
            {
                return;
            }

            minion =
                (Obj_AI_Minion)
                MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 500, MinionTypes.All, MinionTeam.Neutral)
                    .ToList()
                    .FirstOrDefault(
                        buff =>
                        buff.IsValidTarget() && BuffsThatActuallyMakeSenseToSmite.Contains(buff.CharData.BaseSkinName));

            if (minion == null)
            {
                return;
            }

            //Mob = minion;

            if (InitializeMenu.Menu.Item(minion.CharData.BaseSkinName).GetValue<bool>())
            {
                if (SmiteDamage() > minion.Health + 10)
                {
                    Huabian.Player.Spellbook.CastSpell(smite.Slot, minion);
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Huabian.Player.IsDead)
            {
                return;
            }

            try
            {
                JungleSmite();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }
    }
}
