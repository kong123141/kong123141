using System;
using System.Linq;
using EndifsCollections.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Plugins
{
    class Teemo : PluginData
    {
        public Teemo()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 580);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 230);            

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("UseRCombo", "Use R").SetValue(false));                
                config.AddSubMenu(combomenu);
            }
            var harassmenu = new Menu("Harass", "Harass");
            {               
                harassmenu.AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
                config.AddSubMenu(harassmenu);
            }
            var laneclear = new Menu("Farm", "Farm");
            {
                laneclear.AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
                laneclear.AddItem(new MenuItem("UseRFarm", "Use R").SetValue(true));
                laneclear.AddItem(new MenuItem("RFarmValue", "R More Than").SetValue(new Slider(1, 1, 5)));
                laneclear.AddItem(new MenuItem("FarmMana", "Farm Mana >").SetValue(new Slider(50)));
                config.AddSubMenu(laneclear);
            }
            var junglemenu = new Menu("Jungle", "Jungle");
            {
                junglemenu.AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
                junglemenu.AddItem(new MenuItem("UseRJFarm", "Use R").SetValue(true));
                config.AddSubMenu(junglemenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("UseRMisc", "R Gapcloser").SetValue(false));
                config.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("DrawQ", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("DrawR", "R").SetValue(true));
                config.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget() ? TargetSelector.GetSelectedTarget() : TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var UseQ = config.Item("UseQCombo").GetValue<bool>();
            var UseW = config.Item("UseWCombo").GetValue<bool>();
            var UseR = config.Item("UseRCombo").GetValue<bool>();
            if (target.IsValidTarget())
            {
                if (target.InFountain()) return;
                if (myUtility.ImmuneToMagic(target)) return;
                try
                {
                    if (UseQ && Q.IsReady() && Q.IsInRange(target))
                    {
                        if (myUtility.ImmuneToCC(target)) return;
                        Q.Cast();
                    }
                    if (UseW && W.IsReady())
                    {                  
                        var dist = Vector3.Distance(Player.ServerPosition, target.ServerPosition);
                        var msDif = Player.MoveSpeed - target.MoveSpeed;
                        var reachIn = dist / msDif;
                        if (msDif < 0 && reachIn > 2 && W.IsReady())
                        {
                            W.Cast();
                        }
                        else if (msDif > 0 && W.IsReady())
                        {
                           W.Cast();
                        }
                    }
                    if (UseR && R.IsReady())
                    {
                        RPredict(target);
                    }
                }
                catch { }
            }           
        }
        private void Harass()
        {
            var UseQ = config.Item("UseQHarass").GetValue<bool>();
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.IsValidTarget())
            {
                if (UseQ && Q.IsReady())
                {
                    if (Player.UnderTurret(true) && target.UnderTurret(true)) return;
                    Q.Cast(target);
                }
            }
        }
        private void LaneClear()
        {
            if (myUtility.PlayerManaPercentage < config.Item("FarmMana").GetValue<Slider>().Value) return;
            if (Player.UnderTurret(true)) return;
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp && myOrbwalker.IsWaiting())
            {
                var siegeQ = myUtility.GetLargeMinions(Q.Range).FirstOrDefault(x => Q.IsKillable(x));
                if (siegeQ != null && siegeQ.IsValidTarget())
                {
                    Q.Cast(siegeQ);
                }
                else
                {
                    var minionQ = MinionManager.GetMinions(Player.AttackRange).FirstOrDefault(x => Q.IsKillable(x));
                    if (minionQ != null && minionQ.IsValidTarget())
                    {
                        Q.Cast(minionQ);
                    }
                }
            }
            if (config.Item("UseRFarm").GetValue<bool>() && R.IsReady() && !Player.IsWindingUp)
            {
                var minionR = MinionManager.GetMinions(Player.ServerPosition, R.Range);
                if (minionR == null) return;
                var rpred = R.GetCircularFarmLocation(minionR);
                if (rpred.MinionsHit > config.Item("RFarmValue").GetValue<Slider>().Value)
                {
                    R.Cast(rpred.Position);
                }
            }
        }
        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var largemobs = myUtility.GetLargeMonsters(Q.Range).FirstOrDefault();
            if (mobs.Count <= 0) return;
            var mob = mobs[0];
            if (mob != null && !Player.IsWindingUp)
            {
                if (config.Item("UseQJFarm").GetValue<bool>() && Q.IsReady() && Orbwalking.InAutoAttackRange(mob))
                {
                    if (largemobs != null)
                    {
                        Q.Cast(largemobs);
                    }
                    else 
                    {
                        Q.Cast(mob);
                    }
                }
                if (config.Item("UseRJFarm").GetValue<bool>() && R.IsReady())
                {
                    if (largemobs != null)
                    {
                        R.Cast(largemobs.ServerPosition);
                    }
                    var RCircular = R.GetCircularFarmLocation(mobs);
                    if (RCircular.MinionsHit > 0)
                    {
                        R.Cast(RCircular.Position);
                    }
                }
            }
        }
        private void RPredict(Obj_AI_Hero target)
        {
            PredictionOutput pred = R.GetPrediction(target);
            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < R.Range)
            {
                var test1 = Prediction.GetPrediction(target, Q.Instance.SData.SpellCastTime).CastPosition;
                float movement = target.MoveSpeed * 100 / 1000;
                if (target.Distance(test1) > movement) R.Cast(target.ServerPosition.Extend(test1, R.Instance.SData.SpellCastTime * target.MoveSpeed));
                else
                {
                    if (pred.Hitchance >= HitChance.High) R.Cast(pred.CastPosition);
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
            }            
        }
        protected override void OnNonKillableMinion(AttackableUnit minion)
        {
            if (config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && (myUtility.PlayerManaPercentage > config.Item("FarmMana").GetValue<Slider>().Value))
            {
                var target = minion as Obj_AI_Base;
                if (target != null &&
                    Q.IsKillable(target) &&
                    Orbwalking.InAutoAttackRange(target))
                {
                    Q.Cast(target);
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (config.Item("UseRMisc").GetValue<bool>() && R.IsReady())
            {
                if (Vector3.Distance(Player.ServerPosition, gapcloser.End) <= R.Range)
                {
                    if (myUtility.ImmuneToMagic(gapcloser.Sender)) return;
                    R.Cast(gapcloser.End);
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
            if (config.Item("DrawR").GetValue<bool>() && R.Level > 0)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, Color.White);
            }
        }
    }
}
