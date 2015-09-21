namespace ElRengarRevamped
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class ActiveModes : Standards
    {
        #region Public Methods and Operators

        public static void Combo()
        {
            var target = TargetSelector.GetSelectedTarget();
            if (target == null || !target.IsValidTarget() || TargetSelector.GetSelectedTarget() == null)
            {
                target = TargetSelector.GetTarget(spells[Spells.R].Range, TargetSelector.DamageType.Physical);
            }

            if (TargetSelector.GetSelectedTarget() != null)
            {
                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < spells[Spells.R].Range)
                {
                    target = TargetSelector.GetSelectedTarget();
                    TargetSelector.SetTarget(target);
                    //Hud.SelectedUnit = target;
                    //Console.WriteLine("Selected target: {0}", target.ChampionName);
                }
            }

            target = TargetSelector.GetTarget(spells[Spells.R].Range, TargetSelector.DamageType.Physical);
            if (!target.IsValidTarget())
            {
                return;
            }

            if (Player.IsWindingUp)
            {
                return;
            }

            if (Youmuu.IsReady() && Player.Distance(target) <= spells[Spells.Q].Range)
            {
                Youmuu.Cast(Player);
            }

            #region RengarR

            if (Ferocity == 5)
            {
                switch (IsListActive("Combo.Prio").SelectedIndex)
                {
                    case 0:
                        if (!HasPassive && IsActive("Combo.Use.E") && spells[Spells.E].IsReady()
                            && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= spells[Spells.E].Range)
                        {
                            var prediction = spells[Spells.E].GetPrediction(target);
                            if (prediction.Hitchance >= HitChance.High && prediction.CollisionObjects.Count == 0)
                            {
                                spells[Spells.E].Cast(target);
                            }
                        }
                        break;

                    case 1:
                        if (spells[Spells.W].IsReady() && !Player.HasBuff("RengarR")
                            && Vector3.Distance(Player.ServerPosition, target.ServerPosition)
                            < spells[Spells.W].Range * 0x1 / 0x3)
                        {
                            spells[Spells.W].Cast();
                        }
                        break;

                    case 2:

                        if (IsActive("Combo.Use.Q") && !Player.IsWindingUp)
                        {
                            if (SendTime + Game.Ping + 700 - TickCount > 0)
                            {
                                spells[Spells.Q].Cast();
                                Broscience(target);
                            }
                            else if (target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player) - 1))
                                // !ObjectManager.Player.Spellbook.IsAutoAttacking && !Player.IsWindingUp &&
                            {
                                spells[Spells.Q].Cast();
                                Broscience(target);
                            }
                        }
                        break;
                }
            }

            if (Ferocity <= 4)
            {
                // !ObjectManager.Player.Spellbook.IsAutoAttacking &&  // && Player.Distance(target) <= spells[Spells.Q].Range
                if (IsActive("Combo.Use.Q") && !Player.IsWindingUp
                    && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player) - 1))
                {
                    spells[Spells.Q].Cast();
                    Broscience(target);
                }

                if (RengarR)
                {
                    return;
                }

                if (!HasPassive && IsActive("Combo.Use.E") && spells[Spells.E].IsReady()
                    && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= spells[Spells.E].Range
                    && !ObjectManager.Player.Spellbook.IsAutoAttacking)
                {
                    var prediction = spells[Spells.E].GetPrediction(target);
                    if (prediction.Hitchance >= HitChance.High && prediction.CollisionObjects.Count == 0)
                    {
                        spells[Spells.E].Cast(target);
                    }
                }

                if (IsActive("Combo.Use.W")
                    && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < spells[Spells.W].Range * 1 / 3)
                {
                    UseHydra();
                    spells[Spells.W].Cast();
                }

                if (!IsActive("Combo.Use.W")
                    || !spells[Spells.W].IsReady()
                    && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < spells[Spells.W].Range)
                {
                    UseHydra();
                }
            }

            if (IsActive("Combo.Use.E.OutOfRange") && Player.Distance(target) > Player.AttackRange + 100 && !RengarR
                && Ferocity == 5)
            {
                var prediction = spells[Spells.E].GetPrediction(target);
                if (prediction.Hitchance >= HitChance.VeryHigh && prediction.CollisionObjects.Count == 0)
                {
                    spells[Spells.E].Cast(target);
                }
            }

            #region Summoner spells

            if (IsActive("Combo.Use.Ignite") && Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health)
            {
                Player.Spellbook.CastSpell(Ignite, target);
            }

            if (IsActive("Combo.Use.Smite") && Smite != SpellSlot.Unknown
                && Player.Spellbook.CanUseSpell(Smite) == SpellState.Ready)
            {
                Player.Spellbook.CastSpell(Smite, target);
            }

            #endregion
        }

        #endregion

        #endregion

        #region Methods

        //This is BroScience by Asuna
        private static void Broscience(AttackableUnit target)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Utility.DelayAction.Add(
                    (int)(Game.Ping / 2f + spells[Spells.Q].Delay * 1000 + 300f / 1000f + 50f),
                    //this works jajaja
                    () =>
                        {
                            if (target.IsValidTarget() && !Player.IsWindingUp)
                            {
                                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                            }

                            /*Utility.DelayAction.Add(
                                (int)(Game.Ping / 2f + Player.AttackDelay * 1000 + 250 + 50),
                                () => { Player.IssueOrder(GameObjectOrder.AttackUnit, target); });*/
                        });
            }
        }

        #endregion

        public static void Harass()
        {
            var target = TargetSelector.GetSelectedTarget();
            if (target == null || !target.IsValidTarget() || TargetSelector.GetSelectedTarget() == null)
            {
                target = TargetSelector.GetTarget(spells[Spells.R].Range, TargetSelector.DamageType.Physical);
            }

            if (TargetSelector.GetSelectedTarget() != null)
            {
                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < spells[Spells.R].Range)
                {
                    target = TargetSelector.GetSelectedTarget();
                    TargetSelector.SetTarget(target);
                    Hud.SelectedUnit = target;
                    Console.WriteLine("Selected target: {0}", target.ChampionName);
                }
            }

            target = TargetSelector.GetTarget(spells[Spells.R].Range, TargetSelector.DamageType.Physical);
            if (!target.IsValidTarget())
            {
                return;
            }

            #region RengarR

            if (Ferocity == 5)
            {
                switch (IsListActive("Harass.Prio").SelectedIndex)
                {
                    case 0:
                        if (!HasPassive && IsActive("Harass.Use.E") && spells[Spells.E].IsReady()
                            && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= spells[Spells.E].Range)
                        {
                            var prediction = spells[Spells.E].GetPrediction(target);
                            if (prediction.Hitchance >= HitChance.High && prediction.CollisionObjects.Count == 0)
                            {
                                spells[Spells.E].Cast(target.ServerPosition);
                            }
                        }
                        break;

                    case 1:
                        if (IsActive("Harass.Use.Q") && !ObjectManager.Player.Spellbook.IsAutoAttacking
                            && !Player.IsWindingUp
                            && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= spells[Spells.Q].Range)
                        {
                            spells[Spells.Q].Cast();
                        }
                        break;
                }
            }

            if (Ferocity <= 4)
            {
                if (IsActive("Harass.Use.Q") && !ObjectManager.Player.Spellbook.IsAutoAttacking && !Player.IsWindingUp
                    && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= spells[Spells.Q].Range)
                {
                    spells[Spells.Q].Cast();
                }

                if (RengarR)
                {
                    return;
                }

                if (!HasPassive && IsActive("Harass.Use.E") && spells[Spells.E].IsReady()
                    && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= spells[Spells.E].Range)
                {
                    var prediction = spells[Spells.E].GetPrediction(target);
                    if (prediction.Hitchance >= HitChance.High && prediction.CollisionObjects.Count == 0)
                    {
                        spells[Spells.E].Cast(target.ServerPosition);
                    }
                }

                if (IsActive("Harass.Use.W")
                    && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < spells[Spells.W].Range * 1 / 3)
                {
                    UseHydra();
                    spells[Spells.W].Cast();
                }

                if (!IsActive("Harass.Use.W")
                    || !spells[Spells.W].IsReady()
                    && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < spells[Spells.W].Range)
                {
                    UseHydra();
                }
            }
        }

        public static void Jungleclear()
        {
            var minion =
                MinionManager.GetMinions(
                    Player.ServerPosition,
                    spells[Spells.W].Range,
                    MinionTypes.All,
                    MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth).FirstOrDefault();

            if (minion == null)
            {
                return;
            }

            if (Ferocity == 5 && IsActive("Jungle.Save.Ferocity"))
            {
                if (Vector3.Distance(Player.ServerPosition, minion.ServerPosition) < spells[Spells.W].Range)
                {
                    UseHydra();
                }
                return;
            }

            if (IsActive("Jungle.Use.Q") && spells[Spells.Q].IsReady() && !Player.IsWindingUp)
            {
                spells[Spells.Q].Cast();
            }

            if (IsActive("Jungle.Use.W") && spells[Spells.W].IsReady()
                && Vector3.Distance(Player.ServerPosition, minion.ServerPosition) < spells[Spells.W].Range)
            {
                UseHydra();
                spells[Spells.W].Cast();
            }
            //&& spells[Spells.E].GetDamage(minion) > minion.Health
            if (IsActive("Jungle.Use.E") && spells[Spells.E].IsReady()
                && Vector3.Distance(Player.ServerPosition, minion.ServerPosition) < spells[Spells.E].Range)
            {
                spells[Spells.E].Cast(minion.Position);
            }
        }

        public static void Laneclear()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.W].Range).FirstOrDefault();
            if (minion == null)
            {
                return;
            }

            if (Ferocity == 5 && IsActive("Clear.Save.Ferocity"))
            {
                if (Vector3.Distance(Player.ServerPosition, minion.ServerPosition) < spells[Spells.W].Range * 1 / 3)
                {
                    UseHydra();
                }
                return;
            }

            if (IsActive("Clear.Use.Q") && spells[Spells.Q].IsReady() && !Player.IsWindingUp)
            {
                if (spells[Spells.Q].GetDamage(minion) > minion.Health)
                {
                    spells[Spells.Q].Cast();
                }
            }

            if (IsActive("Clear.Use.W") && spells[Spells.W].IsReady()
                && Vector3.Distance(Player.ServerPosition, minion.ServerPosition) < spells[Spells.W].Range * 1 / 3)
            {
                UseHydra();
                spells[Spells.W].Cast();
            }

            if (IsActive("Clear.Use.E") && spells[Spells.E].GetDamage(minion) > minion.Health
                && spells[Spells.E].IsReady()
                && Vector3.Distance(Player.ServerPosition, minion.ServerPosition) < spells[Spells.E].Range)
            {
                spells[Spells.E].Cast(minion.Position);
            }
        }

        #endregion
    }
}