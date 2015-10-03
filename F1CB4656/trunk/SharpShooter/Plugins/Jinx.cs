﻿using System;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

namespace SharpShooter.Plugins
{
    public class Jinx
    {
        private Spell Q, W, E, R;
        private float GetQRange { get { return 590 + ((25 * Q.Level) + 50) + 50; } }
        private bool isQActive { get { return ObjectManager.Player.HasBuff("JinxQ"); } }

        public Jinx()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1450f, TargetSelector.DamageType.Physical) { MinHitChance = HitChance.High };
            E = new Spell(SpellSlot.E, 900f, TargetSelector.DamageType.Physical) { MinHitChance = HitChance.High };
            R = new Spell(SpellSlot.R, 2500f, TargetSelector.DamageType.Physical) { MinHitChance = HitChance.High };

            W.SetSkillshot(0.6f, 60f, 3300f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(1.0f, 100f, 1750f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 140f, 1700f, false, SkillshotType.SkillshotLine);

            MenuProvider.Champion.Combo.addUseQ();
            MenuProvider.Champion.Combo.addUseW();
            MenuProvider.Champion.Combo.addUseE();
            MenuProvider.Champion.Combo.addUseR();

            MenuProvider.Champion.Harass.addUseQ();
            MenuProvider.Champion.Harass.addUseW();
            MenuProvider.Champion.Harass.addIfMana(60);

            MenuProvider.Champion.Laneclear.addUseQ();
            MenuProvider.Champion.Laneclear.addItem("Switch to FISHBONES If will hit minion Number >=", new Slider(3, 2, 7));
            MenuProvider.Champion.Laneclear.addIfMana(60);

            MenuProvider.Champion.Jungleclear.addUseQ();
            MenuProvider.Champion.Jungleclear.addUseW();
            MenuProvider.Champion.Jungleclear.addIfMana(60);

            MenuProvider.Champion.Misc.addUseAntiGapcloser();
            MenuProvider.Champion.Misc.addUseInterrupter();
            MenuProvider.Champion.Misc.addItem("Switch to FISHBONES If will hit enemy Number >=", new Slider(3, 2, 6));

            MenuProvider.Champion.Drawings.addDrawQrange(System.Drawing.Color.DeepSkyBlue, false);
            MenuProvider.Champion.Drawings.addDrawWrange(System.Drawing.Color.DeepSkyBlue, true);
            MenuProvider.Champion.Drawings.addDrawErange(System.Drawing.Color.DeepSkyBlue, false);
            MenuProvider.Champion.Drawings.addDrawRrange(System.Drawing.Color.DeepSkyBlue, true);
            MenuProvider.Champion.Drawings.addDamageIndicator(GetComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;

            Console.WriteLine("Sharpshooter: Jinx Loaded.");
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (Orbwalking.CanMove(10))
                {
                    switch (MenuProvider.Orbwalker.ActiveMode)
                    {
                        case Orbwalking.OrbwalkingMode.Combo:
                            {
                                if (MenuProvider.Champion.Combo.UseQ)
                                    if (Q.isReadyPerfectly())
                                        QSwitchForUnit(TargetSelector.GetTarget(GetQRange, TargetSelector.DamageType.Physical));

                                if (MenuProvider.Champion.Combo.UseW)
                                    if (W.isReadyPerfectly())
                                    {
                                        var Target = TargetSelector.GetTargetNoCollision(W);
                                        if (Target.IsValidTarget(W.Range))
                                            W.Cast(Target);
                                    }

                                if (MenuProvider.Champion.Combo.UseE)
                                    if (E.isReadyPerfectly())
                                        E.CastOnBestTarget();

                                if (MenuProvider.Champion.Combo.UseR)
                                    if (R.isReadyPerfectly())
                                        foreach (var Target in HeroManager.Enemies.Where(x => !Orbwalking.InAutoAttackRange(x) && x.isKillableAndValidTarget(GetRDamage(x), R.Range) && R.GetPrediction(x).Hitchance >= HitChance.High))
                                            if (Prediction.GetPrediction(Target, Q.Delay, Q.Width, Q.Speed, new CollisionableObjects[] { CollisionableObjects.Heroes, CollisionableObjects.YasuoWall }).CollisionObjects.Count == 0)
                                                R.Cast(Target);

                                break;
                            }
                        case Orbwalking.OrbwalkingMode.Mixed:
                            {
                                if (MenuProvider.Champion.Combo.UseQ)
                                    if (Q.isReadyPerfectly())
                                        if (ObjectManager.Player.isManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                            QSwitchForUnit(TargetSelector.GetTarget(GetQRange, TargetSelector.DamageType.Physical));
                                        else
                                            QSwitch(false);

                                if (MenuProvider.Champion.Harass.UseW)
                                    if (ObjectManager.Player.isManaPercentOkay(MenuProvider.Champion.Harass.IfMana))
                                        if (W.isReadyPerfectly())
                                        {
                                            var Target = TargetSelector.GetTargetNoCollision(W);
                                            if (Target.IsValidTarget(W.Range))
                                                W.Cast(Target);
                                        }

                                break;
                            }
                        case Orbwalking.OrbwalkingMode.LaneClear:
                            {
                                if (MenuProvider.Champion.Laneclear.UseQ)
                                    if (Q.isReadyPerfectly())
                                        if (ObjectManager.Player.isManaPercentOkay(MenuProvider.Champion.Laneclear.IfMana))
                                        {
                                            var Target = MenuProvider.Orbwalker.GetTarget();
                                            if (Target != null)
                                                QSwitch(MinionManager.GetMinions(Target.Position, 200).Count() >= MenuProvider.Champion.Laneclear.getSliderValue("Switch to FISHBONES If will hit minion Number >=").Value);
                                        }
                                        else
                                            QSwitch(false);

                                //Jungleclear
                                if (MenuProvider.Champion.Jungleclear.UseQ)
                                    if (ObjectManager.Player.isManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                        if (Q.isReadyPerfectly())
                                        {

                                        }

                                if (MenuProvider.Champion.Jungleclear.UseW)
                                    if (ObjectManager.Player.isManaPercentOkay(MenuProvider.Champion.Jungleclear.IfMana))
                                        if (W.isReadyPerfectly())
                                        {
                                            var Target = MinionManager.GetMinions(700, MinionTypes.All, MinionTeam.Neutral).FirstOrDefault(x => x.IsValidTarget(700) && W.GetPrediction(x).Hitchance >= W.MinHitChance);
                                            if (Target != null)
                                                W.Cast(Target);
                                        }

                                break;
                            }
                    }
                }
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuProvider.Champion.Misc.UseAntiGapcloser)
                if (E.isReadyPerfectly())
                    if (gapcloser.End.Distance(ObjectManager.Player.Position) <= 200)
                        E.Cast(gapcloser.End);
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (MenuProvider.Champion.Misc.UseInterrupter)
                if (E.isReadyPerfectly())
                    if (args.DangerLevel >= Interrupter2.DangerLevel.Medium)
                        E.Cast(sender);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (MenuProvider.Champion.Drawings.DrawQrange.Active && Q.isReadyPerfectly())
                Render.Circle.DrawCircle(ObjectManager.Player.Position, GetQRange, MenuProvider.Champion.Drawings.DrawQrange.Color);

            if (MenuProvider.Champion.Drawings.DrawWrange.Active && W.isReadyPerfectly())
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, MenuProvider.Champion.Drawings.DrawWrange.Color);

            if (MenuProvider.Champion.Drawings.DrawErange.Active && E.isReadyPerfectly())
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, MenuProvider.Champion.Drawings.DrawErange.Color);

            if (MenuProvider.Champion.Drawings.DrawRrange.Active && R.isReadyPerfectly())
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, MenuProvider.Champion.Drawings.DrawRrange.Color);
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            return R.isReadyPerfectly() ? (float)GetRDamage(enemy) : 0;
        }

        private void QSwitch(bool activate)
        {
            if (Q.isReadyPerfectly())
                if (!ObjectManager.Player.IsWindingUp)
                    switch (activate)
                    {
                        case true:
                            if (!ObjectManager.Player.HasBuff("JinxQ"))
                                Q.Cast();
                            break;
                        case false:
                            if (ObjectManager.Player.HasBuff("JinxQ"))
                                Q.Cast();
                            break;
                    }
        }

        private void QSwitchForUnit(AttackableUnit Unit)
        {
            if (Unit == null)
            {
                QSwitch(false);
                return;
            }

            if (Utility.CountEnemiesInRange(Unit.Position, 200) >= MenuProvider.Champion.Misc.getSliderValue("Switch to FISHBONES If will hit enemy Number >=").Value)
            {
                QSwitch(true);
                return;
            }

            QSwitch(!Unit.IsValidTarget(590));
        }

        private double GetRDamage(Obj_AI_Base Target)
        {
            return ObjectManager.Player.CalcDamage(Target, Damage.DamageType.Physical, new double[] { 0, 25, 30, 35 }[R.Level] / 100 * (Target.MaxHealth - Target.Health) + ((new double[] { 0, 25, 35, 45 }[R.Level] + 0.1 * ObjectManager.Player.FlatPhysicalDamageMod) * Math.Min((1 + ObjectManager.Player.Distance(Target.ServerPosition) / 15 * 0.09d), 10)));
        }
    }
}