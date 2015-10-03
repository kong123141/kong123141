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
    class Annie : PluginData
    {
        public Annie()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 625);
            W = new Spell(SpellSlot.W, 625);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 600);

            Q.SetTargetted(Q.Instance.SData.SpellCastTime, 1400);
            W.SetSkillshot(W.Instance.SData.SpellCastTime, 50*(float)Math.PI / 180, float.MaxValue, false, SkillshotType.SkillshotCone, Player.ServerPosition);
            R.SetSkillshot(R.Instance.SData.SpellCastTime, 290, float.MaxValue, false, SkillshotType.SkillshotCircle);                    
            
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var custommenu = new Menu("Summon: Tibbers ", "Custom");
            {
                custommenu.AddItem(new MenuItem("EC.Annie.UseRKey", "Key").SetValue(new KeyBind(Root.Item("CustomMode_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));  //T
                custommenu.AddItem(new MenuItem("EC.Annie.UseRType", "R").SetValue(new StringList(new[] { "Tibbers!", "Flash Tibbers!"  })));
                custommenu.AddItem(new MenuItem("EC.Annie.UseRHitChecks", "Only if hits").SetValue(true));
                custommenu.AddItem(new MenuItem("EC.Annie.UseRStun", "With Stun").SetValue(true));
                custommenu.AddItem(new MenuItem("EC.Annie.UseRDrawTarget", "Draw Target").SetValue(true));
                custommenu.AddItem(new MenuItem("EC.Annie.UseRDrawDistance", "Draw Distance").SetValue(true));
                Root.AddSubMenu(custommenu);
            }
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Annie.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Annie.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Annie.Combo.E", "Use E").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {
                harassmenu.AddItem(new MenuItem("EC.Annie.Harass.Q", "Use Q").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Annie.Harass.W", "Use W").SetValue(true));
                harassmenu.AddItem(new MenuItem("EC.Annie.Harass.E", "Use E").SetValue(true));
                Root.AddSubMenu(harassmenu);
            }
            var laneclearmenu = new Menu("Farm", "Farm");
            {
                laneclearmenu.AddItem(new MenuItem("EC.Annie.Farm.Q", "Use Q").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Annie.Farm.W", "Use W").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Annie.Farm.E", "Use E").SetValue(true));
                laneclearmenu.AddItem(new MenuItem("EC.Annie.Farm.W.Value", "W More Than").SetValue(new Slider(1, 1, 5)));
                laneclearmenu.AddItem(new MenuItem("EC.Annie.Farm.ManaPercent", "Farm Mana >").SetValue(new Slider(50)));
                Root.AddSubMenu(laneclearmenu);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("EC.Annie.Jungle.Q", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Annie.Jungle.W", "Use W").SetValue(true));
                junglemenu.AddItem(new MenuItem("EC.Annie.Jungle.E", "Use E").SetValue(true));
                Root.AddSubMenu(junglemenu);
            }  
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.Annie.WPredHitchance", "W Hitchance").SetValue(new StringList(new[] { "Low", "Medium", "High" })));
                miscmenu.AddItem(new MenuItem("EC.Annie.UseStunMisc", "Stun Interrupts").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Annie.UseStun2Misc", "Stun Gapcloser").SetValue(false));
                miscmenu.AddItem(new MenuItem("EC.Annie.Misc.E", "E Shields").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Annie.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Annie.Draw.W", "W").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = Root.Item("EC.Annie.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Annie.Combo.W").GetValue<bool>();
            var UseE = Root.Item("EC.Annie.Combo.E").GetValue<bool>();
            if (RPet)
            {                
                if (Target.IsValidTarget() && Orbwalking.InAutoAttackRange(Target))
                {
                    PetWalker(Target);
                }
                else
                {
                    var pettarget = HeroManager.Enemies.Where(x => Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= 1500).OrderByDescending(i => myRePriority.ResortDB(i.ChampionName)).ThenBy(u => u.Health).FirstOrDefault();
                    if (pettarget != null)
                    {                        
                        PetWalker(pettarget);
                    }
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                if (myUtility.ImmuneToMagic(Target)) return;
                try
                {                    
                    if (Pyromania)
                    {
                        if (myUtility.ImmuneToCC(Target)) return;
                        if (UseW && W.IsReady())
                        {
                            if (Target.CountEnemiesInRange(200) > 0)
                            {
                                W.CastIfWillHit(Target, Target.CountEnemiesInRange(200));
                            }
                            else
                            {
                                if (!Q.IsReady() || Q.Level <= 0)
                                {
                                    W.CastIfHitchanceEquals(Target, WHitChance);
                                }
                            }
                        }
                        if (UseQ && Q.IsReady()) Q.CastOnUnit(Target);
                    }
                    else
                    {
                        if (UseQ && Q.IsReady())
                        {
                            Q.CastOnUnit(Target);
                        }
                        if (UseW && W.IsReady())
                        {
                            W.CastIfHitchanceEquals(Target, WHitChance);
                        }                       
                    }
                }
                catch { }
            }           
        }
        private void Harass()
        {
            var UseQ = Root.Item("EC.Annie.Harass.Q").GetValue<bool>();
            var UseW = Root.Item("EC.Annie.Harass.W").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                if (Pyromania && !Player.IsWindingUp)
                {
                    if (myUtility.ImmuneToCC(target)) return;
                    if (Pyromania)
                    {
                        if (myUtility.ImmuneToCC(target)) return;
                        if (UseW && W.IsReady())
                        {
                            if (target.CountEnemiesInRange(250) > 0)
                            {
                                W.CastIfWillHit(target, target.CountEnemiesInRange(250));
                            }
                            else
                            {
                                if (!Q.IsReady() || Q.Level <= 0)
                                {
                                    W.CastIfHitchanceEquals(target, WHitChance);
                                }
                            }

                        }
                        if (UseQ && Q.IsReady()) Q.CastOnUnit(target);
                    }
                }
                else
                {
                    if (UseQ && Q.IsReady() && !Player.IsWindingUp)
                    {
                        Q.CastOnUnit(target);
                    }
                    if (UseW && W.IsReady() && !Player.IsWindingUp)
                    {
                        W.Cast(target.Position);
                    }
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.EnoughMana(Root.Item("EC.Annie.Farm.ManaPercent").GetValue<Slider>().Value) && !Pyromania)
            {
                if (Root.Item("EC.Annie.Farm.Q").GetValue<bool>() && Q.IsReady() && myUtility.TickCount - LastAA > myHumazier.ReactionDelay)
                {
                    myFarmManager.LaneLastHit(Q, Q.Range, null, true);
                }
                if (Root.Item("EC.Annie.Farm.W").GetValue<bool>() && W.IsReady() && myUtility.TickCount - LastAA > myHumazier.ReactionDelay)
                {
                    myFarmManager.LaneCircular(W, W.Range, W.Width / 2);
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
                if (myUtility.TickCount - LastAA < myUtility.RandomDelay(300, 400)) return;
                if (Root.Item("EC.Annie.Jungle.Q").GetValue<bool>() && Q.IsReady() && Q.IsInRange(mob) && !Player.IsWindingUp && myUtility.TickCount - LastW > myUtility.RandomDelay(300, 400))
                {
                    if (largemobs != null)
                    {
                        Q.CastOnUnit(largemobs);
                    }
                    else
                    {
                        Q.CastOnUnit(mob);
                    }
                }
                if (Root.Item("EC.Annie.Jungle.W").GetValue<bool>() && W.IsReady() && !Player.IsWindingUp && myUtility.TickCount - LastQ > myUtility.RandomDelay(300,400))
                {
                    if (largemobs != null)
                    {
                        W.Cast(largemobs.ServerPosition);
                    }
                    else
                    {
                        if (myUtility.IsFacing(Player, mob.ServerPosition, 75)) W.Cast(mob.ServerPosition);
                    }
                }
                if (Root.Item("EC.Annie.Jungle.E").GetValue<bool>() && E.IsReady() && !Player.IsWindingUp)
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
                if (Root.Item("EC.Annie.UseRStun").GetValue<bool>() && !Pyromania) return;
                Obj_AI_Hero target; 
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                switch (Root.Item("EC.Annie.UseRType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        if (TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget())
                        {
                            target = TargetSelector.GetSelectedTarget();
                        }
                        else 
                        { 
                            target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                     Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range)
                                     .OrderByDescending(i => i.CountEnemiesInRange(290))
                                     .FirstOrDefault();
                        }
                        if (target != null && target.IsValidTarget())
                        {
                            PredictionOutput pred = R.GetPrediction(target);
                            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) <= R.Range)
                            {
                                if (pred.Hitchance >= HitChance.High)
                                {
                                    R.Cast(target.Position);
                                }
                            }
                        }
                        break;
                    case 1:
                        if (FlashSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready)
                        {
                            if (TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() &&
                                Vector3.Distance(Player.ServerPosition, TargetSelector.GetSelectedTarget().ServerPosition) <= R.Range + 425f)
                            {
                                target = TargetSelector.GetSelectedTarget();
                            }
                            else
                            {
                                target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) > 425f &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range + 425f)
                                    .OrderByDescending(i => i.CountEnemiesInRange(290))
                                    .FirstOrDefault();
                            }
                            if (target != null && target.IsValidTarget())
                            {
                                R.UpdateSourcePosition(Player.ServerPosition.Extend(target.ServerPosition, 425f));
                                Player.Spellbook.CastSpell(FlashSlot, Player.ServerPosition.Extend(target.ServerPosition, 425f));
                                R.Cast(target.Position);
                            }
                        }
                        break;
                }
            }
        }
        
        private bool Pyromania
        {
            get
            {
                return Player.HasBuff("pyromania_particle");                
            }
        }  
        private HitChance WHitChance
        {
            get
            {
                return GetWHitChance();
            }
        }
        private HitChance GetWHitChance()
        {
            switch (Root.Item("EC.Annie.WPredHitchance").GetValue<StringList>().SelectedIndex)
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

        private bool RPet
        {
            get { return ObjectManager.Get<Obj_AI_Minion>().Any(minion => minion.IsValid && minion.IsAlly && !minion.IsDead && minion.CharData.BaseSkinName.Contains("annietibbers")); }
        }
        private Obj_AI_Minion GetPet()
        {
            Obj_AI_Minion Pet = null;
            foreach (var unit in ObjectManager.Get<Obj_AI_Minion>().Where(x => !x.IsMe && x.Name == Player.Name))
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
                    Console.WriteLine(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name);
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
            if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Custom && (Root.Item("EC.Annie.UseRHitChecks").GetValue<bool>()))
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
                if (spell.SData.Name.ToLower() == "disintergrate")
                {
                    LastQ = myUtility.TickCount;
                }
                if (spell.SData.Name.ToLower() == "incinerate")
                {
                    LastW = myUtility.TickCount;
                }                
            }
            if (unit is Obj_AI_Minion && unit.IsEnemy)
            {
                if (Root.Item("EC.Annie.Farm.E").GetValue<bool>() && myUtility.PlayerManaPercentage > Root.Item("EC.Annie.Farm.ManaPercent").GetValue<Slider>().Value)
                {
                    E.CastOnUnit(Player);
                }
            }
            if (unit is Obj_AI_Hero && unit.IsEnemy)
            {
                if (Root.Item("EC.Annie.Misc.E").GetValue<bool>() || (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Root.Item("EC.Annie.Combo.E").GetValue<bool>()) && E.IsReady())
                {
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location) || spell.SData.TargettingType.Equals(SpellDataTargetType.Location2) || spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector) || spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        var box = new Geometry.Polygon.Rectangle(spell.Start, spell.End, Player.BoundingRadius);
                        if (box.Points.Any(point => point.Distance(Player.ServerPosition.To2D()) <= 100))
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.Cast());                       
                        }
                    }
                    else if ((spell.SData.TargettingType.Equals(SpellDataTargetType.Unit) || spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit)) && spell.Target != null && spell.Target.IsMe)
                    {
                        E.Cast();
                    }
                    else if (spell.End.Distance(Player.ServerPosition) <= 125)
                    {
                        Utility.DelayAction.Add(myHumazier.ReactionDelay, () => E.Cast());
                    }
                    else if (spell.SData.IsAutoAttack() && spell.Target != null && spell.Target.IsMe)
                    {
                        E.Cast();
                    }
                }
            }
        }
        protected override void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Root.Item("EC.Annie.UseStunMisc").GetValue<bool>())
            {
                if (sender.IsEnemy && Vector3.Distance(Player.ServerPosition, sender.ServerPosition) < 625)
                {
                    if (myUtility.ImmuneToMagic(sender) || myUtility.ImmuneToCC(sender)) return;
                    if (Pyromania)
                    {
                        if (Q.IsReady()) Q.CastOnUnit(sender);
                        else if (W.IsReady()) W.Cast(sender.ServerPosition);
                    }
                }
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                LastAA = myUtility.TickCount;
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.Annie.UseStun2Misc").GetValue<bool>())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= 625)
                {
                    if (myUtility.ImmuneToMagic(gapcloser.Sender) || myUtility.ImmuneToCC(gapcloser.Sender)) return;
                    if (Pyromania)
                    {
                        if (Q.IsReady())
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => Q.CastOnUnit(gapcloser.Sender)); 
                        }
                        else if (W.IsReady())
                        {
                            Utility.DelayAction.Add(myHumazier.ReactionDelay, () => W.Cast(gapcloser.End)); 
                        }
                    }
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.Annie.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
            if (Root.Item("EC.Annie.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);
            }
            
            if (R.Level > 0 && R.IsReady())
            {
                if (Root.Item("EC.Annie.UseRStun").GetValue<bool>() && !Pyromania) return;
                var EnemyList = HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && !x.IsInvulnerable && !myUtility.ImmuneToCC(x) && !myUtility.ImmuneToMagic(x));
                switch (Root.Item("EC.Annie.UseRType").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        if (Root.Item("EC.Annie.UseRDrawDistance").GetValue<bool>())
                        {
                            Render.Circle.DrawCircle(Player.Position, R.Range, Color.Fuchsia, 7);
                        }
                        if (Root.Item("EC.Annie.UseRDrawTarget").GetValue<bool>())
                        {
                            var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range)
                                    .OrderByDescending(i => i.CountEnemiesInRange(290))
                                    .FirstOrDefault();
                            if (target != null && target.IsValidTarget())
                            {
                                var num = target.CountEnemiesInRange(290);
                                Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.Yellow, "Hits: " + num);
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
                        break;
                    case 1:
                        if (FlashSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready)
                        {
                            if (Root.Item("EC.Annie.UseRDrawDistance").GetValue<bool>())
                            {
                                Render.Circle.DrawCircle(Player.Position, 425f, Color.Fuchsia, 7);
                                Render.Circle.DrawCircle(Player.Position, R.Range + 425f, Color.Fuchsia, 7);
                            }
                            if (Root.Item("EC.Annie.UseRDrawTarget").GetValue<bool>())
                            {
                                var target = EnemyList.Where(x => !x.InFountain() && x.IsVisible &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) > 425f &&
                                    Vector3.Distance(Player.ServerPosition, x.ServerPosition) <= R.Range + 425f)
                                    .OrderByDescending(i => i.CountEnemiesInRange(290))
                                    .FirstOrDefault();
                                if (target != null && target.IsValidTarget())
                                {
                                    var num = target.CountEnemiesInRange(290);
                                    Drawing.DrawText(Player.HPBarPosition.X + 10, Player.HPBarPosition.Y - 15, Color.White, "Hits: " + num);
                                    Render.Circle.DrawCircle(target.Position, target.BoundingRadius, Color.Lime, 7);
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}
