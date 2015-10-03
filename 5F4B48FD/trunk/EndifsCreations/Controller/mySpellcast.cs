#region Todo
    //      Dash
    //      Wall, pins to wall. bool pin ? else default?
    //      Circular Toggle, togglestate condition
    //      Charged spells, Sion / Varus / Vi / Xerath
    //      Fix CircularPrecise isn't "precise"
#endregion Todo

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace EndifsCreations.Controller
{
    internal class mySpellcast : PluginData
    {
        private static Vector3 Pos;
        private static PredictionOutput Pred;

        //  <summary>
        //      Spell cast blocking
        //  </summary>
        private static bool CanCast = true;
        private static void SetAllow(bool value)
        {
            CanCast = value;
        }

        private static bool AllowCasting
        {
            get
            {
                return CanCast && !Player.IsWindingUp && !myOrbwalker.Waiting;
            }
        }
        public static void Pause(int time)
        {
            SetAllow(false);
            Utility.DelayAction.Add(time, () =>
            {
                SetAllow(true);
            });
        }

        //  <summary>
        //      Use for spells that charges up and release
        //  </summary>
        public enum ChargeState
        {            
            Start,
            Release,
            Discharge
        }
        private static int LastCharge;

        public static void Charge(
            Obj_AI_Hero target, 
            Spell spell,
            ChargeState state,
            HitChance hitchance = HitChance.OutOfRange,             
            bool maxcharge = false,
            float maxrange = 0, 
            float adjust = 0,
            float maxchargetime = 0)
        {
            //calculate target inside max range, charge if valid
            //if target no longer valid max range, bool switch target ? discharge (vi full cd end channel) ? go cd (varus / xerath mana refund end channel).
            //always max ? quick release
            //quick release condition
            //adjust +/-
            if (!AllowCasting) return;            
            Pred = spell.GetPrediction(target);
            switch (state)
            {
                case ChargeState.Start:
                    if (!spell.IsCharging)
                    {
                        spell.StartCharging();
                        LastCharge = myUtility.TickCount;
                    }
                    break;
                case ChargeState.Release:
                    if (spell.IsCharging)
                    {
                        Pos = target.ServerPosition.Extend(Pred.CastPosition, adjust/2);
                        
                            switch (maxcharge)
                            {
                                case true:
                                    if (Vector3.Distance(Player.ServerPosition, Pred.CastPosition) <= maxrange && spell.Range >= maxrange)
                                    {
                                        if (myUtility.TickCount - LastCharge >= maxchargetime - 500)
                                        {
                                            if (Pred.Hitchance >= hitchance)
                                            {
                                                spell.Cast(Pos);
                                            }
                                        }
                                    }
                                    break;
                                case false:
                                    if (Vector3.Distance(Player.ServerPosition, Pred.CastPosition) <= maxrange)
                                    {
                                        if (myUtility.MovementDisabled(target) || myUtility.TickCount - LastCharge >= maxchargetime - 500)
                                        {
                                            if (Pred.Hitchance >= hitchance)
                                            {
                                                spell.Cast(Pos);
                                            }
                                        }
                                    }
                                    break;
                            }                        
                    }
                    break;
                case ChargeState.Discharge:
                    if (spell.IsCharging)
                    {
                        if (myUtility.TickCount - LastCharge >= maxchargetime - 500)
                        {
                            spell.Cast(Player.ServerPosition.Extend(Game.CursorPos, maxrange));
                        }
                    }
                    break;
            }
        }
        //  <summary>
        //      Use for spells that can be toggle on and off
        //  </summary>
        public static void Toggle(Obj_AI_Hero target, Spell spell, SpellSlot spellslot, int count, float range)
        {
            if (!AllowCasting) return;
            if (target != null)
            {
                if (Player.Spellbook.GetSpell(spellslot).ToggleState == 1 && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= range)
                {
                    spell.Cast();
                }
                else if (Player.Spellbook.GetSpell(spellslot).ToggleState != 1 && Vector3.Distance(Player.ServerPosition, target.ServerPosition) > range)
                {
                    spell.Cast();
                }
            }
            else
            {
                if (Player.Spellbook.GetSpell(spellslot).ToggleState == 1 && Player.CountEnemiesInRange(range) > count)
                {
                    spell.Cast();
                }
                else if (Player.Spellbook.GetSpell(spellslot).ToggleState != 1 && Player.CountEnemiesInRange(range) <= count)
                {
                    spell.Cast();
                }
            }
        }

        //  <summary>
        //      Use for spells that creates collisionable objects
        //  </summary>
        public static void Wall(Obj_AI_Hero target, Spell spell, HitChance hitchance, bool front = false)
        {
            if (!AllowCasting) return;
            PredictionOutput pred = spell.GetPrediction(target);
            if (myUtility.MovementDisabled(target))
            {
                switch (front)
                {
                    case true:
                        Pos = Player.ServerPosition.Extend(target.ServerPosition, Vector3.Distance(Player.ServerPosition, target.ServerPosition) - target.BoundingRadius - 10);
                        spell.Cast(Pos);
                        break;
                    case false:
                        Pos = Player.ServerPosition.Extend(target.ServerPosition, Vector3.Distance(Player.ServerPosition, target.ServerPosition) + target.BoundingRadius + 10);
                        spell.Cast(Pos);
                        break;                   
                }
            }
            if (pred.Hitchance >= hitchance && Vector3.Distance(Player.ServerPosition, pred.CastPosition) <= spell.Range + (spell.Width / 2))
            {
                switch (front)
                {
                    case true:
                        Pos = Player.ServerPosition.Extend(target.ServerPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition) - target.BoundingRadius);
                        spell.Cast(Pos);
                        break;
                    case false:
                        Pos = Player.ServerPosition.Extend(target.ServerPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition) + target.BoundingRadius);
                        spell.Cast(Pos);
                        break;
                }
            }
        }

        //  <summary>
        //      Use for hook spells
        //  </summary>
        public static void Hook(Obj_AI_Hero target, Spell spell)
        {
            if (!AllowCasting) return;
            Pred = spell.GetPrediction(target);
            var box = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(target.ServerPosition, (spell.Range - (target.BoundingRadius / 2) - spell.Width)), spell.Width);
            if (Pred.CollisionObjects.Count(x => !x.IsAlly && x.IsMinion && x.IsHPBarRendered) == 0 && box.IsInside(target))
            {
                if (!target.IsMoving || myUtility.MovementDisabled(target))
                {
                    spell.Cast(box.End);
                }
                else if (box.IsInside(Pred.CastPosition) && Vector2.Distance(target.ServerPosition.To2D(), myUtility.PredictMovement(target, spell.Delay, spell.Speed).To2D()) <= spell.Width + target.BoundingRadius)
                {
                    spell.Cast(Pred.CastPosition);
                }
            }
        }

        //  <summary>
        //      Use for On Unit
        //  </summary>
        public static void Unit(Obj_AI_Hero target, Spell spell)
        {
            if (!AllowCasting) return;
            if (target == null)
            {
                spell.CastOnUnit(Player);
            }
            else 
            {
                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= spell.Range)
                {
                    spell.CastOnUnit(target);
                }
            }
        }

        //  <summary>
        //      Use for linear spells with secondary range on collision
        //  </summary>
        public static void Extension(Obj_AI_Hero target, Spell spell, float castrange, float maxrange, bool linear = true, bool minion = true, bool enemy = false)
        {
            if (!AllowCasting) return;
            if (target == null || !target.IsValidTarget())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= maxrange).OrderBy(i => i.Health);
                foreach (var potential in EnemyList)
                {
                    if (linear)
                    {
                        var box = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(potential.ServerPosition, maxrange), spell.Width);
                        if (minion)
                        {
                            if (box.Points.Any(x => MinionManager.GetMinions(x.To3D(), spell.Width).Any()))
                            {
                                spell.Cast(Player.ServerPosition.Extend(potential.ServerPosition, castrange));
                            }
                        }
                    }
                    else
                    {
                        var box = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(potential.ServerPosition, maxrange), potential.BoundingRadius);
                        var minioninbox = MinionManager.GetMinions(Player.Position, castrange).Where(x => box.IsInside(x)).ToList();
                        var enemyinbox = HeroManager.Enemies.Where(x => box.IsInside(x) && x.IsValidTarget(castrange)).ToList();
                        if (minion && minioninbox.Any())
                        {
                            spell.Cast(minioninbox[0]);
                        }
                        if (enemy && enemyinbox.Any())
                        {
                            spell.Cast(enemyinbox[0]);
                        }
                    }
                }
            }
            else
            {
                if (linear)
                {
                    var box = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(target.ServerPosition, maxrange), spell.Width);
                    if (minion)
                    {
                        if (box.Points.Any(x => MinionManager.GetMinions(x.To3D(), spell.Width).Any()))
                        {
                            spell.Cast(Player.ServerPosition.Extend(target.ServerPosition, castrange));
                        }
                    }
                }
                else
                {
                    var box = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(target.ServerPosition, maxrange), target.BoundingRadius);
                    if (minion)
                    {
                        if (Player.ChampionName.Equals("Yasuo"))
                        {
                            var minioninbox = MinionManager.GetMinions(Player.Position, castrange).Where(x => box.IsInside(x) && !x.HasBuff("YasuoDashWrapper")).ToList();
                            if (minioninbox.Any())
                            {
                                minioninbox.Reverse();
                                spell.Cast(minioninbox[0]);
                            }
                        }
                        else
                        {
                            var minioninbox = MinionManager.GetMinions(Player.Position, castrange).Where(x => box.IsInside(x)).ToList();
                            if (minioninbox.Any())
                            {
                                spell.Cast(minioninbox[0]);
                            }
                        }
                    }
                    if (enemy)
                    {
                        var enemyinbox = HeroManager.Enemies.Where(x => box.IsInside(x) && x.IsValidTarget(castrange)).ToList();
                        if (enemyinbox.Any())
                        {
                            spell.Cast(enemyinbox[0]);
                        }
                    }
                }
            }
        }

        //  <summary>
        //      Cast linear spell to vector
        //  </summary>
        public static void PointVector(Vector3 vecpos, Spell spell, float extend = 0)
        {
            if (!AllowCasting) return;
            if (Vector3.Distance(Player.ServerPosition, vecpos) <= spell.Range + extend)
            {
                spell.Cast(Player.ServerPosition.Extend(vecpos, Vector3.Distance(Player.ServerPosition, vecpos) + extend));
            }
        }

        //  <summary>
        //      Cast circular aoe spells around player
        //  </summary>
        public static void PointBlank(Obj_AI_Hero target, Spell spell = null, float radius = 0, int count = 0)
        {
            if (!AllowCasting || spell == null) return;
            var findrange = radius > 300 ? (radius * 9 / 10) : radius; //topkek math
            if (target != null)
            {
                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= findrange)
                {
                    if (count > 0)
                    {
                        if (Player.CountEnemiesInRange(findrange) - 1 > count)
                        {
                            spell.Cast();
                        }
                    }
                    else
                    {
                        spell.Cast();
                    }
                    
                }
            }
            else
            {
                if (Player.CountEnemiesInRange(findrange) > count)
                {
                    spell.Cast();
                }
            }
        }

        #region Linear
        //  <summary>
        //      Use for spells that can be cast in linear ways
        //  </summary>
        public static void Linear(Obj_AI_Hero target, Spell spell, HitChance hitchance, bool collision = false, int passable = 0)
        {
            if (!AllowCasting) return;
            Pred = spell.GetPrediction(target);
            if (myUtility.MovementDisabled(target))
            {
                Pos = Player.ServerPosition.Extend(target.ServerPosition, Vector3.Distance(Player.ServerPosition, target.ServerPosition));
                switch (collision)
                {
                    case true:
                        if (Pred.CollisionObjects.Count <= passable)
                        {
                            spell.Cast(Pos);
                        }
                        break;
                    case false:
                        spell.Cast(Pos);
                        break;
                }
            }
            if (Vector3.Distance(Player.ServerPosition, Pred.CastPosition) <= spell.Range)
            {
                Pos = Player.ServerPosition.Extend(Pred.CastPosition, Vector3.Distance(Player.ServerPosition, Pred.CastPosition));
                switch (collision)
                {
                    case true:
                        if (Pred.Hitchance >= hitchance)
                        {
                            if (Pred.CollisionObjects.Count <= passable)
                            {
                                spell.Cast(Pos);
                            }
                        }
                        break;
                    case false:
                        if (myUtility.MovingAway(target) && myUtility.IsFacing(Player, target.ServerPosition))
                        {
                            myDevTools.DebugMode("Running away");
                            if (Vector3.Distance(target.ServerPosition, Player.ServerPosition) < spell.Range / 2)
                            {
                                Pos = Player.ServerPosition.Extend(target.ServerPosition, spell.Range);
                                spell.Cast(Pos);
                                myDevTools.DebugMode("running away, chase, cast1");
                            }
                            else if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) > spell.Range / 2 && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < spell.Range)
                            {
                                var testdist = Vector3.Distance(Player.ServerPosition, target.ServerPosition) + (spell.Range / 2);
                                var testchace = Player.ServerPosition.Extend(target.ServerPosition, testdist > spell.Range ? spell.Range : testdist);
                                spell.Cast(testchace);
                                myDevTools.DebugMode("running away, chase, cast2");
                            }
                        }
                        else if (Pred.Hitchance >= hitchance)
                        {
                            spell.Cast(Pos);
                            myDevTools.DebugMode("hitchance normal");
                            if (target.IsMoving && myUtility.IsFacing(target, Player.ServerPosition))
                            {
                                spell.Cast(Pred.CastPosition);
                                myDevTools.DebugMode("hitchance - moving, cast3");
                            }
                            else
                            {
                                myDevTools.DebugMode("hitchance, cast4");
                                spell.Cast(Pos);
                            }
                        }                       
                        break;
                }
            }
        }

        //  <summary>
        //      Use for spells that shows distinct lines when casted, randomize end points to humanize
        //  </summary>
        public static void LinearRandomized(Obj_AI_Hero target, Spell spell, HitChance hitchance, bool collision = false, bool random = false, int passable = 0, int min = 0, int max = 25, int range = 25)
        {
            if (!AllowCasting) return;
            Pred = spell.GetPrediction(target);
            if (myUtility.MovementDisabled(target))
            {
                Pos = random ? 
                    myUtility.RandomPos(min, max, range, Player.ServerPosition.Extend(target.ServerPosition, Vector3.Distance(Player.ServerPosition, target.ServerPosition))) :                                        
                    Player.ServerPosition.Extend(target.ServerPosition, Vector3.Distance(Player.ServerPosition, target.ServerPosition));

                switch (collision)
                {
                    case true:
                        if (Pred.CollisionObjects.Count <= passable)
                        {
                            spell.Cast(Pos);
                        }
                        break;
                    case false:
                        spell.Cast(Pos);
                        break;
                }
            }
            if (Vector3.Distance(Player.ServerPosition, Pred.CastPosition) <= spell.Range)
            {
                Pos = random ?
                myUtility.RandomPos(min, max, range, Player.ServerPosition.Extend(Pred.CastPosition, Vector3.Distance(Player.ServerPosition, Pred.CastPosition))) :
                Player.ServerPosition.Extend(Pred.CastPosition, Vector3.Distance(Player.ServerPosition, Pred.CastPosition));

                switch (collision)
                {
                    case true:
                        if (Pred.Hitchance >= hitchance)
                        {
                            if (Pred.CollisionObjects.Count <= passable)
                            {
                                spell.Cast(Pos);
                            }
                        }
                        break;
                    case false:
                        if (myUtility.MovingAway(target) && myUtility.IsFacing(Player, target.ServerPosition))
                        {
                            myDevTools.DebugMode("Running away");
                            if (Pred.Hitchance >= hitchance)
                            {
                                spell.Cast(Pos);
                                myDevTools.DebugMode("hitchance");
                            }
                            if (Vector3.Distance(target.ServerPosition, Player.ServerPosition) <= Math.Min(300, (spell.Range / 2)) &&
                                Vector3.Distance(Player.ServerPosition.Extend(target.ServerPosition, spell.Range), target.ServerPosition) < spell.Range)
                            {
                                spell.Cast(target.ServerPosition);
                                myDevTools.DebugMode("chase");
                            }
                        }
                        else if (Pred.Hitchance >= hitchance)
                        {
                            spell.Cast(Pos);
                            myDevTools.DebugMode("hitchance normal");
                        } 
                        break;
                }
            }
        }        

        //  <summary>
        //      Use for rectangular spells without target
        //  </summary>
        public static void LinearBox(Spell spell, HitChance hitchance, int minimum = 1)
        {
            if (!AllowCasting) return;
            var box = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(Game.CursorPos, spell.Range), spell.Width);
            var insidebox = HeroManager.Enemies.Where(x => box.IsInside(x) && x.IsValidTarget()).ToList();
            if (insidebox.Any())
            {
                var predlist = new List<Obj_AI_Hero>();
                foreach (var t in insidebox)
                {
                    Pred = spell.GetPrediction(t);
                    if (Pred.Hitchance >= hitchance)
                    {
                        predlist.Add(t);
                    }
                }
                if (predlist.Count >= minimum)
                {
                    spell.Cast(box.End);
                }
            }
        }

        //  <summary>
        //      Viktor and Rumble, also, these shouldn't be blocked by windwall fyi, thanks rito
        //  </summary>
        public static void LinearTwoPoints(Obj_AI_Hero target, Spell spell, float castrange, float pathlength = 0, int minimum = 0)
        {
            if (!AllowCasting) return;
            //Start point, target or mouse
            //loops targets within maxrange, 
            //basic. expanding soon tm
            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= castrange && Vector3.Distance(Game.CursorPos, target.ServerPosition) <= castrange)
            {
                spell.Cast(Player.ServerPosition.Extend(Game.CursorPos, Vector3.Distance(Player.ServerPosition, Game.CursorPos)), target.ServerPosition);
            }
            else if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= castrange + pathlength)
            {
                spell.Cast(Player.ServerPosition.Extend(target.ServerPosition, castrange), target.ServerPosition);
            }
        }

        #endregion Linear

        #region Circular
        //  <summary>
        //      Use for large circular spells in between player and target
        //  </summary>
        public static void CircularBetween(Obj_AI_Hero target, Spell spell)
        {
            if (!AllowCasting) return;
            var dis = Vector3.Distance(Player.ServerPosition, target.ServerPosition);
            var mid = dis / 2;
            Pos = Player.ServerPosition.Extend(target.ServerPosition, mid + target.BoundingRadius);
            if (Vector3.Distance(Player.ServerPosition, Pos) <= spell.Range && Vector3.Distance(target.ServerPosition, Pos) <= 500)
            {
                spell.Cast(Pos);
            }
        }

        //  <summary>
        //      Use for aoe circular spells, checks for other potential target, if not goes for singular
        //  </summary>
        public static void CircularAoe(Obj_AI_Hero target, Spell spell, HitChance hitchance, float castrange = 0, float radius = 0)
        {
            if (!AllowCasting) return;
            var findrange = castrange + Math.Max(0, radius > 300 ? (radius * 2 / 3) : radius);
            if (myUtility.MovementDisabled(target))
            {
                Pos = myUtility.RandomPos(1, 25, 25, Player.ServerPosition.Extend(target.ServerPosition, Vector3.Distance(Player.ServerPosition, target.ServerPosition)));
                spell.Cast(Pos);
            }
            var nearby = HeroManager.Enemies.Where(x => x.IsValidTarget(findrange) && x != target && Vector3.Distance(target.ServerPosition, x.ServerPosition) <= Math.Max(0, radius > 300 ? (radius * 3 / 4) : radius)).ToList();
            if (nearby.Any())
            {               
                var test = nearby.Where(o => spell.GetPrediction(o).Hitchance >= HitChance.Medium).ToList();
                if (!test.Any())
                {
                    CircularPrecise(target, spell, hitchance, spell.Range);
                }
                else 
                {
                    Pred = spell.GetPrediction(target, true, findrange);
                    if (Pred.AoeTargetsHitCount >= test.Count)
                    {
                        Pos = Player.ServerPosition.Extend(Pred.CastPosition, Math.Min(findrange, Vector3.Distance(Player.ServerPosition, Pred.CastPosition)));
                        spell.Cast(Pos);
                        myDevTools.DebugMode("AOE will hit: " + test.Count);
                    }
                }
            }
            else
            {
                CircularPrecise(target, spell, hitchance, spell.Range);
            }
        }

        //  <summary>
        //      Use for ground targetted spells, e.g traps
        //  </summary>
        public static void CircularPrecise(Obj_AI_Hero target, Spell spell, HitChance hitchance, float castrange, float radius = 0, int min = 0, int max = 10, int range = 10)
        {
            if (!AllowCasting) return;
            var findrange = castrange + Math.Max(0, radius > 300 ? (radius * 2 / 3) : radius);
            Pred = spell.GetPrediction(target);
            if (myUtility.MovementDisabled(target) && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= findrange)
            {
                Pos = myUtility.RandomPos(min, max, range, Player.ServerPosition.Extend(Pred.CastPosition, Vector3.Distance(Player.ServerPosition, target.ServerPosition)));
                spell.Cast(Pos);
            }
            if (myUtility.MovingAway(target) && myUtility.IsFacing(Player, target.ServerPosition))
            {
                myDevTools.DebugMode("Running away 2");

                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < radius)
                {
                    Pos = myUtility.RandomPos(min, max, range, Player.ServerPosition.Extend(target.ServerPosition, Vector3.Distance(Player.ServerPosition, target.ServerPosition) + (radius * 1 / 3)));
                    spell.Cast(Pos);
                    myDevTools.DebugMode("precise - running away, in radius");
                }
                else if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) > radius && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < findrange)
                {
                    var testdist = Vector3.Distance(Player.ServerPosition, target.ServerPosition) + (radius * 2 / 3);
                    var testchace = Player.ServerPosition.Extend(target.ServerPosition, testdist > findrange ? findrange : testdist);
                    spell.Cast(testchace);
                    myDevTools.DebugMode("precise - running away, chase");
                }
            }
            else if (Vector3.Distance(Player.ServerPosition, Pred.CastPosition) <= findrange)
            {
                var centertotarget = Pred.CastPosition.Extend(target.ServerPosition, radius);
                if (Vector3.Distance(Pred.CastPosition, target.ServerPosition) <= Vector3.Distance(Pred.CastPosition, centertotarget))
                {
                    myDevTools.DebugMode("precise - in radius, cast3");
                    spell.Cast(Pred.CastPosition);
                }
                if (Pred.Hitchance >= hitchance)
                {
                    if (target.IsMoving && myUtility.IsFacing(target, Player.ServerPosition))
                    {
                        spell.Cast(Pred.CastPosition);
                        myDevTools.DebugMode("precise - hitchance - moving, cast4");
                    }
                    else
                    {
                        myDevTools.DebugMode("precise - hitchance, cast5");
                        spell.Cast(Pred.CastPosition);
                    }
                }
            }
        }

        //  <summary>
        //      Use for circular spells without hitchance checks
        //  </summary>
        public static void Circular(Vector3 vecpos, Spell spell, int min = 0, int max = 25, int range = 25)
        {
            if (!AllowCasting) return;
            Pos = myUtility.RandomPos(min, max, range, Player.ServerPosition.Extend(vecpos, Vector3.Distance(Player.ServerPosition, vecpos)));
            spell.Cast(Pos);
        }

        #endregion Circular        

        #region Dash
        // <summary>
        //Dash.End ?
        //dash resets?
        //count enemies near Dash.End?
        //should damage enemies or just gapclosing?
        //jump near target when not in range (minions, other allies / enemies ? )
        //http://leagueoflegends.wikia.com/wiki/Dash
        // </summary>

        //  <summary>
        //      Use for linear dash spells, Shen, Leona, Renekton, Sejuani
        //  </summary>
        public static void DashLinear(Obj_AI_Hero target, Spell spell, float castrange, bool maxrange = false, int minhit = 1, bool safecheck = false)
        {
            if (!AllowCasting) return;
            Pred = spell.GetPrediction(target);
            var box = new Geometry.Polygon.Rectangle(
                Player.ServerPosition, 
                Player.ServerPosition.Extend(target.ServerPosition, 
                maxrange ? 
                castrange : 
                (castrange - (target.BoundingRadius / 2) - spell.Width)),
                spell.Width);
            if (minhit == 1)
            {
                if (box.IsInside(target))
                {
                    if (myUtility.MovementDisabled(target))
                    {
                        switch (safecheck)
                        {
                            case true:
                                spell.Cast(Pred.CastPosition);
                                break;
                            case false:
                                spell.Cast(box.End);
                                break;
                        }
                    }
                    else if (box.IsInside(Pred.CastPosition) && Vector2.Distance(target.ServerPosition.To2D(), myUtility.PredictMovement(target, spell.Delay, spell.Speed).To2D()) <= spell.Width + target.BoundingRadius)
                    {
                        switch (safecheck)
                        {
                            case true:
                                spell.Cast(Pred.CastPosition);
                                break;
                            case false:
                                spell.Cast(Pred.CastPosition);
                                break;
                        }                        
                    }
                }
            }
            else
            {
                
            }
        }

        //  <summary>
        //      Use for jump spells, e.g Tristana's rocket jump
        //  </summary>
        public static void DashCircular(Obj_AI_Hero target, Spell spell)
        {
            if (!AllowCasting) return;
        }

        //  <summary>
        //      Use for targetted dash/jump spells, e.g Irelia's Q, Jax's Q
        //  </summary>
        public static void DashTargetted(Obj_AI_Hero target, Spell spell)
        {
            if (!AllowCasting) return;
        }

        #endregion Dash
    }
}