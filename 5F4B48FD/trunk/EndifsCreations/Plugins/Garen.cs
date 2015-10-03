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

            myDamageIndicator.DamageToUnit = GetDamage;
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Demacian Justice", "Custom");
            {
                custommenu.AddItem(new MenuItem("EC.Garen.UseAutoR", "Auto").SetValue(true));
                Root.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Garen.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Garen.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Garen.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Garen.Combo.R", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Garen.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Garen.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Garen.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Garen.Harass.E", "Use E").SetValue(true));
                Root.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Garen.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Garen.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Garen.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Garen.Farm.E.Value", "E More Than").SetValue(new Slider(1, 1, 5)));
                Root.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Garen.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Garen.Jungle.W", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Garen.Jungle.E", "Use E").SetValue(true));
                Root.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Garen.Misc.Q", "Q Turrets").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Garen.Misc.Q2", "Q Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Garen.UseQ3Misc", "Q Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Garen.Misc.W", "W Shields").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Garen.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        private void Custom()
        {
           var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToMagic(x) && R.IsKillable(x));
           foreach (var x in EnemyList.OrderByDescending(z => myRePriority.ResortDB(z.ChampionName)))
           {
               R.CastOnUnit(x);
           }
        }
        private void Combo()
        {
            Target = myUtility.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            var UseQ = Root.Item("EC.Garen.Combo.Q").GetValue<bool>();
            //var UseW = Root.Item("EC.Garen.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Garen.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Garen.Combo.R").GetValue<bool>();
            var CastItems = Root.Item("EC.Garen.Combo.Items").GetValue<bool>();

            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;
                if (CastItems) { myItemManager.UseItems(0, Target); }
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        if (!myUtility.ImmuneToCC(Target) && Orbwalking.InAutoAttackRange(Target))
                        {
                            Q.Cast();
                        }
                        var dist = Vector3.Distance(Player.ServerPosition, Target.ServerPosition);
                        var msDif = Player.MoveSpeed - Target.MoveSpeed;
                        var reachIn = dist / msDif;
                        if (reachIn < 5)
                        {
                            Q.Cast();
                        }
                    }
                    /*
                    if (UseW && W.IsReady() && (QActive || EActive))
                    {
                        W.Cast();
                    }*/
                    if (UseE && E.IsReady())
                    {
                        if (UseQ && Q.IsReady()) return;
                        if (EActive) return;
                        if (Orbwalking.InAutoAttackRange(Target) && (!QActive || LastQActive > 4500 && LastQHit > 4500))
                        {
                            E.Cast();    
                        }
                    }
                    if (UseR && R.IsReady())
                    {
                        if (R.IsKillable(Target))
                        {
                            R.CastOnUnit(Target);
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
                catch
                {
                }
            }
        }
        private void Harass()
        {
            if (EActive) return;
            var UseW = Root.Item("EC.Garen.Harass.W").GetValue<bool>();
            var UseE = Root.Item("EC.Garen.Harass.E").GetValue<bool>();
            var target = TargetSelector.GetTarget(Player.AttackRange * 2, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget() && !myOrbwalker.Waiting && !Player.IsWindingUp)
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
            if (EActive) return;
            if (Root.Item("EC.Garen.Farm.E").GetValue<bool>() && E.IsReady())
            {
                var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (allMinionsE == null) return;
                if (allMinionsE.Count > Root.Item("EC.Garen.Farm.E.Value").GetValue<Slider>().Value)
                {
                    if (Player.UnderTurret(true)) return;
                    E.Cast();
                }
            }
        }
        private void JungleClear()
        {
            if (EActive) return;
            var mobs = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, E.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (Root.Item("EC.Garen.Jungle.W").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && Orbwalking.InAutoAttackRange(largemobs))
                    {
                        W.Cast();
                    }
                    if (Orbwalking.InAutoAttackRange(mob)) W.Cast();
                }
                if (Root.Item("EC.Garen.Jungle.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && E.IsInRange(largemobs))
                    {
                        E.Cast();
                    }
                    if (E.IsInRange(mob)) E.Cast();
                }
            }
        }

        private bool EActive
        {
            get
            {
                return Player.HasBuff("GarenE");                
            }
        }
        private bool QActive
        {
            get
            {
                return Player.HasBuff("GarenQ");
            }
        }
        private int LastQActive;
        private int LastQHit;

        private float GetDamage(Obj_AI_Hero target)
        {
            var damage = 0d;
            if (Q.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.Q);
            }
            if (R.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.R);
            }
            return (float)damage;
        }

        protected override void OnUpdate(EventArgs args)
        {            
            if (Player.IsDead)
            {
                myUtility.Reset();
                return;
            }
            if (Root.Item("EC.Garen.UseAutoR").GetValue<bool>())
            {
                Custom();
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
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if (spell.SData.Name.ToLower() == "garenq")
                {
                    LastQActive = myUtility.TickCount;
                }
                if (spell.SData.Name.ToLower() == "garenqattack")
                {
                    LastQHit = myUtility.TickCount;
                }
            }
            if (unit is Obj_AI_Hero && unit.IsEnemy && !spell.SData.IsAutoAttack() && W.IsReady())
            {
                if (Root.Item("EC.Garen.Misc.W").GetValue<bool>() || myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Garen.Combo.W").GetValue<bool>())
                {
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        var box = new Geometry.Polygon.Rectangle(spell.Start, spell.End, Player.BoundingRadius);
                        if (box.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 100))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast());
                        }
                    }
                    else if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Unit) || spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit)) && spell.Target != null && spell.Target.IsMe)
                    {
                        W.Cast();
                    }
                    else if (spell.End.Distance(Player.ServerPosition) <= 100)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast());
                    }
                }
            }
        }
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (EActive) return;
            if (Root.Item("EC.Garen.Farm.Q").GetValue<bool>() && Q.IsReady())
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
                if (EActive) return;
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (Root.Item("EC.Garen.Combo.Q").GetValue<bool>() && Orbwalking.InAutoAttackRange(target))
                    {
                        Q.Cast();
                    }
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass)
                {
                    if (Root.Item("EC.Garen.Harass.Q").GetValue<bool>() && Orbwalking.InAutoAttackRange(target))
                    {
                        Q.Cast();
                    }
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear)
                {
                    if (target is Obj_AI_Minion && target.Team == GameObjectTeam.Neutral && !target.Name.Contains("Mini") &&
                        Root.Item("EC.Garen.Jungle.Q").GetValue<bool>() && !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                    {
                        Q.Cast();
                    }
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear)
                {
                    if (target is Obj_AI_Turret && target.Team != Player.Team &&
                        Root.Item("EC.Garen.Misc.Q").GetValue<bool>() &&
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
                if (Root.Item("EC.Garen.Misc.Q2").GetValue<bool>() && Q.IsReady() && args.DangerLevel == Interrupter2.DangerLevel.High)
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
            if (Root.Item("EC.Garen.UseQ3Misc").GetValue<bool>() && Q.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= Q.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;
                    Q.Cast();
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Garen.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.White);
            }
            
        }
    }
}