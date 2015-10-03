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
    class Riven : PluginData
    {
        public Riven()
        {
            LoadSpells();
            LoadMenus();
        }       
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 260); 
            W = new Spell(SpellSlot.W, 125); 
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Riven.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Riven.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Riven.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Riven.Combo.R", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Riven.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Riven.Farm.Q", "Use Q").SetValue(true));
                Root.AddSubMenu(laneclearmenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Riven.Misc.Q", "Q Gapclosers").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Riven.Misc.W", "W Gapclosers").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Riven.Misc.Q2", "Q Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Riven.Misc.W2", "W Interrupts").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Riven.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Riven.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Riven.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Riven.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }

        }

        private bool CanQ = true;
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            var UseQ = Root.Item("EC.Riven.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Riven.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Riven.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Riven.Combo.R").GetValue<bool>();
            var CastItems = Root.Item("EC.Riven.Combo.Items").GetValue<bool>();
            if (UseW && W.IsReady())
            {
                mySpellcast.PointBlank(null, W, 150);
            }
            if (Target.IsValidTarget())
            {
                try
                {
                    if (UseQ && Q.IsReady() && CanQ)
                    {
                        Vector3 vec = Player.ServerPosition.Extend(Game.CursorPos, Q.Range);
                        if (Vector3.Distance(vec, Target.ServerPosition) <= 150)
                        {
                            Q.Cast(vec);
                        }
                    }
                    if (UseE && E.IsReady())
                    {
                        if (!Orbwalking.InAutoAttackRange(Target))
                        {
                            Vector3 vec = Player.ServerPosition.Extend(Game.CursorPos, E.Range);
                            if (Vector3.Distance(vec, Target.ServerPosition) <= Player.AttackRange + Player.BoundingRadius)
                            {
                                E.Cast(vec);
                            }
                        }
                        if (Player.HasBuff("riventricleavetwo"))
                        {
                            Vector3 vec = Player.ServerPosition.Extend(Game.CursorPos, E.Range);
                            if (Vector3.Distance(vec, Target.ServerPosition) <= 150)
                            {
                                E.Cast(vec);
                            }
                        }
                    }
                    if (UseR && R.IsReady())
                    {

                    }
                    if (CastItems)
                    {
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= 450f)
                        {
                            myItemManager.UseItems(1, Target);
                        }
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) < 500f)
                        {
                            myItemManager.UseItems(3, null);
                        }
                    }
                }
                catch { }
            }
            
        }

        private int qsteps;
      
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
            }
        }
        protected override void OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            if (Target != null)
            {
                if (args.SourceNetworkId == Player.NetworkId && Target.NetworkId == args.TargetNetworkId)
                {
                    if (myUtility.TickCount - myOrbwalker.LastAATick < 350 && Q.IsReady() && qsteps > 0)
                    {
                        CanQ = true;
                    }
                }
            }
        }
        protected override void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && myOrbwalker.IsAutoAttack(args.SData.Name))
            {
                CanQ = true;
            }
        }
        protected override void OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Buff.Name == "riventricleaveone")
                {
                    if (CanQ) CanQ = false; 
                    qsteps = 1;
                }
                else if (qsteps != 2 && args.Buff.Name == "riventricleavetwo")
                {
                    if (CanQ) CanQ = false; 
                    qsteps = 2;
                }
                else if (qsteps <= 2 && args.Buff.Name == "riventricleavethree")
                {
                    if (CanQ) CanQ = false; 
                    qsteps = 3;
                }
            }
        }      
        protected override void OnBuffRemove(Obj_AI_Base sender, Obj_AI_BaseBuffRemoveEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Buff.Name == "riventricleavethree" && qsteps > 0)
                {
                    qsteps = 0;
                }
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.Q)
            {
                LastQ = myUtility.TickCount;
                myOrbwalker.ResetAutoAttackTimer();
            }
        }
        protected override void ProcessDamageBuffer(Obj_AI_Base sender, Obj_AI_Hero target, SpellData spell, float damage, myDamageBuffer.DamageTriggers type)
        {
            if (sender != null && target.IsMe)
            {
                if (E.IsReady())
                {
                    E.Cast(Player.ServerPosition.Extend(Game.CursorPos, E.Range));
                }
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if ((myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Riven.Combo.Items").GetValue<bool>()) && Orbwalking.InAutoAttackRange(target))
            {
                myItemManager.UseItems(2, null);
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Riven.Misc.Q").GetValue<bool>() && Q.IsReady() && qsteps == 2)
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= Q.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => Q.Cast(gapcloser.End));
                }
            }
            if (Root.Item("EC.Riven.Misc.W").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) <= W.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast());
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (sender.IsEnemy)
            {
                if (myUtility.ImmuneToCC(sender)) return;
                if (Root.Item("EC.Riven.Misc.Q2").GetValue<bool>() && Q.IsReady() && qsteps == 2)
                {
                    if (Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= Q.Range)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => Q.Cast(sender.ServerPosition));
                    }
                }
                if (Root.Item("EC.Riven.Misc.W2").GetValue<bool>() && W.IsReady())
                {
                    if (Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= W.Range)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast());
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Riven.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Riven.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Riven.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Riven.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.White);
            }
        }
    }
}
