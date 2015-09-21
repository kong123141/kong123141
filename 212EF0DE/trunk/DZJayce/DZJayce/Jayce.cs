using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZJayce.Utility;
using LeagueSharp.Common;
using LeagueSharp;
using SharpDX;

namespace DZJayce
{
    class Jayce
    {
        #region
        public static Menu RootMenu { get; set; }

        public static Orbwalking.Orbwalker Orbwalker { get; set; }
        #endregion

        internal static void OnLoad()
        {
            LoadEvents();
            Console.WriteLine("Well hello there");
        }

        private static void LoadEvents()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling())
            {
                return;
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    OnCombo(Orbwalking.OrbwalkingMode.Combo);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    OnMixed();
                    break;
            }

            if (GetMenuValue<KeyBind>("dz191.jayce.misc.castqe").Active)
            {
                if (ObjectManager.Player.IsRangedForm())
                {
                    var RangedTarget = TargetSelector.GetTarget(
                        SpellHandler.spells[Spells.QGate].Range, TargetSelector.DamageType.Physical);

                    if (Spells.QRanged.IsSpellReady() && Spells.ERanged.IsSpellReady() && RangedTarget.IsValidTarget())
                    {
                        Console.WriteLine("Going to perform QE Combo");
                        CastQE(RangedTarget);
                    }
                }
            }

            if (GetMenuValue<KeyBind>("dz191.jayce.misc.castqemouse").Active)
            {
                if (ObjectManager.Player.IsRangedForm())
                {
                    if (Spells.QRanged.IsSpellReady() && Spells.ERanged.IsSpellReady())
                    {
                        CastQE(Game.CursorPos);
                    }
                }
            }
        }

        private static void OnCombo(Orbwalking.OrbwalkingMode currentMode)
        {
            var RangedTarget = TargetSelector.GetTarget(SpellHandler.spells[Spells.QGate].Range, TargetSelector.DamageType.Physical);

            if (ObjectManager.Player.IsRangedForm())
            {
                //Ranged. Time to be annoying.
                if (Spells.QRanged.IsSpellReady(currentMode) && Spells.ERanged.IsSpellReady(currentMode) && RangedTarget.IsValidTarget())
                {
                    Console.WriteLine("Going to perform QE Combo");

                }

            }
            else
            {
                //Melee. Time to slam dunk

            }
        }

        private static void OnMixed()
        {
            throw new NotImplementedException();
        }

        private static void CastQE(Obj_AI_Hero target)
        {
            if (target.IsValidTarget(SpellHandler.spells[Spells.QGate].Range))
            {
                var gatePosition = Helper.getGatePosition(target.ServerPosition);
                if (gatePosition != Vector3.Zero)
                {
                    var gateDistance = ObjectManager.Player.ServerPosition.Distance(gatePosition);
                    var qDelayToGate =  gateDistance / SpellHandler.spells[Spells.QRanged].Speed;
                    var distanceToTarget = gatePosition.Distance(target.ServerPosition);
                    var gateTargetDelay = distanceToTarget / SpellHandler.spells[Spells.QGate].Speed;
                    var totalDelay = qDelayToGate + gateTargetDelay;
                    var prediction = Prediction.GetPrediction(target, totalDelay);

                    if (prediction.Hitchance >= HitChance.High)
                    {
                        SpellHandler.spells[Spells.ERanged].Cast(gatePosition);
                        SpellHandler.spells[Spells.QRanged].Cast(prediction.CastPosition);
                    }
                }
            }
        }
        private static void CastQE(Vector3 Position)
        {
                var gatePosition = Helper.getGatePosition(Position);
                if (gatePosition != Vector3.Zero)
                {
                        SpellHandler.spells[Spells.ERanged].Cast(gatePosition);
                        SpellHandler.spells[Spells.QRanged].Cast(Position);
                }
        }
        
        private static T GetMenuValue<T>(string menuItem)
        {
            return RootMenu.Item(menuItem).GetValue<T>();
        }
    }
}
