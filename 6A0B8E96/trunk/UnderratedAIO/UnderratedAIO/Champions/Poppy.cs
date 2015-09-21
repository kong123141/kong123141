﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
using UnderratedAIO.Helpers;
using Environment = UnderratedAIO.Helpers.Environment;
using Orbwalking = UnderratedAIO.Helpers.Orbwalking;

namespace UnderratedAIO.Champions
{
    internal class Poppy
    {
        public static Menu config;
        public static Orbwalking.Orbwalker orbwalker;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static double[] ultMod = new double[3] { 1.2, 1.3, 1.4 };
        public static double[] eSecond = new double[5] { 75, 125, 175, 225, 275 };
        public static AutoLeveler autoLeveler;

        public Poppy()
        {
            Initpoppy();
            InitMenu();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Poppy</font>");
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Game_OnDraw;
            Orbwalking.AfterAttack += AfterAttack;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += OnInterruptableTarget;
            Jungle.setSmiteSlot();
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe && orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                var mob = Jungle.GetNearest(player.Position);
                if (mob != null && config.Item("useqLCSteal").GetValue<bool>() && Q.IsReady() &&
                    Q.GetDamage(mob) > mob.Health)
                {
                    Q.Cast(config.Item("packets").GetValue<bool>());
                }
                if (mob != null && config.Item("useqbsmite").GetValue<bool>() && Q.IsReady() &&
                    Jungle.SmiteReady(config.Item("useSmite").GetValue<KeyBind>().Active) &&
                    Q.GetDamage(mob) + Jungle.smiteDamage(mob) > mob.Health)
                {
                    Q.Cast(config.Item("packets").GetValue<bool>());
                }
            }
            if (args.Unit.IsMe && Q.IsReady() && config.Item("useq").GetValue<bool>() && args.Target is Obj_AI_Hero &&
                Q.GetDamage((Obj_AI_Base) args.Target) > args.Target.Health)
            {
                Q.Cast(config.Item("packets").GetValue<bool>());
            }
        }

        private void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("useEint").GetValue<bool>() && E.IsReady() && E.CanCast(sender))
            {
                E.CastOnUnit(sender, config.Item("packets").GetValue<bool>());
            }
        }

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe && Q.IsReady() &&
                (((orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo ||
                   config.Item("useeflashforced").GetValue<KeyBind>().Active) && config.Item("useq").GetValue<bool>() &&
                  target.IsEnemy && target is Obj_AI_Hero &&
                  target.Health - player.GetAutoAttackDamage(target as Obj_AI_Hero) > 0) ||
                 (orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear &&
                  player.ManaPercent > config.Item("minmana").GetValue<Slider>().Value &&
                  config.Item("useqLC").GetValue<bool>() && target is Obj_AI_Minion &&
                  target.Health > Q.GetDamage((Obj_AI_Base) target) * 2)))
            {
                Q.Cast(config.Item("packets").GetValue<bool>());
                Orbwalking.ResetAutoAttackTimer();
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            Obj_AI_Hero targetf = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (config.Item("useeflashforced").GetValue<KeyBind>().Active)
            {
                if (targetf == null)
                {
                    player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                }
                else
                {
                    var bestpos = CombatHelper.bestVectorToPoppyFlash2(targetf);
                    bool hasFlash = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerFlash")) ==
                                    SpellState.Ready;
                    if (E.IsReady() && hasFlash && !CheckWalls(player, targetf) && bestpos.IsValid())
                    {
                        player.Spellbook.CastSpell(player.GetSpellSlot("SummonerFlash"), bestpos);
                        player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                    }
                    else if (!hasFlash)
                    {
                        Combo();
                        Orbwalking.Orbwalk(targetf, Game.CursorPos, 90, 90);
                    }
                }
            }
            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Clear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    break;
                default:
                    break;
            }
            Jungle.CastSmite(config.Item("useSmite").GetValue<KeyBind>().Active);
        }

        private static void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            if (target == null)
            {
                return;
            }
            if (config.Item("selected").GetValue<bool>())
            {
                target = CombatHelper.SetTarget(target, TargetSelector.GetSelectedTarget());
                orbwalker.ForceTarget(target);
            }
            if (config.Item("useItems").GetValue<bool>())
            {
                ItemHandler.UseItems(target, config, ComboDamage(target));
            }
            bool hasFlash = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerFlash")) == SpellState.Ready;

            if (config.Item("usew").GetValue<bool>() && player.Distance(target.Position) < R.Range && W.IsReady())
            {
                W.Cast(config.Item("packets").GetValue<bool>());
            }

            if (config.Item("usee").GetValue<bool>() && E.IsReady())
            {
                if (config.Item("useewall").GetValue<bool>())
                {
                    var bestpos = CombatHelper.bestVectorToPoppyFlash2(target);
                    float damage =
                        (float)
                            (ComboDamage(target) +
                             Damage.CalcDamage(
                                 player, target, Damage.DamageType.Magical,
                                 (eSecond[E.Level - 1] + 0.8f * player.FlatMagicDamageMod)) +
                             (player.GetAutoAttackDamage(target) * 4));
                    float damageno = (float) (ComboDamage(target) + (player.GetAutoAttackDamage(target) * 4));
                    if (config.Item("useeflash").GetValue<bool>() && hasFlash && !CheckWalls(player, target) &&
                        damage > target.Health && target.Health > damageno &&
                        CombatHelper.bestVectorToPoppyFlash(target).IsValid())
                    {
                        player.Spellbook.CastSpell(player.GetSpellSlot("SummonerFlash"), bestpos);
                        Utility.DelayAction.Add(
                            100, () => E.CastOnUnit(target, config.Item("packets").GetValue<bool>()));
                    }
                    if (E.CanCast(target) &&
                        (CheckWalls(player, target) ||
                         target.Health < E.GetDamage(target) + player.GetAutoAttackDamage(target, true)))
                    {
                        E.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                    }
                    if (E.CanCast(target) && Q.IsReady() && Q.Instance.ManaCost + E.Instance.ManaCost > player.Mana &&
                        target.Health <
                        E.GetDamage(target) + Q.GetDamage(target) + player.GetAutoAttackDamage(target, true))
                    {
                        Q.Cast(config.Item("packets").GetValue<bool>());
                        E.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                    }
                }
                else
                {
                    if (E.CanCast(target))
                    {
                        E.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                    }
                }
            }
            if (config.Item("user").GetValue<bool>())
            {
                if (R.IsReady() && player.Distance(target.Position) < E.Range &&
                    ComboDamage(target) + player.GetAutoAttackDamage(target) * 5 < target.Health &&
                    (ComboDamage(target) + player.GetAutoAttackDamage(target) * 3) * ultMod[R.Level - 1] > target.Health)
                {
                    R.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                }
            }
            bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;
            var ignitedmg = (float) player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            if (config.Item("useIgnite").GetValue<bool>() && ignitedmg > target.Health && hasIgnite &&
                !E.CanCast(target) && !Q.CanCast(target))
            {
                player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
            }
            if (config.Item("userindanger").GetValue<Slider>().Value < player.CountEnemiesInRange(R.Range))
            {
                if (config.Item("userOnweakest").GetValue<bool>())
                {
                    var tmpTarg =
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                i => i.IsEnemy && i.IsDead && player.Distance(i) < R.Range && i.Health > i.MaxHealth / 2)
                            .OrderBy(i => CombatHelper.GetChampDmgToMe(i))
                            .FirstOrDefault();
                    if (tmpTarg != null)
                    {
                        target = tmpTarg;
                    }
                }
                R.CastOnUnit(target, config.Item("packets").GetValue<bool>());
            }
        }

        private static void Clear()
        {
            var mob = Jungle.GetNearest(player.Position);
            float perc = config.Item("minmana").GetValue<Slider>().Value / 100f;
            if (player.Mana < player.MaxMana * perc)
            {
                return;
            }
            if (config.Item("useeLC").GetValue<bool>() && E.CanCast(mob) && CheckWalls(player, mob))
            {
                E.CastOnUnit(mob, config.Item("packets").GetValue<bool>());
            }
        }

        private static void Game_OnDraw(EventArgs args)
        {
            DrawHelper.DrawCircle(config.Item("drawaa").GetValue<Circle>(), player.AttackRange);
            DrawHelper.DrawCircle(config.Item("drawee").GetValue<Circle>(), E.Range);
            DrawHelper.DrawCircle(config.Item("drawrr").GetValue<Circle>(), R.Range);
            Jungle.ShowSmiteStatus(
                config.Item("useSmite").GetValue<KeyBind>().Active, config.Item("smiteStatus").GetValue<bool>());
            Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();
        }

        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("useEgap").GetValue<bool>() && E.IsReady() && E.CanCast(gapcloser.Sender) &&
                CheckWalls(player, gapcloser.Sender))
            {
                E.CastOnUnit(gapcloser.Sender, config.Item("packets").GetValue<bool>());
            }
        }

        public static bool CheckWalls(Obj_AI_Base player, Obj_AI_Base enemy)
        {
            var distance = player.Position.Distance(enemy.Position);
            for (int i = 1; i < 6; i++)
            {
                if (player.Position.Extend(enemy.Position, distance + 60 * i).IsWall())
                {
                    return true;
                }
            }
            return false;
        }

        public static float ComboDamage(Obj_AI_Hero hero)
        {
            double damage = 0;
            if (Q.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.Q);
            }
            if (E.IsReady())
            {
                damage += (float) Damage.GetSpellDamage(player, hero, SpellSlot.E);
            }
            if ((Items.HasItem(ItemHandler.Bft.Id) && Items.CanUseItem(ItemHandler.Bft.Id)) ||
                (Items.HasItem(ItemHandler.Dfg.Id) && Items.CanUseItem(ItemHandler.Dfg.Id)))
            {
                damage = (float) (damage * 1.2);
            }
            if (player.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready)
            {
                damage += (float) player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }
            damage += ItemHandler.GetItemsDamage(hero);
            return (float) damage;
        }

        private static void Initpoppy()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 525);
            R = new Spell(SpellSlot.R, 900);
        }

        private static void InitMenu()
        {
            config = new Menu("Poppy", "Poppy", true);
            // Target Selector
            Menu menuTS = new Menu("Selector", "tselect");
            TargetSelector.AddToMenu(menuTS);
            config.AddSubMenu(menuTS);

            // Orbwalker
            Menu menuOrb = new Menu("Orbwalker", "orbwalker");
            orbwalker = new Orbwalking.Orbwalker(menuOrb);
            config.AddSubMenu(menuOrb);

            // Draw settings
            Menu menuD = new Menu("Drawings ", "dsettings");
            menuD.AddItem(new MenuItem("drawaa", "Draw AA range")).SetValue(new Circle(false, Color.DarkCyan));
            menuD.AddItem(new MenuItem("drawee", "Draw E range")).SetValue(new Circle(false, Color.DarkCyan));
            menuD.AddItem(new MenuItem("drawrr", "Draw R range")).SetValue(new Circle(false, Color.DarkCyan));
            menuD.AddItem(new MenuItem("drawcombo", "Draw combo damage")).SetValue(true);
            config.AddSubMenu(menuD);
            // Combo Settings
            Menu menuC = new Menu("Combo ", "csettings");
            menuC.AddItem(new MenuItem("useq", "Use Q")).SetValue(true);
            menuC.AddItem(new MenuItem("usew", "Use W")).SetValue(true);
            menuC.AddItem(new MenuItem("usee", "Use E")).SetValue(true);
            menuC.AddItem(new MenuItem("useewall", "Use E only near walls")).SetValue(true);
            menuC.AddItem(new MenuItem("useeflash", "Use flash to positioning")).SetValue(true);
            menuC.AddItem(new MenuItem("useeflashforced", "Forced flash+E if possible"))
                .SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press));
            menuC.AddItem(new MenuItem("user", "Use R to maximize dmg")).SetValue(true);
            menuC.AddItem(new MenuItem("userindanger", "Auto activate if more than")).SetValue(new Slider(3, 1, 6));
            menuC.AddItem(new MenuItem("userOnweakest", "Use on the weakest enemy")).SetValue(true);
            menuC.AddItem(new MenuItem("selected", "Focus Selected target")).SetValue(true);
            menuC.AddItem(new MenuItem("useIgnite", "Use Ignite")).SetValue(true);
            menuC = ItemHandler.addItemOptons(menuC);
            config.AddSubMenu(menuC);
            // LaneClear Settings
            Menu menuLC = new Menu("Clear ", "Lcsettings");
            menuLC.AddItem(new MenuItem("useqLC", "Use Q")).SetValue(true);
            menuLC.AddItem(new MenuItem("useqLCSteal", "Use Q to steal in jungle")).SetValue(true);
            menuLC.AddItem(new MenuItem("useqbsmite", "Use Q before smite")).SetValue(true);
            menuLC.AddItem(new MenuItem("useeLC", "Use E")).SetValue(true);
            menuLC.AddItem(new MenuItem("minmana", "Keep X% mana")).SetValue(new Slider(50, 1, 100));
            config.AddSubMenu(menuLC);
            // Misc Settings
            Menu menuM = new Menu("Misc ", "Msettings");
            menuM.AddItem(new MenuItem("useEint", "Use E interrupt")).SetValue(true);
            menuM.AddItem(new MenuItem("useEgap", "Use E on gapcloser near walls")).SetValue(true);
            menuM = Jungle.addJungleOptions(menuM);


            Menu autolvlM = new Menu("AutoLevel", "AutoLevel");
            autoLeveler = new AutoLeveler(autolvlM);
            menuM.AddSubMenu(autolvlM);

            config.AddSubMenu(menuM);
            config.AddItem(new MenuItem("packets", "Use Packets")).SetValue(false);
            config.AddItem(new MenuItem("UnderratedAIO", "by Soresu v" + Program.version.ToString().Replace(",", ".")));
            config.AddToMainMenu();
        }
    }
}