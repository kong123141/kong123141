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
    class Leona : PluginData
    {
        public Leona()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 1200);

            E.SetSkillshot(0.25f, 100f, 2000f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1f, 300f, float.MaxValue, false, SkillshotType.SkillshotCircle);
      
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Solar Flare", "Custom");
            {
                custommenu.AddItem(new MenuItem("EC.Leona.UseRKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("EC.Leona.UseRHitChecks", "Only if hits").SetValue(true));
                custommenu.AddItem(new MenuItem("EC.Leona.UseRDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("EC.Leona.UseRDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Leona.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Leona.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Leona.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Leona.Combo.Dive", "Turret Dive").SetValue(false));
                combomenu.AddItem(new MenuItem("EC.Leona.Combo.Items", "Use Items").SetValue(true));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Leona.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Leona.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Leona.Harass.E", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Leona.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Leona.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Leona.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Leona.Farm.W.Value", "W More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Leona.Farm.E.Value", "E More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Leona.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Leona.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Leona.Jungle.W", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Leona.Jungle.E", "Use E").SetValue(true));
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Leona.ERPredHitchance", "E/R Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("EC.Leona.Misc.Q", "Q Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Leona.Misc.E", "E Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Leona.Misc.W", "W Shields").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Leona.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Leona.Draw.E", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            var UseW = config.Item("EC.Leona.Combo.W").GetValue<bool>();
            var UseE = config.Item("EC.Leona.Combo.E").GetValue<bool>();
            var CastItems = config.Item("EC.Leona.Combo.Items").GetValue<bool>();
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToDeath(Target)) return;                
                if (CastItems) { myItemManager.UseItems(0, Target); }
                try
                {
                    if (UseW && W.IsReady())
                    {
                        if (myUtility.ImmuneToCC(Target) || myUtility.ImmuneToMagic(Target)) return;
                        if (Vector3.Distance(ObjectManager.Player.ServerPosition, Target.ServerPosition) < 275f)
                        {
                           // W.Cast();
                        }
                    }
                    if (UseE && E.IsReady())
                    {
                        if (Target.UnderTurret(true) && !config.Item("EC.Leona.Combo.Dive").GetValue<bool>()) return;
                        if (myUtility.ImmuneToCC(Target) || myUtility.ImmuneToMagic(Target)) return;
                        mySpellcast.Linear(Target, E, ERHitChance);
                    } 
                    if (CastItems)
                    {
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
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical, false);            
            var UseW = config.Item("EC.Leona.Harass.W").GetValue<bool>();
            var UseE = config.Item("EC.Leona.Harass.E").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (UseW && W.IsReady())
                {
                    if (Player.UnderTurret(true)) return;
                    if (Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) < 275f)
                    {
                        W.Cast();
                    }
                }
                if (UseE && E.IsReady())
                {
                    if (target.UnderTurret(true)) return;
                    EPredict(target);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("EC.Leona.Farm.ManaPercent").GetValue<Slider>().Value) return;
            if (config.Item("EC.Leona.Farm.W").GetValue<bool>())
            {
                var minionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range).Count();
                if (minionsW > config.Item("EC.Leona.Farm.W.Value").GetValue<Slider>().Value) W.Cast();
            }
            if (config.Item("EC.Leona.Farm.E").GetValue<bool>() && E.IsReady())
            {
                var minionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                var ELine = E.GetLineFarmLocation(minionsE);
                if (ELine.MinionsHit > config.Item("EC.Leona.Farm.E.Value").GetValue<Slider>().Value && !Player.IsWindingUp && !myOrbwalker.IsWaiting())
                {
                    if (Player.UnderTurret(true)) return;
                    var target = TargetSelector.GetTarget(Vector3.Distance(ELine.Position.To3D(), Player.ServerPosition) + E.Width, TargetSelector.DamageType.Magical);
                    if (target == null)
                    {
                        if (myUtility.IsFacing(Player, ELine.Position.To3D())) E.Cast(ELine.Position);
                    }
                }   
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myFarmManager.GetLargeMonsters(Player.Position, E.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null && !Player.IsWindingUp)
            {
                if (config.Item("EC.Leona.Jungle.Q").GetValue<bool>() && Q.IsReady())
                {
                    if (largemobs != null && Orbwalking.InAutoAttackRange(largemobs))
                    {
                        Q.Cast();
                    }
                }
                if (config.Item("EC.Leona.Jungle.W").GetValue<bool>() && W.IsReady())
                {
                    if (largemobs != null && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < W.Width)
                    {
                        W.Cast(largemobs);
                    }
                    else if (Vector3.Distance(Player.ServerPosition, mob.ServerPosition) < W.Width)
                    {
                        W.Cast(mob);
                    }
                }
                if (config.Item("EC.Leona.Jungle.E").GetValue<bool>() && E.IsReady())
                {
                    if (largemobs != null && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < E.Range)
                    {
                        E.Cast(largemobs.ServerPosition);
                    }
                }
            }            
        }
        private void Custom()
        {
            if (R.IsReady())
            {
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                             Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range
                             ).OrderByDescending(i => i.CountEnemiesInRange(R.Width)).FirstOrDefault();
                if (target != null && target.IsValidTarget())
                {
                    PredictionOutput pred = R.GetPrediction(target);
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < R.Range)
                    {
                        var test1 = Prediction.GetPrediction(target, R.Instance.SData.MissileSpeed).CastPosition;
                        float movement = target.MoveSpeed * 100 / 1000;
                        if (target.Distance(test1) > movement)
                        {
                            R.Cast(target.ServerPosition.Extend(Player.ServerPosition.Extend(test1, 0.625f * target.MoveSpeed), R.Width / 2));
                        }
                        else
                        {
                            if (pred.Hitchance >= ERHitChance) R.Cast(pred.CastPosition);
                        }
                    }
                }
            }
        }

        private HitChance ERHitChance
        {
            get
            {
                return GetERHitChance();
            }
        }
        private HitChance GetERHitChance()
        {
            switch (config.Item("EC.Leona.ERPredHitchance").GetValue<StringList>().SelectedIndex)
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
            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < E.Range)
            {
                PredictionOutput pred = E.GetPrediction(target);
                if (pred.CollisionObjects.Count == 0)
                {
                    var test1 = Prediction.GetPrediction(target, E.Instance.SData.SpellCastTime).CastPosition;
                    float movement = target.MoveSpeed * 100 / 1000;
                    if (target.Distance(test1) > movement) E.Cast(target.ServerPosition.Extend(test1, E.Instance.SData.MissileSpeed * target.MoveSpeed));
                    else
                    {
                        if (pred.Hitchance >= ERHitChance) E.Cast(pred.CastPosition);
                    }
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
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Custom && (config.Item("EC.Leona.UseRHitChecks").GetValue<bool>()))
            {
                if (args.Slot == SpellSlot.R && myUtility.SpellHits(R).Item1 == 0)
                {
                    args.Process = false;
                }
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if (spell.SData.Name.ToLower() == "leonashieldofdaybreak")
                {
                    myOrbwalker.ResetAutoAttackTimer();
                }
            }
            if (unit is Obj_AI_Hero && unit.IsEnemy && !spell.SData.IsAutoAttack() && W.IsReady())
            {
                if (config.Item("EC.Leona.Misc.W").GetValue<bool>() || config.Item("EC.Leona.Combo.W").GetValue<bool>() && myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)                    
                {
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        var box = new Geometry.Polygon.Rectangle(spell.Start, spell.End, Player.BoundingRadius);
                        if (box.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 100))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast());
                        }
                    }
                    else if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Unit) || spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit)) && spell.Target != null && spell.Target.IsMe)
                    {
                        W.Cast();
                    }
                    else if (spell.End.Distance(Player.ServerPosition) <= 100)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast());
                    }
                }
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe) return;
            if (unit.IsMe && Q.IsReady())
            {
                if (target.Name.ToLower().Contains("ward"))
                {
                    Q.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }                
                if (target is Obj_AI_Minion && target.IsValidTarget())
                {
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear && config.Item("EC.Leona.Farm.Q").GetValue<bool>())
                    {
                        if (myUtility.PlayerManaPercentage < config.Item("EC.Leona.Farm.ManaPercent").GetValue<Slider>().Value) return;
                        if (Q.IsKillable((Obj_AI_Minion)target) ||
                            (Player.GetAutoAttackDamage((Obj_AI_Minion)target) + Player.GetSpellDamage((Obj_AI_Minion)target, SpellSlot.Q)) >= target.Health)
                        {
                            Q.Cast();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                        }
                    }
                }
                if (target is Obj_AI_Hero && target.IsValidTarget())
                {
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && config.Item("EC.Leona.Combo.Q").GetValue<bool>() )
                    {
                        if (myUtility.ImmuneToCC((Obj_AI_Hero)target) || myUtility.ImmuneToMagic((Obj_AI_Hero)target)) return;
                        Q.Cast();
                        Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                    }
                    if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Harass && config.Item("EC.Leona.Harass.Q").GetValue<bool>())
                    {
                        Q.Cast();
                        Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                    }
                }                
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("EC.Leona.Misc.Q").GetValue<bool>() && Q.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= Q.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () => Q.Cast());
                }
            }
            if (config.Item("EC.Leona.Misc.E").GetValue<bool>() && E.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= E.Range)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    Utility.DelayAction.Add(myHumazier.ReactionDelay, () =>  EPredict(gapcloser.Sender));                   
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("EC.Leona.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            if (config.Item("EC.Leona.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
            }
            if (R.Level > 0 && R.IsReady())
            {
                if (config.Item("EC.Leona.UseRDrawDistance").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 7);
                }
                if (config.Item("EC.Leona.UseRDrawTarget").GetValue<bool>())
                {
                    var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                    var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                 Vector3.Distance(Player.ServerPosition, x.ServerPosition) < R.Range
                                 ).OrderByDescending(i => i.CountEnemiesInRange(R.Width)).FirstOrDefault();
                    if (target != null && target.IsValidTarget())
                    {
                        Vector3 pos;
                        PredictionOutput pred = R.GetPrediction(target);
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < R.Range)
                        {
                            var num = target.CountEnemiesInRange(R.Width);
                            Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.White, "Hits: " + num);
                            var test1 = Prediction.GetPrediction(target, R.Instance.SData.MissileSpeed).CastPosition;
                            float movement = target.MoveSpeed * 100 / 1000;
                            if (target.Distance(test1) > movement)
                            {
                                pos = target.ServerPosition.Extend(Player.ServerPosition.Extend(test1, R.Instance.SData.SpellCastTime * target.MoveSpeed), R.Width / 2);
                                Render.Circle.DrawCircle(pos, R.Width, Color.Lime, 7);
                            }
                            else
                            {
                                if (pred.Hitchance >= ERHitChance)
                                {
                                    Render.Circle.DrawCircle(pred.CastPosition, R.Width, Color.Lime, 7);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
