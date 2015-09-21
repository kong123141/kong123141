using System;
using System.Linq;
using EndifsCollections.Controller;
using EndifsCollections.SummonerSpells;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Shyvana : PluginData
    {
        public Shyvana()
        {
            LoadSpells();
            LoadMenus();
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 300);
            E = new Spell(SpellSlot.E, 925);
            R = new Spell(SpellSlot.R, 1000);

            //Dragon Form
            E2 = new Spell(SpellSlot.E, 925);

            E.SetSkillshot(0.5f, E.Instance.SData.LineWidth, float.MaxValue, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(R.Instance.SData.SpellCastTime, R.Instance.SData.LineWidth, R.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotLine);

            E2.SetSkillshot(0.5f, 15f * 2 * (float)Math.PI / 180, float.MaxValue, false, SkillshotType.SkillshotCone);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellList.Add(E2);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Dragon's Descent", "Custom");
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
                laneclear.AddItem(new MenuItem("UseQLastHit", "(Last Hit) Use Q").SetValue(true));
                laneclear.AddItem(new MenuItem("UseWFarm", "Use W").SetValue(true));
                laneclear.AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
                laneclear.AddItem(new MenuItem("WFarmValue", "W More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("EFarmValue", "E More Than").SetValue(new Slider(1, 1, 5)));
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
                miscmenu.AddItem(new MenuItem("EPredHitchance", "E Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("UseQMisc", "Q Turrets").SetValue(false));
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
            
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var CastItems = config.Item("UseItemCombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToPhysical(target)) return;
                if (CastItems) myUtility.UseItems(0, target);
                if (mySmiter.CanSmiteChampions(target)) mySmiter.Smites(target);
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        if (!Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                        {
                            Q.Cast();
                       }
                    }
                    if (UseW && W.IsReady())
                    {
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < W.Range * 2 / 3)
                        {
                            W.Cast();
                        }
                        else
                        {
                            var dist = Vector3.Distance(Player.ServerPosition, target.ServerPosition);
                            var msDif = Player.MoveSpeed - target.MoveSpeed;
                            var reachIn = dist / msDif;
                            if (msDif < 0 && reachIn > 1)
                            {
                                W.Cast();
                            }
                            else if (msDif > 0 && reachIn > 3)
                            {
                                W.Cast();
                            }
                        }
                    }
                    if (UseE && E.IsReady())
                    {
                        EPredict(target);
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
                catch
                {
                }
            }
        }
        private void Harass()
        {
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var UseW = config.Item("UseWHarass").GetValue<bool>();
            var UseE = config.Item("UseEHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (target != null && target.IsValidTarget())
            {
                if (UseQ && Q.IsReady())
                {
                    if (target.UnderTurret(true)) return;
                    if (!Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                    {
                        Q.Cast();
                    }
                }
                if (UseW && W.IsReady())
                {
                    if (target.UnderTurret(true) && Player.UnderTurret(true)) return;
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < W.Range)
                    {
                        W.Cast();
                    }
                }
                if (UseE && E.IsReady())
                {
                    if (target.UnderTurret(true) && Player.UnderTurret(true)) return;
                    EPredict(target);
                }
            }
        }
        private void LaneClear()
        {
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && !myOrbwalker.IsWaiting() && !Player.IsWindingUp)
            {
                var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                var siegeQ = myUtility.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                if (siegeQ != null && siegeQ.IsValidTarget())
                {
                    Q.Cast();
                }
                else
                {
                    var selectQ = allMinionsQ.Where(x => Q.IsKillable(x)).OrderBy(i => i.Distance(Player)).FirstOrDefault();
                    if (selectQ != null && selectQ.IsValidTarget())
                    {
                        Q.Cast();
                    }
                }
            }
            if (config.Item("UseWFarm").GetValue<bool>() && W.IsReady() && !myOrbwalker.IsWaiting())
            {
                var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range);
                if (allMinionsW == null) return;
                if (allMinionsW.Count > config.Item("WFarmValue").GetValue<Slider>().Value)
                {
                    if (Player.UnderTurret(true)) return;
                    W.CastOnUnit(Player);
                }
            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && !myOrbwalker.IsWaiting() && !Player.IsWindingUp)
            {
                var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (allMinionsE == null) return;
                if (DragonForm)
                {
                    foreach (var x in allMinionsE)
                    {
                        if (MinionManager.GetMinions(x.ServerPosition, 275).Count() > config.Item("EFarmValue").GetValue<Slider>().Value)
                        {
                            if (x.IsValidTarget() && x.ServerPosition.IsValid()) E.Cast(x.ServerPosition);
                        }
                    }
                }
                else
                {
                    var ELine = E.GetLineFarmLocation(allMinionsE, E.Width);
                    if (ELine.MinionsHit > config.Item("EFarmValue").GetValue<Slider>().Value)
                    {
                        if (Player.UnderTurret(true)) return;
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
            if (mob != null)
            {
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && Q.IsInRange(largemobs))
                    {
                        Q.Cast();
                    }
                    else if (Q.IsInRange(mob)) Q.Cast();
                }
                if (config.Item("UseWJFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < W.Range)
                    {
                        W.CastOnUnit(Player);
                    }
                    else if (Vector3.Distance(Player.ServerPosition, mob.ServerPosition) < W.Range) W.CastOnUnit(Player);
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null)
                    {
                        E.Cast(largemobs.ServerPosition);
                    }
                    else if (myUtility.IsFacing(Player, mob.ServerPosition)) E.Cast(mob.ServerPosition);
                }
            }
        }
        private void LastHit()
        {
            if (myOrbwalker.IsWaiting() && !Player.IsWindingUp && config.Item("UseQLastHit").GetValue<bool>() && Q.IsReady())
            {
                Q.Cast();
            }
        }
        private void Custom()
        {
            if (R.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                             Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range
                             ).OrderByDescending(i => i.Distance(Player)).ThenBy(v => v.CountEnemiesInRange(R.Width)).FirstOrDefault();
                if (target != null && target.IsValidTarget())
                {
                    PredictionOutput pred = R.GetPrediction(target);
                    if (pred.CollisionObjects.Count == 0 && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < R.Range)
                    {
                        Vector3 pos;
                        var test1 = Prediction.GetPrediction(target, R.Instance.SData.MissileSpeed).CastPosition;
                        float movement = target.MoveSpeed * 100 / 1000;
                        if (target.Distance(test1) > movement)
                        {
                            pos = target.ServerPosition.Extend(Player.ServerPosition.Extend(test1, R.Instance.SData.MissileSpeed * target.MoveSpeed), R.Width);
                            var ext = Player.ServerPosition.Extend(pos, R.Range);
                            R.Cast(ext);
                        }
                        else
                        {
                            if (pred.Hitchance >= ERHitChance)
                            {
                                pos = Player.ServerPosition.Extend(pred.CastPosition, R.Range);
                                R.Cast(pos);
                            }
                        }
                    }
                }
            }           
        }

        private bool DragonForm
        {
            get { return Player.HasBuff("shyvanatransform"); }
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
            switch (config.Item("EPredHitchance").GetValue<StringList>().SelectedIndex)
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
            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= E.Range)
            {
                if (myUtility.MovementImpaired(target))
                {
                    E.Cast(target.ServerPosition);
                }
                Vector3 pos;
                PredictionOutput pred = E.GetPrediction(target);
                var test1 = Prediction.GetPrediction(target, E.Instance.SData.MissileSpeed).CastPosition;
                float movement = target.MoveSpeed * 100 / 1000;
                if (target.Distance(test1) > movement)
                {
                    pos = target.ServerPosition.Extend(test1, E.Instance.SData.MissileSpeed * target.MoveSpeed);
                    if (Vector3.Distance(Player.ServerPosition, pos) <= E.Range)
                    {
                        E.Cast(pos);
                    }
                }
                else
                {
                    pos = pred.CastPosition;
                    if (pred.Hitchance >= ERHitChance && Vector3.Distance(Player.ServerPosition, pos) <= E.Range) E.Cast(pos);
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
                case myOrbwalker.OrbwalkingMode.Lasthit:
                    LastHit();
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
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                var target = minion as Obj_AI_Base;
                if (target != null &&
                    Q.IsKillable(target) &&
                    Orbwalking.InAutoAttackRange(target))
                {
                    Q.Cast();
                }
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Minion && args.Target.Team == GameObjectTeam.Neutral)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear &&
                    !args.Target.Name.Contains("Mini") &&
                    !Player.IsWindingUp &&
                    Orbwalking.InAutoAttackRange(args.Target))
                {
                    myUtility.UseItems(2, null);
                }
            } 
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe )
            {
                if (target is Obj_AI_Hero && target.IsValid<Obj_AI_Hero>() && target.IsValidTarget())
                {
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                    {
                        if (config.Item("UseItemCombo").GetValue<bool>() && !Player.IsWindingUp)
                        {
                            myUtility.UseItems(2, null);
                        }
                        if (config.Item("UseQCombo").GetValue<bool>() && !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target) && Q.IsReady())
                        {
                            Q.Cast();
                        }
                    }
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear)
                {
                    if (target is Obj_AI_Turret && target.Team != Player.Team &&
                        config.Item("UseQMisc").GetValue<bool>() &&
                        !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target) && Q.IsReady())
                    {
                        Q.Cast();
                    }
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
                    var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToPhysical(x));
                    var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                 Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range
                                 ).OrderByDescending(i => i.Distance(Player)).ThenBy(v => v.CountEnemiesInRange(R.Width)).FirstOrDefault();
                    if (target != null && target.IsValidTarget())
                    {
                        var num = target.CountEnemiesInRange(R.Width);
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
                                var ext = Player.ServerPosition.Extend(pos, R.Range);
                                Render.Circle.DrawCircle(
                                    ext,
                                    R.Width, Color.Lime, 7);
                            }
                            else
                            {
                                if (pred.Hitchance >= ERHitChance)
                                {
                                    pos = Player.ServerPosition.Extend(pred.CastPosition, R.Range);
                                    Render.Circle.DrawCircle(
                                        pos, R.Width
                                        , Color.Lime, 7);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}