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
    class Tryndamere : PluginData
    {
        public Tryndamere()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 400);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R);

            W.SetSkillshot(W.Instance.SData.SpellCastTime, W.Instance.SData.LineWidth, W.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(E.Instance.SData.SpellCastTime, E.Instance.SData.LineWidth, E.Instance.SData.MissileSpeed, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Undying Rage", "Custom");
            {
                custommenu.AddItem(new MenuItem("EC.Tryndamere.UndyingRageHP", "HP <").SetValue(new Slider(20)));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Tryndamere.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Tryndamere.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Tryndamere.Combo.R", "R Check").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Tryndamere.Combo.EValue", "E Extends").SetValue(new Slider(50, 0, 225)));
                combomenu.AddItem(new MenuItem("EC.Tryndamere.Combo.Dive", "Turret Dive").SetValue(false));                
                combomenu.AddItem(new MenuItem("EC.Tryndamere.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Tryndamere.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Tryndamere.Harass.E", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Tryndamere.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Tryndamere.EFarmType", "E").SetValue(new StringList(new[] { "Any (Slider Value)", "Furthest" })));
                laneclearmenu.AddItem(new MenuItem("EC.Tryndamere.Farm.E.Value", "(Any) E More Than").SetValue(new Slider(1, 1, 5)));
                config.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Tryndamere.Jungle.E", "Use E").SetValue(true));
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Tryndamere.EPredHitchance", "E Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("EC.Tryndamere.Misc.W", "W Gapcloser").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Tryndamere.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Tryndamere.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Tryndamere.Draw.E", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Tryndamere.Draw.R", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            var UseE = config.Item("EC.Tryndamere.Combo.E").GetValue<bool>();
            var CastItems = config.Item("EC.Tryndamere.Combo.Items").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;                
                if (CastItems) { myItemManager.UseItems(0, Target); }
                try
                {
                    if (UseE && E.IsReady())
                    {
                        EPredict(Target);
                    }
                    if (CastItems)
                    {
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) <= 450f)
                        {
                            myItemManager.UseItems(1, Target);
                        }
                        if (Vector3.Distance(Player.ServerPosition, Target.ServerPosition) < 500f)
                        {
                            myItemManager.UseItems(3, null);
                        }
                    }
                }
                catch { }
            }
            
        }
        private void Harass()
        {
            var UseW = config.Item("EC.Tryndamere.Harass.W").GetValue<bool>();
            var UseE = config.Item("EC.Tryndamere.Harass.E").GetValue<bool>();

            if (UseW && W.IsReady() && !Player.IsWindingUp && !Player.IsDashing())
            {                
                if (Player.CountEnemiesInRange(W.Range) > 0)
                {
                    W.Cast();
                }
            }
            if (UseE && E.IsReady() && !Player.IsWindingUp)
            {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (target.UnderTurret(true) || target == null) return;
                if (Player.ServerPosition.Extend(target.ServerPosition, Vector3.Distance(Player.ServerPosition, target.ServerPosition) + E.Width).UnderTurret(true)) return;                
                E.Cast(Player.ServerPosition.Extend(target.ServerPosition, Vector3.Distance(Player.ServerPosition, target.ServerPosition) + E.Width));
            }
        }
        private void LaneClear()
        {
            if (config.Item("EC.Tryndamere.Farm.E").GetValue<bool>() && E.IsReady())
            {
                var MinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                switch (config.Item("EC.Tryndamere.EFarmType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        var ELine = E.GetLineFarmLocation(MinionsE);
                        if (ELine.Position.IsValid() && !ELine.Position.To3D().UnderTurret(true) && Vector3.Distance(Player.ServerPosition, ELine.Position.To3D()) > Player.AttackRange)
                        {
                            if (ELine.MinionsHit > config.Item("EC.Tryndamere.Farm.E.Value").GetValue<Slider>().Value && !Player.IsWindingUp && !myOrbwalker.IsWaiting())
                            {
                                E.Cast(Player.ServerPosition.Extend(ELine.Position.To3D(), Vector3.Distance(Player.ServerPosition, ELine.Position.To3D())));
                            }
                        }
                        break;
                    case 1:
                        var FurthestE = MinionsE.OrderByDescending(i => i.Distance(Player)).Where(x => !x.UnderTurret(true)).ToList();
                        foreach (var x in FurthestE)
                        {
                            if (MinionManager.GetMinions(x.ServerPosition, 200f).Count() > 1)
                            {
                                E.Cast(Player.ServerPosition.Extend(x.ServerPosition, x.BoundingRadius));
                            }
                        }
                        break;
                }
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, E.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (config.Item("EC.Tryndamere.Jungle.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null)
                    {
                        E.Cast(Player.ServerPosition.Extend(largemobs.ServerPosition, Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) + 75f));
                    }
                    else
                    {
                        E.Cast(Player.ServerPosition.Extend(mob.ServerPosition, Vector3.Distance(Player.ServerPosition, mob.ServerPosition) + 75f));
                    }
                }
            }
        }
        private void UndyingRage()
        {
            if (R.IsReady())
            {
                if (myUtility.PlayerHealthPercentage <= config.Item("EC.Tryndamere.UndyingRageHP").GetValue<Slider>().Value)
                {
                    if (Q.IsReady()) Q.Cast();
                    else R.Cast();
                }
            }
        }
        private void AutoQ()
        {
            if (Q.IsReady())
            {
                if (myUtility.PlayerHealthPercentage < 50)
                {
                    Q.Cast();
                }
            }
        }

        private HitChance EHitChance
        {
            get
            {
                return GetEHitChance();
            }
        }
        private HitChance GetEHitChance()
        {
            switch (config.Item("EC.Tryndamere.EPredHitchance").GetValue<StringList>().SelectedIndex)
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
        private void EPredict(Obj_AI_Hero target)
        {
            PredictionOutput pred = E.GetPrediction(target);
            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= E.Range)
            {
                if (pred.Hitchance >= EHitChance)
                {
                    var pos = Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, target.ServerPosition) + target.BoundingRadius + Player.BoundingRadius);//config.Item("EC.Tryndamere.Combo.EValue").GetValue<Slider>().Value);
                    if (pos.UnderTurret(true))
                    {
                        if (!config.Item("EC.Tryndamere.Combo.Dive").GetValue<bool>()) return;
                        if (config.Item("EC.Tryndamere.Combo.Dive").GetValue<bool>() && config.Item("EC.Tryndamere.TDHaveR").GetValue<bool>() && !R.IsReady()) return;
                        E.Cast(pos);
                    }
                    E.Cast(pos);
                }
            }
        }

        protected override void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsZombie)
            {
                myUtility.Reset();
                return;
            }
            UndyingRage();
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
                    AutoQ();
                    LaneClear();
                    break;
                case myOrbwalker.OrbwalkingMode.Hybrid:
                    LaneClear();
                    Harass();
                    break;
                case myOrbwalker.OrbwalkingMode.JungleClear:
                    AutoQ();
                    JungleClear();
                    break;
            }
        }
        protected override void ProcessDamageBuffer(Obj_AI_Base sender, Obj_AI_Hero target, SpellData spell, myCustomEvents.DamageTriggerType type)
        {
            if (sender != null && target.IsMe)
            {
                switch (type)
                {
                    case myCustomEvents.DamageTriggerType.Killable:
                        if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo &&
                            config.Item("EC.Tryndamere.Combo.R").GetValue<bool>() &&
                            R.IsReady())
                        {
                            R.Cast();
                        }
                        break;
                    case myCustomEvents.DamageTriggerType.TonsOfDamage:
                        if (Q.IsReady())
                        {
                            Q.Cast();
                        }
                        break;
                }
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe)
            {
                if (!Player.IsWindingUp &&
                    (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && config.Item("EC.Tryndamere.Combo.Items").GetValue<bool>()) &&
                    Orbwalking.InAutoAttackRange(target))
                {
                    myItemManager.UseItems(2, null);                    
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("EC.Tryndamere.Misc.W").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= W.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;                   
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast());
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Tryndamere.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Tryndamere.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
        }
    }
}
