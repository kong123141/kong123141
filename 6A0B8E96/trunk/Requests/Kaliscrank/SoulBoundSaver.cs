using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

using Settings = KalistaResurrection.Config.Misc;

namespace KalistaResurrection
{
    public class SoulBoundSaver
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static Spell R { get { return SpellManager.R; } }
        public static Obj_AI_Hero SoulBound { get; private set; }
        private static Spell _q, _e, _r;
        private static Dictionary<float, float> _incomingDamage = new Dictionary<float, float>();
        private static Dictionary<float, float> _instantDamage = new Dictionary<float, float>();
        public static float IncomingDamage
        {
            get { return _incomingDamage.Sum(e => e.Value) + _instantDamage.Sum(e => e.Value); }
        }

        public static void Initialize()
        {
            // Listen to related events
            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }

        private static void OnUpdate(EventArgs args)
        {
            
            // SoulBound is not found yet!
            if (SoulBound == null)
            {
                // TODO: Get the buff display name, I'm not at home so I needed to use xQx' method, which I don't like :D
                SoulBound = HeroManager.Allies.Find(h => h.Buffs.Any(b => b.Caster.IsMe && b.Name.Contains("kalistacoopstrikeally")));
            }
            else if (Settings.SaveSouldBound && R.IsReady())
            {
                // Ult casting
                if (SoulBound.HealthPercentage() < 5 && SoulBound.CountEnemiesInRange(500) > 0 ||
                    IncomingDamage > SoulBound.Health)
                    R.Cast();
                // Get enemies
                foreach (var unit in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy && h.IsHPBarRendered))
                {
                    // Get buffs
                    for (int i = 0; i < unit.Buffs.Count(); i++)
                    {
                        // Check if the Soulbound is in a good range
                        var enemy = HeroManager.Enemies.Where(x => SoulBound.Distance(x.Position) > 800);
                        // Check if the Soulbound is a Blitzcrank
                        // Check if the enemy is hooked
                        // Check if target was far enough for ult
                        if (SoulBound.ChampionName == "Blitzcrank" && unit.Buffs[i].Name == "rocketgrab2" && unit.Buffs[i].IsActive && enemy.Count() > 0 && Config.Misc.UseKaliscrank)
                        {
                            R.Cast();
                        }
                    }
                }
            }

            // Check spell arrival
            foreach (var entry in _incomingDamage)
            {
                if (entry.Key < Game.Time)
                    _incomingDamage.Remove(entry.Key);
            }

            // Instant damage removal
            foreach (var entry in _instantDamage)
            {
                if (entry.Key < Game.Time)
                    _instantDamage.Remove(entry.Key);
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy)
            {
                // Calculations to save your souldbound
                if (SoulBound != null && Settings.SaveSouldBound)
                {
                    // Auto attacks
                    if ((!(sender is Obj_AI_Hero) || args.SData.IsAutoAttack()) && args.Target != null && args.Target.NetworkId == SoulBound.NetworkId)
                    {
                        // Calculate arrival time and damage
                        _incomingDamage.Add(SoulBound.ServerPosition.Distance(sender.ServerPosition) / args.SData.MissileSpeed + Game.Time, (float)sender.GetAutoAttackDamage(SoulBound));
                    }
                    // Sender is a hero
                    else if (sender is Obj_AI_Hero)
                    {
                        var attacker = (Obj_AI_Hero)sender;
                        var slot = attacker.GetSpellSlot(args.SData.Name);

                        if (slot != SpellSlot.Unknown)
                        {
                            if (slot == attacker.GetSpellSlot("SummonerDot") && args.Target != null && args.Target.NetworkId == SoulBound.NetworkId)
                            {
                                // Ingite damage (dangerous)
                                _instantDamage.Add(Game.Time + 2, (float)attacker.GetSummonerSpellDamage(SoulBound, Damage.SummonerSpell.Ignite));
                            }
                            else if (slot.HasFlag(SpellSlot.Q | SpellSlot.W | SpellSlot.E | SpellSlot.R) &&
                                ((args.Target != null && args.Target.NetworkId == SoulBound.NetworkId) ||
                                args.End.Distance(SoulBound.ServerPosition) < Math.Pow(args.SData.LineWidth, 2)))
                            {
                                // Instant damage to target
                                _instantDamage.Add(Game.Time + 2, (float)attacker.GetSpellDamage(SoulBound, slot));
                            }
                        }
                    }
                }
            }
        }
    }
}
