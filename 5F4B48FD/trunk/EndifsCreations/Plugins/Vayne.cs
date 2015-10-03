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
    class Vayne : PluginData
    {
        public Vayne()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 300);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 550); //470
            R = new Spell(SpellSlot.R);

            E.SetTargetted(0.25f, 1600f);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Vayne.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Vayne.Combo.E", "Use E").SetValue(true));                
                combomenu.AddItem(new MenuItem("EC.Vayne.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Vayne.Misc.E", "E Gapclosers").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Vayne.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Vayne.Draw.E", "E").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(Player.AttackRange, TargetSelector.DamageType.Physical, true);

            var UseQ = Root.Item("EC.Vayne.Combo.Q").GetValue<bool>();
            var UseE = Root.Item("EC.Vayne.Combo.E").GetValue<bool>();
            
            var CastItems = Root.Item("EC.Vayne.Combo.Items").GetValue<bool>(); 
            if (Target.IsValidTarget())
            {
                try
                {
                    if (UseQ && Q.IsReady())
                    {
                        Vector3 vec = Player.ServerPosition.Extend(Game.CursorPos, Q.Range);
                        if (RActive)
                        {
                            Q.Cast(vec);
                        }
                        else if (Target.GetBuffCount("vaynesilvereddebuff") >= 2)
                        {
                            if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) > Vector3.Distance(vec, Target.ServerPosition))
                            {
                                Q.Cast(vec);
                            }
                        }
                        else if (UseE && E.IsReady())
                        {
                            var box = new Geometry.Polygon.Rectangle(Target.ServerPosition, vec.Extend(Target.ServerPosition, Vector3.Distance(Player.ServerPosition, Target.ServerPosition) + 400 - Target.BoundingRadius), Target.BoundingRadius); //reduced
                            if (box.Points.Any(point => myUtility.PointCollides(point.To3D())))
                            {
                                E.UpdateSourcePosition(vec);
                                Q.Cast(vec);
                            }
                        }
                        else 
                        {
                            if (!myUtility.EnoughHealth(33) && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) < Vector3.Distance(vec, Target.ServerPosition))
                            {
                                Q.Cast(vec);
                            }
                        }
                       
                    }
                    if (UseE && E.IsReady())
                    {
                        if (E.IsKillable(Target))
                        {
                            E.Cast(Target);
                        }
                        else if (!Player.IsDashing())
                        {
                            var box = new Geometry.Polygon.Rectangle(Target.ServerPosition, Player.ServerPosition.Extend(Target.ServerPosition, Vector3.Distance(Player.ServerPosition, Target.ServerPosition) + 400 - Target.BoundingRadius), Target.BoundingRadius); //reduced
                            if (box.Points.Any(point => myUtility.PointCollides(point.To3D())))
                            {
                                E.Cast(Target);
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
        private bool RActive
        {
            get
            {
                return Player.HasBuff("VayneInquisition");
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
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R && Root.Item("EC.Vayne.Combo.Items").GetValue<bool>())
            {
                Utility.DelayAction.Add(myHumazier.ReactionDelay, () => myItemManager.UseGhostblade());
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsEnemy && Root.Item("EC.Vayne.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (Vector3.Distance(Player.ServerPosition, gapcloser.End) <= Player.BoundingRadius)
                {
                    if ( myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.Cast(gapcloser.Sender));
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Vayne.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Vayne.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
                Target = myUtility.GetTarget(Player.AttackRange, TargetSelector.DamageType.Physical);
                if (Target.IsValidTarget())
                {
                    var box = new Geometry.Polygon.Rectangle(Target.ServerPosition, Player.ServerPosition.Extend(Target.ServerPosition, Vector3.Distance(Player.ServerPosition, Target.ServerPosition) + 400 - Target.BoundingRadius), Target.BoundingRadius);
                    if (box.Points.Any(point => myUtility.PointCollides(point.To3D())))
                    {
                        box.Draw(Color.Red, 4);
                    }
                    else
                    {
                        box.Draw(Color.White, 4);
                    }
                }
            }
        }
    }
}
