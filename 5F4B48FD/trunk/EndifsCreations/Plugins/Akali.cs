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
    class Akali : PluginData
    {
        public Akali()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 700); //400rad
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R, 700); //100-200 pass through, 3 ammo

            R.SetTargetted(0.5f, 1500f);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Akali.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Akali.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Akali.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Akali.Combo.R", "Use R").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Akali.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Akali.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Akali.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Akali.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(W.Range, TargetSelector.DamageType.Magical, true);

            var UseQ = Root.Item("EC.Akali.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Akali.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Akali.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Akali.Combo.R").GetValue<bool>();
            if (UseE && E.IsReady())
            {
                mySpellcast.PointBlank(null, E, 290);
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {
                    if (myUtility.ImmuneToMagic(Target)) return;
                    if (UseQ && Q.IsReady() && !Target.HasBuff("AkaliMota") && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.Unit(Target, Q);
                    }
                    if (UseW && W.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.CircularBetween(Target, W);
                    }
                    if (UseR && R.IsReady() && Player.GetBuffCount("AkaliShadowDance") > 0 && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        if (R.IsKillable(Target))
                        {
                            mySpellcast.Unit(Target, R);
                        }
                        if (TwilightShroud != null && Vector3.Distance(Target.Position, TwilightShroud.Position) <= 300)
                        {
                            mySpellcast.Unit(Target, R);
                        }
                        else if (Target.HasBuff("AkaliMota"))
                        {
                            mySpellcast.Unit(Target, R);
                        }
                    }
                }
                catch { }
            }
        }
        
        private GameObject TwilightShroud;

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
                if ((spell.SData.Name.ToLower() == "akalimota") || (spell.SData.Name.ToLower() == "akalismokebomb") || (spell.SData.Name.ToLower() == "akalishadowswipe"))
                {
                    LastSpell = myUtility.TickCount;
                }
                if (spell.SData.Name.ToLower() == "akalishadowdance")
                {
                    LastR = myUtility.TickCount;
                }
            }
        }
        protected override void OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
            {
                if (args.Target is Obj_AI_Hero)
                {
                    Target = (Obj_AI_Hero)args.Target;
                    if (args.Order == GameObjectOrder.AttackUnit 
                        && Target.HasBuff("AkaliMota") 
                        && Root.Item("EC.Akali.Combo.R").GetValue<bool>() 
                        && R.IsReady())
                    {
                        args.Process = false;
                    }
                }
            }
        }
        protected override void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Akali_") && sender.Name.Contains("smoke_bomb_tar_team_green"))
            {
                TwilightShroud = sender;                             
            }
        }
        protected override void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Akali_") && sender.Name.Contains("smoke_bomb_tar_team_green"))
            {
                TwilightShroud = null;
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Akali.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Akali.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Akali.Draw.E").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
            }
        }
    }
}
