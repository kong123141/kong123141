﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace UnderratedAIO.Helpers
{
    public class CombatHelper
    {
        public static Obj_AI_Hero player = ObjectManager.Player;

        private static List<string> dotsHighDmg =
            new List<string>(
                new string[]
                {
                    "karthusfallenonecastsound", "CaitlynAceintheHole", "zedulttargetmark", "timebombenemybuff",
                    "VladimirHemoplague"
                });

        private static List<string> dotsMedDmg =
            new List<string>(
                new string[]
                {
                    "summonerdot", "cassiopeiamiasmapoison", "cassiopeianoxiousblastpoison", "bantamtraptarget",
                    "explosiveshotdebuff", "swainbeamdamage", "SwainTorment", "AlZaharMaleficVisions",
                    "fizzmarinerdoombomb"
                });

        private static List<string> dotsSmallDmg =
            new List<string>(
                new string[]
                { "deadlyvenom", "toxicshotparticle", "MordekaiserChildrenOfTheGrave", "dariushemo", "brandablaze" });

        private static List<string> defSpells = new List<string>(new string[] { "summonerheal", "summonerbarrier" });

        private static List<string> autoAttacks =
            new List<string>(
                new string[]
                {
                    "frostarrow", "CaitlynHeadshotMissile", "KennenMegaProc", "QuinnWEnhanced", "TrundleQ",
                    "XenZhaoThrust", "XenZhaoThrust2", "XenZhaoThrust3", "RenektonExecute", "RenektonSuperExecute",
                    "MasterYiDoubleStrike", "Parley"
                });

        public static List<string> TargetedCC =
            new List<string>(
                new string[]
                {
                    "TristanaR", "BlindMonkRKick", "AlZaharNetherGrasp", "VayneCondemn", "JayceThunderingBlow", "Headbutt",
                    "Drain", "BlindingDart", "RunePrison", "IceBlast", "Dazzle", "Fling", "MaokaiUnstableGrowth",
                    "MordekaiserChildrenOfTheGrave", "ZedUlt", "LuluW", "PantheonW", "ViR", "JudicatorReckoning",
                    "IreliaEquilibriumStrike", "InfiniteDuress", "SkarnerImpale", "SowTheWind", "PuncturingTaunt",
                    "UrgotSwap2", "NasusW", "VolibearW", "Feast", "NocturneUnspeakableHorror", "Terrify", "VeigarPrimordialBurst"
                });

        public static List<string> invulnerable =
            new List<string>(
                new string[]
                {
                    "sionpassivezombie", "willrevive", "BraumShieldRaise", "UndyingRage", "PoppyDiplomaticImmunity",
                    "LissandraRSelf", "JudicatorIntervention", "ZacRebirthReady", "AatroxPassiveReady", "Rebirth",
                    "alistartrample", "NocturneShroudofDarknessShield", "SpellShield"
                });

        private static List<int> defItems =
            new List<int>(new int[] { ItemHandler.Qss.Id, ItemHandler.Qss.Id, ItemHandler.Dervish.Id });

        public static Obj_AI_Hero lastTarget;
        public static float lastTargetingTime;

        public static Obj_AI_Hero SetTarget(Obj_AI_Hero target, Obj_AI_Hero targetSelected)
        {
            //later
            return target;
        }

        #region Poppy

        public static Vector3 bestVectorToPoppyFlash(Obj_AI_Base target)
        {
            if (target == null)
            {
                return new Vector3();
            }
            Vector3 newPos = new Vector3();
            for (int i = 1; i < 7; i++)
            {
                for (int j = 1; j < 6; j++)
                {
                    newPos = new Vector3(target.Position.X + 65 * j, target.Position.Y + 65 * j, target.Position.Z);
                    var rotated = newPos.To2D().RotateAroundPoint(target.Position.To2D(), 45 * i).To3D();
                    if (rotated.IsValid() && Environment.Map.CheckWalls(rotated, target.Position) &&
                        player.Distance(rotated) < 400)
                    {
                        return rotated;
                    }
                }
            }

            return new Vector3();
        }

        public static Vector3 bestVectorToPoppyFlash2(Obj_AI_Base target)
        {
            if (target == null)
            {
                return new Vector3();
            }
            return
                PointsAroundTheTarget(target.Position, 500)
                    .Where(
                        p =>
                            p.IsValid() && target.Distance(p) > 80 && target.Distance(p) < 485 &&
                            player.Distance(p) < 400 && !p.IsWall() && Environment.Map.CheckWalls(p, target.Position))
                    .FirstOrDefault();
        }

        public static Vector3 PositionToPoppyE(Obj_AI_Base target)
        {
            if (target == null)
            {
                return new Vector3();
            }
            return
                PointsAroundTheTarget(target.Position, 500)
                    .Where(
                        p =>
                            p.Distance(player.Position) < 500 && p.IsValid() &&
                            target.Distance(p) < Orbwalking.GetRealAutoAttackRange(player) && !p.IsWall() &&
                            Environment.Map.CheckWalls(p, target.Position))
                    .OrderBy(p => p.Distance(player.Position))
                    .FirstOrDefault();
        }

        #endregion

        #region Riven

        private static float RivenDamageQ(SpellDataInst spell, Obj_AI_Hero src, Obj_AI_Hero dsc)
        {
            double dmg = 0;
            if (spell.IsReady())
            {
                dmg += src.CalcDamage(
                    dsc, Damage.DamageType.Physical,
                    (-10 + (spell.Level * 20) +
                     (0.35 + (spell.Level * 0.05)) * (src.FlatPhysicalDamageMod + src.BaseAttackDamage)) * 3);
            }
            return (float) dmg;
        }

        #endregion

        #region Sejuani

        public static int SejuaniCountFrostHero(float p)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(i => i.IsEnemy && !i.IsDead && player.Distance(i) < p)
                    .SelectMany(enemy => enemy.Buffs)
                    .Count(buff => buff.Name == "sejuanifrost");
        }

        public static int KennenCountMarkHero(float p)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(i => i.IsEnemy && !i.IsDead && player.Distance(i) < p)
                    .SelectMany(enemy => enemy.Buffs)
                    .Count(buff => buff.Name == "KennenMarkOfStorm");
        }

        public static int SejuaniCountFrostMinion(float p)
        {
            var num = 0;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Minion>().Where(i => !i.IsDead && player.Distance(i) < p))
            {
                foreach (BuffInstance buff in enemy.Buffs)
                {
                    if (buff.Name == "sejuanifrost")
                    {
                        num++;
                    }
                }
            }
            return num;
        }

        #endregion

        #region Common

        public static HitChance GetHitChance(int qHit)
        {
            var hitC = HitChance.High;
            switch (qHit)
            {
                case 1:
                    hitC = HitChance.Low;
                    break;
                case 2:
                    hitC = HitChance.Medium;
                    break;
                case 3:
                    hitC = HitChance.High;
                    break;
                case 4:
                    hitC = HitChance.VeryHigh;
                    break;
            }
            return hitC;
        }

        public static List<Vector3> PointsAroundTheTarget(Obj_AI_Base target, float dist)
        {
            if (target == null)
            {
                return new List<Vector3>();
            }
            List<Vector3> list = new List<Vector3>();
            var newPos = new Vector3();
            var prec = 15;
            if (dist > 1)
            {
                prec = 30;
            }
            var k = (float) ((2 * dist * Math.PI) / prec);
            for (int i = 1; i < prec + 1; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var perimeter =
                        target.Position.Extend(
                            new Vector3(target.Direction.X, target.Direction.Y, target.Position.Z), dist);
                    newPos = new Vector3(perimeter.X + 65 * j, perimeter.Y + 65 * j, target.Position.Z);
                    var rotated = newPos.To2D().RotateAroundPoint(target.Position.To2D(), k * i).To3D();
                    list.Add(rotated);
                }
            }

            return list;
        }

        public static List<Vector3> PointsAroundTheTarget(Vector3 pos, float dist, float prec = 15, float prec2 = 6)
        {
            if (!pos.IsValid())
            {
                return new List<Vector3>();
            }
            List<Vector3> list = new List<Vector3>();
            if (dist > 205)
            {
                prec = 30;
                prec2 = 8;
            }
            if (dist > 805)
            {
                dist = (float) (dist * 1.5);
                prec = 45;
                prec2 = 10;
            }
            var angle = 360 / prec * Math.PI / 180.0f;
            var step = dist * 2 / prec2;
            for (int i = 0; i < prec; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    list.Add(
                        new Vector3(
                            pos.X + (float) (Math.Cos(angle * i) * (j * step)),
                            pos.Y + (float) (Math.Sin(angle * i) * (j * step)) - 90, pos.Z));
                }
            }

            return list;
        }

        public static List<Vector3> PointsAroundTheTargetOuterRing(Vector3 pos, float dist, float width = 15)
        {
            if (!pos.IsValid())
            {
                return new List<Vector3>();
            }
            List<Vector3> list = new List<Vector3>();
            var max = 2 * dist / 2 * Math.PI / width / 2;
            var angle = 360f / max * Math.PI / 180.0f;
            for (int i = 0; i < max; i++)
            {
                list.Add(
                    new Vector3(
                        pos.X + (float) (Math.Cos(angle * i) * dist), pos.Y + (float) (Math.Sin(angle * i) * dist),
                        pos.Z));
            }

            return list;
        }

        public static bool IsFacing(Obj_AI_Base source, Vector3 target, float angle = 90)
        {
            if (source == null || !target.IsValid())
            {
                return false;
            }
            return
                (double)
                    Geometry.AngleBetween(
                        Geometry.Perpendicular(Geometry.To2D(source.Direction)), Geometry.To2D(target - source.Position)) <
                angle;
        }

        public static double GetAngle(Obj_AI_Base source, Vector3 target)
        {
            if (source == null || !target.IsValid())
            {
                return 0;
            }
            return Geometry.AngleBetween(
                Geometry.Perpendicular(Geometry.To2D(source.Direction)), Geometry.To2D(target - source.Position));
            ;
        }

        public static bool CheckCriticalBuffs(Obj_AI_Hero i)
        {
            foreach (BuffInstance buff in i.Buffs)
            {
                if (i.Health <= 6 * player.Level && dotsSmallDmg.Contains(buff.Name))
                {
                    return true;
                }
                if (i.Health <= 12 * player.Level && dotsMedDmg.Contains(buff.Name))
                {
                    return true;
                }
                if (i.Health <= 25 * player.Level && dotsHighDmg.Contains(buff.Name))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckBuffs(Obj_AI_Hero i)
        {
            foreach (BuffInstance buff in i.Buffs)
            {
                if (dotsSmallDmg.Contains(buff.Name))
                {
                    return true;
                }
                if (dotsMedDmg.Contains(buff.Name))
                {
                    return true;
                }
                if (dotsHighDmg.Contains(buff.Name))
                {
                    return true;
                }
            }
            return false;
        }

        public static float getIncDmg()
        {
            double result = 0;
            foreach (var enemy in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        i =>
                            i.Distance(player.Position) < 950 && i.IsEnemy && !i.IsAlly && !i.IsDead && !i.IsMinion &&
                            !i.IsMe)) {}


            return (float) result;
        }

        public static float GetChampDmgToMe(Obj_AI_Hero enemy)
        {
            double result = 0;
            double basicDmg = 0;
            int attacks = (int) Math.Floor(enemy.AttackSpeedMod * 5);
            for (int i = 0; i < attacks; i++)
            {
                if (enemy.Crit > 0)
                {
                    basicDmg += enemy.GetAutoAttackDamage(player) * (1 + enemy.Crit / attacks);
                }
                else
                {
                    basicDmg += enemy.GetAutoAttackDamage(player);
                }
            }
            result += basicDmg;
            var spells = enemy.Spellbook.Spells;
            foreach (var spell in spells)
            {
                var t = spell.CooldownExpires - Game.Time;
                if (t < 0.5)
                {
                    switch (enemy.SkinName)
                    {
                        case "Ahri":
                            if (spell.Slot == SpellSlot.Q)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot));
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot, 1));
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Akali":
                            if (spell.Slot == SpellSlot.R)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot) * spell.Ammo);
                            }
                            else if (spell.Slot == SpellSlot.Q)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot));
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot, 1));
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Amumu":
                            if (spell.Slot == SpellSlot.W)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot) * 5);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Cassiopeia":
                            if (spell.Slot == SpellSlot.Q || spell.Slot == SpellSlot.E || spell.Slot == SpellSlot.W)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot) * 2);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Fiddlesticks":
                            if (spell.Slot == SpellSlot.W || spell.Slot == SpellSlot.E)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot) * 5);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Garen":
                            if (spell.Slot == SpellSlot.E)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot) * 3);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Irelia":
                            if (spell.Slot == SpellSlot.W)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot) * attacks);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Karthus":
                            if (spell.Slot == SpellSlot.Q)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot) * 4);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "KogMaw":
                            if (spell.Slot == SpellSlot.W)
                            {
                                result += (Damage.GetSpellDamage(enemy, player, spell.Slot) * attacks);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "LeeSin":
                            if (spell.Slot == SpellSlot.Q)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot, 1);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Lucian":
                            if (spell.Slot == SpellSlot.R)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot) * 4;
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Nunu":
                            if (spell.Slot != SpellSlot.R && spell.Slot != SpellSlot.Q)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "MasterYi":
                            if (spell.Slot != SpellSlot.E)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot) * attacks;
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "MonkeyKing":
                            if (spell.Slot != SpellSlot.R)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot) * 4;
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Pantheon":
                            if (spell.Slot == SpellSlot.E)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot) * 3;
                            }
                            else if (spell.Slot == SpellSlot.R)
                            {
                                result += 0;
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }

                            break;
                        case "Rammus":
                            if (spell.Slot == SpellSlot.R)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot) * 6;
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Riven":
                            if (spell.Slot == SpellSlot.Q)
                            {
                                result += RivenDamageQ(spell, enemy, player);
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Viktor":
                            if (spell.Slot == SpellSlot.R)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot, 1) * 5;
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        case "Vladimir":
                            if (spell.Slot == SpellSlot.E)
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot) * 2;
                            }
                            else
                            {
                                result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            }
                            break;
                        default:
                            result += Damage.GetSpellDamage(enemy, player, spell.Slot);
                            break;
                    }
                }
            }
            if (enemy.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready)
            {
                result += enemy.GetSummonerSpellDamage(player, Damage.SummonerSpell.Ignite);
            }
            foreach (var minions in
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(i => i.Distance(player.Position) < 750 && i.IsMinion && !i.IsAlly && !i.IsDead))
            {
                result += minions.GetAutoAttackDamage(player, false);
            }
            return (float) result;
        }

        public static bool HasDef(Obj_AI_Hero target)
        {
            foreach (SpellDataInst spell in target.Spellbook.Spells)
            {
                if (defSpells.Contains(spell.Name) && (spell.CooldownExpires - Game.Time) < 0)
                {
                    return true;
                }
            }
            foreach (var item in target.InventoryItems)
            {
                if (defItems.Contains((int) item.Id))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool isTargetedCC(string Spellname)
        {
            return TargetedCC.Contains(Spellname);
        }

        public static bool IsPossibleToReachHim(Obj_AI_Hero target, float moveSpeedBuff, float duration)
        {
            var distance = player.Distance(target);
            var diff = Math.Abs((player.MoveSpeed * (1 + moveSpeedBuff)) - target.MoveSpeed);
            if (diff * duration > distance)
            {
                return true;
            }
            return false;
        }

        public static bool IsPossibleToReachHim2(Obj_AI_Hero target, float moveSpeedBuff, float duration)
        {
            var distance = player.Distance(target);
            if (player.MoveSpeed * (1 + moveSpeedBuff) * duration > distance)
            {
                return true;
            }
            return false;
        }

        public static bool IsAutoattack(string spellName)
        {
            if (autoAttacks.Contains(spellName))
            {
                return true;
            }
            return false;
        }

        public static bool CheckInterrupt(Vector3 pos, float range)
        {
            return
                !HeroManager.Enemies.Any(
                    e =>
                        e.Distance(pos) < range &&
                        (e.HasBuff("GarenQ") || e.HasBuff("powerfist") || e.HasBuff("JaxCounterStrike") ||
                         e.HasBuff("PowerBall") || e.HasBuff("renektonpreexecute") || e.HasBuff("xenzhaocombotarget") ||
                         (e.HasBuff("UdyrBearStance") && !player.HasBuff("UdyrBearStunCheck"))));
        }

        public static float GetBuffTime(BuffInstance buff)
        {
            return (float) buff.EndTime - Game.ClockTime;
        }

        public static float IgniteDamage(Obj_AI_Hero target)
        {

            var igniteBuff =
                target.Buffs.Where(buff => buff.Name == "summonerdot").OrderBy(buff => buff.StartTime).FirstOrDefault();
            if (igniteBuff == null)
            {
                return 0;
            }
            else
            {
                var igniteDamage = Math.Floor(igniteBuff.EndTime - Game.ClockTime) *
                                   ((Obj_AI_Hero) igniteBuff.Caster).GetSummonerSpellDamage(
                                       target, Damage.SummonerSpell.Ignite) / 5;
                return (float) igniteDamage;
            }
        }


        #endregion

        internal static int CountEnemiesInRangeAfterTime(Vector3 pos, float range, float delay, bool nowToo)
        {
            var enemies = (from h in HeroManager.Enemies
                let pred = Prediction.GetPrediction(h, delay)
                where pred.UnitPosition.Distance(pos) < range
                select h);
            return nowToo ? enemies.Count(h => h.Distance(pos) < range) : enemies.Count();
        }

        public static bool isDangerousSpell(string spellName,
            Obj_AI_Hero target,
            Obj_AI_Hero hero,
            Vector3 end,
            float spellRange)
        {
            if (spellName == "CurseofTheSadMummy")
            {
                if (player.Distance(hero.Position) <= 600f)
                {
                    return true;
                }
            }
            if (CombatHelper.IsFacing(target, player.Position) &&
                (spellName == "EnchantedCrystalArrow" || spellName == "rivenizunablade" ||
                 spellName == "EzrealTrueshotBarrage" || spellName == "JinxR" || spellName == "sejuaniglacialprison"))
            {
                if (player.Distance(hero.Position) <= spellRange - 60)
                {
                    return true;
                }
            }
            if (spellName == "InfernalGuardian" || spellName == "UFSlash" ||
                (spellName == "RivenW" && player.HealthPercent < 25))
            {
                if (player.Distance(end) <= 270f)
                {
                    return true;
                }
            }
            if (spellName == "BlindMonkRKick" || spellName == "SyndraR" || spellName == "VeigarPrimordialBurst" ||
                spellName == "AlZaharNetherGrasp" || spellName == "LissandraR")
            {
                if (target.IsMe)
                {
                    return true;
                }
            }
            if (spellName == "TristanaR" || spellName == "ViR")
            {
                if (target.IsMe || player.Distance(target.Position) <= 100f)
                {
                    return true;
                }
            }
            if (spellName == "GalioIdolOfDurand")
            {
                if (player.Distance(hero.Position) <= 600f)
                {
                    return true;
                }
            }
            if (target != null && target.IsMe)
            {
                if (CombatHelper.isTargetedCC(spellName) && spellName != "NasusW" && spellName != "ZedUlt")
                {
                    return true;
                }
            }
            return false;
        }
    }
}