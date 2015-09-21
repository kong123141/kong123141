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
    class Heimerdinger : PluginData
    {
        public Heimerdinger()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 50);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 920);
            R = new Spell(SpellSlot.R);
            
            E2 = new Spell(SpellSlot.E, 1465);

            W.SetSkillshot(.25f, 40, 2500, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(.25f, 120, 1000, false, SkillshotType.SkillshotCircle);
            E2.SetSkillshot(.25f, 120, 1000, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellList.Add(E2);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Heimerdinger.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Heimerdinger.Combo.E", "Use E").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Heimerdinger.Misc.E", "E Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Heimerdinger.Misc.E2", "E Gapclosers").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Heimerdinger.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Heimerdinger.Draw.E", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            var UseW = config.Item("EC.Heimerdinger.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Heimerdinger.Combo.E").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                try
                {
                    if (!Upgrade)
                    {
                        if (UseW && W.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                        {
                            mySpellcast.Linear(Target, W, HitChance.High, true);
                        }
                        if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                        {
                            E.Range = 925;
                            mySpellcast.CircularPrecise(Target, E, HitChance.High);
                        }
                    }
                    else
                    {
                        if (UseW && W.IsReady())
                        {
                            mySpellcast.Linear(Target, W, HitChance.High);
                        }
                        if (UseE && E.IsReady())
                        {
                            if (myUtility.MovementDisabled(Target))
                            {
                                E.Range = 925 + 540;
                                mySpellcast.CircularPrecise(Target, E, HitChance.High);                               
                            }
                        }
                    }
                }
                catch { }
            }
        }
        private bool Upgrade
        {
            get
            {
                return Player.HasBuff("HeimerdingerR");
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
            }            
        }

        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "heimerdingerw") || (spell.SData.Name.ToLower() == "heimerdingere"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("EC.Heimerdinger.Misc.E").GetValue<bool>() && E.IsReady())
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
            if (config.Item("EC.Heimerdinger.Misc.E2").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Circular(gapcloser.End, E));      
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Heimerdinger.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Heimerdinger.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
        }
    }
}
