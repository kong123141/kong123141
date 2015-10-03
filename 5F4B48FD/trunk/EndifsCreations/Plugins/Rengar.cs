using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.SummonerSpells;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Drawing;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
{
    class Rengar : PluginData
    {
        public Rengar()
        {
            LoadSpells();
            LoadMenus();
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 500);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R);

            E2 = new Spell(SpellSlot.E, 1000);

            E.SetSkillshot(0.5f, 70f, 1600f, true, SkillshotType.SkillshotLine);
            E2.SetSkillshot(0.5f, 70f, 1600f, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellList.Add(E2);

            myDamageIndicator.DamageToUnit = GetDamage;
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Rengar.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Rengar.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Rengar.Combo.E", "Use E").SetValue(true));
                //combomenu.AddItem(new MenuItem("EC.Rengar.Combo.Dive", "Turret Dive").SetValue(false));
                //combomenu.AddItem(new MenuItem("EC.Rengar.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Rengar.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Rengar.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Rengar.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            var UseW = Root.Item("EC.Rengar.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Rengar.Combo.E").GetValue<bool>();
            if (!Player.IsDashing() && !Player.HasBuff("rengarr"))
            {
                if (UseW && W.IsReady())
                {
                    mySpellcast.PointBlank(null, W, 400);
                }
                if (Target.IsValidTarget())
                {
                    if (UseE && E.IsReady())
                    {
                        if (myUtility.IsInBush(Player) && !myUtility.IsInBush(Target) && !Player.IsDashing()) return;
                        mySpellcast.Linear(Target, E, HitChance.High, true);
                    }
                }
            }
        }
        private void LaneClear()
        {
            if (W.IsReady())
            {
                myFarmManager.LanePointBlank(W, W.Range, false, Ferocity >= 5);
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
            if (E.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.E);
            }
            if (Items.HasItem(3077) && Items.CanUseItem(3077))
            {
                damage += Player.GetItemDamage(target, Damage.DamageItems.Tiamat);
            }
            if (Items.HasItem(3074) && Items.CanUseItem(3074))
            {
                damage += Player.GetItemDamage(target, Damage.DamageItems.Hydra);
            }
            if (Items.HasItem(3748) && Items.CanUseItem(3748))
            {
                damage += Player.GetItemDamage(target, Damage.DamageItems.Hydra);
            }
            return (float)damage;
        }
        
        private int Ferocity
        {
            get { return (int)Player.Mana; }
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
                    LaneClear();
                    break;
                case myOrbwalker.OrbwalkingMode.Harass:
                    Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                    if (Target.IsValidTarget())
                    {
                        if (E.IsReady())
                        {
                            mySpellcast.PointVector(Target.Position, E, Target.BoundingRadius);
                        }
                    }
                    break;
                case myOrbwalker.OrbwalkingMode.JungleClear:

                    if (E.IsReady() && Ferocity != 5)
                    {
   
                            myFarmManager.JungleLinear(E, E.Range);
                        
                    }
                    break;
            }
        }
        protected override void OnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
            {
                if (sender.IsMe)
                {
                    Q.Cast();
                    Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                    if (Target.IsValidTarget())
                    {                              
                        var x = (int)(Vector3.Distance(Player.ServerPosition, Target.ServerPosition) / 2f);                              
                        Utility.DelayAction.Add(x, () =>
                        {
                            mySpellcast.PointVector(Target.ServerPosition, E);               
                            W.Cast();
                            myItemManager.UseItems(1, Target);
                            myItemManager.UseItems(2, null);
                        }
                        );
                    }
                }
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R)
            {
                Utility.DelayAction.Add(1500, () => myItemManager.UseGhostblade());
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (Ferocity <=4)
            {
                Q.Cast();
            }
            else
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (Root.Item("EC.Rengar.Combo.Q").GetValue<bool>() && Q.IsReady())
                    {
                        Q.Cast();
                    }
                }
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (Root.Item("EC.Rengar.Combo.E").GetValue<bool>() && E.IsReady() && Orbwalking.InAutoAttackRange(target) && !Player.IsDashing())
                    {
                        mySpellcast.PointVector(target.Position, E, target.BoundingRadius);
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            var color = Ferocity == 5 ? Color.Yellow : Color.White;
            if (Root.Item("EC.Rengar.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, color);
            }
            if (Root.Item("EC.Rengar.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, color);
            }
        }
    }
}