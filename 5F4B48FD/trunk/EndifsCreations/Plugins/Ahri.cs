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
    class Ahri : PluginData
    {
        public Ahri()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 880);
            W = new Spell(SpellSlot.W, 550);
            E = new Spell(SpellSlot.E, 975);
            R = new Spell(SpellSlot.R, 400);

            Q.SetSkillshot(0.25f, 100, 1600f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 100, 1500f, true, SkillshotType.SkillshotLine);

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
                combomenu.AddItem(new MenuItem("EC.Ahri.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Ahri.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Ahri.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Ahri.Combo.R", "Use R").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Ahri.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                laneclearmenu.AddItem(new MenuItem("EC.Ahri.Farm.Q", "Use Q").SetValue(true));
                //laneclearmenu.AddItem(new MenuItem("EC.Ahri.Farm.W", "Use W").SetValue(true));
                //laneclearmenu.AddItem(new MenuItem("EC.Ahri.Farm.E", "Use E").SetValue(true));
                Root.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Ahri.Jungle.Q", "Use Q").SetValue(true));
                //junglemenu.AddItem(new MenuItem("EC.Ahri.Jungle.W", "Use W").SetValue(true));
                //junglemenu.AddItem(new MenuItem("EC.Ahri.Jungle.E", "Use E").SetValue(true));
                Root.AddSubMenu(junglemenu);
            } 
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Ahri.Misc.E", "E Gapclosers").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Ahri.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Ahri.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Ahri.Draw.E", "E").SetValue(true));                
                drawmenu.AddItem(new MenuItem("EC.Ahri.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical, true);

            var UseQ = Root.Item("EC.Ahri.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Ahri.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Ahri.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Ahri.Combo.R").GetValue<bool>();
                    
            if (Target.IsValidTarget())
            {
                if (UseR && R.Instance.Ammo > 0 && myUtility.TickCount - LastR > myHumazier.SpellDelay)
                {                    
                    Vector3 vec = Player.ServerPosition.Extend(Game.CursorPos, R.Range);
                    if (UseQ && Q.IsReady())
                    {
                        Q.UpdateSourcePosition(vec);
                    }
                     
                    if (UseE && E.IsReady())
                    {
                        E.UpdateSourcePosition(vec);
                    }
                    R.Cast(vec);
                }
                if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                {
                    mySpellcast.Linear(Target, Q, HitChance.High);
                }
                if (UseW && W.IsReady())
                {
                    mySpellcast.PointBlank(Target, W, W.Range);
                } 
                if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                {
                    mySpellcast.Linear(Target, E, HitChance.High, true);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.EnoughMana(Root.Item("EC.Ahri.Farm.ManaPercent").GetValue<Slider>().Value))
            {
                if (Root.Item("EC.Ahri.Farm.Q").GetValue<bool>() && Q.IsReady())
                {
                    myFarmManager.LaneLinear(Q, Q.Range, Player.HasBuff("ahrisoulcrusher"));
                }
            }
        }
        private void JungleClear()
        {
            if (Root.Item("EC.Ahri.Jungle.Q").GetValue<bool>() && Q.IsReady())
            {
                myFarmManager.JungleLinear(Q, Q.Range);
            }
            if (Root.Item("EC.Ahri.Jungle.E").GetValue<bool>() && E.IsReady())
            {
                
            }
        }

        private float GetDamage(Obj_AI_Hero target)
        {
            var damage = 0d;
            if (Q.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.Q);
            }
            if (W.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.W);
            }
            if (E.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.E);
            }
            if (R.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.R);
            }
            return (float)damage;
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
                    LaneClear();
                    break;
                case myOrbwalker.OrbwalkingMode.JungleClear:
                    JungleClear();
                    break;
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "ahriorbofdeception") || (spell.SData.Name.ToLower() == "ahrifoxfire") || (spell.SData.Name.ToLower() == "ahriseduce"))
                {
                    LastSpell = myUtility.TickCount;
                }
                if (spell.SData.Name.ToLower() == "ahritumble")
                {
                    LastR = myUtility.TickCount;
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Ahri.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Linear(gapcloser.Sender, E, HitChance.High, true));
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Ahri.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Ahri.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Ahri.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Ahri.Draw.R").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
