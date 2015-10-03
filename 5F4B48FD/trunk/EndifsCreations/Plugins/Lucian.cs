using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
{
    class Lucian : PluginData
    {
        public Lucian()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 650); //1100
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 425);
            R = new Spell(SpellSlot.R, 1400);

            Q2 = new Spell(SpellSlot.Q, 1100);

            Q.SetTargetted(0.25f, float.MaxValue);
            W.SetSkillshot(0.4f, 150f, 1600, true, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 1f, float.MaxValue, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.2f, 110f, 2500, true, SkillshotType.SkillshotLine);

            Q2.SetSkillshot(0.55f, 65f, float.MaxValue, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Lucian.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Lucian.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Lucian.Combo.E", "Use E").SetValue(true));
                //combomenu.AddItem(new MenuItem("EC.Lucian.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Lucian.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Lucian.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                laneclearmenu.AddItem(new MenuItem("EC.Lucian.Farm.Q", "Use Q").SetValue(true));
                Root.AddSubMenu(laneclearmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Lucian.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Lucian.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Lucian.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Lucian.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q2.Range, TargetSelector.DamageType.Physical, true);

            var UseQ = Root.Item("EC.Lucian.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Lucian.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Lucian.Combo.E").GetValue<bool>();
            //var UseR = Root.Item("EC.Lucian.Combo.R").GetValue<bool>();            
            var CastItems = Root.Item("EC.Lucian.Combo.Items").GetValue<bool>();

            if (UseQ && Q.IsReady() && !Player.IsDashing() && !Lightslinger)
            {
                if (Target == null || !Target.IsValidTarget())
                {
                    mySpellcast.Extension(null, Q, Q.Range, Q2.Range, false, true, true);
                }
            }

            if (Target != null && Target.IsValidTarget())
            {
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {                        
                        if (!Lightslinger && (!Player.IsDashing() && myUtility.TickCount - Player.GetDashInfo().EndTick > 600))
                        {
                            if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= Q.Range)
                            {
                                mySpellcast.Unit(Target, Q);
                            }
                            else if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) > Q.Range && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= Q2.Range)
                            {
                                mySpellcast.Extension(Target, Q, Q.Range, Q2.Range, false, true, true);
                            }
                        }
                    }
                    if (UseW && W.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay && (!Player.IsDashing() && myUtility.TickCount - Player.GetDashInfo().EndTick > 600))
                    {
                        mySpellcast.Linear(Target, W, HitChance.High, true);
                    }
                    if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > (myHumazier.SpellDelay + myHumazier.ReactionDelay))
                    {
                        var vec = Player.ServerPosition.Extend(Game.CursorPos, E.Range);
                        if (!Lightslinger)
                        {                            
                            if (myUtility.EnoughHealth(33) && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) >= Vector3.Distance(vec, Target.ServerPosition))
                            {
                                E.Cast(vec);
                            }
                            else if (!myUtility.EnoughHealth(33) && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= Vector3.Distance(vec, Target.ServerPosition))
                            {
                                E.Cast(vec);
                            }
                        }
                        else if (Lightslinger)
                        {
                            if (RActive)
                            {
                                E.Cast(vec);
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
        private bool Lightslinger;
        private bool RActive
        {
            get
            {
                return Player.HasBuff("LucianR");
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
                case myOrbwalker.OrbwalkingMode.LaneClear:
                    if (Root.Item("EC.Lucian.Farm.Q").GetValue<bool>())
                    {
                        if (Q.IsReady() && myUtility.EnoughMana(Root.Item("EC.Lucian.Farm.ManaPercent").GetValue<Slider>().Value) && !Lightslinger)
                        {
                            myFarmManager.LaneLinearTargetted(Q, Q.Range, Q2.Width, (Q2.Range - Q.Range));
                        }
                    }
                    break;
            }
        }
        protected override void OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            if (sender.IsMe && args.Buff.Name == "lucianpassivebuff")
            {
                Lightslinger = true;
            }
        }
        protected override void OnBuffRemove(Obj_AI_Base sender, Obj_AI_BaseBuffRemoveEventArgs args)
        {
            if (sender.IsMe && args.Buff.Name == "lucianpassivebuff" && myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.None)
            {
                Utility.DelayAction.Add(myHumazier.ReactionDelay, () => Lightslinger = false);
            }
        }
        protected override void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && myOrbwalker.IsAutoAttack(args.SData.Name) && Lightslinger && !Player.IsWindingUp)
            {
                Utility.DelayAction.Add(myHumazier.ReactionDelay, () => Lightslinger = false);
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {                
                if ((spell.SData.Name.ToLower() == "lucianq") || (spell.SData.Name.ToLower() == "lucianw") || (spell.SData.Name.ToLower() == "luciane"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.Q || args.Slot == SpellSlot.W || args.Slot == SpellSlot.E)
            {
                Utility.DelayAction.Add(myHumazier.ReactionDelay, myOrbwalker.ResetAutoAttackTimer);
            }
            if (args.Slot == SpellSlot.R && Root.Item("EC.Lucian.Combo.Items").GetValue<bool>())
            {
                myItemManager.UseGhostblade();
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Lucian.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.Cyan);
                Render.Circle.DrawCircle(Player.Position, Q2.Range, Color.Cyan);
            }
            if (Root.Item("EC.Lucian.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Lucian.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Lucian.Draw.R").GetValue<bool>() && R.Level > 0 && R.IsReady())
            {
                var box = new Geometry.Polygon.Rectangle(Player.ServerPosition, Player.ServerPosition.Extend(Game.CursorPos, R.Range), R.Width);
                var insidebox = HeroManager.Enemies.Where(x => box.IsInside(x) && x.IsValidTarget()).ToList();
                if (insidebox.Any())
                {
                    if (insidebox.Count() >= 4)
                    {
                        Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.Red, "Hits: " + insidebox.Count());
                    }
                    else
                    {
                        Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.Yellow, "Hits: " + insidebox.Count());
                    }
                }
                box.Draw(Color.Red);
            }
        }
    }
}
