using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace FuckingAwesomeRiven.EvadeUtils
{
    internal class AutoE
    {
        public static void Init()
        {
            var mainMenu2 = MenuHandler.Config.SubMenu("Anti Spells");
            var poop = mainMenu2.AddSubMenu(new Menu("Auto E", "AutoE"));
            poop.AddItem(new MenuItem("EnabledautoE", "Enabled").SetValue(false));
            poop.AddItem(new MenuItem("donteCC", "Disable For CC").SetValue(true));
            foreach (var champions in ObjectManager.Get<Obj_AI_Hero>().Where(a => a.IsEnemy))
            {
                var afdsdf = poop.AddSubMenu(new Menu(champions.ChampionName, champions.ChampionName + "kek"));
                var foreachChampions = champions;
                foreach (
                    var s in
                        TargetSpellDatabase.Spells.Where(
                            s =>
                                foreachChampions.ChampionName.ToLower() == s.ChampionName && s.Type != SpellType.Self &&
                                s.Type != SpellType.AutoAttack))
                {
                    afdsdf.AddItem(new MenuItem(s.Name, s.Name + " - " + s.Spellslot).SetValue(true));
                }
            }
        }

        public static void autoE(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsValidTarget() || !sender.IsChampion(sender.CharData.BaseSkinName) ||
                sender.Distance(ObjectManager.Player) > 1500 || !SpellHandler.Spells[LeagueSharp.SpellSlot.E].IsReady() ||
                args.SData.IsAutoAttack() || !MenuHandler.Config.Item("EnabledautoE").GetValue<bool>())
            {
                return;
            }

            var sData = TargetSpellDatabase.GetByName(args.SData.Name);

            if (MenuHandler.Config.Item(sData.Name) == null)
            {
                Console.WriteLine("Menu missing");
                return;
            }

            if ((MenuHandler.Config.Item("donteCC").GetValue<bool>() && sData.Type != SpellType.AutoAttack &&
                 sData.CcType != CcType.No) ||
                !MenuHandler.Config.Item(sData.Name).GetValue<bool>() || sData.Type == SpellType.Self)
            {
                return;
            }


            if (sData.Type == SpellType.Skillshot)
            {
                var sShot = SpellDatabase.GetByName(args.SData.Name);
                if (sShot.Type == SkillShotType.SkillshotMissileLine || sShot.Type == SkillShotType.SkillshotLine)
                {
                    var value = sShot.Range/sShot.Radius;
                    for (var i = 0; i < value; i++)
                    {
                        var vector = sender.Position.Extend(args.End, (i*sShot.Radius));
                        if (!(ObjectManager.Player.Distance(vector) < sShot.Radius))
                        {
                            continue;
                        }

                        SpellHandler.CastE((MenuHandler.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None)
                            ? (StateHandler.Target.IsValidTarget() ? StateHandler.Target.Position : Game.CursorPos)
                            : Game.CursorPos);
                        break;
                    }
                }
                if (sShot.Type == SkillShotType.SkillshotCircle)
                {
                    if (sShot.MissileSpeed == int.MaxValue) return;
                    if (ObjectManager.Player.Position.Distance(args.End) < sShot.Radius)
                    {
                        SpellHandler.CastE((MenuHandler.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None)
                            ? (StateHandler.Target.IsValidTarget() ? StateHandler.Target.Position : Game.CursorPos)
                            : Game.CursorPos);
                    }
                }
                if (args.End.Distance(ObjectManager.Player.Position) < 100)
                {
                    SpellHandler.CastE((MenuHandler.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None)
                        ? (StateHandler.Target.IsValidTarget() ? StateHandler.Target.Position : Game.CursorPos)
                        : Game.CursorPos);
                    return;
                }
            }


            if (sData.Type == SpellType.Targeted && args.Target.IsMe)
            {
                SpellHandler.CastE((MenuHandler.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None)
                    ? (StateHandler.Target.IsValidTarget() ? StateHandler.Target.Position : Game.CursorPos)
                    : Game.CursorPos);
            }
        }
    }
}
