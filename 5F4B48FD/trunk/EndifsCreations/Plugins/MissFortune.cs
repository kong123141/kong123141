using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace EndifsCreations.Plugins
{
    class MissFortune : PluginData
    {
        public MissFortune()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 1400);

            Q.SetTargetted(0.29f, 1400f);
            E.SetSkillshot(0.5f, 100f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 15f * 2 * (float)Math.PI / 180, 2000f, false, SkillshotType.SkillshotCone);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.MissFortune.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.MissFortune.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.MissFortune.Combo.E", "Use E").SetValue(true));
                //combomenu.AddItem(new MenuItem("EC.MissFortune.Combo.R", "Use R").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.MissFortune.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.MissFortune.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.MissFortune.Draw.R", "R").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            var UseQ = Root.Item("EC.MissFortune.Combo.Q").GetValue<bool>();
            var UseE = Root.Item("EC.MissFortune.Combo.E").GetValue<bool>();

            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {
                    if (UseQ && Q.IsReady() && Q.IsInRange(Target) && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        Q.CastOnUnit(Target);
                    }
                    if (UseE && E.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.CircularAoe(Target, E, HitChance.High, E.Range, 100);
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
                    if (Player.HasBuff("Muramana") && (myUtility.PlayerManaPercentage < 30))
                    {
                        if (Items.HasItem(3042) && Items.CanUseItem(3042)) Items.UseItem(3042);
                    }
                    break;
                case myOrbwalker.OrbwalkingMode.Combo:
                    Combo();
                    break;
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe)
            {
                if (args.Slot == SpellSlot.R)
                {
                    LastR = myUtility.TickCount;
                    mySpellcast.Pause(2000 + Game.Ping);
                }
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (args.Slot == SpellSlot.Q)
                    {
                        if (ItemData.Muramana.GetItem().IsReady() && !Player.HasBuff("Muramana") && myUtility.PlayerManaPercentage > 30)
                        {
                            if (Items.HasItem(3042) && Items.CanUseItem(3042)) Items.UseItem(3042);
                        }
                    }
                }
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "missfortunericochetshot") || (spell.SData.Name.ToLower() == "missfortunescattershot"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (Player.IsChannelingImportantSpell() || myUtility.TickCount - LastR <= 0.5f) 
            {
                args.Process = false;
            }
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (Root.Item("EC.MissFortune.Combo.W").GetValue<bool>() && W.IsReady())
                    {
                        W.Cast();
                    }
                }
            }
        }        
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.MissFortune.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.MissFortune.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (Root.Item("EC.MissFortune.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.White);
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                        Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range)
                        .OrderByDescending(i => i.CountEnemiesInRange(290))
                        .FirstOrDefault();
                if (target != null && target.IsValidTarget())
                {
                    var num = target.CountEnemiesInRange(290);
                    Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.White, "Hits: " + num);
                    PredictionOutput pred = R.GetPrediction(target);
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= R.Range)
                    {
                        if (pred.Hitchance >= HitChance.High)
                        {
                            Render.Circle.DrawCircle(target.Position, target.BoundingRadius, Color.Lime, 7);
                        }
                    }
                }
            }
        }
    }
}
