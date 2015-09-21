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
    class Trundle : PluginData
    {
        public Trundle()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 900);//1000 radius
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R, 700);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Trundle.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Trundle.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Trundle.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Trundle.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Trundle.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Trundle.Misc.E", "E Interrupts").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Trundle.Draw.E", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            var UseQ = config.Item("EC.Trundle.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Trundle.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Trundle.Combo.E").GetValue<bool>();
            var UseR = config.Item("EC.Trundle.Combo.R").GetValue<bool>();
            var CastItems = config.Item("EC.Trundle.Combo.Items").GetValue<bool>();
            if (UseR && R.IsReady() && Player.ServerPosition.CountEnemiesInRange(500) > 0 && Player.Spellbook.IsAutoAttacking)
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToMagic(x));
                foreach (var x in EnemyList.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range).OrderByDescending(a => a.Armor).ThenBy(mr => mr.PercentMagicReduction))
                {
                    R.CastOnUnit(x);
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {
                    if (myUtility.ImmuneToDeath(Target)) return;
                    if (CastItems) { myItemManager.UseItems(0, Target); }
                    if (UseQ && Q.IsReady() && Orbwalking.InAutoAttackRange(Target))
                    {
                        Q.Cast(Target);
                    }
                    if (UseW && W.IsReady())
                    {
                        mySpellcast.CircularBetween(Target, W);                      
                    }
                    if (UseE && E.IsReady())
                    {
                        mySpellcast.Wall(Target, E, HitChance.High);                        
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
            Target = myUtility.GetTarget(E.Range + (E.Width / 2), TargetSelector.DamageType.Physical);
            if (Target.IsValidTarget())
            {
                mySpellcast.Wall(Target, E, HitChance.High);    
            }
        }

        private GameObject FrozenDomain;

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
                case myOrbwalker.OrbwalkingMode.LaneClear:
                    {
                        if (Q.IsReady())
                        {
                            var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                            if (minionQ == null) return;
                            var siegeQ = myFarmManager.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                            if (siegeQ != null && siegeQ.IsValidTarget())
                            {
                                Q.CastOnUnit(siegeQ);
                            }
                            else
                            {
                                if (FrozenDomain != null && Vector3.Distance(Player.Position, FrozenDomain.Position) <= 1000)
                                {
                                    foreach (var AnyQ in minionQ.Where(x => Q.IsKillable(x)))
                                    {
                                        Q.CastOnUnit(AnyQ);
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }
        protected override void OnCreate(GameObject sender, EventArgs args)
        {            
            if (sender.Name.Equals("Trundle_W"))
            {
                FrozenDomain = sender;
            }
        }
        protected override void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Equals("Trundle_W"))
            {
                FrozenDomain = null;
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (config.Item("EC.Trundle.Combo.Q").GetValue<bool>() && Q.IsReady() && Orbwalking.InAutoAttackRange(target))
                    {
                        Q.Cast();
                    }
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear)
                {
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear && target is Obj_AI_Minion && Q.IsKillable((Obj_AI_Minion)target))
                    {
                        Q.Cast();
                    }
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("EC.Trundle.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(sender)) return;
                    mySpellcast.Wall(sender, E, HitChance.High);
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Trundle.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
        }
    }
}
