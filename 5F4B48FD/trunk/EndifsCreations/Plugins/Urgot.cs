using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using ItemData = LeagueSharp.Common.Data.ItemData;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
{
    class Urgot : PluginData
    {
        public Urgot()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 975f);            
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 900);
            R = new Spell(SpellSlot.R);

            Q2 = new Spell(SpellSlot.Q, 1200f);

            Q.SetSkillshot(0.2667f, 60f, 1600f, true, SkillshotType.SkillshotLine);            
            E.SetSkillshot(0.2658f, 120f, 1500f, false, SkillshotType.SkillshotCircle);

            Q2.SetSkillshot(0.3f, 60f, 1800f, false, SkillshotType.SkillshotLine);

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
                combomenu.AddItem(new MenuItem("EC.Urgot.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Urgot.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Urgot.Combo.E", "Use E").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Urgot.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Urgot.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Urgot.Harass.E", "Use E").SetValue(true));
                Root.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Urgot.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Urgot.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Urgot.Farm.E.Value", "E More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Urgot.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                Root.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Urgot.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Urgot.Jungle.E", "Use E").SetValue(true));
                Root.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Urgot.QPredHitchance", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("EC.Urgot.Misc.W", "W Shields").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Urgot.Muramana", "Muramana").SetValue(new Slider(50)));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Urgot.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Urgot.Draw.Q2", "Q2").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Urgot.Draw.E", "E").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(Q2.Range, TargetSelector.DamageType.Physical);

            var UseQ = Root.Item("EC.Urgot.Combo.Q").GetValue<bool>();
            var UseE = Root.Item("EC.Urgot.Combo.E").GetValue<bool>();
            if (UseQ && Q.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.IsTargetable);
                var qt = EnemyList.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= Q2.Range && x.HasBuff("urgotcorrosivedebuff")).OrderBy(i => i.Health).FirstOrDefault();
                if (qt != null)
                {
                    CastQ(qt);
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;                
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        if (UseE && E.IsReady() && !Target.HasBuff("urgotcorrosivedebuff")) return;
                        CastQ(Target);
                    }                    
                    if (UseE && E.IsReady())
                    {
                        mySpellcast.CircularAoe(Target, E, HitChance.High, E.Range, 120);
                    }
                }
                catch { }
            }
            
        }
        private void Harass()
        {
            var UseQ = Root.Item("EC.Urgot.Harass.Q").GetValue<bool>();
            var UseE = Root.Item("EC.Urgot.Harass.E").GetValue<bool>();

            if (UseQ && Q.IsReady() && !myOrbwalker.Waiting && !Player.IsWindingUp)
            {
                var corrode = HeroManager.Enemies.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) < Q2.Range && x.HasBuff("urgotcorrosivedebuff")).OrderBy(i => i.Health).FirstOrDefault();
                if (corrode != null && corrode.IsValidTarget())
                {
                    Q2.Cast(corrode.ServerPosition);
                }
                else
                {
                    var targetQ = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                    if (targetQ != null && targetQ.IsValidTarget()) CastQ(targetQ);
                }
            }
            if (UseE && E.IsReady() && !myOrbwalker.Waiting && !Player.IsWindingUp)
            {
                var targetE = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (targetE != null && targetE.IsValidTarget())
                {
                    mySpellcast.CircularAoe(targetE, E, HitChance.High,E.Range,120);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < Root.Item("EC.Urgot.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (Root.Item("EC.Urgot.Farm.Q").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (minionQ == null) return;
                var corrodeQ = minionQ.Where(x => x.HasBuff("urgotcorrosivedebuff")).OrderBy(i => i.Health);
                foreach (var mcQ in corrodeQ.Where(x => Q2.IsKillable(x)))
                {
                    Q2.Cast(mcQ.ServerPosition);
                }
                var normQ = minionQ.OrderBy(i => i.Health);
                foreach (var nmQ in normQ.Where(x => Q.IsKillable(x)))
                {
                    Q.CastIfHitchanceEquals(nmQ, HitChance.High);
                }
            }
            if (Root.Item("EC.Urgot.Farm.E").GetValue<bool>() && E.IsReady())
            {
                var minionE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range + E.Width);
                var ECircular = E.GetCircularFarmLocation(minionE);
                if (ECircular.MinionsHit > Root.Item("EC.Urgot.Farm.E.Value").GetValue<Slider>().Value)
                {
                    E.Cast(ECircular.Position);
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
                if (Root.Item("EC.Urgot.Jungle.Q").GetValue<bool>() && Q.IsReady())
                {
                    if (largemobs != null)
                    {
                        if (largemobs.HasBuff("urgotcorrosivedebuff")) Q2.Cast(largemobs.ServerPosition);
                        else Q.CastIfHitchanceEquals(largemobs, HitChance.High);
                    }
                    else
                    {
                        if (mob.HasBuff("urgotcorrosivedebuff")) Q2.Cast(mob.ServerPosition);
                        else Q.CastIfHitchanceEquals(mob, HitChance.High);
                    }
                }
                if (Root.Item("EC.Urgot.Jungle.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null)
                    {
                        E.Cast(largemobs.ServerPosition);
                    }
                    else
                    {
                        var mobE = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.Health);
                        if (mobE == null) return;
                        var epred = E.GetCircularFarmLocation(mobE);
                        if (epred.MinionsHit > 0)
                        {
                            E.Cast(epred.Position);
                        }
                    }
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
            switch (Root.Item("EC.Urgot.QPredHitchance").GetValue<StringList>().SelectedIndex)
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
        private void CastQ(Obj_AI_Hero target)
        {
            if (target.HasBuff("urgotcorrosivedebuff"))
            {
                mySpellcast.Linear(target, Q2, QHitChance);
            }
            else
            {
                mySpellcast.Linear(target, Q, QHitChance, true);
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
                    if (Player.HasBuff("Muramana") || (myUtility.PlayerManaPercentage < Root.Item("EC.Urgot.Muramana").GetValue<Slider>().Value))
                    {
                        if (Items.HasItem(3042) && Items.CanUseItem(3042)) Items.UseItem(3042);
                    }
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
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (Player.IsChannelingImportantSpell() || myUtility.TickCount - LastR <= 0.5f)
            {
                args.Process = false;
            }
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (Root.Item("EC.Urgot.Combo.W").GetValue<bool>() && W.IsReady())
                    {
                        W.Cast();
                    }                  
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (Root.Item("EC.Urgot.Harass.W").GetValue<bool>() && W.IsReady())
                    {
                        W.Cast();
                    }
                }
            }
        }        
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe)
            {
                if (args.Slot == SpellSlot.R)
                {
                    LastR = myUtility.TickCount;
                    mySpellcast.Pause(1100 + Game.Ping);
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (args.Slot == SpellSlot.Q)
                    {
                        if (args.Target is Obj_AI_Hero)
                        {
                            var x = (Obj_AI_Hero)args.Target;
                            if (x.HasBuff("urgotcorrosivedebuff") && ItemData.Muramana.GetItem().IsReady() && !Player.HasBuff("Muramana") && myUtility.PlayerManaPercentage > Root.Item("EC.Urgot.Muramana").GetValue<Slider>().Value)
                            {
                                if (Items.HasItem(3042) && Items.CanUseItem(3042)) Items.UseItem(3042);
                            }
                        }
                    }
                }
            }
        }

        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit is Obj_AI_Hero && unit.IsEnemy)
            {
                if (Root.Item("EC.Urgot.Misc.W").GetValue<bool>() && W.IsReady())
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
                    else if (spell.SData.IsAutoAttack() && spell.Target != null && spell.Target.IsMe)
                    {
                        W.Cast();
                    }
                }
            }            
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Urgot.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Urgot.Draw.Q2").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q2.Range, Color.White);
            }
            if (Root.Item("EC.Urgot.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Urgot.Draw.E").GetValue<bool>() && R.Level > 0 && R.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
