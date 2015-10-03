using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace SivirHelper
{
    internal class Program
    {
        internal static Menu Config;
        internal static bool Casted;
        internal static int Timestamp;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += eventArgs =>
            {
                if (ObjectManager.Player.ChampionName == "Sivir")
                {
                    Config = new Menu("SivirHelper", "sivirhelper", true);
                    Config.AddItem(new MenuItem("blockmove", "Spell Shield Block Movement")).SetValue(true);
                    Config.AddItem(new MenuItem("blockduration", "Spell Shield Block Duration"))
                        .SetValue(new Slider(300, 50, 500));
                    Config.AddToMainMenu();

                    Game.OnUpdate += Game_OnUpdate;
                    Obj_AI_Base.OnIssueOrder += Obj_AI_Hero_OnIssueOrder;
                    Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
                }
            };
        }

        static void Obj_AI_Hero_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (sender.IsMe && args.Order == GameObjectOrder.MoveTo)
            {
                if (Config.Item("blockmove").GetValue<bool>())
                {
                    args.Process = !Casted;
                }
            }
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Casted && Utils.GameTimeTickCount - Timestamp > 250)
            {
                if (!ObjectManager.Player.HasBuff("SivirE"))
                {
                    Casted = false;
                }

                if (Utils.GameTimeTickCount - Timestamp >= Config.Item("blockduration").GetValue<Slider>().Value)
                {
                    Casted = false;
                }
            }
        }

        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "SivirE")
            {
                if (!Casted)
                {
                    Casted = true;
                    Timestamp = Utils.GameTimeTickCount;
                }
            }
        }
    }
}
