﻿using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace CCChainer
{
    class SpellLogic
    {
        private static Obj_AI_Hero Player = Program.Player;

        public static void JannaQ(Vector3 castpos)
        {
            var Q = Program.Q;
            Q.Cast(castpos);
            Q.Cast();
        }


        public static void CastSpellSelfAuto(Spell spell, Obj_AI_Hero target)
        {
            if (Player.Distance(target) <= Player.AttackRange)
            {
                Orbwalking.ResetAutoAttackTimer();
                spell.CastOnUnit(ObjectManager.Player);
                Program.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }

        }

        private static void CastSpellSSAuto(Spell spell, Vector3 predpos, Obj_AI_Hero target)
        {
            Orbwalking.ResetAutoAttackTimer();
            spell.Cast(predpos);
            if (Player.Distance(target) <= Player.AttackRange)
            {
                Program.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
        }

   

    }
}
