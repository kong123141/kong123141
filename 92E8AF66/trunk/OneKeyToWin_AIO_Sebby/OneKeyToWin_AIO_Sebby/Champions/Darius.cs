﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace OneKeyToWin_AIO_Sebby.Champions
{
    class Darius
    {
        private Menu Config = Program.Config;
        public static Orbwalking.Orbwalker Orbwalker = Program.Orbwalker;
        public Spell Q, W, E, R;
        private float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        private Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q, 400);
            W = new Spell(SpellSlot.W, 145);
            E = new Spell(SpellSlot.E, 540);
            R = new Spell(SpellSlot.R, 460);

            E.SetSkillshot(0.1f, 50f, float.MaxValue, false, SkillshotType.SkillshotLine);

            LoadMenuOKTW();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.BeforeAttack += BeforeAttack;
            Orbwalking.AfterAttack += afterAttack;
            Interrupter.OnPossibleToInterrupt += OnInterruptableSpell;
        }

        private void LoadMenuOKTW()
        {
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("qRange", "Q range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("eRange", "E range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("rRange", "R range", true).SetValue(false));
            Config.SubMenu(Player.ChampionName).SubMenu("Draw").AddItem(new MenuItem("onlyRdy", "Draw when skill rdy", true).SetValue(true));

            
            Config.SubMenu(Player.ChampionName).SubMenu("Q option").AddItem(new MenuItem("haras", "Harras Q", true).SetValue(true));
            
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("autoR", "Auto R", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("R option").AddItem(new MenuItem("useR", "Semi-manual cast R key", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press))); //32 == space
            
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmW", "Farm W", true).SetValue(true));
            Config.SubMenu(Player.ChampionName).SubMenu("Farm").AddItem(new MenuItem("farmQ", "Farm Q", true).SetValue(true));
        }

        private void OnInterruptableSpell(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (E.IsReady()  && unit.IsValidTarget(E.Range))
                E.Cast(unit);
        }


        private void afterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (Player.Mana < RMANA + WMANA || !W.IsReady() || !unit.IsMe)
                return;

            var t = target as Obj_AI_Hero;

            if (t.IsValidTarget())
                W.Cast();
            else if (Config.Item("farmW", true).GetValue<bool>())
            {
                var minions = MinionManager.GetMinions(Player.Position, Player.AttackRange, MinionTypes.All);

                if (minions == null || minions.Count == 0)
                    return;

                int countMinions = 0;

                foreach (var minion in minions.Where(minion => minion.Health < W.GetDamage(minion)))
                {
                    countMinions++;
                }

                if (countMinions > 0)
                    W.Cast();
            }
        }

        private void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
           
            
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (R.IsReady() && Config.Item("useR", true).GetValue<KeyBind>().Active)
            {
                var targetR = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.True);
                if (targetR.IsValidTarget())
                    R.Cast(targetR, true);
            }
            
            if (Program.LagFree(0))
            {
                SetMana();
            }

            if (Program.LagFree(2) && Q.IsReady())
                LogicQ();
            if (Program.LagFree(3) && E.IsReady())
                LogicE();
            if (Program.LagFree(4) && R.IsReady() && Config.Item("autoR", true).GetValue<bool>())
                LogicR();
        }

        private void LogicE()
        {
            if (Player.Mana > RMANA + EMANA )
            {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (target.IsValidTarget() && ((Player.UnderTurret(false) && !Player.UnderTurret(true)) || Program.Combo) )
                {
                    if (!Orbwalking.InAutoAttackRange(target))
                        E.Cast(target, true);
                }
            }
        }

        private void LogicQ()
        {
            if (Player.CountEnemiesInRange(Q.Range) > 0)
            {
                if (Player.Mana > RMANA + QMANA && Program.Combo)
                    Q.Cast();
                else if (Program.Farm && ObjectManager.Player.Mana > RMANA + QMANA + EMANA + WMANA && Config.Item("haras", true).GetValue<bool>())
                    Q.Cast();
                if (!R.IsReady())
                {
                    var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                    if (target.IsValidTarget() && Player.Distance(target.Position) < Q.Range && Q.GetDamage(target) > target.Health)
                        Q.Cast();
                }
            }
            else if (Config.Item("farmQ", true).GetValue<bool>() && Player.Mana > RMANA + QMANA + EMANA + WMANA && Program.LaneClear)
            {
                var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All);
                foreach (var minion in allMinionsQ)
                    if (Player.Distance(minion.ServerPosition) > 300 && minion.Health < Player.GetSpellDamage(minion, SpellSlot.Q) * 0.6)
                        Q.Cast();
            }
        }

        private void LogicR()
        {
            foreach (var target in Program.Enemies.Where(target => OktwCommon.ValidUlt(target) && target.IsValidTarget(R.Range) ))
            {
                var dmgR = R.GetDamage(target);
                if (target.HasBuff("dariushemo"))
                    dmgR += R.GetDamage(target) * target.GetBuff("dariushemo").Count * 0.2f;

                if (dmgR > target.Health + target.HPRegenRate)
                {
                    R.Cast(target);
                }
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("qRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>() && Q.IsReady())
                    if (Q.IsReady())
                        Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
                    else
                        Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
            }

            if (Config.Item("eRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>() && E.IsReady())
                    if (E.IsReady())
                        Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Orange, 1, 1);
                    else
                        Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Orange, 1, 1);
            } 
            if (Config.Item("rRange", true).GetValue<bool>())
            {
                if (Config.Item("onlyRdy", true).GetValue<bool>() && R.IsReady())
                    if (R.IsReady())
                        Utility.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Red, 1, 1);
                    else
                        Utility.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.Red, 1, 1);
            }
        }
        private void SetMana()
        {
            if ((Config.Item("manaDisable", true).GetValue<bool>() && Program.Combo) || Player.HealthPercent < 20)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
                return;
            }

            QMANA = Q.Instance.ManaCost;
            WMANA = W.Instance.ManaCost;
            EMANA = E.Instance.ManaCost;

            if (!R.IsReady())
                RMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;
            else
                RMANA = R.Instance.ManaCost;
        }
        
    }
}
