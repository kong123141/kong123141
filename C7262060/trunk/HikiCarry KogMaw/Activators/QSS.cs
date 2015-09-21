using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace HikiCarry_KogMaw.Activators
{
    public static class QSS
    {
        private static Obj_AI_Base kogmaw = null;
        public static bool hikiQSS { get; set; }
        public const string championName = "KogMaw";

        public static Obj_AI_Base kogmawQSS
        {
            get
            {
                if (kogmaw != null && kogmaw.IsValid)
                {
                    return kogmaw;
                }
                return null;
            }
        }
        static QSS()
        {
            Game.OnUpdate += Game_OnUpdate;
            kogmaw = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(x => x.IsMe && x.CharData.BaseSkinName == championName);
            if (kogmaw != null)
            {
                Console.Write(kogmaw.CharData.BaseSkinName);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (kogmaw == null)
            {
                kogmaw = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(x => x.IsMe && x.CharData.BaseSkinName == championName);
            }
            if (kogmaw != null)
            {
                var useQSS = Program.Config.Item("use.qss").GetValue<bool>();  // use qss
                var clearIgnite = Program.Config.Item("clear.ignite").GetValue<bool>(); // clear ignite
                var clearExhaust = Program.Config.Item("clear.exhaust").GetValue<bool>(); // clear exhaust
                var clearZedR = Program.Config.Item("clear.zedult").GetValue<bool>(); // clear zed r
                var clearFizzR = Program.Config.Item("clear.fizzult").GetValue<bool>(); // clear fizz r
                var clearMalzR = Program.Config.Item("clear.malzaharult").GetValue<bool>(); // clear malz r
                var clearVladR = Program.Config.Item("clear.vladulti").GetValue<bool>(); // clear vlad r

                if (useQSS)
                {
                    if (kogmaw.HasBuffOfType(BuffType.Charm) || kogmaw.HasBuffOfType(BuffType.Flee) ||
                    kogmaw.HasBuffOfType(BuffType.Polymorph) || kogmaw.HasBuffOfType(BuffType.Snare) ||
                    kogmaw.HasBuffOfType(BuffType.Stun) || kogmaw.HasBuffOfType(BuffType.Suppression) ||
                    kogmaw.HasBuffOfType(BuffType.Taunt) && kogmaw.HasBuffOfType(BuffType.SpellShield) && kogmaw.HasBuffOfType(BuffType.SpellImmunity))
                    {
                        if (Items.HasItem(3140) && Items.CanUseItem(3140))
                        {
                            Items.UseItem(3140);
                        }
                        if (Items.HasItem(3139) && Items.CanUseItem(3139))
                        {
                            Items.UseItem(3139);
                        }
                    }
                    if (clearIgnite)
                    {
                        if (kogmaw.HasBuff("summonerdot"))
                        {
                            if (Items.HasItem(3140) && Items.CanUseItem(3140))
                            {
                                Items.UseItem(3140);
                            }
                            if (Items.HasItem(3139) && Items.CanUseItem(3139))
                            {
                                Items.UseItem(3139);
                            }
                        }
                    }
                    if (clearExhaust)
                    {
                        if (kogmaw.HasBuff("summonerexhaust"))
                        {
                            if (Items.HasItem(3140) && Items.CanUseItem(3140))
                            {
                                Items.UseItem(3140);
                            }
                            if (Items.HasItem(3139) && Items.CanUseItem(3139))
                            {
                                Items.UseItem(3139);
                            }
                        }
                    }
                    if (clearZedR)
                    {
                        if (kogmaw.HasBuff("zedulttargetmark"))
                        {
                            if (Items.HasItem(3140) && Items.CanUseItem(3140))
                            {
                                Items.UseItem(3140);
                            }
                            if (Items.HasItem(3139) && Items.CanUseItem(3139))
                            {
                                Items.HasItem(3140);
                            }
                        }
                    }
                    if (clearFizzR)
                    {
                        if (kogmaw.HasBuff("FizzMarinerDoom"))
                        {
                            if (Items.HasItem(3140) && Items.CanUseItem(3140))
                            {
                                Items.UseItem(3140);
                            }
                            if (Items.HasItem(3139) && Items.CanUseItem(3139))
                            {
                                Items.HasItem(3140);
                            }
                        }

                    }
                    if (clearMalzR)
                    {
                        if (kogmaw.HasBuff("AlZaharNetherGrasp"))
                        {
                            if (Items.HasItem(3140) && Items.CanUseItem(3140))
                            {
                                Items.UseItem(3140);
                            }
                            if (Items.HasItem(3139) && Items.CanUseItem(3139))
                            {
                                Items.HasItem(3140);
                            }
                        }
                    }
                    if (clearVladR)
                    {
                        if (kogmaw.HasBuff("VladimirHemoplague"))
                        {
                            if (Items.HasItem(3140) && Items.CanUseItem(3140))
                            {
                                Items.UseItem(3140);
                            }
                            if (Items.HasItem(3139) && Items.CanUseItem(3139))
                            {
                                Items.HasItem(3140);
                            }
                        }
                    }
                }

            }
        }
    }
}