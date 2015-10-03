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
    class Ziggs : PluginData
    {
        public Ziggs()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 850);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 900);
            R = new Spell(SpellSlot.R);

            Q2 = new Spell(SpellSlot.Q, 1400);

            Q.SetSkillshot(0.3f, 130f, 1700f, false, SkillshotType.SkillshotCircle);
            Q2.SetSkillshot(0.9f, 130f, 1700f, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f, 275f, 1750f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 100f, 1750f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1f, 500f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellList.Add(Q2);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Ziggs.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Ziggs.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Ziggs.Combo.E", "Use E").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Ziggs.Misc.E", "E Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Ziggs.Misc.E2", "E Gapcloser").SetValue(false));                
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Ziggs.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Ziggs.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Ziggs.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Ziggs.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q2.Range, TargetSelector.DamageType.Magical);

            var UseQ = Root.Item("EC.Ziggs.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Ziggs.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Ziggs.Combo.E").GetValue<bool>();
            if (UseW && W.IsReady() && SatchelCharge != null)
            {
                if (Target.IsValidTarget() && Vector3.Distance(Target.Position, SatchelCharge.Position) <= 325)
                {
                    if (myUtility.MovementDisabled(Target))
                    {
                        W.Cast();
                    }
                    if (W.IsKillable(Target))
                    {
                        W.Cast();
                    }
                }
                else if (SatchelCharge.Position.CountEnemiesInRange(325) > 0 && Vector3.Distance(Player.Position, SatchelCharge.Position) > 325)
                {
                    W.Cast();
                }
            }
            if (Target.IsValidTarget())
            {
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        if (Orbwalking.InAutoAttackRange(Target)) return;
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= Q.Range)
                        {
                            mySpellcast.CircularPrecise(Target, Q, HitChance.High, Q.Range, 130);
                        }
                        else if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) > Q.Range && Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= Q2.Range)
                        {
                            mySpellcast.CircularPrecise(Target, Q2, HitChance.High, Q.Range, 130);
                        }
                    }
                    if (UseW && W.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        if (SatchelCharge == null)
                        {
                            mySpellcast.Wall(Target, W, HitChance.High, myUtility.PlayerHealthPercentage > Target.HealthPercent);                            
                        }
                    }
                    if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.CircularAoe(Target, E, HitChance.High, E.Range, 100);
                    }
                }
                catch { }
            }
        }
        private GameObject SatchelCharge;  

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
        protected override void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Ziggs") && sender.Name.Contains("_aoe_green"))//sender.Name.Contains("_W_tar"))
            {
                SatchelCharge = sender;
            }
        }
        protected override void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Ziggs") && sender.Name.Contains("_aoe_green"))//sender.Name.Contains("_W_tar"))
            {
                SatchelCharge = null;
            }
        }
        
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Root.Item("EC.Ziggs.Misc.E").GetValue<bool>() && E.IsReady() && SatchelCharge == null)
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= E.Range + (E.Width / 2))
                {
                    if (myUtility.ImmuneToCC(sender) || myUtility.ImmuneToMagic(sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Circular(sender.ServerPosition, E));
                }
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {

            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "ziggsq") || (spell.SData.Name.ToLower() == "ziggsw") || (spell.SData.Name.ToLower() == "ziggse"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Ziggs.Misc.E2").GetValue<bool>() && E.IsReady() && SatchelCharge == null)
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) <= E.Range + (E.Width / 2))
                {
                    if (myUtility.ImmuneToMagic(gapcloser.Sender) || myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Circular(gapcloser.End, E, 0, 10, 10));  
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Ziggs.Draw.W").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, 850, Color.Cyan);
                Render.Circle.DrawCircle(Player.Position, 1400, Color.Cyan);
            }
            if (Root.Item("EC.Ziggs.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (Root.Item("EC.Ziggs.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.Ziggs.Draw.R").GetValue<bool>() && R.Level > 0 && R.IsReady())
            {
                var tomouse = Player.ServerPosition.Extend(Game.CursorPos, Vector3.Distance(Player.ServerPosition, Game.CursorPos));
                var tomax = Player.ServerPosition.Extend(Game.CursorPos, R.Range);
                var newvec = Vector3.Distance(Player.ServerPosition, tomouse) >= Vector3.Distance(Player.ServerPosition, tomax) ? tomax : tomouse;
                var wts = Drawing.WorldToScreen(newvec);
                var wtf = Drawing.WorldToScreen(Player.ServerPosition);
                Drawing.DrawLine(wtf, wts, 2, Color.GhostWhite);
                Render.Circle.DrawCircle(newvec, 550, Color.GhostWhite, 2);
                Drawing.DrawText(wts.X - 20, wts.Y - 50, Color.Yellow, "Hits: " + newvec.CountEnemiesInRange(550));
            }
        }
    }
}
