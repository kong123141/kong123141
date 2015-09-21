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
    class Gangplank : PluginData
    {
        public Gangplank()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1000);

            Q.SetTargetted(0.29f, 1400f);

            SpellList.Add(Q);
            SpellList.Add(W);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Gangplank.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Gangplank.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Gangplank.Combo.E", "Use E").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Gangplank.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Gangplank.DrawBarrels", "Barrels").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            var UseQ = config.Item("EC.Gangplank.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Gangplank.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Gangplank.Combo.E").GetValue<bool>();

            if (UseQ && Q.IsReady())
            {
                if (Target.IsValidTarget() && Q.IsKillable(Target))
                {
                    Q.CastOnUnit(Target);
                }
                else
                {
                    foreach (var x in AllBarrels.Where(o => Vector3.Distance(Player.ServerPosition, o.ServerPosition) <= Q.Range))
                    {
                        if (x.Health <= 1 && x.CountEnemiesInRange(400) > 0)
                        {
                            Q.CastOnUnit(x);
                        }
                    }
                }
            }
            if (UseW && W.IsReady())
            {
                if (myUtility.MovementDisabled(Player) || myUtility.PlayerHealthPercentage < 75)
                {
                    W.Cast();
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {
                    var barrel = AllBarrels.FirstOrDefault(o => Vector3.Distance(Target.ServerPosition, o.ServerPosition) <= 400 && o.Health <= 1);
                    if (UseQ && Q.IsReady())
                    {                        
                        if (barrel != null)
                        {
                            Q.CastOnUnit(barrel);
                        }
                        else
                        {
                            Q.CastOnUnit(Target);
                        }
                    }
                    if (UseE && E.IsReady())
                    {
                        if (barrel != null)
                        {
                            mySpellcast.Circular(barrel.Position, E, 100, 400, 400);
                        }
                    }
                }
                catch { }
            }
        }

        private static List<Obj_AI_Minion> Barrels = new List<Obj_AI_Minion>();
        private static List<Obj_AI_Minion> AllBarrels
        {
            get { return Barrels.Where(s => s.IsValid).ToList(); }
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
                    foreach (var x in AllBarrels)
                    {
                        if (x.Health <= 1)
                        {
                            Q.CastOnUnit(x);
                        }
                    }
                    break;
                case myOrbwalker.OrbwalkingMode.LaneClear:
                    {
                        if (Q.IsReady())
                        {
                            var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                            if (minionQ == null) return;
                            var siegeQ = myFarmManager.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                            if (siegeQ != null && siegeQ.IsValidTarget())
                            {
                                Q.CastOnUnit(siegeQ);
                            }
                            if (AllBarrels.Any())
                            {
                                foreach (var x in AllBarrels)
                                {
                                    if (x.Health <= 1 && MinionManager.GetMinions(x.Position, 400).Count() >= 3)
                                    {
                                        Q.CastOnUnit(x);
                                    }
                                }
                            }
                            else
                            {
                                var AnyQ = minionQ.FirstOrDefault(x => Q.IsKillable(x));
                                if (AnyQ != null && AnyQ.IsValidTarget())
                                {
                                    Q.CastOnUnit(AnyQ);
                                }
                            }
                        }
                    }
                    break;
            }
        }
        protected override void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender is Obj_AI_Minion && sender.Name.Contains("Barrel"))
            {
                Barrels.Add((Obj_AI_Minion)sender);
            }
        }
        protected override void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender is Obj_AI_Minion && sender.Name.Contains("Barrel"))
            {
                Barrels.RemoveAll(s => s.NetworkId == sender.NetworkId);
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Gangplank.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Gangplank.DrawBarrels").GetValue<bool>())
            {
                foreach (var barrel in Barrels.Where(s => s.IsValid))
                {
                    Render.Circle.DrawCircle(barrel.Position, 400, Color.Red);
                    Render.Circle.DrawCircle(barrel.Position, 650, Color.Yellow);
                }
            }
        }
    }
}
