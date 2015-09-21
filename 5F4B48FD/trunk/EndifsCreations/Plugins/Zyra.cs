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
    class Zyra : PluginData
    {
        public Zyra()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 800);
            W = new Spell(SpellSlot.W, 850);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 700);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Zyra.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Zyra.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Zyra.Combo.E", "Use E").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Zyra.Misc.E", "E Gapclosers").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Zyra.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Zyra.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Zyra.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Zyra.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Zyra.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Zyra.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Zyra.Combo.E").GetValue<bool>();
            if (AllSeeds.Any())
            {
                if (UseQ && Q.IsReady())
                {
                    foreach (var x in AllSeeds.Where(o => Vector3.Distance(Player.ServerPosition, o.ServerPosition) <= Q.Range).OrderByDescending(i => i.CountEnemiesInRange(750)))
                    {
                        if (x.CountEnemiesInRange(750) > 0)
                        {
                            mySpellcast.LinearVector(x.ServerPosition, Q, 125);
                        }
                    }
                }
                else if (UseE && E.IsReady())
                {
                    foreach (var x in AllSeeds.Where(o => Vector3.Distance(Player.ServerPosition, o.ServerPosition) <= E.Range).OrderByDescending(i => i.CountEnemiesInRange(400)))
                    {
                        if (x.CountEnemiesInRange(400) > 0)
                        {
                            mySpellcast.LinearVector(x.ServerPosition, E, 125);
                        }
                    }
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                try
                {
                    if (myUtility.ImmuneToMagic(Target)) return;
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.CircularAoe(Target, Q, HitChance.High);
                    }
                    if (UseW && W.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        if (myUtility.MovementDisabled(Target))
                        {
                            mySpellcast.CircularPrecise(Target, W, HitChance.High);
                        }
                        else if (myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                        {
                            mySpellcast.CircularPrecise(Target, W, HitChance.High, 10, 50, 50);
                        }
                    }
                    if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.Linear(Target, E, HitChance.High);
                    }
                }
                catch { }
            }
        }

        private static List<Obj_AI_Minion> Seeds = new List<Obj_AI_Minion>();
        private static List<Obj_AI_Minion> AllSeeds
        {
            get { return Seeds.Where(s => s.IsValid && !s.IsMoving).ToList(); }
        }       

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                myUtility.Reset();
                return;
            }
            if (Player.IsZombie)
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable);
                foreach (var q in EnemyList.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= Q.Range).OrderBy(i => i.Health))
                {
                    mySpellcast.Linear(q, Q, HitChance.High);
                }
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
            if (sender is Obj_AI_Minion && sender.Name == "Seed")
            {
                Seeds.Add((Obj_AI_Minion)sender);
            }
        }
        protected override void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender is Obj_AI_Minion && sender.Name == "Seed")
            {
                Seeds.RemoveAll(s => s.NetworkId == sender.NetworkId);
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "zyraqfissure") || (spell.SData.Name.ToLower() == "zyraseed") || (spell.SData.Name.ToLower() == "zyragraspingroots"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("EC.Zyra.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range)
                {
                    if (myUtility.ImmuneToMagic(gapcloser.Sender) || myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.LinearVector(gapcloser.End, E));                   
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Zyra.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Zyra.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Zyra.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (config.Item("EC.Zyra.Draw.R").GetValue<bool>() && R.Level > 0 && R.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia);
                var tomouse = Player.ServerPosition.Extend(Game.CursorPos, Vector3.Distance(Player.ServerPosition, Game.CursorPos));
                var tomax = Player.ServerPosition.Extend(Game.CursorPos, R.Range);
                var newvec = Vector3.Distance(Player.ServerPosition, tomouse) >= Vector3.Distance(Player.ServerPosition, tomax) ? tomax : tomouse;
                var wts = Drawing.WorldToScreen(newvec);
                var wtf = Drawing.WorldToScreen(Player.ServerPosition);
                Drawing.DrawLine(wtf, wts, 2, Color.GhostWhite);
                Render.Circle.DrawCircle(newvec, 400, Color.GhostWhite, 2);
                Drawing.DrawText(wts.X - 20, wts.Y - 50, Color.Yellow, "Hits: " + newvec.CountEnemiesInRange(400));
            }
        }
    }
}
