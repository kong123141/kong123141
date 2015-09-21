using System;
using System.Linq;
using EndifsCollections.Controller;
using EndifsCollections.SummonerSpells;
using EndifsCollections.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Vi : PluginData
    {
        public Vi()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 860);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 800);

            Q2 = new Spell(SpellSlot.Q, 860);
            E2 = new Spell(SpellSlot.E, 600);

            Q.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed, true, SkillshotType.SkillshotLine);            
            Q.SetCharged("ViQ", "ViQ", 100, 860, 1f);            
            E.SetSkillshot(0.25f, 15f * 2 * (float)Math.PI / 180, 2000f, false, SkillshotType.SkillshotCone);
            R.SetTargetted(0.15f, 1500f);

            Q2.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotLine);
            Q2.SetCharged("ViQ", "ViQ", 100, 860, 1f);
            E2.SetSkillshot(0.25f, 15f * 2 * (float)Math.PI / 180, 2000f, false, SkillshotType.SkillshotCone);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellList.Add(Q2);            
            SpellList.Add(E2);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Assault and Battery", "Custom");
            {
                custommenu.AddItem(new MenuItem("UseRKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("UseRType", "R").SetValue(new StringList(new[] { "Less Hit", "R Killable", "YOLO", "Furthest", "Lowest HP" })));
                custommenu.AddItem(new MenuItem("UseRTypeLessHit", "Hit <").SetValue(new Slider(4, 1, 10)));
                custommenu.AddItem(new MenuItem("UseRDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("UseRDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("UseQComboMax", "Q Max charge").SetValue(true));
                combomenu.AddItem(new MenuItem("TurretDive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("UseItemCombo", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclear = new Menu("Farm", "Farm");
            {
                laneclear.AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
                laneclear.AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
                laneclear.AddItem(new MenuItem("QFarmType", "Q").SetValue(new StringList(new[] { "Any (Slider Value)", "Furthest" })));
                laneclear.AddItem(new MenuItem("QFarmValue", "(Any) Q More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("FarmMana", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclear);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("QPredHitchance", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("UseQMisc", "Q Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseQ2Misc", "Q Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseEMisc", "E Turrets").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawE", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(Q.Range + 425, TargetSelector.DamageType.Physical);          
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var CastItems = config.Item("UseItemCombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToPhysical(target)) return;                
                if (CastItems) { myUtility.UseItems(0, target); }
                if (mySmiter.CanSmiteChampions(target)) mySmiter.Smites(target);
                try
                {
                    if (UseQ)
                    {
                        if (target.ServerPosition.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                        if (Q.IsReady() && myUtility.IsFacing(Player, target.ServerPosition, 90))
                        {
                            Q.StartCharging();
                        }
                        PredictionOutput pred = Q.GetPrediction(target);
                        if (pred.CollisionObjects.Count == 0)
                        {
                            var test1 = Prediction.GetPrediction(target, Q.Instance.SData.MissileSpeed).CastPosition;
                            float movement = target.MoveSpeed * 100 / 1000;
                            if (Q.IsCharging)
                            {
                                if (target.Distance(test1) > movement)
                                {
                                    var pos = Player.ServerPosition.Extend(target.ServerPosition.Extend(test1, Q.Instance.SData.MissileSpeed * target.MoveSpeed), Q.Range);
                                    if (
                                        (config.Item("UseQComboMax").GetValue<bool>() && 
                                        (Q.Range >= Q.ChargedMaxRange && Vector3.Distance(Player.ServerPosition, pos) <= Q.ChargedMaxRange)) ||
                                        (!config.Item("UseQComboMax").GetValue<bool>() && 
                                        ((Q.Range >= Q.ChargedMaxRange && Vector3.Distance(Player.ServerPosition, pos) <= Q.Range) || Q.Range >= Vector3.Distance(Player.ServerPosition, pos))))
                                        Q.Cast(pos);
                                }
                                else
                                {
                                    if (pred.Hitchance >= QHitChance)
                                    {
                                        if ((config.Item("UseQComboMax").GetValue<bool>() && 
                                            (Q.Range >= Q.ChargedMaxRange && Vector3.Distance(Player.ServerPosition, pred.CastPosition) <= Q.ChargedMaxRange)) ||
                                            (!config.Item("UseQComboMax").GetValue<bool>() && 
                                            ((Q.Range >= Q.ChargedMaxRange && Vector3.Distance(Player.ServerPosition, pred.CastPosition) <= Q.ChargedMaxRange) || Q.Range >= Vector3.Distance(Player.ServerPosition, pred.CastPosition))))
                                            Q.Cast(Player.ServerPosition.Extend(pred.CastPosition, Q.Range));
                                    }
                                }
                            }                            
                        }
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
            if (target.IsValidTarget())
            {
                if (UseQ)
                {
                    if (target.ServerPosition.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                    if (Q.IsReady())
                    {
                        Q.StartCharging();
                    }
                    PredictionOutput pred = Q.GetPrediction(target);
                    if (pred.CollisionObjects.Count == 0)
                    {
                        var test1 = Prediction.GetPrediction(target, Q.Instance.SData.MissileSpeed).CastPosition;
                        float movement = target.MoveSpeed * 100 / 1000;
                        if (Q.IsCharging)
                        {
                            if (target.Distance(test1) > movement)
                            {
                                var pos = Player.ServerPosition.Extend(target.ServerPosition.Extend(test1, Q.Instance.SData.MissileSpeed * target.MoveSpeed), Q.Range);
                                if (Q.Range >= Q.ChargedMaxRange || Q.Range >= Vector3.Distance(Player.ServerPosition, pos))
                                    Q.Cast(pos);
                            }
                            else
                            {
                                if (pred.Hitchance >= QHitChance)
                                {
                                    if (Q.Range >= Q.ChargedMaxRange || Q.Range >= Vector3.Distance(Player.ServerPosition, pred.CastPosition))
                                        Q.Cast(Player.ServerPosition.Extend(pred.CastPosition, Q.Range));
                                }
                            }
                        }
                    }
                }              
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (config.Item("UseQFarm").GetValue<bool>()  && !Player.IsWindingUp && !Player.IsDashing())
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range).ToList();
                switch (config.Item("QFarmType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        var QLine = Q2.GetLineFarmLocation(minionQ);
                        if (QLine.Position.IsValid() && !QLine.Position.To3D().UnderTurret(true))
                        {
                            if (Q.IsReady())
                            {
                                Q.StartCharging();
                            }
                            if (Q.IsCharging && QLine.MinionsHit > config.Item("QFarmValue").GetValue<Slider>().Value)
                            {
                                if (Q.Range >= Q.ChargedMaxRange || Q.Range >= Vector3.Distance(Player.ServerPosition,QLine.Position.To3D()) + (Player.AttackRange * 2/3))
                                {
                                    if (QLine.Position.IsValid()) Q2.Cast(QLine.Position);
                                }
                            }
                        }
                        break;
                    case 1:
                        var FurthestQ = minionQ.OrderByDescending(i => i.Distance(Player)).FirstOrDefault(x => !x.UnderTurret(true));
                        if (FurthestQ != null && FurthestQ.Position.IsValid() && !Orbwalking.InAutoAttackRange(FurthestQ))
                        {
                            if (Q.IsReady())
                            {
                                Q.StartCharging();
                            }
                            if (Q.IsCharging && (Q.Range >= Q.ChargedMaxRange || Q.Range > Vector3.Distance(Player.ServerPosition, FurthestQ.Position)))
                            {
                                Q2.Cast(FurthestQ.Position);
                            }
                            
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
                if (config.Item("UseQJFarm").GetValue<bool>())
                {
                    if (Q.IsReady())
                    {
                        Q.StartCharging();
                    }
                    if (Q.IsCharging)
                    {
                        if (largemobs != null)
                        {
                            if (Q.Range >= Q.ChargedMaxRange || Q.Range >= Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition))
                            {
                                Q2.Cast(Player.ServerPosition.Extend(largemobs.ServerPosition, Q2.Range));
                            }
                        }
                        else
                        {
                            if (Q.Range >= Q.ChargedMaxRange || Q.Range >= Vector3.Distance(Player.ServerPosition, mob.ServerPosition))
                            {
                                Q2.Cast(Player.ServerPosition.Extend(mob.ServerPosition, Q2.Range));
                            }                            
                        }
                    }
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && Orbwalking.InAutoAttackRange(largemobs))
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
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.IsTargetable && !myUtility.ImmuneToPhysical(x) && !myUtility.ImmuneToCC(x));
                var targets = EnemyList.Where(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range);
                Obj_AI_Hero punchthis = null;
                switch (config.Item("UseRType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        punchthis = targets.OrderBy(i => (i.Health / Player.GetAutoAttackDamage(i))).FirstOrDefault();
                        break;
                    case 1:
                        punchthis = targets.FirstOrDefault(x => R.IsKillable(x));
                        break;
                    case 2:
                        punchthis = targets.OrderByDescending(z => myRePriority.ResortDB(z.ChampionName)).ThenBy(i => i.Health).FirstOrDefault(x => x.Health < Player.Health);
                        break;
                    case 3:
                        punchthis = targets.OrderByDescending(i => i.Distance(Player)).FirstOrDefault();
                        break;
                    case 4:
                        punchthis = targets.OrderBy(i => i.Health).FirstOrDefault();
                        break;
                }
                if (punchthis != null && punchthis.IsValidTarget())
                {
                    R.Cast(punchthis);
                }
            } 
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
            switch (config.Item("QPredHitchance").GetValue<StringList>().SelectedIndex)
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
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && (myUtility.PlayerManaPercentage > config.Item("FarmMana").GetValue<Slider>().Value))
            {
                var target = minion as Obj_AI_Base;
                if (target != null &&
                    E.IsKillable(target) &&
                    Orbwalking.InAutoAttackRange(target))
                {
                    E.Cast();
                }
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name.ToLower() == "vie")
                {
                    myOrbwalker.ResetAutoAttackTimer();
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
                    if (Q.IsCharging && Q.Range > Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition))
                    {
                        Q2.Cast(Player.ServerPosition.Extend(gapcloser.Sender.ServerPosition, Q2.Range));
                    }
                    else if (Q.IsReady())
                    {
                        Q.StartCharging();
                    }
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("UseQ2Misc").GetValue<bool>() && Q.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) < Q.Range && args.DangerLevel == Interrupter2.DangerLevel.High)
                {
                    if (myUtility.ImmuneToCC(sender)) return;
                    if (Q.IsCharging && Q.Range > Vector3.Distance(Player.ServerPosition, sender.ServerPosition))
                    {
                        Q.Cast(Player.ServerPosition.Extend(sender.ServerPosition, Q.Range));
                    }
                    else if (Q.IsReady())
                    {
                        Q.StartCharging();
                    }
                }
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe && !Player.IsWindingUp && E.IsReady() && target.IsValidTarget())
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && target is Obj_AI_Hero)
                {
                    if (config.Item("UseECombo").GetValue<bool>()) E.Cast();
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass && target is Obj_AI_Hero)
                {
                    if (config.Item("UseEHarass").GetValue<bool>()) E.Cast();
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear && target is Obj_AI_Minion)
                {
                    if (config.Item("UseEFarm").GetValue<bool>() && (myUtility.PlayerManaPercentage > config.Item("FarmMana").GetValue<Slider>().Value)) E.Cast();
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear)
                {
                    if (target is Obj_AI_Turret && target.Team != Player.Team &&
                        config.Item("UseEMisc").GetValue<bool>() &&
                        !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                    {
                        E.Cast();
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("DrawQ").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.ChargedMaxRange, Color.White);
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
                    var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.IsTargetable && !myUtility.ImmuneToPhysical(x) && !myUtility.ImmuneToCC(x));
                    var targets = EnemyList.Where(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range);
                    Obj_AI_Hero drawthis = null;
                    switch (config.Item("UseRType").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            drawthis = targets.OrderBy(i => (i.Health / Player.GetAutoAttackDamage(i))).FirstOrDefault(x => x.Health / Player.GetAutoAttackDamage(x) <= config.Item("UseRTypeLessHit").GetValue<Slider>().Value);
                            break;
                        case 1:
                            drawthis = targets.FirstOrDefault(x => R.IsKillable(x));
                            break;
                        case 2:
                            drawthis = targets.OrderByDescending(z => myRePriority.ResortDB(z.ChampionName)).ThenBy(i => i.Health).FirstOrDefault(x => x.Health < Player.Health);
                            break;
                        case 3:
                            drawthis = targets.OrderByDescending(i => i.Distance(Player)).FirstOrDefault();
                            break;
                        case 4:
                            drawthis = targets.OrderBy(i => i.Health).FirstOrDefault();
                            break;
                    }
                    if (drawthis != null && drawthis.IsValidTarget())
                    {
                        Render.Circle.DrawCircle(drawthis.Position, drawthis.BoundingRadius, Color.Lime, 7);
                    }
                }
            }
        }
    }
}
