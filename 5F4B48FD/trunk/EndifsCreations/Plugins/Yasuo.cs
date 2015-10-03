#region Todo
    //      Windwall allow/ignore list
#endregion Todo

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
    class Yasuo : PluginData
    {
        public Yasuo()
        {
            LoadSpells();
            LoadMenus();
        }
       
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 475); //linear
            W = new Spell(SpellSlot.W, 750); //300 / 350 / 400 / 450 / 500 width
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R, 1200);

            Q2 = new Spell(SpellSlot.Q, 900); //skill shot
            Q3 = new Spell(SpellSlot.Q, 375); //only dash

            Q.SetSkillshot(((float)1 / 1 / 0.5f * Player.AttackSpeedMod), 50f, float.MaxValue, false, SkillshotType.SkillshotLine);
            Q2.SetSkillshot(0.25f, 50f, 1200f, false, SkillshotType.SkillshotLine);
            Q3.SetSkillshot(0f, 375f, float.MaxValue, false, SkillshotType.SkillshotCircle, Player.Position);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Yasuo.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Yasuo.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Yasuo.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Yasuo.Combo.R", "Use R").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Yasuo.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Yasuo.Farm.Q", "Use Q").SetValue(true));
                Root.AddSubMenu(laneclearmenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Yasuo.Misc.Q", "Q Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Yasuo.Misc.Q2", "Q Gapclosers").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Yasuo.Misc.W", "W Spell Block").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }

            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Yasuo.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Yasuo.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Yasuo.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Yasuo.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }

        }

        private void Combo()
        {
            Target = myUtility.GetTarget(Q2.Range, TargetSelector.DamageType.Physical);

            var UseQ = Root.Item("EC.Yasuo.Combo.Q").GetValue<bool>();
            var UseE = Root.Item("EC.Yasuo.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Yasuo.Combo.R").GetValue<bool>();
            var CastItems = Root.Item("EC.Yasuo.Combo.Items").GetValue<bool>();
            
            if (Target.IsValidTarget())
            {
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        if (Player.HasBuff("YasuoQ3W"))
                        {
                            //Q2 or Q3
                            if (!Player.IsDashing() || myUtility.TickCount - Player.GetDashInfo().EndTick > 600)
                            {
                                mySpellcast.Linear(Target, Q2, HitChance.Medium);
                            }
                            else
                            {
                                if (Player.GetDashInfo().EndPos.To3D().CountEnemiesInRange(Q3.Range) > 1)
                                {
                                    Q3.Cast();
                                }
                            }
                        }
                        else
                        {
                            if (!Player.IsDashing() || myUtility.TickCount - Player.GetDashInfo().EndTick >= 0.5f)
                            {
                                mySpellcast.Linear(Target, Q, HitChance.High);
                            }
                        }
                    }
                    if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= E.Range)
                        {
                            E.Cast(Target);
                        }
                        else
                        {
                            mySpellcast.Extension(Target, E, E.Range, E.Range, false);
                        }
                    }
                    if (UseR && R.IsReady())
                    {
                        if (Target.IsValidTarget())
                        {
                            if ((Target.HasBuffOfType(BuffType.Knockup) || Target.HasBuffOfType(BuffType.Knockback)) && R.IsKillable(Target))
                            {
                                Utility.DelayAction.Add((int)(myUtility.DisabledDuration(Target) - myHumazier.ReactionDelay), () => R.Cast());
                            }
                            else if (HeroManager.Enemies.Count(x => x.HasBuffOfType(BuffType.Knockup) && R.IsInRange(x)) >= 4)
                            {
                                Utility.DelayAction.Add((int)(myHumazier.ReactionDelay), () => R.Cast());
                            }
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

        private static readonly string[] AllowSpellBlock =
        {
            "galioresolutesmite", "luxlightstrikekugel"
        };
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
                case myOrbwalker.OrbwalkingMode.LaneClear:
                    if (Root.Item("EC.Yasuo.Farm.Q").GetValue<bool>())
                    {
                        if (Q.IsReady() && (!Player.IsDashing() || myUtility.TickCount - Player.GetDashInfo().EndTick >= 500))
                        {
                            if (Player.HasBuff("YasuoQ3W"))
                            {
                                myFarmManager.LaneLinear(Q2, Q2.Range);
                            }
                            else
                            {
                                myFarmManager.LaneLastHit(Q, Q.Range, null, true);
                            }
                        }
                    }
                    break;
                case myOrbwalker.OrbwalkingMode.JungleClear:
                    
                    if (Q.IsReady())
                    {
                        if (Player.HasBuff("YasuoQ3W"))
                        {
                            myFarmManager.JungleLinear(Q2, Q2.Range);
                        }
                        else
                        {
                            myFarmManager.JungleLinear(Q, Q.Range);
                        }
                    }
                    break;
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R && Root.Item("EC.Yasuo.Combo.Items").GetValue<bool>())
            {
                Utility.DelayAction.Add(myHumazier.ReactionDelay, () => myItemManager.UseGhostblade());
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "yasuoq") || (spell.SData.Name.ToLower() == "yasuowmovingwall") || (spell.SData.Name.ToLower() == "yasuodashwrapper"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
            if (unit is Obj_AI_Hero && unit.IsEnemy && !spell.SData.IsAutoAttack() && W.IsReady())
            {
                if ((myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Yasuo.Combo.W").GetValue<bool>()) || (Root.Item("EC.Yasuo.Misc.W").GetValue<bool>()))
                {
                    if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone)) &&
                        spell.SData.MissileSpeed > 20)
                    {
                        var box = new Geometry.Polygon.Rectangle(spell.Start, spell.End, spell.SData.LineWidth);
                        if (box.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 150))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast(spell.Start));
                        }
                    }
                    else if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Unit) || spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit)) && spell.Target != null && spell.Target.IsMe)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast(spell.Start));
                    }
                    else if (spell.SData.TargettingType.Equals(SpellDataTargetType.LocationAoe) && AllowSpellBlock.Contains(spell.SData.Name.ToLower()) && spell.End.Distance(Player.ServerPosition) <= 100)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast(spell.Start));
                    }
                }
            }   
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if ((myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Yasuo.Combo.Items").GetValue<bool>()) && Orbwalking.InAutoAttackRange(target))
            {
                myItemManager.UseItems(2, null);
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Root.Item("EC.Yasuo.Misc.Q2").GetValue<bool>() && Q.IsReady() && Player.HasBuff("YasuoQ3W"))
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= Q2.Range)
                {
                    if (myUtility.ImmuneToMagic(sender) || myUtility.ImmuneToCC(sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Linear(sender, Q2, HitChance.Medium));
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Yasuo.Misc.Q2").GetValue<bool>() && Q.IsReady() && Player.HasBuff("YasuoQ3W"))
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= Q2.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () =>  mySpellcast.PointVector(gapcloser.End, Q2, Target.BoundingRadius));
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Yasuo.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                if (Player.HasBuff("YasuoQ3W"))
                {
                    var box = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(Game.CursorPos, Q2.Range), Q2.Width);
                    box.Draw(Color.Red);
                }
                else
                {
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
                }
            }
            if (Root.Item("EC.Yasuo.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Yasuo.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Yasuo.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.White);
            }
        }
    }
}
