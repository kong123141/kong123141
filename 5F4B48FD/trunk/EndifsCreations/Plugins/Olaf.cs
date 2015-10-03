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
    class Olaf : PluginData
    {
        public Olaf()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1000); //longest throw            
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R);

            Q2 = new Spell(SpellSlot.Q, 400); //shortest throw

            Q.SetSkillshot(0.25f, 75f, 1500f, false, SkillshotType.SkillshotLine);
            Q2.SetSkillshot(0.25f, 75f, 1600f, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellList.Add(Q2);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Olaf.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Olaf.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Olaf.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Olaf.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Olaf.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Olaf.Draw.Q", "Q").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            var UseQ = Root.Item("EC.Olaf.Combo.Q").GetValue<bool>();
            //var UseW = Root.Item("EC.Olaf.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Olaf.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Olaf.Combo.R").GetValue<bool>();
            var CastItems = Root.Item("EC.Olaf.Combo.Items").GetValue<bool>();

            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {
                    if (myUtility.ImmuneToDeath(Target)) return;
                    if (CastItems) { myItemManager.UseItems(0, Target); }
                    if (UseQ && Q.IsReady())
                    {
                        PredictionOutput pred = Q.GetPrediction(Target);
                        if (Vector3.Distance(Player.ServerPosition, pred.CastPosition) <= Q.Range)
                        {
                            if (!Orbwalking.InAutoAttackRange(Target))
                            {
                                if (pred.Hitchance >= HitChance.Medium)
                                {
                                    Q.Cast(Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition) + Target.BoundingRadius));
                                }
                            }
                            Q2.Cast(Player.ServerPosition.Extend(pred.CastPosition, Target.BoundingRadius));
                        }
                    }
                    if (UseE && E.IsReady() && E.IsInRange(Target))
                    {
                        E.CastOnUnit(Target);
                    }
                    if (UseR && R.IsReady())
                    {
                        if (Player.CountEnemiesInRange(300) > 1 && myUtility.MovementDisabled(Player))
                        {
                            R.Cast();
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

        private static bool WActive
        {
            get { return ObjectManager.Player.HasBuff("OlafFrenziedStrikes"); }
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
            } 
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (Root.Item("EC.Olaf.Combo.W").GetValue<bool>() && W.IsReady() && Orbwalking.InAutoAttackRange(target))
                    {
                        W.Cast();
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Olaf.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
        }
    }
}
