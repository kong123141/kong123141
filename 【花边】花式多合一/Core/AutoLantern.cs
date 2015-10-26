using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace 花边_花式多合一.Core
{
    internal class AutoLantern
    {
        public static SpellSlot LanternSlot = (SpellSlot)62;
        public static int LastLantern;

        public static SpellDataInst LanternSpell
        {
            get { return Huabian.Player.Spellbook.GetSpell(LanternSlot); }
        }
        private static bool ThreshInGame()
        {
            return HeroManager.Allies.Any(h => !h.IsMe && h.ChampionName.Equals("Thresh"));
        }


        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if (!ThreshInGame())
                {
                    return;
                }
                if (!InitializeMenu.Menu.Item("AutoLanter").GetValue<bool>()) return;

                if (InitializeMenu.Menu.Item("PermaShowLantern").IsActive())
                {
                    var active = IsLanternSpellActive();
                    InitializeMenu.Menu.Item("LanternReady")
                        .Permashow(
                            true, active ? "鑷姩鐏:鍑嗗" : "鑷姩鐏:鐐瑰嚮",
                            active ? Color.Green : Color.Red);
                }

                InitializeMenu.Menu.Item("PermaShowLantern").ValueChanged += (sender, eventArgs) =>
                {
                    var lanternActive = IsLanternSpellActive();
                    InitializeMenu.Menu.Item("LanternReady")
                        .Permashow(
                            eventArgs.GetNewValue<bool>(),
                            lanternActive ? "鑷姩鐏:鍑嗗" : "鑷姩鐏:鐐瑰嚮",
                            lanternActive ? Color.Green : Color.Red);
                };

                Game.OnUpdate += OnGameUpdate;
                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

            }
            catch (Exception ex)
            {
                Console.WriteLine("AutoLantern error occurred: '{0}'", ex);
            }
        }

        private static bool IsLanternSpellActive()
        {
            return LanternSpell != null && LanternSpell.Name.Equals("LanternWAlly");
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender is Obj_AI_Hero && sender.IsAlly && args.SData.Name.Equals("LanternWAlly"))
            {
                LastLantern = Utils.TickCount;
            }
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (!IsLanternSpellActive())
            {
                InitializeMenu.Menu.Item("LanternReady").SetValue(false);
                return;
            }

            InitializeMenu.Menu.Item("LanternReady").SetValue(true);

            if (InitializeMenu.Menu.Item("Auto").IsActive() && IsLow() && UseLantern())
            {
                return;
            }

            if (!InitializeMenu.Menu.Item("LanternHotkey").IsActive())
            {
                return;
            }

            UseLantern();
        }

        private static bool IsLow()
        {
            return Huabian.Player.HealthPercent <= InitializeMenu.Menu.Item("LowLantern").GetValue<Slider>().Value;
        }

        private static bool UseLantern()
        {
            var lantern =
                ObjectManager.Get<Obj_AI_Base>()
                .FirstOrDefault(
                    o => o.IsValid && o.IsAlly && o.Name.Equals("ThreshLantern") && Huabian.Player.Distance(o) <= 500);

            return lantern != null && lantern.IsVisible && Utils.TickCount - LastLantern > 5000 &&
                Huabian.Player.Spellbook.CastSpell(LanternSlot, lantern);
        }
    }
}