#region Todo    
    //      Use HealthPrediction
    //      https://github.com/LeagueSharp/LeagueSharp.Common/blob/master/HealthPrediction.cs#L136
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

        //  <summary>
        //      Spell cast blocking
        //  </summary>
        private static bool AllowCasting = true;
        private static void SetAllow(bool value)
        {
            AllowCasting = value;            
        }
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
            "SRU_Baron", "SRU_Dragon", "SRU_Blue", "SRU_Red", "SRU_Gromp", "SRU_Murkwolf", "SRU_Krug", "SRU_Razorbeak", "Sru_Crab",
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
            return minion.CharData.BaseSkinName.ToLower().Contains("minion");
        }

        public static bool IsWard(Obj_AI_Minion minion)
        {
            var name = minion.CharData.BaseSkinName.ToLower();
            return  name.Contains("ward") || name.Contains("trinket");
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
            if (!AllowCasting) return;
            Minions = GetMinions(ObjectManager.Player.Position, castrange);
            switch (collision)
            {
                case true:
                    if (ObjectManager.Player.ChampionName == "Veigar")
                    {
                        myOrbwalker.SetAttack(false);
                        foreach (var testloop in Minions.Where(x => spell.IsKillable(x) && x.IsValidTarget() && !x.IsDead))
                        {
                            Pred = spell.GetPrediction(testloop, false, castrange);
                            var frontbox = new Geometry.Polygon.Rectangle(Player.ServerPosition, testloop.ServerPosition, spell.Width);
                            var backbox = new Geometry.Polygon.Rectangle(testloop.ServerPosition, Player.Position.Extend(testloop.ServerPosition, castrange), spell.Width);                           
                            if (Pred.CollisionObjects.Count == 0)
                            {
                                var next = Minions.Where(x => x != testloop && backbox.IsInside(x)).OrderBy(i => i.Distance(testloop)).FirstOrDefault();
                                if (next != null && spell.IsKillable(next))
                                {
                                    spell.Cast(Pred.CastPosition);
                                    //myDevTools.DebugMode("backbox next killable");
                                }
                                spell.Cast(Pred.CastPosition);
                                //myDevTools.DebugMode("backbox null");
                            }
                            if (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable)
                            {
                                var before = Minions.Where(x => x != testloop && frontbox.IsInside(x)).OrderByDescending(i => i.Distance(testloop));
                                myDevTools.DebugMode("before:" + before.Count());
                                if (before.Count() <= passable)
                                {
                                    spell.Cast(Pred.CastPosition);
                                    //myDevTools.DebugMode("before");
                                }
                            }
                        }
                        myOrbwalker.SetAttack(true); 
                    }
                    else
                    {
                        var test = Minions.Where(m => m.IsValidTarget() && m.IsHPBarRendered && !m.IsDead && spell.IsKillable(m) && Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= castrange);
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
                    var result = MinionManager.GetBestLineFarmLocation(Minions.Where(m => m.IsValidTarget() && m.IsHPBarRendered && !m.IsDead && Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= spell.Range).Select(m => m.ServerPosition.To2D()).ToList(), spell.Width, castrange);
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
                    }
                    break;
            }
        }

        //  <summary>
        //      Floor targetted spells
        //      GetBestCircularFarmLocation
        //      https://github.com/LeagueSharp/LeagueSharp.Common/blob/master/MinionManager.cs#L120      
        //  </summary>
        public static void LaneCircular(Spell spell, float castrange = 0, float radius = 0)
        {
            if (!AllowCasting) return;
            Minions = GetMinions(ObjectManager.Player.Position, castrange);
            if (Minions.Count() <= 1) return;
            var result = MinionManager.GetBestCircularFarmLocation(Minions.Where(m => m.IsValidTarget() && m.IsHPBarRendered && !m.IsDead && Vector3.Distance(m.ServerPosition, Player.ServerPosition) <= spell.Range).Select(m => m.ServerPosition.To2D()).ToList(), radius, castrange);
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
            }
        }

        //  <summary>
        //      Last Hit with spells (not on hit aa spells)
        //  </summary>
        public static void LaneLastHit(Spell spell, float castrange = 0, List<Obj_AI_Base> list = null, bool disableattack = false)
        {
            if (!AllowCasting) return;
            var disabled = false;
            if (disableattack)
            {
                myOrbwalker.SetAttack(false);
                disabled = true;
            }
            if (list == null && !Player.IsWindingUp)
            {
                Minions = GetMinions(ObjectManager.Player.Position, castrange)
                    .Where(
                    x => 
                        spell.IsKillable(x) &&
                        x.IsValidTarget() && x.IsHPBarRendered && !x.IsDead &&
                        (x.NetworkId == myOrbwalker.LaneClearID || x.NetworkId == myOrbwalker.LastHitID || x.NetworkId == myTowerAggro.TurretTargetID)
                        )
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
            else if (list != null && !Player.IsWindingUp)
            {
                Minions = list.Where(x => spell.IsKillable(x) && x.IsValidTarget() && x.IsHPBarRendered && !x.IsDead && (x.NetworkId == myOrbwalker.LaneClearID || x.NetworkId == myOrbwalker.LastHitID || x.NetworkId == myTowerAggro.TurretTargetID)).ToList();
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
            if (disabled) { myOrbwalker.SetAttack(true); }            
        }

        //  <summary>
        //      GetBestLineFarmLocation, targetted
        //  </summary>
        public static void LaneLinearTargetted(Spell spell, float castrange = 0, float width = 0, float extend = 0, bool front = true, bool back = true)
        {
            if (!AllowCasting) return;            
            Geometry.Polygon.Rectangle frontbox, backbox;
            var total = 0;
            var max = 0;
            Minions = GetMinions(ObjectManager.Player.Position, castrange + extend);
            foreach (var loop in Minions.Where(x => x.IsValidTarget() && !x.IsDead && x.IsHPBarRendered))
            {             
                if (front)
                {
                     frontbox = new Geometry.Polygon.Rectangle(Player.Position, loop.Position, width);
                     total += MinionManager.GetMinions(castrange).Where(x => x != loop && x.IsValidTarget() && x.IsHPBarRendered && !x.IsDead).Count(x => frontbox.IsInside(x));
                     max += 3;
                }
                if (back)
                {
                    backbox = new Geometry.Polygon.Rectangle(loop.Position, Player.Position.Extend(loop.Position, castrange) + extend, width);
                    total += MinionManager.GetMinions(castrange + extend).Where(x => x != loop && x.IsValidTarget() && x.IsHPBarRendered && !x.IsDead).Count(x => backbox.IsInside(x));
                    max += 3;
                }                                                
                if (Minions.Count() >= total && Minions.Count() >= max)
                {
                    spell.Cast(loop);
                }
                else if (total >= (Minions.Count() * 3 / 4) && total >= Math.Max(3,(max/2)))
                {
                    spell.Cast(loop);  
                }              
            }
        }

        //  <summary>
        //      PointBlank, AOE spells surrounding the player.
        //  </summary>
        public static void LanePointBlank(Spell spell, float radius, bool toggle = false, bool max = false)
        {
            if (!AllowCasting) return;
            Minions = GetMinions(ObjectManager.Player.Position, radius);
            if (Minions.Count() <= 1) return;
            switch (toggle)
            {
                case true:
                    switch (max)
                    {
                        case true:
                            break;
                        case false:
                            break;
                    }
                    break;
                case false:
                    switch (max)
                    {
                        case true:
                            if (Minions.Count() >= 6)
                            {
                                spell.Cast();
                            }
                            break;
                        case false:
                            if (Minions.Count() >= 6)
                            {
                                spell.Cast();
                                myDevTools.DebugMode("Minions.Count(): " + Minions.Count() + " 6");
                            }
                            else if (Minions.Count() >= (Minions.Count() * 3 / 4) && Minions.Count() >= 4)
                            {
                                spell.Cast();
                                myDevTools.DebugMode("Minions.Count(): " + Minions.Count() + " 3/4? " + Minions.Count() * 3 / 4);
                            }
                            else if (Minions.Count() >= (Minions.Count() * 1 / 3) && Minions.Count() >= 3)
                            {
                                spell.Cast();
                                myDevTools.DebugMode("Minions.Count(): " + Minions.Count() + " 1/3? " + Minions.Count() * 1 / 3);
                            }
                            break;
                    }
                    break;
            }
        }

        #endregion Laning

        #region Jungling
        //  <summary>
        //      GetBestLineFarmLocation, max jungle minion then -1.
        //  </summary>
        public static void JungleLinear(Spell spell, float castrange = 0, bool aoe = false, bool collision = false, int passable = 0, ObjectOrderTypes order = ObjectOrderTypes.None)
        {
            if (!AllowCasting) return;
            NeutralMinionCamp = GetNearestCamp(Player.Position, castrange, order);
            if (NeutralMinionCamp.Any())
            {
                var camp = NeutralMinionCamp[0];
                JungleMinions = GetMinions(camp.Position, castrange, MinionTypes.All, MinionTeam.Neutral);
                switch (camp.Name)
                {
                    #region Baron
                    case "monsterCamp_12":
                        var baron = GetLargeMonsters(camp.Position, castrange).Where(x =>x.IsValidTarget() && !x.IsDead).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Baron"));
                        if (baron != null && baron.IsValidTarget())
                        {
                            if (collision)
                            {
                                Pred = spell.GetPrediction(baron, aoe, castrange);
                                if (Pred.CollisionObjects.Count == 0 || (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable))
                                {
                                    if (Pred.Hitchance >= HitChance.High) spell.Cast(Pred.CastPosition);
                                }
                            }
                            else
                            {
                                spell.Cast(baron.ServerPosition);
                            }
                        }
                        break;
                    #endregion
                    #region Dragon
                    case "monsterCamp_6":
                        var dragon = GetLargeMonsters(camp.Position, castrange).Where(x => x.IsValidTarget() && !x.IsDead).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Dragon"));
                        if (dragon != null && dragon.IsValidTarget())
                        {
                            if (collision)
                            {
                                Pred = spell.GetPrediction(dragon, aoe, castrange);
                                if (Pred.CollisionObjects.Count == 0 || (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable))
                                {
                                    if (Pred.Hitchance >= HitChance.High) spell.Cast(Pred.CastPosition);
                                }
                            }
                            else
                            {
                                spell.Cast(dragon.ServerPosition);
                            }
                        }
                        break;
                    #endregion
                    #region Red Brambleback
                    case "monsterCamp_4":
                    case "monsterCamp_10":
                        var bramblebackl = GetLargeMonsters(Player.Position, castrange).Where(x => x.IsValidTarget() && !x.IsDead).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Red"));
                        var bramblebacks = JungleMinions.Where(x => x.IsValidTarget() && !x.IsDead &&x.CharData.BaseSkinName.Contains("SRU_Red")).ToList();
                        switch (collision)
                        {
                            case true:
                                if (bramblebackl != null && bramblebackl.IsValidTarget())
                                {
                                    Pred = spell.GetPrediction(bramblebackl, aoe, castrange);
                                    if (Pred.CollisionObjects.Count == 0 || (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable))
                                    {
                                        if (Pred.Hitchance >= HitChance.High) spell.Cast(Pred.CastPosition);
                                    }
                                }
                                else
                                {
                                    foreach (var loop in bramblebacks)
                                    {
                                        Pred = spell.GetPrediction(loop, aoe, castrange);
                                        if (Pred.CollisionObjects.Count == 0 || (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable))
                                        {
                                            if (Pred.Hitchance >= HitChance.High) spell.Cast(Pred.CastPosition);
                                        }
                                    }
                                }
                                break;
                            case false:
                                var result = MinionManager.GetBestLineFarmLocation(bramblebacks.Select(m => m.ServerPosition.To2D()).ToList(), spell.Width, castrange);
                                if (result.MinionsHit >= bramblebacks.Count() || result.MinionsHit >= Math.Max(1, bramblebacks.Count() - 1))
                                {
                                    spell.Cast(result.Position);
                                    myDevTools.DebugMode("[Red Bramblebacks] Hit: " + result.MinionsHit + " Count: " + bramblebacks.Count());
                                }
                                break;
                        }
                        break;
                    #endregion
                    #region Blue Sentinel
                    case "monsterCamp_1":
                    case "monsterCamp_7":
                        var sentinell = GetLargeMonsters(camp.Position, castrange).Where(x => x.IsValidTarget() && !x.IsDead).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Blue"));
                        var sentinels = JungleMinions.Where(x => x.IsValidTarget() && !x.IsDead && x.CharData.BaseSkinName.Contains("SRU_Blue")).ToList();
                        switch (collision)
                        {
                            case true:
                                if (sentinell != null && sentinell.IsValidTarget())
                                {
                                    Pred = spell.GetPrediction(sentinell, aoe, castrange);
                                    if (Pred.CollisionObjects.Count == 0 || (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable))
                                    {
                                        if (Pred.Hitchance >= HitChance.High) spell.Cast(Pred.CastPosition);
                                    }
                                }
                                else
                                {
                                    foreach (var loop in sentinels)
                                    {
                                        Pred = spell.GetPrediction(loop, aoe, castrange);
                                        if (Pred.CollisionObjects.Count == 0 || (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable))
                                        {
                                            if (Pred.Hitchance >= HitChance.High) spell.Cast(Pred.CastPosition);
                                        }
                                    }
                                }
                                break;
                            case false:
                                var result = MinionManager.GetBestLineFarmLocation(sentinels.Select(m => m.ServerPosition.To2D()).ToList(), spell.Width, castrange);
                                if (result.MinionsHit >= sentinels.Count() || result.MinionsHit >= Math.Max(1, sentinels.Count() - 1))
                                {
                                    spell.Cast(result.Position);
                                    myDevTools.DebugMode("[Blue Sentinels] Hit: " + result.MinionsHit + " Count: " + sentinels.Count());
                                }
                                break;
                        }
                        break;
                    #endregion
                    #region Gromp
                    case "monsterCamp_13":
                    case "monsterCamp_14":
                        var gromp = GetLargeMonsters(camp.Position, castrange).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Gromp"));
                        if (gromp != null && gromp.IsValidTarget())
                        {
                            if (collision)
                            {
                                Pred = spell.GetPrediction(gromp, aoe, castrange);
                                if (Pred.CollisionObjects.Count == 0 || (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable))
                                {
                                    if (Pred.Hitchance >= HitChance.High) spell.Cast(Pred.CastPosition);
                                }
                            }
                            else
                            {
                                spell.Cast(gromp.ServerPosition);
                            }
                           
                        }                        
                        break;
                    #endregion
                    #region Krugs
                    case "monsterCamp_5":
                    case "monsterCamp_11":
                        var krugl = GetLargeMonsters(camp.Position, castrange).Where(x => x.IsValidTarget() && !x.IsDead).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Krug"));
                        var krugs = JungleMinions.Where(x => x.IsValidTarget() && !x.IsDead && x.CharData.BaseSkinName.Contains("SRU_Krug")).ToList();
                        switch (collision)
                        {
                            case true:
                                if (krugl != null && krugl.IsValidTarget())
                                {
                                    Pred = spell.GetPrediction(krugl, aoe, castrange);
                                    if (Pred.CollisionObjects.Count == 0 || (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable))
                                    {
                                        if (Pred.Hitchance >= HitChance.High) spell.Cast(Pred.CastPosition);
                                    }
                                }
                                else
                                {
                                    foreach (var loop in krugs)
                                    {
                                        Pred = spell.GetPrediction(loop, aoe, castrange);
                                        if (Pred.CollisionObjects.Count == 0 || (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable))
                                        {
                                            if (Pred.Hitchance >= HitChance.High) spell.Cast(Pred.CastPosition);
                                        }
                                    }
                                }
                                break;
                            case false:
                                var result = MinionManager.GetBestLineFarmLocation(krugs.Select(m => m.ServerPosition.To2D()).ToList(), spell.Width, castrange);
                                if (krugl != null && krugl.IsValidTarget())
                                {
                                    spell.Cast(krugl.Position);
                                }
                                else
                                {
                                    spell.Cast(result.Position);
                                    myDevTools.DebugMode("[Krugs] Hit: " + result.MinionsHit + " Count: " + krugs.Count());
                                }
                                break;
                        }
                        break;
                    #endregion
                    #region Murkwolves
                    case "monsterCamp_2":
                    case "monsterCamp_8":
                        var murkwolfl = GetLargeMonsters(camp.Position, castrange).Where(x => x.IsValidTarget() && !x.IsDead).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Murkwolf"));
                        var murkwolfls = JungleMinions.Where(x => x.IsValidTarget() && !x.IsDead && x.CharData.BaseSkinName.Contains("SRU_Murkwolf")).ToList();
                        switch (collision)
                        {
                            case true:
                                if (murkwolfl != null && murkwolfl.IsValidTarget())
                                {
                                    Pred = spell.GetPrediction(murkwolfl, aoe, castrange);
                                    if (Pred.CollisionObjects.Count == 0 || (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable))
                                    {
                                        if (Pred.Hitchance >= HitChance.High) spell.Cast(Pred.CastPosition);
                                    }
                                }
                                else
                                {
                                    foreach (var loop in murkwolfls)
                                    {
                                        Pred = spell.GetPrediction(loop, aoe, castrange);
                                        if (Pred.CollisionObjects.Count == 0 || (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable))
                                        {
                                            if (Pred.Hitchance >= HitChance.High) spell.Cast(Pred.CastPosition);
                                        }
                                    }
                                }
                                break;
                            case false:
                                var result = MinionManager.GetBestLineFarmLocation(murkwolfls.Select(m => m.ServerPosition.To2D()).ToList(), spell.Width, castrange);
                                if (result.MinionsHit >= murkwolfls.Count() || result.MinionsHit >= Math.Max(1, murkwolfls.Count() - 1))
                                {
                                    spell.Cast(result.Position);
                                    myDevTools.DebugMode("[Murkwolves] Hit: " + result.MinionsHit + " Count: " + murkwolfls.Count());
                                }
                                break;
                        }
                        break;
                    #endregion
                    #region Razorbeak
                    case "monsterCamp_3":
                    case "monsterCamp_9":
                        var razorbeakl = GetLargeMonsters(camp.Position, castrange).Where(x => x.IsValidTarget() && !x.IsDead).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Razorbeak"));
                        var razorbeaks = JungleMinions.Where(x => x.IsValidTarget() && !x.IsDead && x.CharData.BaseSkinName.Contains("SRU_Razorbeak")).ToList();
                        switch (collision)
                        {
                            case true:
                                if (razorbeakl != null && razorbeakl.IsValidTarget())
                                {
                                    Pred = spell.GetPrediction(razorbeakl, aoe, castrange);
                                    if (Pred.CollisionObjects.Count == 0 || (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable))
                                    {
                                        if (Pred.Hitchance >= HitChance.High) spell.Cast(Pred.CastPosition);
                                    }
                                }
                                else
                                {
                                    foreach (var loop in razorbeaks)
                                    {
                                        Pred = spell.GetPrediction(loop, aoe, castrange);
                                        if (Pred.CollisionObjects.Count == 0 || (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable))
                                        {
                                            if (Pred.Hitchance >= HitChance.High) spell.Cast(Pred.CastPosition);
                                        }
                                    }
                                }
                                break;
                            case false:
                                var result = MinionManager.GetBestLineFarmLocation(razorbeaks.Select(m => m.ServerPosition.To2D()).ToList(), spell.Width, castrange);
                                if (result.MinionsHit >= razorbeaks.Count() || result.MinionsHit >= Math.Max(1, razorbeaks.Count() - 1))
                                {
                                    spell.Cast(result.Position);
                                    myDevTools.DebugMode("[Murkwolves] Hit: " + result.MinionsHit + " Count: " + razorbeaks.Count());
                                }
                                break;
                        }
                        break;
                    #endregion
                    #region Crab
                    case "monsterCamp_15":
                    case "monsterCamp_16":
                        var crab = GetLargeMonsters(Player.Position, castrange).Where(x => x.IsValidTarget() && !x.IsDead).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("Crab"));
                        if (crab != null && crab.IsValidTarget())
                        {
                            if (collision)
                            {
                                Pred = spell.GetPrediction(crab, aoe, castrange);
                                if (Pred.CollisionObjects.Count == 0 ||  (Pred.CollisionObjects.Count > 0 && Pred.CollisionObjects.Count <= passable))
                                {
                                    if (Pred.Hitchance >= HitChance.High) spell.Cast(Pred.CastPosition);
                                }
                            }
                            else
                            {
                                spell.Cast(crab.ServerPosition);
                            }                           
                        }
                        break;
                    #endregion
                }
            }
        }

        //  <summary>
        //      GetBestCircularFarmLocation, max jungle minion.
        //      Single = randomized
        //  </summary>
        public static void JungleCircular(Spell spell, float castrange = 0, float radius = 0, ObjectOrderTypes order = ObjectOrderTypes.None)
        {
            if (!AllowCasting) return;
            var findrange = castrange + Math.Max(0, radius > 300 ? (radius * 2 / 3) : radius);
            NeutralMinionCamp = GetNearestCamp(Player.Position, findrange, order);
            if (NeutralMinionCamp.Any())
            {
                var camp = NeutralMinionCamp[0];
                JungleMinions = GetMinions(camp.Position, findrange, MinionTypes.All, MinionTeam.Neutral);                
                switch (camp.Name)
                {
                    #region Baron
                    case "monsterCamp_12":
                        var baron = GetLargeMonsters(camp.Position, castrange).Where(x => x.IsValidTarget() && !x.IsDead).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Baron"));
                        if (baron != null && baron.IsValidTarget())
                        {
                            Pred = spell.GetPrediction(baron);
                            if (Vector3.Distance(ObjectManager.Player.ServerPosition, Pred.CastPosition) <= findrange)
                            {
                                if (Pred.Hitchance >= HitChance.High)
                                {
                                    Pos = myUtility.RandomPos(5, 5, 5, Player.ServerPosition.Extend(Pred.CastPosition, Vector3.Distance(Player.ServerPosition, baron.ServerPosition)));
                                    spell.Cast(Pos);
                                }
                            }
                        }
                        break;
                    #endregion
                    #region Dragon
                    case "monsterCamp_6":
                        var dragon = GetLargeMonsters(camp.Position, castrange).Where(x => x.IsValidTarget() && !x.IsDead).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Dragon"));
                        if (dragon != null && dragon.IsValidTarget())
                        {
                            Pred = spell.GetPrediction(dragon);
                            if (Vector3.Distance(ObjectManager.Player.ServerPosition, Pred.CastPosition) <= findrange)
                            {
                                if (Pred.Hitchance >= HitChance.High)
                                {
                                    Pos = myUtility.RandomPos(5, 5, 5, Player.ServerPosition.Extend(Pred.CastPosition, Vector3.Distance(Player.ServerPosition, dragon.ServerPosition)));
                                    spell.Cast(Pos);
                                }
                            }
                        }
                        break;
                    #endregion
                    #region Red Brambleback
                    case "monsterCamp_4":
                    case "monsterCamp_10":
                        var bramblebacks = JungleMinions.Where(x => x.IsValidTarget() && !x.IsDead && x.CharData.BaseSkinName.Contains("SRU_Red")).ToList();
                        var redresult = MinionManager.GetBestCircularFarmLocation(bramblebacks.Select(m => m.ServerPosition.To2D()).ToList(), radius, findrange);
                        if (redresult.MinionsHit >= bramblebacks.Count())
                        {
                            spell.Cast(redresult.Position);
                            myDevTools.DebugMode("[Red Bramblebacks] Hit: " + redresult.MinionsHit + " Count: " + bramblebacks.Count());
                        }
                        break;
                    #endregion
                    #region Blue Sentinel
                    case "monsterCamp_1":
                    case "monsterCamp_7":
                        var sentinels = JungleMinions.Where(x => x.IsValidTarget() && !x.IsDead && x.CharData.BaseSkinName.Contains("SRU_Blue")).ToList();
                        var blueresult = MinionManager.GetBestCircularFarmLocation(sentinels.Select(m => m.ServerPosition.To2D()).ToList(), radius, findrange);
                        if (blueresult.MinionsHit >= sentinels.Count())
                        {
                            spell.Cast(blueresult.Position);
                            myDevTools.DebugMode("[Blue Sentinels] Hit: " + blueresult.MinionsHit + " Count: " + sentinels.Count());
                        }
                        break;
                    #endregion
                    #region Gromp
                    case "monsterCamp_13":
                    case "monsterCamp_14":
                        var gromp = JungleMinions.Where(x => x.IsValidTarget() && !x.IsDead).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("SRU_Gromp"));
                        if (gromp != null && gromp.IsValidTarget())
                        {
                            Pred = spell.GetPrediction(gromp);
                            if (Vector3.Distance(ObjectManager.Player.ServerPosition, Pred.CastPosition) <= findrange)
                            {
                                if (Pred.Hitchance >= HitChance.High)
                                {
                                    Pos = myUtility.RandomPos(5, 5, 5, Player.ServerPosition.Extend(Pred.CastPosition, Vector3.Distance(Player.ServerPosition, gromp.ServerPosition)));
                                    spell.Cast(Pos);
                                }
                            }
                        }
                        break;
                    #endregion
                    #region Krugs
                    case "monsterCamp_5":
                    case "monsterCamp_11":
                        var krugs = JungleMinions.Where(x => x.IsValidTarget() && !x.IsDead && x.CharData.BaseSkinName.Contains("SRU_Krug")).ToList();
                        var krugresult = MinionManager.GetBestCircularFarmLocation(krugs.Select(m => m.ServerPosition.To2D()).ToList(), radius, findrange);
                        if (krugresult.MinionsHit >= krugs.Count())
                        {
                            spell.Cast(krugresult.Position);
                            myDevTools.DebugMode("[Krugs] Hit: " + krugresult.MinionsHit + " Count: " + krugs.Count());
                        }
                        break;
                    #endregion
                    #region Murkwolves
                    case "monsterCamp_2":
                    case "monsterCamp_8":
                        var wolves = JungleMinions.Where(x => x.IsValidTarget() && !x.IsDead && x.CharData.BaseSkinName.Contains("SRU_Murkwolf")).ToList();
                        var wolvesresult = MinionManager.GetBestCircularFarmLocation(wolves.Select(m => m.ServerPosition.To2D()).ToList(), radius, findrange);
                        if (wolvesresult.MinionsHit >= wolves.Count())
                        {
                            spell.Cast(wolvesresult.Position);
                            myDevTools.DebugMode("[Murkwolves] Hit: " + wolvesresult.MinionsHit + " Count: " + wolves.Count());
                        }
                        break;
                    #endregion
                    #region Razorbeak
                    case "monsterCamp_3":
                    case "monsterCamp_9":
                        var razorbeak = JungleMinions.Where(x => x.IsValidTarget() && !x.IsDead && x.CharData.BaseSkinName.Contains("SRU_Razorbeak")).ToList();
                        var result = MinionManager.GetBestCircularFarmLocation(razorbeak.Select(m => m.ServerPosition.To2D()).ToList(), radius, findrange);
                        if (result.MinionsHit >= razorbeak.Count())
                        {
                            spell.Cast(result.Position);
                            myDevTools.DebugMode("[Razorbeak] Hit: " + result.MinionsHit + " Count: " + razorbeak.Count());
                        }
                        break;
                    #endregion
                    #region Crab
                    case "monsterCamp_15":
                    case "monsterCamp_16":
                        var crab = JungleMinions.Where(x => x.IsValidTarget() && !x.IsDead).FirstOrDefault(x => x.CharData.BaseSkinName.Contains("Crab"));
                        if (crab != null && crab.IsValidTarget())
                        {
                            Pred = spell.GetPrediction(crab);
                            if (Vector3.Distance(ObjectManager.Player.ServerPosition, Pred.CastPosition) <= findrange)
                            {
                                if (Pred.Hitchance >= HitChance.High)
                                {
                                    spell.Cast(Pred.CastPosition);
                                }
                            }
                        }
                        break;
                    #endregion
                }
            }
        }

        public static void JungleTest(Spell spell, float castrange = 0, bool smite = false)
        {
        }
        #endregion Jungling
    }
}