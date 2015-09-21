using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Controller
{
    internal class myUtility : PluginData
    {
        public static float PlayerHealthPercentage
        {
            get { return ObjectManager.Player.Health * 100 / ObjectManager.Player.MaxHealth; }
        }

        public static float PlayerManaPercentage
        {
            get { return ObjectManager.Player.Mana * 100 / ObjectManager.Player.MaxMana; }
        }        

        public static int TickCount
        {
            get { return (int) (Game.Time * 1000); }
        }

        public static bool EnoughHealth(float value)
        {
            return PlayerHealthPercentage >= value;
        }

        public static bool EnoughMana(float value)
        {
            return PlayerManaPercentage >= value;
        }

        public static bool ImmuneToMagic(Obj_AI_Hero target)
        {
            return
                target.HasBuff("sivire") ||
                target.HasBuff("nocturneshroudofdarkness") ||
                target.HasBuff("bansheesveil") ||
                target.HasBuffOfType(BuffType.SpellImmunity);
                //diplomatic immunity && i'm not target 
        }

        public static bool ImmuneToCC(Obj_AI_Hero target)
        {
            return
                target.HasBuff("bansheesveil") ||
                //riposte
                target.HasBuff("blackshield") ||
                target.HasBuff("nocturneshroudofdarkness") ||
                target.HasBuff("ragnarok") ||
                //diplomatic immunity && i'm not target
                target.HasBuff("sivire");
        }

        public static bool ImmuneToDeath(Obj_AI_Hero target)
        {
            return (
                target.HasBuff("JudicatorIntervention") ||
                (target.HasBuff("Undying Rage") && (target.Health * 100 / target.MaxHealth) < 5) ||
                (target.HasBuff("Chrono Shift") && (target.Health * 100 / target.MaxHealth) < 5)
                //to do poppy
                );
        }
        
        public static bool MovementDisabled(Obj_AI_Hero target)
        {
            return target.HasBuffOfType(BuffType.Stun) || 
                   target.HasBuffOfType(BuffType.Snare) ||
                   target.HasBuffOfType(BuffType.Knockup) ||
                   target.HasBuffOfType(BuffType.Knockback) ||
                   target.HasBuffOfType(BuffType.Charm) || 
                   target.HasBuffOfType(BuffType.Fear) ||
                   target.HasBuffOfType(BuffType.Taunt) || 
                   target.HasBuffOfType(BuffType.Suppression) ||
                   target.IsStunned || 
                   target.IsChannelingImportantSpell();
        }

        public static float DisabledDuration(Obj_AI_Hero target)
        {
            return target.Buffs.Where(x => x.IsActive && Game.Time < x.EndTime &&
                (
                x.Type == BuffType.Stun ||
                x.Type == BuffType.Snare ||
                x.Type == BuffType.Knockback ||
                x.Type == BuffType.Knockup ||
                x.Type == BuffType.Charm ||
                x.Type == BuffType.Fear ||
                x.Type == BuffType.Taunt ||
                x.Type == BuffType.Suppression
                ))
                .Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time;
        }

        public static bool IsInBush(Obj_AI_Base target)
        {
            return NavMesh.IsWallOfGrass(target.Position, target.BoundingRadius);
        }

        public static double HitsToKill(Obj_AI_Base target)
        {
            return target.Health / ObjectManager.Player.GetAutoAttackDamage(target);
        }

        public static void Notify(string msg, Color color, int duration = -1, bool dispose = true)
        {
            Notifications.AddNotification(new Notification(msg, duration, dispose).SetTextColor(color));
        }

        public static void Reset()
        {
            LastTarget = null;
            LockedTarget = null;
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

        public static int RandomRange(int x, int y)
        {
            var random = new Random();
            return random.Next(x, y);
        }

        public static Vector3 RandomPos(int x, int y, int range, Vector3 pos)
        {
            var random = new Random();
            var a = Math.PI * random.Next(x, y);
            return new Vector3(pos.X + range * (float) Math.Cos(a), pos.Y + range * (float) Math.Sin(a), pos.Z);
        }        

        public static Tuple<int, List<Obj_AI_Hero>> SpellHits(Spell spell)
        {
            var hits =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        enemy =>
                            enemy.IsValidTarget() && spell.WillHit(enemy, spell.GetPrediction(enemy, true).CastPosition))
                    .ToList();
            return new Tuple<int, List<Obj_AI_Hero>>(hits.Count, hits);
        }

        public static Obj_AI_Hero GetTarget(float range, TargetSelector.DamageType damagetype, bool locked = false)
        {
            if (locked)
            {
                if (LockedTarget == null)
                {
                    LockedTarget = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() && !ImmuneToDeath(TargetSelector.GetSelectedTarget()) ?
                        TargetSelector.GetSelectedTarget() :
                        TargetSelector.GetTarget(range, damagetype);
                }
            }
            return 
                locked && LockedTarget.IsValidTarget(range)
                ? LockedTarget 
                : TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() && !ImmuneToDeath(TargetSelector.GetSelectedTarget()) 
                ? TargetSelector.GetSelectedTarget() 
                : TargetSelector.GetTarget(range, damagetype);            
        }

        // https://github.com/LeagueSharp/LeagueSharp.Common/blob/master/Utility.cs#L208-L215
        public static bool PointCollides(Vector3 vec)
        {
            return NavMesh.GetCollisionFlags(vec).HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(vec).HasFlag(CollisionFlags.Building);
        }

        public static Vector2 PredictMovement(Obj_AI_Hero target, float time, float speed = float.MaxValue)
        {
            var movement = time * speed;
            var path = target.GetWaypoints();

            for (var i = 0; i < path.Count - 1; i++)
            {
                var to = path[i + 1];
                var from = path[i];

                var dist = Vector2.Distance(from,to);

                if (dist < movement)
                {
                    movement -= dist;
                }
                else
                {
                    return from + movement * (to - from).Normalized();
                }
            }
            return path[path.Count - 1];
        }

        public static bool MovingAway(Obj_AI_Hero target)
        {                      
            return !IsFacing(target, Player.ServerPosition) && target.IsMoving && !MovementDisabled(target);
        }
        public static bool IsInvulnerable(Obj_AI_Hero target)
        {
            return target.IsInvulnerable || target.HasBuff("JudicatorIntervention") || target.HasBuff("Undying Rage") || target.HasBuff("Chrono Shift");
        }
    }
}