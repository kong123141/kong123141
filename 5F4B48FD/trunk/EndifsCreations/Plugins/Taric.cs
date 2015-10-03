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
    class Taric : PluginData
    {
        public Taric()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 750);
            W = new Spell(SpellSlot.W, 200);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 200);

            E.SetTargetted(0.29f, 1400f);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Taric.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Taric.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Taric.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Taric.Combo.R", "Use R").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Taric.Misc.E", "E Gapclosers").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Taric.Misc.E2", "E Interrupts").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Taric.Draw.E", "E").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Magical);

            var UseQ = Root.Item("EC.Taric.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Taric.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Taric.Combo.E").GetValue<bool>();
            var UseR = Root.Item("EC.Taric.Combo.R").GetValue<bool>();
            if (UseQ && Q.IsReady())
            {
                if (myUtility.PlayerHealthPercentage < 75 && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                {
                    mySpellcast.Unit(null, Q);
                }
                else
                {
                    var Allies = HeroManager.Allies.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= Q.Range).OrderBy(i => i.Health);
                    foreach (var heal in Allies.Where(x => x.Health < x.MaxHealth && x.HealthPercent < 75))
                    {
                        mySpellcast.Unit(heal, Q);
                    }
                }
            }
            if (UseW && W.IsReady())
            {
                mySpellcast.PointBlank(null, W, W.Range);
            }
            if (UseR && R.IsReady())
            {
                mySpellcast.PointBlank(null, R, R.Range, 1);
            }
            if (Target.IsValidTarget())
            {
                if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                {
                    if (myUtility.ImmuneToCC(Target) || myUtility.ImmuneToMagic(Target)) return;
                    mySpellcast.Unit(Target, E);
                }
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
                if ((spell.SData.Name.ToLower() == "imbue") || (spell.SData.Name.ToLower() == "shatter") || (spell.SData.Name.ToLower() == "dazzle"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Taric.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;                    
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.CastOnUnit(gapcloser.Sender));
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Root.Item("EC.Taric.Misc.E2").GetValue<bool>() && E.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(sender) || myUtility.ImmuneToMagic(sender)) return;
                    E.CastOnUnit(sender);
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Taric.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
        }
    }
}
