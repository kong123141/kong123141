using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SH = FuckingAwesomeRiven.SpellHandler;

namespace FuckingAwesomeRiven
{
    internal class CheckHandler
    {
        public static Obj_AI_Base Player { get { return ObjectManager.Player; } }
        public static int LastQ,
            LastQ2,
            LastW,
            LastE,
            LastAa,
            LastPassive,
            LastFr,
            LastTiamat,
            LastR2,
            LastECancelSpell,
            LastTiamatCancel;

        public static bool CanQ, CanW, CanE, CanR, CanAa, CanMove, CanSr, MidQ, MidW, MidE, MidAa, RState, BurstFinished;
        public static int PassiveStacks, QCount, FullComboState;

        public static void Init()
        {
            CanAa = true;
            CanMove = true;
            CanQ = true;
            CanW = true;
            CanE = true;
            CanR = true;
            RState = false;

            LastQ = Environment.TickCount;
            LastQ2 = Environment.TickCount;
            LastW = Environment.TickCount;
            LastE = Environment.TickCount;
            LastAa = Environment.TickCount;
            LastPassive = Environment.TickCount;
            LastFr = Environment.TickCount;
        }

        public static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spell = args.SData;
            if (!sender.IsMe)
            {
                return;
            }

            if (spell.Name == "ItemTiamatCleave")
            {
                LastTiamat = Environment.TickCount;
            }

            if (!MidQ && spell.Name.Contains("RivenBasicAttack"))
            {
                LastAa = Environment.TickCount;
                LastTiamatCancel = Environment.TickCount + (int) ObjectManager.Player.AttackCastDelay;
                LastPassive = Environment.TickCount;
                if (PassiveStacks >= 1)
                {
                    PassiveStacks = PassiveStacks - 1;
                }

                MidAa = true;
                CanMove = false;
                CanAa = false;
                SmoothMouse.addMouseEvent(args.Target.Position, true);
            }

            if (spell.Name.Contains("RivenTriCleave"))
            {
                Queuer.Remove("Q");
                LastQ = Environment.TickCount;
                LastPassive = Environment.TickCount;
                LastECancelSpell = Environment.TickCount + 50;
                if (PassiveStacks <= 2)
                {
                    PassiveStacks = PassiveStacks + 1;
                }

                if (QCount <= 1)
                {
                    LastQ2 = Environment.TickCount;
                    QCount = QCount + 1;
                }
                else if (QCount == 2)
                {
                    QCount = 0;
                }

                Utility.DelayAction.Add(350, Orbwalking.ResetAutoAttackTimer);
                Utility.DelayAction.Add(40, () => SH.AnimCancel(StateHandler.Target));

                MidQ = true;
                CanMove = false;
                CanQ = false;
                FullComboState = 0;
                BurstFinished = true;
            }

            if (spell.Name.Contains("RivenMartyr"))
            {
                Queuer.Remove("W");
                Utility.DelayAction.Add(40, () => SH.AnimCancel(StateHandler.Target));
                LastW = Environment.TickCount;
                LastPassive = Environment.TickCount;
                LastECancelSpell = Environment.TickCount + 50;
                LastTiamatCancel = Environment.TickCount + (int) ObjectManager.Player.AttackCastDelay;
                if (LastPassive <= 2)
                {
                    PassiveStacks = PassiveStacks + 1;
                }

                MidW = true;
                CanW = false;
                FullComboState = 2;
            }

            if (spell.Name.Contains("RivenFeint"))
            {
                Queuer.Remove("E");
                Queuer.EPos = new Vector3();
                LastE = Environment.TickCount;
                PassiveStacks = Environment.TickCount;
                LastTiamatCancel = Environment.TickCount + 50;
                if (LastPassive <= 2)
                {
                    PassiveStacks = PassiveStacks + 1;
                }

                MidE = true;
                CanE = false;
            }

            if (spell.Name.Contains("RivenFengShuiEngine"))
            {
                RState = true;
                Queuer.Remove("R");
                LastFr = Environment.TickCount;
                LastPassive = Environment.TickCount;
                LastECancelSpell = Environment.TickCount + 50;
                if (PassiveStacks <= 2)
                {
                    PassiveStacks = PassiveStacks + 1;
                }

                FullComboState = 1;
            }

            if (spell.Name.Contains("rivenizunablade"))
            {
                RState = false;
                Queuer.Remove("R2");
                Queuer.R2Target = null;
                LastPassive = Environment.TickCount;
                if (PassiveStacks <= 2)
                {
                    PassiveStacks = PassiveStacks + 1;
                }

                LastR2 = Environment.TickCount;
                CanSr = false;
                FullComboState = 3;
            }
        }

        public static void Checks()
        {
            if (MidQ && LastQ + 50 < Environment.TickCount)
            {
                MidQ = false;
                CanMove = true;
                CanAa = true;
            }

            if (MidW && Environment.TickCount - LastW >= 266.7)
            {
                MidW = false;
                CanMove = true;
            }

            if (MidE && Environment.TickCount - LastE >= 500)
            {
                MidE = false;
                CanMove = true;
            }

            if (PassiveStacks != 0 && Environment.TickCount - LastPassive >= 5000)
            {
                PassiveStacks = 0;
            }

            if (QCount != 0 && Environment.TickCount - LastQ >= 4000)
            {
                QCount = 0;
            }

            if (!CanW && !(MidAa || MidQ || MidE) && SH.Spells[SpellSlot.W].IsReady())
            {
                CanW = true;
            }

            if (!CanE && !(MidAa || MidQ || MidW) && SH.Spells[SpellSlot.E].IsReady())
            {
                CanE = true;
            }

            if (RState && Environment.TickCount - LastFr >= 15000)
            {
                RState = false;
            }

            if (MidAa && !Player.IsWindingUp && Environment.TickCount > LastAa + (Player.AttackCastDelay * 1000) + (Game.Ping * 0.5) + MenuHandler.Config.Item("bonusCancelDelay").GetValue<Slider>().Value)
            {
                CanMove = true;
                CanQ = true;
                CanW = true;
                CanE = true;
                CanSr = true;
                MidAa = false;
            }

            if (!CanMove && !Player.IsWindingUp && Environment.TickCount > LastAa + Game.Ping + 80 + MenuHandler.Config.Item("bonusCancelDelay").GetValue<Slider>().Value)
            {
                CanMove = true;
            }

            if (!CanAa &&
                Environment.TickCount + Game.Ping / 2 + 25 >= LastAa + ObjectManager.Player.AttackDelay * 1000)
            {
                CanAa = true;
            }
        }
    }
}