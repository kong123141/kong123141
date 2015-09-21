using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Sejuani : PluginData
    {
        public Sejuani()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 650);            
            W = new Spell(SpellSlot.W, 350);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R, 1175);

            Q2 = new Spell(SpellSlot.Q, 650);
            
            Q.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed, true, SkillshotType.SkillshotLine);            
            R.SetSkillshot(250, 110, 1600, true, SkillshotType.SkillshotLine);

            Q2.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellList.Add(Q2);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Glacial Prison", "Custom");
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
                laneclear.AddItem(new MenuItem("QFarmType", "Q").SetValue(new StringList(new[] { "Slider Value", "Furthest" })));
                laneclear.AddItem(new MenuItem("QFarmValue", "Q More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("EFarmType", "E").SetValue(new StringList(new[] { "Any", "Most", "Only Siege" })));
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
                miscmenu.AddItem(new MenuItem("QRPredHitchance", "Q/R Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("UseQMisc", "Q Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseWMisc", "W Turrets").SetValue(false));
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
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
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
                    if (UseQ && Q.IsReady() && !Player.IsWindingUp && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range)
                    {
                        if (target.ServerPosition.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                        QPredict(target);
                    }
                    if (UseW && W.IsReady() && Orbwalking.InAutoAttackRange(target) && !Player.IsDashing())
                    {
                        if (!Player.IsWindingUp) W.Cast();
                    }
                    if (UseE && E.IsReady() && target.HasBuff("SejuaniFrost") && !Player.IsDashing() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < 1000)
                    {
                        E.Cast();
                    }
                    if (CastItems)
                    {
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= 450f)
                        {
                            myUtility.UseItems(1, target);
                        }
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
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical, false);
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var UseE = config.Item("UseEHarass").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady() && !Player.IsWindingUp && !target.UnderTurret(true) && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range)
                {
                    QPredict(target);
                }
                if (UseE && E.IsReady() && target.HasBuff("SejuaniFrost") && !Player.IsWindingUp && !Player.IsDashing() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < 1000)
                {
                    E.Cast();
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp && !Player.IsDashing())
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range).ToList();
                switch (config.Item("QFarmType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:                       
                        var QLine = Q2.GetLineFarmLocation(minionQ, Q.Width);
                        if (QLine.Position.IsValid() && !QLine.Position.To3D().UnderTurret(true))
                        {
                            if (QLine.MinionsHit > config.Item("QFarmValue").GetValue<Slider>().Value) Q2.Cast(QLine.Position);
                        }
                        break;
                    case 1:
                        var FurthestQ = minionQ.OrderByDescending(i => i.Distance(Player)).FirstOrDefault(x => !x.UnderTurret(true));
                        if (FurthestQ != null && FurthestQ.Position.IsValid() && !Orbwalking.InAutoAttackRange(FurthestQ))
                        {
                            Q2.Cast(FurthestQ.Position);
                        }
                        break;
                }
            }
            if (config.Item("UseWFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
            {
                W.Cast();
            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp && !Player.UnderTurret(true))
            {
                var minionE = MinionManager.GetMinions(Player.ServerPosition, E.Range).ToList();
                switch (config.Item("EFarmType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        //Any
                        var AnyMinionsE = minionE.Count(x => E.IsKillable(x) && x.HasBuff("sejuanifrost"));
                        if (AnyMinionsE > 0)
                        {
                            E.Cast();
                        }
                        break;
                    case 1:
                        //Most
                        var frostminionE = minionE.Count(x => x.HasBuff("sejuanifrost"));
                        if (frostminionE >= minionE.Count() * 1/2)
                        {
                            E.Cast();
                        }
                        break;
                    case 2:
                        //Siege
                        var siegeE = myUtility.GetLargeMinions(E.Range).Count(x => x.HasBuff("sejuanifrost") && E.IsKillable(x));
                        if (siegeE > 0)
                        {
                            E.Cast();
                        }
                        break;
                }
            }   
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q2.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myUtility.GetLargeMonsters(Q2.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (config.Item("UseQJFarm").GetValue<bool>() && Q2.IsReady() && Q2.IsInRange(mob))
                {
                    if (largemobs != null)
                    {
                        Q2.Cast(Player.ServerPosition.Extend(largemobs.ServerPosition, Q2.Range));
                    }
                    else
                    {
                        Q2.Cast(Player.ServerPosition.Extend(mob.ServerPosition, Q2.Range));
                    }
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !Player.IsDashing())
                {
                    var mobsE = mobs.Count(x => x.HasBuff("sejuanifrost") && E.IsInRange(x));
                    if (largemobs != null && largemobs.HasBuff("sejuanifrost") && E.IsKillable(largemobs))
                    {
                        E.Cast();
                    }
                    else if (mobsE > 0)
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
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                             Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range
                             ).OrderByDescending(i => i.CountEnemiesInRange(400)).FirstOrDefault();
                if (target != null && target.IsValidTarget())
                {
                    PredictionOutput pred = R.GetPrediction(target);
                    if (pred.CollisionObjects.Count == 0 && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= R.Range)
                    {
                        var test1 = Prediction.GetPrediction(target, R.Instance.SData.MissileSpeed).CastPosition;
                        float movement = target.MoveSpeed * 100 / 1000;
                        if (target.Distance(test1) > movement)
                        {
                            R.Cast(target.ServerPosition.Extend(Player.ServerPosition.Extend(test1, R.Instance.SData.MissileSpeed * target.MoveSpeed), R.Width));                            
                        }
                        else
                        {
                            if (pred.Hitchance >= QRHitChance) R.Cast(pred.CastPosition);
                        }
                    }   
                }
            }
        }

        private HitChance QRHitChance
        {
            get
            {
                return GetQRHitChance();
            }
        }
        private HitChance GetQRHitChance()
        {
            switch (config.Item("QRPredHitchance").GetValue<StringList>().SelectedIndex)
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

        private void QPredict(Obj_AI_Hero target)
        {
            PredictionOutput pred = Q.GetPrediction(target);
            if (pred.CollisionObjects.Count == 0 && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range)
            {
                var test1 = Prediction.GetPrediction(target, Q.Instance.SData.MissileSpeed).CastPosition;
                float movement = target.MoveSpeed * 100 / 1000;
                if (target.Distance(test1) > movement)
                {
                    Q.Cast(Player.ServerPosition.Extend(target.ServerPosition.Extend(test1, Q.Instance.SData.MissileSpeed * target.MoveSpeed), Q.Width));
                }
                else
                {
                    if (pred.Hitchance >= QRHitChance)
                    {
                        Q.Cast(Player.ServerPosition.Extend(pred.CastPosition, Q.Width));
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
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("UseQMisc").GetValue<bool>() && Q.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) < Q.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;
                    QPredict(gapcloser.Sender);
                }
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass)
                {
                    if (config.Item("UseWHarass").GetValue<bool>() &&
                        !Player.IsWindingUp &&
                        W.IsReady() &&
                        target.IsValidTarget()) W.Cast();
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear)
                {
                    if (target is Obj_AI_Turret && target.Team != Player.Team &&
                        config.Item("UseWMisc").GetValue<bool>() &&
                        !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                    {
                        W.Cast();
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
            if (R.Level > 0)
            {
                if (config.Item("UseRDrawDistance").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 7);
                }
                if (config.Item("UseRDrawTarget").GetValue<bool>())
                {
                    if (R.IsReady())
                    {
                        var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                        var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                     Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range
                                     ).OrderByDescending(i => i.CountEnemiesInRange(400)).FirstOrDefault();
                        if (target != null && target.IsValidTarget())
                        {
                            var num = EnemyList.Count(x => Vector3.Distance(target.ServerPosition, x.ServerPosition) <= 400);
                            Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.White, "Hits: " + num);
                            Vector3 pos;
                            PredictionOutput pred = R.GetPrediction(target);
                            if (pred.CollisionObjects.Count == 0 && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < R.Range)
                            {
                                var test1 = Prediction.GetPrediction(target, R.Instance.SData.MissileSpeed).CastPosition;
                                float movement = target.MoveSpeed * 100 / 1000;
                                if (target.Distance(test1) > movement)
                                {
                                    pos = target.ServerPosition.Extend(Player.ServerPosition.Extend(test1, R.Instance.SData.MissileSpeed * target.MoveSpeed), R.Width);
                                    Render.Circle.DrawCircle(pos, R.Width, Color.Lime, 7);
                                }
                                else
                                {
                                    if (pred.Hitchance >= QRHitChance)
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
}
