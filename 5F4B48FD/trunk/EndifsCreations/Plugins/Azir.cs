#region Credits
    //      Testing purpose
    //      Esk0r's Azir  https://github.com/Esk0r/LeagueSharp/tree/master/Azir
#endregion Credits

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
    class Azir : PluginData
    {
        public Azir()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 825);            
            W = new Spell(SpellSlot.W, 450);
            E = new Spell(SpellSlot.E, 1250);
            R = new Spell(SpellSlot.R, 450);

            Q.SetSkillshot(0, 70, 1600, false, SkillshotType.SkillshotLine);            
            E.SetSkillshot(0, 100, 1700, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.5f, 0, 1400, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            myDamageIndicator.DamageToUnit = GetDamage;
        }
        private void LoadMenus()
        {
            
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Azir.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Azir.Combo.W", "Use W").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Azir.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Azir.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Azir.Farm.Q.Value", "Q More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Azir.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclearmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Azir.Draw.Q", "Q Range").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Azir.DrawSS", "Soldiers AA Range").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range + 250, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Azir.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Azir.Combo.W").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                try
                {
                    if (UseQ && Q.IsReady() && AllSoldiers.Count > 1)
                    {
                        foreach (var obj in AllSoldiers)
                        {
                            Q.SetSkillshot(0.0f, 65f, 1500f, false, SkillshotType.SkillshotLine, obj.Position, Player.Position);
                            Q.Cast(Target.Position);
                        }
                    }
                    if (UseW && W.Instance.Ammo > 0 && AllSoldiers.Count == 0)
                    {
                        var p = Player.Distance(Target, true) > W.RangeSqr ? Player.Position.To2D().Extend(Target.Position.To2D(), W.Range) : Target.Position.To2D();
                        W.Cast(p);
                    }
                }
                catch { }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("EC.Azir.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (Player.UnderTurret(true)) return;
            var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range).OrderBy(x => x.MaxHealth).ToList();
            if (minions == null) return;
            if (config.Item("EC.Azir.Farm.Q").GetValue<bool>() && Q.IsReady())
            {
                Vector3 pos;
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (minionQ == null) return;
                var siegeQ = myFarmManager.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                if (siegeQ != null && siegeQ.IsValidTarget())
                {
                    pos = siegeQ.Position.Extend(siegeQ.Position, 10);
                    if (!pos.UnderTurret(true))
                    {
                        Q.Cast(pos);
                    }
                }
                else
                {
                    var FurthestQ = minionQ.OrderByDescending(i => Vector3.Distance(Player.ServerPosition, i.ServerPosition)).FirstOrDefault(x => Q.IsKillable(x));
                    if (FurthestQ != null && FurthestQ.IsValidTarget())
                    {
                        pos = FurthestQ.Position.Extend(FurthestQ.Position, Q.Range/2);
                        if (!pos.UnderTurret(true))
                        {
                            Q.Cast(pos);
                        }
                    }
                }
            }
            if (config.Item("EC.Azir.Farm.W").GetValue<bool>() && W.IsReady() && W.Instance.Ammo > 0)
            {
                Vector3 pos;
                if (minions.Count >= 5) //use 3
                {
                    if (AllSoldiers.Count == 0)
                    {
                        pos = myUtility.RandomPos(1, 10, 10, Player.Position.Extend(minions[0].Position, W.Range));
                        if (!pos.UnderTurret(true))
                        {
                            W.Cast(pos);
                        }
                    }
                    else if (AllSoldiers.Count == 1)
                    {
                        pos = myUtility.RandomPos(1, 10, 10, Player.Position.Extend(minions[1].Position, W.Range));
                        if (!pos.UnderTurret(true))
                        {
                            W.Cast(pos);
                        }
                    }
                    else
                    {
                        pos = myUtility.RandomPos(1, 10, 10, Player.Position.Extend(minions[2].Position, W.Range));
                        if (!pos.UnderTurret(true))
                        {
                            W.Cast(pos);
                        }
                    }
                }
                else if (minions.Count < 5 && minions.Count > 2) //use 1 or 2
                {
                    if (AllSoldiers.Count == 0)
                    {
                        pos = myUtility.RandomPos(1, 10, 10, Player.Position.Extend(minions[0].Position, W.Range));
                        if (!pos.UnderTurret(true))
                        {
                            W.Cast(pos);
                        }
                    }
                    else if (AllSoldiers.Count == 1)
                    {
                        pos = myUtility.RandomPos(1, 10, 10, Player.Position.Extend(minions[1].Position, W.Range));
                        if (!pos.UnderTurret(true))
                        {
                            W.Cast(pos);
                        }
                    }
                }
            }
        }
        private float GetDamage(Obj_AI_Hero target)
        {
            var damage = 0d;
            if (Q.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.Q);
            }
            return (float)damage;
        }
        private static List<Obj_AI_Minion> Soldiers = new List<Obj_AI_Minion>();
        private static List<Obj_AI_Minion> AllSoldiers
        {
            get { return Soldiers.Where(s => s.IsValid && !s.IsDead && !s.IsMoving && !s.IsWindingUp).ToList(); }
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
                case myOrbwalker.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                case myOrbwalker.OrbwalkingMode.Combo:
                    Combo();
                    break;

            }
        }
        protected override void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender is Obj_AI_Minion && sender.Name == "AzirSoldier")
            {
                Soldiers.Add((Obj_AI_Minion)sender);
            }
        }
        protected override void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender is Obj_AI_Minion && sender.Name == "AzirSoldier")
            {
                Soldiers.RemoveAll(s => s.NetworkId == sender.NetworkId);
            }
        }
    }
}
