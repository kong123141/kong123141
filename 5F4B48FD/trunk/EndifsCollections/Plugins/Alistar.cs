using System;
using System.Linq;
using EndifsCollections.Controller;
using EndifsCollections.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Alistar : PluginData
    {
        public Alistar()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 365);
            W = new Spell(SpellSlot.W, 650);
            E = new Spell(SpellSlot.E, 575);
            R = new Spell(SpellSlot.R);

            W.SetTargetted(0.5f, float.MaxValue);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Flash Plays", "Custom");
            {
                custommenu.AddItem(new MenuItem("UseFIKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("UseFIType", "Flash").SetValue(new StringList(new[] { "Pulverize", "Headbutt"  })));
                custommenu.AddItem(new MenuItem("UseFIDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("UseFIDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("TurretDive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("UseItemCombo", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclear = new Menu("Farm", "Farm");
            {
                laneclear.AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
                laneclear.AddItem(new MenuItem("UseWFarm", "Use W").SetValue(true));
                laneclear.AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
                laneclear.AddItem(new MenuItem("QFarmValue", "Q More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("FarmMana", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclear);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("UseQMisc", "Q Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseWMisc", "W Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseQ2Misc", "Q Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseW2Misc", "W Interrupts").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawW", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawE", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var UseR = config.Item("UseRCombo").GetValue<bool>();
            var CastItems = config.Item("UseItemCombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToPhysical(target)) return;                               
                if (CastItems) { myUtility.UseItems(0, target); }
                try
                {
                    if (UseQ && UseW && Player.Mana > (Q.Instance.ManaCost + W.Instance.ManaCost))
                    {
                        if (target.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                        if (myUtility.ImmuneToCC(target) || myUtility.ImmuneToMagic(target)) return;
                        if (W.IsReady())
                        {
                            W.Cast(target);
                        }
                        if (Q.IsReady() && myUtility.TickCount - DashTime < (700 + Game.Ping))
                        {
                            if (Vector3.Distance(target.ServerPosition, DashEnd) <= Q.Range) Q.Cast();
                        }
                    }
                    else
                    {
                        if (UseQ && Q.IsReady())
                        {
                            if (myUtility.ImmuneToCC(target) || myUtility.ImmuneToMagic(target)) return;
                            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= Q.Range * 2/3) Q.Cast();
                        }
                        if (UseW && W.IsReady())
                        {
                            if (target.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                            if (myUtility.ImmuneToCC(target) || myUtility.ImmuneToMagic(target)) return;
                            W.Cast(target);
                        }
                        if (UseE && E.IsReady())
                        {
                            if (myUtility.PlayerHealthPercentage < 100 || Player.CountAlliesInRange(E.Range) > 0)
                            {
                                E.Cast();
                            }
                        }
                    }
                    if (UseR && R.IsReady())
                    {
                        if (myUtility.MovementImpaired(Player)) Utility.DelayAction.Add(500, () => R.Cast());
                    }
                    if (CastItems)
                    {
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < 500f)
                        {
                            myUtility.UseItems(3, null);
                        }
                    }
                }
                catch { }
            }
            
        }
        private void Harass()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical, false);            
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var UseW = config.Item("UseWHarass").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady())
                {
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range) Q.Cast();
                }
                if (UseW && W.IsReady())
                {
                    W.Cast(target);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                if (Player.UnderTurret(true)) return;
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (minionQ == null) return;
                var qpred = Q.GetCircularFarmLocation(minionQ);
                if (qpred.MinionsHit > config.Item("QFarmValue").GetValue<Slider>().Value)
                {
                    if (Vector3.Distance(Player.ServerPosition,qpred.Position.To3D()) <= Player.AttackRange) Q.Cast();
                }
            }
            if (config.Item("UseWFarm").GetValue<bool>() && W.IsReady())
            {
                var siegeW = MinionManager.GetMinions(Player.ServerPosition, W.Range).FirstOrDefault(x => x.BaseSkinName.Contains("Siege") && Player.GetAutoAttackDamage(x) > x.Health && W.IsKillable(x));
                if (siegeW != null && siegeW.IsValidTarget())
                {
                    W.Cast(siegeW);
                }                
            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady())
            {                
                var turret = ObjectManager.Get<Obj_AI_Turret>()
                            .Where(t => t.IsEnemy)
                            .OrderBy(t => t.Distance(Player.Position))
                            .FirstOrDefault();
                if (turret != null && turret.IsValid)
                {
                    var m = turret.Target as Obj_AI_Minion;
                    if (m != null && m.IsValidTarget() && m.IsAlly)
                    {
                        if (Vector3.Distance(m.ServerPosition, Player.ServerPosition) < E.Range) E.Cast();
                    }
                }
                var minionE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (minionE == null) return;
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myUtility.GetLargeMonsters(Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady())
                {
                    if (largemobs != null && largemobs.IsValidTarget() && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < Q.Range)
                    {
                        Q.Cast();
                    }
                }
                if (config.Item("UseWJFarm").GetValue<bool>() && W.IsReady())
                {
                    if (largemobs != null && largemobs.IsValidTarget() && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) <= W.Range)
                    {
                        if (W.IsKillable(largemobs)) W.Cast(largemobs);
                    }
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady())
                {
                    if (mobs.Count() > 1 || myUtility.PlayerHealthPercentage < 75)
                    {
                        E.Cast();
                    }
                }
            }            
        }
        private void Custom()
        {
            if (FlashSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready)
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                switch (config.Item("UseFIType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        if (Q.IsReady())
                        {                            
                            var FP = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                Vector3.Distance(Player.ServerPosition, x.ServerPosition) > 425f &&
                                Vector3.Distance(Player.ServerPosition, x.ServerPosition) < Q.Range + 425f
                                ).OrderByDescending(i => i.CountEnemiesInRange(365)).FirstOrDefault();
                            if (FP == null) return;
                            Q.UpdateSourcePosition(Player.ServerPosition.Extend(FP.ServerPosition, 425f));
                            Player.Spellbook.CastSpell(FlashSlot, Player.ServerPosition.Extend(FP.ServerPosition, 425f));
                            Q.Cast(FP.ServerPosition);
                        }
                        break;
                    case 1:
                        if (W.IsReady())
                        {                           
                            var FH = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                Vector3.Distance(Player.ServerPosition, x.ServerPosition) < 425f - x.BoundingRadius - Player.BoundingRadius
                                ).OrderByDescending(i => myRePriority.ResortDB(i.ChampionName)).ThenBy(i => i.Health).FirstOrDefault();
                            if (FH == null) return;
                            W.UpdateSourcePosition(Player.ServerPosition.Extend(FH.ServerPosition, 425f));
                            Player.Spellbook.CastSpell(FlashSlot, Player.ServerPosition.Extend(FH.ServerPosition, 425f));
                            W.Cast(FH);
                        }
                        break;
                }
            }
        }

        private Vector3 DashEnd;
        private int? DashTime;

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
        protected override void OnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            if (sender.IsMe)
            {
                DashEnd = args.EndPos.To3D();
                DashTime = myUtility.TickCount;
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("UseQMisc").GetValue<bool>() && Q.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) < Q.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Q.Cast();
                }
            }
            if (config.Item("UseWMisc").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) <= W.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myUtility.RandomDelay(0, 200), () => W.Cast(gapcloser.Sender));    
                    //W.Cast(gapcloser.Sender);
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (sender.IsEnemy)
            {
                if (myUtility.ImmuneToMagic(sender) || myUtility.ImmuneToCC(sender)) return;
                if (config.Item("UseQ2Misc").GetValue<bool>() && Q.IsReady())
                {
                    if (Vector3.Distance(Player.ServerPosition, sender.ServerPosition) < Q.Range)
                    {
                        Utility.DelayAction.Add(myUtility.RandomDelay(0, 200), () => Q.Cast());
                        
                    }
                }
                if (config.Item("UseW2Misc").GetValue<bool>() && W.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.High)
                {
                    if (Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= W.Range)
                    {
                        Utility.DelayAction.Add(myUtility.RandomDelay(0, 200), () => W.Cast(sender));                        
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("DrawQ").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.White);
            }
            if (config.Item("DrawW").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Color.White);
            }
            if (config.Item("DrawE").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.White);
            }
            if (FlashSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready)
            {
                switch (config.Item("UseFIType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        if (Q.IsReady())
                        {
                            if (config.Item("UseFIDrawDistance").GetValue<bool>())
                            {
                                Render.Circle.DrawCircle(Player.Position, 425f, Color.Fuchsia, 7);
                                Render.Circle.DrawCircle(Player.Position, Q.Range + 425f, Color.Fuchsia, 7);
                            }
                            if (config.Item("UseFIDrawTarget").GetValue<bool>())
                            {
                                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                                var FP = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) > 425f &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) < Q.Range + 425f
                                    ).OrderByDescending(i => i.CountEnemiesInRange(365)).FirstOrDefault();
                                if (FP != null && FP.IsValidTarget())
                                {
                                    var num = FP.CountEnemiesInRange(365);
                                    Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.White, "Hits: " + num);
                                    Render.Circle.DrawCircle(FP.Position, FP.BoundingRadius, Color.Lime, 7);
                                }
                            }
                        }
                        break;
                    case 1:
                        if (W.IsReady())
                        {
                            if (config.Item("UseFIDrawDistance").GetValue<bool>())
                            {
                                Render.Circle.DrawCircle(Player.Position, 400f, Color.Fuchsia, 7);
                            }
                            if (config.Item("UseFIDrawTarget").GetValue<bool>())
                            {
                                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                                var FH = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) < 425f - x.BoundingRadius - Player.BoundingRadius
                                    ).OrderByDescending(i => myRePriority.ResortDB(i.ChampionName)).ThenBy(i => i.Health).FirstOrDefault();
                                if (FH != null && FH.IsValidTarget())
                                {
                                    Render.Circle.DrawCircle(FH.Position, FH.BoundingRadius, Color.Lime, 7);
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}
