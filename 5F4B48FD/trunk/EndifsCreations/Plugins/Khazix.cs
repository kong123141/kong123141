using System;
using System.Linq;
using EndifsCreations.Controller;
using EndifsCreations.SummonerSpells;
using EndifsCreations.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
{
    class Khazix : PluginData
    {
        public Khazix()
        {
            LoadSpells();
            LoadMenus();
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 300);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R);

            W.SetSkillshot(0.225f, 100f, 828.5f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 100f, 1000f, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            myDamageIndicator.DamageToUnit = GetDamage;
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.Khazix.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Khazix.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Khazix.Combo.E", "Use E").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.Khazix.Combo.R", "Use R").SetValue(false));
                //combomenu.AddItem(new MenuItem("EC.Khazix.Combo.Dive", "Turret Dive").SetValue(false));
                //combomenu.AddItem(new MenuItem("EC.Khazix.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.Khazix.Draw.Q", "Q").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Khazix.Draw.W", "W").SetValue(true));
                drawmenu.AddItem(new MenuItem("EC.Khazix.Draw.E", "E").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
                    
        }

        private void Combo()
        {
            
            if (Player.HasBuff("khazixeevo"))
            {
                Target = myUtility.GetTarget(1000, TargetSelector.DamageType.Physical);//TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);
                
                if (LockedTarget == null && (Target != null && Target.IsValidTarget()))
                {
                    LockedTarget = Target;
                }
                if (LockedTarget != null)
                {
                    Vector3 vec;
                    var x = (int)(Vector3.Distance(Player.ServerPosition, LockedTarget.ServerPosition) / 3f);
                    if (!Player.IsDashing() && !LockedTarget.IsDead)
                    {
                        vec = Player.ServerPosition.Extend(LockedTarget.ServerPosition, Vector3.Distance(Player.ServerPosition, LockedTarget.ServerPosition) + 125);
                        E.Cast(vec);
                        Utility.DelayAction.Add(x, () =>
                        {
                            myItemManager.UseItems(1, LockedTarget);
                            myItemManager.UseItems(2, null);
                        }
                        );
                    }
                    else if (Player.IsDashing() && !LockedTarget.IsDead)
                    {
                        Q.CastOnUnit(Target);
                    }
                    else if (Player.IsDashing() && LockedTarget.IsDead)
                    {
                        if (E.IsReady())
                        {
                            vec = Player.ServerPosition.Extend(Game.CursorPos, E.Range);
                            E.Cast(vec);
                        }
                    }
                    else if (!Player.IsDashing() && LockedTarget.IsDead)
                    {
                        if (E.IsReady())
                        {
                            if (Player.GetDashInfo().EndPos.To3D().UnderTurret(true) || Player.GetDashInfo().EndPos.To3D().CountEnemiesInRange(500) > 0)
                            {
                                vec = Player.ServerPosition.Extend(Game.CursorPos, E.Range);
                                E.Cast(vec);
                            }
                        }
                    }
                }
            }
            else
            {
                Target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                if (Target.IsValidTarget())
                {
                    if (!Orbwalking.InAutoAttackRange(Target) && Q.IsInRange(Target))
                    {
                        Q.CastOnUnit(Target);
                    }
                    mySpellcast.Linear(Target, W, HitChance.High, true);
                }
            }
        }
        private void UpdateSpells()
        {
            if (Player.HasBuff("khazixqevo") && Q.Range < 350f)
            {
                Q.Range = 350;
            }
            if (Player.HasBuff("khazixwevo"))
            {
                W.SetSkillshot(0.225f, 15f * 2 * (float)Math.PI / 180, 828.5f, true, SkillshotType.SkillshotCone);
            }
            if (Player.HasBuff("khazixeevo") && E.Range < 1000f)
            {
                E.Range = 1000;
            }
        }
        
        private float GetDamage(Obj_AI_Hero target)
        {
            var damage = 0d;
            if (Q.IsReady())
            {
                if (Player.HasBuff("khazixqevo"))
                {
                    if (Isolated(target))
                    {
                        damage += Player.GetSpellDamage(target, SpellSlot.Q, 3);
                    }
                    else damage += Player.GetSpellDamage(target, SpellSlot.Q, 2);
                }
                else
                {
                    if (Isolated(target))
                    {
                        damage += Player.GetSpellDamage(target, SpellSlot.Q, 1);
                    }
                    else damage += Player.GetSpellDamage(target, SpellSlot.Q, 0);
                }
            }
            if (E.IsReady())
            {
                damage += Player.GetSpellDamage(target, SpellSlot.E);
            }
            if (Items.HasItem(3077) && Items.CanUseItem(3077))
            {
                damage += Player.GetItemDamage(target, Damage.DamageItems.Tiamat);
            }
            if (Items.HasItem(3074) && Items.CanUseItem(3074))
            {
                damage += Player.GetItemDamage(target, Damage.DamageItems.Hydra);
            }
            if (Items.HasItem(3748) && Items.CanUseItem(3748))
            {
                //damage += Player.GetItemDamage(target, Damage.DamageItems);
            }

            return (float)damage;
        }
        private bool Isolated(Obj_AI_Base target)
        {
            var Selects = 
                HeroManager.Enemies.Where(x => 
                x.NetworkId != target.NetworkId && 
                Vector3.Distance(target.ServerPosition, x.ServerPosition) < 425).ToArray();
            return !Selects.Any();
        } 

        protected override void OnUpdate(EventArgs args)
        {
            UpdateSpells();
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
                    Console.WriteLine(Player.GetSpellDamage(Player, SpellSlot.Q, 2));
                    break;
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell) 
        {
            if (unit.IsMe)
            {
                if (spell.SData.Name.Contains("khazixe"))
                {
                    LastE = myUtility.TickCount;
                }
            }
        }
        protected override void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.R)
            {
                Utility.DelayAction.Add(500, () => myItemManager.UseGhostblade());
            }
        }
        protected override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo && Orbwalking.InAutoAttackRange(target) && !Player.IsDashing())
                {
                    if (Root.Item("EC.Khazix.Combo.Q").GetValue<bool>() && Q.IsReady())
                    {
                        Q.CastOnUnit((Obj_AI_Hero)target);
                    }
                }
            }
        }    
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Player.IsDashing())
            {
                var box = new Geometry.Polygon.Rectangle(Player.GetDashInfo().StartPos, Player.GetDashInfo().StartPos.Extend(Player.GetDashInfo().EndPos, E.Range), Q.Range);
                box.Draw(Color.Red, 2);
            }
            
            if (Root.Item("EC.Khazix.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {                
                if (Player.HasBuff("khazixqevo"))
                {
                    Render.Circle.DrawCircle(Player.Position, 350, Color.Cyan);
                }
                else
                {
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
                }
            }
            if (Root.Item("EC.Khazix.Draw.W").GetValue<bool>() && W.Level > 0)
            {
                Color color = Player.HasBuff("khazixwevo") ? Color.Cyan : Color.White;
                Render.Circle.DrawCircle(Player.Position, W.Range, color);
            }
            if (Root.Item("EC.Khazix.Draw.E").GetValue<bool>() && E.Level > 0)
            {
                if (Player.HasBuff("khazixeevo"))
                {
                    Render.Circle.DrawCircle(Player.Position, 1000, Color.Cyan);
                }
                else
                {
                    Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);
                }
            }
        }
    }
}
