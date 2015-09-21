using System;
using System.Collections.Generic;
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
    class Rengar : PluginData
    {
        private static IEnumerable<Obj_AI_Hero> AllEnemies;
        public Rengar()
        {
            LoadSpells();
            LoadMenus();
            AllEnemies = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy);
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 500);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R);

            E.SetSkillshot(0.5f, E.Instance.SData.LineWidth, float.MaxValue, true, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);                       
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("C5Stacks", "Empowered").SetValue(new StringList(new[] { "Q", "W", "E" })));
                combomenu.AddItem(new MenuItem("TurretDive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("UseItemCombo", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
                harassmenu.AddItem(new MenuItem("H5Stacks", "Empowered").SetValue(new StringList(new[] { "Q", "W", "E" })));
                config.AddSubMenu(harassmenu);
            }
            var laneclear = new Menu("Farm", "Farm");
            {
                laneclear.AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
                laneclear.AddItem(new MenuItem("UseWFarm", "Use W").SetValue(true));
                laneclear.AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
                laneclear.AddItem(new MenuItem("WFarmValue", "W More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("F5Stacks", "Empowered").SetValue(new StringList(new[] { "Q", "W", "E" })));
                config.AddSubMenu(laneclear);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
                junglemenu.AddItem(new MenuItem("J5Stacks", "Empowered").SetValue(new StringList(new[] { "Q", "W", "E" })));
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EPredHitchance", "E Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("UseQMisc", "Q Turrets").SetValue(false));   
                miscmenu.AddItem(new MenuItem("UseEMisc", "E Gapcloser").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawW", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawE", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawR", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Obj_AI_Hero target = 
                TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ?
                TargetSelector.GetSelectedTarget() :
                AllEnemies.Where(x => x.IsValid<Obj_AI_Hero>() &&
                    Vector3.Distance(ObjectManager.Player.ServerPosition, x.ServerPosition) <= VisionRange &&
                    !x.IsDead && !x.IsZombie && x.IsTargetable && !myUtility.ImmuneToPhysical(x))
                .OrderByDescending(x => myRePriority.ResortDB(x.ChampionName))
                .ThenBy(i => i.Health).FirstOrDefault();
            
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var UseR = config.Item("UseRCombo").GetValue<bool>();
            var CastItems = config.Item("UseItemCombo").GetValue<bool>();
            if (target != null && target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (target.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                if (myUtility.ImmuneToPhysical(target)) return;
                if (!Stealth && mySmiter.CanSmiteChampions(target)) mySmiter.Smites(target);                
                try
                {
                    if (UseR && R.IsReady() && !Stealth)
                    {
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= LeapRange) return;
                        var dist = Vector3.Distance(Player.ServerPosition, target.ServerPosition);
                        var msDif = Player.MoveSpeed - target.MoveSpeed;
                        if (msDif < 0 && dist <= VisionRange && CastItems && Items.HasItem(3142) && Items.CanUseItem(3142))
                        {
                            myUtility.UseItems(0, null);
                            R.Cast();
                        }
                        else if (msDif > 0 && dist <= VisionRange)
                        {
                            R.Cast();
                        }
                    }
                    if (Ferocity == 5)
                    {
                        switch (config.Item("C5Stacks").GetValue<StringList>().SelectedIndex)
                        {
                            case 0:
                                if (UseQ && Q.IsReady() && !Savagery)
                                {
                                    if (myUtility.TickCount - JumpTime < (700 + Game.Ping))
                                    {
                                        Q.Cast();
                                        if (CastItems) myUtility.UseItems(0, null);
                                    }
                                    else if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= 150f)
                                    {
                                        if (CastItems) myUtility.UseItems(0, target);
                                        Q.Cast();
                                    }
                                }
                                break;
                            case 1:
                                if (UseW && W.IsReady() && 
                                    Vector3.Distance(Player.ServerPosition, target.ServerPosition) < W.Range * 1 / 3)
                                {
                                    W.Cast();
                                    if (CastItems) myUtility.UseItems(0, target);
                                }                                
                                break;
                            case 2:                                
                                if (UseE && E.IsReady())
                                {
                                    if (myUtility.TickCount - JumpTime < (700 + Game.Ping))
                                    {
                                        if (CastItems) myUtility.UseItems(0, null);
                                        EPredict(target);
                                    }
                                    else
                                    {
                                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= LeapRange)
                                        {
                                            if (CastItems) myUtility.UseItems(0, target);
                                            EPredict(target);
                                        }
                                    }
                                }
                                break;
                        }
                        if (CastItems)
                        {
                            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= 450f)
                            {
                                myUtility.UseItems(1, target);
                            }
                        }
                    }
                    if (Ferocity <= 4 && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= 500)
                    {
                        if (CastItems) myUtility.UseItems(0, target);
                        if (UseQ && Q.IsReady() && !Savagery)
                        {
                            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= 150f)
                            {
                                Q.Cast();
                            }
                        }
                        if (UseW && W.IsReady() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < W.Range * 1 / 3)
                        {
                            W.Cast();
                        }
                        if (UseE && E.IsReady())
                        {
                            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= LeapRange)
                            {
                                EPredict(target);
                            }
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
                if (Ferocity <= 4)
                {
                    if (UseQ && Q.IsReady())
                    {
                        if (target.UnderTurret(true)) return;
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= 150)
                        {
                            Q.Cast();
                        }
                    }
                    if (UseW && W.IsReady())
                    {
                        if (target.UnderTurret(true) && Player.UnderTurret(true)) return;
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < W.Range * 2 / 3)
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
                if (Ferocity >= 5)
                {
                    switch (config.Item("H5Stacks").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            if (UseQ && Q.IsReady())
                            {
                                if (target.UnderTurret(true)) return;
                                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= 150)
                                {
                                    Q.Cast();
                                }
                            }
                            break;
                        case 1:
                            if (UseW && W.IsReady())
                            {
                                if (target.UnderTurret(true) && Player.UnderTurret(true)) return;
                                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < W.Range * 2 / 3)
                                {
                                    W.Cast();
                                }
                            }
                            break;
                        case 2:
                            if (UseE && E.IsReady())
                            {
                                if (target.UnderTurret(true) && Player.UnderTurret(true)) return;
                                EPredict(target);
                            }
                            break;
                    }
                }
            }    
        }
        private void LaneClear()
        {
            if (Ferocity <= 4)
            {
                if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && !myOrbwalker.IsWaiting() && !Savagery)
                {
                    var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                    var siegeQ = myUtility.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                    if (siegeQ != null && siegeQ.IsValidTarget())
                    {
                        Q.Cast();
                        Player.IssueOrder(GameObjectOrder.AttackUnit, siegeQ);
                    }
                    else
                    {
                        var selectQ = allMinionsQ.Where(x => Q.IsKillable(x) && Player.GetAutoAttackDamage(x) < x.Health).OrderBy(i => i.Distance(Player)).FirstOrDefault();
                        if (selectQ != null && selectQ.IsValidTarget())
                        {
                            Q.Cast();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, selectQ);
                        }
                    }
                }
                if (config.Item("UseWFarm").GetValue<bool>() && W.IsReady() && !myOrbwalker.IsWaiting())
                {
                    if (Player.UnderTurret(true)) return;
                    var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range);
                    if (allMinionsW == null) return;                    
                    if (allMinionsW.Count > config.Item("WFarmValue").GetValue<Slider>().Value && !myUtility.IsInBush(Player) && !Player.IsDashing())
                    {
                        W.Cast();
                    }
                }
                if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && !myOrbwalker.IsWaiting() && !myUtility.IsInBush(Player))
                {
                    var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                    var SelectE = allMinionsE.OrderBy(i => i.Distance(Player)).FirstOrDefault();
                    if (SelectE != null && SelectE.IsValidTarget())
                    {
                        if (myUtility.IsFacing(Player, SelectE.ServerPosition)) E.Cast(SelectE.ServerPosition);
                    }
                }
            }
           
            if (Ferocity == 5)
            {
                switch (config.Item("F5Stacks").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && !myOrbwalker.IsWaiting() && !Savagery)
                        {
                            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                            var siegeQ = myUtility.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                            if (siegeQ != null && siegeQ.IsValidTarget())
                            {
                                Q.Cast();
                                Player.IssueOrder(GameObjectOrder.AttackUnit, siegeQ);
                            }
                            else
                            {
                                var selectQ = allMinionsQ.Where(x => Q.IsKillable(x) && Player.GetAutoAttackDamage(x) < x.Health).OrderBy(i => i.Distance(Player)).FirstOrDefault();
                                if (selectQ != null && selectQ.IsValidTarget())
                                {
                                    Q.Cast();
                                    Player.IssueOrder(GameObjectOrder.AttackUnit, selectQ);
                                }
                            }
                        }
                        break;
                    case 1:
                        if (config.Item("UseWFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
                        {
                            if (Player.UnderTurret(true)) return;
                            var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range);
                            if (allMinionsW == null) return;
                            if (allMinionsW.Count >= 5)
                            {
                                W.Cast();
                            }
                        }
                        break;
                    case 2:
                        if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && !myOrbwalker.IsWaiting() && !Player.IsWindingUp && !myUtility.IsInBush(Player) && !Savagery)
                        {
                            var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                            var SelectE = allMinionsE.OrderBy(i => i.Distance(Player)).FirstOrDefault();
                            if (SelectE != null && SelectE.IsValidTarget())
                            {
                                if (myUtility.IsFacing(Player, SelectE.ServerPosition)) E.Cast(SelectE.ServerPosition);
                            }
                        }
                        break;
                }
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, LeapRange, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myUtility.GetLargeMonsters(LeapRange).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (Ferocity <= 4)
                {
                    if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp && Q.IsInRange(mob))
                    {
                        if (largemobs != null)
                        {
                            Q.Cast();
                        }
                        Q.Cast();
                    }
                    if (config.Item("UseWJFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
                    {
                        if (largemobs != null && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) <= W.Range * 2/3)
                        {
                            W.Cast();
                        }
                        else if (Vector3.Distance(Player.ServerPosition, mob.ServerPosition) <= W.Range * 2/3) W.Cast();
                    }
                    if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
                    {                      
                        if (largemobs != null && myUtility.TickCount - JumpTime < (700 + Game.Ping))
                        {
                            PredictionOutput pred = E.GetPrediction(largemobs);
                            if (pred.CollisionObjects.Count == 0)
                            {
                                if (myUtility.IsFacing(Player, largemobs.ServerPosition)) E.Cast(largemobs.ServerPosition);
                            }                            
                        }
                        PredictionOutput pred2 = E.GetPrediction(mob);
                        if (pred2.CollisionObjects.Count == 0)
                        {
                            if (myUtility.IsFacing(Player, mob.ServerPosition)) E.Cast(mob.ServerPosition);
                        }
                    }
                }
                if (Ferocity == 5)
                {
                    switch (config.Item("J5Stacks").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            if (largemobs != null && config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp && Q.IsInRange(largemobs))
                            {
                                Q.Cast();
                                if (Orbwalking.InAutoAttackRange(largemobs)) Player.IssueOrder(GameObjectOrder.AttackUnit, largemobs);
                            }
                            break;
                        case 1:
                            if (config.Item("UseWJFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp && myUtility.PlayerHealthPercentage < 90)
                            {
                                if (largemobs != null)
                                {
                                    W.Cast();
                                }
                            }
                            break;
                        case 2:
                            if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp && !Savagery)
                            {
                                if (largemobs != null && myUtility.TickCount - JumpTime < (700 + Game.Ping))
                                {
                                    PredictionOutput pred = E.GetPrediction(largemobs);
                                    if (pred.CollisionObjects.Count == 0)
                                    {
                                        E.Cast(largemobs.ServerPosition);
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }
        
        private bool Passive
        {
            get { return Player.HasBuff("rengarpassivebuff"); }
        }
        private bool Stealth
        {
            get { return Player.HasBuff("rengarr"); }
        }
        private bool SixthThrophyActive
        {
            get { return Player.HasBuff("rengarbushspeedbuff"); }
        }
        private bool Savagery
        {
            get { return Player.Buffs.Any(x => x.Name.Contains("rengarq")); } //get { return Player.HasBuff("rengarqemp") || Player.HasBuff("rengarq"); }
        }
        private int Ferocity
        {
            get { return (int)Player.Mana; }
        }        
        private int AttackRange
        {
            get { return Passive && SixthThrophyActive ? 725 : Passive && !SixthThrophyActive ? 600 : 150; }
        }
        private int LeapRange
        {
            get { return Passive && SixthThrophyActive ? 725 : 600; }
        }
        private int VisionRange
        {
            get { return R.Level > 0 ? 1000 + (R.Level * 1000) : 1000; }
        }
        private int FindRange()
        {
            if (myUtility.IsInBush(Player)) return LeapRange;
            if (Stealth) return VisionRange;
            return AttackRange;
        }
        private int? JumpTime;
        private HitChance EHitChance
        {
            get
            {
                return GetEHitChance();
            }
        }
        private HitChance GetEHitChance()
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
            PredictionOutput pred = E.GetPrediction(target);
            if (pred.CollisionObjects.Count == 0 && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= E.Range)
            {
                Vector3 pos;
                var test1 = Prediction.GetPrediction(target, E.Instance.SData.MissileSpeed).CastPosition;
                float movement = target.MoveSpeed * 100 / 1000;
                if (target.Distance(test1) > movement)
                {
                    pos = target.ServerPosition.Extend(test1, E.Instance.SData.MissileSpeed * target.MoveSpeed);
                    E.Cast(pos);
                }
                else
                {
                    pos = pred.CastPosition;
                    if (pred.Hitchance >= EHitChance) E.Cast(pos);
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
            }
        }
        protected override void OnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            if (sender.IsMe)
            {
                JumpTime = myUtility.TickCount;
            }
        }
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                if (Ferocity <= 4 || Ferocity == 5 && config.Item("F5Stacks").GetValue<StringList>().SelectedIndex == 0)
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
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Minion && args.Target.Team == GameObjectTeam.Neutral && !Passive)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear &&
                    !args.Target.Name.Contains("Mini") &&                   
                    Orbwalking.InAutoAttackRange(args.Target))
                {
                    myUtility.UseItems(2, null);
                }
            } 
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe && Q.IsReady())
            {
                if (target is Obj_AI_Hero && target.IsValidTarget())
                {
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                    {
                        if (config.Item("UseItemCombo").GetValue<bool>())
                        {
                            myUtility.UseItems(2, null);
                        }
                        if (config.Item("UseQCombo").GetValue<bool>())
                        //if (config.Item("UseQCombo").GetValue<bool>() && !Savagery)
                        {
                            Q.Cast();
                        }
                    }
                }
                if (target is Obj_AI_Minion && target.IsValidTarget())
                {
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear)
                    {
                        if (config.Item("UseQFarm").GetValue<bool>() && Ferocity <= 4)
                        {
                            if (Q.IsKillable((Obj_AI_Minion)target))
                            {
                                Q.Cast();
                            }
                        }
                    }
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear)
                    {
                        if (config.Item("UseQJFarm").GetValue<bool>() && Ferocity <= 4)
                        {
                            if (Q.IsKillable((Obj_AI_Minion)target))
                            {
                                Q.Cast();
                            }
                        }
                    }
                }

                if (target is Obj_AI_Turret && target.Team != Player.Team)
                {
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear &&
                        config.Item("UseQMisc").GetValue<bool>() 
                       && Orbwalking.InAutoAttackRange(target))
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
            if (config.Item("DrawR").GetValue<bool>() && R.Level > 0)
            {
                 Render.Circle.DrawCircle(ObjectManager.Player.Position, VisionRange, Color.Fuchsia);
                 var target = AllEnemies.Where(x => x.IsValid<Obj_AI_Hero>() &&
                     Vector3.Distance(ObjectManager.Player.ServerPosition, x.ServerPosition) <= VisionRange &&
                     !x.IsDead && !x.IsZombie && x.IsTargetable && !myUtility.ImmuneToPhysical(x))
                 .OrderByDescending(x => myRePriority.ResortDB(x.ChampionName))
                 .ThenBy(i => i.Health).FirstOrDefault();
                 if (target != null && target.IsValidTarget())
                 {
                     Render.Circle.DrawCircle(target.Position, target.BoundingRadius, Color.Lime, 7);
                 }
            }
        }
    }
}