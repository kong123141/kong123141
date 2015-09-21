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
    class Veigar : PluginData
    {
        public Veigar()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {            
            Q = new Spell(SpellSlot.Q, 950);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 600);

            Q.SetSkillshot(0.25f, 70f, 2000f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(1.25f, 225f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(1.2f, 25f, float.MaxValue, false, SkillshotType.SkillshotCircle);

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
                combomenu.AddItem(new MenuItem("EC.Veigar.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Veigar.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Veigar.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Veigar.Combo.R", "Use R").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Veigar.Misc.E", "E Gapclosers").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Veigar.Misc.E2", "E Interrupts").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Veigar.Draw.Q", "Q").SetValue(true));
                //drawmenu.AddItem(new MenuItem("EC.Veigar.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Veigar.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Veigar.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Veigar.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Veigar.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Veigar.Combo.E").GetValue<bool>();
            var UseR = config.Item("EC.Veigar.Combo.R").GetValue<bool>();
            if (UseR && R.IsReady())
            {
                if (Target.IsValidTarget())
                {
                    if (R.IsKillable(Target) && !myUtility.ImmuneToMagic(Target))
                    {
                        mySpellcast.Unit(Target, R);
                    }
                }
                else
                {
                    var EnemyList = HeroManager.Enemies.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range && !myUtility.ImmuneToMagic(Target) && R.IsKillable(Target));
                    foreach (var rtarget in EnemyList)
                    {
                        mySpellcast.Unit(rtarget, R);
                    }
                }
            }
         
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        if (Q.IsKillable(Target))
                        {
                            mySpellcast.Linear(Target, Q, HitChance.High, true, 1);
                        }
                        else if (myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                        {
                            mySpellcast.Linear(Target, Q, HitChance.High, true, 1);
                        }
                    }
                    if (UseW && W.IsReady())
                    {
                        mySpellcast.CircularPrecise(Target, W, HitChance.High);
                    }
                    if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        EPredict(Target);
                    }
                    
                }
                catch { }
            }
        }
        //todo -offsets
        private void EPredict(Obj_AI_Hero target)
        {
            var targE = E.GetPrediction(target);
            var pos = targE.CastPosition;
            if (pos.IsValid() && pos.Distance(Player.Position) < E.Range && targE.Hitchance >= HitChance.High)
            {
                E.Cast(pos.Extend(Player.Position, 375)); //offsets center to side
            }
        }

        private float GetDamage(Obj_AI_Hero target)
        {
            var damage = 0d;
            if (Q.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.Q);
            }
            if (W.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.W);
            }
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
                            myFarmManager.LaneLinear(Q, Q.Range, true, false ,1);
                        }
                    }
                    break;
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {                
                if ((spell.SData.Name.ToLower() == "veigarbalefulstrike") || (spell.SData.Name.ToLower() == "veigardarkmatter") || (spell.SData.Name.ToLower() == "veigareventhorizon"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("EC.Veigar.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range + (E.Width/2))
                {
                    if (myUtility.ImmuneToMagic(gapcloser.Sender) || myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Vector3 pos = myUtility.RandomPos(1, 25, 25, gapcloser.End);
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.Cast(pos));
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("EC.Veigar.Misc.E2").GetValue<bool>() && E.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(sender)) return;
                    PredictionOutput pred = E.GetPrediction(sender);
                    if (Vector3.Distance(Player.ServerPosition, pred.CastPosition) <= E.Range)
                    {
                        if (pred.Hitchance >= HitChance.High)
                        {
                            Vector3 pos = myUtility.RandomPos(1, 25, 25, Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition)));
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.Cast(pos));
                        }
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Veigar.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Veigar.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Veigar.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
