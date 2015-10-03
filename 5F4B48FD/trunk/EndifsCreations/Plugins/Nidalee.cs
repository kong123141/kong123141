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
    class Nidalee : PluginData
    {
        public Nidalee()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1500);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R);

            Q2 = new Spell(SpellSlot.Q, 200);
            W2 = new Spell(SpellSlot.W, 375); //750 prowled
            E2 = new Spell(SpellSlot.E, 300);
            R2 = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.25f, 40, 1300, true, SkillshotType.SkillshotLine);

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
                combomenu.AddItem(new MenuItem("EC.Nidalee.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Nidalee.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Nidalee.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Nidalee.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Nidalee.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }            

            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Nidalee.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Nidalee.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Nidalee.Draw.E", "E").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = 
                TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : 
                Human ? TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical) :           
                Hunted != null && Hunted.IsValidTarget() ? Hunted : TargetSelector.GetTarget(W2.Range, TargetSelector.DamageType.Magical);                                               

            var UseQ = Root.Item("EC.Nidalee.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Nidalee.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Nidalee.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Nidalee.Combo.R").GetValue<bool>();            
            var CastItems = Root.Item("EC.Nidalee.Combo.Items").GetValue<bool>();
            if (Human)
            {
                if (UseE && E.IsReady())
                {
                    if (myUtility.PlayerHealthPercentage < 75 || (!Player.HasBuff("PrimalSurge") && Target.IsValidTarget() && Orbwalking.InAutoAttackRange(Target)))
                    {
                        E.CastOnUnit(Player);
                    }
                }
            }
            if (Cougar)
            {
                if (UseR && R.IsReady())
                {
                    if (Target.IsValidTarget() && !Orbwalking.InAutoAttackRange(Target)) R.Cast();
                    else if (UseE && E2.IsReady())
                    {
                        W2.Cast(Player.ServerPosition.Extend(Game.CursorPos, 400));
                        Utility.DelayAction.Add(200, () => R.Cast());
                    }
                    R.Cast();
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {                                        
                    if (Human)
                    {
                        if (UseQ && Q.IsReady())
                        {                            
                            mySpellcast.Linear(Target, Q, HitChance.High);
                        }
                        if (UseW && W.IsReady())
                        {
                            mySpellcast.CircularPrecise(Target, W, HitChance.High, W.Range, 50);
                        }
                        if (UseR && R.IsReady())
                        {
                            if (UseQ && UseW && !Q.IsReady() && !W.IsReady())
                            {
                                if (Target.HasBuff("nidaleepassivehunted") && Vector3.Distance(Player.ServerPosition,Target.ServerPosition) <= 150)
                                {
                                    R.Cast();
                                }
                                else if (UseE && E.IsReady() && !Player.HasBuff("PrimalSurge"))
                                {
                                    E.Cast(Player);
                                    R.Cast();
                                }
                                else if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= W2.Range && Player.HasBuff("PrimalSurge"))
                                {
                                    R.Cast();
                                }
                            }
                        }
                    }
                    else if (Cougar)
                    {
                        if (UseW && W2.IsReady())
                        {
                            if (Target.HasBuff("nidaleepassivehunted"))
                            {
                                if (Target.CountEnemiesInRange(500) > 0) return;
                                if (Target.UnderTurret(true)) return;

                                W2.Range = 750;
                                if (W2.IsKillable(Target))
                                {
                                    W2.Cast(Player.ServerPosition.Extend(Target.ServerPosition, Vector3.Distance(Player.ServerPosition, Target.ServerPosition) + Target.BoundingRadius));
                                }
                                W2.Cast(Player.ServerPosition.Extend(Target.ServerPosition, Vector3.Distance(Player.ServerPosition, Target.ServerPosition) + Target.BoundingRadius));                                
                            }
                            else
                            {
                                W2.Cast(Player.ServerPosition.Extend(Target.ServerPosition, Vector3.Distance(Player.ServerPosition, Target.ServerPosition) + Target.BoundingRadius));
                            }
                            W2.Range = 375;
                        }
                        if (UseQ && UseW && UseE && !Q.IsReady() && !W.IsReady() && !E.IsReady())
                        {
                            if (Orbwalking.InAutoAttackRange(Target)) return;
                            R.Cast();
                        }
                    }
                }
                catch { }
            }
        }
        private void Harass()
        {
            Target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() :
                Human ? TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical) : TargetSelector.GetTarget(W2.Range, TargetSelector.DamageType.Physical);
            if (Target.IsValidTarget())
            {
                if (Human)
                {
                    mySpellcast.Linear(Target, Q, HitChance.High);
                }
            }
            if (Cougar)
            {
                if (Player.CountEnemiesInRange(400) <= 0)
                {
                    if (W2.IsReady()) W2.Cast(Player.ServerPosition.Extend(Game.CursorPos, 400));
                    if (R2.IsReady()) Utility.DelayAction.Add(200, () => R.Cast());
                }
            }
        }
        private Obj_AI_Hero Hunted
        {
            get
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && x.HasBuff("nidaleepassivehunted") && Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= 750).OrderBy(i => i.Health);
                return EnemyList.FirstOrDefault();
            }
        }

        private bool Human
        {
            get
            {
                return Player.Spellbook.GetSpell(SpellSlot.W).Name == "Bushwhack";
            }
        }
        private bool Cougar
        {
            get
            {
                return Player.Spellbook.GetSpell(SpellSlot.W).Name == "Pounce";
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
                    break;
                case myOrbwalker.OrbwalkingMode.Combo:
                    Combo();                    
                    break;
                case myOrbwalker.OrbwalkingMode.Harass:
                    Harass();
                    break;
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (Root.Item("EC.Nidalee.Combo.Q").GetValue<bool>())
                    {
                        if (Cougar)
                        {
                            Q2.Cast();
                        }
                    }
                    if (Root.Item("EC.Nidalee.Combo.E").GetValue<bool>())
                    {
                        if (Human)
                        {
                            E.Cast(Player);
                        }
                    }
                }
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (Root.Item("EC.Nidalee.Combo.E").GetValue<bool>() && Orbwalking.InAutoAttackRange(target))
                    {
                        if (Cougar)
                        {
                            E2.Cast((Obj_AI_Hero)target);
                        }
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Human)
            {
                if (Root.Item("EC.Nidalee.Draw.Q").GetValue<bool>() && Q.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
                }
            }
            if (Cougar)
            {
                if (Root.Item("EC.Nidalee.Draw.W").GetValue<bool>() && W2.Level > 0)
                {
                    if (W2.Range >= 750)
                    {
                        Render.Circle.DrawCircle(Player.Position, W2.Range, Color.Cyan);
                    }
                    Render.Circle.DrawCircle(Player.Position, W2.Range, Color.White);
                }
            } 
        }
    }
}
