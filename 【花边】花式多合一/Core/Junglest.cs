using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace 花边_花式多合一.Core
{
    class Junglest
    {
        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                Console.Clear();
                if (Game.Mode == GameMode.Running)
                    new junglerslack.Switch();
                else Game.OnStart += (a) => 
                new junglerslack.Switch();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Junglest error occurred: '{0}'", ex);
            }
        }
    }

    public static class junglerslack
    {
        public static Player myHero = Huabian.Player.Data<Player>();
        public static Obj_SpawnPoint allySpawn = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(spawn => spawn.IsValid && spawn.Team == Huabian.Player.Team);
        public static Obj_SpawnPoint enemySpawn = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(spawn => spawn.IsValid && spawn.Team != Huabian.Player.Team);

        // position related
        public static double Distance(this Vector3 from, Vector3 to)
        {
            return Math.Sqrt(Math.Pow(to.X - from.X, 2) + Math.Pow(to.Y - from.Y, 2));
        }

        public static double Rad(this Vector3 from, Vector3 to)
        {
            if (to.Y > from.Y) return Math.Atan2(to.Y - from.Y, to.X - from.X);
            else return Math.PI * 2 + Math.Atan2(to.Y - from.Y, to.X - from.X);
        }

        public static Vector3 Pos(this Vector3 from, Vector3 to, double range)
        {
            var rad = from.Rad(to);

            return new Vector3((float)(from.X + Math.Cos(rad) * range), from.Z, (float)(from.Y + Math.Sin(rad) * range));
        }

        public static bool CanSee(this Vector3 from, Vector3 to) { return from.Distance(to) < 1200 && NavMesh.LineOfSightTest(from, to); }

        public static bool CanSeeEx(this Vector3 from, Vector3 to)
        {
            //#warning bugged;
            var toEx = from.Pos(to, (Math.Max(0, from.Distance(to) - 20)));
            return from.Distance(to) < 1200 && NavMesh.LineOfSightTest(from, toEx);
        }

        public static GameObjectTeam Side(this Vector3 unit)
        {
            var dist = allySpawn.Position.Distance(unit) - enemySpawn.Position.Distance(unit);

            if (dist > 1250)
            {
                return enemySpawn.Team;
            }

            else if (dist < -1250)
            {
                return allySpawn.Team;
            }
            else
            {
                return GameObjectTeam.Neutral;
            }
        }


        public class Object
        {
            // members
            public GameObject @ref;
            public string @class;
            private GameObject unit;

            public Object(GameObject a)
            {
                @ref = a;
                @class = Class(a);
            }

            public static string Class(GameObject unit)
            {
                var type = unit.Type;

                switch (type)
                {
                    case GameObjectType.obj_GeneralParticleEmitter:

                    case GameObjectType.obj_AI_Marker:

                    case GameObjectType.FollowerObject:
                        {
                            return "visual";
                        }

                    case GameObjectType.obj_AI_Minion:

                        var minion = (unit as Obj_AI_Minion);
                        var name = unit.Name.ToLower();

                        if (minion.CampNumber != 0)
                            return "creep"; //L#
                        else if (name.Contains("minion"))
                            return "minion";
                        else if (name.Contains("ward"))
                            return "ward";
                        else if (name.Contains("buffplat") || name == "odinneutralguardian")
                            return "point";
                        else if (name.Contains("shrine") || name.Contains("relic"))
                            return "event";
                        else if (Game.MapId == GameMapId.SummonersRift && Regex.IsMatch(name, @"\d+\.\d+")
                            && (name.Contains("baron") || name.Contains("dragon") || name.Contains("blue") || name.Contains("red") || name.Contains("crab")
                            || name.Contains("krug") || name.Contains("gromp") || name.Contains("wolf") || name.Contains("razor")))
                            return "creep";
                        else if (Game.MapId == GameMapId.TwistedTreeline && Regex.IsMatch(name, @"\d+\.\d+")
                            && (name.Contains("wraith") || name.Contains("golem") || name.Contains("wolf") || name.Contains("spider")))
                            return "creep";
                        else if (!minion.IsTargetable)
                            return "trap";
                        else return "error";

                    case GameObjectType.obj_AI_Turret:
                        return "tower";
                    case GameObjectType.obj_AI_Hero:
                        return "player";
                    case GameObjectType.obj_Shop:
                        return "shop";
                    case GameObjectType.obj_HQ:
                        return "nexus";
                    case GameObjectType.obj_BarracksDampener:
                        return "inhibotor";
                    case GameObjectType.obj_SpawnPoint:
                        return "spawn";
                    case GameObjectType.obj_Barracks:
                        return "minionSpawn";
                    case GameObjectType.NeutralMinionCamp:
                        return "creepSpawn";
                    case GameObjectType.obj_InfoPoint:
                        return "event";
                    case GameObjectType.Missile:

                    case GameObjectType.MissileClient:

                    case GameObjectType.obj_SpellMissile:

                    case GameObjectType.obj_SpellCircleMissile:

                    case GameObjectType.obj_SpellLineMissile:
                        return "spell";

                    case GameObjectType.obj_Turret:

                    case GameObjectType.obj_Levelsizer:

                    case GameObjectType.obj_NavPoint:

                    case GameObjectType.LevelPropSpawnerPoint:

                    case GameObjectType.LevelPropGameObject:

                    case GameObjectType.GrassObject:

                    case GameObjectType.obj_Lake:

                    case GameObjectType.obj_LampBulb:

                    case GameObjectType.DrawFX:
                        return "useless";

                }
                return "error";
            }
        }

        public class Unit : Object
        {
            // members
            new public Obj_AI_Base @ref;

            public Unit(Obj_AI_Base a) : base(a)
            {
                @ref = a;
            }

            // methods
            public bool InRange(Unit target)
            {
                return @ref.ServerPosition.Distance(target.@ref.Position) < @ref.AttackRange + @ref.BoundingRadius + target.@ref.BoundingRadius;
            }

            public bool InRange(Unit target, SpellSlot spell)
            {
                return @ref.ServerPosition.Distance(target.@ref.Position) < @ref.Spellbook.GetSpell(spell).SData.CastRange;
            }

            public bool Buff(string name)
            {
                foreach (var buff in @ref.Buffs)
                {
                    if (buff.IsValid && buff.IsActive && buff.Name == name) return true;
                }
                return false;
            }

            public bool Buff(List<string> list)
            {
                foreach (var buff in list) if (Buff(buff)) return true;
                return false;
            }

            public bool CanUse(SpellSlot spell)
            {
                return @ref.Spellbook.GetSpell(spell).State == SpellState.Ready; // optimize it
            } 

            public SpellSlot Item(int id, bool usable = false)
            {
                var item = @ref.InventoryItems.FirstOrDefault(slot => slot.Id == (ItemId)id && (!usable || CanUse(slot.SpellSlot)));

                if (item != null)
                    return item.SpellSlot;
                else return SpellSlot.Unknown;
            }

            public SpellSlot Item(List<int> list, bool usable = false)
            {
                foreach (var id in list)
                {
                    var spell = Item(id);
                    if (spell != SpellSlot.Unknown) return spell;
                }
                return SpellSlot.Unknown;
            }
        }

        public class Player : Unit
        {
            // members
            new public Obj_AI_Hero @ref;
            public static Player myHero = ObjectManager.Player.Data<Player>();

            public Player(Obj_AI_Hero a) : base(a)
            {
                @ref = a;
            }

            // methods
            public int SmiteDamage()
            {
                var damage = 370 + @ref.Level * 20;
                if (@ref.Level > 4) damage = damage + (@ref.Level - 4) * 10;
                if (@ref.Level > 9) damage = damage + (@ref.Level - 9) * 10;
                if (@ref.Level > 14) damage = damage + (@ref.Level - 14) * 10;
                return damage;
            }

            public void Level(List<LeagueSharp.SpellSlot> list)
            {
                int req_Q = 0, _Q = @ref.Spellbook.GetSpell(SpellSlot.Q).Level;
                int req_W = 0, _W = @ref.Spellbook.GetSpell(SpellSlot.W).Level;
                int req_E = 0, _E = @ref.Spellbook.GetSpell(SpellSlot.E).Level;
                int req_R = 0, _R = @ref.Spellbook.GetSpell(SpellSlot.R).Level;

                foreach (var spell in list)
                {
                    switch (spell)
                    {
                        case SpellSlot.Q:
                            req_Q += 1;
                            if (req_Q > _Q) @ref.Spellbook.LevelSpell(spell);
                            break;
                        case SpellSlot.W:
                            req_W += 1;
                            if (req_W > _W) @ref.Spellbook.LevelSpell(spell);
                            break;
                        case SpellSlot.E:
                            req_E += 1;
                            if (req_E > _E) @ref.Spellbook.LevelSpell(spell);
                            break;
                        case SpellSlot.R:
                            req_R += 1;
                            if (req_R > _R) @ref.Spellbook.LevelSpell(spell);
                            break;
                    }
                }
            }

            public void Attack(Unit target)
            {
                @ref.IssueOrder(GameObjectOrder.AttackUnit, target.@ref);
            }

            public void MoveTo(Object target)
            {
                @ref.IssueOrder(GameObjectOrder.MoveTo, target.@ref.Position);
            }

            public bool Cast(SpellSlot spell)
            {
                return (spell != SpellSlot.Unknown && CanUse(spell) && @ref.Spellbook.CastSpell(spell));
            }

            public bool Cast(SpellSlot spell, GameObject target)
            {
                return (spell != SpellSlot.Unknown && CanUse(spell) && @ref.Spellbook.CastSpell(spell, target));
            }
        }

        public class CreepSpawn : Object
        {
            // members
            new public NeutralMinionCamp @ref;
            public int campNumber;
            public float spawn, respawn, dead;

            public CreepSpawn(NeutralMinionCamp a) : base(a)
            {
                @ref = a;
                campNumber = CampNumber();
                spawn = Spawn() + 30; //bug with timers
                respawn = Respawn();
                dead = spawn;
                a.GetHashCode();
            }

            // methods
            public int CampNumber()
            {
                var result = Regex.Match(@ref.Name, @"\d+");
                if (!result.Success) return 0;
                else
                {
                    int num = 0;
                    if (int.TryParse(result.Value, out num)) return num; else return 0;
                }
            }

            public bool Test(int id, GameMapId map = GameMapId.SummonersRift)
            {
                return LeagueSharp.Game.MapId == map && campNumber == id;
            }

            public bool Test(int[] id, GameMapId map = GameMapId.SummonersRift)
            {
                return Game.MapId == map && id.Contains(campNumber);
            }

            public bool Dragon()
            {
                return Test(6);
            }

            public bool Nashor()
            {
                return Test(12);
            }

            public bool Blue()
            {
                return Test(new[] { 1, 7 });
            }

            public bool Red()
            {
                return Test(new[] { 4, 10 });
            }

            public bool Cancer()
            {
                return Test(new[] { 15, 16 });
            }

            public bool Wolf()
            {
                return Test(new[] { 2, 8 });
            }

            public bool Wraith()
            {
                return Test(new[] { 3, 9 });
            }

            public bool Golem()
            {
                return Test(new[] { 5, 11 });
            }

            public bool Wight()
            {
                return Test(new[] { 13, 14 });
            }

            public float Spawn()
            {
                if (Dragon()) return 150;
                else if (Nashor()) return 750;
                else if (Cancer()) return 150;
                else return 120;
            }

            public float Respawn()
            {
                if (Dragon()) return 360;
                else if (Nashor()) return 420;
                else if (Cancer()) return 180;
                else if (Blue() || Red()) return 300;
                else return 100;
            }

            public IEnumerable<Creep> Creeps(bool dead = false)
            {
                return Interface.data.Values.OfType<Creep>().Where(c => c.@ref.IsVisible && c.@ref.IsDead == dead && c.@ref.CampNumber == campNumber);
            }

            public float Health()
            {
                return Creeps().Sum(creep => creep.@ref.Health);
            }

            public Creep BigCreep()
            {
                Creep max = null;
                foreach (var c in Creeps()) { if (max == null || c.@ref.MaxHealth > max.@ref.MaxHealth) max = c; }
                return max;
            }

            public bool Started()
            {
                foreach (var c in Creeps()) { if (c.@ref.Health < c.@ref.MaxHealth) return true; }
                return false;
            }

            public void Set(bool state)
            {
                if (state) dead = 0; else dead = Game.ClockTime + respawn;
            }

            public float Get()
            {
                return Math.Max(0, dead - Game.ClockTime);
            }

            public void Refresh(bool force = false)
            {
                if (Creeps().Any()) Set(true);
                else if (Get() == 0 && force) Set(false);
                else if (Get() == 0 && !myHero.@ref.IsDead && !Cancer() && myHero.@ref.ServerPosition.CanSee(@ref.Position))
                    Timer.Once((h) => { if (myHero.@ref.ServerPosition.CanSee(@ref.Position)) Refresh(true); }).Cooldown(Game.Ping / 1000 + 0.05f).Start();
                else if (Creeps(true).Any()) Set(false);
            }
        }

        public class Creep : Unit
        {
            // members
            new public Obj_AI_Minion @ref;
            public int campNumber;

            public Creep(Obj_AI_Minion a) : base(a)
            {
                @ref = a;
                campNumber = a.CampNumber;
            }

            // methods
            public CreepSpawn CreepSpawn()
            {
                return Interface.data.Values.OfType<CreepSpawn>().FirstOrDefault(cs => cs.campNumber == campNumber);
            }
        }

        public static class Timer
        {
            public class Handle
            {
                public bool later = true;
                public float lastcall, cooldown = 0;

                public Action<Handle> callback = delegate { };

                public void Disable()
                {
                    list.Remove(this);
                }

                public Handle Start()
                {
                    later = false; return this;
                }

                public Handle Stop()
                {
                    later = true; return this;
                }

                public Handle Cooldown(float v)
                {
                    cooldown = v; return this;
                }

                public Handle Callback(Action<Handle> @in)
                {
                    callback = @in; return this;
                }
            }
            public static List<Handle> list = new List<Handle>();

            public static Handle Add(Action<Handle> callback)
            {
                var h = new Handle();
                list.Add(h);
                return h.Callback(callback);
            }

            public static Handle Once(Action<Handle> callback)
            {
                return Add(delegate (Handle h)
                {
                    callback(h); h.Disable();
                });
            }

            static Timer()
            {
                Game.OnUpdate += delegate {
                    for (int i = list.Count; i-- > 0;)
                    {
                        var h = list[i];
                        if (!h.later && h.lastcall + h.cooldown <= Game.ClockTime)
                        {
                            h.lastcall = Game.ClockTime;
                            h.callback(h);
                        }
                    }
                };
            }
        }

        abstract public class Parent
        {
            abstract public void Reset();
            public Parent()
            {
                Reset();
            }
        }

        public class HealthPot : Parent
        {
            public List<int> id;
            public List<string> buff;
            public Func<bool> Need;
            public Action Logic;

            override public void Reset()
            {
                id = new List<int>
                {
                    2041, 2003, 2010, 2009
                };

                buff = new List<string>
                {
                    "ItemCrystalFlask", "RegenerationPotion", "ItemMiniRegenPotion"
                };

                Need = delegate 
                {
                    return myHero.@ref.Health / myHero.@ref.MaxHealth < 0.45;
                };

                Logic = delegate 
                {
                    if (Need())
                        myHero.Cast(myHero.Item(id));
                };
            }

        }

        public class SpellAttack : Parent
        {
            public Func<AttackableUnit, bool> T(SpellSlot spell)
            {
                var gs = myHero.@ref.Spellbook.GetSpell(spell);
                switch (gs.SData.TargettingType)
                {
                    case SpellDataTargetType.Unit:
                        return delegate (AttackableUnit target)
                        {
                            return myHero.Cast(spell, target);
                        };
                    case SpellDataTargetType.Self:
                    case SpellDataTargetType.SelfAoe:
                        return delegate (AttackableUnit target)
                        {
                            return myHero.Cast(spell);
                        };
                }
                return delegate { return false; };
            }

            public Func<AttackableUnit, bool> Q, W, E, R;
            public Func<AttackableUnit, bool> Logic;

            override public void Reset()
            {
                Q = T(SpellSlot.Q);
                W = T(SpellSlot.W);
                E = T(SpellSlot.E);
                R = T(SpellSlot.R);
                Logic = delegate (AttackableUnit target)
                {
                    return Q(target) || W(target) || E(target) || R(target);
                };
            }
        }

        public class SpellSelf : Parent
        {
            public Func<bool> T(SpellSlot spell)
            {
                var gs = myHero.@ref.Spellbook.GetSpell(spell);
                switch (gs.SData.TargettingType)
                {
                    case SpellDataTargetType.SelfAoe:
                    case SpellDataTargetType.Self:
                        return delegate
                        {
                            return myHero.Cast(spell);
                        };
                }
                return delegate { return false; };
            }

            public Func<bool> Q, W, E, R;
            public Func<bool> Logic;

            override public void Reset()
            {
                Q = T(SpellSlot.Q);
                W = T(SpellSlot.W);
                E = T(SpellSlot.E);
                R = T(SpellSlot.R);
                Logic = delegate
                {
                    return Q() || W() || E() || R();
                };
            }
        }

        public class RedBuff : Parent
        {
            public string own, apply;
            override public void Reset()
            {
                own = "blessingofthelizardelder";
                apply = "blessingofthelizardelderslow";
            }
        }

        public class Spell : SpellAttack
        {
            public Func<Creep, CreepSpawn, bool> Q_Worth, W_Worth, E_Worth, R_Worth;
            new public Func<Creep, CreepSpawn, bool> Logic;

            override public void Reset()
            {
                Logic = (c, cs) => {
                    if (Q_Worth(c, cs) && Q(c.@ref)) return true;
                    else if (W_Worth(c, cs) && W(c.@ref)) return true;
                    else if (E_Worth(c, cs) && E(c.@ref)) return true;
                    else if (R_Worth(c, cs) && R(c.@ref)) return true;
                    return false;
                };

                Q = T(SpellSlot.Q);
                W = T(SpellSlot.W);
                E = T(SpellSlot.E);
                R = T(SpellSlot.R);

                Q_Worth = W_Worth = E_Worth = R_Worth = delegate
                {
                    return false;
                };
            }
        }

        public class Channel : Parent
        {
            public Func<bool> Worth, State, Logic;
            override public void Reset()
            {
                Worth = delegate () { return true; };
                State = delegate () { return myHero.@ref.Spellbook.IsCastingSpell || myHero.@ref.Spellbook.IsChanneling; };
                Logic = delegate () { return Worth() && State(); };
            }
        }

        public class Smite : Parent
        {
            public Func<Creep, CreepSpawn, bool> WorthStart, Worth, WorthEx, Logic;
            public SpellSlot spell;

            public static SpellSlot Find()
            {
                if (myHero.@ref.Spellbook.GetSpell(SpellSlot.Summoner1).Name.ToLower().Contains("smite"))
                    return SpellSlot.Summoner1;
                else if (myHero.@ref.Spellbook.GetSpell(SpellSlot.Summoner2).Name.ToLower().Contains("smite"))
                    return SpellSlot.Summoner2;
                else return SpellSlot.Unknown;
            }

            override public void Reset()
            {
                WorthStart = (c, cs) => 
                {
                    return cs.Started() && c.@ref.MaxHealth > myHero.SmiteDamage() * 2; // not started or too small
                };

                Worth = (c, cs) => {
                    if (c.@ref.Health > myHero.SmiteDamage()) return false; // cant smitesteal
                    else return (cs.Dragon() || cs.Nashor() || cs.Blue() || cs.Red());
                };

                WorthEx = (c, cs) =>
                {
                    return false;
                };

                Logic = (c, cs) => {
                    if (c.@ref.CampNumber == 0 || myHero.@ref.ServerPosition.Distance(c.@ref.ServerPosition) > 850)
                        return false;
                    else return WorthStart(c, cs) && (WorthEx(c, cs) || Worth(c, cs)) && myHero.Cast(spell, c.@ref);
                };

                spell = Find();
            }
        }

        public class SmiteActive : Parent
        {
            public Smite smite;
            public Timer.Handle timer;

            public override void Reset()
            {
                smite = new Smite();

                smite.WorthEx = (c, cs) => {
                    return (cs.Wight() || cs.Golem()) &&
                        !ObjectManager.Get<Obj_AI_Hero>().Any(player =>
                        player.IsValid && !player.IsMe && !player.IsDead && player.ServerPosition.Distance(myHero.@ref.ServerPosition) < 1500);
                };

                if (timer != null) timer.Disable();

                timer = Timer.Add(delegate {
                    if (!myHero.@ref.IsDead)
                        foreach (var c in Interface.data.Values.OfType<Creep>())
                        {
                            smite.Logic(c, c.CreepSpawn());
                        };
                }).Start();
            }
        }

        public class RefreshActive : Parent
        {
            public static RefreshActive instance;
            public Timer.Handle timer;

            public override void Reset()
            {
                if (instance != null)
                    return;

                instance = this;

                if (timer != null) timer.Disable();

                timer = Timer.Add(delegate {
                    if (!myHero.@ref.IsDead)
                        foreach (var cs in Interface.data.Values.OfType<CreepSpawn>()) cs.Refresh();
                }).Start();
            }
        }

        public class Nav : Parent
        {
            public Func<bool> Fast;
            public Func<IEnumerable<CreepSpawn>> Candidates;
            public Func<CreepSpawn> CreepSpawn, CreepSpawnEx;

            override public void Reset()
            {
                Fast = delegate
                {
                    return false;
                };

                Candidates = () => Interface.data.Values.OfType<CreepSpawn>().Where(ics => ics.@ref.Position.Side() == myHero.@ref.Team);

                CreepSpawn = delegate
                {
                    CreepSpawn result = CreepSpawnEx();
                    if (result != null)
                        return result;
                    var fast = Fast();
                    var candidates = Candidates();
                    var resultScore = double.MaxValue;

                    foreach (var cs in candidates)
                    {
                        // score
                        var score = 0d;
                        var get = cs.Get() * myHero.@ref.MoveSpeed;
                        var dist = myHero.@ref.ServerPosition.Distance(cs.@ref.Position);

                        if (cs.dead == cs.spawn || dist > get) score = dist;
                        else score = dist + (get - dist) * 1.4;

                        if (cs.Started())
                        {
                            if (cs.respawn > 200) score -= 3300; else score -= 2000;
                        }
                        else if (Game.MapId == GameMapId.SummonersRift)
                        {
                            if (myHero.@ref.Level == 1)
                            {
                                if ((!fast && cs.Golem()) || (fast && cs.Wight()))
                                    score = score - 3300;
                            }
                            else if (myHero.@ref.Level >= 3 && fast)
                            {
                                if (cs.Wolf() && (candidates.First((ics) =>
                                ics.Blue()).Get() < 3200 / myHero.@ref.MoveSpeed || candidates.First((ics) =>
                                ics.Wight()).Get() < 3200 / myHero.@ref.MoveSpeed))
                                    score += 3300;
                                else if (cs.Wraith() && (candidates.First((ics) =>
                                ics.Red()).Get() < 3200 / myHero.@ref.MoveSpeed || candidates.First((ics) =>
                                ics.Golem()).Get() < 3200 / myHero.@ref.MoveSpeed))
                                    score += 3300;
                            }
                        }
                        if (score < resultScore) { result = cs; resultScore = score; }
                    }
                    return result;
                };

                CreepSpawnEx = delegate { return null; };
            }
        }

        public class Target : Parent
        {
            public RedBuff redBuff;
            public Func<CreepSpawn, bool> Cleave;
            public Func<CreepSpawn, Creep> Creep, CreepEx;
            override public void Reset()
            {
                redBuff = new RedBuff();
                Cleave = (c) => false;
                CreepEx = (cs) => null;
                Creep = (cs) => {
                    var target = CreepEx(cs); if (target != null) return target;
                    var cleave = Cleave(cs);
                    var red = myHero.Buff(redBuff.own);
                    foreach (var c in cs.Creeps())
                    {
                        if (target == null) target = c;
                        else if (red && (target.Buff(redBuff.apply) || c.Buff(redBuff.apply)))
                        {
                            if (target.Buff(redBuff.apply) && !c.Buff(redBuff.apply)) target = c;
                        }
                        else if (cleave && c.@ref.MaxHealth > target.@ref.MaxHealth) target = c;
                        else if (!cleave && c.@ref.MaxHealth < target.@ref.MaxHealth) target = c;
                        else if (c.@ref.MaxHealth == target.@ref.MaxHealth && c.@ref.NetworkId > target.@ref.NetworkId) target = c;
                    }
                    return target;
                };

            }
        }

        public class Kill : Parent
        {
            public HealthPot pot;
            public Channel channel;
            public Spell spell;
            public Action<Creep, CreepSpawn> Logic;
            override public void Reset()
            {
                pot = new HealthPot();
                channel = new Channel();
                spell = new Spell();
                Logic = (c, cs) => {
                    pot.Logic();
                    if (channel.Logic()) return;
                    spell.Logic(c, cs);
                    myHero.Attack(c);
                };
            }
        }

        public class Move : Parent
        {
            public Channel channel;
            public Func<CreepSpawn, bool> LogicEx, Logic;

            override public void Reset()
            {
                channel = new Channel();
                LogicEx = (cs) => false;
                Logic = (cs) => {
                    if (channel.Logic() || LogicEx(cs))
                        return true;
                    else if (myHero.@ref.ServerPosition.Distance(cs.@ref.Position) < 50)
                        return false;
                    myHero.MoveTo(cs);
                    return true;
                };
            }
        }

        public class Cycle : Parent
        {
            public Nav nav;
            public Target target;
            public Kill kill;
            public Move move;
            public RefreshActive refresh;
            public SmiteActive smite;
            public System.Func<bool> Logic;

            override public void Reset()
            {
                nav = new Nav();
                target = new Target();
                kill = new Kill();
                move = new Move();
                refresh = new RefreshActive();
                smite = new SmiteActive();

                Logic = delegate {
                    var creepSpawn = nav.CreepSpawn();
                    if (Hud.SelectedUnit != null && Hud.SelectedUnit.Data<Object>().@class == "creep") creepSpawn = Hud.SelectedUnit.Data<Creep>().CreepSpawn();
                    if (creepSpawn == null) return false;
                    var creep = target.Creep(creepSpawn);
                    if (creep != null) kill.Logic(creep, creepSpawn);
                    else move.Logic(creepSpawn);
                    return true;
                };
            }
        }

        public class CycleEx : Cycle
        {
            public void Transform(string charName)
            {
                Reset();

                switch (charName)
                {
                    case "Warwick":
                        kill.spell.Q_Worth = (c, cs) =>
                        {
                            return true;
                        };

                        kill.spell.W_Worth = (c, cs) =>
                        {
                            return myHero.InRange(c) && cs.Health() > myHero.SmiteDamage();
                        };
                        break;
                    case "MasterYi":
                        kill.spell.Q_Worth = (c, cs) =>
                        {
                            return true;
                        };

                        kill.spell.E_Worth = new CycleEx("Warwick").kill.spell.W_Worth;

                        move.channel.Worth = kill.channel.Worth = () =>
                        {
                            return myHero.@ref.Health / myHero.@ref.MaxHealth < 1;
                        };

                        move.LogicEx = (cs) =>
                        {
                            return myHero.@ref.Health / myHero.@ref.MaxHealth < 0.45 && myHero.Cast(SpellSlot.W);
                        };
                        break;
                }
            }

            public CycleEx(string charName)
            {
                Transform(charName);
            }
        }

        public class Switch : CycleEx
        {
            public uint switchButton;
            public Timer.Handle timer;

            public void GuiFull()
            {
                // farm button
                var f1 = InitializeMenu.Menu.SubMenu("关于打野").SubMenu("打野疲惫期").AddItem(new MenuItem("junglerslackkey", "打钱按键").SetValue(new KeyBind(switchButton, KeyBindType.Toggle, false)));
                var f2 = f1.GetValue<KeyBind>();
                var farmTick = InitializeMenu.Menu.SubMenu("关于打野").SubMenu("打野疲惫期").AddItem(new MenuItem("farm", "打钱"));
                farmTick.SetValue(f1);
                farmTick.ValueChanged += (h, a) => timer.later = !a.GetNewValue<KeyBind>().Active;
                farmTick.DontSave();
                InitializeMenu.Menu.SubMenu("关于打野").SubMenu("打野疲惫期").AddItem(farmTick);

                // farm button disabler
                Game.OnWndProc += (a) => {
                    if (a.Msg == 516 && a.WParam == 2)
                    {
                        f2.Active = false;
                        farmTick.SetValue(f1);
                    }
                };

                // smite button
                var smiteTick = InitializeMenu.Menu.SubMenu("关于打野").SubMenu("打野疲惫期").AddItem(new MenuItem("smite", "惩戒", true).SetValue(true));
                smiteTick.ValueChanged += (h, a) => smite.timer.later = !a.GetNewValue<bool>();
                InitializeMenu.Menu.SubMenu("关于打野").SubMenu("打野疲惫期").AddItem(smiteTick);

                // fast button
                var fastTick = InitializeMenu.Menu.SubMenu("关于打野").SubMenu("打野疲惫期").AddItem(new MenuItem("fast", "快速", true).SetValue(true));
                nav.Fast = () => fastTick.GetValue<bool>();
                InitializeMenu.Menu.SubMenu("关于打野").SubMenu("打野疲惫期").AddItem(fastTick);

                // leveling
                var recordMenu = InitializeMenu.Menu.SubMenu("关于打野").SubMenu("打野疲惫期").AddSubMenu(new Menu("记录", "records"));
                InitializeMenu.Menu.SubMenu("关于打野").SubMenu("打野疲惫期").AddSubMenu(recordMenu);

                Obj_AI_Base.OnLevelUp += (unit, a) => {
                    if (unit.IsMe)
                    {
                        var me = unit as Obj_AI_Hero;
                        if (me.Level == 3 || me.Level == 6 || me.Level == 9 || me.Level == 11 || me.Level == 16)
                        {
                            var record = InitializeMenu.Menu.SubMenu("关于打野").SubMenu("打野疲惫期").AddItem(new MenuItem(me.Level.ToString(), "等级 " + me.Level + " : " + TimeSpan.FromSeconds(Game.ClockTime).ToString(@"mm\:ss")));
                            record.DontSave();
                            recordMenu.AddItem(record);
                        }
                    }
                };

                // leveling sated
                Obj_AI_Base.OnBuffAdd += (unit, a) => {
                    if (unit.IsMe)
                    {
                        //#warning l# bug
                        //System.Console.WriteLine(a.Buff.Name);
                    }
                };

                // leveling init
                var startRecord = InitializeMenu.Menu.SubMenu("关于打野").SubMenu("打野疲惫期").AddItem(new MenuItem("start", "开始 : " + TimeSpan.FromSeconds(Game.ClockTime).ToString(@"mm\:ss")));
                startRecord.DontSave();
                recordMenu.AddItem(startRecord);

            }

            public void GuiLess()
            {
                Game.OnWndProc += delegate (WndEventArgs a)
                {
                    if (a.Msg == 256 && a.WParam == switchButton) timer.later = !timer.later; //F1
                    if (a.Msg == 516 && a.WParam == 2) timer.later = true; // RIGHT CLICK
                };
            }

            public Switch() : base(myHero.@ref.ChampionName)
            {
                switchButton = 112; // F1
                timer = Timer.Add(delegate { Logic(); }).Cooldown(0.1f);
                GuiFull();
            }
        }

    }
}
