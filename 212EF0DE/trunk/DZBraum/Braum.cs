using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace DZBraum
{
    class Braum
    {
        public static Menu BraumMenu { get; set; }
        public static Orbwalking.Orbwalker Orbwalker { get; set; }

        //DO YOU HAVE A MOMENT TO TALK ABOUT DIKTIONARIESS!=!=!=!==??!?!? -Everance 2k15
        public static Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>()
        {
            {SpellSlot.Q, new Spell(SpellSlot.Q, 1000f)},
            {SpellSlot.W, new Spell(SpellSlot.W, 650f)},
            {SpellSlot.E, new Spell(SpellSlot.E)},
            {SpellSlot.E, new Spell(SpellSlot.R, 1200)}
        };
        public static void OnLoad()
        {
            InitSkills();
            InitEvents();
        }

        private static void InitSkills()
        {
            spells[SpellSlot.Q].SetSkillshot(0.25f, 50f, 1550f, true, SkillshotType.SkillshotLine);
            spells[SpellSlot.R].SetSkillshot(0.55f, 108f, 1350f, false, SkillshotType.SkillshotLine);
        }

        private static void InitEvents()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    OnCombo();
                    break;
            }
            OnUpdateFunctions();
        }

        #region Modes
        private static void OnCombo()
        {
            
        }
        #endregion

        #region OnUpdate Functions
        private static void OnUpdateFunctions()
        {
            
        }
        #endregion

    }
}
