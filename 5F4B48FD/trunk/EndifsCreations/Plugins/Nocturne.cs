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
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Nocturne.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Nocturne.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Nocturne.Combo.E", "Use E").SetValue(true));              
                combomenu.AddItem(new MenuItem("EC.Nocturne.Combo.Dive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Nocturne.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Nocturne.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Nocturne.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Nocturne.Harass.E", "Use E").SetValue(true));
                Root.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Nocturne.Farm.Q", "Use Q").SetValue(false));
                laneclearmenu.AddItem(new MenuItem("EC.Nocturne.Farm.Q.Value", "Q More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Nocturne.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                Root.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Nocturne.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Nocturne.Jungle.E", "Use E").SetValue(true)); 
                Root.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Nocturne.QPredHitchance", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("EC.Nocturne.Misc.W", "W Spellblock").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Nocturne.Misc.E", "E Gapcloser").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Nocturne.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Nocturne.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Nocturne.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            var UseQ = Root.Item("EC.Nocturne.Combo.Q").GetValue<bool>();
            var UseE = Root.Item("EC.Nocturne.Combo.E").GetValue<bool>();
            var CastItems = Root.Item("EC.Nocturne.Combo.Items").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;                
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        mySpellcast.Linear(Target, Q, QHitChance);
                    }
                    if (UseE && E.IsReady() && E.IsInRange(Target))
                    {
                        if (myUtility.ImmuneToMagic(Target)) return;
                        E.CastOnUnit(Target);
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
            var UseQ = Root.Item("EC.Nocturne.Harass.Q").GetValue<bool>();
            var UseE = Root.Item("EC.Nocturne.Harass.E").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target.IsValidTarget() && !Player.UnderTurret(true) && !target.UnderTurret(true) && !Player.IsWindingUp)
            {
                if (UseQ && Q.IsReady() && Q.IsInRange(target))
                {
                    mySpellcast.Linear(target, Q, QHitChance);
                }
                if (UseE && E.IsReady() && E.IsInRange(target))
                {
                    E.CastOnUnit(target);
                } 
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < Root.Item("EC.Nocturne.Farm.ManaPercent").GetValue<Slider>().Value) return;
            var minions = MinionManager.GetMinions(Player.ServerPosition, Player.AttackRange * 2, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None);
            if (minions.Count >= 3 && !myOrbwalker.Waiting && !Player.IsWindingUp)
            {
                myItemManager.UseItems(2, null);
            }
            if (Root.Item("EC.Nocturne.Farm.Q").GetValue<bool>() && Q.IsReady())
            {
                var MinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                var QLine = Q.GetLineFarmLocation(MinionsQ);
                if (QLine.Position.IsValid() && Vector3.Distance(Player.ServerPosition, QLine.Position.To3D()) > Player.AttackRange)
                {
                    if (Player.UnderTurret(true)) return;
                    if (QLine.MinionsHit > Root.Item("EC.Nocturne.Farm.Q.Value").GetValue<Slider>().Value)
                    {
                        if (myUtility.IsFacing(Player, QLine.Position.To3D())) Q.Cast(QLine.Position);
                    }
                }
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob == null) return;
            if (Root.Item("EC.Nocturne.Jungle.Q").GetValue<bool>() && Q.IsReady())
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
            if (Root.Item("EC.Nocturne.Jungle.E").GetValue<bool>() && E.IsReady())
            {
                if (largemobs != null && E.IsInRange(largemobs))
                {
                    E.CastOnUnit(largemobs);
                }
            }
            
        }

        private void UpdateR2()
        {
            if (R.Level > 0)
            {
                R2.Range = 1750 + R.Level * 750;
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
            switch (Root.Item("EC.Nocturne.QPredHitchance").GetValue<StringList>().SelectedIndex)
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
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit is Obj_AI_Hero && unit.IsEnemy && !spell.SData.IsAutoAttack() && W.IsReady())
            {
                if ((myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Nocturne.Combo.W").GetValue<bool>()) ||
                    (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass && Root.Item("EC.Nocturne.Harass.W").GetValue<bool>()) ||
                    (Root.Item("EC.Nocturne.Misc.W").GetValue<bool>())
                    )
                {
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        var box = new Geometry.Polygon.Rectangle(spell.Start, spell.End, Player.BoundingRadius);
                        if (box.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 100))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.CastOnUnit(Player));
                        }
                    }
                    else if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Unit) || spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit)) && spell.Target != null && spell.Target.IsMe)
                    {
                        W.CastOnUnit(Player);
                    }
                    else if (spell.End.Distance(Player.ServerPosition) <= 100)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.CastOnUnit(Player));
                    }
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Nocturne.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.CastOnUnit(gapcloser.Sender));
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Root.Item("EC.Nocturne.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(sender) || myUtility.ImmuneToMagic(sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.CastOnUnit(sender));
                }
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (Root.Item("EC.Nocturne.Combo.Items").GetValue<bool>())
                    {
                        myItemManager.UseItems(0, null);
                    }
                }
            }
            if (args.Target is Obj_AI_Minion && args.Target.Team == GameObjectTeam.Neutral)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.JungleClear &&
                    !args.Target.Name.Contains("Mini") &&
                    !Player.IsWindingUp &&
                    Orbwalking.InAutoAttackRange(args.Target))
                {
                    myItemManager.UseItems(2, null);
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
                        myItemManager.UseItems(2, null);
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Nocturne.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Nocturne.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Nocturne.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R2.Range, Color.Fuchsia);
            }
        }
    }
}
