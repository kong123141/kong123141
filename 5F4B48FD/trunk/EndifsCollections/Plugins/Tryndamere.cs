using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
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
                custommenu.AddItem(new MenuItem("UndyingRageHP", "HP <").SetValue(new Slider(20)));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("UseEComboValue", "E Extends").SetValue(new Slider(50, 0, 225)));
                combomenu.AddItem(new MenuItem("TurretDive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("TDHaveR", "R Check").SetValue(false));
                combomenu.AddItem(new MenuItem("UseItemCombo", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclear = new Menu("Farm", "Farm");
            {
                laneclear.AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
                laneclear.AddItem(new MenuItem("EFarmType", "E").SetValue(new StringList(new[] { "Any (Slider Value)", "Furthest" })));
                laneclear.AddItem(new MenuItem("EFarmValue", "(Any) E More Than").SetValue(new Slider(1, 1, 5)));
                config.AddSubMenu(laneclear);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EPredHitchance", "E Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("UseWMisc", "W Gapcloser").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawW", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawE", "E").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawR", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }

        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var CastItems = config.Item("UseItemCombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToPhysical(target)) return;                
                if (CastItems) { myUtility.UseItems(0, target); }
                try
                {
                    if (UseE && E.IsReady() && !Player.IsWindingUp)
                    {
                        EPredict(target);
                    }
                    if (CastItems)
                    {
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= 450f)
                        {
                            myUtility.UseItems(1, target);
                        }
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < 500f)
                        {
                            myUtility.UseItems(3, null);
                        }
                    }
                }
                catch { }
            }
            
        }
        private void Harass()
        {
            var UseW = config.Item("UseWHarass").GetValue<bool>();
            var UseE = config.Item("UseEHarass").GetValue<bool>();

            if (UseW && W.IsReady() && !Player.IsWindingUp && !Player.IsDashing())
            {
                var ValidW = HeroManager.Enemies.Where(enemy => !enemy.IsDead && enemy.IsValidTarget() && Vector3.Distance(Player.ServerPosition, enemy.ServerPosition) < W.Range);
                if (ValidW.Any())
                {
                    W.Cast();
                }
            }
            if (UseE && E.IsReady() && !Player.IsWindingUp)
            {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (target.UnderTurret(true)) return;
                if (Player.ServerPosition.Extend(target.ServerPosition, Vector3.Distance(Player.ServerPosition, target.ServerPosition) + E.Width).UnderTurret(true)) return;                
                E.Cast(Player.ServerPosition.Extend(target.ServerPosition, Vector3.Distance(Player.ServerPosition, target.ServerPosition) + E.Width));
            }
        }
        private void LaneClear()
        {
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady())
            {
                var MinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                switch (config.Item("EFarmType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        var ELine = E.GetLineFarmLocation(MinionsE);
                        if (ELine.Position.IsValid() && !ELine.Position.To3D().UnderTurret(true) && Vector3.Distance(Player.ServerPosition, ELine.Position.To3D()) > Player.AttackRange)
                        {
                            if (ELine.MinionsHit > config.Item("EFarmValue").GetValue<Slider>().Value && !Player.IsWindingUp && !myOrbwalker.IsWaiting())
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
            var largemobs = myUtility.GetLargeMonsters(E.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
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
                if (myUtility.PlayerHealthPercentage <= config.Item("UndyingRageHP").GetValue<Slider>().Value)
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
            switch (config.Item("EPredHitchance").GetValue<StringList>().SelectedIndex)
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
        private void EPredict(Obj_AI_Base target)
        {
            PredictionOutput pred = E.GetPrediction(target);
            if (pred.CollisionObjects.Count == 0 && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < E.Range)
            {
                var test1 = Prediction.GetPrediction(target, E.Instance.SData.SpellCastTime).CastPosition;
                
                float movement = target.MoveSpeed * 100 / 1000;
                if (target.Distance(test1) > movement)
                {
                    var pos = Player.ServerPosition.Extend(target.ServerPosition.Extend(test1, movement), Vector3.Distance(Player.ServerPosition, target.ServerPosition) + config.Item("UseEComboValue").GetValue<Slider>().Value);
                    if (pos.UnderTurret(true))
                    {
                        if (!config.Item("TurretDive").GetValue<bool>()) return;
                        if (config.Item("TurretDive").GetValue<bool>() && config.Item("TDHaveR").GetValue<bool>() && !R.IsReady()) return;
                        E.Cast(pos);

                    }
                    E.Cast(pos);
                }
                else
                {
                    if (pred.Hitchance >= EHitChance)
                    {
                        var pos = Player.ServerPosition.Extend(pred.CastPosition, Vector3.Distance(Player.ServerPosition, target.ServerPosition) + config.Item("UseEComboValue").GetValue<Slider>().Value);
                        if (pos.UnderTurret(true))
                        {
                            if (!config.Item("TurretDive").GetValue<bool>()) return;
                            if (config.Item("TurretDive").GetValue<bool>() && config.Item("TDHaveR").GetValue<bool>() && !R.IsReady()) return;
                            E.Cast(pos);
                        }
                        E.Cast(pos);
                    }
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
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe)
            {
                if (!Player.IsWindingUp &&
                    (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && config.Item("UseItemCombo").GetValue<bool>()) &&
                    Orbwalking.InAutoAttackRange(target))
                {
                    myUtility.UseItems(2, null);                    
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("UseWMisc").GetValue<bool>() && W.IsReady())
            {
                if (Vector3.Distance(Player.ServerPosition, gapcloser.End) < W.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    W.Cast();
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("DrawW").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Color.White);
            }
            if (config.Item("DrawE").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.White);
            }
        }
    }
}
