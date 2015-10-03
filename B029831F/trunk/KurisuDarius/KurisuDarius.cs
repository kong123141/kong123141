﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using KL = KurisuDarius.KurisuLib;

namespace KurisuDarius
{
    internal class KurisuDarius
    {
        internal static Menu Config;
        internal static SpellSlot Ignite;
        internal static int LastGrabTimeStamp;
        internal static int LastDunkTimeStamp;
        internal static HpBarIndicator HPi = new HpBarIndicator();
        internal static Orbwalking.Orbwalker Orbwalker;

        internal static readonly int ECost = 45;
        internal static readonly int WCost = 30;
        internal static readonly int[] QCost = { 30, 30, 35, 40, 45, 50 };
        internal static readonly int[] RCost = { 100, 100, 100, 0 };

        public KurisuDarius()
        {
            if (ObjectManager.Player.ChampionName != "Darius")
                return;
          
            Config = new Menu("Kurisu's Darius", "darius", true);

            var drmenu = new Menu(":: Drawings", "drawings");
            drmenu.AddItem(new MenuItem("drawe", "Draw E"))
                .SetValue(new Circle(true, System.Drawing.Color.FromArgb(150, System.Drawing.Color.Red)));
            drmenu.AddItem(new MenuItem("drawq", "Draw Q"))
                .SetValue(new Circle(true, System.Drawing.Color.FromArgb(150, System.Drawing.Color.Red)));
            drmenu.AddItem(new MenuItem("drawr", "Draw R"))
                .SetValue(new Circle(true, System.Drawing.Color.FromArgb(150, System.Drawing.Color.DarkRed)));
            drmenu.AddItem(new MenuItem("drawfill", "Draw R Damage Fill")).SetValue(true);
            drmenu.AddItem(new MenuItem("drawstack", "Draw Stack Count")).SetValue(true);
            Config.AddSubMenu(drmenu);

            var omenu = new Menu(":: Orbwalker", "omenu");
            Orbwalker = new Orbwalking.Orbwalker(omenu);
            Config.AddSubMenu(omenu);

            var tsmenu = new Menu(":: Target Selector", "tmenu");
            TargetSelector.AddToMenu(tsmenu);
            Config.AddSubMenu(tsmenu);

            var cmenu = new Menu(":: Main Settings", "cmenu");
            cmenu.AddItem(new MenuItem("useq", "Use Q")).SetValue(true);
            cmenu.AddItem(new MenuItem("usew", "Use W")).SetValue(true);
            cmenu.AddItem(new MenuItem("usee", "Use E")).SetValue(true);
            cmenu.AddItem(new MenuItem("user", "Use R")).SetValue(true);
            cmenu.AddItem(new MenuItem("harassq", "Harass Q")).SetValue(true);
            Config.AddSubMenu(cmenu);

            var kmenu = new Menu(":: Miscellaneous", "kmenu");
            kmenu.AddItem(new MenuItem("ksr", "Auto R on killable targets")).SetValue(true);
            kmenu.AddItem(new MenuItem("wwww", "Don't W slowed targets")).SetValue(false);
            kmenu.AddItem(new MenuItem("iiii", "Use Hydra/Tiamat/Titanic")).SetValue(true);
            kmenu.AddItem(new MenuItem("eeee", "Use advance E logic (beta)")).SetValue(false);
            kmenu.AddItem(new MenuItem("ksr1", "Use early if target will bleed to death (1v1)")).SetValue(false);
            kmenu.AddItem(new MenuItem("rmodi", "Adjust ult damage (Less if target doesnt die)")).SetValue(new Slider(0, -250, 250));
            Config.AddSubMenu(kmenu);

            Config.AddToMainMenu();

            if (KL.Player.Spellbook.GetSpell(SpellSlot.Summoner1).Name.ToLower().Contains("dot"))
                Ignite = SpellSlot.Summoner1;

            if (KL.Player.Spellbook.GetSpell(SpellSlot.Summoner2).Name.ToLower().Contains("dot"))
                Ignite = SpellSlot.Summoner2;

            Game.OnUpdate += Game_OnUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;

            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;

            foreach (var obj in ObjectManager.Get<Obj_AI_Turret>())
            {
                if (!KL.TurretCache.ContainsKey(obj.NetworkId))
                     KL.TurretCache.Add(obj.NetworkId, obj);
            }     

            Obj_AI_Base.OnProcessSpellCast += (sender, args) =>
            {
                if (sender.IsMe && args.SData.Name == "DariusAxeGrabCone")
                    LastGrabTimeStamp = Utils.GameTimeTickCount;

                if (sender.IsMe && args.SData.Name == "DariusExecute")
                    LastDunkTimeStamp = Utils.GameTimeTickCount;

                if (sender.IsMe && args.SData.Name == "DariusCleave")
                    Utility.DelayAction.Add(Game.Ping + 800, Orbwalking.ResetAutoAttackTimer);

                if (sender.IsMe && args.SData.Name == "DariusAxeGrabCone")
                    Utility.DelayAction.Add(Game.Ping + 100, Orbwalking.ResetAutoAttackTimer);

                if (sender.IsMe && args.SData.Name == "DariusExecute")
                    Utility.DelayAction.Add(Game.Ping + 300, Orbwalking.ResetAutoAttackTimer);
            };
        }

        internal void Drawing_OnEndScene(EventArgs args)
        {
            if (!Config.Item("drawfill").GetValue<bool>())
                return;

            foreach (var enemy in HeroManager.Enemies.Where(ene => ene.IsValidTarget() && !ene.IsZombie))
            {
                HPi.unit = enemy;
                HPi.drawDmg(
                    KL.RDmg(enemy, 
                        enemy.GetBuffCount("dariushemo") <= 0 ? 0 
                      : enemy.GetBuffCount("dariushemo")),new ColorBGRA(255, 255, 0, 90));
            }
        }

        internal static bool CanE(Obj_AI_Hero target)
        {
            if (!Config.Item("eeee").GetValue<bool>())
                return true;
  
            var t = KL.TurretCache.Values.FirstOrDefault(x => x.IsEnemy && x.Distance(KL.Player.ServerPosition) <= 1500);
            if (t == null || t.IsDead || !t.IsValid)
                return true;

            if (KL.Player.Distance(t) <= 1200 && target.Distance(t) <= 1200)
            {
                if (target.Distance(t) > KL.Player.Distance(t))
                {
                    if (target.IsFacing(t))
                        return false;
                }
            }
            
            return true;
        }

        internal static void Drawing_OnDraw(EventArgs args)
        {
            if (KL.Player.IsDead)
            {
                return;
            }

            var acircle = Config.Item("drawe").GetValue<Circle>();
            var rcircle = Config.Item("drawr").GetValue<Circle>();
            var qcircle = Config.Item("drawq").GetValue<Circle>();

            if (acircle.Active)
                Render.Circle.DrawCircle(KL.Player.Position, KL.Spellbook["E"].Range, acircle.Color, 3);

            if (rcircle.Active)
                Render.Circle.DrawCircle(KL.Player.Position, KL.Spellbook["R"].Range, rcircle.Color, 3);

            if (qcircle.Active)
                Render.Circle.DrawCircle(KL.Player.Position, KL.Spellbook["Q"].Range, qcircle.Color, 3);

            if (!Config.Item("drawstack").GetValue<bool>())
                return;

            var plaz = Drawing.WorldToScreen(KL.Player.Position);
            if (KL.Player.GetBuffCount("dariusexecutemulticast") > 0)
            {
                var executetime = KL.Player.GetBuff("dariusexecutemulticast").EndTime - Game.Time;
                Drawing.DrawText(plaz[0] - 15, plaz[1] + 55, System.Drawing.Color.OrangeRed, executetime.ToString("0.0"));
            }

            foreach (var enemy in HeroManager.Enemies.Where(ene => ene.IsValidTarget() && !ene.IsZombie))
            {
                var enez = Drawing.WorldToScreen(enemy.Position);
                if (enemy.GetBuffCount("dariushemo") > 0)
                {
                    var endtime = enemy.GetBuff("dariushemo").EndTime - Game.Time;
                    Drawing.DrawText(enez[0] - 50, enez[1], System.Drawing.Color.OrangeRed,  "Stack Count: " + enemy.GetBuffCount("dariushemo"));
                    Drawing.DrawText(enez[0] - 25, enez[1] + 20, System.Drawing.Color.OrangeRed, endtime.ToString("0.0"));
                }
            }
        }


        internal static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var hero = unit as Obj_AI_Hero;
            if (hero == null || !hero.IsValid<Obj_AI_Hero>())
                return;

            if (hero.Type != GameObjectType.obj_AI_Hero)
                return;

            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                return;

            if (KL.Spellbook["R"].IsReady() && KL.Player.Mana - WCost > RCost[KL.Spellbook["R"].Level - 1] || 
               !KL.Spellbook["R"].IsReady())
            {
                if (!hero.HasBuffOfType(BuffType.Slow) || !Config.Item("wwww").GetValue<bool>())
                    KL.Spellbook["W"].Cast();
            }

            if (KL.Spellbook["W"].IsReady() || !Config.Item("iiii").GetValue<bool>())
                return;

            if (Items.HasItem(3077) && Items.CanUseItem(3077))
                Items.UseItem(3077);

            if (Items.HasItem(3074) && Items.CanUseItem(3074))
                Items.UseItem(3074);

            if (Items.HasItem(3748) && Items.CanUseItem(3748))
                Items.UseItem(3748);

        }


        internal static float Rmodi;

        internal static int PassiveCount(Obj_AI_Base unit)
        {
            return unit.GetBuffCount("dariushemo") > 0 ? unit.GetBuffCount("dariushemo") : 0;
        }

        internal static void Game_OnUpdate(EventArgs args)
        {
            Rmodi = Config.Item("rmodi").GetValue<Slider>().Value;

            if (KL.Spellbook["R"].IsReady() && Config.Item("ksr").GetValue<bool>())
            {
                foreach (var unit in HeroManager.Enemies.Where(ene => ene.IsValidTarget(KL.Spellbook["R"].Range) && !ene.IsZombie))
                {
                    if (unit.CountEnemiesInRange(1200) <= 1 && Config.Item("ksr1").GetValue<bool>())
                    {
                        if (KL.RDmg(unit, PassiveCount(unit)) + Rmodi + KL.Hemorrhage(unit, PassiveCount(unit)) >= unit.Health)
                        {
                            if (!TargetSelector.IsInvulnerable(unit, TargetSelector.DamageType.True))
                                KL.Spellbook["R"].CastOnUnit(unit);
                        }
                    }

                    if (KL.RDmg(unit, PassiveCount(unit)) + Rmodi >= unit.Health + KL.Hemorrhage(unit, 1))
                    {
                        if (!TargetSelector.IsInvulnerable(unit, TargetSelector.DamageType.True))
                            KL.Spellbook["R"].CastOnUnit(unit);
                    }
                }
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo(Config.Item("useq").GetValue<bool>(), Config.Item("usew").GetValue<bool>(),
                          Config.Item("usee").GetValue<bool>(), Config.Item("user").GetValue<bool>());
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
            }
        }

        internal static bool CanQ(Obj_AI_Base unit)
        {
            if (!unit.IsValidTarget() || unit.IsZombie || 
                TargetSelector.IsInvulnerable(unit, TargetSelector.DamageType.Physical))
                return false;

            if (KL.Player.Distance(unit.ServerPosition) < 175)
                return false;

            if (KL.Spellbook["R"].IsReady() &&
                KL.Player.Mana - QCost[KL.Spellbook["Q"].Level - 1] < RCost[KL.Spellbook["R"].Level - 1])
                return false;

            if (KL.Spellbook["W"].IsReady() && KL.WDmg(unit) >= unit.Health &&
                unit.Distance(KL.Player.ServerPosition) <= 200)
                return false;

            if (Utils.GameTimeTickCount - LastGrabTimeStamp < 250)
                return false;

            if (KL.Spellbook["W"].IsReady() && KL.Player.HasBuff("DariusNoxonTactictsONH") &&
                unit.Distance(KL.Player.ServerPosition) <= 205)
                return false;

            if (KL.Player.Distance(unit.ServerPosition) > KL.Spellbook["Q"].Range)
                return false;

            if (KL.Spellbook["R"].IsReady() && unit.Distance(KL.Player.ServerPosition) <= 460 &&
                KL.RDmg(unit, PassiveCount(unit)) - KL.Hemorrhage(unit, 1) >= unit.Health)
                return false;

            if (KL.Player.GetAutoAttackDamage(unit) * 2 + KL.Hemorrhage(unit, PassiveCount(unit)) >= unit.Health)
                if (KL.Player.Distance(unit.ServerPosition) <= 180)
                    return false;

            return true;
        }

        internal static void Harass()
        {
            if (Config.Item("harassq").GetValue<bool>() && KL.Spellbook["Q"].IsReady())
            {
                if (KL.Player.Mana / KL.Player.MaxMana * 100 > 60)
                {
                    if (CanQ(TargetSelector.GetTarget(KL.Spellbook["E"].Range, TargetSelector.DamageType.Physical)))
                    {
                        KL.Spellbook["Q"].Cast();
                    }
                }
            }   
        }

        internal static void Combo(bool useq, bool usew, bool usee, bool user)
        {
            if (useq && KL.Spellbook["Q"].IsReady())
            {
                if (CanQ(TargetSelector.GetTarget(KL.Spellbook["E"].Range, TargetSelector.DamageType.Physical)))
                {
                    KL.Spellbook["Q"].Cast();
                }
            }

            if (usew && KL.Spellbook["W"].IsReady())
            {
                var wtarget = TargetSelector.GetTarget(KL.Spellbook["E"].Range, TargetSelector.DamageType.Physical);
                if (wtarget.IsValidTarget(KL.Spellbook["W"].Range) && !wtarget.IsZombie)
                {
                    if (wtarget.Distance(KL.Player.ServerPosition) <= 200 && KL.WDmg(wtarget) >= wtarget.Health)
                    {
                        if (Utils.GameTimeTickCount - LastDunkTimeStamp >= 500)
                        {
                            KL.Spellbook["W"].Cast();
                        }
                    }
                }
            }

            if (usee && KL.Spellbook["E"].IsReady())
            {
                var etarget = TargetSelector.GetTarget(KL.Spellbook["E"].Range, TargetSelector.DamageType.Physical);
                if (etarget.IsValidTarget() && CanE(etarget))
                {
                    if (etarget.Distance(KL.Player.ServerPosition) > 250)
                    {
                        if (KL.Player.CountAlliesInRange(1000) >= 1)
                            KL.Spellbook["E"].Cast(etarget.ServerPosition);

                        if (KL.RDmg(etarget, PassiveCount(etarget)) - KL.Hemorrhage(etarget, 1) >= etarget.Health)
                            KL.Spellbook["E"].Cast(etarget.ServerPosition);

                        if (KL.Spellbook["Q"].IsReady() || KL.Spellbook["W"].IsReady())
                            KL.Spellbook["E"].Cast(etarget.ServerPosition);

                        if (KL.Player.GetAutoAttackDamage(etarget) + KL.Hemorrhage(etarget, 3) * 3 >= etarget.Health)
                            KL.Spellbook["E"].Cast(etarget.ServerPosition);
                    }           
                }
            }

            if (user && KL.Spellbook["R"].IsReady())
            {
                var unit = TargetSelector.GetTarget(KL.Spellbook["E"].Range, TargetSelector.DamageType.Physical);

                if (unit.IsValidTarget(KL.Spellbook["R"].Range) && !unit.IsZombie)
                {
                    if (!unit.HasBuffOfType(BuffType.Invulnerability) && !unit.HasBuffOfType(BuffType.SpellShield))
                    {
                        if (KL.RDmg(unit, PassiveCount(unit)) + Rmodi >= unit.Health + KL.Hemorrhage(unit, 1))
                        {
                            if (!TargetSelector.IsInvulnerable(unit, TargetSelector.DamageType.True))
                            {
                                KL.Spellbook["R"].CastOnUnit(unit);
                            }
                        }
                    }
                }
            }
        }
    }
}
