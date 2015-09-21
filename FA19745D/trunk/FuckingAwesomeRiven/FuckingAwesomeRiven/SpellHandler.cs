using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;
using CH = FuckingAwesomeRiven.CheckHandler;

namespace FuckingAwesomeRiven
{
    public static class SpellHandler
    {
        public enum SummonerSpell
        {
            Flash,
            Ignite,
            Smite
        }

        public static Dictionary<SpellSlot, Spell> Spells = new Dictionary<SpellSlot, Spell>
        {
            {SpellSlot.Q, new Spell(SpellSlot.Q, 300)},
            {SpellSlot.W, new Spell(SpellSlot.W, 250)},
            {SpellSlot.E, new Spell(SpellSlot.E, 325)},
            {SpellSlot.R, new Spell(SpellSlot.R, 900)}
        };

        public static Dictionary<SummonerSpell, SpellSlot> SummonerDictionary =
            new Dictionary<SummonerSpell, SpellSlot>
            {
                {SummonerSpell.Flash, Player.GetSpellSlot("SummonerFlash")},
                {SummonerSpell.Ignite, Player.GetSpellSlot("SummonerDot")}
            };

        public static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static int QRange
        {
            get { return CheckHandler.RState ? 325 : 300; }
        }

        public static int WRange
        {
            get { return CheckHandler.RState ? 270 : 250; }
        }

        public static void CastQ(Obj_AI_Base target = null)
        {
            if (!Spells[SpellSlot.Q].IsReady())
            {
                return;
            }
            if (target != null)
            SmoothMouse.addMouseEvent(target.Position);
            Spells[SpellSlot.Q].Cast(target != null ? target.Position : Game.CursorPos, true);
        }

        public static void CastW(Obj_AI_Hero target = null)
        {
            if (!Spells[SpellSlot.W].IsReady())
            {
                return;
            }

            if (target.IsValidTarget(WRange))
            {
                CastItems(target);
                Spells[SpellSlot.W].Cast();
            }

            if (target == null)
            {
                Spells[SpellSlot.W].Cast();
            }
        }

        public static void CastE(Vector3 pos)
        {
            if (!Spells[SpellSlot.E].IsReady())
            {
                return;
            }

            SmoothMouse.addMouseEvent(pos);

            Spells[SpellSlot.E].Cast(pos);
        }

        public static void CastR()
        {
            Spells[SpellSlot.R].Cast();
        }

        public static void CastR2(Obj_AI_Base target)
        {
            var r2 = new Spell(SpellSlot.R, 900);
            r2.SetSkillshot(0.25f, 45, 1200, false, SkillshotType.SkillshotCone);
            SmoothMouse.addMouseEvent(target.Position);
            r2.Cast(target);
        }

        public static void CastFlash(Vector3 pos)
        {
            if (!SummonerDictionary[SummonerSpell.Flash].IsReady())
            {
                return;
            }
            SmoothMouse.addMouseEvent(pos);
            Player.Spellbook.CastSpell(SummonerDictionary[SummonerSpell.Flash], pos);
        }

        public static void CastIgnite(Obj_AI_Hero target)
        {
            if (!SummonerDictionary[SummonerSpell.Ignite].IsReady())
            {
                return;
            }

            SmoothMouse.addMouseEvent(target.Position);
            Player.Spellbook.CastSpell(SummonerDictionary[SummonerSpell.Ignite], target);
        }

        public static void CastItems(Obj_AI_Base target)
        {
            if (!target.IsValidTarget())
            {
                return;
            }

            if (!target.IsValid<Obj_AI_Hero>() && target.IsValidTarget(300))
            {
                if (LeagueSharp.Common.Data.ItemData.Tiamat_Melee_Only.GetItem().IsReady())
                {
                    LeagueSharp.Common.Data.ItemData.Tiamat_Melee_Only.GetItem().Cast();
                }

                if (LeagueSharp.Common.Data.ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
                {
                    LeagueSharp.Common.Data.ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
                }
            }
            else
            {
                if (target.IsValidTarget(300))
                {
                    if (LeagueSharp.Common.Data.ItemData.Tiamat_Melee_Only.GetItem().IsReady())
                    {
                        LeagueSharp.Common.Data.ItemData.Tiamat_Melee_Only.GetItem().Cast();
                    }

                    if (LeagueSharp.Common.Data.ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
                    {
                        LeagueSharp.Common.Data.ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
                    }
                }

                if (LeagueSharp.Common.Data.ItemData.Youmuus_Ghostblade.GetItem().IsReady())
                {
                    LeagueSharp.Common.Data.ItemData.Youmuus_Ghostblade.GetItem().Cast();
                }
            }
        }

        public static void AnimCancel(Obj_AI_Base target)
        {
            if (!CheckHandler.CanMove || MenuHandler.Config.Item("flee").GetValue<KeyBind>().Active ||
                MenuHandler.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)
            {
                return;
            }

            var pos2 = Player.Position.Extend(Game.CursorPos, 400);
            if (target.IsValidTarget())
            {
                pos2 = Player.Position.Extend(target.ServerPosition, 540);
                SmoothMouse.addMouseEvent(pos2, true);
            }

            
            Player.IssueOrder(GameObjectOrder.MoveTo, pos2);
        }

        public static int LastMove;

        public static void Orbwalk(Obj_AI_Base target = null)
        {
            if (MenuHandler.Config.Item("normalCombo").GetValue<KeyBind>().Active)
            {
                MenuHandler.Orbwalker.SetAttack(false);
                MenuHandler.Orbwalker.SetMovement(false);
            }
            else
            {
                MenuHandler.Orbwalker.SetMovement(true);
                MenuHandler.Orbwalker.SetAttack(true);
            }

            if (CH.CanMove)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                MenuHandler.Orbwalker.SetMovement(true); 
                var random = new Random().Next(500);
                if (LastMove + 200 + random < Environment.TickCount && MenuHandler.Config.Item("streamMouse").GetValue<bool>())
                {
                    LastMove = Environment.TickCount;
                }
            }

            if (!target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) || !CH.CanAa)
            {
                return;
            }

            MenuHandler.Orbwalker.SetAttack(true);
            CH.CanMove = false;
            Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            CH.CanQ = false;
            CH.CanW = false;
            CH.CanE = false;
            CH.CanSr = false;
            CH.LastAa = Environment.TickCount;
        }
    }
}
