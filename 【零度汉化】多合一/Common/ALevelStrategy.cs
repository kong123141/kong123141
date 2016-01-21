namespace Flowers_Utility.Common
{
    using System.Collections.Generic;
    using LeagueSharp;
    using Pluging;
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

    public class DefaultLevelStrategy : ALevelStrategy
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
                if (baselevel && currentLevel >= AutoLevels.GetMinLevel(s) && CanLevel(currentLevel, s))
                {
                    return s;
                }
            }
            return SpellSlot.Unknown;
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
