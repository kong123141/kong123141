using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LeagueSharp;
using LeagueSharp.Common;

namespace DZJayce.Utility
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class SpellHandler
    {
        public static Spell Q
        {
            get { return ObjectManager.Player.IsRangedForm() ? spells[Spells.QRanged] : spells[Spells.QMelee]; }
        }

        public static Spell W
        {
            get { return ObjectManager.Player.IsRangedForm() ? spells[Spells.WRanged] : spells[Spells.WMelee]; }
        }

        public static Spell E
        {
            get { return ObjectManager.Player.IsRangedForm() ? spells[Spells.ERanged] : spells[Spells.EMelee]; }
        }

        public static Spell R
        {
            get { return spells[Spells.R]; }
        }

        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            {Spells.QMelee, new Spell(SpellSlot.Q, 600)},
            {Spells.WMelee, new Spell(SpellSlot.W, 285)},
            {Spells.EMelee, new Spell(SpellSlot.E, 240)},
            {Spells.QRanged, new Spell(SpellSlot.Q, 1050)},
            {Spells.QGate, new Spell(SpellSlot.Q, 1650)},
            {Spells.WRanged, new Spell(SpellSlot.W)},
            {Spells.ERanged, new Spell(SpellSlot.E, 650)},
            {Spells.R, new Spell(SpellSlot.R)}
        };

        public static Dictionary<Spells, float> spellCooldowns = new Dictionary<Spells, float>
        {
            {Spells.QMelee, 0f},
            {Spells.WMelee, 0f},
            {Spells.EMelee, 0f},
            {Spells.QRanged, 0f},
            {Spells.QGate, 0f},
            {Spells.WRanged, 0f},
            {Spells.ERanged, 0f},
            {Spells.R, 0f}
        };

        private static readonly float[] QMeleeCooldowns = {16, 14, 12, 10, 8, 6};
        private static readonly float[] WMeleeCooldowns = { 10, 10, 10, 10, 10 };
        private static readonly float[] EMeleeCooldowns = { 15, 14, 13, 12, 11, 10 };
        private static readonly float[] QRangedCooldowns = { 8, 8, 8, 8, 8 };
        private static readonly float[] WRangedCooldowns = { 13, 11.4f, 9.8f, 8.2f, 6.6f, 5 };
        private static readonly float[] ERangedCooldowns = { 16, 16, 16, 16, 16 };

        public static void OnLoad()
        {
            spells[Spells.QRanged].SetSkillshot(0.25f, 80f, 1200, true, SkillshotType.SkillshotLine);
            spells[Spells.QGate].SetSkillshot(0.35f, 100f, 1600, true, SkillshotType.SkillshotLine);
            spells[Spells.QMelee].SetTargetted(0.25f, float.MaxValue);

            spells[Spells.ERanged].SetSkillshot(0.15f, 120, float.MaxValue, false, SkillshotType.SkillshotLine);
            spells[Spells.EMelee].SetTargetted(0.25f, float.MaxValue);

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                switch (args.SData.Name)
                {
                    case "JayceToTheSkies":
                        //Q Melee
                        spellCooldowns[Spells.QMelee] = Utils.GameTimeTickCount +
                                                         FactorCDReduction(QMeleeCooldowns[Q.Level]);
                        break;
                    case "JayceStaticField":
                        //W Melee
                        spellCooldowns[Spells.WMelee] = Utils.GameTimeTickCount +
                                                         FactorCDReduction(WMeleeCooldowns[W.Level]);
                        break;
                    case "JayceThunderingBlow":
                        //E Melee
                        spellCooldowns[Spells.EMelee] = Utils.GameTimeTickCount +
                                                         FactorCDReduction(EMeleeCooldowns[E.Level]);
                        break;
                    case "jayceshockblast":
                        //Q Ranged
                        spellCooldowns[Spells.QRanged] = Utils.GameTimeTickCount +
                                                         FactorCDReduction(QRangedCooldowns[Q.Level]);
                        break;
                    case "jaycehypercharge":
                        //W Ranged
                        spellCooldowns[Spells.WRanged] = Utils.GameTimeTickCount +
                                                         FactorCDReduction(WRangedCooldowns[W.Level]);
                        break;
                    case "jayceaccelerationgate":
                        //E Ranged
                        spellCooldowns[Spells.ERanged] = Utils.GameTimeTickCount +
                                                         FactorCDReduction(ERangedCooldowns[W.Level]);
                        break;
                }
            }
        }

        public static float FactorCDReduction(float CD)
        {
            return CD + (CD*ObjectManager.Player.PercentCooldownMod);
        }

        public static float GetCooldown(Spells Spell)
        {
            return Spell == Spells.R  ?
                  ((Utils.GameTimeTickCount - ObjectManager.Player.GetSpell(SpellSlot.R).SData.Cooldown > 0) ? Utils.GameTimeTickCount - ObjectManager.Player.GetSpell(SpellSlot.R).SData.Cooldown : 0)
                : (Utils.GameTimeTickCount - spellCooldowns[Spell] > 0) ? Utils.GameTimeTickCount - spellCooldowns[Spell] : 0 ;
        }

    }

    internal enum Spells
    {
        QMelee, WMelee, EMelee, QRanged, WRanged, ERanged, R, QGate
    }
}
