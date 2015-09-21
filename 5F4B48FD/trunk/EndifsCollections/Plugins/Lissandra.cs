using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Lissandra : PluginData
    {
        public Lissandra()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 725); 
            W = new Spell(SpellSlot.W, 450);
            E = new Spell(SpellSlot.E, 1050);
            R = new Spell(SpellSlot.R, 550);

            Q2 = new Spell(SpellSlot.Q, 825); 

            Q.SetSkillshot(Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileFixedTravelTime, false, SkillshotType.SkillshotLine);            
            W.SetSkillshot(0.2f, 175f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 110, 850, false, SkillshotType.SkillshotLine);
            R.SetTargetted(0.4f, float.MaxValue);

            Q2.SetSkillshot(Q2.Instance.SData.SpellCastTime, Q2.Instance.SData.LineWidth, Q2.Instance.SData.MissileFixedTravelTime, false, SkillshotType.SkillshotCone);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            SpellList.Add(Q2);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Frozen Tomb", "Custom");
            {
                custommenu.AddItem(new MenuItem("UseRKey", "Key").SetValue(new KeyBind(config.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("UseRType", "R").SetValue(new StringList(new[] { "Self", "Target" })));
                custommenu.AddItem(new MenuItem("UseRDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("UseRDrawDistance", "Draw Distance").SetValue(true));
                config.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("UseE2Combo", "Use E Second Cast").SetValue(true));
                combomenu.AddItem(new MenuItem("TurretDive", "Turret Dive").SetValue(false));
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclear = new Menu("Farm", "Farm");
            {
                laneclear.AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
                laneclear.AddItem(new MenuItem("UseWFarm", "Use W").SetValue(true));
                laneclear.AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
                laneclear.AddItem(new MenuItem("WFarmValue", "W More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("EFarmValue", "E More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("FarmMana", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclear);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
                config.AddSubMenu(junglemenu);
            }  
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("QPredHitchance", "Q Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));                
                miscmenu.AddItem(new MenuItem("UseWMisc", "W Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("UseRMisc", "R Interrupts").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawW", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawE", "E").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseE = config.Item("UseECombo").GetValue<bool>();
            var UseE2 = config.Item("UseE2Combo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToMagic(target)) return;
                try
                {
                    if (UseQ && Q.IsReady() && !Player.IsWindingUp)
                    {
                        if (myUtility.ImmuneToCC(target)) return;
                        QPredict(target);
                    }
                    if (UseW && W.IsReady())
                    {
                        if (myUtility.ImmuneToCC(target)) return;
                        if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < W.Range)
                        {
                            W.Cast();
                        }
                    }
                    if (UseE && E.IsReady() && !Player.IsWindingUp && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < E.Range)                    
                    {
                        if (myUtility.TickCount - ETick > 1800)
                        {
                            E.Cast(target.ServerPosition);
                        }
                        if (UseE2 && myUtility.TickCount - ETick > (Vector3.Distance(Player.ServerPosition, target.ServerPosition)/0.7))
                        {
                            if (target.UnderTurret(true) && !config.Item("TurretDive").GetValue<bool>()) return;
                            E.Cast();
                        }
                    }
                }
                catch { }
            }           
        }
        private void Harass()
        {
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var UseW = config.Item("UseWHarass").GetValue<bool>();
            var UseE = config.Item("UseEHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                if (UseQ && Q.IsReady() && !Player.IsWindingUp)
                {                    
                    QPredict(target);
                }
                if (UseW && W.IsReady())
                {
                    if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < W.Range)
                    {
                        W.Cast();
                    }
                }
                if (UseE && E.IsReady() && !Player.IsWindingUp && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < E.Range && 
                    myUtility.TickCount - ETick > 1800)
                {
                    E.Cast(target.ServerPosition);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value)
            {
                if (!Passive) return;
            }
            if (Player.UnderTurret(true)) return;            
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
            {
                var minionQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (minionQ == null) return;
                var siegeQ = myUtility.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                if (siegeQ != null && siegeQ.IsValidTarget())
                {
                    Q.Cast(siegeQ.ServerPosition);
                }
                else
                {
                    var FurthestQ = minionQ.OrderByDescending(i => i.Distance(Player)).FirstOrDefault(x => Q.IsKillable(x));
                    if (FurthestQ != null && FurthestQ.IsValidTarget())
                    {
                        if (myUtility.IsFacing(Player, FurthestQ.ServerPosition, 60)) Q.Cast(FurthestQ.ServerPosition);
                    }
                }
            }
            if (config.Item("UseWFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
            {
                var minionW = MinionManager.GetMinions(Player.ServerPosition, W.Range);
                if (minionW == null) return;
                if (minionW.Count() > config.Item("WFarmValue").GetValue<Slider>().Value)
                {
                   W.Cast();
                }
            }
            if (config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && myUtility.TickCount - ETick > 1800)
            {
                var minionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                var ELine = E.GetLineFarmLocation(minionsE);
                if (ELine.MinionsHit > config.Item("EFarmValue").GetValue<Slider>().Value)
                {
                    if (myUtility.IsFacing(Player, ELine.Position.To3D())) E.Cast(ELine.Position);
                }
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myUtility.GetLargeMonsters(Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null)
            {
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && Q.IsInRange(mob) && !Player.IsWindingUp)
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
                if (config.Item("UseWJFarm").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp)
                {
                    if (largemobs != null && Vector3.Distance(Player.ServerPosition, largemobs.ServerPosition) < W.Range)
                    {
                        W.Cast();
                    }
                    if (Vector3.Distance(Player.ServerPosition, mob.ServerPosition) < W.Range) W.Cast();
                }
                if (config.Item("UseEJFarm").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp && myUtility.TickCount - ETick > 1800)
                {
                    if (largemobs != null)
                    {
                        E.Cast(largemobs.ServerPosition);
                    }
                    else
                    {
                        E.Cast(mob.ServerPosition);
                    }
                }
            }
        }
        private void Custom()
        {
            if (R.IsReady())
            {
                Obj_AI_Hero target; 
                double timedist = 0;
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                switch (config.Item("UseRType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        if (E.IsReady() && Player.Mana > E.Instance.ManaCost + R.Instance.ManaCost)
                        {
                            if (TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget())
                            {
                                target = TargetSelector.GetSelectedTarget();
                            }
                            else
                            {
                                target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                         Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= E.Range + 225
                                         ).OrderByDescending(i => i.CountEnemiesInRange(550)).FirstOrDefault();
                            }
                            if (target != null && target.IsValidTarget() && myUtility.TickCount - ETick > 1800)
                            {
                                E.Cast(target.ServerPosition);
                                timedist = (Vector3.Distance(Player.ServerPosition, target.ServerPosition) / 0.7);
                            }
                            if (myUtility.TickCount - ETick >= timedist)
                            {
                                E.Cast();
                                R.CastOnUnit(Player);
                            }
                            /*  1.5seconds to 1050 range
                                0.5 seconds = 350
                                1 seconds = 700
                             * 1000 = 700 / 0.7
                             */                            
                        }
                        break;
                    case 1:
                        if (TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() &&
                            Vector3.Distance(Player.ServerPosition, TargetSelector.GetSelectedTarget().ServerPosition) <= R.Range)
                        {
                            target = TargetSelector.GetSelectedTarget();
                        }
                        else
                        {
                            target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                     Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range
                                     ).OrderByDescending(i => i.CountEnemiesInRange(550)).FirstOrDefault();
                        }
                        if (target != null && target.IsValidTarget())
                        {
                            R.CastOnUnit(target);
                        }
                        break;
                }
            }
        }

        private bool Passive
        {
            get
            {
                return Player.HasBuff("lissandrapassiveready");
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
            switch (config.Item("QPredHitchance").GetValue<StringList>().SelectedIndex)
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
        private void QPredict(Obj_AI_Hero target)
        {
            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= Q.Range)
            {
                if (myUtility.MovementImpaired(target))
                {
                    Q.Cast(target.ServerPosition);
                }
                Vector3 pos;
                PredictionOutput pred = Q.GetPrediction(target);
                if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= Q.Range)
                {
                    var test1 = Prediction.GetPrediction(target, Q.Instance.SData.MissileSpeed).CastPosition;
                    float movement = target.MoveSpeed * 100 / 1000;
                    if (target.Distance(test1) > movement)
                    {
                        pos = target.ServerPosition.Extend(test1, target.MoveSpeed);
                        if (Vector3.Distance(pos, target.ServerPosition) <= Q.Width)
                        {
                            Q.Cast(pos);
                        }
                    }
                    else
                    {
                        if (pred.Hitchance >= QHitChance)
                        {
                            var vc1 = new Vector3(
                                target.ServerPosition.X + ((pred.CastPosition.X - target.ServerPosition.X) / 2),
                                target.ServerPosition.Y + ((pred.CastPosition.Y - target.ServerPosition.Y) / 2),
                                target.ServerPosition.Z);
                            if (Vector3.Distance(vc1, target.ServerPosition) <= Q.Width)
                            {
                                Q.Cast(vc1);
                            }
                        }
                    }
                }
            }
            else if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) > Q.Range && Vector3.Distance(Player.ServerPosition, target.ServerPosition) < Q2.Range)
            {
                //Find minion to bounce
                var mm = MinionManager.GetMinions(Player.ServerPosition, Q2.Range);
                if (mm == null) return;
                foreach(var i in mm)
                {
                    if (Vector3.Distance(Player.ServerPosition, i.ServerPosition) < Q.Range && 
                        myUtility.IsFacing(Player, i.ServerPosition) &&                         
                        myUtility.IsFacing(Player, target.ServerPosition) &&
                        Vector3.Distance(target.ServerPosition, i.ServerPosition) < Q2.Width)
                    {
                        Q.Cast(i.ServerPosition);
                    }
                }                
            }
        }

        private int ETick;
        
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
                if (spell.SData.Name.ToLower() == "lissandraemissile")
                {
                    ETick = myUtility.TickCount;
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (config.Item("UseRMisc").GetValue<bool>() && R.IsReady())
            {
                if (Vector3.Distance(Player.ServerPosition,sender.ServerPosition) < R.Range && args.DangerLevel == Interrupter2.DangerLevel.High)
                {
                    if (myUtility.ImmuneToCC(sender)) return;
                    R.CastOnUnit(sender);
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("UseWMisc").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.Sender.ServerPosition) < W.Range)
                {
                    W.Cast();
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (config.Item("DrawQ").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.White);
            }
            if (config.Item("DrawW").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, Color.White);
            }
            if (config.Item("DrawE").GetValue<bool>() && E.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.White);
            }
            if (R.Level > 0 && R.IsReady())
            {
                switch (config.Item("UseRType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                       if (config.Item("UseRDrawDistance").GetValue<bool>())
                        {
                            Render.Circle.DrawCircle(Player.Position, E.Range + 225, Color.Fuchsia, 7);
                        }
                        if (config.Item("UseRDrawTarget").GetValue<bool>())
                        {
                            var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                            var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                         Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= E.Range + 225
                                         ).OrderByDescending(i => i.CountEnemiesInRange(550)).FirstOrDefault();                            
                            if (target != null && target.IsValidTarget())
                            {
                                var num = EnemyList.Count(x => Vector3.Distance(target.ServerPosition, x.ServerPosition) <= 550);
                                Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.White, "Hits: " + num);
                                Render.Circle.DrawCircle(target.ServerPosition, target.BoundingRadius, Color.Lime, 7);                               
                            }
                        }
                        break;
                    case 1:
                       if (config.Item("UseRDrawDistance").GetValue<bool>())
                        {
                            Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 7);
                        }
                       if (config.Item("UseRDrawTarget").GetValue<bool>())
                       {
                           var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                           var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                        Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range
                                        ).OrderByDescending(i => i.CountEnemiesInRange(550)).FirstOrDefault();
                           if (target != null && target.IsValidTarget())
                           {
                               var num = EnemyList.Count(x => Vector3.Distance(target.ServerPosition, x.ServerPosition) < 550);
                               Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.White, "Hits: " + num);
                               Render.Circle.DrawCircle(target.ServerPosition, target.BoundingRadius, Color.Lime, 7);
                           }
                       }
                        break;
                }
            }
        }
    }
}
  