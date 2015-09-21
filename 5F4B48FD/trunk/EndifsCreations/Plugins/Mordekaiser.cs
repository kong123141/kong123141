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
    class Mordekaiser : PluginData
    {
        public Mordekaiser()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1000); //750 cast range, 250 aura
            E = new Spell(SpellSlot.E, 675);            
            R = new Spell(SpellSlot.R, 650);

            E.SetSkillshot(E.Instance.SData.SpellCastTime, E.Instance.SData.LineWidth, E.Speed, false, SkillshotType.SkillshotCone);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Children of the Grave", "Custom");
            {
                custommenu.AddItem(new MenuItem("EC.Mordekaiser.UseAutoR", "Auto").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Mordekaiser.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Mordekaiser.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Mordekaiser.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Mordekaiser.Combo.R", "Use R").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Mordekaiser.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Mordekaiser.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Mordekaiser.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            var UseW = config.Item("EC.Mordekaiser.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Mordekaiser.Combo.E").GetValue<bool>();
            var UseR = config.Item("EC.Mordekaiser.Combo.R").GetValue<bool>();

            if (UseW && W.IsReady() && !Player.HasBuff("mordekaisercreepingdeath"))
            {
                var WAllies = HeroManager.Allies.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= W.Range);
                foreach (var waura in WAllies)
                {
                    if (W.IsInRange(waura) && 
                        Vector3.Distance(Player.ServerPosition, waura.ServerPosition) <= 300 &&
                        HeroManager.Enemies.Any(x => Vector3.Distance(waura.ServerPosition, x.ServerPosition) <= 300))
                    {
                        W.Cast(waura);
                    }
                }
            }
            if (GetPet() != null)
            {
                Obj_AI_Base Pet = GetPet();
                if (UseW && W.IsReady())
                {
                    W.Cast(Pet);
                }
                var pettarget = HeroManager.Enemies.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= 1500).OrderByDescending(i => myRePriority.ResortDB(i.ChampionName)).ThenBy(u => u.Health).FirstOrDefault();
                if (pettarget != null)
                {
                    PetWalker(pettarget);
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;                
                try
                {
                    if (myUtility.ImmuneToDeath(Target)) return;
                    if (UseE && E.IsReady())
                    {
                        E.CastIfHitchanceEquals(Target, HitChance.High);
                    }
                    if (UseR && R.IsReady() && !RPet)
                    {
                        if (myUtility.ImmuneToMagic(Target)) return;
                        if (Target.HealthPercent < 30)
                        {
                            R.CastOnUnit(Target);
                        }
                    }
                }
                catch { }
            }
        }
        private void Custom()
        {
            if (!RPet)
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToMagic(x) && R.IsKillable(x));
                foreach (var x in EnemyList.OrderByDescending(z => myRePriority.ResortDB(z.ChampionName)))
                {
                    R.CastOnUnit(x);
                }
            }
        }

        private bool RPet
        {
            get { return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name == "mordekaisercotgguide"; }
        }
        private Obj_AI_Minion GetPet()
        {
            Obj_AI_Minion Pet = null;
            foreach (var unit in ObjectManager.Get<Obj_AI_Minion>().Where(x => !x.IsMe &&x.IsValid && !x.IsDead && x.Name == Player.Name))
            {
                Pet = unit;
            }
            return Pet;
        }
        private int PetLastOrder;
        private void PetWalker(Obj_AI_Base target)
        {
            if (RPet)
            {
                Obj_AI_Base Pet = GetPet();
                if (myUtility.TickCount > PetLastOrder + 250)
                {
                    if (target != null && !Pet.IsWindingUp)
                    {
                        R.Cast(target);
                    }
                    else
                    {
                        R.Cast(Game.CursorPos);
                    }
                    PetLastOrder = myUtility.TickCount;
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
            if (config.Item("EC.Mordekaiser.UseAutoR").GetValue<bool>())
            {
                Custom();
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
        
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero && args.Target.Team != Player.Team)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(args.Target))
                {
                    if (config.Item("EC.Mordekaiser.Combo.Q").GetValue<bool>() && Q.IsReady())
                    {
                        Q.Cast();
                    }
                }
            }
        }/*
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
                {
                    if (config.Item("EC.Mordekaiser.Combo.Q").GetValue<bool>() && Q.IsReady() && Orbwalking.InAutoAttackRange(target))
                    {
                        Q.Cast();
                    }
                }
            }
        }*/
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Mordekaiser.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (config.Item("EC.Mordekaiser.Draw.R").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.White);
            }
        }
    }
}
