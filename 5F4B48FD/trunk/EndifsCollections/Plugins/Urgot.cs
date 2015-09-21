using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCollections.Controller;
using EndifsCollections.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using ItemData = LeagueSharp.Common.Data.ItemData;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Urgot : PluginData
    {
        public Urgot()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 975f);            
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 900);
            R = new Spell(SpellSlot.R);

            Q2 = new Spell(SpellSlot.Q, 1200f);

            Q.SetSkillshot(0.2667f, 60f, 1600f, true, SkillshotType.SkillshotLine);            
            E.SetSkillshot(0.2658f, 120f, 1500f, false, SkillshotType.SkillshotCircle);

            Q2.SetSkillshot(0.3f, 60f, 1800f, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellList.Add(Q2);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Hyper-Kinetic Position Reverser", "Custom");
            {
                custommenu.AddItem(new MenuItem("UseRKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T                
                custommenu.AddItem(new MenuItem("UseRType", "R").SetValue(new StringList(new[] { "Less Hit", "YOLO", "Furthest", "Lowest HP", "Flash HKPR" })));
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
                laneclear.AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
                laneclear.AddItem(new MenuItem("EFarmValue", "E More Than").SetValue(new Slider(1, 1, 5)));
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
                miscmenu.AddItem(new MenuItem("UseWMisc", "W Shields").SetValue(false));
                miscmenu.AddItem(new MenuItem("Muramana", "Muramana").SetValue(new Slider(50)));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawQ2", "Q2").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawE", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Physical);
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToPhysical(target)) return;
                
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        if (target.HasBuff("urgotcorrosivedebuff"))
                        {
                            Q2.Cast(target.ServerPosition);
                        }
                        else
                        {
                            QPredict(target);
                        }
                    }                    
                    if (UseE && E.IsReady())
                    {
                        EPredict(target);
                    }
                }
                catch { }
            }
            
        }
        private void Harass()
        {
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var UseE = config.Item("UseEHarass").GetValue<bool>();

            if (UseQ && Q.IsReady() && !myOrbwalker.IsWaiting() && !Player.IsWindingUp)
            {
                var corrode = HeroManager.Enemies.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) < Q2.Range && x.HasBuff("urgotcorrosivedebuff")).OrderBy(i => i.Health).FirstOrDefault();
                if (corrode != null && corrode.IsValidTarget())
                {
                    Q2.Cast(corrode.ServerPosition);
                }
                else
                {
                    var targetQ = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                    if (targetQ != null && targetQ.IsValidTarget()) QPredict(targetQ);
                }
            }
            if (UseE && E.IsReady() && !myOrbwalker.IsWaiting() && !Player.IsWindingUp)
            {
                var targetE = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (targetE != null && targetE.IsValidTarget()) EPredict(targetE);
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (minionQ == null) return;
                var corrodeQ = minionQ.Where(x => x.HasBuff("urgotcorrosivedebuff")).OrderBy(i => i.Health);
                foreach (var mcQ in corrodeQ.Where(x => Q2.IsKillable(x)))
                {
                    Q2.Cast(mcQ.ServerPosition);
                }
                var normQ = minionQ.OrderBy(i => i.Health);
                foreach (var nmQ in normQ.Where(x => Q.IsKillable(x)))
                {
                    Q.CastIfHitchanceEquals(nmQ, HitChance.High);
                }
            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady())
            {
                var minionE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range + E.Width);
                var ECircular = E.GetCircularFarmLocation(minionE);
                if (ECircular.MinionsHit > config.Item("EFarmValue").GetValue<Slider>().Value)
                {
                    E.Cast(ECircular.Position);
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
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady())
                {
                    if (largemobs != null)
                    {
                        if (largemobs.HasBuff("urgotcorrosivedebuff")) Q2.Cast(largemobs.ServerPosition);
                        else Q.CastIfHitchanceEquals(largemobs, HitChance.High);
                    }
                    else
                    {
                        if (mob.HasBuff("urgotcorrosivedebuff")) Q2.Cast(mob.ServerPosition);
                        else Q.CastIfHitchanceEquals(mob, HitChance.High);
                    }
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null)
                    {
                        E.Cast(largemobs.ServerPosition);
                    }
                    else
                    {
                        var mobE = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.Health);
                        if (mobE == null) return;
                        var epred = E.GetCircularFarmLocation(mobE);
                        if (epred.MinionsHit > 0)
                        {
                            E.Cast(epred.Position);
                        }
                    }
                }
            }
        }
        private void Custom()
        {
            if (R.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.IsTargetable);
                var targets = EnemyList.Where(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                Obj_AI_Hero reversethis = null;
                
                switch (config.Item("UseRType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        reversethis = targets.OrderBy(i => (i.Health / Player.GetAutoAttackDamage(i))).FirstOrDefault();
                        break;
                    case 1:
                        reversethis = targets.OrderByDescending(z => myRePriority.ResortDB(z.ChampionName)).ThenBy(i => i.Health).FirstOrDefault(x => x.Health < Player.Health);
                        break;
                    case 2:
                        reversethis = targets.OrderByDescending(i => i.Distance(Player)).FirstOrDefault();
                        break;
                    case 3:
                        reversethis = targets.OrderBy(i => i.Health).FirstOrDefault();                        
                        break;
                }
                if (reversethis != null && reversethis.IsValidTarget() && !Player.IsRooted)
                {
                    R.Cast(reversethis);
                }
                else if (config.Item("UseRType").GetValue<StringList>().SelectedIndex == 4 && 
                    FlashSlot != SpellSlot.Unknown && 
                    Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready)
                {                    
                    Obj_AI_Hero FHKPR = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) > R.Range &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range + 425f &&
                                    !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x)
                                    ).OrderBy(i => i.Health).FirstOrDefault();
                    if (FHKPR == null) return;
                    R.UpdateSourcePosition(Player.ServerPosition.Extend(FHKPR.ServerPosition, 425f));
                    Player.Spellbook.CastSpell(FlashSlot, Player.ServerPosition.Extend(FHKPR.ServerPosition, 425f));
                    R.Cast(FHKPR);
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
        private void QPredict(Obj_AI_Base target)
        {
            PredictionOutput pred = Q.GetPrediction(target);
            if (pred.CollisionObjects.Count == 0 && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range)
            {
                var test1 = Prediction.GetPrediction(target, Q.Instance.SData.MissileSpeed).CastPosition;
                float movement = target.MoveSpeed * 100 / 1000;
                if (target.Distance(test1) > movement) Q.Cast(target.ServerPosition.Extend(test1, Q.Instance.SData.MissileSpeed * target.MoveSpeed));
                else
                {
                    if (pred.Hitchance >= QHitChance) Q.Cast(pred.CastPosition);
                }
            }
        }
        private void EPredict(Obj_AI_Base target)
        {
            var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where champ.IsValidTarget(E.Range) && target != champ select champ).ToList();
            if (nearChamps.Count > 0)
            {
                var closeToPrediction = new List<Obj_AI_Hero>();
                foreach (var enemy in nearChamps)
                {
                    PredictionOutput prediction = E.GetPrediction(enemy);
                    if (prediction.Hitchance >= HitChance.Medium && (Vector3.Distance(Player.ServerPosition, enemy.ServerPosition) < E.Range))
                    {
                        closeToPrediction.Add(enemy);
                    }
                }
                if (closeToPrediction.Count == 0)
                {
                    E.CastIfHitchanceEquals(target, QHitChance);
                }
                else if (closeToPrediction.Count > 0)
                {
                    E.CastIfWillHit(target, closeToPrediction.Count, false);
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
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe && W.IsReady() && target.IsValidTarget())
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && target is Obj_AI_Hero)
                {
                    if (config.Item("UseWCombo").GetValue<bool>()) W.Cast();
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass && target is Obj_AI_Hero)
                {
                    if (config.Item("UseWHarass").GetValue<bool>()) W.Cast();
                }               
            }
        }
        protected override void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender is Obj_SpellMissile && sender.IsValid)
            {
                var missile = (Obj_SpellMissile)sender;
                if (missile.SpellCaster.IsMe)
                {
                    if (sender.Type == GameObjectType.obj_SpellCircleMissile || sender.Type == GameObjectType.obj_SpellLineMissile)
                    {
                        if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo &&
                            ItemData.Muramana.GetItem().IsReady() &&
                            !Player.HasBuff("Muramana") &&
                            myUtility.PlayerManaPercentage > config.Item("Muramana").GetValue<Slider>().Value)
                        {
                            if (Items.HasItem(3042) && Items.CanUseItem(3042)) Items.UseItem(3042);
                        }
                    }
                }
            }
        }
        protected override void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender is Obj_SpellMissile && sender.IsValid)
            {
                var missile = (Obj_SpellMissile)sender;
                if (missile.SpellCaster.IsMe)
                {
                    if (sender.Type == GameObjectType.obj_SpellCircleMissile || sender.Type == GameObjectType.obj_SpellLineMissile)
                    {
                        if (myUtility.PlayerManaPercentage < config.Item("Muramana").GetValue<Slider>().Value ||
                            (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.None && Player.HasBuff("Muramana")))
                        {
                            if (Items.HasItem(3042) && Items.CanUseItem(3042)) Items.UseItem(3042);
                        }
                    }
                }
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
                        if (config.Item("UseWMisc").GetValue<bool>())
                        {
                            W.Cast();
                        }
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
            if (config.Item("DrawQ2").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q2.Range, Color.White);
            }
            if (config.Item("DrawE").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.White);
            }
            if (R.IsReady())
            {
                if (config.Item("UseRDrawDistance").GetValue<bool>())
                {
                    if (config.Item("UseRType").GetValue<StringList>().SelectedIndex == 4)
                    {
                        Render.Circle.DrawCircle(Player.Position, 425f, Color.Fuchsia, 7);
                        Render.Circle.DrawCircle(Player.Position, R.Range + 425f, Color.Fuchsia, 7);
                    }
                    else Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 7);
                }
                if (config.Item("UseRDrawTarget").GetValue<bool>())
                {
                    var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.IsTargetable);
                    var targets = EnemyList.Where(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                    Obj_AI_Hero drawthis = null;
                    switch (config.Item("UseRType").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            drawthis = targets.OrderBy(i => (i.Health / Player.GetAutoAttackDamage(i))).FirstOrDefault(x => x.Health / Player.GetAutoAttackDamage(x) <= config.Item("UseRTypeLessHit").GetValue<Slider>().Value);
                            break;
                        case 1:
                            drawthis = targets.OrderByDescending(z => myRePriority.ResortDB(z.ChampionName)).ThenBy(i => i.Health).FirstOrDefault(x => x.Health < Player.Health);
                            break;
                        case 2:
                            drawthis = targets.OrderByDescending(i => i.Distance(Player)).FirstOrDefault();
                            break;
                        case 3:
                            drawthis = targets.OrderBy(i => i.Health).FirstOrDefault();
                            break;
                    }
                    if (drawthis != null && drawthis.IsValidTarget())
                    {
                        Render.Circle.DrawCircle(drawthis.Position, drawthis.BoundingRadius, Color.Lime, 7);
                    }
                    else if (config.Item("UseRType").GetValue<StringList>().SelectedIndex == 4)
                    {
                        Obj_AI_Hero FHKPR = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) > R.Range &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range + 425f &&
                                    !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x)
                                    ).OrderBy(i => i.Health).FirstOrDefault();
                        if (FHKPR != null && FHKPR.IsValidTarget())
                        {
                            Render.Circle.DrawCircle(FHKPR.Position, FHKPR.BoundingRadius, Color.Lime, 7);
                        }
                    }
                }
            }
        }
    }
}
