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
    class Sejuani : PluginData
    {
        public Sejuani()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 650);            
            W = new Spell(SpellSlot.W, 350);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R, 1175);

            Q2 = new Spell(SpellSlot.Q, 650);
            
            Q.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed, true, SkillshotType.SkillshotLine);            
            R.SetSkillshot(250, 110, 1600, true, SkillshotType.SkillshotLine);

            Q2.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellList.Add(Q2);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Sejuani.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Sejuani.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Sejuani.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Sejuani.Combo.Dive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Sejuani.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Sejuani.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Sejuani.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Sejuani.Harass.E", "Use E").SetValue(true));
                Root.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Sejuani.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Sejuani.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Sejuani.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Sejuani.QFarmType", "Q").SetValue(new StringList(new[] { "Slider Value", "Furthest" })));
                laneclearmenu.AddItem(new MenuItem("EC.Sejuani.Farm.Q.Value", "Q More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Sejuani.EFarmType", "E").SetValue(new StringList(new[] { "Any", "Most", "Only Siege" })));
                laneclearmenu.AddItem(new MenuItem("EC.Sejuani.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                Root.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Sejuani.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Sejuani.Jungle.W", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Sejuani.Jungle.E", "Use E").SetValue(true));
                Root.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Sejuani.QRPredHitchance", "Q/R Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("EC.Sejuani.Misc.Q", "Q Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Sejuani.Misc.W", "W Turrets").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Sejuani.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Sejuani.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Sejuani.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Sejuani.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            var UseQ = Root.Item("EC.Sejuani.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Sejuani.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Sejuani.Combo.E").GetValue<bool>();
            var CastItems = Root.Item("EC.Sejuani.Combo.Items").GetValue<bool>();
            if (UseW && W.IsReady())
            {                
                mySpellcast.PointBlank(null, W, 350);
            }
            if (UseE && E.IsReady() && !Player.IsDashing() && Player.CountEnemiesInRange(E.Range) > 0)
            {
                if (Target.IsValidTarget() && Target.HasBuff("SejuaniFrost")) E.Cast();
                else
                {
                    var Frosted = HeroManager.Enemies.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= E.Range && x.HasBuff("SejuaniFrost"));
                    if (Frosted.Count() > 1) E.Cast();
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;                
                if (CastItems) { myItemManager.UseItems(0, Target); }
                try
                {
                    if (UseQ && Q.IsReady() && !Player.IsWindingUp && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= Q.Range)
                    {
                        if (Target.ServerPosition.UnderTurret(true) && !Root.Item("EC.Sejuani.Combo.Dive").GetValue<bool>()) return;
                        QPredict(Target);
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
            var UseQ = Root.Item("EC.Sejuani.Harass.Q").GetValue<bool>();
            var UseE = Root.Item("EC.Sejuani.Harass.E").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady() && !Player.IsWindingUp && !target.UnderTurret(true) && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q.Range)
                {
                    QPredict(target);
                }
                if (UseE && E.IsReady() && target.HasBuff("SejuaniFrost") && !Player.IsWindingUp && !Player.IsDashing() && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < 1000)
                {
                    E.Cast();
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < Root.Item("EC.Sejuani.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (Root.Item("EC.Sejuani.Farm.Q").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp && !Player.IsDashing())
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range).ToList();
                switch (Root.Item("EC.Sejuani.QFarmType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:                       
                        var QLine = Q2.GetLineFarmLocation(minionQ, Q.Width);
                        if (QLine.Position.IsValid() && !QLine.Position.To3D().UnderTurret(true))
                        {
                            if (QLine.MinionsHit > Root.Item("EC.Sejuani.Farm.Q.Value").GetValue<Slider>().Value) Q2.Cast(QLine.Position);
                        }
                        break;
                    case 1:
                        var FurthestQ = minionQ.OrderByDescending(i => i.Distance(Player)).FirstOrDefault(x => !x.UnderTurret(true));
                        if (FurthestQ != null && FurthestQ.Position.IsValid() && !Orbwalking.InAutoAttackRange(FurthestQ))
                        {
                            Q2.Cast(FurthestQ.Position);
                        }
                        break;
                }
            }
            if (Root.Item("EC.Sejuani.Farm.W").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
            {
                W.Cast();
            }
            if (Root.Item("EC.Sejuani.Farm.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp && !Player.UnderTurret(true))
            {
                var minionE = MinionManager.GetMinions(Player.ServerPosition, E.Range).ToList();
                switch (Root.Item("EC.Sejuani.EFarmType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        //Any
                        var AnyMinionsE = minionE.Count(x => E.IsKillable(x) && x.HasBuff("sejuanifrost"));
                        if (AnyMinionsE > 0)
                        {
                            E.Cast();
                        }
                        break;
                    case 1:
                        //Most
                        var frostminionE = minionE.Count(x => x.HasBuff("sejuanifrost"));
                        if (frostminionE >= minionE.Count() * 1/2)
                        {
                            E.Cast();
                        }
                        break;
                    case 2:
                        //Siege
                        var siegeE = myFarmManager.GetLargeMinions(E.Range).Count(x => x.HasBuff("sejuanifrost") && E.IsKillable(x));
                        if (siegeE > 0)
                        {
                            E.Cast();
                        }
                        break;
                }
            }   
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q2.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, Q2.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (Root.Item("EC.Sejuani.Jungle.Q").GetValue<bool>() && Q2.IsReady() && Q2.IsInRange(mob))
                {
                    if (largemobs != null)
                    {
                        Q2.Cast(Player.ServerPosition.Extend(largemobs.ServerPosition, Q2.Range));
                    }
                    else
                    {
                        Q2.Cast(Player.ServerPosition.Extend(mob.ServerPosition, Q2.Range));
                    }
                }
                if (Root.Item("EC.Sejuani.Jungle.E").GetValue<bool>() && E.IsReady() && !Player.IsDashing())
                {
                    var mobsE = mobs.Count(x => x.HasBuff("sejuanifrost") && E.IsInRange(x));
                    if (largemobs != null && largemobs.HasBuff("sejuanifrost") && E.IsKillable(largemobs))
                    {
                        E.Cast();
                    }
                    else if (mobsE > 0)
                    {
                        E.Cast();
                    }
                }
            }            
        }

        private HitChance QRHitChance
        {
            get
            {
                return GetQRHitChance();
            }
        }
        private HitChance GetQRHitChance()
        {
            switch (Root.Item("EC.Sejuani.QRPredHitchance").GetValue<StringList>().SelectedIndex)
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

        private void QPredict(Obj_AI_Hero target)
        {
            PredictionOutput pred = Q.GetPrediction(target);
            if (pred.CollisionObjects.Count == 0 && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= Q.Range)
            {
                if (pred.Hitchance >= QRHitChance)
                {
                    Q.Cast(Player.ServerPosition.Extend(pred.CastPosition, Q.Width + target.BoundingRadius));
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
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Sejuani.Misc.Q").GetValue<bool>() && Q.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= Q.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;                    
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => QPredict(gapcloser.Sender));
                }
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass)
                {
                    if (Root.Item("EC.Sejuani.Harass.W").GetValue<bool>() &&
                        !Player.IsWindingUp &&
                        W.IsReady() &&
                        target.IsValidTarget()) W.Cast();
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear)
                {
                    if (target is Obj_AI_Turret && target.Team != Player.Team &&
                        Root.Item("EC.Sejuani.Misc.W").GetValue<bool>() &&
                        !Player.IsWindingUp && Orbwalking.InAutoAttackRange(target))
                    {
                        W.Cast();
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Sejuani.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Sejuani.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Sejuani.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Sejuani.Draw.R").GetValue<bool>() && R.Level > 0 && R.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
                var tomouse = Player.ServerPosition.Extend(Game.CursorPos, Vector3.Distance(Player.ServerPosition, Game.CursorPos));
                var tomax = Player.ServerPosition.Extend(Game.CursorPos, R.Range);
                var newvec = Vector3.Distance(Player.ServerPosition, tomouse) >= Vector3.Distance(Player.ServerPosition, tomax) ? tomax : tomouse;
                var wts = Drawing.WorldToScreen(newvec);
                var wtf = Drawing.WorldToScreen(Player.ServerPosition);
                Drawing.DrawLine(wtf, wts, 2, Color.GhostWhite);
                Render.Circle.DrawCircle(newvec, 400, Color.GhostWhite, 2);
                Drawing.DrawText(wts.X - 20, wts.Y - 50, Color.Yellow, "Hits: " + newvec.CountEnemiesInRange(400));
            }
        }
    }
}
