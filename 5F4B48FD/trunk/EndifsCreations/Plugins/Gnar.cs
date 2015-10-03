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
    class Gnar : PluginData
    {
        public Gnar()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {

            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R);
            
            Q2 = new Spell(SpellSlot.Q, 250);
            W2 = new Spell(SpellSlot.W, 525);
            E2 = new Spell(SpellSlot.E, 475);
            R2 = new Spell(SpellSlot.R, 420);

            Q.SetSkillshot(0.25f, 60, 1200, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.5f, 150, float.MaxValue, false, SkillshotType.SkillshotCircle);
            
            Q2.SetSkillshot(0.25f, 80, 1200, true, SkillshotType.SkillshotLine);
            W2.SetSkillshot(0.25f, 80, float.MaxValue, false, SkillshotType.SkillshotLine);
            E2.SetSkillshot(0.5f, 150, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R2.Delay = 0.25f;

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
                combomenu.AddItem(new MenuItem("EC.Gnar.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Gnar.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Gnar.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Gnar.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Gnar.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Gnar.Misc.W", "W Gapclosers").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Gnar.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Gnar.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Gnar.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Gnar.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = 
                TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() :
                Mini ? TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical) : TargetSelector.GetTarget(W2.Range, TargetSelector.DamageType.Physical);

            var UseQ = Root.Item("EC.Gnar.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Gnar.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Gnar.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Gnar.Combo.R").GetValue<bool>();

            var CastItems = Root.Item("EC.Gnar.Combo.Items").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {                    
                    if (myUtility.ImmuneToDeath(Target)) return;
                    if (Mini)
                    {
                        if (UseQ && Q.IsReady() && !Transforming)
                        {
                            mySpellcast.Linear(Target, Q, HitChance.High, true, 1);
                        }
                        if (UseE && E.IsReady())
                        {
                            if (!Transforming || Transforming && UseR && R.IsReady())
                            {
                                PredictionOutput pred = E.GetPrediction(Target);
                                if (pred.Hitchance >= HitChance.High)
                                {
                                    Vector3 pos = Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition) + E.Range);
                                    if (pos.UnderTurret(true)) return;
                                    if (!Transforming && pos.CountEnemiesInRange(1500) > 0) return;
                                    E.Cast(pred.CastPosition);
                                }
                            }
                        }
                    }
                    else if (Mega)
                    {
                        if (UseQ && Q2.IsReady())
                        {
                            mySpellcast.Linear(Target, Q2, HitChance.High, true, 0);
                        }
                        if (UseW && W2.IsReady())
                        {
                            W2.Cast(Target);
                        }
                        if (UseE && E2.IsReady())
                        {
                            PredictionOutput pred = E2.GetPrediction(Target);
                            if (pred.Hitchance >= HitChance.High)
                            {                               
                                E2.Cast(Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition) + Target.BoundingRadius));
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
                Mini ? TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical) : TargetSelector.GetTarget(W2.Range, TargetSelector.DamageType.Physical);
            if (Target.IsValidTarget())
            {
                if (Mini)
                {
                    if (Q.IsReady() && !Transforming)
                    {
                        mySpellcast.Linear(Target, Q, HitChance.High, true, 1);
                    }
                }
            }
        }
        private bool Mini
        {
            get
            {
                return Player.CharData.BaseSkinName == "Gnar";
            }
        }
        private bool Mega
        {
            get
            {
                return Player.CharData.BaseSkinName == "gnarbig";
            }
        }

        private bool Transforming
        {
            get
            {
                return Mini && (Player.Mana <= Player.MaxMana && (Player.HasBuff("gnartransformsoon") || Player.HasBuff("gnartransform"))) //|| Mega && myUtility.PlayerManaPercentage <= 1
                    ;
            }
        }

        private static int LastWR;

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
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && Mega)
            {
                if (args.Slot == SpellSlot.W || args.Slot == SpellSlot.R)
                {
                    LastWR = myUtility.TickCount;
                }                
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Gnar.Misc.W").GetValue<bool>() && W2.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    if (Mega)
                    {                        
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W2.Cast(gapcloser.End));
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Mini)
            {
                if (Root.Item("EC.Gnar.Draw.Q").GetValue<bool>() && Q.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
                }
                if (Root.Item("EC.Gnar.Draw.E").GetValue<bool>() && E.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
                }
            }
            if (Mega)
            {
                if (Root.Item("EC.Gnar.Draw.Q").GetValue<bool>() && Q2.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, Q2.Range, Color.White);
                }
                if (Root.Item("EC.Gnar.Draw.W").GetValue<bool>() && W2.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, W2.Range, Color.White);
                }
                if (Root.Item("EC.Gnar.Draw.E").GetValue<bool>() && E2.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, E2.Range, Color.White);
                }
                if (Root.Item("EC.Gnar.Draw.R").GetValue<bool>() && R.Level > 0)
                {
                    Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
                }
            }
        }
       
    }
}
