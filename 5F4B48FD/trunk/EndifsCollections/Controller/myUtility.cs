using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Controller
{
    internal class myUtility : PluginData
    {
        public static readonly string[] LargeNeutral =
        {
            "SRU_Baron", "SRU_Dragon", "SRU_Blue", "SRU_Red", "SRU_Gromp",
            "SRU_Murkwolf", "SRU_Krug", "SRU_Razorbeak", "SRU_Crab", //Summoner's Rift, small = mini
            "TT_Spiderboss", "TT_NGolem", "TT_NWolf", "TT_NWraith" //Twisted Treeline, small = 2
        };

        public static IEnumerable<Obj_AI_Base> GetLargeMonsters(float range)
        {
            return MinionManager.GetMinions(ObjectManager.Player.ServerPosition, range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.Health).
                Where(x =>
                    LargeNeutral.Contains(x.BaseSkinName) &&
                    !(x.BaseSkinName.Contains("SRU_") && x.BaseSkinName.Contains("Mini")) &&
                    !(x.BaseSkinName.Contains("TT_") && x.BaseSkinName.Contains("2"))
                    );
        }

        public static IEnumerable<Obj_AI_Base> GetLargeMinions(float range)
        {
            return MinionManager.GetMinions(ObjectManager.Player.ServerPosition, range)
                        .Where(
                            x =>
                                x.IsValidTarget() &&
                                (x.BaseSkinName.ToLower().Contains("super") || x.BaseSkinName.ToLower().Contains("siege") || x.BaseSkinName.ToLower().Contains("cannon")));
        }

        public static float PlayerHealthPercentage
        {
            get { return ObjectManager.Player.Health * 100 / ObjectManager.Player.MaxHealth; }
        }

        public static float PlayerManaPercentage
        {
            get { return ObjectManager.Player.Mana * 100 / ObjectManager.Player.MaxMana; }
        }

        public static float TargetShields(Obj_AI_Base target)
        {
            float result = 0;
            //if (target.AttackShield > 0) result += target.AttackShield;
            //if (target.MagicShield > 0) result += target.MagicShield;
            return result;
        }

        public static int TickCount
        {
            get { return (int) (Game.Time * 1000); }
        }

        public static bool ImmuneToMagic(Obj_AI_Hero target)
        {
            return target.HasBuff("sivire") || target.HasBuff("nocturneshroudofdarkness") ||
                   target.HasBuff("bansheesveil") || target.HasBuffOfType(BuffType.SpellImmunity);
        }

        public static bool ImmuneToCC(Obj_AI_Hero target)
        {
            return target.HasBuff("blackshield") || target.HasBuff("ragnarok");
        }

        public static bool ImmuneToPhysical(Obj_AI_Hero target)
        {
            return (target.HasBuff("JudicatorIntervention") ||
                    (target.HasBuff("Undying Rage") && (target.Health * 100 / target.MaxHealth) < 5) ||
                    (target.HasBuff("Chrono Shift") && (target.Health * 100 / target.MaxHealth) < 5)
                    );
        }

        public static bool MovementImpaired(Obj_AI_Hero target)
        {
            /*
            return target.HasBuffOfType(BuffType.Flee) || target.HasBuffOfType(BuffType.Charm) ||
                   target.HasBuffOfType(BuffType.Taunt) //Forced Action
                   || target.HasBuffOfType(BuffType.Knockback) || target.HasBuffOfType(BuffType.Knockup) //Hard CC
                   || target.HasBuffOfType(BuffType.Slow) || target.HasBuffOfType(BuffType.Snare)
                //Can still cast skills
                   || target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Suppression); //lost control*/
            return target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Stun) ||
                   target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Suppression);
        }

        public static bool IsInBush(Obj_AI_Base target)
        {
            return NavMesh.IsWallOfGrass(target.Position, target.BoundingRadius);
        }

        public static double HitsToKill(Obj_AI_Base target)
        {
            return target.Health / ObjectManager.Player.GetAutoAttackDamage(target);
        }

        public static void UseItems(int index, Obj_AI_Base target)
        {
            Int16[] SelfBuffItems =
            {
                3142, //Ghostblade
                3184, //Entropy
            };
            Int16[] TargettedItems =
            {
                3144, //Bilgewater Cutlass
                3153, //Blade of the Ruined King      
                3146, //Hextech Gunblade                
            };
            Int16[] AoeOffenseItems =
            {
                3074, //Hydra
                3077, //Tiamat
            };
            Int16[] AoeDefenseItems =
            {
                3143, //Randuin
            };
            switch (index)
            {
                case 0:
                    foreach (var itemId in SelfBuffItems.Where(itemId => Items.HasItem(itemId) && Items.CanUseItem(itemId)))
                    {
                        if (target != null)
                        {
                            if (Orbwalking.InAutoAttackRange(target)) Items.UseItem(itemId);
                        }
                        else
                        {
                            Items.UseItem(itemId);
                        }
                    }
                    break;
                case 1:
                    foreach (var itemId in TargettedItems.Where(itemId => Items.HasItem(itemId) && Items.CanUseItem(itemId)))
                    {
                        if (target != null)
                        {
                            Items.UseItem(itemId, target);
                        }
                    }
                    break;
                case 2:
                    foreach (var itemId in AoeOffenseItems.Where(itemId => Items.HasItem(itemId) && Items.CanUseItem(itemId)))
                    {
                        Items.UseItem(itemId);
                    }
                    break;
                case 3:
                    foreach (var itemId in AoeDefenseItems.Where(itemId => Items.HasItem(itemId) && Items.CanUseItem(itemId)))
                    {
                        Items.UseItem(itemId);
                    }
                    break;
            }
        }

        public static double CastItemsDamage(Obj_AI_Hero target = null)
        {
            double id = 0;
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
            {
                id += ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Bilgewater);
            }
            if (Items.HasItem(3153) && Items.CanUseItem(3153))
            {
                id += ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Botrk);
            }
            if (Items.HasItem(3146) && Items.CanUseItem(3146))
            {
                id += ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Hexgun);
            }
            return id;
        }

        public static void Notify(string msg, Color color, int duration = -1, bool dispose = true)
        {
            Notifications.AddNotification(new Notification(msg, duration, dispose).SetTextColor(color));
        }

        public static void Reset()
        {
            LockedTargetSelector.UnlockTarget();
            myOrbwalker.CustomPointReset();
        }

        public static bool IsFacing(Obj_AI_Hero source, Vector3 direction, float angle = 90)
        {
            if (source == null || direction == Vector3.Zero)
            {
                return false;
            }
            return source.Direction.To2D().Perpendicular().AngleBetween((direction - source.Position).To2D()) <= angle;
        }

        public static int RandomDelay(int x, int y)
        {
            var random = new Random();
            return random.Next(x + Game.Ping, y + Game.Ping);
        }

        public static Vector3 RandomPos(int x, int y, int range, Vector3 pos)
        {
            var random = new Random();
            var a = Math.PI * random.Next(x, y);
            return new Vector3(pos.X + range * (float) Math.Cos(a), pos.Y + range * (float) Math.Sin(a), pos.Z);
        }

        public static void BuffCheck(Obj_AI_Hero target)
        {
            foreach (var x in target.Buffs)
            {
                Console.WriteLine(x.Name);
            }
        }

        public static Tuple<int, List<Obj_AI_Hero>> SpellHits(Spell spell)
        {
            var Hits =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        enemy =>
                            enemy.IsValidTarget() && spell.WillHit(enemy, spell.GetPrediction(enemy, true).CastPosition))
                    .ToList();
            return new Tuple<int, List<Obj_AI_Hero>>(Hits.Count, Hits);
        }
    }
}
