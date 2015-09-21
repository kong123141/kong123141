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
    class Annie : PluginData
    {
        public Annie()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 625);
            W = new Spell(SpellSlot.W, 625);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 600);

            Q.SetTargetted(Q.Instance.SData.SpellCastTime, 1400);
            W.SetSkillshot(W.Instance.SData.SpellCastTime, 50*(float)Math.PI / 180, float.MaxValue, false, SkillshotType.SkillshotCone, Player.ServerPosition);
            R.SetSkillshot(R.Instance.SData.SpellCastTime, 290, float.MaxValue, false, SkillshotType.SkillshotCircle);                    
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Summon: Tibbers ", "Custom");
            {
                custommenu.AddItem(new MenuItem("UseRKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("UseRType", "R").SetValue(new StringList(new[] { "Tibbers!", "Flash Tibbers!"  })));
                custommenu.AddItem(new MenuItem("UseRHitChecks", "Only if hits").SetValue(true));
                custommenu.AddItem(new MenuItem("UseRStun", "With Stun").SetValue(true));
                custommenu.AddItem(new MenuItem("UseRDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("UseRDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
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
                laneclear.AddItem(new MenuItem("WFarmValue", "W More Than").SetValue(new Slider(1, 1, 5)));
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
                miscmenu.AddItem(new MenuItem("WPredHitchance", "W Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("UseStunMisc", "Stun Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseStun2Misc", "Stun Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseEMisc", "E Shields").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawW", "W").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToMagic(target)) return;
                try
                {
                    if (TibbersSpawned) TibbersWalker(target); 
                    if (Pyromania)
                    {
                        if (myUtility.ImmuneToCC(target)) return;
                        if (UseW && W.IsReady())
                        {
                            if (target.CountEnemiesInRange(250) > 0)
                            {
                                W.CastIfWillHit(target, target.CountEnemiesInRange(250));
                            }
                            else
                            {
                                if (!Q.IsReady() || Q.Level <= 0)
                                {
                                    W.CastIfHitchanceEquals(target, WHitChance);
                                }
                            }

                        }
                        if (UseQ && Q.IsReady()) Q.CastOnUnit(target);
                    }
                    else
                    {
                        if (UseQ && Q.IsReady())
                        {
                            Q.CastOnUnit(target);
                        }
                        if (UseW && W.IsReady())
                        {
                            W.CastIfHitchanceEquals(target, WHitChance);
                        }
                        if (UseE && E.IsReady())
                        {
                            E.CastOnUnit(Player);
                        }
                    }
                }
                catch { }
            }           
        }
        private void Harass()
        {
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var UseW = config.Item("UseWHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                if (TibbersSpawned) TibbersWalker(target); 
                if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                if (Pyromania && !Player.IsWindingUp)
                {
                    if (myUtility.ImmuneToCC(target)) return;
                    if (Pyromania)
                    {
                        if (myUtility.ImmuneToCC(target)) return;
                        if (UseW && W.IsReady())
                        {
                            if (target.CountEnemiesInRange(250) > 0)
                            {
                                W.CastIfWillHit(target, target.CountEnemiesInRange(250));
                            }
                            else
                            {
                                if (!Q.IsReady() || Q.Level <= 0)
                                {
                                    W.CastIfHitchanceEquals(target, WHitChance);
                                }
                            }

                        }
                        if (UseQ && Q.IsReady()) Q.CastOnUnit(target);
                    }
                }
                else
                {
                    if (UseQ && Q.IsReady() && !Player.IsWindingUp)
                    {
                        Q.CastOnUnit(target);
                    }
                    if (UseW && W.IsReady() && !Player.IsWindingUp)
                    {
                        W.Cast(target.Position);
                    }
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (myUtility.TickCount - LastAA < Player.AttackCastDelay * 1000) return;
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp && myUtility.TickCount - LastW > myUtility.RandomDelay(500, 1000))
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (minionQ == null) return;
                var siegeQ = myUtility.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                if (siegeQ != null && siegeQ.IsValidTarget())
                {
                    Q.CastOnUnit(siegeQ);
                }
                else
                {
                    if (Pyromania) return;
                    var FurthestQ = minionQ.OrderByDescending(i => Vector3.Distance(Player.ServerPosition, i.ServerPosition)).FirstOrDefault(x => Q.IsKillable(x));
                    if (FurthestQ != null && FurthestQ.IsValidTarget())
                    {
                        if (myUtility.IsFacing(Player, FurthestQ.ServerPosition, 60)) Q.CastOnUnit(FurthestQ);
                    }
                }
            }
            if (config.Item("UseWFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp && myUtility.TickCount - LastQ > myUtility.RandomDelay(500, 1000))
            {
                if (Player.UnderTurret(true)) return;
                if (Pyromania) return;
                var minionW = MinionManager.GetMinions(Player.ServerPosition, W.Range);
                if (minionW == null) return;
                var siegeW = minionW.FirstOrDefault(x => x.BaseSkinName.Contains("Siege") && W.IsKillable(x));
                if (siegeW != null && siegeW.IsValidTarget())
                {
                    W.Cast(siegeW.ServerPosition);
                }
                else if (minionW.Count() > 1)
                {
                    foreach (var x in minionW)
                    {
                        if (MinionManager.GetMinions(x.ServerPosition, 275).Count() > config.Item("WFarmValue").GetValue<Slider>().Value)
                        {
                            if (x.IsValidTarget() && x.ServerPosition.IsValid() && myUtility.IsFacing(Player, x.ServerPosition)) W.Cast(x.ServerPosition);
                        }
                    }
                }
                else
                {                    
                    var SelectW = minionW.Where(x => W.IsKillable(x)).OrderByDescending(i => Vector3.Distance(Player.ServerPosition, i.ServerPosition)).FirstOrDefault();
                    if (SelectW != null && SelectW.IsValidTarget())
                    {
                        if (myUtility.IsFacing(Player, SelectW.ServerPosition, 60)) W.Cast(SelectW.ServerPosition);
                    }
                }
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
                if (myUtility.TickCount - LastAA < myUtility.RandomDelay(300, 400)) return;
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && Q.IsInRange(mob) && !Player.IsWindingUp && myUtility.TickCount - LastW > myUtility.RandomDelay(300, 400))
                {
                    if (largemobs != null)
                    {
                        Q.CastOnUnit(largemobs);
                    }
                    else
                    {
                        Q.CastOnUnit(mob);
                    }
                }
                if (config.Item("UseWJFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp && myUtility.TickCount - LastQ > myUtility.RandomDelay(300,400))
                {
                    if (largemobs != null)
                    {
                        W.Cast(largemobs.ServerPosition);
                    }
                    else
                    {
                        if (myUtility.IsFacing(Player, mob.ServerPosition, 75)) W.Cast(mob.ServerPosition);
                    }
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null)
                    {
                        E.Cast();
                    }
                }
            }
        }
        private void Custom()
        {
            if (R.IsReady())
            {
                if (config.Item("UseRStun").GetValue<bool>() && !Pyromania) return;
                Obj_AI_Hero target; 
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                switch (config.Item("UseRType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        if (TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget())
                        {
                            target = TargetSelector.GetSelectedTarget();
                        }
                        else 
                        { 
                            target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                     Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range)
                                     .OrderByDescending(i => i.CountEnemiesInRange(290))
                                     .FirstOrDefault();
                        }
                        if (target != null && target.IsValidTarget())
                        {
                            PredictionOutput pred = R.GetPrediction(target);
                            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= R.Range)
                            {
                                if (pred.Hitchance >= HitChance.High)
                                {
                                    R.Cast(target.Position);
                                }
                            }
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
                                    .OrderByDescending(i => i.CountEnemiesInRange(290))
                                    .FirstOrDefault();
                            }
                            if (target != null && target.IsValidTarget())
                            {
                                R.UpdateSourcePosition(Player.ServerPosition.Extend(target.ServerPosition, 425f));
                                Player.Spellbook.CastSpell(FlashSlot, Player.ServerPosition.Extend(target.ServerPosition, 425f));
                                R.Cast(target.Position);
                            }
                        }
                        break;
                }
            }
        }

        private int LastOrder;
        private void TibbersWalker(Obj_AI_Base t)
        {
            var Tibbers = Player.Pet as Obj_AI_Base;
            if (Tibbers != null && Tibbers.IsValid && myUtility.TickCount - LastOrder > 1000)
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable);
                var target = t != null && t.IsValidTarget() ? t : EnemyList.Where(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= 3000).OrderByDescending(z => myRePriority.ResortDB(z.ChampionName)).ThenBy(i => i.Health).FirstOrDefault();
                switch (myOrbwalker.ActiveMode)
                {
                    case myOrbwalker.OrbwalkingMode.Combo:
                        if (target != null && target.IsValidTarget())
                        {
                            Tibbers.IssueOrder(GameObjectOrder.AutoAttackPet, target);
                            R.Cast(target);
                        }
                        break;
                    case myOrbwalker.OrbwalkingMode.Harass:
                        if (target != null && target.IsValidTarget())
                        {
                            Tibbers.IssueOrder(GameObjectOrder.AutoAttackPet, target);
                            R.Cast(target);
                        }
                        break;
                    case myOrbwalker.OrbwalkingMode.LaneClear:
                        var minionT = MinionManager.GetMinions(Tibbers.ServerPosition, 3000).OrderBy(x => Vector3.Distance(Tibbers.ServerPosition, x.ServerPosition)).FirstOrDefault();
                        if (minionT != null && minionT.IsValidTarget())
                        {
                            Tibbers.IssueOrder(GameObjectOrder.AutoAttackPet, minionT);
                            R.Cast(minionT);
                        }
                        break;
                    case myOrbwalker.OrbwalkingMode.JungleClear:
                        var mobT = MinionManager.GetMinions(Tibbers.ServerPosition, 3000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).OrderBy(x => Vector3.Distance(Tibbers.ServerPosition, x.ServerPosition)).FirstOrDefault();
                        if (mobT != null && mobT.IsValidTarget())
                        {
                            Tibbers.IssueOrder(GameObjectOrder.AutoAttackPet, mobT);
                            R.Cast(mobT);
                        }
                        break;
                }
                LastOrder = myUtility.TickCount;
            }
        }

        private bool Pyromania
        {
            get
            {
                return Player.HasBuff("pyromania_particle");                
            }
        }
        private int PyromaniaCount
        {
            get
            {
                return Player.Buffs.Count(x => x.Name == "pyromania");
            }
        }  
        private HitChance WHitChance
        {
            get
            {
                return GetWHitChance();
            }
        }
        private HitChance GetWHitChance()
        {
            switch (config.Item("WPredHitchance").GetValue<StringList>().SelectedIndex)
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
        private int LastAA;
        private int LastQ;
        private int LastW;
        
        private bool TibbersSpawned
        {
            get { return ObjectManager.Get<Obj_AI_Minion>().Any(minion => minion.IsValid && minion.IsAlly && minion.BaseSkinName.Contains("annietibbers")); }
        }
        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                myUtility.Reset();
                return;
            }
            if (TibbersSpawned) TibbersWalker(null); 
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
            if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Custom && (config.Item("UseRHitChecks").GetValue<bool>()))
            {
                if (args.Slot == SpellSlot.R && myUtility.SpellHits(R).Item1 == 0)
                {
                    args.Process = false;
                }
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if (spell.SData.Name.ToLower().Contains("basicattack"))
                {
                    LastAA = myUtility.TickCount;
                }
                if (spell.SData.Name.ToLower() == "disintergrate")
                {
                    LastQ = myUtility.TickCount;
                }
                if (spell.SData.Name.ToLower() == "incinerate")
                {
                    LastW = myUtility.TickCount;
                }                
            }
            if (!unit.IsMe && unit.IsEnemy)
            {
                if (spell.Target == null || !spell.Target.IsValid || !spell.Target.IsMe)
                {
                    return;
                }
                if (spell.SData.IsAutoAttack() && spell.Target.IsMe && E.IsReady())
                {
                    if (unit.IsChampion(unit.BaseSkinName) && unit.IsValid<Obj_AI_Hero>())
                    {   
                        if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                        {
                            if (config.Item("UseECombo").GetValue<bool>())
                            {
                                Utility.DelayAction.Add(myUtility.RandomDelay(0, 200), () => E.CastOnUnit(Player));
                            }
                        }
                        if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass)
                        {
                            if (config.Item("UseEHarass").GetValue<bool>())
                            {
                                Utility.DelayAction.Add(myUtility.RandomDelay(0, 200), () => E.CastOnUnit(Player));
                            }
                        }
                        if (config.Item("UseEMisc").GetValue<bool>())
                        {
                            Utility.DelayAction.Add(myUtility.RandomDelay(0, 200), () => E.CastOnUnit(Player));
                        }
                    }
                    else if (unit.IsMinion && unit.IsValid<Obj_AI_Minion>())
                    {
                        if (config.Item("UseEFarm").GetValue<bool>() &&
                            myUtility.PlayerManaPercentage > config.Item("FarmMana").GetValue<Slider>().Value)
                        {
                            E.CastOnUnit(Player);
                        }
                    }
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("UseStunMisc").GetValue<bool>())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) < 625)
                {
                    if (myUtility.ImmuneToMagic(sender) || myUtility.ImmuneToCC(sender)) return;
                    if (Pyromania)
                    {
                        if (Q.IsReady()) Q.CastOnUnit(sender);
                        else if (W.IsReady()) W.Cast(sender.ServerPosition);
                    }
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("UseStun2Misc").GetValue<bool>())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) < 625)
                {
                    if (myUtility.ImmuneToMagic(gapcloser.Sender) || myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    if (Pyromania)
                    {
                        if (Q.IsReady()) Q.CastOnUnit(gapcloser.Sender);
                        else if (W.IsReady()) W.Cast(gapcloser.Sender.ServerPosition);
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
            
            if (R.Level > 0 && R.IsReady())
            {
                if (config.Item("UseRStun").GetValue<bool>() && !Pyromania) return;
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                switch (config.Item("UseRType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        if (config.Item("UseRDrawDistance").GetValue<bool>())
                        {
                            Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 7);
                        }
                        if (config.Item("UseRDrawTarget").GetValue<bool>())
                        {
                            var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range)
                                    .OrderByDescending(i => i.CountEnemiesInRange(290))
                                    .FirstOrDefault();
                            if (target != null && target.IsValidTarget())
                            {
                                var num = target.CountEnemiesInRange(290);
                                Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.White, "Hits: " + num);
                                PredictionOutput pred = R.GetPrediction(target);
                                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= R.Range)
                                {
                                    if (pred.Hitchance >= HitChance.High)
                                    {
                                        Render.Circle.DrawCircle(target.Position, target.BoundingRadius, Color.Lime, 7);
                                    }
                                }
                            }
                        }
                        break;
                    case 1:
                        if (FlashSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready)
                        {
                            if (config.Item("UseRDrawDistance").GetValue<bool>())
                            {
                                Render.Circle.DrawCircle(Player.Position, 425f, Color.Fuchsia, 7);
                                Render.Circle.DrawCircle(Player.Position, R.Range + 425f, Color.Fuchsia, 7);
                            }
                            if (config.Item("UseRDrawTarget").GetValue<bool>())
                            {
                                var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) > 425f &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range + 425f)
                                    .OrderByDescending(i => i.CountEnemiesInRange(290))
                                    .FirstOrDefault();
                                if (target != null && target.IsValidTarget())
                                {
                                    var num = target.CountEnemiesInRange(290);
                                    Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.White, "Hits: " + num);
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
