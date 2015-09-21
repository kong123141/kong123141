using System;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.SummonerSpells;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
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
                custommenu.AddItem(new MenuItem("EC.Vi.UseRKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("EC.Vi.UseRType", "R").SetValue(new StringList(new[] { "Less Hit", "R Killable", "YOLO", "Furthest", "Lowest HP" })));
                custommenu.AddItem(new MenuItem("EC.Vi.UseRTypeLessHit", "Hit <").SetValue(new Slider(4, 1, 10)));
                custommenu.AddItem(new MenuItem("EC.Vi.UseRDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("EC.Vi.UseRDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Vi.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Vi.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Vi.Combo.QMax", "Q Max charge").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Vi.Combo.Dive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Vi.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Vi.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Vi.Harass.E", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Vi.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Vi.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Vi.QFarmType", "Q").SetValue(new StringList(new[] { "Any (Slider Value)", "Furthest" })));
                laneclearmenu.AddItem(new MenuItem("EC.Vi.Farm.Q.Value", "(Any) Q More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Vi.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Vi.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Vi.Jungle.E", "Use E").SetValue(true));
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Vi.QPredHitchance", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("EC.Vi.Misc.Q", "Q Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Vi.Misc.Q2", "Q Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Vi.Misc.E", "E Turrets").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Vi.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Vi.Draw.E", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range + 425, TargetSelector.DamageType.Physical);       
   
            var UseQ = config.Item("EC.Vi.Combo.Q").GetValue<bool>();
            var CastItems = config.Item("EC.Vi.Combo.Items").GetValue<bool>();
            if (UseQ && Q.IsReady() && Q.IsCharging)
            {
                if (Target.IsValidTarget() && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= Q.ChargedMaxRange - Target.BoundingRadius)
                {
                    PredictionOutput pred = Q.GetPrediction(Target);
                    if (pred.Hitchance >= QHitChance)
                    {
                        if (Orbwalking.InAutoAttackRange(Target))
                        {
                            Q.Cast(Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition,Target.ServerPosition) + Target.BoundingRadius));
                        }
                        else if (config.Item("EC.Vi.Combo.QMax").GetValue<bool>())
                        {
                            if (Vector3.Distance(Player.ServerPosition, pred.CastPosition) + Target.BoundingRadius <= Q.ChargedMaxRange)
                            {
                                Q.Cast(Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition) + Target.BoundingRadius));
                            }
                        }
                        else
                        {
                            if ((Vector3.Distance(Player.ServerPosition, pred.CastPosition) + Target.BoundingRadius <= Q.Range))
                            {
                                Q.Cast(Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, pred.CastPosition) + Target.BoundingRadius));
                            }
                        }
                    }
                }
                if (!Target.IsValidTarget() || Target == null)
                {
                    return;
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;                
                if (CastItems) { myItemManager.UseItems(0, Target); }
                try
                {
                    if (UseQ)
                    {
                        if (Target.ServerPosition.UnderTurret(true) && !config.Item("EC.Vi.Combo.Dive").GetValue<bool>()) return;
                        if (!Orbwalking.InAutoAttackRange(Target) && Q.IsReady() && !Q.IsCharging)
                        {                            
                            Q.StartCharging();
                        }
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
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical, false);
            var UseQ = config.Item("EC.Vi.Harass.Q").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (UseQ)
                {
                    if (target.ServerPosition.UnderTurret(true) && !config.Item("EC.Vi.Combo.Dive").GetValue<bool>()) return;
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
            if (myUtility.PlayerManaPercentage < config.Item("EC.Vi.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (config.Item("EC.Vi.Farm.Q").GetValue<bool>()  && !Player.IsWindingUp && !Player.IsDashing())
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range).ToList();
                switch (config.Item("EC.Vi.QFarmType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        var QLine = Q2.GetLineFarmLocation(minionQ);
                        if (QLine.Position.IsValid() && !QLine.Position.To3D().UnderTurret(true))
                        {
                            if (Q.IsReady())
                            {
                                Q.StartCharging();
                            }
                            if (Q.IsCharging && QLine.MinionsHit > config.Item("EC.Vi.Farm.Q.Value").GetValue<Slider>().Value)
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
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, Q2.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (config.Item("EC.Vi.Jungle.Q").GetValue<bool>())
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
                if (config.Item("EC.Vi.Jungle.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
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
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.IsTargetable && !myUtility.ImmuneToDeath(x) && !myUtility.ImmuneToCC(x));
                var targets = EnemyList.Where(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range);
                Obj_AI_Hero punchthis = null;
                switch (config.Item("EC.Vi.UseRType").GetValue<StringList>().SelectedIndex)
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
            switch (config.Item("EC.Vi.QPredHitchance").GetValue<StringList>().SelectedIndex)
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
            if (config.Item("EC.Vi.Farm.E").GetValue<bool>() && E.IsReady() && (myUtility.PlayerManaPercentage > config.Item("EC.Vi.Farm.ManaPercent").GetValue<Slider>().Value))
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
            if (config.Item("EC.Vi.Misc.Q").GetValue<bool>() && Q.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= Q.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    if (Q.IsCharging && (Q.Range + gapcloser.Sender.BoundingRadius) > Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition))
                    {
                        Q2.Cast(Player.ServerPosition.Extend(gapcloser.End, Q2.Range));
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
            if (config.Item("EC.Vi.Misc.Q2").GetValue<bool>() && Q.IsReady())
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
            if (unit.IsMe && E.IsReady() && target.IsValidTarget())
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && target is Obj_AI_Hero)
                {
                    if (config.Item("EC.Vi.Combo.E").GetValue<bool>()) E.Cast();
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass && target is Obj_AI_Hero)
                {
                    if (config.Item("EC.Vi.Harass.E").GetValue<bool>()) E.Cast();
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear && target is Obj_AI_Minion)
                {
                    if (config.Item("EC.Vi.Farm.E").GetValue<bool>() && (myUtility.PlayerManaPercentage > config.Item("EC.Vi.Farm.ManaPercent").GetValue<Slider>().Value)) E.Cast();
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear)
                {
                    if (target is Obj_AI_Turret && target.Team != Player.Team &&
                        config.Item("EC.Vi.Misc.E").GetValue<bool>() &&
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
            if (config.Item("EC.Vi.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.ChargedMaxRange, Color.White);
            }
            if (config.Item("EC.Vi.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (R.Level > 0 && R.IsReady())
            {
                if (config.Item("EC.Vi.UseRDrawDistance").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 7);
                }
                if (config.Item("EC.Vi.UseRDrawTarget").GetValue<bool>())
                {
                    var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.IsTargetable && !myUtility.ImmuneToDeath(x) && !myUtility.ImmuneToCC(x));
                    var targets = EnemyList.Where(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range);
                    Obj_AI_Hero drawthis = null;
                    switch (config.Item("EC.Vi.UseRType").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            drawthis = targets.OrderBy(i => (i.Health / Player.GetAutoAttackDamage(i))).FirstOrDefault(x => x.Health / Player.GetAutoAttackDamage(x) <= config.Item("EC.Vi.UseRTypeLessHit").GetValue<Slider>().Value);
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
