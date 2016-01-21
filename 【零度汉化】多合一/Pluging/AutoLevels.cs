namespace Pluging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using Flowers_Utility.Common;

    public class AutoLevels
    {
        public static Menu Menu;
        public static MenuItem _activate;
        public static SpellSlot[] _priority;
        public static IDictionary<MenuItem, int> _menuMap;
        public static bool _lastFormatCorrect = true;
        public static int _level;
        public static ALevelStrategy _levelStrategy;
        public static MenuItem _delay;

        private static readonly IDictionary<SpellSlot, int> DefaultSpellSlotPriorities = new Dictionary<SpellSlot, int>
        {
            { SpellSlot.Q, 2 },
            { SpellSlot.W, 3 },
            { SpellSlot.E, 4 },
            { SpellSlot.R, 1 }
        };

        private static readonly IList<SpellSlot> SpellSlots = new List<SpellSlot>
        {
            SpellSlot.Q,
            SpellSlot.W,
            SpellSlot.E,
            SpellSlot.R
        };

        private static readonly IDictionary<string, ALevelStrategy> LevelStrategyByChampion = new Dictionary<string, ALevelStrategy>
        {
            { "Jayce", new LevelOneUltiStrategy() },
            { "Karma", new LevelOneUltiStrategy() },
            { "Nidalee", new LevelOneUltiStrategy() },
            { "Elise", new LevelOneUltiStrategy() },
            { "Udyr", new UdyrStrategy() },
        };

        public AutoLevels(Menu mainMenu)
        {
            Menu = mainMenu;

            Menu AutoLevelsMenu = new Menu("[FL] 自动加点", "AutoLevels");

            foreach (var entry in DefaultSpellSlotPriorities)
            {
                MenuItem menuItem = MakeSlider(entry.Key.ToString(), entry.Key.ToString(), entry.Value, 1, DefaultSpellSlotPriorities.Count);
                menuItem.ValueChanged += menuItem_ValueChanged;
                AutoLevelsMenu.AddItem(menuItem);

                var subMenu = new Menu(entry.Key + " 加点设置", entry.Key + "extra");
                subMenu.AddItem(MakeSlider(entry.Key + "extra", "几级后停止加", 1, 1, 18));
                AutoLevelsMenu.AddSubMenu(subMenu);
            }

            _activate = new MenuItem("activate", "几级开始加点?").SetValue(new StringList(new[] { "2", "3", "4" }));
            _delay = new MenuItem("delay", "加点延迟 (ms)").SetValue(new Slider(0, 0, 2000));
            AutoLevelsMenu.AddItem(_activate);
            AutoLevelsMenu.AddItem(_delay);

            Menu.AddSubMenu(AutoLevelsMenu);

            _menuMap = new Dictionary<MenuItem, int>();

            foreach (var entry in DefaultSpellSlotPriorities)
            {
                MenuItem item = Menu.GetSlider(entry.Key.ToString());
                _menuMap[item] = item.GetValue<Slider>().Value;
            }

            ParseMenu();

            _levelStrategy = new DefaultLevelStrategy();

            if (LevelStrategyByChampion.ContainsKey(ObjectManager.Player.ChampionName))
            {
                _levelStrategy = LevelStrategyByChampion[ObjectManager.Player.ChampionName];
            }

            _level = ObjectManager.Player.Level;

            Game.OnUpdate += OnUpdate;
        }

        private void OnUpdate(EventArgs args)
        {
            try
            {
                int newLevel = ObjectManager.Player.Level;

                if (_level < newLevel)
                {
                    var levelupArgs = new CustomEvents.Unit.OnLevelUpEventArgs
                    {
                        NewLevel = newLevel,
                        RemainingPoints = newLevel - _level
                    };

                    _level = newLevel;

                    UnitOnOnLevelUp(ObjectManager.Player, levelupArgs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in AutoLevels.OnUpdate + " + ex);
            }
        }

        private void menuItem_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            try
            {
                int oldValue = _menuMap[((MenuItem)sender)];

                int newValue = e.GetNewValue<Slider>().Value;

                if (oldValue != newValue)
                {
                    _menuMap[((MenuItem)sender)] = newValue;

                    ParseMenu();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in AutoLevels.menuItem_ValueChanged + " + ex);
            }
        }

        private void ParseMenu()
        {
            try
            {
                var indices = new bool[DefaultSpellSlotPriorities.Count];

                bool format = true;

                _priority = new SpellSlot[DefaultSpellSlotPriorities.Count];

                foreach (var entry in DefaultSpellSlotPriorities)
                {
                    int index = _menuMap[Menu.GetSlider(entry.Key.ToString())] - 1;

                    if (indices[index])
                    {
                        format = false;
                    }

                    indices[index] = true;

                    _priority[index] = entry.Key;
                }

                if (!format)
                {
                    _priority = null;

                    _lastFormatCorrect = false;
                }
                else
                {
                    if (!_lastFormatCorrect)
                    {
                        //Ignore
                    }

                    _lastFormatCorrect = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in AutoLevels.ParseMenu + " + ex);
            }
        }

        public static int GetMinLevel(SpellSlot s)
        {
            return Menu.SubMenu(s + "extra").GetSlider(s + "extra").GetValue<Slider>().Value;
        }

        public static MenuItem MakeSlider(string name, string display, int value, int min, int max)
        {
            var item = new MenuItem(name + ObjectManager.Player.ChampionName, display);

            item.SetValue(new Slider(value, min, max));

            return item;
        }

        private static void UnitOnOnLevelUp(Obj_AI_Base sender, CustomEvents.Unit.OnLevelUpEventArgs args)
        {
            try
            {
                if (!sender.IsValid || !sender.IsMe || _priority == null || TotalLeveled() - _levelStrategy.LevelOneSkills == 0)
                {
                    return;
                }

                foreach (SpellSlot spellSlot in _priority)
                {
                    if (ObjectManager.Player.Spellbook.GetSpell(spellSlot).Level == 0 && args.NewLevel >= GetMinLevel(spellSlot) && args.NewLevel > _levelStrategy.MinimumLevel(spellSlot) && _levelStrategy.CanLevel(args.NewLevel, spellSlot))
                    {
                        Level(spellSlot);

                        return;
                    }
                }

                var sl = _activate.GetValue<StringList>();

                if (args.NewLevel >= int.Parse(sl.SList[sl.SelectedIndex]))
                {
                    SpellSlot spellSlot = _levelStrategy.GetSpellSlotToLevel(args.NewLevel, _priority, false);

                    if (spellSlot != SpellSlot.Unknown)
                    {
                        Level(spellSlot);
                    }
                    else
                    {
                        SpellSlot spellSlotIgnoreBaseLevel = _levelStrategy.GetSpellSlotToLevel(args.NewLevel, _priority, true);

                        if (spellSlotIgnoreBaseLevel != SpellSlot.Unknown)
                        {
                            Level(spellSlotIgnoreBaseLevel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in AutoLevels.UnitOnOnLevelUp + " + ex);
            }
        }

        private static void Level(SpellSlot spellSlot)
        {
            Utility.DelayAction.Add(_delay.GetValue<Slider>().Value, () => ObjectManager.Player.Spellbook.LevelSpell(spellSlot));
        }

        private static int TotalLeveled()
        {
            return SpellSlots.Sum(s => ObjectManager.Player.Spellbook.GetSpell(s).Level);
        }
    }
}