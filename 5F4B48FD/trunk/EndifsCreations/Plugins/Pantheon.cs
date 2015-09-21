using System;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
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
                custommenu.AddItem(new MenuItem("EC.Pantheon.UseRKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("EC.Pantheon.UseRType", "R").SetValue(new StringList(new[] { "Less Hit", "R Killable", "YOLO", "Furthest", "Lowest HP" })));
                custommenu.AddItem(new MenuItem("EC.Pantheon.UseRTypeLessHit", "Hit <").SetValue(new Slider(4, 1, 10)));
                custommenu.AddItem(new MenuItem("EC.Pantheon.UseRDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("EC.Pantheon.UseRDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Pantheon.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Pantheon.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Pantheon.Combo.E", "Use E").SetValue(true));              
                combomenu.AddItem(new MenuItem("EC.Pantheon.Combo.Dive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Pantheon.TDHaveShield", "Shield Check").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Pantheon.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Pantheon.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Pantheon.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Pantheon.Harass.E", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Pantheon.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Pantheon.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Pantheon.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Pantheon.QFarmType", "Q").SetValue(new StringList(new[] { "Any", "Only Siege", "Furthest" })));
                laneclearmenu.AddItem(new MenuItem("EC.Pantheon.WFarmType", "W").SetValue(new StringList(new[] { "Any", "Only Siege", "Smart Shield" })));
                laneclearmenu.AddItem(new MenuItem("EC.Pantheon.EFarmType", "E").SetValue(new StringList(new[] { "Any", "Only Siege", "Slider Value" })));
                laneclearmenu.AddItem(new MenuItem("EC.Pantheon.Farm.E.Value", "(Slider Value) E More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Pantheon.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Pantheon.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Pantheon.Jungle.W", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Pantheon.Jungle.E", "Use E").SetValue(true)); 
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Pantheon.Misc.W", "W Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Pantheon.Misc.W2", "W Interrupts ").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Pantheon.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Pantheon.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Pantheon.Draw.E", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Physical, true);

            var UseQ = config.Item("EC.Pantheon.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Pantheon.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Pantheon.Combo.E").GetValue<bool>();
            var CastItems = config.Item("EC.Pantheon.Combo.Items").GetValue<bool>();

            if (Target.IsValidTarget())
            {            
                if (CastItems) { myItemManager.UseItems(0, Target); }
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        if (!Player.IsDashing())
                        {
                            if (Orbwalking.InAutoAttackRange(Target))
                            {
                                mySpellcast.Unit(Target,Q);
                            }
                            else 
                            {
                                mySpellcast.Unit(Target, Q);
                            }
                        }
                    }
                    if (UseW && W.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        if (Target.UnderTurret(true) && !config.Item("EC.Pantheon.Combo.Dive").GetValue<bool>()) return;
                        if (Target.UnderTurret(true) && config.Item("EC.Pantheon.Combo.Dive").GetValue<bool>() && config.Item("EC.Pantheon.TDHaveShield").GetValue<bool>() && !Shield) return;
                        mySpellcast.Unit(Target, W);
                    }
                    if (UseE && E.IsReady())
                    {
                        if (myUtility.MovementDisabled(Target))
                        {
                            E.Cast(Target);
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
            var UseQ = config.Item("EC.Pantheon.Harass.Q").GetValue<bool>();
            var UseW = config.Item("EC.Pantheon.Harass.W").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady() && !Player.IsDashing())
                {
                    mySpellcast.Unit(target, Q);
                }
                if (UseW && W.IsReady())
                {
                    mySpellcast.Unit(target, W);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("EC.Pantheon.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (Player.IsCastingInterruptableSpell() || Player.IsChannelingImportantSpell()) return;
            var minions = MinionManager.GetMinions(Player.ServerPosition, Player.AttackRange * 2, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None);
            if (minions.Count >= 3 && !myOrbwalker.IsWaiting() && !Player.IsWindingUp)
            {
                myItemManager.UseItems(2, null);
            }
            if (config.Item("EC.Pantheon.Farm.Q").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp && !Player.IsDashing())
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range).ToList();
                switch (config.Item("EC.Pantheon.QFarmType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        var SelectQ = minionQ.FirstOrDefault(x => Q.IsKillable(x) && !Orbwalking.InAutoAttackRange(x));
                        if (SelectQ.IsValidTarget()) Q.CastOnUnit(SelectQ);
                        break;
                    case 1:
                        var SiegeQ = myFarmManager.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                        if (SiegeQ.IsValidTarget()) Q.CastOnUnit(SiegeQ);
                        break;
                    case 2:
                        var FurthestQ = minionQ.OrderByDescending(i => i.Distance(Player)).FirstOrDefault(x => Q.IsKillable(x) && !Orbwalking.InAutoAttackRange(x));
                        if (FurthestQ.IsValidTarget()) Q.CastOnUnit(FurthestQ);
                        break;
                }
            }
            if (config.Item("EC.Pantheon.Farm.W").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
            {
                var minionW = MinionManager.GetMinions(Player.ServerPosition, W.Range).OrderBy(i => i.Distance(Player)).Where(x => !x.UnderTurret(true)).ToList();
                switch (config.Item("EC.Pantheon.WFarmType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        var SelectW = minionW.FirstOrDefault(x => W.IsKillable(x));
                        if (SelectW.IsValidTarget()) W.Cast(SelectW);
                        break;
                    case 1:
                        var siegeW = myFarmManager.GetLargeMinions(Q.Range).FirstOrDefault(x => W.IsKillable(x));
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
            if (config.Item("EC.Pantheon.Farm.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp && !Player.UnderTurret(true))
            {
                var AllE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                switch (config.Item("EC.Pantheon.EFarmType").GetValue<StringList>().SelectedIndex)
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
                        var siegeE = myFarmManager.GetLargeMinions(Q.Range).Where(x => W.IsKillable(x)).ToList();
                        if (siegeE[0] != null)
                        {
                            E.Cast(siegeE[0].ServerPosition);
                        }
                        break;
                    case 2:
                        //Most                        
                        foreach (var x in AllE)
                        {
                            if (E.IsInRange(x) && MinionManager.GetMinions(x.ServerPosition, 300).Count() > config.Item("EC.Pantheon.Farm.E.Value").GetValue<Slider>().Value)
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
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null && !Player.IsWindingUp)
            {
                if (config.Item("EC.Pantheon.Jungle.Q").GetValue<bool>() && Q.IsReady() && Q.IsInRange(mob) && !Player.IsDashing())
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
                if (config.Item("EC.Pantheon.Jungle.W").GetValue<bool>() && W.IsReady() && W.IsInRange(mob))
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
                if (config.Item("EC.Pantheon.Jungle.E").GetValue<bool>() && E.IsReady())
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
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.IsTargetable && !myUtility.ImmuneToDeath(x));
                var targets = EnemyList.Where(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range);
                Obj_AI_Hero mandropthis = null;
                switch (config.Item("EC.Pantheon.UseRType").GetValue<StringList>().SelectedIndex)
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

        
        private bool Shield
        {
            get { return Player.HasBuff("pantheonpassiveshield"); }
        }
        private bool SpearShot;
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
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "pantheonq") ||  (spell.SData.Name.ToLower() == "pantheonw"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot == SpellSlot.R)
            {
                LastR = myUtility.TickCount;
                mySpellcast.Pause(2000 + Game.Ping);
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (Player.IsChannelingImportantSpell() || myUtility.TickCount - LastR <= 0.5f)
            {
                args.Process = false;
            }
        }       
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("EC.Pantheon.Misc.W").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= W.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.CastOnUnit(gapcloser.Sender));
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("EC.Pantheon.Misc.W2").GetValue<bool>() && W.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= W.Range)
                {
                    if (myUtility.ImmuneToCC(sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.CastOnUnit(sender));
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
                    if (!Player.IsWindingUp && config.Item("EC.Pantheon.Combo.Items").GetValue<bool>() && Orbwalking.InAutoAttackRange(target))
                    {
                        myItemManager.UseItems(2, null);
                    }
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear)
                {
                    if (target is Obj_AI_Minion && target.Team == GameObjectTeam.Neutral && !target.Name.Contains("Mini") &&
                        !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                    {
                        myItemManager.UseItems(2, null);
                    }
                }
            }
        }
        protected override void OnEndScene(EventArgs args)
        {
            if (ObjectManager.Player.IsDead) return;
            if (R.IsReady())
            {
                if (config.Item("EC.Pantheon.UseRDrawDistance").GetValue<bool>())
                {
                    Utility.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 1, 30, true);
                }
                if (config.Item("EC.Pantheon.UseRDrawTarget").GetValue<bool>())
                {
                    var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.IsTargetable && !myUtility.ImmuneToDeath(x));
                    var targets = EnemyList.Where(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range);
                    Obj_AI_Hero drawthis = null;
                    switch (config.Item("EC.Pantheon.UseRType").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            drawthis = targets.OrderBy(i => (i.Health / Player.GetAutoAttackDamage(i))).FirstOrDefault(x => x.Health / Player.GetAutoAttackDamage(x) <= config.Item("EC.Pantheon.UseRTypeLessHit").GetValue<Slider>().Value);
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
            if (config.Item("EC.Pantheon.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Pantheon.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Pantheon.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
        }
    }
}
