using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace HikiCarry_Kalista.Activators
{
    public static class QSS
    {
        private static Obj_AI_Base kalista = null;
        public static bool hikiQSS { get; set; }
        public const string championName = "Kalista";

        public static Obj_AI_Base KalistaQSS
        {
            get
            {
                if (kalista != null && kalista.IsValid)
                {
                    return kalista;
                }
                return null;
            }
        }
        static QSS()
        {
            Game.OnUpdate += Game_OnUpdate;
            kalista = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(x => x.IsMe && x.CharData.BaseSkinName == championName);
            if (kalista != null)
            {
                Console.Write(kalista.CharData.BaseSkinName);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (kalista == null)
            {
                kalista = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(x => x.IsMe && x.CharData.BaseSkinName == championName);
            }
            if (kalista != null)
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
                    if (kalista.HasBuffOfType(BuffType.Charm) || kalista.HasBuffOfType(BuffType.Flee) ||
                    kalista.HasBuffOfType(BuffType.Polymorph) || kalista.HasBuffOfType(BuffType.Snare) ||
                    kalista.HasBuffOfType(BuffType.Stun) || kalista.HasBuffOfType(BuffType.Suppression) ||
                    kalista.HasBuffOfType(BuffType.Taunt) && kalista.HasBuffOfType(BuffType.SpellShield) && kalista.HasBuffOfType(BuffType.SpellImmunity))
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
                        if (kalista.HasBuff("summonerdot"))
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
                        if (kalista.HasBuff("summonerexhaust"))
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
                        if (kalista.HasBuff("zedulttargetmark"))
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
                        if (kalista.HasBuff("FizzMarinerDoom"))
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
                        if (kalista.HasBuff("AlZaharNetherGrasp"))
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
                        if (kalista.HasBuff("VladimirHemoplague"))
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
