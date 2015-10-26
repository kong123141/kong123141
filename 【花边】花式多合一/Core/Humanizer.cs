using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace 花边_花式多合一.Core
{
    internal static class Humanizer
    {
        public static float LastMove;
        public static List<string> SpellList = new List<string> { "Q", "W", "E", "R" };
        public static List<float> LastCast = new List<float> { 0, 0, 0, 0 };


        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            var spell = (int)args.Slot;
            var senderValid = sender != null && sender.Owner != null && sender.Owner.IsMe;

            if (!senderValid || !args.Slot.IsMainSpell() || !InitializeMenu.Menu.Item("Enabled" + spell, true).IsActive())
            {
                return;
            }

            var min = InitializeMenu.Menu.Item("MinDelay" + spell, true).GetValue<Slider>().Value;
            var max = InitializeMenu.Menu.Item("MaxDelay" + spell, true).GetValue<Slider>().Value;
            var delay = min > max ? min : WeightedRandom.Next(min, max);

            if (Utils.TickCount - LastCast[spell] < delay)
            {
                args.Process = false;
                return;
            }

            LastCast[spell] = Utils.TickCount;
        }

        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if (!InitializeMenu.Menu.Item("Humanizer").GetValue<bool>()) return;
                Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
                Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Humanizer error occurred: '{0}'", ex);
            }
        }

        private static void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            var senderValid = sender != null && sender.IsValid && sender.IsMe;

            if (!senderValid || args.Order != GameObjectOrder.MoveTo || !InitializeMenu.Menu.Item("MovementEnabled").IsActive())
            {
                return;
            }
            var min = InitializeMenu.Menu.Item("MinDelay").GetValue<Slider>().Value;
            var max = InitializeMenu.Menu.Item("MaxDelay").GetValue<Slider>().Value;
            var delay = min > max ? min : WeightedRandom.Next(min, max);

            if (Utils.TickCount - LastMove < delay)
            {
                args.Process = false;
                return;
            }

            LastMove = Utils.TickCount;
        }

        public static bool IsMainSpell(this SpellSlot slot)
        {
            return slot == SpellSlot.Q || slot == SpellSlot.W || slot == SpellSlot.E || slot == SpellSlot.R;
        }
    }
}