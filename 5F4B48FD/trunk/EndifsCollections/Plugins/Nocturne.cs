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
    class Nocturne : PluginData
    {
        public Nocturne()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1200);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 425);
            R = new Spell(SpellSlot.R);

            R2 = new Spell(SpellSlot.R, 2000);
           
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellList.Add(R2);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Paranoia", "Custom");
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
                laneclear.AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(false));
                laneclear.AddItem(new MenuItem("QFarmValue", "Q More Than").SetValue(new Slider(1, 1, 5)));
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
                miscmenu.AddItem(new MenuItem("UseWMisc", "W Spellblock").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseEMisc", "E Gapcloser").SetValue(false));
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
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var CastItems = config.Item("UseItemCombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToPhysical(target)) return;                
                if (CastItems) { myUtility.UseItems(0, target); }
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        QPredict(target);
                    }
                    if (UseE && E.IsReady() && E.IsInRange(target) && !Player.IsWindingUp)
                    {
                        if (myUtility.ImmuneToMagic(target)) return;
                        E.CastOnUnit(target);
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
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var UseE = config.Item("UseEHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget() && !Player.UnderTurret(true) && !target.UnderTurret(true) && !Player.IsWindingUp)
            {
                if (UseQ && Q.IsReady() && Q.IsInRange(target))
                {
                    if (myUtility.IsFacing(Player, target.ServerPosition, 60)) QPredict(target);
                }
                if (UseE && E.IsReady() && E.IsInRange(target))
                {
                    if (myUtility.IsFacing(Player, target.ServerPosition)) E.CastOnUnit(target);
                } 
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            var minions = MinionManager.GetMinions(Player.ServerPosition, Player.AttackRange * 2, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None);
            if (minions.Count >= 3 && !myOrbwalker.IsWaiting() && !Player.IsWindingUp)
            {
                myUtility.UseItems(2, null);
            }
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                var MinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                var QLine = Q.GetLineFarmLocation(MinionsQ);
                if (QLine.Position.IsValid() && Vector3.Distance(Player.ServerPosition, QLine.Position.To3D()) > Player.AttackRange)
                {
                    if (Player.UnderTurret(true)) return;
                    if (QLine.MinionsHit > config.Item("QFarmValue").GetValue<Slider>().Value)
                    {
                        if (myUtility.IsFacing(Player, QLine.Position.To3D())) Q.Cast(QLine.Position);
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
            if (mob == null) return;
            if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady())
            {
                if (largemobs != null)
                {
                    Q.Cast(largemobs.ServerPosition);
                }
                var QLine = Q.GetLineFarmLocation(mobs);
                if (QLine.MinionsHit > 0)
                {
                    Q.Cast(QLine.Position);
                }
            }
            if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady())
            {
                if (largemobs != null && E.IsInRange(largemobs))
                {
                    E.CastOnUnit(largemobs);
                }
            }
            
        }
        private void Custom()
        {
            if (R.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.IsTargetable && !myUtility.ImmuneToPhysical(x));
                var targets = EnemyList.Where(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R2.Range);
                Obj_AI_Hero paranoiathis = null;
                switch (config.Item("UseRType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        paranoiathis = targets.OrderBy(i => (i.Health / Player.GetAutoAttackDamage(i))).FirstOrDefault();
                        break;
                    case 1:
                        paranoiathis = targets.FirstOrDefault(x => R.IsKillable(x));
                        break;
                    case 2:
                        paranoiathis = targets.OrderByDescending(z => myRePriority.ResortDB(z.ChampionName)).ThenBy(i => i.Health).FirstOrDefault();
                        break;
                    case 3:
                        paranoiathis = targets.OrderByDescending(i => i.Distance(Player)).FirstOrDefault();
                        break;
                    case 4:
                        paranoiathis = targets.OrderBy(i => i.Health).FirstOrDefault();
                        break;
                }
                Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : paranoiathis;
                if (target != null && target.IsValidTarget())
                {
                    R.Cast();
                    R.CastOnUnit(target);
                }
            } 
        }

        private void UpdateR2()
        {
            if (R.Level > 0)
            {
                R2.Range = 1250 + R.Level * 750;
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
        private void QPredict(Obj_AI_Hero target)
        {            
            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range)
            {
                if (myUtility.MovementImpaired(target))
                {
                    Q.Cast(target.ServerPosition);
                }
                PredictionOutput pred = Q.GetPrediction(target);
                var test1 = Prediction.GetPrediction(target, Q.Instance.SData.SpellCastTime).CastPosition;
                float movement = target.MoveSpeed * 100 / 1000;
                if (target.Distance(test1) > movement) Q.Cast(target.ServerPosition.Extend(test1, Q.Instance.SData.SpellCastTime * target.MoveSpeed));
                else
                {
                    if (pred.Hitchance >= QHitChance) Q.Cast(pred.CastPosition);
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
            UpdateR2();
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
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (!unit.IsMe && unit.IsEnemy && unit.IsValid<Obj_AI_Hero>())
            {
                if (spell.Target == null || !spell.Target.IsValid || !spell.Target.IsMe)
                {
                    return;
                }
                if (!spell.SData.IsAutoAttack() && spell.Target.IsMe && W.IsReady())
                {
                    if (unit.IsChampion(unit.BaseSkinName))
                    {
                        if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                        {
                            if (config.Item("UseWCombo").GetValue<bool>())
                            {
                                 Utility.DelayAction.Add(myUtility.RandomDelay(0, 100), () => W.CastOnUnit(Player));
                            }
                        }
                        if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass)
                        {
                            if (config.Item("UseWHarass").GetValue<bool>())
                            {
                                Utility.DelayAction.Add(myUtility.RandomDelay(0, 100), () => W.CastOnUnit(Player));
                            }
                        }
                        if (config.Item("UseWMisc").GetValue<bool>())
                        {
                            Utility.DelayAction.Add(myUtility.RandomDelay(0, 100), () => W.CastOnUnit(Player));
                        }
                    }
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("UseEMisc").GetValue<bool>() && E.IsReady())
            {
                if (E.IsReady() && Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;
                    E.CastOnUnit(gapcloser.Sender);
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("UseEMisc").GetValue<bool>() && E.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(sender) || myUtility.ImmuneToMagic(sender)) return;
                    E.CastOnUnit(sender);
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
            if (unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear)
                {
                    if (target is Obj_AI_Minion && target.Team == GameObjectTeam.Neutral && !target.Name.Contains("Mini") &&
                        !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                    {
                        myUtility.UseItems(2, null);
                    }
                }
            }
        }
        protected override void OnEndScene(EventArgs args)
        {
            if (Player.IsDead) return;
            if (R.Level > 0 && R.IsReady())
            {
                if (config.Item("UseRDrawDistance").GetValue<bool>())
                {
                    Utility.DrawCircle(Player.Position, R2.Range, Color.Fuchsia, 1, 30, true);
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
            if (config.Item("DrawE").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.White);
            }
            if (R.Level > 0 && R.IsReady())
            {
                if (config.Item("UseRDrawDistance").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R2.Range, Color.Fuchsia);
                }
                if (config.Item("UseRDrawTarget").GetValue<bool>())
                {
                    var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.IsTargetable && !myUtility.ImmuneToPhysical(x));
                    var targets = EnemyList.Where(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R2.Range);
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
                            drawthis = targets.OrderByDescending(z => myRePriority.ResortDB(z.ChampionName)).ThenBy(i => i.Health).FirstOrDefault();
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
