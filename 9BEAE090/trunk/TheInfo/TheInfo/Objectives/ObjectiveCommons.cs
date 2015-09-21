using System;
using System.Linq;
using LeagueSharp;
using SharpDX;

namespace TheInfo.Objectives
{
    public static class ObjectiveCommons
    {
        public static int TeamSize;
        private static readonly string[] ChampionsGoodAgainstTowers = /* Champions that are good against towers (special abilities)*/ { "Diana", "Akali", "Garen", "Lucian", "Master Yi", "Nidalee", "Shen", "Shyvana", "Sona", "Twisted Fate", "Vi", "Wukong", "Ziggs", "Blitzcrank" };
        private const float MediumHeroModifier = 1.15f;
        private static readonly string[] ChampionsVeryGoodAgainstTowers = /* Champions that are very good against towers (special abilities)*/ { "Nasus", "Xin Zhao", "Trundle" };
        private const float GoodHeroModifier = 1.4f;
        private static int _lastEnemyDragonStacks;

        static ObjectiveCommons()
        {
            TeamSize = ObjectManager.Get<Obj_AI_Hero>().Count(hero => hero.Team == ObjectManager.Player.Team);
        }

        public static float GetEstimatedTowerDamage(Obj_AI_Hero hero, bool hasMinions = true)
        {
            var dmg = hero.BaseAttackDamage + Math.Max(hero.TotalAttackDamage - hero.BaseAttackDamage, hero.TotalMagicalDamage * 0.4f);
            if (ChampionsGoodAgainstTowers.Contains(hero.ChampionName))
                dmg *= MediumHeroModifier;
            else if (ChampionsVeryGoodAgainstTowers.Contains(hero.ChampionName))
                dmg *= GoodHeroModifier;
            return dmg * (100f / (100f + (hasMinions ? 100 : 300))); // armor/mr function
        }

        public static float GetNeededMoveTime(Obj_AI_Hero hero, Vector3 moveTo)
        {
            return (moveTo - hero.Position).Length() * 1.2f/hero.MoveSpeed; //1.2 is a dirty ~ estimate to get the additional time needed to travel throught he jungle. Cause walls and stuffs
        }

        public static float GetNeededMoveTimeWithoutMs(Obj_AI_Hero hero, Vector3 moveTo)
        {
            return (moveTo - hero.Position).Length() * 1.2f; //1.2 is a dirty ~ estimate to get the additional time needed to travel throught he jungle. Cause walls and stuffs
        }

        public static float GetNeededMoveTimeHeavy(Obj_AI_Hero hero, Vector3 moveTo)
        {
            var path = hero.GetPath(moveTo);
            var time = (path[0] - hero.Position).Length() / hero.MoveSpeed;
            for (int i = 1; i < path.Length; i++)
                time += (path[i] - path[i - 1]).Length() / hero.MoveSpeed;
            return time;
        }

        public static float GetNeededMoveTimeWithoutMsHeavy(Obj_AI_Hero hero, Vector3 moveTo)
        {
            Console.WriteLine(hero.Name);
            var path = hero.GetPath(moveTo);
            var time = (path[0] - hero.Position).Length();
            for (int i = 1; i < path.Length; i++)
                time += (path[i] - path[i - 1]).Length();
            return time;
        }

        public static int GetEnemyDragonStacks()
        {
            foreach (var player in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (!player.IsVisible || !player.IsEnemy) continue;
                var buff = player.Buffs.FirstOrDefault(x => x.Name == "s5test_dragonslayerbuff");
                if (buff != null)
                    _lastEnemyDragonStacks = buff.Count;
                break;
            }
            return _lastEnemyDragonStacks;
        }

        public static int GetAllyDragonStacks()
        {
            var dragBuff = ObjectManager.Player.Buffs.FirstOrDefault(buff => buff.Name == "s5test_dragonslayerbuff");
            return dragBuff == null ? 0 : dragBuff.Count;
        }

        public static bool HasAnyoneNashorBuff()
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var player in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (!player.IsVisible) continue;
                var buff = player.Buffs.FirstOrDefault(x => x.Name == "exaltedwithbaronnashor");
                if (buff != null)
                    return true;
            }
            return false;
        }
    }
}
