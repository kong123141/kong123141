using System;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
{
    class Nunu : PluginData
    {
        public Nunu()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 550);            
            R = new Spell(SpellSlot.R, 650);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            myDamageIndicator.DamageToUnit = GetDamage;
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Nunu.Combo.Q", "Use Q").SetValue(true));                
                combomenu.AddItem(new MenuItem("EC.Nunu.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Nunu.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Nunu.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Nunu.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Nunu.Misc.E", "E Gapclosers").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Nunu.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Nunu.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            var UseW = Root.Item("EC.Nunu.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Nunu.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Nunu.Combo.R").GetValue<bool>();
            var CastItems = Root.Item("EC.Nunu.Combo.Items").GetValue<bool>();
            if (UseE && E.IsReady())
            {
                if (Target.IsValidTarget())
                {
                     mySpellcast.Unit(Target, E);
                }
                else
                {
                    var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.IsTargetable && !myUtility.ImmuneToMagic(x));
                    var et = EnemyList.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= E.Range).OrderBy(i => i.Health).FirstOrDefault();
                    if (et != null)
                    {
                        mySpellcast.Unit(Target, E);
                    }
                }
            }
            if (UseW && W.IsReady())
            {
                if (Player.CountAlliesInRange(W.Range) > 0 && Player.CountEnemiesInRange(W.Range) > 0)
                {
                    mySpellcast.Unit(null, W);
                }
            }
            if (UseR && R.IsReady())
            {                
                mySpellcast.PointBlank(null, R, R.Range, 3);
            }
        }
        private float GetDamage(Obj_AI_Hero target)
        {
            var damage = 0d;           
            if (E.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.E);
            }
            return (float)damage;
        }

        protected override void OnUpdate(EventArgs args)
        {           
            if (Player.IsDead)
            {
                myUtility.Reset();
                return;
            }
            switch (myOrbwalker.ActiveMode)
            {
                case myOrbwalker.OrbwalkingMode.None:
                    myUtility.Reset();
                    break;
                case myOrbwalker.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case myOrbwalker.OrbwalkingMode.LaneClear:
                    {
                        if (Q.IsReady())
                        {
                            var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                            if (minionQ == null) return;
                            var siegeQ = myFarmManager.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                            if (siegeQ != null && siegeQ.IsValidTarget())
                            {
                                Q.CastOnUnit(siegeQ);
                            }
                            else
                            {
                                var AnyQ = minionQ.OrderBy(i => i.Distance(Player)).FirstOrDefault(x => Q.IsKillable(x));
                                if (AnyQ != null && AnyQ.IsValidTarget())
                                {
                                    Q.CastOnUnit(AnyQ);
                                }
                            }
                        }
                    }
                    break;
            } 
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Nunu.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Unit(gapcloser.Sender, E));
                }
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.R)
            {
                LastR = myUtility.TickCount;
                mySpellcast.Pause(3000 + Game.Ping);
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (Player.IsChannelingImportantSpell() || myUtility.TickCount - LastR <= 0.25f)
            {
                args.Process = false;
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Nunu.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Nunu.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                var color = Player.CountEnemiesInRange(R.Range) >= 4 ? Color.Red : Color.Yellow;
                Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, color, "Hits: " + Player.CountEnemiesInRange(R.Range));
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.White);
            }
        }
    }
}
