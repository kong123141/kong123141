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
    class Zilean : PluginData
    {
        public Zilean()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 550);
            R = new Spell(SpellSlot.R, 900);

            Q.SetSkillshot(0.30f, 210f, 2000f, false, SkillshotType.SkillshotCircle);
            E.SetTargetted(float.MaxValue, float.MaxValue);
            R.SetTargetted(float.MaxValue, float.MaxValue);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Zilean.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Zilean.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Zilean.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Zilean.Combo.R", "Use R").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Zilean.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Zilean.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Zilean.Draw.E", "E").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = Root.Item("EC.Zilean.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Zilean.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Zilean.Combo.E").GetValue<bool>();

            if (Target.IsValidTarget())
            {
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {                        
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= Q.Range)
                        {
                            if (Target.HasBuff("ZileanQEnemyBomb"))
                            {
                                mySpellcast.CircularPrecise(Target, Q, HitChance.High, Q.Range,200, 0, 0, 0);
                            }
                            else
                            {
                                mySpellcast.CircularPrecise(Target, Q, HitChance.High, Q.Range, 200);
                            }
                        }
                    }
                    if (UseW && W.IsReady())
                    {
                        if (UseQ && Target.HasBuff("ZileanQEnemyBomb"))
                        {
                            W.Cast();
                        }
                    }
                    if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= E.Range)
                        {
                            if (UseQ && Q.IsReady() && !myUtility.MovementDisabled(Target)) return;
                            E.Cast(Target);
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
                    break;
                case myOrbwalker.OrbwalkingMode.Combo:
                    Combo();
                    break;                
            }
        }
        protected override void ProcessDamageBuffer(Obj_AI_Base sender, Obj_AI_Hero target, SpellData spell, float damage, myDamageBuffer.DamageTriggers type)
        {
            if (sender != null && target.IsMe)
            {
                switch (type)
                {
                    case myDamageBuffer.DamageTriggers.Killable:
                        if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Zilean.Combo.R").GetValue<bool>() && R.IsReady())
                        {
                            mySpellcast.Unit(null, R);
                        }
                        break;
                    case myDamageBuffer.DamageTriggers.TonsOfDamage:
                        break;
                }
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "zileanq") || (spell.SData.Name.ToLower() == "zileanw") || (spell.SData.Name.ToLower() == "zileane"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
           
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Zilean.Draw.W").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Zilean.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Zilean.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
        }
    }
}
