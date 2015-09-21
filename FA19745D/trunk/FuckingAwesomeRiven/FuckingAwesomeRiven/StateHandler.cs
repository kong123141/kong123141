using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SH = FuckingAwesomeRiven.SpellHandler;
using CH = FuckingAwesomeRiven.CheckHandler;

namespace FuckingAwesomeRiven
{
    internal class StateHandler
    {
        public static Obj_AI_Hero Target;
        public static Obj_AI_Hero Player;
        public static bool CastedFlash = false;
        public static bool CastedTia;

        public static void Tick()
        {
            Target = TargetSelector.GetTarget(800, TargetSelector.DamageType.Physical);
            Player = ObjectManager.Player;
        }

        public static void Harass()
        {
            SH.Orbwalk(Target);
            if (Queuer.Queue.Count > 0 || !Target.IsValidTarget()) return;

            if (MenuHandler.GetMenuBool("HQ3AAWE") && CH.QCount == 2 && SH.Spells[SpellSlot.Q].IsReady() && SH.Spells[SpellSlot.W].IsReady() &&
                SH.Spells[SpellSlot.E].IsReady() && Target.IsValidTarget(SH.QRange))
            {
                Queuer.Add("Q");
                Queuer.Add("AA");
                Queuer.Add("W");
                return;
            }

            if (MenuHandler.GetMenuBool("HQAA3WE") && SH.Spells[SpellSlot.Q].IsReady() && SH.Spells[SpellSlot.W].IsReady() && SH.Spells[SpellSlot.E].IsReady() &&
                Target.IsValidTarget(SH.QRange))
            {
                for (int i = 0; i < 3- CH.QCount; i++)
                {
                    AddQaa();
                }
                Queuer.Add("W");
            }

            if (MenuHandler.GetMenuBool("HW") && SH.Spells[SpellSlot.W].IsReady() && Target.IsValidTarget(SH.WRange))
            {
                Queuer.Add("W");
                Queuer.Add("Hydra");
                return;
            }

            if (MenuHandler.GetMenuBool("HQ") && SH.Spells[SpellSlot.Q].IsReady() && Target.IsValidTarget(SH.QRange))
            {
                AddQaa();
                return;
            }


            if (false && MenuHandler.GetMenuBool("HE") && !SH.Spells[SpellSlot.Q].IsReady() && !SH.Spells[SpellSlot.W].IsReady())
            {
                Queuer.Add("E", Player.Position.Extend(Target.Position, -SH.Spells[SpellSlot.E].Range));
            }
        }

        public static void LastHit()
        {
            var minion = MinionManager.GetMinions(Player.Position, SH.QRange).FirstOrDefault();
            if (Queuer.Queue.Count > 0)
            {
                return;
            }

            if (minion == null)
            {
                return;
            }

            if (SH.Spells[SpellSlot.W].IsReady() && MenuHandler.GetMenuBool("WLH") && CH.CanW &&
                Environment.TickCount - CH.LastE >= 250 && minion.IsValidTarget(SH.Spells[SpellSlot.W].Range) &&
                SH.Spells[SpellSlot.W].GetDamage(minion) > minion.Health)
            {
                SH.CastW();
            }

            if (SH.Spells[SpellSlot.Q].IsReady() && MenuHandler.GetMenuBool("QLH") &&
                Environment.TickCount - CH.LastE >= 250 && (SH.Spells[SpellSlot.Q].GetDamage(minion) > minion.Health))
            {
                if (minion.IsValidTarget(SH.QRange) && CH.CanQ)
                {
                    SH.CastQ(minion);
                }
            }
        }

        public static void Laneclear()
        {
            var minion = MinionManager.GetMinions(Player.Position, SH.QRange).FirstOrDefault();
            if (!minion.IsValidTarget())
            {
                return;
            }

            if (Queuer.Queue.Count > 0)
            {
                return;
            }

            if (CH.LastTiamatCancel < Environment.TickCount)
            {
                CH.LastTiamatCancel = int.MaxValue;
                SH.CastItems(Target);
            }

            if (HealthPrediction.GetHealthPrediction(minion, (int) (ObjectManager.Player.AttackCastDelay*1000)) > 0 &&
                Player.GetAutoAttackDamage(minion) >
                HealthPrediction.GetHealthPrediction(minion, (int) (ObjectManager.Player.AttackCastDelay*1000)))
            {
                SH.Orbwalk(minion);
            }

            if (minion != null && (SH.Spells[SpellSlot.W].IsReady() && MenuHandler.GetMenuBool("WWC") && CH.CanW &&
                                   Environment.TickCount - CH.LastE >= 250 &&
                                   minion.IsValidTarget(SH.Spells[SpellSlot.W].Range) &&
                                   SH.Spells[SpellSlot.W].GetDamage(minion) > minion.Health))
            {
                SH.CastW();
            }

            if (minion != null && (SH.Spells[SpellSlot.Q].IsReady() && MenuHandler.GetMenuBool("QWC") &&
                                   Environment.TickCount - CH.LastE >= 250 &&
                                   (SH.Spells[SpellSlot.Q].GetDamage(minion) + Player.GetAutoAttackDamage(minion) >
                                    minion.Health &&
                                    MenuHandler.GetMenuBool("QWC-AA")) ||
                                   (SH.Spells[SpellSlot.Q].GetDamage(minion) > minion.Health &&
                                    MenuHandler.GetMenuBool("QWC-LH"))))
            {
                if (minion.IsValidTarget(SH.QRange) && CH.CanQ)
                {
                    SH.CastQ(minion);
                }
            }
        }

        public static void JungleFarm()
        {
            var minion =
                MinionManager.GetMinions(
                    Player.Position, 600, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth)
                    .FirstOrDefault();
            if (!minion.IsValidTarget())
            {
                return;
            }

            if (Queuer.Queue.Count > 0)
            {
                return;
            }

            if (CH.LastTiamatCancel < Environment.TickCount)
            {
                CH.LastTiamatCancel = int.MaxValue;
                SH.CastItems(Target);
            }

            SH.Orbwalk(minion);
            if (SH.Spells[SpellSlot.E].IsReady() && CH.CanE && MenuHandler.GetMenuBool("EJ"))
            {
                if (minion.IsValidTarget(SH.Spells[SpellSlot.E].Range))
                {
                    if (minion != null)
                    {
                        SH.Spells[SpellSlot.E].Cast(minion.Position);
                    }
                }
            }

            if (SH.Spells[SpellSlot.W].IsReady() && CH.CanW && Environment.TickCount - CH.LastE >= 250 &&
                minion.IsValidTarget(SH.Spells[SpellSlot.W].Range) && MenuHandler.GetMenuBool("WJ"))
            {
                SH.CastW();
            }

            SH.CastItems(minion);
            if (SH.Spells[SpellSlot.Q].IsReady() && Environment.TickCount - CH.LastE >= 250 &&
                MenuHandler.GetMenuBool("QJ"))
            {
                if (minion.IsValidTarget(SH.QRange) && CH.CanMove)
                {
                    SH.CastQ(minion);
                }
            }
        }

        private static void AddQaa(bool qFirst = false)
        {
            var qAa = MenuHandler.Config.Item("QAA").GetValue<StringList>().SelectedIndex == 1;
            if (qFirst)
            {
                Queuer.Add("Q");
            }

            if (qAa)
            {
                Queuer.Add("AA");
                Queuer.Add("Q");
                return;
            }

            Queuer.Add("Q");
            Queuer.Add("AA");
        }

        public static void MainCombo()
        {
            SH.Orbwalk(Target);
            if (Queuer.Queue.Count > 0)
            {
                return;
            }

            if (!Target.IsValidTarget())
            {
                return;
            }

            var comboRDmg = DamageHandler.GetComboDmg(true, Target);
            var comboNoR = DamageHandler.GetComboDmg(false, Target);
            if (SH.Spells[SpellSlot.R].IsReady() &&
                !CH.RState && MenuHandler.GetMenuBool("CR") &&
                (MenuHandler.Config.Item("forcedR").GetValue<KeyBind>().Active ||
                 comboNoR < Target.Health && comboRDmg > Target.Health))
            {
                if (MenuHandler.GetMenuBool("CREWHQ") && SH.Spells[SpellSlot.Q].IsReady() &&
                    SH.Spells[SpellSlot.W].IsReady() && SH.Spells[SpellSlot.E].IsReady() &&
                    Target.IsValidTarget(325 + SpellHandler.QRange))
                {
                    Queuer.Add("R");
                    Queuer.Add("E", Target.Position);
                    Queuer.Add("AA");
                    Queuer.Add("W");
                    Queuer.Add("Hydra");
                    Queuer.Add("AA");
                    AddQaa(true);
                    return;
                }

                if (MenuHandler.GetMenuBool("CREWH") && SH.Spells[SpellSlot.E].IsReady() &&
                    SH.Spells[SpellSlot.W].IsReady() && Target.IsValidTarget(325 + SpellHandler.WRange))
                {
                    Queuer.Add("R");
                    Queuer.Add("E", Target.Position);
                    Queuer.Add("AA");
                    Queuer.Add("W");
                    Queuer.Add("Hydra");
                    return;
                }

                if (MenuHandler.GetMenuBool("CREAAHQ") && SH.Spells[SpellSlot.Q].IsReady() &&
                    SH.Spells[SpellSlot.E].IsReady() && Target.IsValidTarget(325 + SpellHandler.QRange))
                {
                    Queuer.Add("R");
                    Queuer.Add("E", Target.Position);
                    Queuer.Add("AA");
                    Queuer.Add("Hydra");
                    AddQaa(true);
                    return;
                }

                if (MenuHandler.GetMenuBool("CRWAAHQ") && SH.Spells[SpellSlot.Q].IsReady() &&
                    SH.Spells[SpellSlot.W].IsReady() && Target.IsValidTarget(SpellHandler.QRange))
                {
                    Queuer.Add("R");
                    Queuer.Add("W");
                    Queuer.Add("AA");
                    Queuer.Add("Hydra");
                    AddQaa(true);
                    return;
                }

                if (MenuHandler.GetMenuBool("CR1CC") && SH.Spells[SpellSlot.R].IsReady())
                {
                    Queuer.Add("R");
                    return;
                }
            }

            if (MenuHandler.GetMenuBool("CR2") && SH.Spells[SpellSlot.R].IsReady() && CH.RState)
            {
                if (MenuHandler.GetMenuBool("CR2WQ") && SH.Spells[SpellSlot.Q].IsReady() &&
                    SH.Spells[SpellSlot.W].IsReady() &&
                    SpellHandler.Spells[SpellSlot.R].GetDamage(Target) +
                    SpellHandler.Spells[SpellSlot.W].GetDamage(Target) +
                    SpellHandler.Spells[SpellSlot.Q].GetDamage(Target) > Target.Health)
                {
                    Queuer.Add("R2", Target);
                    Queuer.Add("W");
                    AddQaa(true);
                    return;
                }

                if (MenuHandler.GetMenuBool("CR2W") && SH.Spells[SpellSlot.W].IsReady() &&
                    SpellHandler.Spells[SpellSlot.R].GetDamage(Target) +
                    SpellHandler.Spells[SpellSlot.W].GetDamage(Target) > Target.Health)
                {
                    Queuer.Add("R2", Target);
                    Queuer.Add("W");
                    return;
                }

                if (MenuHandler.GetMenuBool("CR2Q") && SH.Spells[SpellSlot.Q].IsReady() &&
                    SpellHandler.Spells[SpellSlot.R].GetDamage(Target) +
                    SpellHandler.Spells[SpellSlot.Q].GetDamage(Target) > Target.Health)
                {
                    Queuer.Add("R2", Target);
                    AddQaa(true);
                    return;
                }

                if (MenuHandler.GetMenuBool("CR2CC") &&
                    SpellHandler.Spells[SpellSlot.R].GetDamage(Target) > Target.Health)
                {
                    Queuer.Add("R2", Target);
                    return;
                }
            }

            // skills based on cds / engages
            if (MenuHandler.GetMenuBool("CEWHQ") && SH.Spells[SpellSlot.Q].IsReady() &&
                SH.Spells[SpellSlot.W].IsReady() && SH.Spells[SpellSlot.E].IsReady() &&
                Target.IsValidTarget(325 + SpellHandler.QRange))
            {
                Queuer.Add("E", Target.Position);
                Queuer.Add("W");
                Queuer.Add("Hydra");
                Queuer.Add("AA");
                AddQaa(true);
                return;
            }

            if (MenuHandler.GetMenuBool("CQWH") && SH.Spells[SpellSlot.Q].IsReady() &&
                SH.Spells[SpellSlot.W].IsReady() && CH.QCount == 2 && Target.IsValidTarget(SpellHandler.QRange))
            {
                Queuer.Add("Q");
                Queuer.Add("W");
                Queuer.Add("Hydra");
                Queuer.Add("AA");
                AddQaa(true);
                return;
            }

            if (MenuHandler.GetMenuBool("CEHQ") && SH.Spells[SpellSlot.Q].IsReady() &&
                SH.Spells[SpellSlot.E].IsReady() && Target.IsValidTarget(SH.QRange + 325))
            {
                Queuer.Add("E", Target.Position);
                Queuer.Add("Hydra");
                Queuer.Add("Q");
                Queuer.Add("AA");
                AddQaa(true);
                return;
            }

            if (MenuHandler.GetMenuBool("CEW") && SH.Spells[SpellSlot.E].IsReady() &&
                Target.IsValidTarget(325 + SH.WRange) && !Orbwalking.InAutoAttackRange(Target))
            {
                Queuer.Add("E", Target.Position);
                Queuer.Add("W");
                Queuer.Add("AA");
                return;
            }
            // End

            // When only one skill is up
            if (MenuHandler.GetMenuBool("CW") && SH.Spells[SpellSlot.W].IsReady() && Target.IsValidTarget(SH.WRange))
            {
                Queuer.Add("W");
                Queuer.Add("Hydra");
                return;
            }

            if (MenuHandler.GetMenuBool("CE") && SH.Spells[SpellSlot.E].IsReady() &&
                Target.IsValidTarget(325 + Orbwalking.GetRealAutoAttackRange(Player)) &&
                !Orbwalking.InAutoAttackRange(Target))
            {
                Queuer.Add("E", Target.Position);
                return;
            }

            if (MenuHandler.GetMenuBool("CQ") && SH.Spells[SpellSlot.Q].IsReady() && Target.IsValidTarget(SH.QRange))
            {
                AddQaa();
            }
            // End
        }

        public static void BurstCombo()
        {
            SH.Orbwalk(Target);
            if (!Target.IsValidTarget())
            {
                return;
            }

            if (Queuer.Queue.Count > 0)
            {
                return;
            }

            if (MenuHandler.GetMenuBool("flashlessBurst") && Target.IsValidTarget(325 + SH.WRange) &&
                SH.Spells[SpellSlot.Q].IsReady() && SH.Spells[SpellSlot.W].IsReady() &&
                SH.Spells[SpellSlot.E].IsReady() && SH.Spells[SpellSlot.R].IsReady() && Queuer.Queue.Count == 0)
            {
                Queuer.Add("E", Target.Position);
                Queuer.Add("R");
                Queuer.Add("Q");
                Queuer.Add("W");
                Queuer.Add("Hydra");
                Queuer.Add("AA");
                Queuer.Add("Q");
                Queuer.Add("R2", Target);
                return;
            }

            // Kyzer 3rd Q Combo
            if (MenuHandler.GetMenuBool("kyzerCombo") && Target.IsValidTarget(400 + 325 + (SH.WRange/2)) &&
                SH.Spells[SpellSlot.Q].IsReady() && SH.Spells[SpellSlot.W].IsReady() &&
                SH.Spells[SpellSlot.E].IsReady() && SH.Spells[SpellSlot.R].IsReady() && CH.QCount == 2 &&
                Queuer.Queue.Count == 0)
            {
                Queuer.Add("E", Target.Position);
                Queuer.Add("R");
                Queuer.Add("Flash", Target.Position, true);
                Queuer.Add("Q");
                Queuer.Add("AA");
                Queuer.Add("Hydra");
                Queuer.Add("W");
                Queuer.Add("AA");
                Queuer.Add("R2", Target);
                Queuer.Add("Q");
                return;
            }

            // Shy Combo
            if (MenuHandler.GetMenuBool("shyCombo") && Target.IsValidTarget(400 + 325 + (SH.WRange/2)) &&
                SH.Spells[SpellSlot.Q].IsReady() && SH.Spells[SpellSlot.W].IsReady() &&
                SH.Spells[SpellSlot.E].IsReady() && SH.Spells[SpellSlot.R].IsReady() && Queuer.Queue.Count == 0)
            {
                Queuer.Add("E", Target.Position);
                Queuer.Add("R");
                Queuer.Add("Flash", Target.Position, true);
                Queuer.Add("AA");
                Queuer.Add("Hydra");
                Queuer.Add("W");
                Queuer.Add("R2", Target);
                Queuer.Add("Q");
                Queuer.Add("AA");
                return;
            }

            MainCombo();
        }

        public static void Flee()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            var random = new Random().Next(500);
            if (SpellHandler.LastMove + 200 + random < Environment.TickCount && MenuHandler.Config.Item("streamMouse").GetValue<bool>())
            {
                SpellHandler.LastMove = Environment.TickCount;
                SmoothMouse.doMouseClick();
            }

            if (SH.Spells[SpellSlot.E].IsReady() && CH.LastQ + 250 < Environment.TickCount &&
                MenuHandler.GetMenuBool("EFlee"))
            {
                SH.CastE(Game.CursorPos);
            }

            if ((!SH.Spells[SpellSlot.Q].IsReady() || CH.LastE + 250 >= Environment.TickCount ||
                 !MenuHandler.GetMenuBool("QFlee")))
            {
                return;
            }

            if ((MenuHandler.Config.Item("Ward Mechanic").GetValue<bool>() && CheckHandler.QCount == 2))
            {
                return;
            }

            SH.CastQ();
        }
    }
}