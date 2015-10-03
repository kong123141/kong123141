using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
{
    class Jinx : PluginData
    {
        public Jinx()
        {
            LoadSpells();
            LoadMenus();
            myDamageIndicator.DamageToUnit = GetDamage;
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1450f);
            E = new Spell(SpellSlot.E, 850f);
            R = new Spell(SpellSlot.R, 2500f);

            W.SetSkillshot(0.6f, 60f, 3300f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(1.2f, 1f, 1750f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 140f, 1700f, false, SkillshotType.SkillshotLine);
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Jinx.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Jinx.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Jinx.Combo.E", "Use E").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Jinx.Misc.E", "E Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Jinx.Misc.E2", "E Gapclosers").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Jinx.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Jinx.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Jinx.Draw.E", "E").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(W.Range, TargetSelector.DamageType.Physical);

            var UseW = Root.Item("EC.Jinx.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Jinx.Combo.E").GetValue<bool>();
            
            if (Target.IsValidTarget())
            {
                try
                {
                    if (UseW && W.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.Linear(Target, W, HitChance.High, true);
                    }
                    if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.Wall(Target, E, HitChance.High, myUtility.PlayerHealthPercentage > Target.HealthPercent); 
                    }
                }
                catch { }
            }
        }
        private void UpdateQ()
        {
            if (Q.Level > 0)
            {
                Q.Range = Player.BoundingRadius + 525 + (50 + 25 * Q.Level);
            }
        }
        private float GetDamage(Obj_AI_Hero target)
        {
            var damage = 0d;
            if (R.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.R);
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
            UpdateQ();
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
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if (spell.SData.Name.ToLower() == "jinxq")
                {
                    LastQ = myUtility.TickCount;
                }
                if ((spell.SData.Name.ToLower() == "jinxw") || (spell.SData.Name.ToLower() == "jinxe"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Root.Item("EC.Jinx.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Circular(sender.ServerPosition, E));
                }
            }
        }        
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Jinx.Misc.E2").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;                    
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Circular(gapcloser.End, E));
                }
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (Root.Item("EC.Jinx.Combo.Q").GetValue<bool>() && Player.HasBuff("JinxQ"))
                    {
                        if (args.Target.CountEnemiesInRange(300) <= 1 && Vector3.Distance(Player.ServerPosition, args.Target.ServerPosition) <= 525)
                        {
                            Q.Cast();
                        } 
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Jinx.Draw.Q").GetValue<bool>() && Q.Level > 0 )
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }

            if (Root.Item("EC.Jinx.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Jinx.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
        }
    }
}
