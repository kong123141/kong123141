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
    class Karthus : PluginData
    {
        public Karthus()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {

            Q = new Spell(SpellSlot.Q, 875); 
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 425);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.625f, 100, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.5f, W.Instance.SData.CastRadius, 1600, false, SkillshotType.SkillshotCircle); //800 / 900 / 1000 / 1100 / 1200 

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Karthus.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Karthus.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Karthus.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Karthus.Combo.R", "Use R").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Karthus.Misc.W", "W Gapclosers").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Karthus.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Karthus.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Karthus.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Karthus.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = Root.Item("EC.Karthus.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Karthus.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Karthus.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Karthus.Combo.R").GetValue<bool>();

            if (UseE && myUtility.TickCount - LastSpell > (myHumazier.SpellDelay * 2))
            {
                mySpellcast.Toggle(null, E, SpellSlot.E, 0, 400);  
            }
            if (UseR && R.IsReady())
            {
                if (Player.CountEnemiesInRange(E.Range) > 0) return;
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                var rtarget = EnemyList.Where(x => R.IsKillable(x));
                if (rtarget.Count() >= 3)
                {
                    R.Cast();
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {
                    if (myUtility.ImmuneToMagic(Target)) return;
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.CircularPrecise(Target, Q, HitChance.High, Q.Range, 100);
                    }
                    if (UseW && W.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.CircularAoe(Target, W, HitChance.High, W.Range);
                    }
                }
                catch { }
            }
        }

        private static bool EActive
        {
            get { return ObjectManager.Player.HasBuff("KarthusDefile"); }
        }

        protected override void OnUpdate(EventArgs args)
        {           
            if (Player.IsDead)
            {
                myUtility.Reset();
                return;
            }
            if (Player.IsZombie)
            {
                if (!EActive)
                {
                    E.Cast();
                }
                if (R.IsReady())
                {
                    var rList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                    var rtarget = rList.Where(x => R.IsKillable(x));
                    if (rtarget.Any())
                    {
                        R.Cast();
                    }
                }
                if (W.IsReady())
                {
                    var wt = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
                    if (wt.IsValidTarget())
                    {
                        mySpellcast.CircularAoe(wt, W, HitChance.Medium);
                    }
                } 
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable);
                foreach (var q in EnemyList.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= Q.Range + 100))
                {
                    mySpellcast.CircularPrecise(q, Q, HitChance.High, Q.Range, 100);
                }
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
                            myOrbwalker.SetAttack(false);
                            var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                            if (minionQ == null) return;
                            var siegeQ = myFarmManager.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                            if (siegeQ != null && siegeQ.IsValidTarget())
                            {
                                Q.Cast(siegeQ);
                            }
                            else
                            {
                                var AnyQ = minionQ.FirstOrDefault(x => Q.IsKillable(x));
                                if (AnyQ != null && AnyQ.IsValidTarget())
                                {
                                    Q.Cast(AnyQ);
                                }
                            }
                            myOrbwalker.SetAttack(true);
                        }
                    }
                    break;
            } 
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if (spell.SData.Name.ToLower().Contains("karthuslaywastea"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.R)
            {
                LastR = myUtility.TickCount;
                mySpellcast.Pause(3000 + Game.Ping);
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (Player.IsChannelingImportantSpell() || myUtility.TickCount - LastR <= 0.5f)
            {
                args.Process = false;
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Karthus.Misc.W").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= W.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.CircularAoe(gapcloser.Sender, W, HitChance.Medium));
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Karthus.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Karthus.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Karthus.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Karthus.Draw.R").GetValue<bool>() && R.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                var rtarget = EnemyList.Where(x => R.IsKillable(x)).ToList();
                if (rtarget.Any())
                {
                    Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.Yellow, "Hits: " + rtarget.Count());
                }
            }
            
        }
    }
}
