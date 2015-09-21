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
    class Pantheon : PluginData
    {
        public Pantheon()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 5500);

            Q.SetTargetted(0.2f, 1700f);
            W.SetTargetted(0.2f, 1700f);
            E.SetSkillshot(0.25f, 15f * 2 * (float)Math.PI / 180, 2000f, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(R.Instance.SData.SpellCastTime, 700f, R.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotCircle);
           
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Grand Skyfall", "Custom");
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
                combomenu.AddItem(new MenuItem("TDHaveShield", "Shield Check").SetValue(false));
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
                laneclear.AddItem(new MenuItem("QFarmType", "Q").SetValue(new StringList(new[] { "Any", "Only Siege", "Furthest" })));
                laneclear.AddItem(new MenuItem("WFarmType", "W").SetValue(new StringList(new[] { "Any", "Only Siege", "Smart Shield" })));
                laneclear.AddItem(new MenuItem("EFarmType", "E").SetValue(new StringList(new[] { "Any", "Only Siege", "Slider Value" })));
                laneclear.AddItem(new MenuItem("EFarmValue", "(Slider Value) E More Than").SetValue(new Slider(1, 1, 5)));
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
                miscmenu.AddItem(new MenuItem("UseWMisc", "W Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseW2Misc", "W Interrupts ").SetValue(false));
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
                    if (UseQ && Q.IsReady() && Q.IsInRange(target) && !Player.IsDashing())
                    {
                        Q.CastOnUnit(target);
                    }
                    if (UseW && W.IsReady() && W.IsInRange(target) && !QShot)
                    {
                        if (target.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                        if (target.UnderTurret(true) && config.Item("TurretDive").GetValue<bool>() && config.Item("TDHaveShield").GetValue<bool>() && !Shield) return;
                        W.CastOnUnit(target);
                    }
                    if (UseE && E.IsReady())
                    {
                        if (target.HasBuffOfType(BuffType.Stun))
                        {
                            E.Cast(target);
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
            var UseW = config.Item("UseWHarass").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady() && Q.IsInRange(target) && !Player.IsDashing())
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    Q.CastOnUnit(target);
                }
                if (UseW && W.IsReady() && W.IsInRange(target) && !QShot)
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    W.CastOnUnit(target);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (Player.IsCastingInterruptableSpell() || Player.IsChannelingImportantSpell()) return;
            var minions = MinionManager.GetMinions(Player.ServerPosition, Player.AttackRange * 2, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None);
            if (minions.Count >= 3 && !myOrbwalker.IsWaiting() && !Player.IsWindingUp)
            {
                myUtility.UseItems(2, null);
            }
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp && !Player.IsDashing())
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range).ToList();
                switch (config.Item("QFarmType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        var SelectQ = minionQ.FirstOrDefault(x => Q.IsKillable(x) && !Orbwalking.InAutoAttackRange(x));
                        if (SelectQ.IsValidTarget()) Q.CastOnUnit(SelectQ);
                        break;
                    case 1:
                        var SiegeQ = myUtility.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                        if (SiegeQ.IsValidTarget()) Q.CastOnUnit(SiegeQ);
                        break;
                    case 2:
                        var FurthestQ = minionQ.OrderByDescending(i => i.Distance(Player)).FirstOrDefault(x => Q.IsKillable(x) && !Orbwalking.InAutoAttackRange(x));
                        if (FurthestQ.IsValidTarget()) Q.CastOnUnit(FurthestQ);
                        break;
                }
            }
            if (config.Item("UseWFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp && !QShot)
            {
                var minionW = MinionManager.GetMinions(Player.ServerPosition, W.Range).OrderBy(i => i.Distance(Player)).Where(x => !x.UnderTurret(true)).ToList();
                switch (config.Item("WFarmType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        var SelectW = minionW.FirstOrDefault(x => W.IsKillable(x));
                        if (SelectW.IsValidTarget()) W.Cast(SelectW);
                        break;
                    case 1:
                        var siegeW = myUtility.GetLargeMinions(Q.Range).FirstOrDefault(x => W.IsKillable(x));
                        if (siegeW.IsValidTarget()) W.Cast(siegeW);
                        break;
                    case 2:
                        if (PassiveCount() <= 3 && !Shield && minionW[0].IsValidTarget())
                        {
                            W.Cast(minionW[0]);
                        }
                        break;
                }
            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp && !Player.UnderTurret(true))
            {
                var AllE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                switch (config.Item("EFarmType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        //Any
                        if (AllE[0] != null)
                        {
                            E.Cast(AllE[0].ServerPosition);
                        }
                        break;
                    case 1:
                        //Siege
                        var siegeE = myUtility.GetLargeMinions(Q.Range).Where(x => W.IsKillable(x)).ToList();
                        if (siegeE[0] != null)
                        {
                            E.Cast(siegeE[0].ServerPosition);
                        }
                        break;
                    case 2:
                        //Most                        
                        foreach (var x in AllE)
                        {
                            if (E.IsInRange(x) && MinionManager.GetMinions(x.ServerPosition, 300).Count() > config.Item("EFarmValue").GetValue<Slider>().Value)
                            {
                                E.Cast(x.ServerPosition);
                            }
                        }
                        break;
                }
            }   
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myUtility.GetLargeMonsters(Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null && !Player.IsWindingUp)
            {
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && Q.IsInRange(mob) && !Player.IsDashing())
                {
                    if (largemobs != null)
                    {
                        Q.CastOnUnit(largemobs);
                    }
                    else
                    {
                        Q.CastOnUnit(mob);
                    }
                }
                if (config.Item("UseWJFarm").GetValue<bool>() && W.IsReady() && W.IsInRange(mob) && !QShot)
                {
                    if (PassiveCount() <= 3 && !Shield)
                    {
                        if (largemobs != null)
                        {
                            W.CastOnUnit(largemobs);
                        }
                        else
                        {
                            W.CastOnUnit(mob);
                        }
                    }
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady())
                {
                    if (largemobs != null)
                    {
                        E.Cast(largemobs);
                    }
                    else
                    {
                        E.Cast(mob);
                    }
                }
            }
            
        }
        private void Custom()
        {
            if (R.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.IsTargetable && !myUtility.ImmuneToPhysical(x));
                var targets = EnemyList.Where(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range);
                Obj_AI_Hero mandropthis = null;
                switch (config.Item("UseRType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        mandropthis = targets.OrderBy(i => (i.Health / Player.GetAutoAttackDamage(i))).FirstOrDefault();
                        break;
                    case 1:
                        mandropthis = targets.FirstOrDefault(x => R.IsKillable(x));
                        break;
                    case 2:
                        mandropthis = targets.OrderByDescending(z => myRePriority.ResortDB(z.ChampionName)).ThenBy(i => i.Health).FirstOrDefault(x => x.Health < Player.Health);
                        break;
                    case 3:
                        mandropthis = targets.OrderByDescending(i => i.Distance(Player)).FirstOrDefault();             
                        break;
                    case 4:
                        mandropthis = targets.OrderBy(i => i.Health).FirstOrDefault();        
                        break;
                }
                if (mandropthis != null && mandropthis.IsValidTarget())
                {
                    R.Cast(mandropthis.ServerPosition);
                }
            } 
        }

        private bool QShot;
        private bool Shield
        {
            get { return Player.HasBuff("pantheonpassiveshield"); }
        }
        private int PassiveCount()
        {
            foreach (var buffs in Player.Buffs.Where(buffs => buffs.Name == "pantheonpassivecounter"))
            {
                return buffs.Count;
            }
            return 0;
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
        protected override void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender is Obj_SpellMissile)
            {
                var missle = (Obj_SpellMissile)sender;
                if (missle.SpellCaster == Player && missle.SData.Name == "PantheonQ")
                {
                    QShot = true;
                }
            }
        }
        protected override void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender is Obj_SpellMissile)
            {
                var missle = (Obj_SpellMissile)sender;
                if (missle.SpellCaster == Player && missle.SData.Name == "PantheonQ")
                {
                    QShot = false;
                }
            }
        }
        protected override void OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (Player.IsChannelingImportantSpell() || Player.IsCastingInterruptableSpell()) args.Process = false;
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("UseWMisc").GetValue<bool>() && W.IsReady())
            {
                if (Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) < W.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    W.CastOnUnit(gapcloser.Sender);
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("UseW2Misc").GetValue<bool>() && W.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) < W.Range)
                {
                    if (myUtility.ImmuneToCC(sender)) return;
                    W.CastOnUnit(sender);
                }
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (!Player.IsWindingUp && config.Item("UseItemCombo").GetValue<bool>() && Orbwalking.InAutoAttackRange(target))
                    {
                        myUtility.UseItems(2, null);
                    }
                }
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
            if (ObjectManager.Player.IsDead) return;
            if (R.IsReady())
            {
                if (config.Item("UseRDrawDistance").GetValue<bool>())
                {
                    Utility.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 1, 30, true);
                }
                if (config.Item("UseRDrawTarget").GetValue<bool>())
                {
                    var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.IsTargetable && !myUtility.ImmuneToPhysical(x));
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
                        Utility.DrawCircle(drawthis.Position, drawthis.BoundingRadius, Color.Fuchsia, 1, 30, true);
                        Render.Circle.DrawCircle(drawthis.Position, drawthis.BoundingRadius, Color.Lime, 7);
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
        }
    }
}
