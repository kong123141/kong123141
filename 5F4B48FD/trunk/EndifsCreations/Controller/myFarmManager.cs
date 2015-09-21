#region Todo
    //      Explore more methods.
    //      Block spellcast last hitting
#endregion Todo

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace EndifsCreations.Controller
{
    internal class myFarmManager : PluginData
    {
        private static Vector3 Pos;
        private static PredictionOutput Pred;

        public enum ObjectOrderTypes
        {
            None,
            NearestToPlayer,
            NearestToMouse
        }
                
        //  <summary>
        //      Strings of neutral minion base skin names
        //  </summary>
        public static readonly string[] NeutralCampList =
        {
            "SRU_Baron", "SRU_Dragon", "SRU_Blue", "SRU_Red", "SRU_Gromp", "SRU_Murkwolf", "SRU_Krug", "SRU_Razorbeak", "SRU_Crab",
            "TT_Spiderboss", "TT_NGolem", "TT_NWolf", "TT_NWraith"
        };

        //  <summary>
        //      Filters NeutralCampList to get legendary, epic, buffs neutral minions
        //  </summary>
        public static IEnumerable<Obj_AI_Base> GetLargeMonsters(Vector3 position, float range)
        {
            return MinionManager.GetMinions(position, range, MinionTypes.All, MinionTeam.Neutral)
                .Where(
                x => 
                    NeutralCampList.Contains(x.CharData.BaseSkinName) &&
                    ((Game.MapId == GameMapId.SummonersRift && !x.CharData.BaseSkinName.Contains("Mini")) ||
                    (Game.MapId == GameMapId.TwistedTreeline && !x.CharData.BaseSkinName.Contains("2")))
                );
        }           

        //  <summary>
        //      Filters minion lists to get Cannon, Siege, and Super minions
        //  </summary>
        public static IEnumerable<Obj_AI_Base> GetLargeMinions(float range)
        {
            return MinionManager.GetMinions(ObjectManager.Player.ServerPosition, range)
                .Where(
                x =>
                    x.IsValidTarget() && (
                    x.CharData.BaseSkinName.ToLower().Contains("super") ||
                    x.CharData.BaseSkinName.ToLower().Contains("siege") ||
                    x.CharData.BaseSkinName.ToLower().Contains("cannon"))
                );
        }
        
        //  <summary>
        //      Modified MinionManager
        //      Returns the minions in range from vector
        //      https://github.com/LeagueSharp/LeagueSharp.Common/blob/master/MinionManager.cs#L58                
        //  </summary>
        public static List<Obj_AI_Base> GetMinions(
            Vector3 from,
            float range,
            MinionTypes type = MinionTypes.All,
            MinionTeam team = MinionTeam.Enemy,
            MinionOrderTypes order = MinionOrderTypes.None, 
            bool includepets = false)
        {
            var result = (from minion in ObjectManager.Get<Obj_AI_Minion>()
                where minion.IsValidTarget(range, false, @from)
                let minionTeam = minion.Team
                where
                    team == MinionTeam.Neutral && minionTeam == GameObjectTeam.Neutral ||
                    team == MinionTeam.Ally && minionTeam == (ObjectManager.Player.Team == GameObjectTeam.Chaos ? GameObjectTeam.Chaos : GameObjectTeam.Order) ||
                    team == MinionTeam.Enemy && minionTeam == (ObjectManager.Player.Team == GameObjectTeam.Chaos ? GameObjectTeam.Order : GameObjectTeam.Chaos) ||
                    team == MinionTeam.NotAlly && minionTeam != ObjectManager.Player.Team || 
                    team == MinionTeam.NotAllyForEnemy && (minionTeam == ObjectManager.Player.Team || minionTeam == GameObjectTeam.Neutral) ||
                    team == MinionTeam.All
                where
                    minion.IsMelee() && type == MinionTypes.Melee || 
                    !minion.IsMelee() && type == MinionTypes.Ranged ||
                    type == MinionTypes.All
                where 
                    IsMinion(minion) ||
                    (includepets && IsPet(minion)) || 
                    minionTeam == GameObjectTeam.Neutral
                select minion).Cast<Obj_AI_Base>().ToList();

            switch (order)
            {
                case MinionOrderTypes.Health:
                    result = result.OrderBy(o => o.Health).ToList();
                    break;
                case MinionOrderTypes.MaxHealth:
                    result = result.OrderBy(o => o.MaxHealth).Reverse().ToList();
                    break;
                case MinionOrderTypes.None:
                    result = result.OrderBy(i => i.Distance(ObjectManager.Player)).ToList();
                    break;
            }
            return result;
        }

        public static bool IsMinion(Obj_AI_Minion minion)
        {
            var name = minion.CharData.BaseSkinName.ToLower();
            return 
                name.Contains("minion");
        }
        public static bool IsWard(Obj_AI_Minion minion)
        {
            var name = minion.CharData.BaseSkinName.ToLower();
            return 
                name.Contains("ward") || 
                name.Contains("trinket");
        }
         
        //https://github.com/LeagueSharp/LeagueSharp.Common/commit/e2be4e013bc83a6c8054f41bdafc273f32979492

        private static readonly string[] PetsList =
        {
            "gangplankbarrel", "elisespiderling", "malzaharvoidling", "kalistaspawn", "annietibbers", "teemomushroom", "shacobox",
            "zyrathornplant", "zyragraspingplant", "heimertyellow", "heimertblue", "yorickspectralghoul", "yorickdecayedghoul", "yorickravenousghoul"
        };
        public static bool IsPet(Obj_AI_Minion minion)
        {
            return PetsList.Contains(minion.CharData.BaseSkinName.ToLower());
        }
        
        //  <summary>
        //      Returns the game object Neutral Minion Camp in range from vector
        //  </summary>        
        public static List<GameObject> GetNearestCamp(Vector3 from, 
            float range, 
            ObjectOrderTypes order = 
            ObjectOrderTypes.None)
        {
            var result = (from obj in ObjectManager.Get<GameObject>()
                          where 
                          obj.Type == GameObjectType.NeutralMinionCamp && 
                          obj.Name.Contains("monsterCamp_") && 
                          Vector3.Distance(obj.Position, Player.Position) <= range
                          select obj).ToList();
            switch (order)
            {
                case ObjectOrderTypes.NearestToMouse:
                    result = result.OrderBy(o => Vector3.Distance(o.Position, Game.CursorPos)).ToList();
                    break;
                case ObjectOrderTypes.NearestToPlayer:
                    result = result.OrderBy(o => Vector3.Distance(o.Position, Player.Position)).ToList();
                    break;                
            }
            return result;
        }

        //  <summary>
        //      Declare list
        //  </summary>
        private static List<Obj_AI_Base> Minions, Sieges, Killable = new List<Obj_AI_Base>();
        private static List<Obj_AI_Base> JungleMinions = new List<Obj_AI_Base>();
        private static List<GameObject> NeutralMinionCamp = new List<GameObject>();

        #region Laning
        //  <summary>
        //      GetBestLineFarmLocation
        //      https://github.com/LeagueSharp/LeagueSharp.Common/blob/master/MinionManager.cs#L177  
        //  </summary>
        public static void LaneLinear(Spell spell, float castrange = 0, bool max = false, bool collision = false, int passable = 0)
        {
            Minions = GetMinions(ObjectManager.Player.Position, castrange);
            switch (collision)
            {
                case true:
                    var test = Minions.Where(m => !m.IsDead && spell.IsKillable(m) && Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= spell.Range);
                    if (ObjectManager.Player.ChampionName == "Veigar")
                    {
                        foreach (var loop in test)
                        {
                            Pred = spell.GetPrediction(loop, false, castrange);

                            var frontbox = new Geometry.Polygon.Rectangle(Player.Position, Pred.CastPosition, spell.Width);
                            var backbox = new Geometry.Polygon.Rectangle(Pred.CastPosition, Player.Position.Extend(Pred.CastPosition, spell.Range), spell.Width);

                            if (Pred.CollisionObjects.Count == 0)
                            {
                                if (backbox.Points.Any(
                                    point => 
                                        Minions.Where(x => x != loop && !x.IsDead && spell.IsKillable(x))
                                        .Select(m => m.ServerPosition.To2D()).ToList().Any()))
                                {
                                    spell.Cast(Pred.CastPosition);
                                }
                            }
                            else if (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable)
                            {
                                if (frontbox.Points.Any(
                                    point => 
                                        Minions.Where(x => x != loop && !x.IsDead && spell.IsKillable(x))
                                        .Select(m => m.ServerPosition.To2D()).ToList().Any()))
                                {
                                    spell.Cast(Pred.CastPosition);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var loop in test)
                        {
                            Pred = spell.GetPrediction(loop, false, castrange);
                            if (Pred.CollisionObjects.Count <= passable && !Player.Spellbook.IsAutoAttacking)
                            {
                                spell.Cast(loop);
                            }
                        }
                    }
                    break;
                case false:
                    if (Minions.Count() <= 1) return;
                    var result = MinionManager.GetBestLineFarmLocation(Minions.Where(m => Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= spell.Range).Select(m => m.ServerPosition.To2D()).ToList(), spell.Width, castrange);
                    if (Vector2.Distance(result.Position, Player.Position.To2D()) <= castrange)
                    {
                        if (max)
                        {
                            if (Minions.Count() >= result.MinionsHit && Minions.Count() >= 6)
                            {
                                spell.Cast(result.Position);
                            }
                        }
                        if (Minions.Count() >= result.MinionsHit && Minions.Count() >= 6)
                        {
                            spell.Cast(result.Position);
                        }
                        else if (result.MinionsHit >= (Minions.Count() * 3 / 4) && result.MinionsHit >= 3)
                        {
                            spell.Cast(result.Position);
                        }
                        else if (result.MinionsHit >= (Minions.Count() * 1 / 3) && result.MinionsHit >= 3)
                        {
                            spell.Cast(result.Position);
                        }
                        else
                        {
                            //Console.WriteLine("Skipping. Hit: " + result.MinionsHit + " Count: " + Minions.Count());
                        }
                    }
                    break;
            }
        }

        //  <summary>
        //      GetBestCircularFarmLocation
        //      https://github.com/LeagueSharp/LeagueSharp.Common/blob/master/MinionManager.cs#L120      
        //  </summary>
        public static void LaneCircular(Spell spell, float castrange = 0, float radius = 0)
        {
            Minions = GetMinions(ObjectManager.Player.Position, castrange);
            if (Minions.Count() <= 1) return;
            var result = MinionManager.GetBestCircularFarmLocation(Minions.Where(m => Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= spell.Range).Select(m => m.ServerPosition.To2D()).ToList(), radius, castrange);
            if (Vector2.Distance(result.Position, Player.Position.To2D()) <= castrange)
            {
                if (Minions.Count() >= result.MinionsHit && Minions.Count() >= 6)
                {
                    spell.Cast(result.Position);
                }
                else if (result.MinionsHit >= (Minions.Count() * 3 / 4) && result.MinionsHit >= 3)
                {
                    spell.Cast(result.Position);
                }
                else if (result.MinionsHit >= (Minions.Count() * 1 / 3) && result.MinionsHit >= 3)
                {
                    spell.Cast(result.Position);
                }
                else
                {
                    //Console.WriteLine("Skipping. Hit: " + result.MinionsHit + " Count: " + Minions.Count());
                }
            }
        }

        //  <summary>
        //      Last Hit with spells (not on hit aa spells)
        //  </summary>
        public static void LaneLastHit(Spell spell, float castrange = 0, List<Obj_AI_Base> list = null, bool disableattack = false)
        {
            var disabled = false;
            if (disableattack)
            {
                myOrbwalker.SetAttack(false);
                disabled = true;
            }
            if (list == null)
            {
                Minions = GetMinions(ObjectManager.Player.Position, castrange)
                    .Where(
                    x => 
                        spell.IsKillable(x) && 
                        x.NetworkId == myOrbwalker.LaneClearID ||
                        Player.IsMelee() && x.NetworkId != myOrbwalker.LastHitID ||
                        !Player.IsMelee() && x.NetworkId == myOrbwalker.LastHitID)
                        .ToList();
                Sieges = Minions
                    .Where(
                    x =>
                        x.IsValidTarget() && (
                        x.CharData.BaseSkinName.ToLower().Contains("super") ||
                        x.CharData.BaseSkinName.ToLower().Contains("siege") ||
                        x.CharData.BaseSkinName.ToLower().Contains("cannon")))
                    .OrderBy(i => i.Health).ToList();
                if (Sieges.Any())
                {
                    foreach (var x in Sieges)
                    {
                        spell.Cast(x);                        
                    }
                }
                else
                {
                    foreach (var x in Minions)
                    {
                        spell.Cast(x);
                    }
                }
            }
            else
            {
                foreach (var x in list
                    .Where(
                    x => 
                        spell.IsKillable(x) &&
                        x.NetworkId == myOrbwalker.LaneClearID ||
                        Player.IsMelee() && x.NetworkId != myOrbwalker.LastHitID ||
                        !Player.IsMelee() && x.NetworkId == myOrbwalker.LastHitID))
                {
                    spell.Cast(x);
                }
            }
            if (disabled) { myOrbwalker.SetAttack(true); }            
        }

        //  <summary>
        //      GetBestLineFarmLocation, targetted
        //  </summary>
        public static void LaneLinearTargetted(Spell spell, float castrange = 0, float width = 0, float extend = 0, bool front = true, bool back = true)
        {
            Minions = GetMinions(ObjectManager.Player.Position, castrange + extend);
            Geometry.Polygon.Rectangle frontbox;
            Geometry.Polygon.Rectangle backbox;
            var total = 0;
            var max = 0;
            foreach (var loop in Minions)
            {             
                if (front)
                {
                     frontbox = new Geometry.Polygon.Rectangle(Player.Position, loop.Position, width);
                     total += MinionManager.GetMinions(castrange).Where(x => x != loop).Count(x => frontbox.IsInside(x));
                     max += 3;
                }
                if (back)
                {
                    backbox = new Geometry.Polygon.Rectangle(loop.Position, Player.Position.Extend(loop.Position, castrange) + extend, width);
                    total += MinionManager.GetMinions(castrange + extend).Where(x => x != loop).Count(x => backbox.IsInside(x));
                    max += 3;
                }                                                
                if (Minions.Count() >= total && Minions.Count() >= max)
                {
                    spell.Cast(loop);
                }
                else if (total >= Math.Abs((Minions.Count() * 3 / 4)) && total >= Math.Max(3,(max/2)))
                {
                    spell.Cast(loop);  
                }
                else
                {
                    //Console.WriteLine("Skipping. Hit: " + total + " Count: " + Minions.Count());
                }                
            }
        }
        #endregion Laning

        #region Jungling
        //  <summary>
        //      GetBestLineFarmLocation, max jungle minion then -1.
        //  </summary>
        public static void JungleLinear(Spell spell, float castrange = 0, bool aoe = false, bool collision = false, int passable = 0, ObjectOrderTypes order = ObjectOrderTypes.None)
        {
            NeutralMinionCamp = GetNearestCamp(Player.Position, castrange, order);
            if (NeutralMinionCamp.Any())
            {
                var camp = NeutralMinionCamp[0];
                JungleMinions = GetMinions(NeutralMinionCamp[0].Position, castrange,MinionTypes.All,MinionTeam.Neutral);
                var result = MinionManager.GetBestLineFarmLocation(JungleMinions.Where(m => Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= spell.Range).Select(m => m.ServerPosition.To2D()).ToList(), spell.Width, castrange);
                switch (camp.Name)
                {
                    //Baron
                    case "monsterCamp_12":
                        var baron = GetLargeMonsters(camp.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Baron"));
                        if (baron != null && baron.IsValidTarget())
                        {
                            if (collision)
                            {
                                Pred = spell.GetPrediction(baron, aoe, castrange);
                                if (Pred.CollisionObjects.Count == 0)
                                {
                                    spell.Cast(Pred.CastPosition);
                                }
                                else if (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable)
                                {
                                    spell.Cast(Pred.CastPosition);
                                }
                            }
                            else
                            {
                                spell.Cast(baron.ServerPosition);
                            }
                           
                        }
                        break;

                    //Dragon
                    case "monsterCamp_6":
                        var dragon = GetLargeMonsters(camp.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Dragon"));
                        if (dragon != null && dragon.IsValidTarget())
                        {
                            if (collision)
                            {
                                Pred = spell.GetPrediction(dragon, aoe, castrange);
                                if (Pred.CollisionObjects.Count == 0)
                                {
                                    spell.Cast(Pred.CastPosition);
                                }
                                else if (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable)
                                {
                                    spell.Cast(Pred.CastPosition);
                                }
                            }
                            else
                            {
                                spell.Cast(dragon.ServerPosition);
                            }
                        }
                        break;

                    //Red Brambleback                        
                    case "monsterCamp_4":
                    case "monsterCamp_10":
                        var bramblebackl = GetLargeMonsters(Player.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Red"));
                        if (bramblebackl != null && bramblebackl.IsValidTarget())
                        {
                            switch (collision)
                            {
                                case true:
                                    Pred = spell.GetPrediction(bramblebackl, aoe, castrange);
                                    if (Pred.CollisionObjects.Count == 0)
                                    {
                                        spell.Cast(Pred.CastPosition);
                                    }
                                    else if (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable)
                                    {
                                        spell.Cast(Pred.CastPosition);
                                    }
                                    break;
                                case false:
                                    if (JungleMinions.Count() >= 3 && result.MinionsHit >= 2)
                                    {
                                        spell.Cast(result.Position);
                                    }
                                    break;
                            }
                        }
                        break;

                    //Blue Sentinel
                    case "monsterCamp_1":
                    case "monsterCamp_7":
                        var sentinell = GetLargeMonsters(camp.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Blue"));
                        if (sentinell != null && sentinell.IsValidTarget())
                        {
                            switch (collision)
                            {
                                case true:
                                    Pred = spell.GetPrediction(sentinell, aoe, castrange);
                                    if (Pred.CollisionObjects.Count == 0)
                                    {
                                        spell.Cast(Pred.CastPosition);
                                    }
                                    else if (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable)
                                    {
                                        spell.Cast(Pred.CastPosition);
                                    }
                                    break;
                                case false:
                                    if (JungleMinions.Count() >= 3 && result.MinionsHit >= 2)
                                    {
                                        spell.Cast(result.Position);
                                    }
                                    break;
                            }
                        }
                        break;

                    //Gromp
                    case "monsterCamp_13":
                    case "monsterCamp_14":
                        var gromp = GetLargeMonsters(camp.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Gromp"));
                        if (gromp != null && gromp.IsValidTarget())
                        {
                            if (collision)
                            {
                                Pred = spell.GetPrediction(gromp, aoe, castrange);
                                if (Pred.CollisionObjects.Count == 0)
                                {
                                    spell.Cast(Pred.CastPosition);
                                }
                                else if (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable)
                                {
                                    spell.Cast(Pred.CastPosition);
                                }
                            }
                            else
                            {
                                spell.Cast(gromp.ServerPosition);
                            }
                           
                        }                        
                        break;

                    //Krugs
                    case "monsterCamp_5":
                    case "monsterCamp_11":
                        var krugl = GetLargeMonsters(camp.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Krug"));
                        if (krugl != null && krugl.IsValidTarget())
                        {
                            switch (collision)
                            {
                                case true:
                                    Pred = spell.GetPrediction(krugl, aoe, castrange);
                                    if (Pred.CollisionObjects.Count == 0)
                                    {
                                        spell.Cast(Pred.CastPosition);
                                    }
                                    else if (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable)
                                    {
                                        spell.Cast(Pred.CastPosition);
                                    }
                                    break;
                                case false:
                                    if (JungleMinions.Count() >= 2 && result.MinionsHit >= 2)
                                    {
                                        spell.Cast(result.Position);
                                    }
                                    break;
                            }
                        }
                        break;

                    //wolf
                    case "monsterCamp_2":
                    case "monsterCamp_8":
                        var murkwolfl = GetLargeMonsters(camp.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Murkwolf"));
                        if (murkwolfl != null && murkwolfl.IsValidTarget())
                        {
                            switch (collision)
                            {
                                case true:
                                    Pred = spell.GetPrediction(murkwolfl, aoe, castrange);
                                    if (Pred.CollisionObjects.Count == 0)
                                    {
                                        spell.Cast(Pred.CastPosition);
                                    }
                                    else if (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable)
                                    {
                                        spell.Cast(Pred.CastPosition);
                                    }
                                    break;
                                case false:
                                    if (JungleMinions.Count() >= 3 && result.MinionsHit >= 2)
                                    {
                                        spell.Cast(result.Position);
                                    }
                                    break;
                            }
                        }
                        break;

                    //Razorbeak
                    case "monsterCamp_3":
                    case "monsterCamp_9":
                        var razorbeak = GetLargeMonsters(camp.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Razorbeak"));
                        if (razorbeak != null && razorbeak.IsValidTarget())
                        {
                            switch (collision)
                            {
                                case true:
                                    Pred = spell.GetPrediction(razorbeak, aoe, castrange);
                                    if (Pred.CollisionObjects.Count == 0)
                                    {
                                        spell.Cast(Pred.CastPosition);
                                    }
                                    else if (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable)
                                    {
                                        spell.Cast(Pred.CastPosition);
                                    }
                                    break;
                                case false:
                                    if (JungleMinions.Count() >= 4 && result.MinionsHit >= 4)
                                    {
                                        spell.Cast(result.Position);
                                    }
                                    break;
                            }
                        }
                        break;

                    //Crab
                    case "monsterCamp_15":
                    case "monsterCamp_16":
                        var crab = GetLargeMonsters(Player.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Crab"));
                        if (crab != null && crab.IsValidTarget())
                        {
                            if (collision)
                            {
                                Pred = spell.GetPrediction(crab, aoe, castrange);
                                if (Pred.CollisionObjects.Count == 0)
                                {
                                    spell.Cast(Pred.CastPosition);
                                }
                                else if (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable)
                                {
                                    spell.Cast(Pred.CastPosition);
                                }
                            }
                            else
                            {
                                spell.Cast(crab.ServerPosition);
                            }
                           
                        }
                        break;
                }
            }
        }

        //  <summary>
        //      GetBestCircularFarmLocation, max jungle minion then -1.
        //      Single = randomized
        //  </summary>
        public static void JungleCircular(Spell spell, float castrange = 0, float radius = 0, ObjectOrderTypes order = ObjectOrderTypes.None)
        {
            NeutralMinionCamp = GetNearestCamp(Player.Position, castrange, order);
            if (NeutralMinionCamp.Any())
            {
                var camp = NeutralMinionCamp[0];
                JungleMinions = GetMinions(NeutralMinionCamp[0].Position, castrange, MinionTypes.All, MinionTeam.Neutral);
                var result = MinionManager.GetBestCircularFarmLocation(JungleMinions.Where(m => Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= spell.Range).Select(m => m.ServerPosition.To2D()).ToList(), radius, castrange);
                switch (camp.Name)
                {
                    //Baron
                    case "monsterCamp_12":
                        var baron = GetLargeMonsters(camp.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Baron"));
                        if (baron != null && baron.IsValidTarget())
                        {
                            Pred = spell.GetPrediction(baron);
                            if (Vector3.Distance(ObjectManager.Player.ServerPosition, Pred.CastPosition) <= spell.Range + radius)
                            {
                                if (Pred.Hitchance >= HitChance.High)
                                {
                                    Pos = myUtility.RandomPos(1, 25, 25, Pred.CastPosition.Extend(Player.ServerPosition, radius));
                                    spell.Cast(Pos);
                                }
                            }
                        }
                        break;

                    //Dragon
                    case "monsterCamp_6":
                        var dragon = GetLargeMonsters(camp.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Dragon"));
                        if (dragon != null && dragon.IsValidTarget())
                        {
                            Pred = spell.GetPrediction(dragon);
                            if (Vector3.Distance(ObjectManager.Player.ServerPosition, Pred.CastPosition) <= spell.Range + radius)
                            {
                                if (Pred.Hitchance >= HitChance.High)
                                {
                                    Pos = myUtility.RandomPos(1, 25, 25, Pred.CastPosition.Extend(Player.ServerPosition, radius));
                                    spell.Cast(Pos);
                                }
                            }
                        }
                        break;

                    //Red Brambleback                        
                    case "monsterCamp_4":
                    case "monsterCamp_10":
                        var bramblebackl = GetLargeMonsters(Player.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Red"));
                        if (bramblebackl != null && bramblebackl.IsValidTarget())
                        {
                            if (Vector2.Distance(result.Position, Player.Position.To2D()) <= castrange)
                            {
                                if (Minions.Count() >= result.MinionsHit && Minions.Count() >= 3)
                                {
                                    spell.Cast(result.Position);
                                }
                                else if (Minions.Count() >= 2 && result.MinionsHit >= 2)
                                {
                                    spell.Cast(result.Position);
                                }
                                else
                                {
                                    //Console.WriteLine("Skipping. Hit: " + result.MinionsHit + " Count: " + Minions.Count());
                                }
                            }
                        }
                        break;

                    //Blue Sentinel
                    case "monsterCamp_1":
                    case "monsterCamp_7":
                        var sentinell = GetLargeMonsters(camp.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Blue"));
                        if (sentinell != null && sentinell.IsValidTarget())
                        {
                            if (Vector2.Distance(result.Position, Player.Position.To2D()) <= castrange)
                            {
                                if (Minions.Count() >= result.MinionsHit && Minions.Count() >= 3)
                                {
                                    spell.Cast(result.Position);
                                }
                                else if (Minions.Count() >= 2 && result.MinionsHit >= 2)
                                {
                                    spell.Cast(result.Position);
                                }
                                else
                                {
                                    //Console.WriteLine("Skipping. Hit: " + result.MinionsHit + " Count: " + Minions.Count());
                                }
                            }
                        }
                        break;

                    //Gromp
                    case "monsterCamp_13":
                    case "monsterCamp_14":
                        var gromp = GetLargeMonsters(camp.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Gromp"));
                        if (gromp != null && gromp.IsValidTarget())
                        {
                            Pred = spell.GetPrediction(gromp);
                            if (Vector3.Distance(ObjectManager.Player.ServerPosition, Pred.CastPosition) <= spell.Range + radius)
                            {
                                if (Pred.Hitchance >= HitChance.High)
                                {
                                    Pos = myUtility.RandomPos(1, 25, 25, Pred.CastPosition.Extend(Player.ServerPosition, radius));
                                    spell.Cast(Pos);
                                }
                            }
                        }
                        break;

                    //Krugs
                    case "monsterCamp_5":
                    case "monsterCamp_11":
                        var krugl = GetLargeMonsters(camp.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Krug"));
                        if (krugl != null && krugl.IsValidTarget())
                        {
                            if (Minions.Count() >= result.MinionsHit && Minions.Count() >= 2)
                            {
                                spell.Cast(result.Position);
                            }
                            else
                            {
                                //Console.WriteLine("Skipping. Hit: " + result.MinionsHit + " Count: " + Minions.Count());
                            }
                        }
                        break;

                    //wolf
                    case "monsterCamp_2":
                    case "monsterCamp_8":
                        var murkwolfl = GetLargeMonsters(camp.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Murkwolf"));
                        if (murkwolfl != null && murkwolfl.IsValidTarget())
                        {
                            if (Minions.Count() >= result.MinionsHit && Minions.Count() >= 3)
                            {
                                spell.Cast(result.Position);
                            }
                            else
                            {
                                //Console.WriteLine("Skipping. Hit: " + result.MinionsHit + " Count: " + Minions.Count());
                            }
                        }
                        break;

                    //Razorbeak
                    case "monsterCamp_3":
                    case "monsterCamp_9":
                        var razorbeak = GetLargeMonsters(camp.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Razorbeak"));
                        if (razorbeak != null && razorbeak.IsValidTarget())
                        {
                            if (Minions.Count() >= result.MinionsHit && Minions.Count() >= 4)
                            {
                                spell.Cast(result.Position);
                            }
                            else if (Minions.Count() >= 3 && result.MinionsHit >= 3)
                            {
                                spell.Cast(result.Position);
                            }
                            else
                            {
                                //Console.WriteLine("Skipping. Hit: " + result.MinionsHit + " Count: " + Minions.Count());
                            }
                        }
                        break;

                    //Crab
                    case "monsterCamp_15":
                    case "monsterCamp_16":
                        var crab = GetLargeMonsters(Player.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Crab"));
                        if (crab != null && crab.IsValidTarget())
                        {
                            Pred = spell.GetPrediction(crab);
                            if (Vector3.Distance(ObjectManager.Player.ServerPosition, Pred.CastPosition) <= spell.Range + radius)
                            {
                                if (Pred.Hitchance >= HitChance.High)
                                {
                                    Pos = myUtility.RandomPos(1, 25, 25, Pred.CastPosition.Extend(Player.ServerPosition, radius));
                                    spell.Cast(Pos);
                                }
                            }
                        }
                        break;
                }
            }
        }
     
        public static void JungleLastHit(Spell spell, float castrange = 0)
        {
            //filters legendary, epic, buffs
            //+smite
        }

        #endregion Jungling
    }
}