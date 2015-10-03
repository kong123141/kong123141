using System;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using ItemData = LeagueSharp.Common.Data.ItemData;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
{
    class Jayce : PluginData
    {
        public Jayce()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 285);
            E = new Spell(SpellSlot.E, 240);
            R = new Spell(SpellSlot.R);

            Q2 = new Spell(SpellSlot.Q, 1050); //1470  gate //200 aoe
            W2 = new Spell(SpellSlot.W, 285);
            E2 = new Spell(SpellSlot.E, 650);
            R2 = new Spell(SpellSlot.R);

            Q2.SetSkillshot(0.3f, 70f, 1500, true, SkillshotType.SkillshotLine);            

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellList.Add(Q2);
            SpellList.Add(W2);
            SpellList.Add(E2);
            SpellList.Add(R2);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Jayce.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Jayce.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Jayce.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Jayce.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Jayce.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }            
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Jayce.Muramana", "Muramana").SetValue(new Slider(50)));
                miscmenu.AddItem(new MenuItem("EC.Jayce.Misc.E", "E Gapclosers").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Jayce.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Jayce.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Jayce.Draw.E", "E").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() :
                Hammer ? TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical) : TargetSelector.GetTarget(1470f, TargetSelector.DamageType.Physical);

            var UseQ = Root.Item("EC.Jayce.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Jayce.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Jayce.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Jayce.Combo.R").GetValue<bool>();            
            var CastItems = Root.Item("EC.Jayce.Combo.Items").GetValue<bool>();
            if (Hammer)
            {
                if (UseR && R.IsReady())
                {
                    if (Target == null || !Target.IsValidTarget())
                    {
                        R.Cast();
                    }
                }
                if (UseW && W.IsReady())
                {
                    mySpellcast.PointBlank(null, W, W.Range);
                }
            }
            
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {                    
                    if (myUtility.ImmuneToDeath(Target)) return;
                    if (Hammer)
                    {
                        if (UseQ && Q.IsReady())
                        {
                            if (Orbwalking.InAutoAttackRange(Target) && UseE && E.IsReady()) return;
                            Q.Cast(Target);
                        }
                        if (UseE && E.IsReady())
                        {
                            E.Cast(Target);
                        }
                        if (UseR && R.IsReady())
                        {
                            if (UseQ && UseW && UseE && !Q.IsReady() && !W.IsReady() && !E.IsReady())
                            {
                                if (Orbwalking.InAutoAttackRange(Target)) return;
                                R.Cast();
                            }
                        }
                    }
                    else if (Cannon)
                    {
                        if (UseQ && UseE)
                        {
                            if (E2.IsReady() && Q2.IsReady() && Player.Mana > E2.Instance.ManaCost + Q2.Instance.ManaCost)
                            {
                                Q2.Range = 1500;
                                Q2.SetSkillshot(0.3f, 70f, 2180, true, SkillshotType.SkillshotLine);

                                PredictionOutput pred = Q2.GetPrediction(Target);
                                if (pred.CollisionObjects.Count == 0 && Vector3.Distance(Player.ServerPosition, pred.CastPosition) <= Q2.Range)
                                {
                                    if (pred.Hitchance >= HitChance.High)
                                    {
                                        Vector3 pos = myUtility.RandomPos(25, 25, 25, Player.ServerPosition.Extend(pred.CastPosition, Math.Min(200,Vector3.Distance(Player.ServerPosition, pred.CastPosition) - Target.BoundingRadius)));
                                        E2.Cast(pos);
                                        Q2.Cast(Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition)));
                                    }
                                }
                                Q2.Range = 1050;
                                Q2.SetSkillshot(0.3f, 70f, 1500, true, SkillshotType.SkillshotLine);    
                            }
                        }
                        if (UseQ && Q2.IsReady())
                        {
                            if (UseE && E2.IsReady()) return;
                            PredictionOutput pred = Q2.GetPrediction(Target);
                            if (pred.CollisionObjects.Count == 0 && Vector3.Distance(Player.ServerPosition, pred.CastPosition) <= Q2.Range)
                            {
                                if (pred.Hitchance >= HitChance.High)
                                {
                                    Q2.Cast(Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition)));
                                }
                            }
                        }
                        if (UseR && R.IsReady())
                        {
                            if (UseQ && UseE && !Q2.IsReady() && !W2.IsReady() && !E2.IsReady() && !Player.HasBuff("jaycehypercharge"))
                            {
                                if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) >= 200)
                                {
                                    R.Cast();
                                }
                            }
                        }
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
        private void Harass()
        {
            Target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() :
                Cannon ? TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Physical) : TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (Target.IsValidTarget())
            {
                if (Cannon)
                {
                    if (E2.IsReady() && Q2.IsReady() && Player.Mana > E2.Instance.ManaCost + Q2.Instance.ManaCost)
                    {
                        Q2.Range = 1500;
                        Q2.SetSkillshot(0.3f, 70f, 2180, true, SkillshotType.SkillshotLine);

                        PredictionOutput pred = Q2.GetPrediction(Target);
                        if (pred.CollisionObjects.Count == 0 && Vector3.Distance(Player.ServerPosition, pred.CastPosition) <= Q2.Range)
                        {
                            if (pred.Hitchance >= HitChance.High)
                            {
                                E2.Cast(Player.ServerPosition.Extend(pred.CastPosition, Math.Min(200, Vector3.Distance(Player.ServerPosition, pred.CastPosition) - Target.BoundingRadius)));
                                Q2.Cast(Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition)));
                            }
                        }

                        Q2.Range = 1050;
                        Q2.SetSkillshot(0.3f, 70f, 1500, true, SkillshotType.SkillshotLine);
                    }
                    else if (Q2.IsReady())
                    {
                        PredictionOutput pred = Q2.GetPrediction(Target);
                        if (pred.CollisionObjects.Count == 0 && Vector3.Distance(Player.ServerPosition, pred.CastPosition) <= Q2.Range)
                        {
                            if (pred.Hitchance >= HitChance.High)
                            {
                                Q2.Cast(Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition)));
                            }
                        }
                    }
                }                
            }
        }

        private bool Hammer
        {
            get
            {
                return Player.Spellbook.GetSpell(SpellSlot.R).Name == "JayceStanceHtG";
            }
        }
        private bool Cannon
        {
            get
            {
                return Player.Spellbook.GetSpell(SpellSlot.R).Name == "jaycestancegth";
            }
        }

        private static GameObject AccelerationGate;
        protected override void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Jayce_") && sender.Name.Contains("_accel_gate_start"))
            {
                AccelerationGate = sender;
            }
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
                    if (Player.HasBuff("Muramana"))
                    {                                   
                        if (Cannon || (myUtility.PlayerManaPercentage < Root.Item("EC.Jayce.Muramana").GetValue<Slider>().Value))
                        {
                            if (Items.HasItem(3042) && Items.CanUseItem(3042)) Items.UseItem(3042);
                        }
                    }
                    break;
                case myOrbwalker.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case myOrbwalker.OrbwalkingMode.Harass:
                    Harass();
                    break;
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
            {
                if (Hammer)
                {
                    if (args.Slot == SpellSlot.Q || args.Slot == SpellSlot.E)
                    {
                        if (ItemData.Muramana.GetItem().IsReady() && !Player.HasBuff("Muramana") && myUtility.PlayerManaPercentage > Root.Item("EC.Jayce.Muramana").GetValue<Slider>().Value)
                        {
                            if (Items.HasItem(3042) && Items.CanUseItem(3042)) Items.UseItem(3042);
                        }
                    }
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Jayce.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    if (Hammer)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.Cast(gapcloser.Sender));                        
                    }
                    else if (Cannon && R.IsReady())
                    {
                        R.Cast();
                    }
                }
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (Root.Item("EC.Jayce.Combo.W").GetValue<bool>())
                    {
                        if (Cannon)
                        {
                            W2.Cast();
                        }
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Hammer)
            {
                if (Root.Item("EC.Jayce.Draw.Q").GetValue<bool>() && Q.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
                }
                if (Root.Item("EC.Jayce.Draw.W").GetValue<bool>() && W.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
                }
                if (Root.Item("EC.Jayce.Draw.E").GetValue<bool>() && E.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
                }
                
            }
            else if (Cannon)
            {
                if (Root.Item("EC.Jayce.Draw.Q").GetValue<bool>() && Q2.Level > 0)
                {
                    if (Q2.Range >= 1500)
                    {
                        Render.Circle.DrawCircle(Player.Position, 1500, Color.Cyan);    
                    }
                    Render.Circle.DrawCircle(Player.Position, Q2.Range, Color.White);
                }               
                if (Root.Item("EC.Jayce.Draw.E").GetValue<bool>() && E2.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, E2.Range, Color.White);
                }
            }
        }
    }
}
