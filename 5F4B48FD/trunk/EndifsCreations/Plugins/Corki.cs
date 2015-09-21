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
    class Corki : PluginData
    {
        public Corki()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 825); //250 rad
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 600); //35 deg
            R = new Spell(SpellSlot.R, 1225); //75 wd

            R2 = new Spell(SpellSlot.R, 1225); //150 wd

            Q.SetSkillshot(0.50f, 250f, 1135f, false,SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f, 450, 1200, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0,25f, float.MaxValue, false, SkillshotType.SkillshotCone,Player.Position);
            R.SetSkillshot(0.25f, 75f, 2000f, true,SkillshotType.SkillshotLine);

            R2.SetSkillshot(0.25f, 100f, 2000f, true,SkillshotType.SkillshotLine); //third shot

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellList.Add(R2);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Corki.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Corki.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Corki.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Corki.Combo.R", "Use R").SetValue(true));
                config.AddSubMenu(combomenu);
            }            
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Corki.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Corki.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Corki.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Corki.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            var UseQ = config.Item("EC.Corki.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Corki.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Corki.Combo.E").GetValue<bool>();
            var UseR = config.Item("EC.Corki.Combo.R").GetValue<bool>();

            if (Target.IsValidTarget())
            {
                try
                {   
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.CircularPrecise(Target, Q, HitChance.High);
                    }
                    if (UseW && W.IsReady())
                    {
                       
                    }
                    if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        if (myUtility.IsFacing(Player,Target.ServerPosition,35))
                        {
                            mySpellcast.Linear(Target, E, HitChance.High);
                        }
                    }
                    if (UseR && R.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        if (Player.HasBuff("corkimissilebarragecounterbig"))
                        {
                            mySpellcast.Linear(Target, R2, HitChance.High, true);
                        }
                        else
                        {
                            mySpellcast.Linear(Target, R, HitChance.High, true);
                        }
                    }
                }
                catch { }
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
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "phosphorusbomb") || (spell.SData.Name.ToLower() == "carpetbomb") || (spell.SData.Name.ToLower() == "ggun") || (spell.SData.Name.ToLower() == "missilebarrage"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Corki.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Corki.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Corki.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (config.Item("EC.Corki.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                if (Player.HasBuff("corkimissilebarragecounterbig"))
                {
                    Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.Yellow, "Big One");
                }
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
