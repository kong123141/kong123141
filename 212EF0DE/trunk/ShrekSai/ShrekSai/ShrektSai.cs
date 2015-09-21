using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using ShrekSai.Utility;
using ShrekSai.Utility.Enumerations;

namespace ShrekSai
{
    class ShrektSai
    {
        #region Fields & Operators
        public static Menu RootMenu { get; set; }

        public static Orbwalking.Orbwalker OrbwalkerInstance { get; set; }

        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            {Spells.Q, new Spell(SpellSlot.Q, 300f)},
            {Spells.W, new Spell(SpellSlot.W, 250f)},
            {Spells.E, new Spell(SpellSlot.W, 250f)},
            {Spells.QBurrow, new Spell(SpellSlot.W, 1500f, TargetSelector.DamageType.Magical)},
            {Spells.WBurrow, new Spell(SpellSlot.W, 250f)},
            {Spells.EBurrow, new Spell(SpellSlot.W, 750f)},
        };

        public static Spells Q
        {
            get
            {
                return Helpers.IsPlayerBurrowed() ? Spells.Q : Spells.QBurrow;
            }
        }

        public static Spells W
        {
            get
            {
                return Helpers.IsPlayerBurrowed() ? Spells.W : Spells.WBurrow;
            }
        }

        public static Spells E
        {
            get
            {
                return Helpers.IsPlayerBurrowed() ? Spells.E : Spells.EBurrow;
            }
        }

        #endregion

        public static void OnLoad()
        {
            LoadEvents();
            LoadSpells();

        }

        private static void LoadEvents()
        {
            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }

        private static void LoadSpells()
        {

        }

        #region Event Delegates

        private static void Game_OnUpdate(EventArgs args)
        {
            //TODO
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            //TODO
        }
        #endregion
    }
}
