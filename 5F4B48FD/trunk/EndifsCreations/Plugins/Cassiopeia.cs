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
    class Cassiopeia : PluginData
    {
        public Cassiopeia()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 850f);            
            W = new Spell(SpellSlot.W, 850f);            
            E = new Spell(SpellSlot.E, 700f);            
            R = new Spell(SpellSlot.R, 825f);

            Q.SetSkillshot(0.75f, Q.Instance.SData.CastRadius, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.5f, W.Instance.SData.CastRadius, W.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotCircle);
            E.SetTargetted(0.2f, float.MaxValue);
            R.SetSkillshot(0.3f, (float)(80 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);     
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Cassiopeia.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Cassiopeia.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Cassiopeia.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Cassiopeia.Combo.R", "Use R").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Cassiopeia.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                laneclearmenu.AddItem(new MenuItem("EC.Cassiopeia.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Cassiopeia.Farm.E", "Use E").SetValue(true));
                Root.AddSubMenu(laneclearmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Cassiopeia.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Cassiopeia.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Cassiopeia.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Cassiopeia.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            var UseQ = Root.Item("EC.Cassiopeia.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Cassiopeia.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Cassiopeia.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Cassiopeia.Combo.R").GetValue<bool>();
           
            if (Target.IsValidTarget())
            {
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.CircularPrecise(Target, Q, HitChance.High, Q.Range, Q.Width);
                    }
                    if (UseW && W.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.CircularAoe(Target, W, HitChance.High, W.Range, W.Instance.SData.CastRadius);
                    }
                    if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.Unit(Target, E);                        
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
                case myOrbwalker.OrbwalkingMode.LaneClear:
                    if (myUtility.EnoughMana(Root.Item("EC.Cassiopeia.Farm.ManaPercent").GetValue<Slider>().Value))
                    {
                        var list = MinionManager.GetMinions(Player.ServerPosition, E.Range).Where(x => x.HasBuffOfType(BuffType.Poison)).ToList();
                        if (Root.Item("EC.Cassiopeia.Farm.W").GetValue<bool>() && W.IsReady() && list.Count <= 0)
                        {
                            myFarmManager.LaneCircular(W, W.Range, 125);
                        }
                        if (Root.Item("EC.Cassiopeia.Farm.E").GetValue<bool>() && E.IsReady())
                        {
                            if (myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                            {                                
                                myFarmManager.LaneLastHit(E, E.Range, list, true);
                            }
                        }
                    }
                    break;
            }
        }  
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "cassiopeianoxiousblast") || (spell.SData.Name.ToLower() == "cassiopeiamiasma") || (spell.SData.Name.ToLower() == "cassiopeiatwinfang"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Cassiopeia.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Cassiopeia.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Cassiopeia.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Cassiopeia.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
