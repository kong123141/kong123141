using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace 花边_花式多合一.Core
{
    public static class AutoLevels
    {

        public static MenuItem _activate;
        public static SpellSlot[] _priority;
        public static IDictionary<MenuItem, int> _menuMap;
        public static bool _lastFormatCorrect = true;
        public static int _level;
        public static ALevelStrategy _levelStrategy;
        public static MenuItem _delay;

        public static readonly IDictionary<SpellSlot, int> DefaultSpellSlotPriorities = new Dictionary<SpellSlot, int>
        {
            { SpellSlot.Q, 2 },
            { SpellSlot.W, 3 },
            { SpellSlot.E, 4 },
            { SpellSlot.R, 1 }
        };

        public static readonly IList<SpellSlot> SpellSlots = new List<SpellSlot>
        {
            SpellSlot.Q,
            SpellSlot.W,
            SpellSlot.E,
            SpellSlot.R
        };

        public static readonly IDictionary<string, ALevelStrategy> LevelStrategyByChampion =
            new Dictionary<string, ALevelStrategy>
            {
                { "Jayce", new LevelOneUltiStrategy() },
                { "Karma", new LevelOneUltiStrategy() },
                { "Nidalee", new LevelOneUltiStrategy() },
                { "Elise", new LevelOneUltiStrategy() },
                { "Udyr", new UdyrStrategy() },
            };

        public static MenuItem MakeSlider(string name, string display, int value, int min, int max)
        {
            var item = InitializeMenu.Menu.SubMenu("自动功能").SubMenu("自动加点").AddItem(new MenuItem(name + ObjectManager.Player.ChampionName, display));
            item.SetValue(new Slider(value, min, max));
            return item;
        }

        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if (!InitializeMenu.Menu.Item("AutoLevelsEnable").GetValue<bool>()) return;

                _menuMap = new Dictionary<MenuItem, int>();

                foreach (var entry in DefaultSpellSlotPriorities)
                {
                    MenuItem item = InitializeMenu.Menu.GetSlider(entry.Key.ToString());
                    _menuMap[item] = item.GetValue<Slider>().Value;
                }

                ParseMenu();

                _levelStrategy = new AutoLevels.DefaultLevelStrategy();
                if (LevelStrategyByChampion.ContainsKey(ObjectManager.Player.ChampionName))
                {
                    _levelStrategy = LevelStrategyByChampion[ObjectManager.Player.ChampionName];
                }
                _level = ObjectManager.Player.Level;

                Game.OnUpdate += Game_OnUpdate;

            }
            catch (Exception ex)
            {
                Console.WriteLine("AutoLevels error occurred: '{0}'", ex);
            }

        }

        private static void Game_OnUpdate(EventArgs args)
        {
            int newLevel = ObjectManager.Player.Level;
            if (_level < newLevel)
            {
                var levelupArgs = new LeagueSharp.Common.CustomEvents.Unit.OnLevelUpEventArgs
                {
                    NewLevel = newLevel,
                    RemainingPoints = newLevel - _level
                };
                _level = newLevel;

                UnitOnOnLevelUp(ObjectManager.Player, levelupArgs);
            }
        }

        private static void ParseMenu()
        {
            var indices = new bool[DefaultSpellSlotPriorities.Count];
            bool format = true;
            _priority = new SpellSlot[DefaultSpellSlotPriorities.Count];
            foreach (var entry in DefaultSpellSlotPriorities)
            {
                int index = _menuMap[InitializeMenu.Menu.GetSlider(entry.Key.ToString())] - 1;
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
                }
                _lastFormatCorrect = true;
            }
        }

        public static MenuItem GetSlider(this Menu menu, string name)
        {
            return InitializeMenu.Menu.Item(name + ObjectManager.Player.ChampionName);
        }

        public static void menuItem_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            int oldValue = _menuMap[((MenuItem)sender)];
            int newValue = e.GetNewValue<Slider>().Value;
            if (oldValue != newValue)
            {
                _menuMap[((MenuItem)sender)] = newValue;
                ParseMenu();
            }
        }

        public static int GetMinLevel(SpellSlot s)
        {
            return InitializeMenu.Menu.SubMenu(s + "extra").GetSlider(s + "extra").GetValue<Slider>().Value;
        }

        private static void UnitOnOnLevelUp(Obj_AI_Base sender, LeagueSharp.Common.CustomEvents.Unit.OnLevelUpEventArgs args)
        {
            if (!sender.IsValid || !sender.IsMe || _priority == null ||
                TotalLeveled() - _levelStrategy.LevelOneSkills == 0)
            {
                return;
            }

            foreach (SpellSlot spellSlot in _priority)
            {
                if (ObjectManager.Player.Spellbook.GetSpell(spellSlot).Level == 0 &&
                    args.NewLevel >= GetMinLevel(spellSlot) && args.NewLevel > _levelStrategy.MinimumLevel(spellSlot) &&
                    _levelStrategy.CanLevel(args.NewLevel, spellSlot))
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

        private static void Level(SpellSlot spellSlot)
        {
            Utility.DelayAction.Add(_delay.GetValue<Slider>().Value, () => ObjectManager.Player.Spellbook.LevelSpell(spellSlot));
        }

        private static int TotalLeveled()
        {
            return SpellSlots.Sum(s => ObjectManager.Player.Spellbook.GetSpell(s).Level);
        }

        private class DefaultLevelStrategy : ALevelStrategy
        {
            private readonly IDictionary<SpellSlot, int> _minimumLevelBySpellSlot = new Dictionary<SpellSlot, int>
            {
                { SpellSlot.Q, 3 },
                { SpellSlot.W, 3 },
                { SpellSlot.E, 3 },
                { SpellSlot.R, 6 },
            };

            public override int LevelOneSkills
            {
                get { return 0; }
            }

            public override int MinimumLevel(SpellSlot spellSlot)
            {
                return _minimumLevelBySpellSlot[spellSlot];
            }

            public override bool CanLevel(int currentLevel, SpellSlot spellSlot)
            {
                if (spellSlot == SpellSlot.R)
                {
                    return base.CanLevel(currentLevel, spellSlot) && CanLevelUlti(currentLevel, 0, spellSlot);
                }
                return base.CanLevel(currentLevel, spellSlot);
            }

            public override SpellSlot GetSpellSlotToLevel(int currentLevel, SpellSlot[] priorities, bool ignoreBaseLevel)
            {
                foreach (SpellSlot s in priorities)
                {
                    bool baselevel = ignoreBaseLevel || ((ObjectManager.Player.Spellbook.GetSpell(s).Level == 0 && currentLevel <= 3) ||
                                      currentLevel > 3);
                    if (baselevel && currentLevel >= GetMinLevel(s) && CanLevel(currentLevel, s))
                    {
                        return s;
                    }
                }
                return SpellSlot.Unknown;
            }
        }
    }

    public class LevelOneUltiStrategy : ALevelStrategy
    {
        private readonly IDictionary<SpellSlot, int> _minimumLevelBySpellSlot = new Dictionary<SpellSlot, int>
        {
            { SpellSlot.Q, 3 },
            { SpellSlot.W, 3 },
            { SpellSlot.E, 3 },
            { SpellSlot.R, 6 },
        };

        public override int LevelOneSkills
        {
            get { return 1; }
        }

        public override int MinimumLevel(SpellSlot spellSlot)
        {
            return _minimumLevelBySpellSlot[spellSlot];
        }

        public override bool CanLevel(int currentLevel, SpellSlot spellSlot)
        {
            if (spellSlot == SpellSlot.R)
            {
                return base.CanLevel(currentLevel, spellSlot) && CanLevelUlti(currentLevel, 1, spellSlot);
            }
            return base.CanLevel(currentLevel, spellSlot);
        }

        public override SpellSlot GetSpellSlotToLevel(int currentLevel, SpellSlot[] priorities, bool ignoreBaseLevel)
        {
            foreach (SpellSlot s in priorities)
            {
                bool baselevel = ignoreBaseLevel || ((ObjectManager.Player.Spellbook.GetSpell(s).Level == 0 && currentLevel <= 3) ||
                                  currentLevel > 3);
                if (baselevel && currentLevel >= AutoLevels.GetMinLevel(s) && CanLevel(currentLevel, s))
                {
                    return s;
                }
            }
            return SpellSlot.Unknown;
        }
    }

    public abstract class ALevelStrategy
    {
        private readonly IDictionary<int, int> _summonerLevelByUltiLevel = new Dictionary<int, int>
        {
            { 1, 6 },
            { 2, 11 },
            { 3, 16 },
        };

        public abstract int LevelOneSkills { get; }

        public abstract int MinimumLevel(SpellSlot spellSlot);

        public abstract SpellSlot GetSpellSlotToLevel(int currentLevel, SpellSlot[] priorities, bool ignoreBaseLevel);

        public virtual bool CanLevel(int currentLevel, SpellSlot spellSlot)
        {
            int spellLevel = ObjectManager.Player.Spellbook.GetSpell(spellSlot).Level;
            if (spellLevel >= 5)
            {
                return false;
            }
            int div = currentLevel / 2;
            if (((currentLevel ^ 2) >= 0) && (currentLevel % 2 != 0))
            {
                div++;
            }
            return spellLevel < div;
        }

        protected bool CanLevelUlti(int currentLevel, int defaultLevel, SpellSlot spellSlot)
        {
            int levelUlti = ObjectManager.Player.Spellbook.GetSpell(spellSlot).Level - defaultLevel;
            if (levelUlti >= 3)
            {
                return false;
            }
            return currentLevel >= _summonerLevelByUltiLevel[levelUlti + 1];
        }
    }

    public class UdyrStrategy : ALevelStrategy
    {
        private readonly IDictionary<SpellSlot, int> _minimumLevelBySpellSlot = new Dictionary<SpellSlot, int>
        {
            { SpellSlot.Q, 4 },
            { SpellSlot.W, 4 },
            { SpellSlot.E, 4 },
            { SpellSlot.R, 4 },
        };

        public override int LevelOneSkills
        {
            get { return 0; }
        }

        public override int MinimumLevel(SpellSlot spellSlot)
        {
            return _minimumLevelBySpellSlot[spellSlot];
        }

        public override SpellSlot GetSpellSlotToLevel(int currentLevel, SpellSlot[] priorities, bool ignoreBaseLevel)
        {
            foreach (SpellSlot s in priorities)
            {
                bool baselevel = ignoreBaseLevel || ((ObjectManager.Player.Spellbook.GetSpell(s).Level == 0 && currentLevel <= 4) ||
                                  currentLevel > 4);
                if (baselevel && currentLevel >= AutoLevels.GetMinLevel(s) && CanLevel(currentLevel, s))
                {
                    return s;
                }
            }
            return SpellSlot.Unknown;
        }
    }

}
