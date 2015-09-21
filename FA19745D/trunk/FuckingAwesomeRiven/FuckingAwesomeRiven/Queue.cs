using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;
using C = FuckingAwesomeRiven.CheckHandler;
using S = FuckingAwesomeRiven.SpellHandler;

namespace FuckingAwesomeRiven
{
    internal class Queuer
    {
        public static List<String> Queue = new List<string>();
        public static Obj_AI_Base R2Target;
        public static Vector3 EPos;
        public static Vector3 FlashPos;
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private static readonly int QDelay = (int)(ObjectManager.Player.AttackCastDelay * 1000 - (Game.Ping * 0.5));

        public static void DoQueue()
        {
            if (Queue.Count == 0)
            {
                return;
            }

            switch (Queue[0])
            {
                case "AA":
                    Aa();
                    break;
                case "Q":
                    Qq();
                    break;
                case "W":
                    Qw();
                    break;
                case "E":
                    Qe();
                    break;
                case "R":
                    Qr();
                    break;
                case "R2":
                    Qr2();
                    break;
                case "Flash":
                    Flash();
                    break;
                case "Hydra":
                    Hydra();
                    break;
            }
        }

        public static void Add(String spell)
        {
            Queue.Add(spell);
        }

        public static void Add(String spell, Obj_AI_Base target)
        {
            Queue.Add(spell);
            R2Target = target;
        }

        public static void Add(String spell, Vector3 pos, bool isFlash = false)
        {
            Queue.Add(spell);
            if (isFlash)
            {
                FlashPos = pos;
                return;
            }

            EPos = pos;
        }

        public static void Remove(String spell)
        {
            if (Queue.Count == 0 || Queue[0] != spell)
            {
                return;
            }

            Queue.RemoveAt(0);
        }

        private static void Aa()
        {
            if (StateHandler.Target == null || StateHandler.Target.Distance(Player) > Orbwalking.GetRealAutoAttackRange(Player) + 100 || CheckHandler.MidAa)
            {
                Remove("AA");
                return;
            }
            Player.IssueOrder(GameObjectOrder.AttackUnit, StateHandler.Target);
            SmoothMouse.addMouseEvent(Game.CursorPos, true);
        }

        private static void Qq()
        {
            if (!CheckHandler.MidAa)
            {
                return;
            }

            if (!S.Spells[SpellSlot.Q].IsReady())
            {
                Queue.Remove("Q");
                return;
            }

            if (S.Spells[SpellSlot.Q].IsReady())
            {

                S.CastQ(StateHandler.Target);
                SmoothMouse.addMouseEvent(Player.Position.Extend(Game.CursorPos,300));
            }
        }

        private static void Qw()
        {
            if (!CheckHandler.MidAa)
            {
                return;
            }

            if (!S.Spells[SpellSlot.W].IsReady())
            {
                Queue.Remove("W");
                return;
            }

            if (S.Spells[SpellSlot.W].IsReady())
            {
                S.CastW(StateHandler.Target);
                SmoothMouse.addMouseEvent(Game.CursorPos);
            }
        }

        private static void Qe()
        {
            if (!CheckHandler.MidAa)
            {
                return;
            }

            if (!S.Spells[SpellSlot.E].IsReady() || !EPos.IsValid())
            {
                Queue.Remove("E");
                return;
            }

            if (S.Spells[SpellSlot.E].IsReady())
            {
                S.CastE(StateHandler.Target.IsValidTarget() ? StateHandler.Target.Position : EPos);
                SmoothMouse.addMouseEvent(StateHandler.Target.IsValidTarget() ? StateHandler.Target.Position : EPos, false);
            }
        }

        private static void Qr()
        {
            if (!S.Spells[SpellSlot.Q].IsReady() || C.RState)
            {
                Queue.Remove("R");
                return;
            }

            if (S.Spells[SpellSlot.R].IsReady())
            {
                S.CastR();
                SmoothMouse.addMouseEvent(Player.Position.Extend(Game.CursorPos,300), false);
            }
        }

        private static void Qr2()
        {
            if (!S.Spells[SpellSlot.R].IsReady() || R2Target == null)
            {
                Queue.Remove("R2");
                return;
            }

            if (!S.Spells[SpellSlot.R].IsReady() || !C.RState || !R2Target.IsValidTarget())
            {
                return;
            }

            var r2 = new Spell(SpellSlot.R, 900);
            r2.SetSkillshot(0.25f, 45, 1200, false, SkillshotType.SkillshotCone);
            r2.Cast(R2Target);

            SmoothMouse.addMouseEvent(R2Target.Position, false);
        }

        private static void Hydra()
        {
            if (Player.IsWindingUp || !CheckHandler.MidAa)
            {
                return;
            }

            if (!LeagueSharp.Common.Data.ItemData.Tiamat_Melee_Only.GetItem().IsReady() &&
                !LeagueSharp.Common.Data.ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
            {
                Queue.Remove("Hydra");
                return;
            }

            if (!LeagueSharp.Common.Data.ItemData.Tiamat_Melee_Only.GetItem().IsReady() &&
                !LeagueSharp.Common.Data.ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
            {
                return;
            }

            LeagueSharp.Common.Data.ItemData.Tiamat_Melee_Only.GetItem().Cast();
            LeagueSharp.Common.Data.ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
        }

        private static void Flash()
        {
            if (!S.SummonerDictionary[SpellHandler.SummonerSpell.Flash].IsReady() || !FlashPos.IsValid())
            {
                Queue.Remove("Flash");
                return;
            }
            SmoothMouse.addMouseEvent(FlashPos);
            SpellHandler.CastFlash(FlashPos);
        }
    }
}
