using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace FuckingAwesomeRiven
{
    internal class Antispells
    {
        public static void Init()
        {
            var mainMenu2 = MenuHandler.Config.SubMenu("Anti Spells");
            var mainMenu = mainMenu2.AddSubMenu(new Menu("Anti GapCloser", "Anti GapCloser"));
            var spellMenu = mainMenu.AddSubMenu(new Menu("Enabled Spells", "Enabled SpellsAnti GapCloser"));
            mainMenu.AddItem(new MenuItem("EnabledGC", "Enabled").SetValue(false));
            mainMenu.AddItem(new MenuItem("Ward Mechanic", "Ward Mechanic").SetValue(false));

            var mainMenuinterrupter = mainMenu2.AddSubMenu(new Menu("Interrupter", "Interrupter"));
            mainMenuinterrupter.AddItem(new MenuItem("EnabledInterrupter", "Enabled").SetValue(false));
            mainMenuinterrupter.AddItem(
                new MenuItem("minChannel", "Minimum Channel Priority").SetValue(
                    new StringList(new[] {"HIGH", "MEDIUM", "LOW"})));


            foreach (var champ in ObjectManager.Get<Obj_AI_Hero>().Where(a => a.IsEnemy))
            {
                var champmenu = spellMenu.AddSubMenu(new Menu(champ.ChampionName, champ.ChampionName + "GC"));
                var foreachChamp = champ;
                foreach (
                    var gcSpell in
                        AntiGapcloser.Spells.Where(gcSpell => gcSpell.ChampionName == foreachChamp.ChampionName))
                {
                    champmenu.AddItem(
                        new MenuItem(gcSpell.SpellName, gcSpell.SpellName + "- " + gcSpell.Slot).SetValue(true));
                }
            }

            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!MenuHandler.Config.Item("EnabledGC").GetValue<bool>())
            {
                return;
            }

            if (MenuHandler.Config.Item(gapcloser.Sender.LastCastedSpellName().ToLower()) == null)
            {
                return;
            }

            if (!MenuHandler.Config.Item(gapcloser.Sender.LastCastedSpellName().ToLower()).GetValue<bool>() ||
                !gapcloser.Sender.IsValidTarget())
            {
                return;
            }

            if (CheckHandler.QCount == 2 && MenuHandler.Config.Item("Ward Mechanic").GetValue<bool>() &&
                MenuHandler.Config.Item("flee").GetValue<KeyBind>().Active &&
                gapcloser.Sender.Distance(ObjectManager.Player) < SpellHandler.QRange)
            {
                if (Queuer.Queue.Contains("Q") || Items.GetWardSlot() == null)
                {
                    return;
                }

                ObjectManager.Player.Spellbook.CastSpell(
                    Items.GetWardSlot().SpellSlot, ObjectManager.Player.Position.Extend(gapcloser.End, 50));
                Queuer.Queue.Insert(0, "Q");
                Utility.DelayAction.Add(
                    200, () => ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos));
                return;
            }

            if (gapcloser.Sender.Distance(ObjectManager.Player) < SpellHandler.WRange &&
                !MenuHandler.Config.Item("Ward Mechanic").GetValue<bool>())
            {
                Queuer.Add("W");
                return;
            }

            if (gapcloser.Sender.Distance(ObjectManager.Player) < SpellHandler.QRange && CheckHandler.QCount == 2 &&
                !MenuHandler.Config.Item("Ward Mechanic").GetValue<bool>())
            {
                Queuer.Add("Q");
            }
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!MenuHandler.Config.Item("EnabledInterrupter").GetValue<bool>() || !sender.IsValidTarget())
            {
                return;
            }

            Interrupter2.DangerLevel a;
            switch (MenuHandler.Config.Item("minChannel").GetValue<StringList>().SelectedValue)
            {
                case "HIGH":
                    a = Interrupter2.DangerLevel.High;
                    break;
                case "MEDIUM":
                    a = Interrupter2.DangerLevel.Medium;
                    break;
                default:
                    a = Interrupter2.DangerLevel.Low;
                    break;
            }

            if (args.DangerLevel != Interrupter2.DangerLevel.High &&
                (args.DangerLevel != Interrupter2.DangerLevel.Medium || a == Interrupter2.DangerLevel.High) &&
                (args.DangerLevel != Interrupter2.DangerLevel.Medium || a == Interrupter2.DangerLevel.Medium ||
                 a == Interrupter2.DangerLevel.High))
            {
                return;
            }

            if (sender.Distance(ObjectManager.Player) < SpellHandler.WRange)
            {
                Queuer.Add("W");
                return;
            }

            if (sender.Distance(ObjectManager.Player) < 250 + 325)
            {
                Queuer.Add("E", sender.Position);
                Queuer.Add("W");
                return;
            }

            if (sender.Distance(ObjectManager.Player) < SpellHandler.QRange && CheckHandler.QCount == 2)
            {
                Queuer.Add("Q");
            }
        }
    }
}