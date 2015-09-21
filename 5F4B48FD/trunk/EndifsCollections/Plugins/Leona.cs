using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Leona : PluginData
    {
        public Leona()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 1200);

            E.SetSkillshot(0.25f, 100f, 2000f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1f, 300f, float.MaxValue, false, SkillshotType.SkillshotCircle);
      
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Solar Flare", "Custom");
            {
                custommenu.AddItem(new MenuItem("UseRKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("UseRHitChecks", "Only if hits").SetValue(true));
                custommenu.AddItem(new MenuItem("UseRDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("UseRDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
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
                laneclear.AddItem(new MenuItem("WFarmValue", "W More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("EFarmValue", "E More Than").SetValue(new Slider(1, 1, 5)));
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
                miscmenu.AddItem(new MenuItem("ERPredHitchance", "E/R Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("UseQMisc", "Q Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseEMisc", "E Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseWMisc", "W Shields").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawW", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawE", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var CastItems = config.Item("UseItemCombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToPhysical(target)) return;
                
                if (CastItems) { myUtility.UseItems(0, target); }
                try
                {
                    if (UseW && W.IsReady())
                    {
                        if (myUtility.ImmuneToCC(target) || myUtility.ImmuneToMagic(target)) return;
                        if (Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) < 275f)
                        {
                            W.Cast();
                        }
                    }
                    if (UseE && E.IsReady())
                    {
                        if (target.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                        if (myUtility.ImmuneToCC(target) || myUtility.ImmuneToMagic(target)) return;
                        EPredict(target);
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
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical, false);            
            var UseW = config.Item("UseWHarass").GetValue<bool>();
            var UseE = config.Item("UseEHarass").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (UseW && W.IsReady())
                {
                    if (Player.UnderTurret(true)) return;
                    if (Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) < 275f)
                    {
                        W.Cast();
                    }
                }
                if (UseE && E.IsReady())
                {
                    if (target.UnderTurret(true)) return;
                    EPredict(target);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (config.Item("UseWFarm").GetValue<bool>())
            {
                var minionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range).Count();
                if (minionsW > config.Item("WFarmValue").GetValue<Slider>().Value) W.Cast();
            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady())
            {
                var minionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                var ELine = E.GetLineFarmLocation(minionsE);
                if (ELine.MinionsHit > config.Item("EFarmValue").GetValue<Slider>().Value && !Player.IsWindingUp && !myOrbwalker.IsWaiting())
                {
                    if (Player.UnderTurret(true)) return;
                    var target = TargetSelector.GetTarget(Vector3.Distance(ELine.Position.To3D(), Player.ServerPosition) + E.Width, TargetSelector.DamageType.Magical);
                    if (target == null)
                    {
                        if (myUtility.IsFacing(Player, ELine.Position.To3D())) E.Cast(ELine.Position);
                    }
                }   
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myUtility.GetLargeMonsters(E.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null && !Player.IsWindingUp)
            {
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady())
                {
                    if (largemobs != null && Orbwalking.InAutoAttackRange(largemobs))
                    {
                        Q.Cast();
                    }
                }
                if (config.Item("UseWJFarm").GetValue<bool>() && W.IsReady())
                {
                    if (largemobs != null && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < W.Width)
                    {
                        W.Cast(largemobs);
                    }
                    else if (Vector3.Distance(Player.ServerPosition, mob.ServerPosition) < W.Width)
                    {
                        W.Cast(mob);
                    }
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady())
                {
                    if (largemobs != null && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < E.Range)
                    {
                        E.Cast(largemobs.ServerPosition);
                    }
                }
            }            
        }
        private void Custom()
        {
            if (R.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                             Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range
                             ).OrderByDescending(i => i.CountEnemiesInRange(R.Width)).FirstOrDefault();
                if (target != null && target.IsValidTarget())
                {
                    PredictionOutput pred = R.GetPrediction(target);
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < R.Range)
                    {
                        var test1 = Prediction.GetPrediction(target, R.Instance.SData.MissileSpeed).CastPosition;
                        float movement = target.MoveSpeed * 100 / 1000;
                        if (target.Distance(test1) > movement)
                        {
                            R.Cast(target.ServerPosition.Extend(Player.ServerPosition.Extend(test1, 0.625f * target.MoveSpeed), R.Width / 2));
                        }
                        else
                        {
                            if (pred.Hitchance >= ERHitChance) R.Cast(pred.CastPosition);
                        }
                    }
                }
            }
        }

        private HitChance ERHitChance
        {
            get
            {
                return GetERHitChance();
            }
        }
        private HitChance GetERHitChance()
        {
            switch (config.Item("ERPredHitchance").GetValue<StringList>().SelectedIndex)
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
        private void EPredict(Obj_AI_Hero target)
        {
            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < E.Range)
            {
                PredictionOutput pred = E.GetPrediction(target);
                if (pred.CollisionObjects.Count == 0)
                {
                    var test1 = Prediction.GetPrediction(target, E.Instance.SData.SpellCastTime).CastPosition;
                    float movement = target.MoveSpeed * 100 / 1000;
                    if (target.Distance(test1) > movement) E.Cast(target.ServerPosition.Extend(test1, E.Instance.SData.MissileSpeed * target.MoveSpeed));
                    else
                    {
                        if (pred.Hitchance >= ERHitChance) E.Cast(pred.CastPosition);
                    }
                }
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
                if (spell.SData.Name.ToLower() == "leonashieldofdaybreak")
                {
                    myOrbwalker.ResetAutoAttackTimer();
                }
            }
            if (!unit.IsMe && unit.IsEnemy && unit.IsValid<Obj_AI_Hero>())
            {
                if (spell.Target == null || !spell.Target.IsValid || !spell.Target.IsMe)
                {
                    return;
                }
                if (spell.Target.IsMe && W.IsReady())
                {
                    if (unit.IsChampion(unit.BaseSkinName))
                    {
                        if (config.Item("UseWMisc").GetValue<bool>())
                        {
                            Utility.DelayAction.Add(myUtility.RandomDelay(0, 200), () =>  W.Cast());
                        }
                    }
                }
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe && Q.IsReady())
            {
                if (target.Name.ToLower().Contains("ward"))
                {
                    Q.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }                
                if (target is Obj_AI_Minion && target.IsValidTarget() && !Player.IsWindingUp)
                {
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear && config.Item("UseQFarm").GetValue<bool>())
                    {
                        if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
                        if (Q.IsKillable((Obj_AI_Minion)target) ||
                            (Player.GetAutoAttackDamage((Obj_AI_Minion)target) + Player.GetSpellDamage((Obj_AI_Minion)target, SpellSlot.Q)) >= target.Health)
                        {
                            Q.Cast();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                        }
                    }
                }
                if (target is Obj_AI_Hero && target.IsValidTarget() && !Player.IsWindingUp)
                {
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && config.Item("UseQCombo").GetValue<bool>() )
                    {
                        if (myUtility.ImmuneToCC((Obj_AI_Hero)target) || myUtility.ImmuneToMagic((Obj_AI_Hero)target)) return;
                        Q.Cast();
                        Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                    }
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass && config.Item("UseQHarass").GetValue<bool>())
                    {
                        Q.Cast();
                        Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                    }
                }                
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
            if (config.Item("UseEMisc").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) < E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    EPredict(gapcloser.Sender);
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("DrawW").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Color.White);
            }
            if (config.Item("DrawE").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.White);
            }
            if (R.Level > 0 && R.IsReady())
            {
                if (config.Item("UseRDrawDistance").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 7);
                }
                if (config.Item("UseRDrawTarget").GetValue<bool>())
                {
                    var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                    var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                 Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range
                                 ).OrderByDescending(i => i.CountEnemiesInRange(R.Width)).FirstOrDefault();
                    if (target != null && target.IsValidTarget())
                    {
                        Vector3 pos;
                        PredictionOutput pred = R.GetPrediction(target);
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < R.Range)
                        {
                            var num = target.CountEnemiesInRange(R.Width);
                            Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.White, "Hits: " + num);
                            var test1 = Prediction.GetPrediction(target, R.Instance.SData.MissileSpeed).CastPosition;
                            float movement = target.MoveSpeed * 100 / 1000;
                            if (target.Distance(test1) > movement)
                            {
                                pos = target.ServerPosition.Extend(Player.ServerPosition.Extend(test1, R.Instance.SData.SpellCastTime * target.MoveSpeed), R.Width / 2);
                                Render.Circle.DrawCircle(pos, R.Width, Color.Lime, 7);
                            }
                            else
                            {
                                if (pred.Hitchance >= ERHitChance)
                                {
                                    Render.Circle.DrawCircle(pred.CastPosition, R.Width, Color.Lime, 7);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
