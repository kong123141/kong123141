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
    class JarvanIV : PluginData
    {
        public JarvanIV()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 770);
            W = new Spell(SpellSlot.W, 450);
            E = new Spell(SpellSlot.E, 830); 
            R = new Spell(SpellSlot.R, 650); //325 radius

            Q.SetSkillshot(0.5f, 70f, float.MaxValue, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.5f, 70f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.JarvanIV.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.JarvanIV.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.JarvanIV.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.JarvanIV.Combo.R", "Use R").SetValue(true));
                config.AddSubMenu(combomenu);
            }            
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.JarvanIV.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.JarvanIV.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.JarvanIV.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.JarvanIV.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            var UseQ = config.Item("EC.JarvanIV.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.JarvanIV.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.JarvanIV.Combo.E").GetValue<bool>();
            var UseR = config.Item("EC.JarvanIV.Combo.R").GetValue<bool>();

            if (UseW && W.IsReady())
            {
                if (Player.CountEnemiesInRange(W.Range) > 0)
                {
                    W.Cast();
                }
            }
            if (Target.IsValidTarget())
            {
                try
                {
                    if (UseQ && Q.IsReady())
                    {                        
                        if (DemacianFlag != null)
                        {                                                    
                            var box = new Geometry.Polygon.Rectangle(Player.Position, DemacianFlag.Position, Q.Width);                            
                            if (box.Points.Any(point => point == Target.ServerPosition.To2D()))
                            {
                                Q.Cast(DemacianFlag.Position);
                            }
                            else
                            {
                                mySpellcast.Linear(Target, Q, HitChance.High);
                            }
                        }
                        if (!E.CanCast(Target))
                        {
                            mySpellcast.Linear(Target, Q, HitChance.High);
                        }
                    }
                    if (UseE && E.IsReady())
                    {
                        mySpellcast.Wall(Target, E, HitChance.High);
                    }
                    if (UseR && R.IsReady() && Cataclysm == null)
                    {
                        if ((UseQ && Q.CanCast(Target)) || (UseE && E.CanCast(Target))) return;
                        if (R.IsKillable(Target))
                        {
                            R.Cast(Target);
                        }
                    }
                }
                catch { }
            }
        }
        private GameObject DemacianFlag;
        private GameObject Cataclysm;

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
                    /*
                    if (DemacianFlag == null)
                    {
                        target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                        mySpellcast.Linear(target, Q, HitChance.High);
                    }*/
                    Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                    if (Target.IsValidTarget() && E.IsReady())
                    {
                        mySpellcast.CircularPrecise(Target, E, HitChance.High,0,0,0);
                    }
                    break;
            }            
        }

        protected override void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Equals("JarvanDemacianStandard_hit"))
            {
                DemacianFlag = sender;
            }
            if (sender.Name.Equals("JarvanCataclysm_tar"))
            {
                Cataclysm = sender;
            }
        }
        protected override void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Equals("JarvanDemacianStandard_flag"))
            {
                 DemacianFlag = null;
            }
            if (sender.Name.Equals("JarvanCataclysm_tar"))
            {
                Cataclysm = null;
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "jarvanivdragonstrike") || (spell.SData.Name.ToLower() == "jarvanivgoldenaegis") || (spell.SData.Name.ToLower() == "jarvanivdemacianstandard"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.JarvanIV.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.JarvanIV.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.JarvanIV.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (config.Item("EC.JarvanIV.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (Target.IsValidTarget())
                {
                    Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.Yellow, "Hits: " + Target.CountEnemiesInRange(325) + "Allies: " + Target.CountAlliesInRange(325));
                }
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
