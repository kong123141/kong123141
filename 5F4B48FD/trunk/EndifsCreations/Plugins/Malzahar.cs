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
    class Malzahar : PluginData
    {
        public Malzahar()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W, 750);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 700);

            Q.SetSkillshot(0.5f, 100, Q.Instance.SData.SpellCastTime, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.5f, 240, 20, false, SkillshotType.SkillshotCircle);     

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Nether Grasp", "Custom");
            {
                custommenu.AddItem(new MenuItem("EC.Malzahar.UseRKey", "Key").SetValue(new KeyBind(Root.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("EC.Malzahar.UseRType", "R").SetValue(new StringList(new[] { "Default", "Flash in" })));
                custommenu.AddItem(new MenuItem("EC.Malzahar.UseRDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("EC.Malzahar.UseRDrawDistance", "Draw Distance").SetValue(true));
                Root.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Malzahar.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Malzahar.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Malzahar.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Malzahar.Combo.R", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Malzahar.Combo.EType", "E Voidlings check").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Malzahar.Combo.RType", "R").SetValue(new StringList(new[] { "Always", "Killable", "Voidlings", "When all spells are used" })));
                combomenu.AddItem(new MenuItem("EC.Malzahar.NoRValue", "Don't R if > enemy").SetValue(new Slider(1, 1, 5)));
                
                Root.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {               
                harassmenu.AddItem(new MenuItem("EC.Malzahar.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Malzahar.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Malzahar.Harass.E", "Use E").SetValue(true));
                Root.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Malzahar.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                laneclearmenu.AddItem(new MenuItem("EC.Malzahar.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Malzahar.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Malzahar.Farm.E", "Use E").SetValue(true));                
                Root.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Malzahar.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Malzahar.Jungle.W", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Malzahar.Jungle.E", "Use E").SetValue(true));
                Root.AddSubMenu(junglemenu);
            }  
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Malzahar.QPredHitchance", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("EC.Malzahar.Misc.Q", "Q Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Malzahar.Misc.E", "E Gapcloser").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Malzahar.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Malzahar.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Malzahar.Draw.E", "E").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(W.Range, TargetSelector.DamageType.Magical);

            var UseQ = Root.Item("EC.Malzahar.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Malzahar.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Malzahar.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Malzahar.Combo.R").GetValue<bool>();
            var UseEVoidlings = Root.Item("EC.Malzahar.Combo.EType").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToMagic(Target)) return;
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.CircularAoe(Target, Q, QHitChance);
                    }
                    if (UseW && W.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        //mySpellcast.CircularAoe(Target, W, HitChance.Medium, W.Range, 240);
                        mySpellcast.CircularPrecise(Target, W, HitChance.Medium, W.Range, 200);
                    }
                    if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        if (UseEVoidlings)
                        {
                            if (NextCastSummons || VoidlingsTotal >= 1)
                            {
                                mySpellcast.Unit(Target, E);
                            }
                        }
                        else
                        {
                            mySpellcast.Unit(Target, E);
                        }
                    }
                    if (UseR && R.IsReady())
                    {
                        if ((Player.ServerPosition.CountEnemiesInRange(R.Range) - 1) < Root.Item("EC.Malzahar.NoRValue").GetValue<Slider>().Value)
                        {                            
                            switch (Root.Item("EC.Malzahar.Combo.RType").GetValue<StringList>().SelectedIndex)
                            {
                                case 0:
                                    mySpellcast.Unit(Target, R);
                                    break;
                                case 1:
                                    if (R.IsKillable(Target))
                                    {
                                        mySpellcast.Unit(Target, R);
                                    }
                                    break;
                                case 2:
                                    if (NextCastSummons || VoidlingsTotal >= 1)
                                    {
                                        mySpellcast.Unit(Target, R);
                                    }
                                    break;
                                case 3:
                                    if (UseQ && Q.CanCast(Target)) return;
                                    if (UseE && E.CanCast(Target)) return;
                                    if (UseW && W.CanCast(Target)) return;
                                    mySpellcast.Unit(Target, R);                                    
                                    break;
                            }
                        }
                    }
                }
                catch { }
            }           
        }
        private void Harass()
        {
            var UseQ = Root.Item("EC.Malzahar.Harass.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Malzahar.Harass.W").GetValue<bool>();
            var UseE = Root.Item("EC.Malzahar.Harass.E").GetValue<bool>();
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady() && Q.IsInRange(target))
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    Q.Cast(target.ServerPosition);
                }
                if (UseW && W.IsReady() && W.IsInRange(target))
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    mySpellcast.CircularAoe(target, W, HitChance.High, W.Range, 200);
                }
                if (UseE && E.IsReady() && E.IsInRange(target))
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    if (NextCastSummons || VoidlingsTotal >= 1)
                    {
                        mySpellcast.Unit(target, E);
                    }
                }
            }
        }
        private void LaneClear()
        {            
            if (myUtility.EnoughMana(Root.Item("EC.Malzahar.Farm.ManaPercent").GetValue<Slider>().Value))
            {
                if (Root.Item("EC.Malzahar.Farm.Q").GetValue<bool>() && Q.IsReady())
                {
                    myFarmManager.LaneCircular(Q, Q.Range, 100);
                }
                if (Root.Item("EC.Malzahar.Farm.W").GetValue<bool>() && W.IsReady())
                {
                    myFarmManager.LaneCircular(W, W.Range, 240);
                }
                if (Root.Item("EC.Malzahar.Farm.E").GetValue<bool>() && E.IsReady())
                {
                    myFarmManager.LaneLastHit(E, E.Range, null, true);
                }
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (Root.Item("EC.Malzahar.Jungle.Q").GetValue<bool>() && Q.IsReady() && Q.IsInRange(mob) && !Player.IsWindingUp)
                {
                    if (largemobs != null)
                    {
                        Q.Cast(largemobs.ServerPosition);
                    }
                    else
                    {
                        var mobQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.Health);
                        if (mobQ == null) return;
                        var qpred = Q.GetCircularFarmLocation(mobQ);
                        if (qpred.MinionsHit > 0)
                        {
                            Q.Cast(qpred.Position);
                        }
                    }
                }
                if (Root.Item("EC.Malzahar.Jungle.W").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null)
                    {
                        W.Cast(largemobs.ServerPosition);
                    }
                    else
                    {
                        var mobW = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.Health);
                        if (mobW == null) return;
                        var wpred = W.GetCircularFarmLocation(mobW);
                        if (wpred.MinionsHit > 0)
                        {
                            W.Cast(wpred.Position);
                        }
                    }
                }
                if (Root.Item("EC.Malzahar.Jungle.E").GetValue<bool>() && E.IsReady() && E.IsInRange(mob) && !Player.IsWindingUp)
                {
                    if (largemobs != null)
                    {
                        E.CastOnUnit(largemobs);
                    }
                    else
                    {
                        E.CastOnUnit(mob);
                    }
                }
            }
        }
        private void Custom()
        {
            if (R.IsReady())
            {
                Obj_AI_Hero target;
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                switch (Root.Item("EC.Malzahar.UseRType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        if (TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() && 
                            Vector3.Distance(Player.ServerPosition, TargetSelector.GetSelectedTarget().ServerPosition) <= R.Range)
                        {
                            target = TargetSelector.GetSelectedTarget();
                        }
                        else
                        {
                            target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range)
                                .OrderByDescending(i => myRePriority.ResortDB(i.ChampionName))
                                .ThenBy(i => i.Health)
                                .FirstOrDefault();
                        }
                        if (target != null && target.IsValidTarget())
                        {
                            mySpellcast.Unit(target, R);
                        }
                        break;
                    case 1:
                        if (FlashSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready)
                        {                            
                            if (TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() &&
                                Vector3.Distance(Player.ServerPosition, TargetSelector.GetSelectedTarget().ServerPosition) <= R.Range + 425f)
                            {
                                target = TargetSelector.GetSelectedTarget();
                            }
                            else
                            {
                                target = EnemyList.Where(x => !x.InFountain() && x.IsVisible && 
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) > 425f && 
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range + 425f)
                                    .OrderByDescending(i => myRePriority.ResortDB(i.ChampionName))
                                    .ThenBy(i => i.Health)
                                    .FirstOrDefault();
                            }
                            if (target != null && target.IsValidTarget())
                            {
                                R.UpdateSourcePosition(Player.ServerPosition.Extend(target.ServerPosition, 425f));
                                Player.Spellbook.CastSpell(FlashSlot, Player.ServerPosition.Extend(target.ServerPosition, 425f));
                                mySpellcast.Unit(target, R);
                            }
                        }
                        break;
                }                
            }
        }

        private bool NextCastSummons
        {
            get { return Player.HasBuff("alzaharsummonvoidling"); }
        }
        private int VoidlingsTotal
        {
            get { return ObjectManager.Get<Obj_AI_Minion>().Count(minion => minion.IsValid && minion.IsAlly && minion.CharData.BaseSkinName.Contains("voidling")); }
        }

        private HitChance QHitChance
        {
            get
            {
                return GetQHitChance();
            }
        }
        private HitChance GetQHitChance()
        {
            switch (Root.Item("EC.Malzahar.QPredHitchance").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                default:
                    return HitChance.Medium;
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
                case myOrbwalker.OrbwalkingMode.Harass:
                    Harass();
                    break;
                case myOrbwalker.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                case myOrbwalker.OrbwalkingMode.Hybrid:
                    LaneClear();
                    Harass();
                    break;
                case myOrbwalker.OrbwalkingMode.JungleClear:
                    JungleClear();
                    break;
                case myOrbwalker.OrbwalkingMode.Custom:
                    Custom();
                    break;
            }            
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.R)
            {
                LastR = myUtility.TickCount;
                mySpellcast.Pause(2500 + Game.Ping);
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {                
                if ((spell.SData.Name.ToLower() == "alzaharnullzone") || (spell.SData.Name.ToLower() == "alzaharcallofthevoid") || (spell.SData.Name.ToLower() == "alzaharmaleficvisions"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (Player.IsChannelingImportantSpell() || myUtility.TickCount - LastR <= 0.5f)
            {
                args.Process = false;
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Root.Item("EC.Malzahar.Misc.Q").GetValue<bool>() && Q.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= Q.Range)
                {
                    if (myUtility.ImmuneToMagic(sender) || myUtility.ImmuneToCC(sender)) return;
                    mySpellcast.Circular(sender.ServerPosition, Q);
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Malzahar.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Malzahar.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Malzahar.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (R.Level > 0 && R.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                switch (Root.Item("EC.Malzahar.UseRType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                       if (Root.Item("EC.Malzahar.UseRDrawDistance").GetValue<bool>())
                        {
                            Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 7);
                        }
                        if (Root.Item("EC.Malzahar.UseRDrawTarget").GetValue<bool>())
                        {
                            var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range)
                                .OrderByDescending(i => myRePriority.ResortDB(i.ChampionName))
                                .ThenBy(i => i.Health)
                                .FirstOrDefault();
                            if (target != null && target.IsValidTarget())
                            {
                                Render.Circle.DrawCircle(target.Position, target.BoundingRadius, Color.Lime, 7);
                            }
                        }
                        break;
                    case 1:
                        if (FlashSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready)
                        {
                            if (Root.Item("EC.Malzahar.UseRDrawDistance").GetValue<bool>())
                            {
                                Render.Circle.DrawCircle(Player.Position, 425f, Color.Fuchsia, 7);
                                Render.Circle.DrawCircle(Player.Position, R.Range + 425f, Color.Fuchsia, 7);
                            }
                            if (Root.Item("EC.Malzahar.UseRDrawTarget").GetValue<bool>())
                            {
                                var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) > 425f &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range + 425f)
                                    .OrderByDescending(i => myRePriority.ResortDB(i.ChampionName))
                                    .ThenBy(i => i.Health)
                                    .FirstOrDefault();
                                if (target != null && target.IsValidTarget())
                                {
                                    Render.Circle.DrawCircle(target.Position, target.BoundingRadius, Color.Lime, 7);
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}
