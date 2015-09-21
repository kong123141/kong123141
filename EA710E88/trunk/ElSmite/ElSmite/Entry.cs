﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace ElSmite
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Entry
    {
        static SpellSlot smiteSlot;
        static Obj_AI_Hero Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        public static String ScriptVersion
        {
            get
            {
                return typeof(Entry).Assembly.GetName().Version.ToString();
            }
        }

        public static bool IsTwistedTreeline
        {
            get
            {
                return Game.MapId == GameMapId.TwistedTreeline;
            }
        }

        public static bool IsSummonersRift
        {
            get
            {
                return Game.MapId == GameMapId.SummonersRift;
            }
        }

        #region Smite

        static SpellDataInst slot1;
        static SpellDataInst slot2;

        static Spell smite;

        static readonly string[] BuffsThatActuallyMakeSenseToSmite =
        {
            "SRU_Red", "SRU_Blue", "SRU_Dragon",
                "SRU_Baron", "SRU_Gromp", "SRU_Murkwolf",
                "SRU_Razorbeak", "SRU_Krug", "Sru_Crab",
                "TT_Spiderboss", "TTNGolem", "TTNWolf",
                "TTNWraith"
        };

        #endregion


        #region OnLoad

        public static void OnLoad(EventArgs args)
        {
         
            try
            {
                slot1 = Player.Spellbook.GetSpell(SpellSlot.Summoner1);
                slot2 = Player.Spellbook.GetSpell(SpellSlot.Summoner2);
                var smiteNames = new[]
                {
                    "s5_summonersmiteplayerganker", "itemsmiteaoe", "s5_summonersmitequick", "s5_summonersmiteduel",
                    "summonersmite"
                };

                if (smiteNames.Contains(slot1.Name))
                {
                    smite = new Spell(SpellSlot.Summoner1, 550f);
                    smiteSlot = SpellSlot.Summoner1;
                }else if (smiteNames.Contains(slot2.Name))
                {
                    smite = new Spell(SpellSlot.Summoner2, 550f);
                    smiteSlot = SpellSlot.Summoner2;
                }
                else
                {
                    Console.WriteLine("You don't have smite faggot");
                    return;
                }

                Notifications.AddNotification(String.Format("ElSmite by jQuery v{0}", ScriptVersion), 10000);
                InitializeMenu.Load();
                Game.OnUpdate += OnUpdate;
                Drawing.OnDraw += OnDraw;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion

        #region OnUpdate    

        static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead) return;

            try
            {
                JungleSmite();
                SmiteKill();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion

        #region Smite

        static void JungleSmite()
        {
            if (!InitializeMenu.Menu.Item("ElSmite.Activated").GetValue<KeyBind>().Active) return;

            Obj_AI_Minion minion = (Obj_AI_Minion) MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 500, MinionTypes.All, MinionTeam.Neutral).ToList().FirstOrDefault(buff => buff.IsValidTarget() && BuffsThatActuallyMakeSenseToSmite.Contains(buff.CharData.BaseSkinName));

            if (minion == null)
            {
                return;
            }

            if (InitializeMenu.Menu.Item(minion.CharData.BaseSkinName).GetValue<bool>())
            {
                if (SmiteDamage() > minion.Health + 10)
                {
                    Player.Spellbook.CastSpell(smite.Slot, minion);
                }
            }
        }

        #endregion


        #region SmiteKill

        static void SmiteKill()
        {
            if (!InitializeMenu.Menu.Item("ElSmite.KS.Activated").GetValue<bool>()) return;

            var kSableEnemy =
                HeroManager.Enemies.FirstOrDefault(
                    hero => hero.IsValidTarget(550) &&
                        SmiteChampDamage() >= hero.Health);
            if (kSableEnemy != null)
            {
                Player.Spellbook.CastSpell(smite.Slot, kSableEnemy);
            }
         }
        
        #endregion

        #region SmiteDamages

        static double SmiteDamage()
        {
            var damage = new int[] { 20 * Player.Level + 370, 30 * Player.Level + 330, 40 *+ Player.Level + 240, 50 * Player.Level + 100 };

            return Player.Spellbook.CanUseSpell(smite.Slot) == SpellState.Ready ? damage.Max() : 0;
        }

        static double SmiteChampDamage()
        {
            if (smite.Slot == Player.GetSpellSlot("s5_summonersmiteduel"))
            {
                var damage = new int[] { 54 + 6 * Player.Level };
                return Player.Spellbook.CanUseSpell(smite.Slot) == SpellState.Ready ? damage.Max() : 0;
            }

            if (smite.Slot == Player.GetSpellSlot("s5_summonersmiteplayerganker"))
            {
                var damage = new int[] { 20 + 8 * Player.Level };
                return Player.Spellbook.CanUseSpell(smite.Slot) == SpellState.Ready ? damage.Max() : 0;
            }

            return 0;
        }

        #endregion

        #region OnDraw

        static void OnDraw(EventArgs args)
        {
            var smiteActive = InitializeMenu.Menu.Item("ElSmite.Activated").GetValue<KeyBind>().Active;
            var drawSmite = InitializeMenu.Menu.Item("ElSmite.Draw.Range").GetValue<Circle>();
            var drawText = InitializeMenu.Menu.Item("ElSmite.Draw.Text").GetValue<bool>();
            var playerPos = Drawing.WorldToScreen(ObjectManager.Player.Position);
            var drawDamage  = InitializeMenu.Menu.Item("ElSmite.Draw.Damage").GetValue<bool>();

            if (smiteActive)
            {
                if (drawText && Player.Spellbook.CanUseSpell(smite.Slot) == SpellState.Ready)
                {
                    Drawing.DrawText(playerPos.X - 70, playerPos.Y + 40, Color.GhostWhite, "Smite active");
                }

                if (drawText && Player.Spellbook.CanUseSpell(smite.Slot) != SpellState.Ready)
                {
                    Drawing.DrawText(playerPos.X - 70, playerPos.Y + 40, Color.Red, "Smite cooldown");
                }

                if (drawDamage && Entry.SmiteDamage() != 0)
                {
                    var minions =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(
                                m =>
                                    m.Team == GameObjectTeam.Neutral && m.IsValidTarget() &&
                                    BuffsThatActuallyMakeSenseToSmite.Contains(m.CharData.BaseSkinName));
                    
                    foreach (var Minion in minions.Where(m => m.IsHPBarRendered))
                    {
                        var hpBarPosition = Minion.HPBarPosition;
                        var maxHealth = Minion.MaxHealth;
                        var smiteDamage = Entry.SmiteDamage();
                        //SmiteDamage : MaxHealth = x : 100
                        //Ratio math for this ^
                        var x = Entry.SmiteDamage() / maxHealth;
                        var barWidth = 0;

                        /*
                        * DON'T STEAL THE OFFSETS FOUND BY ASUNA DON'T STEAL THEM JUST GET OUT WTF MAN.
                        * EL SMITE IS THE BEST SMITE ASSEMBLY ON LEAGUESHARP AND YOU WILL NOT FIND A BETTER ONE.
                        * THE DRAWINGS ACTUALLY MAKE FUCKING SENSE AND THEY ARE FUCKING GOOD
                        * GTFO HERE SERIOUSLY OR I CALL DETUKS FOR YOU GUYS
                        * NO STEAL OR DMC FUCKING A REPORT.
                        * HELLO COPYRIGHT BY ASUNA 2015 ALL AUSTRALIAN RIGHTS RESERVED BY UNIVERSAL GTFO SERIOUSLY THO
                        * NO ALSO NO CREDITS JUST GET OUT DUDE GET OUTTTTTTTTTTTTTTTTTTTTTTT
                        */
                        
                        switch (Minion.CharData.BaseSkinName)
                        {
                            case "SRU_Red":
                            case "SRU_Blue":
                            case "SRU_Dragon":
                                barWidth = 145;
                                Drawing.DrawLine(new Vector2(hpBarPosition.X + 3 + (float)(barWidth * x), hpBarPosition.Y + 18), new Vector2(hpBarPosition.X + 3 + (float)(barWidth * x), hpBarPosition.Y + 28), 2f, Color.Chartreuse);
                                Drawing.DrawText(hpBarPosition.X - 22 + (float)(barWidth * x), hpBarPosition.Y, Color.Chartreuse, smiteDamage.ToString());
                                break; 
                            case "SRU_Baron":
                                barWidth = 194;
                                Drawing.DrawLine(new Vector2(hpBarPosition.X - 22 + (float)(barWidth * x), hpBarPosition.Y + 13), new Vector2(hpBarPosition.X - 22 + (float)(barWidth * x), hpBarPosition.Y + 29), 2f, Color.Chartreuse);
                                Drawing.DrawText(hpBarPosition.X - 22 + (float)(barWidth * x), hpBarPosition.Y - 3, Color.Chartreuse, smiteDamage.ToString());
                                break;
                            case "Sru_Crab":
                                barWidth = 61;
                                Drawing.DrawLine(new Vector2(hpBarPosition.X + 45 + (float)(barWidth * x), hpBarPosition.Y + 34), new Vector2(hpBarPosition.X  + 45 + (float)(barWidth * x), hpBarPosition.Y + 37), 2f, Color.Chartreuse);
                                Drawing.DrawText(hpBarPosition.X + 40 + (float)(barWidth * x), hpBarPosition.Y + 16, Color.Chartreuse, smiteDamage.ToString());
                                break;
                            case "SRU_Murkwolf":
                                barWidth = 75;
                                Drawing.DrawLine(new Vector2(hpBarPosition.X + 54 + (float)(barWidth * x), hpBarPosition.Y + 19), new Vector2(hpBarPosition.X + 54 + (float)(barWidth * x), hpBarPosition.Y + 23), 2f, Color.Chartreuse);
                                Drawing.DrawText(hpBarPosition.X + 50 + (float)(barWidth * x), hpBarPosition.Y, Color.Chartreuse, smiteDamage.ToString());
                                break;
                            case "SRU_Razorbeak":
                                barWidth = 75;
                                Drawing.DrawLine(new Vector2(hpBarPosition.X + 54 + (float)(barWidth * x), hpBarPosition.Y + 18), new Vector2(hpBarPosition.X + 54 + (float)(barWidth * x), hpBarPosition.Y + 22), 2f, Color.Chartreuse);
                                Drawing.DrawText(hpBarPosition.X + 54 + (float)(barWidth * x), hpBarPosition.Y, Color.Chartreuse, smiteDamage.ToString());
                                break;
                            case "SRU_Krug":
                                barWidth = 81;
                                Drawing.DrawLine(new Vector2(hpBarPosition.X + 58 + (float)(barWidth * x), hpBarPosition.Y + 18), new Vector2(hpBarPosition.X + 58 + (float)(barWidth * x), hpBarPosition.Y + 22), 2f, Color.Chartreuse);
                                Drawing.DrawText(hpBarPosition.X + 54 + (float)(barWidth * x), hpBarPosition.Y, Color.Chartreuse, smiteDamage.ToString());
                                break;
                            case "SRU_Gromp":
                                barWidth = 87;
                                Drawing.DrawLine(new Vector2(hpBarPosition.X + 62 + (float)(barWidth * x), hpBarPosition.Y + 18), new Vector2(hpBarPosition.X + 62 + (float)(barWidth * x), hpBarPosition.Y + 22), 2f, Color.Chartreuse);
                                Drawing.DrawText(hpBarPosition.X + 58 + (float)(barWidth * x), hpBarPosition.Y, Color.Chartreuse, smiteDamage.ToString());
                                break;

                        }
                        
                    }
                }
            }
            else
            {
                if(drawText)
                    Drawing.DrawText(playerPos.X - 70, playerPos.Y + 40, Color.Red, "Smite not active");
            }
            

            if (smiteActive && drawSmite.Active && Player.Spellbook.CanUseSpell(smite.Slot) == SpellState.Ready)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 500, Color.Green);

            if (drawSmite.Active && Player.Spellbook.CanUseSpell(smite.Slot) != SpellState.Ready)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 500, Color.Red);

        }
        #endregion
    }
}
