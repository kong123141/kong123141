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
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Vladimir.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Vladimir.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Vladimir.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Vladimir.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Vladimir.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Vladimir.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Vladimir.Harass.E", "Use E").SetValue(true));
                Root.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Vladimir.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Vladimir.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Vladimir.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Vladimir.Farm.W.Value", "W More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Vladimir.Farm.E.Value", "E More Than").SetValue(new Slider(1, 1, 5)));
                Root.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Vladimir.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Vladimir.Jungle.W", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Vladimir.Jungle.E", "Use E").SetValue(true));
                Root.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Vladimir.RPredHitchance", "R Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("EC.Vladimir.Misc.W", "W Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Vladimir.Misc.W2", "W Spelldodge").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Vladimir.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Vladimir.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Vladimir.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Vladimir.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(R.Range, TargetSelector.DamageType.Magical);       
     
            var UseQ = Root.Item("EC.Vladimir.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Vladimir.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Vladimir.Combo.E").GetValue<bool>();
            if (UseE && E.IsReady())
            {                
                mySpellcast.PointBlank(null, E, 550);
            }
            if (Target.IsValidTarget())
            {
                if (myUtility.ImmuneToMagic(Target)) return;
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        mySpellcast.Unit(Target, Q);
                    }
                    if (UseW && W.IsReady())
                    {
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= W.Range)
                        {
                            W.Cast();
                        }
                        else
                        {
                            var dist = Vector3.Distance(Player.ServerPosition, Target.ServerPosition);
                            var msDif = Player.MoveSpeed - Target.MoveSpeed;
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
                }
                catch
                {
                }
            }
        }
        private void Harass()
        {
            var UseQ = Root.Item("EC.Vladimir.Harass.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Vladimir.Harass.W").GetValue<bool>();
            var UseE = Root.Item("EC.Vladimir.Harass.E").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget() && !myOrbwalker.Waiting)
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
                if (UseE && E.IsReady() && myUtility.TickCount - LastE > 6000)
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
            if (Root.Item("EC.Vladimir.Farm.Q").GetValue<bool>() && Q.IsReady())
            {
                var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (allMinionsQ == null) return;
                var siegeQ = myFarmManager.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                if (siegeQ != null && siegeQ.IsValidTarget())
                {
                    Q.CastOnUnit(siegeQ);
                }
                else
                {
                    var selectQ = allMinionsQ.OrderByDescending(i => i.Health).FirstOrDefault();
                    if (selectQ != null && selectQ.IsValidTarget())
                    {
                        Q.CastOnUnit(selectQ);
                    }
                    /*
                    var selectQ = allMinionsQ.Where(x => Q.IsKillable(x) && Player.BaseAttackDamage < x.Health).OrderByDescending(i => i.Health).FirstOrDefault();
                    if (selectQ != null && selectQ.IsValidTarget())
                    {
                        Q.CastOnUnit(selectQ);
                    }*/
                }
            }
            if (Root.Item("EC.Vladimir.Farm.W").GetValue<bool>() && W.IsReady() && EBuffStacks >= 4)
            {
                var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range);
                if (allMinionsW == null) return;
                if (allMinionsW.Count > Root.Item("EC.Vladimir.Farm.W.Value").GetValue<Slider>().Value)
                {
                    if (Player.UnderTurret(true)) return;
                    W.Cast();
                }
            }
            if (Root.Item("EC.Vladimir.Farm.E").GetValue<bool>() && E.IsReady() && (EBuffStacks < 4 && myUtility.TickCount - LastE > 8000 || EBuffStacks >= 4))
            {
                var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                if (allMinionsE == null) return;
                if (allMinionsE.Count > Root.Item("EC.Vladimir.Farm.E.Value").GetValue<Slider>().Value)
                {
                    if (Player.UnderTurret(true)) return;
                    E.Cast();
                }
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (Root.Item("EC.Vladimir.Jungle.Q").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp && Q.IsInRange(mob))
                {
                    if (largemobs != null)
                    {
                        Q.CastOnUnit(largemobs);
                    }
                    Q.CastOnUnit(mob);
                }
                if (Root.Item("EC.Vladimir.Jungle.W").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < W.Range)
                    {
                        W.Cast();
                    }
                    if (Vector3.Distance(Player.ServerPosition, mob.ServerPosition) < W.Range) W.Cast();
                }
                if (Root.Item("EC.Vladimir.Jungle.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < E.Range)
                    {
                        E.Cast();
                    }
                    E.Cast();
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
        
        private HitChance RHitChance
        {
            get
            {
                return GetRHitChance();
            }
        }
        private HitChance GetRHitChance()
        {
            switch (Root.Item("EC.Vladimir.RPredHitchance").GetValue<StringList>().SelectedIndex)
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
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (Root.Item("EC.Vladimir.Farm.Q").GetValue<bool>() && Q.IsReady())
            {
                var target = minion as Obj_AI_Base;
                if (target != null && Q.IsKillable(target) && Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= Q.Range)
                {
                    Q.CastOnUnit(target);
                }
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if (spell.SData.Name.ToLower() == "vladimirtidesofblood")
                {
                    LastE = myUtility.TickCount;
                }
            }
            if (unit is Obj_AI_Hero && unit.IsEnemy && !spell.SData.IsAutoAttack() && W.IsReady())
            {
                if (Root.Item("EC.Vladimir.Misc.W2").GetValue<bool>())
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
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Vladimir.Misc.W").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= 125 + Player.BoundingRadius)
                {
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast());
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Vladimir.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Vladimir.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Vladimir.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Vladimir.Draw.R").GetValue<bool>() && R.Level > 0 && R.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
                var tomouse = Player.ServerPosition.Extend(Game.CursorPos, Vector3.Distance(Player.ServerPosition, Game.CursorPos));
                var tomax = Player.ServerPosition.Extend(Game.CursorPos, R.Range);
                var newvec = Vector3.Distance(Player.ServerPosition, tomouse) >= Vector3.Distance(Player.ServerPosition, tomax) ? tomax : tomouse;
                var wts = Drawing.WorldToScreen(newvec);
                var wtf = Drawing.WorldToScreen(Player.ServerPosition);
                Drawing.DrawLine(wtf, wts, 2, Color.GhostWhite);
                Render.Circle.DrawCircle(newvec, 175, Color.GhostWhite, 2);
                Drawing.DrawText(wts.X - 20, wts.Y - 50, Color.Yellow, "Hits: " + newvec.CountEnemiesInRange(175));
            }
        }
    }
}