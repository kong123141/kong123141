using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Vladimir : PluginData
    {
        public Vladimir()
        {
            LoadSpells();
            LoadMenus();
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 590);
            W = new Spell(SpellSlot.W, 310);
            E = new Spell(SpellSlot.E, 610);
            R = new Spell(SpellSlot.R, 700);

            R.SetSkillshot(R.Instance.SData.SpellCastTime, R.Instance.SData.LineWidth, R.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Hemoplague", "Custom");
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
                miscmenu.AddItem(new MenuItem("RPredHitchance", "R Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("UseWMisc", "W Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseW2Misc", "W Spelldodge").SetValue(false));
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
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);            
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToMagic(target)) return;
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range)
                        {
                            Q.CastOnUnit(target);
                        }                       
                    }
                    if (UseW && W.IsReady())
                    {
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < W.Range)
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
                            else if (msDif > 0 && reachIn > 2)
                            {
                                W.Cast();
                            }
                        }
                    }
                    if (UseE && E.IsReady())
                    {
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < E.Range)
                        {
                            E.Cast();
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
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget() && !myOrbwalker.IsWaiting() && !Player.IsWindingUp)
            {
                if (UseQ && Q.IsReady() && Q.IsInRange(target))
                {
                    if (target.UnderTurret(true) && Player.UnderTurret(true)) return;
                    Q.CastOnUnit(target);
                }
                if (UseW && W.IsReady())
                {
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < W.Range)
                    {
                        W.Cast();
                    }
                }
                if (UseE && E.IsReady() && myUtility.TickCount - ETick > 6000)
                {
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < E.Range)
                    {
                        if (target.UnderTurret(true) && Player.UnderTurret(true)) return;
                        E.Cast();
                    }
                }
            }
        }
        private void LaneClear()
        {
            if (WActive) return;
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (allMinionsQ == null) return;
                var siegeQ = myUtility.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                if (siegeQ != null && siegeQ.IsValidTarget())
                {
                    Q.CastOnUnit(siegeQ);
                }
                else
                {
                    var selectQ = allMinionsQ.Where(x => Q.IsKillable(x) && Player.BaseAttackDamage < x.Health).OrderByDescending(i => i.Health).FirstOrDefault();
                    if (selectQ != null && selectQ.IsValidTarget())
                    {
                        if (myUtility.IsFacing(Player, selectQ.ServerPosition)) Q.CastOnUnit(selectQ);
                    }
                }
            }
            if (config.Item("UseWFarm").GetValue<bool>() && W.IsReady() && EBuffStacks >= 4)
            {
                var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range);
                if (allMinionsW == null) return;
                if (allMinionsW.Count > config.Item("WFarmValue").GetValue<Slider>().Value)
                {
                    if (Player.UnderTurret(true)) return;
                    W.Cast();
                }
            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && (EBuffStacks < 4 && myUtility.TickCount - ETick > 8000 || EBuffStacks >= 4))
            {
                var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (allMinionsE == null) return;
                if (allMinionsE.Count > config.Item("EFarmValue").GetValue<Slider>().Value)
                {
                    if (Player.UnderTurret(true)) return;
                    E.Cast();
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
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp && Q.IsInRange(mob))
                {
                    if (largemobs != null)
                    {
                        Q.CastOnUnit(largemobs);
                    }
                    Q.CastOnUnit(mob);
                }
                if (config.Item("UseWJFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < W.Range)
                    {
                        W.Cast();
                    }
                    if (Vector3.Distance(Player.ServerPosition, mob.ServerPosition) < W.Range) W.Cast();
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < E.Range)
                    {
                        E.Cast();
                    }
                    E.Cast();
                }
            }
        }
        private void Custom()
        {
            if (R.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToMagic(x));
                var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                             Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range
                             ).OrderByDescending(i => i.CountEnemiesInRange(175)).FirstOrDefault();
                if (target != null && target.IsValidTarget())
                {
                    PredictionOutput pred = R.GetPrediction(target);
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < R.Range)
                    {
                        var test1 = Prediction.GetPrediction(target, R.Instance.SData.SpellCastTime).CastPosition;
                        float movement = target.MoveSpeed * 100 / 1000;
                        if (target.Distance(test1) > movement)
                        {
                            R.Cast(target.ServerPosition.Extend(Player.ServerPosition.Extend(test1, R.Instance.SData.SpellCastTime * target.MoveSpeed), R.Width * 2 / 3));
                        }
                        else
                        {
                            if (pred.Hitchance >= RHitChance) R.Cast(target.ServerPosition.Extend(pred.CastPosition, R.Range));
                        }
                    }
                }
            }
           
        }

        private bool WActive
        {
            get
            {
                return Player.HasBuff("vladimirsanguinepool");                
            }
        }
        private int EBuffStacks
        {
            get { return Player.Buffs.Count(x => x.Name == "vladimirtidesofbloodcost"); }
        }
        private int ETick;
        private HitChance RHitChance
        {
            get
            {
                return GetRHitChance();
            }
        }
        private HitChance GetRHitChance()
        {
            switch (config.Item("RPredHitchance").GetValue<StringList>().SelectedIndex)
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
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                var target = minion as Obj_AI_Base;
                if (target != null &&
                    Q.IsKillable(target) && Player.GetAutoAttackDamage(target) < target.Health && 
                    Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range
                    )
                {
                    if (myUtility.IsFacing(Player, target.ServerPosition, 60)) Q.CastOnUnit(target);
                }
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
                if (spell.SData.Name.ToLower() == "vladimirtidesofblood")
                {
                    ETick = myUtility.TickCount;
                }
            }
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
                        if (config.Item("UseW2Misc").GetValue<bool>())
                        {
                            W.Cast();
                        }
                    }
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("UseWMisc").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) < W.Range)
                {
                    W.Cast();
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
                                     ).OrderByDescending(i => i.CountEnemiesInRange(175)).FirstOrDefault();
                        if (target != null && target.IsValidTarget())
                        {
                            var num = EnemyList.Count(x => Vector3.Distance(target.ServerPosition, x.ServerPosition) < 175);
                            Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.White, "Hits: " + num);
                            Vector3 pos;
                            PredictionOutput pred = R.GetPrediction(target);
                            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < R.Range)
                            {
                                var test1 = Prediction.GetPrediction(target, R.Instance.SData.SpellCastTime).CastPosition;
                                float movement = target.MoveSpeed * 100 / 1000;
                                if (target.Distance(test1) > movement)
                                {
                                    pos = target.ServerPosition.Extend(Player.ServerPosition.Extend(test1, R.Instance.SData.SpellCastTime * target.MoveSpeed), R.Width);
                                    Render.Circle.DrawCircle(pos, R.Width, Color.Lime, 7);
                                }
                                else
                                {
                                    if (pred.Hitchance >= RHitChance)
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