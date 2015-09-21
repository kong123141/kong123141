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
    class Morgana : PluginData
    {
        public Morgana()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 750);
            R = new Spell(SpellSlot.R, 600);

            Q.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileFixedTravelTime, true, SkillshotType.SkillshotLine, Player.Position);
            W.SetSkillshot(0.28f, 175f, float.MaxValue, false, SkillshotType.SkillshotCircle);                       
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Soul Shackles", "Custom");
            {
                custommenu.AddItem(new MenuItem("EC.Morgana.UseRKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("EC.Morgana.UseRDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("EC.Morgana.UseRDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Morgana.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Morgana.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Morgana.Combo.E", "Use E").SetValue(true));                
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Morgana.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Morgana.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Morgana.Harass.E", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Morgana.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Morgana.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Morgana.Farm.W.Value", "W More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Morgana.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Morgana.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Morgana.Jungle.W", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Morgana.Jungle.E", "Use E").SetValue(true));
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Morgana.QPredHitchance", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("EC.Morgana.Misc.Q", "Q Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Morgana.Misc.Q2", "Q Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Morgana.Misc.W", "W Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Morgana.Misc.E", "E Spellblock").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Morgana.UseESupport", "E Supports").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Morgana.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Morgana.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Morgana.Draw.E", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = config.Item("EC.Morgana.Combo.Q").GetValue<bool>();
            var UseW = config.Item("EC.Morgana.Combo.W").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToMagic(Target)) return;
                try
                {
                    if (UseQ && Q.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        if (myUtility.ImmuneToCC(Target)) return;
                        mySpellcast.Linear(Target, Q, QHitChance, true);
                    }
                    if (UseW && W.IsReady() && myUtility.TickCount - LastSpell > myHumazier.SpellDelay)
                    {
                        mySpellcast.CircularAoe(Target, W, HitChance.High);
                    }
                }
                catch { }
            }
        }
        private void Harass()
        {
            var UseQ = config.Item("EC.Morgana.Harass.Q").GetValue<bool>();
            var UseW = config.Item("EC.Morgana.Harass.W").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady() && Q.IsInRange(target))
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    mySpellcast.Linear(target, Q, QHitChance, true);
                }
                if (UseW && W.IsReady() && W.IsInRange(target))
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    mySpellcast.CircularAoe(target, W, HitChance.High);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("EC.Morgana.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (Player.UnderTurret(true)) return;
            if (config.Item("EC.Morgana.Farm.Q").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (minionQ == null) return;
                var SelectQ = minionQ.Where(x => Q.IsKillable(x) && Player.GetAutoAttackDamage(x) < x.Health).OrderBy(i => i.Distance(Player)).FirstOrDefault();
                if (SelectQ != null && SelectQ.IsValidTarget())
                {
                    Q.Cast(SelectQ.ServerPosition);
                }
            }
            if (config.Item("EC.Morgana.Farm.W").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
            {
                var minionW = MinionManager.GetMinions(Player.ServerPosition, W.Range);
                if (minionW == null) return;
                var wpred = W.GetCircularFarmLocation(minionW);
                if (wpred.MinionsHit > config.Item("EC.Morgana.Farm.W.Value").GetValue<Slider>().Value)
                {
                    W.Cast(wpred.Position);
                }
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (config.Item("EC.Morgana.Jungle.Q").GetValue<bool>() && Q.IsReady() && Q.IsInRange(mob) && !Player.IsWindingUp)
                {
                    if (largemobs != null)
                    {
                        Q.Cast(largemobs.ServerPosition);
                    }
                    else
                    {
                        Q.Cast(mob.Position);
                    }
                }
                if (config.Item("EC.Morgana.Jungle.W").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null)
                    {
                        W.Cast(largemobs.ServerPosition);
                    }
                    else
                    {
                        var mobW = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.Health);
                        if (mobW == null) return;
                        var wpred = W.GetCircularFarmLocation(mobW);
                        if (wpred.MinionsHit > 0)
                        {
                            W.Cast(wpred.Position);
                        }
                    }
                }
                if (config.Item("EC.Morgana.Jungle.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null)
                    {
                        E.Cast();
                    }
                }
            }
        }
        private void Custom()
        {
            if (R.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                var InShakleRange = EnemyList.Where(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range);
                var InTetherRange = EnemyList.Where(x => !x.InFountain() && 
                    (Vector3.Distance(Player.ServerPosition, x.ServerPosition) >= R.Range &&
                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range + 350)                    
                    );
                if (InShakleRange.Any() && InShakleRange.Count() > InTetherRange.Count())
                {
                    R.Cast();
                }
            }
        }
      
        private HitChance QHitChance
        {
            get
            {
                return GetQHitChance();
            }
        }
        private HitChance GetQHitChance()
        {
            switch (config.Item("EC.Morgana.QPredHitchance").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                default:
                    return HitChance.Medium;
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
                case myOrbwalker.OrbwalkingMode.Harass:
                    Harass();
                    break;
                case myOrbwalker.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                case myOrbwalker.OrbwalkingMode.Hybrid:
                    LaneClear();
                    Harass();
                    break;
                case myOrbwalker.OrbwalkingMode.JungleClear:
                    JungleClear();
                    break;
                case myOrbwalker.OrbwalkingMode.Custom:
                    Custom();
                    break;
            }            
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if ((spell.SData.Name.ToLower() == "darkbindingmissile") || (spell.SData.Name.ToLower() == "tormentedsoil"))
                {
                    LastSpell = myUtility.TickCount;
                }
            }
            if (unit is Obj_AI_Hero && unit.IsAlly && !unit.IsMe)
            {
                if (config.Item("EC.Morgana.UseESupport").GetValue<bool>())
                {
                    if (spell.Target is Obj_AI_Hero && spell.SData.IsAutoAttack() && spell.Target.IsEnemy && Vector3.Distance(Player.ServerPosition, unit.ServerPosition) <= E.Range)
                    {
                        E.CastOnUnit(unit);
                    }
                }
            }
            if (unit is Obj_AI_Hero && unit.IsEnemy && !spell.SData.IsAutoAttack() && E.IsReady())
            {
                if ((myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && config.Item("EC.Morgana.Combo.E").GetValue<bool>()) ||
                    (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass && config.Item("EC.Morgana.Harass.E").GetValue<bool>()) ||
                    (config.Item("EC.Morgana.Misc.E").GetValue<bool>())
                    )
                {
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        var box = new Geometry.Polygon.Rectangle(spell.Start, spell.End, Player.BoundingRadius);
                        if (box.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 100))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.CastOnUnit(Player));
                        }
                    }
                    else if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Unit) || spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit)) && spell.Target != null && spell.Target.IsMe)
                    {
                        E.CastOnUnit(Player);
                    }
                    else if (spell.End.Distance(Player.ServerPosition) <= 100)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.CastOnUnit(Player));
                    }
                }
            }           
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("EC.Morgana.Misc.Q").GetValue<bool>() && Q.IsReady())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) <= Q.Range)
                {
                    if (myUtility.ImmuneToMagic(sender) || myUtility.ImmuneToCC(sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Linear(sender, Q, QHitChance, true));
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("EC.Morgana.Misc.Q2").GetValue<bool>() && Q.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= Q.Range)
                {
                    if (myUtility.ImmuneToMagic(gapcloser.Sender) || myUtility.ImmuneToCC(gapcloser.Sender)) return;                    
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => mySpellcast.Linear(gapcloser.Sender, Q, QHitChance, true));
                }
            }
            if (config.Item("EC.Morgana.Misc.W").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= W.Range)
                {                    
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast(gapcloser.End));
                }
            }
            if (config.Item("EC.Morgana.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy &&  gapcloser.Sender.Target.IsMe)
                {                    
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.Cast());
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Morgana.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (config.Item("EC.Morgana.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (R.Level > 0 && R.IsReady())
            {
                if (config.Item("EC.Morgana.UseRDrawDistance").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 7);
                    Render.Circle.DrawCircle(Player.Position, R.Range + 350, Color.Fuchsia, 7);
                }
                if (config.Item("EC.Morgana.UseRDrawTarget").GetValue<bool>())
                {
                    var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                    var num = EnemyList.Count(x => !x.InFountain() && Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range);
                    Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.White, "Hits: " + num);
                }
            }
        }
    }
}
