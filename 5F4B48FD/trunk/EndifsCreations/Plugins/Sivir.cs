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
    class Sivir : PluginData
    {
        public Sivir()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1250);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Sivir.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Sivir.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Sivir.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Sivir.Combo.R", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Sivir.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Sivir.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Sivir.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Sivir.Harass.E", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Sivir.Farm.Q", "Use Q").SetValue(false));
                laneclearmenu.AddItem(new MenuItem("EC.Sivir.Farm.W", "Use W").SetValue(false));
                laneclearmenu.AddItem(new MenuItem("EC.Sivir.Farm.Q.Value", "Q More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Sivir.Farm.W.Value", "W More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Sivir.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Sivir.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Sivir.Jungle.W", "Use W").SetValue(true)); 
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Sivir.QPredHitchance", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("EC.Sivir.Misc.E", "E Spellblock").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Sivir.Draw.Q", "Q").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            var UseQ = config.Item("EC.Sivir.Combo.Q").GetValue<bool>();            
            var UseR = config.Item("EC.Sivir.Combo.R").GetValue<bool>();
            var CastItems = config.Item("EC.Sivir.Combo.Items").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;                
                if (CastItems) { myItemManager.UseItems(0, Target); }
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        mySpellcast.Linear(Target, Q, QHitChance);
                    }                    
                    if (UseR && R.IsReady())
                    {
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) < Q.Range &&
                            Vector3.Distance(Player.ServerPosition, Target.ServerPosition) > Player.AttackRange)
                        {
                            var dist = Vector3.Distance(Player.ServerPosition, Target.ServerPosition);
                            var msDif = Player.MoveSpeed - Target.MoveSpeed;
                            var reachIn = dist / msDif;
                            if (msDif < 0 && reachIn > 3)
                            {
                                R.Cast();
                            }
                            else if (msDif > 0 && reachIn > 4)
                            {
                                R.Cast();
                            }
                        }
                    }
                    if (CastItems)
                    {
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= 450f)
                        {
                            myItemManager.UseItems(1, Target);
                        }
                    }
                }
                catch { }
            }
            
        }
        private void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var UseQ = config.Item("EC.Sivir.Harass.Q").GetValue<bool>();            
            if (target.IsValidTarget() && !Player.IsWindingUp)
            {
                if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                if (UseQ && Q.IsReady() && Q.IsInRange(target))
                {
                    if (myUtility.IsFacing(Player, target.ServerPosition, 60)) mySpellcast.Linear(target, Q, QHitChance); ;
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("EC.Sivir.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (Player.UnderTurret(true)) return;
            if (config.Item("EC.Sivir.Farm.Q").GetValue<bool>() && Q.IsReady())
            {
                var MinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                var QLine = Q.GetLineFarmLocation(MinionsQ);
                if (QLine.Position.IsValid() && QLine.MinionsHit > config.Item("EC.Sivir.Farm.Q.Value").GetValue<Slider>().Value)
                {
                    if (myUtility.IsFacing(Player, QLine.Position.To3D())) Q.Cast(QLine.Position);
                }
            }
            if (config.Item("EC.Sivir.Farm.W").GetValue<bool>() && W.IsReady())
            {
                var MinionsW = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (MinionsW == null) return;
                if (MinionsW.Count > config.Item("EC.Sivir.Farm.W.Value").GetValue<Slider>().Value)
                {                   
                    W.Cast();
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
            if (config.Item("EC.Sivir.Jungle.Q").GetValue<bool>() && Q.IsReady())
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
            switch (config.Item("EC.Sivir.QPredHitchance").GetValue<StringList>().SelectedIndex)
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
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit is Obj_AI_Hero && unit.IsEnemy && !spell.SData.IsAutoAttack() && E.IsReady())
            {
                if ((myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && config.Item("EC.Sivir.Combo.E").GetValue<bool>()) ||
                    (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass && config.Item("EC.Sivir.Harass.E").GetValue<bool>()) ||
                    (config.Item("EC.Sivir.Misc.E").GetValue<bool>())
                    )
                {
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        var box = new Geometry.Polygon.Rectangle(spell.Start, spell.End, Player.BoundingRadius);
                        if (box.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 100))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.CastOnUnit(Player));
                        }
                    }
                    else if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Unit) || spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit)) && spell.Target != null && spell.Target.IsMe)
                    {
                        E.CastOnUnit(Player);
                    }
                    else if (spell.End.Distance(Player.ServerPosition) <= 100)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.CastOnUnit(Player));
                    }
                }
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (config.Item("EC.Sivir.Combo.W").GetValue<bool>() && W.IsReady())
                    {
                        W.Cast();
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Sivir.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
        }
    }
}
