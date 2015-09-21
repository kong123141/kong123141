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
    class Khazix : PluginData
    {
        public Khazix()
        {
            LoadSpells();
            LoadMenus();
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 325);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 1200);

            W.SetSkillshot(0.225f, 100f, 828.5f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 100f, 1000f, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Double Jump", "Custom");
            {
                custommenu.AddItem(new MenuItem("DoubleJumpKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("DoubleJumpDirection", "Jump Back to").SetValue(new StringList(new[] { "Start Direction", "Cursor Direction", "Continue Direction" })));
                custommenu.AddItem(new MenuItem("DoubleJumpDrawTargets", "Draw Targets").SetValue(true));
                custommenu.AddItem(new MenuItem("DoubleJumpDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("TurretDive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("UseItemCombo", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclear = new Menu("Farm", "Farm");
            {
                laneclear.AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
                laneclear.AddItem(new MenuItem("UseWFarm", "Use W").SetValue(true));
                laneclear.AddItem(new MenuItem("WFarmValue", "W More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("FarmMana", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclear);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true)); 
                config.AddSubMenu(junglemenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawW", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawE", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawR", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
                    
        }
        
        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(E.Range * 2, TargetSelector.DamageType.Physical);            
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var UseR = config.Item("UseRCombo").GetValue<bool>();
            var CastItems = config.Item("UseItemCombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToPhysical(target)) return;                
                if (CastItems && !Stealth)
                {
                    myUtility.UseItems(0, target);
                }
                if (UseR && R.IsReady() && !Player.IsDashing() && !Stealth)
                {
                    var dist = Vector3.Distance(Player.ServerPosition, target.ServerPosition);
                    var msDif = Player.MoveSpeed - target.MoveSpeed;
                    var reachIn = dist / msDif;
                    if (msDif < 0 && reachIn > 2 && R.IsReady())
                    {
                        R.Cast();
                    }
                    else if (msDif > 0 && R.IsReady() && !Passive)
                    {
                        R.Cast();
                    }
                }
                try
                {
                    if (UseE && E.IsReady() && !Orbwalking.InAutoAttackRange(target) && myUtility.TickCount - LastCast > EDelay)
                    {                                           
                        PredictionOutput pred = E.GetPrediction(target);
                        if (pred.Hitchance >= HitChance.High && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < E.Range)
                        {
                            var test1 = Prediction.GetPrediction(target, E.Instance.SData.SpellCastTime).CastPosition;
                            float movement = target.MoveSpeed * 100 / 1000;
                            if (target.Distance(test1) > movement)
                            {
                                var mcast = Player.ServerPosition.Extend(target.ServerPosition.Extend(test1, E.Instance.SData.MissileSpeed * target.MoveSpeed), E.Range);
                                if (mcast.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                                if (mcast.UnderTurret(true) && config.Item("TurretDive").GetValue<bool>() && myUtility.PlayerHealthPercentage < 25 && !EvolvedE) return;
                                E.Cast(mcast);                                
                            }
                            else
                            {
                                var v1 = target.Distance(Player);
                                var v2 = target.Distance(pred.CastPosition);
                                var scast = ExtendedE(target.Position, Math.Abs(v1 + v2 / 2));
                                if (scast.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                                if (scast.UnderTurret(true) && config.Item("TurretDive").GetValue<bool>() && myUtility.PlayerHealthPercentage < 25 && !EvolvedE) return;
                                E.Cast(scast);
                            }
                        }
                    }
                    if (UseQ && Q.IsReady())
                    {
                        if (Airborne)
                        {
                            Q.CastOnUnit(target);
                            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < 500f) myUtility.UseItems(2, target);
                        }
                        else if (Landed && Q.IsInRange(target))
                        {
                            Q.CastOnUnit(target);
                            if (Orbwalking.InAutoAttackRange(target)) Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                        }
                    }
                    if (UseW && W.IsReady())
                    {
                        WPredict(target);
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
            var UseW = config.Item("UseWHarass").GetValue<bool>();
            if (UseQ && Q.IsReady())
            {
                var targetQ = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (targetQ != null && targetQ.IsValidTarget() && Q.IsInRange(targetQ))
                {
                    if (targetQ.UnderTurret(true) && Player.UnderTurret(true)) return;
                    Q.CastOnUnit(targetQ);
                    if (Orbwalking.InAutoAttackRange(targetQ)) Player.IssueOrder(GameObjectOrder.AttackUnit, targetQ);
                }
            }
            if (UseW && W.IsReady())
            {
                var targetW = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                if (targetW == null || !targetW.IsValidTarget()) return;
                if (targetW.UnderTurret(true) && Player.UnderTurret(true)) return;
                if (Vector3.Distance(Player.ServerPosition, targetW.ServerPosition) < W.Range)
                {
                    WPredict(targetW);
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
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp && !myOrbwalker.IsWaiting())
            {
                var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                var siegeQ = myUtility.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                if (siegeQ != null && siegeQ.IsValidTarget())
                {
                    Q.CastOnUnit(siegeQ);
                }
                else
                {
                    var selectQ = allMinionsQ.Where(x => Q.IsKillable(x) && Player.GetAutoAttackDamage(x) < x.Health).OrderByDescending(i => i.Distance(Player)).FirstOrDefault();
                    if (selectQ != null && selectQ.IsValidTarget())
                    {
                        Q.CastOnUnit(selectQ);
                    }
                }
            }
            if (config.Item("UseWFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp && !myOrbwalker.IsWaiting())
            {
                var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range);
                if (allMinionsW == null) return;
                foreach (var x in allMinionsW)
                {
                    var InW = MinionManager.GetMinions(x.ServerPosition, 250f).Count();
                    if (InW > config.Item("WFarmValue").GetValue<Slider>().Value && x.IsValidTarget())
                    {
                        if (myUtility.IsFacing(Player,x.ServerPosition)) W.Cast(x.ServerPosition);
                    }
                    else if (InW >= 0 && x.IsValidTarget() && Orbwalking.InAutoAttackRange(x) && W.IsKillable(x) && myUtility.PlayerHealthPercentage < 90)
                    {
                        if (myUtility.IsFacing(Player, x.ServerPosition)) W.Cast(x.ServerPosition);
                    }
                }
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myUtility.GetLargeMonsters(W.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {                
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null)
                    {
                        if (Airborne)
                        {
                            Q.CastOnUnit(largemobs);
                        }
                        else if (Landed && Q.IsInRange(largemobs))
                        {
                            Q.CastOnUnit(largemobs);
                            if (Orbwalking.InAutoAttackRange(largemobs)) Player.IssueOrder(GameObjectOrder.AttackUnit, largemobs);
                        }
                    }
                    else
                    {
                        if (Airborne)
                        {
                            Q.CastOnUnit(mob);
                        }
                        else if (Landed && Q.IsInRange(mob))
                        {
                            Q.CastOnUnit(mob);
                            if (Orbwalking.InAutoAttackRange(mob)) Player.IssueOrder(GameObjectOrder.AttackUnit, mob);
                        }
                    }
                }
                if (config.Item("UseWJFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null)
                    {
                        W.Cast(largemobs.ServerPosition);
                        if (Orbwalking.InAutoAttackRange(largemobs)) Player.IssueOrder(GameObjectOrder.AttackUnit, largemobs);
                    }
                    W.Cast(mob.ServerPosition);
                    if (Orbwalking.InAutoAttackRange(mob)) Player.IssueOrder(GameObjectOrder.AttackUnit, mob);
                }
            }            
        }
        private void Custom()
        {
            if (!EvolvedE) return;           
            var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.IsTargetable && !myUtility.ImmuneToPhysical(x));
            var target = TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : EnemyList.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) < (E.Range + (Q.Range * 1 / 2)) && TestDamage(x) > x.Health).OrderByDescending(z => myRePriority.ResortDB(z.ChampionName)).ThenBy(i => i.Health).FirstOrDefault();
            if (Assasinate == null && target != null)
            {
                Assasinate = target;
                myOrbwalker.SetForcedTarget(target);
                JumpStart = Vector3.Zero;
                JumpEnd = Vector3.Zero;
                JumpTime = null;
                JumpBool = false;
            }
            if (Assasinate != null)
            {
                if (!JumpBool)
                {
                    if (Q.IsReady() && E.IsReady() && Player.Mana >= (Q.Instance.ManaCost + E.Instance.ManaCost))
                    {
                        if (!Passive && Player.Mana >= (Q.Instance.ManaCost + E.Instance.ManaCost + R.Instance.ManaCost) && R.IsReady())
                        {
                            R.Cast();
                        }
                        var newcas = ExtendedE(Assasinate.ServerPosition, E.Range + (Q.Range * 1 / 2));
                        E.Cast(newcas);
                        myUtility.UseItems(0, null);
                        JumpBool = true;
                    }
                }
                if (JumpBool)
                {                    
                    try
                    {
                        if ((myUtility.TickCount - JumpTime < (700 + Game.Ping)))
                        {
                            if (mySmiter.CanSmiteChampions(target)) mySmiter.Smites(Assasinate);
                            Q.CastOnUnit(Assasinate);
                            myUtility.UseItems(1, target);
                            if (Assasinate.IsDead)
                            {
                                switch (config.Item("DoubleJumpDirection").GetValue<StringList>().SelectedIndex)
                                {
                                    case 0:
                                        E.Cast(ExtendedE(JumpStart, (E.Range + (Q.Range * 1 / 2))));
                                        break;
                                    case 1:
                                        E.Cast(ExtendedE(Game.CursorPos, (E.Range + (Q.Range * 1 / 2))));
                                        break;
                                    case 2:
                                        E.Cast(ExtendedE(JumpEnd, (E.Range + (Q.Range * 1 / 2))));
                                        break;
                                }
                            }
                        }
                        else if ((myUtility.TickCount - JumpTime > (700 + Game.Ping)) )
                        {
                            if (!Assasinate.IsDead && W.IsReady() && W.IsKillable(Assasinate) && !E.IsReady())
                            {
                                W.Cast(Assasinate.ServerPosition);
                            }
                            if (Assasinate.IsDead && E.IsReady())
                            {
                                switch (config.Item("DoubleJumpDirection").GetValue<StringList>().SelectedIndex)
                                {
                                    case 0:
                                        E.Cast(ExtendedE(JumpStart, (E.Range + (Q.Range * 1 / 2))));
                                        break;
                                    case 1:
                                        E.Cast(ExtendedE(Game.CursorPos, (E.Range + (Q.Range * 1 / 2))));
                                        break;
                                    case 2:
                                        E.Cast(ExtendedE(JumpEnd, (E.Range + (Q.Range * 1 / 2))));
                                        break;
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        private bool EvolvedQ;
        private bool EvolvedW;
        private bool EvolvedE;
        private Obj_AI_Hero Assasinate;
        private int LastCast;
        private int EDelay = 3000;
        private Vector3 JumpStart, JumpEnd;
        private int? JumpTime;
        private bool JumpBool;
        private bool Airborne;
        private bool Landed = true;        
        private void WPredict(Obj_AI_Hero target)
        {
            PredictionOutput pred = W.GetPrediction(target);
            if (pred.CollisionObjects.Count == 0 && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < W.Range)
            {
                var test1 = Prediction.GetPrediction(target, W.Instance.SData.SpellCastTime).CastPosition;
                float movement = target.MoveSpeed * 100 / 1000;
                if (target.Distance(test1) > movement) W.Cast(target.ServerPosition.Extend(test1, W.Instance.SData.SpellCastTime * target.MoveSpeed));
                else
                {
                    if (pred.Hitchance >= HitChance.High) W.Cast(pred.CastPosition);
                }
            }
        }
        private bool Passive
        {
            get { return Player.HasBuff("khazixpdamage"); }
        }
        private bool Stealth
        {
            get { return Player.HasBuff("khazixrstealth");}
        }
        private void EvolvedSpell()
        {
            if (Player.HasBuff("khazixqevo", true))
            {
                EvolvedQ = true;
                Q.Range = 375;
            }
            if (Player.HasBuff("khazixwevo", true))
            {
                EvolvedW = true;
                W.SetSkillshot(0.225f, 15f * 2 * (float)Math.PI / 180, 828.5f, true, SkillshotType.SkillshotCone);
            }
            if (Player.HasBuff("khazixeevo", true))
            {
                EvolvedE = true;
                E.Range = 1000;
            }
        }
        private void JumpStatus()
        {
            if (Player.IsDashing()) Airborne = true;
            else Airborne = false;
        }
        private double GetQDamage(Obj_AI_Hero target)
        {
            var damage = 0d;
            if (EvolvedQ)
            {
                if (Isolated(target))
                {
                    damage += Player.GetSpellDamage(target, SpellSlot.Q, 3);
                }
                else damage += Player.GetSpellDamage(target, SpellSlot.Q, 2);
            }
            else
            {
                if (Isolated(target))
                {
                    damage += Player.GetSpellDamage(target, SpellSlot.Q, 1);
                }
                else damage += Player.GetSpellDamage(target, SpellSlot.Q, 0);
            }
            return damage;
        }
        private double TestDamage(Obj_AI_Hero target)
        {
            var t1 = Player.GetSpellDamage(target, SpellSlot.Q,EvolvedQ ? 2 : 0);
            t1 += myUtility.CastItemsDamage(target);
            if (mySmiter.CanSmiteChampions(target)) t1 += mySmiter.SmiteDamageChampions;
            return t1;
        }
        private bool Isolated(Obj_AI_Hero target)
        {
            var Selects = HeroManager.Enemies.Where(x => x.NetworkId != target.NetworkId && Vector3.Distance(target.ServerPosition, x.ServerPosition) < 425 && !x.IsMe).ToArray();
            return !Selects.Any();
        }        
        private Vector3 ExtendedE(Vector3 posTarget, float modifier)
        {
            var newRange = Player.ServerPosition.Extend(new Vector3(posTarget.X, posTarget.Y, posTarget.Z), modifier);
            return newRange;
        }
        
        protected override void OnUpdate(EventArgs args)
        {
            EvolvedSpell();
            JumpStatus();
            if (Player.IsDead)
            {                
                myOrbwalker.UnlockTarget();
                myUtility.Reset();
                Assasinate = null;
                return;
            }
            switch (myOrbwalker.ActiveMode)
            {
                case myOrbwalker.OrbwalkingMode.None:                    
                    myOrbwalker.UnlockTarget();
                    myUtility.Reset();
                    Assasinate = null;
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
        protected override void OnDash(Obj_AI_Base sender, Dash.DashItem args)
        {
            if (sender.IsMe)
            {
                JumpStart = args.StartPos.To3D();
                JumpEnd = args.EndPos.To3D();
                JumpTime = myUtility.TickCount;
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell) 
        {
            if (unit.IsMe)
            {
                if (spell.SData.Name.ToLower() == "khazixq")
                {
                    myOrbwalker.ResetAutoAttackTimer();
                }
                if (spell.SData.Name.ToLower() == "khazixqlong")
                {
                    myOrbwalker.ResetAutoAttackTimer();
                }
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.E)
            {
                LastCast = myUtility.TickCount;
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
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Landed)
                {
                    if (!Player.IsWindingUp && config.Item("UseQCombo").GetValue<bool>() && Orbwalking.InAutoAttackRange(target))
                    {
                        Q.CastOnUnit((Obj_AI_Hero)target);
                    }
                    if (!Player.IsWindingUp && config.Item("UseItemCombo").GetValue<bool>() && Orbwalking.InAutoAttackRange(target))
                    {
                        myUtility.UseItems(2, null);                        
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
            if (config.Item("DrawR").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, Color.White);
            }
            if (Q.IsReady() && E.IsReady() && EvolvedE)
            {
                if (config.Item("DoubleJumpDrawDistance").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, E.Range, Color.Fuchsia, 7);
                    Render.Circle.DrawCircle(Player.Position, E.Range + Q.Range, Color.Fuchsia, 7);
                }
                if (config.Item("DoubleJumpDrawTargets").GetValue<bool>())
                {
                    var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.IsTargetable && !myUtility.ImmuneToPhysical(x));
                    var targets = EnemyList.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) < (E.Range + (Q.Range * 1 / 2)) && TestDamage(x) > x.Health).OrderByDescending(z => myRePriority.ResortDB(z.ChampionName)).ThenBy(i => i.Health);
                    foreach (var x in targets)
                    {
                        Render.Circle.DrawCircle(x.Position, x.BoundingRadius, Color.Lime, 7);
                    }
                }
            }
        }
    }
}
