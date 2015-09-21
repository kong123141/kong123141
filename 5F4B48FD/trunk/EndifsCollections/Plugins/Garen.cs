using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Garen : PluginData
    {
        public Garen()
        {
            LoadSpells();
            LoadMenus();
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R,400);            

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
                miscmenu.AddItem(new MenuItem("UseQMisc", "Q Turrets").SetValue(false));                
                miscmenu.AddItem(new MenuItem("UseQ2Misc", "Q Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseQ3Misc", "Q Gapcloser").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawR", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            if (Spin2WinActive) return;
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(Player.AttackRange * 3, TargetSelector.DamageType.Physical);            
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var UseR = config.Item("UseRCombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToPhysical(target)) return;
                try
                {
                    if (UseW && W.IsReady())
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
                    if (UseE && E.IsReady())
                    {
                        if (Orbwalking.InAutoAttackRange(target))
                        {
                            E.Cast();
                        }
                    }
                    if (UseR && R.IsReady())
                    {
                        if (R.IsKillable(target))
                        {
                            R.CastOnUnit(target);
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
            if (Spin2WinActive) return;
            var UseW = config.Item("UseWHarass").GetValue<bool>();
            var UseE = config.Item("UseEHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(Player.AttackRange * 2, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget() && !myOrbwalker.IsWaiting() && !Player.IsWindingUp)
            {
                if (UseW && W.IsReady())
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
                if (UseE && E.IsReady())
                {
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Player.AttackRange*2)
                    {
                        E.Cast();
                    }
                }
            }
        }
        private void LaneClear()
        {
            if (Spin2WinActive) return;
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady())
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
            if (Spin2WinActive) return;
            var mobs = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myUtility.GetLargeMonsters(E.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (config.Item("UseWJFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && Orbwalking.InAutoAttackRange(largemobs))
                    {
                        W.Cast();
                    }
                    if (Orbwalking.InAutoAttackRange(mob)) W.Cast();
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && E.IsInRange(largemobs))
                    {
                        E.Cast();
                    }
                    if (E.IsInRange(mob)) E.Cast();
                }
            }
        }

        private bool Spin2WinActive
        {
            get
            {
                return Player.HasBuff("GarenE");                
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
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (Spin2WinActive) return;
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady())
            {
                var target = minion as Obj_AI_Base;
                if (target != null && Q.IsKillable(target)  && Orbwalking.InAutoAttackRange(target) && !Player.IsWindingUp)
                {
                    Q.Cast();
                }
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe)
            {
                if (Spin2WinActive) return;
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (!Player.IsWindingUp &&  config.Item("UseQCombo").GetValue<bool>() && Orbwalking.InAutoAttackRange(target))
                    {
                        Q.Cast();
                    }
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass)
                {
                    if (!Player.IsWindingUp && config.Item("UseQHarass").GetValue<bool>() && Orbwalking.InAutoAttackRange(target))
                    {
                        Q.Cast();
                    }
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear)
                {
                    if (target is Obj_AI_Minion && target.Team == GameObjectTeam.Neutral && !target.Name.Contains("Mini") &&
                        config.Item("UseQJFarm").GetValue<bool>() && !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                    {
                        Q.Cast();
                    }
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear)
                {
                    if (target is Obj_AI_Turret && target.Team != Player.Team &&
                        config.Item("UseQMisc").GetValue<bool>() &&
                        !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                    {
                        Q.Cast();
                    }
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (sender.IsEnemy)
            {
                if (myUtility.ImmuneToCC(sender)) return;
                if (config.Item("UseQ2Misc").GetValue<bool>() && Q.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.High)
                {
                    if (Vector3.Distance(Player.ServerPosition, sender.ServerPosition) < Player.AttackRange*3)
                    {
                        Q.Cast();
                    }
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("UseQ3Misc").GetValue<bool>() && Q.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) < Q.Range)
                {
                    Q.Cast();
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("DrawR").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, Color.White);
            }
            
        }
    }
}