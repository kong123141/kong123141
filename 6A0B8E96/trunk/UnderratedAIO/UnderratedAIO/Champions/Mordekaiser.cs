﻿using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;
using UnderratedAIO.Helpers;
using Environment = UnderratedAIO.Helpers.Environment;
using Orbwalking = UnderratedAIO.Helpers.Orbwalking;

namespace UnderratedAIO.Champions
{
    internal class Mordekaiser
    {
        public static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static bool hasGhost = false;
        public static bool GhostDelay, justW;
        public static int GhostRange = 2200;
        public static AutoLeveler autoLeveler;
        public static int LastAATick;
        public Obj_AI_Hero IgniteTarget;

        public Mordekaiser()
        {
            InitMordekaiser();
            InitMenu();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Mordekaiser</font>");
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += AfterAttack;
            Orbwalking.BeforeAttack += BeforeAttack;
            Drawing.OnDraw += Game_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Helpers.Jungle.setSmiteSlot();
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base hero, GameObjectProcessSpellCastEventArgs args)
        {
            if (MordeGhost)
            {
                var clone = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(m => m.HasBuff("mordekaisercotgpetbuff2"));

                if (args == null || clone == null)
                {
                    return;
                }
                if (hero.NetworkId != clone.NetworkId)
                {
                    return;
                }
                LastAATick = Utils.GameTimeTickCount;
            }
            if (hero.IsMe && args.SData.Name == "MordekaiserCreepingDeathCast")
            {
                if (!justW)
                {
                    justW = true;
                    Utility.DelayAction.Add(1000, () => justW = false);
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
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

        private void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe && Q.IsReady() && orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear &&
                config.Item("useqLC", true).GetValue<bool>() &&
                Environment.Minion.countMinionsInrange(player.Position, 600f) >
                config.Item("qhitLC", true).GetValue<Slider>().Value)
            {
                Q.Cast(config.Item("packets").GetValue<bool>());
                Orbwalking.ResetAutoAttackTimer();
                //player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
        }

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe && Q.IsReady() &&
                ((orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && config.Item("useq", true).GetValue<bool>() &&
                  target.IsEnemy && target.Team != player.Team) ||
                 (config.Item("useqLC", true).GetValue<bool>() &&
                  Jungle.GetNearest(player.Position).Distance(player.Position) < player.AttackRange + 30)))
            {
                Q.Cast(config.Item("packets").GetValue<bool>());
                Orbwalking.ResetAutoAttackTimer();
                //player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
        }

        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            if (target == null)
            {
                if (MordeGhost && !GhostDelay && config.Item("follow", true).GetValue<bool>())
                {
                    R.Cast(Game.CursorPos.Extend(player.Position, 100));
                    GhostDelay = true;
                    Utility.DelayAction.Add(200, () => GhostDelay = false);
                }
                return;
            }
            if (config.Item("useItems").GetValue<bool>())
            {
                ItemHandler.UseItems(target, config, ComboDamage(target));
            }
            bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;
            if (config.Item("usew", true).GetValue<bool>() && W.IsReady())
            {
                CastW();
            }
            if (config.Item("usee", true).GetValue<bool>() && E.CanCast(target) && player.Distance(target) < E.Range)
            {
                E.CastIfHitchanceEquals(target, HitChance.High, config.Item("packets").GetValue<bool>());
            }
            var ignitedmg = (float) player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            var canUlt = config.Item("user", true).GetValue<bool>() && !MordeGhost &&
                         !config.Item("ult" + target.SkinName, true).GetValue<bool>() &&
                         (!config.Item("ultDef", true).GetValue<bool>() ||
                          (config.Item("ultDef", true).GetValue<bool>() && !CombatHelper.HasDef(target)));
            if (canUlt &&
                (player.Distance(target.Position) <= 400f ||
                 (R.CanCast(target) && target.Health < 250f &&
                  Environment.Hero.countChampsAtrangeA(target.Position, 600f) >= 1)) &&
                R.GetDamage(target) * 0.8f > target.Health)
            {
                R.CastOnUnit(target, config.Item("packets").GetValue<bool>());
            }
            if (canUlt && hasIgnite && player.Distance(target) < 600 &&
                R.GetDamage(target) * 0.8f + ignitedmg > HealthPrediction.GetHealthPrediction(target, 400))
            {
                IgniteTarget = target;
                Utility.DelayAction.Add(500, () => IgniteTarget = null);
                R.CastOnUnit(target, config.Item("packets").GetValue<bool>());
            }
            if (config.Item("useIgnite").GetValue<bool>() && ignitedmg > target.Health && hasIgnite)
            {
                if (IgniteTarget != null)
                {
                    player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), IgniteTarget);
                    return;
                }
                player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
            }
            if (MordeGhost && !GhostDelay && config.Item("moveGhost", true).GetValue<bool>())
            {
                var ghost = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(m => m.HasBuff("mordekaisercotgpetbuff2"));
                var Gtarget = TargetSelector.GetTarget(GhostRange, TargetSelector.DamageType.Magical);
                switch (config.Item("ghostTarget", true).GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        Gtarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                        break;
                    case 1:
                        Gtarget =
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(i => i.IsEnemy && !i.IsDead && player.Distance(i) <= R.Range)
                                .OrderBy(i => i.Health)
                                .FirstOrDefault();
                        break;
                    case 2:
                        Gtarget =
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(i => i.IsEnemy && !i.IsDead && player.Distance(i) <= R.Range)
                                .OrderBy(i => player.Distance(i))
                                .FirstOrDefault();
                        break;
                    default:
                        break;
                }
                if (ghost != null && Gtarget.IsValid && !ghost.IsWindingUp)
                {
                    if (ghost.IsMelee)
                    {
                        if (CanCloneAttack(ghost) || player.HealthPercent < 25)
                        {
                            R.CastOnUnit(Gtarget, config.Item("packets").GetValue<bool>());
                        }
                        else
                        {
                            var prediction = Prediction.GetPrediction(Gtarget, 2);
                            R.Cast(
                                target.Position.Extend(
                                    prediction.UnitPosition, Orbwalking.GetRealAutoAttackRange(Gtarget)),
                                config.Item("packets").GetValue<bool>());
                        }
                    }
                    else
                    {
                        if (CanCloneAttack(ghost) || player.HealthPercent < 25)
                        {
                            R.CastOnUnit(Gtarget, config.Item("packets").GetValue<bool>());
                        }
                        else
                        {
                            var pred = Prediction.GetPrediction(Gtarget, 0.5f);
                            var point =
                                CombatHelper.PointsAroundTheTargetOuterRing(
                                    pred.UnitPosition, Gtarget.AttackRange / 2, 15)
                                    .Where(p => !p.IsWall())
                                    .OrderBy(p => p.CountEnemiesInRange(500))
                                    .ThenBy(p => p.Distance(player.Position))
                                    .FirstOrDefault();

                            if (point.IsValid())
                            {
                                R.Cast(point, config.Item("packets").GetValue<bool>());
                            }
                        }
                    }

                    GhostDelay = true;
                    Utility.DelayAction.Add(200, () => GhostDelay = false);
                }
            }
        }

        private void CastW()
        {
            if (justW)
            {
                return;
            }
            var allyW = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(o => o.HasBuff("mordekaisercreepingdeath"));
            if (allyW != null)
            {
                if (allyW.HealthPercent < 20 || player.HealthPercent < 20 ||
                    CombatHelper.GetBuffTime(allyW.GetBuff("mordekaisercreepingdeath")) < 0.5f)
                {
                    if ((allyW.CountEnemiesInRange(250) +
                         Environment.Minion.countMinionsInrange(allyW.Position, 250f) / 2f >= 1 ||
                         player.CountEnemiesInRange(250f) +
                         Environment.Minion.countMinionsInrange(player.Position, 250f) / 2f >= 1))
                    {
                        W.Cast(config.Item("packets").GetValue<bool>());
                    }
                }
            }
            else
            {
                Obj_AI_Base wTarget = Environment.Hero.mostEnemyAtFriend(player, W.Range, 250f);
                if (MordeGhost)
                {
                    var ghost =
                        ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(m => m.HasBuff("mordekaisercotgpetbuff2"));
                    if (wTarget == null || ghost.CountEnemiesInRange(250f) > wTarget.CountEnemiesInRange(250f))
                    {
                        wTarget = ghost;
                    }
                }
                if (wTarget != null && (wTarget.CountEnemiesInRange(250) > 0 || player.CountEnemiesInRange(250) > 0))
                {
                    W.Cast(wTarget, config.Item("packets").GetValue<bool>());
                }
            }
        }

        private static bool MordeGhost
        {
            get { return player.Spellbook.GetSpell(SpellSlot.R).Name == "mordekaisercotgguide"; }
        }

        public static bool CanCloneAttack(Obj_AI_Minion ghost)
        {
            if (ghost != null)
            {
                return Utils.GameTimeTickCount >= LastAATick + (ghost.AttackDelay - ghost.AttackCastDelay) * 1000;
            }
            return false;
        }

        private void Harass()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target == null)
            {
                return;
            }
            if (config.Item("useeH", true).GetValue<bool>() && E.CanCast(target))
            {
                E.Cast(target, config.Item("packets").GetValue<bool>());
            }
        }

        private void Clear()
        {
            MinionManager.FarmLocation bestPosition =
                E.GetCircularFarmLocation(
                    MinionManager.GetMinions(E.Range - 100f, MinionTypes.All, MinionTeam.NotAlly), 200f);
            if (config.Item("useeLC", true).GetValue<bool>() && E.IsReady() &&
                bestPosition.MinionsHit > config.Item("ehitLC", true).GetValue<Slider>().Value)
            {
                E.Cast(bestPosition.Position, config.Item("packets").GetValue<bool>());
            }
        }

        private void Game_OnDraw(EventArgs args)
        {
            DrawHelper.DrawCircle(config.Item("drawaa", true).GetValue<Circle>(), player.AttackRange);
            DrawHelper.DrawCircle(config.Item("drawww", true).GetValue<Circle>(), W.Range);
            DrawHelper.DrawCircle(config.Item("drawee", true).GetValue<Circle>(), E.Range);
            DrawHelper.DrawCircle(config.Item("drawrr", true).GetValue<Circle>(), R.Range);
            Jungle.ShowSmiteStatus(
                config.Item("useSmite").GetValue<KeyBind>().Active, config.Item("smiteStatus").GetValue<bool>());
            Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();
        }

        private float ComboDamage(Obj_AI_Hero hero)
        {
            double damage = 0;
            if (Q.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.Q);
            }
            if (W.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.W);
            }
            if (E.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.E);
            }
            if (R.IsReady())
            {
                damage += Damage.GetSpellDamage(player, hero, SpellSlot.R);
            }

            damage += ItemHandler.GetItemsDamage(hero);

            if ((Items.HasItem(ItemHandler.Bft.Id) && Items.CanUseItem(ItemHandler.Bft.Id)) ||
                (Items.HasItem(ItemHandler.Dfg.Id) && Items.CanUseItem(ItemHandler.Dfg.Id)))
            {
                damage = (float) (damage * 1.2);
            }

            var ignitedmg = player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            if (player.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready &&
                hero.Health < damage + ignitedmg)
            {
                damage += ignitedmg;
            }
            return (float) damage;
        }

        private void InitMordekaiser()
        {
            Q = new Spell(SpellSlot.Q, player.AttackRange);
            W = new Spell(SpellSlot.W, 750);
            E = new Spell(SpellSlot.E, 650);
            E.SetSkillshot(0.5f, 45, 1500, false, SkillshotType.SkillshotCone);
            R = new Spell(SpellSlot.R, 850);
        }

        private void InitMenu()
        {
            config = new Menu("Mordekaiser", "Mordekaiser", true);
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
            menuD.AddItem(new MenuItem("drawaa", "Draw AA range", true))
                .SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
            menuD.AddItem(new MenuItem("drawww", "Draw W range", true))
                .SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
            menuD.AddItem(new MenuItem("drawee", "Draw E range", true))
                .SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
            menuD.AddItem(new MenuItem("drawrr", "Draw R range", true))
                .SetValue(new Circle(false, Color.FromArgb(180, 109, 111, 126)));
            menuD.AddItem(new MenuItem("drawcombo", "Draw combo damage")).SetValue(true);
            config.AddSubMenu(menuD);
            // Combo Settings
            Menu menuC = new Menu("Combo ", "csettings");
            menuC.AddItem(new MenuItem("useq", "Use Q", true)).SetValue(true);
            menuC.AddItem(new MenuItem("usew", "Use W", true)).SetValue(true);
            menuC.AddItem(new MenuItem("usee", "Use E", true)).SetValue(true);
            menuC.AddItem(new MenuItem("user", "Use R", true)).SetValue(true);
            menuC.AddItem(new MenuItem("ultDef", "   Don't use on Qss/barrier etc...", true)).SetValue(true);
            menuC.AddItem(new MenuItem("moveGhost", "   Move ghost", true)).SetValue(true);
            menuC.AddItem(new MenuItem("follow", "   Follow without target", true)).SetValue(true);
            menuC.AddItem(new MenuItem("selected", "Focus Selected target", true)).SetValue(true);
            menuC.AddItem(new MenuItem("useIgnite", "Use Ignite")).SetValue(true);
            menuC = ItemHandler.addItemOptons(menuC);
            config.AddSubMenu(menuC);
            // Harass Settings
            Menu menuH = new Menu("Harass ", "Hsettings");
            menuH.AddItem(new MenuItem("useeH", "Use E", true)).SetValue(true);
            config.AddSubMenu(menuH);
            // LaneClear Settings
            Menu menuLC = new Menu("LaneClear ", "Lcsettings");
            menuLC.AddItem(new MenuItem("useqLC", "Use Q", true)).SetValue(true);
            menuLC.AddItem(new MenuItem("qhitLC", "   Min hit", true).SetValue(new Slider(2, 1, 3)));
            menuLC.AddItem(new MenuItem("useeLC", "Use E", true)).SetValue(true);
            menuLC.AddItem(new MenuItem("ehitLC", "   Min hit", true).SetValue(new Slider(2, 1, 5)));
            config.AddSubMenu(menuLC);
            // Misc Settings
            Menu menuM = new Menu("Misc ", "Msettings");
            menuM.AddItem(new MenuItem("ghostTarget", "Ghost target priority", true))
                .SetValue(new StringList(new[] { "Targetselector", "Lowest health", "Closest to you" }, 0));
            menuM = Jungle.addJungleOptions(menuM);


            Menu autolvlM = new Menu("AutoLevel", "AutoLevel");
            autoLeveler = new AutoLeveler(autolvlM);
            menuM.AddSubMenu(autolvlM);

            config.AddSubMenu(menuM);
            var sulti = new Menu("Don't ult on ", "dontult");
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                sulti.AddItem(new MenuItem("ult" + hero.SkinName, hero.SkinName, true)).SetValue(false);
            }
            config.AddSubMenu(sulti);
            config.AddItem(new MenuItem("packets", "Use Packets")).SetValue(false);
            config.AddItem(new MenuItem("UnderratedAIO", "by Soresu v" + Program.version.ToString().Replace(",", ".")));
            config.AddToMainMenu();
        }
    }
}