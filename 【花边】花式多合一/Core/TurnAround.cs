using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace 花边_花式多合一.Core
{
    class TurnAround
    {
        public enum MovementDirection
        {
            Forward = 1,
            Backward = 2
        }

        public static readonly List<ChampionInfo> ExistingChampions = new List<ChampionInfo>();

        public static void AddChampions()
        {
            ExistingChampions.Add(
                new ChampionInfo
                {
                    CharName = "Cassiopeia",
                    Slot = SpellSlot.R,
                    Range = 1000,
                    SpellName = "Petrifying Gaze (R)",
                    Movement = MovementDirection.Backward,
                    CastTime = 1.5f
                });

            ExistingChampions.Add(
                new ChampionInfo
                {
                    CharName = "Tryndamere",
                    Slot = SpellSlot.W,
                    Range = 900,
                    SpellName = "Mocking Shout (W)",
                    Movement = MovementDirection.Forward,
                    CastTime = 0.65f
                });
        }

        public static int MoveTo(MovementDirection direction)
        {
            switch (direction)
            {
                case MovementDirection.Forward:
                    return 100;
                case MovementDirection.Backward:
                    return -100;
                default:
                    throw new ArgumentOutOfRangeException("direction");
            }
        }

        internal class Load
        {
            public Load()
            {
                try
                {
                    if (!InitializeMenu.Menu.Item("AutoTurnAround").GetValue<bool>()) return;
                    AddChampions();
                    Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("TrunAround error occurred: '{0}'", ex);
                }
            }
        }

        public class ChampionInfo
        {
            public string CharName { get; set; }
            public SpellSlot Slot { get; set; }
            public float Range { get; set; }
            public string SpellName { get; set; }
            public MovementDirection Movement { get; set; }
            public float CastTime { get; set; }
        }
        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!InitializeMenu.Menu.Item("AutoTurnAround").GetValue<bool>() || !Huabian.Player.IsTargetable || (sender == null || sender.Team == Huabian.Player.Team))
            {
                return;
            }

            foreach (var champ in ExistingChampions)
            {
                if ((InitializeMenu.Menu.SubMenu("躲避技能").SubMenu(champ.CharName) == null) ||
                    (InitializeMenu.Menu.SubMenu("躲避技能").SubMenu(champ.CharName).Item(champ.Slot.ToString()) == null) ||
                    (!InitializeMenu.Menu.SubMenu("躲避技能")
                        .SubMenu(champ.CharName)
                        .Item(champ.Slot.ToString())
                        .GetValue<bool>()))
                {
                    continue;
                }

                if (champ.Slot != (sender as Obj_AI_Hero).GetSpellSlot(args.SData.Name) ||
                    (!(Huabian.Player.Distance(sender.Position) <= champ.Range) && args.Target != Huabian.Player))
                {
                    continue;
                }

                var vector =
                    new Vector3(
                        Huabian.Player.Position.X +
                        ((sender.Position.X - Huabian.Player.Position.X) * (MoveTo(champ.Movement)) /
                         Huabian.Player.Distance(sender.Position)),
                        Huabian.Player.Position.Y +
                        ((sender.Position.Y - Huabian.Player.Position.Y) * (MoveTo(champ.Movement)) /
                         Huabian.Player.Distance(sender.Position)), 0);
                Huabian.Player.IssueOrder(GameObjectOrder.MoveTo, vector);
                Orbwalking.Move = false;
                Utility.DelayAction.Add((int)(champ.CastTime + 0.1) * 1000, () => Orbwalking.Move = true);
            }
        }
    }
}
