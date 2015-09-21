using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCreations.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using ItemData = LeagueSharp.Common.Data.ItemData;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
{
    class Blitzcrank : PluginData
    {
        public Blitzcrank()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 940); //1050
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 600);

            Q.SetSkillshot(0.25f, 70f, 1800f, true, SkillshotType.SkillshotLine);           
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Blitzcrank.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Blitzcrank.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Blitzcrank.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Blitzcrank.Combo.R", "Use R").SetValue(true));
                config.AddSubMenu(combomenu);
            }            
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Blitzcrank.Muramana", "Muramana").SetValue(new Slider(50)));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Blitzcrank.Draw.Q", "Q").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Blitzcrank.Combo.Q").GetValue<bool>();
            var UseR = config.Item("EC.Blitzcrank.Combo.R").GetValue<bool>();
            if (UseR && R.IsReady())
            {
                if (Player.CountEnemiesInRange(R.Range) >= 4)
                {
                    R.Cast();
                }
            }
            if (Target.IsValidTarget())
            {
                try
                {
                    if (myUtility.ImmuneToMagic(Target)) return;
                    if (UseQ && Q.IsReady())
                    {
                        if (myUtility.ImmuneToCC(Target)) return;
                        mySpellcast.Hook(Target, Q);                          
                    }
                    if (UseR && R.IsReady())
                    {
                        if (R.IsKillable(Target) && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) < R.Range)
                        {
                            R.Cast();
                        }
                    }
                }
                catch { }
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
                        if (myUtility.PlayerManaPercentage < config.Item("EC.Blitzcrank.Muramana").GetValue<Slider>().Value)
                        {
                            if (Items.HasItem(3042) && Items.CanUseItem(3042)) Items.UseItem(3042);
                        }
                    }
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
                if (spell.SData.Name.ToLower() == "rocketgrab" && 
                    myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && 
                    config.Item("EC.Blitzcrank.Combo.E").GetValue<bool>() && E.IsReady())
                {
                   
                        if ((Obj_AI_Hero)spell.Target != null && spell.Target.IsEnemy)
                        {                           
                            var x = (Obj_AI_Hero)spell.Target;
                            if (x.HasBuff(("rocketgrab2")))
                            {
                                E.Cast();
                            }
                        }
                    
                }
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
            {
                if (args.Slot == SpellSlot.E)
                {
                    if (ItemData.Muramana.GetItem().IsReady() && !Player.HasBuff("Muramana") && myUtility.PlayerManaPercentage > config.Item("EC.Blitzcrank.Muramana").GetValue<Slider>().Value)
                    {
                        if (Items.HasItem(3042) && Items.CanUseItem(3042)) Items.UseItem(3042);
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
                    if (config.Item("EC.Blitzcrank.Combo.W").GetValue<bool>() && W.IsReady())
                    {
                        W.Cast();
                    }
                    if (config.Item("EC.Blitzcrank.Combo.E").GetValue<bool>() && E.IsReady())
                    {
                        E.Cast();
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Blitzcrank.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);                
                Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (Target.IsValidTarget(Q.Range))
                {
                    PredictionOutput pred = Q.GetPrediction(Target);
                    var box = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(Target.ServerPosition, (Q.Range - (Target.BoundingRadius / 2) - Q.Width)), Q.Width);
                    if (pred.CollisionObjects.Count(x => !x.IsAlly && x.IsMinion && x.IsHPBarRendered) == 0 && box.IsInside(Target))
                    {
                        if (!Target.IsMoving || myUtility.MovementDisabled(Target))
                        {
                            box.Draw(Color.Blue);
                        }
                        else if (box.IsInside(pred.CastPosition) && Vector2.Distance(Target.ServerPosition.To2D(), myUtility.PredictMovement(Target, Q.Delay, Q.Speed)) <= Q.Width + Target.BoundingRadius)
                        {
                            box.Draw(Color.Red);
                        }
                        else
                        {
                            box.Draw(Color.White);
                        }
                    }                    
                }
            }
        }
    }
}
