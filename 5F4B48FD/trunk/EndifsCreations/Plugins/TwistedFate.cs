using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCreations.Controller;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Plugins
{
    class TwistedFate : PluginData
    {
        public TwistedFate()
        {
            LoadSpells();
            LoadMenus();
        }
        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1450);
            W = new Spell(SpellSlot.W);

            Q.SetSkillshot(0.25f, 40f, 1000f, false, SkillshotType.SkillshotLine);          
            
            SpellList.Add(Q);
            SpellList.Add(W);
        }
        private void LoadMenus()
        {
            var combomenu = new Menu("Combo", "Combo");
            {
                combomenu.AddItem(new MenuItem("EC.TwistedFate.Combo.Q", "Use Q").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.TwistedFate.Combo.W", "Use W").SetValue(true));
                combomenu.AddItem(new MenuItem("EC.TwistedFate.Combo.Items", "Use Items").SetValue(true));
                Root.AddSubMenu(combomenu);
            }
            var miscmenu = new Menu("Misc", "Misc");
            {
                miscmenu.AddItem(new MenuItem("EC.TwistedFate.Misc.W", "W Gapclosers").SetValue(false));
                Root.AddSubMenu(miscmenu);
            }
            var drawmenu = new Menu("Draw", "Draw");
            {
                drawmenu.AddItem(new MenuItem("EC.TwistedFate.Draw.Q", "Q").SetValue(true));
                Root.AddSubMenu(drawmenu);
            }
        }
        
        private void Combo()
        {
            Target = myUtility.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            var UseQ = Root.Item("EC.TwistedFate.Combo.Q").GetValue<bool>();
            var UseW = Root.Item("EC.TwistedFate.Combo.W").GetValue<bool>();
            var CastItems = Root.Item("EC.TwistedFate.Combo.Items").GetValue<bool>();
            if (UseW && W.IsReady())
            {
                if (Target.IsValidTarget() && Orbwalking.InAutoAttackRange(Target))
                {
                    if ((Player.Mana <= Q.Instance.ManaCost + W.Instance.ManaCost) || myUtility.ImmuneToCC(Target) || myUtility.ImmuneToMagic(Target))
                    {
                        PickACard(Cards.Blue);
                    }
                    else if (myUtility.MovementDisabled(Target))
                    {
                        PickACard(Cards.Gold);
                    }
                    else if (Player.HasBuff("cardmasterstackparticle"))
                    {
                        PickACard(Cards.Blue);
                    }
                    PickACard(Cards.Gold);
                }
                else if (Player.CountEnemiesInRange(525) > 0)
                {
                    PickACard(Cards.Red);
                }
            }
            if (Target.IsValidTarget())
            {
                if (Target.InFountain()) return;
                try
                {
                    if (CastItems) { myItemManager.UseItems(0, Target); }
                    if (UseQ && Q.IsReady())
                    {
                        mySpellcast.Linear(Target, Q, HitChance.High);
                    }
                }
                catch { }
            }
        }

        private void WildCard()
        {
            var spellname = Player.Spellbook.GetSpell(SpellSlot.W).Name;
            var spellstate = Player.Spellbook.CanUseSpell(SpellSlot.W);
            if ((spellstate == SpellState.Ready && spellname == "PickACard" && (Status != WStatus.Selecting || myUtility.TickCount - LastPick > 500)) || Player.IsDead)
            {
                Status = WStatus.Ready;
            }
            else if (spellstate == SpellState.Cooldown && spellname == "PickACard")
            {
                Pick = Cards.None;
                Status = WStatus.Cooldown;
            }
            else if (spellstate == SpellState.Surpressed && !Player.IsDead)
            {
                Status = WStatus.Selected;
            }
            if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.Combo)
            {
                if (myUtility.TickCount - LastSelecting > 1000)
                {
                    if (Pick == Cards.Blue && spellname == "bluecardlock")
                    {
                        Player.Spellbook.CastSpell(SpellSlot.W, false);
                    }
                    else if (Pick == Cards.Gold && spellname == "goldcardlock")
                    {
                        Player.Spellbook.CastSpell(SpellSlot.W, false);
                    }
                    else if (Pick == Cards.Red && spellname == "redcardlock")
                    {
                        Player.Spellbook.CastSpell(SpellSlot.W, false);
                    }
                }
            }
            else
            {
                if (Pick == Cards.Blue && spellname == "bluecardlock")
                {
                    Player.Spellbook.CastSpell(SpellSlot.W, false);
                }
                else if (Pick == Cards.Gold && spellname == "goldcardlock")
                {
                    Player.Spellbook.CastSpell(SpellSlot.W, false);
                }
                else if (Pick == Cards.Red && spellname == "redcardlock")
                {
                    Player.Spellbook.CastSpell(SpellSlot.W, false);
                }
            }
        }
        private void PickACard(Cards card)
        {
            if (Player.Spellbook.GetSpell(SpellSlot.W).Name == "PickACard" && Status == WStatus.Ready)
            {
                Pick = card;
                if (myUtility.TickCount - LastPick > 170 + Game.Ping / 2)
                {
                    Player.Spellbook.CastSpell(SpellSlot.W, ObjectManager.Player);
                    LastPick = myUtility.TickCount;
                }
            }
        }
        private enum Cards
        {
            Red,
            Gold,
            Blue,
            None,
        }
        private enum WStatus
        {
            Ready,
            Selecting,
            Selected,
            Cooldown,
        }
        private static Cards Pick;
        private static WStatus Status { get; set; }

        private int LastPick;
        private int LastSelecting;

        protected override void OnUpdate(EventArgs args)
        {
            WildCard();
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
                case myOrbwalker.OrbwalkingMode.LaneClear:
                    {
                        var minionW = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                        if (minionW.Any())
                        {
                            PickACard(Cards.Blue);
                        }
                    }
                    break;
            }
        }
        protected override void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (unit.IsMe)
            {
                if (spell.SData.Name == "gate")
                {
                    PickACard(Cards.Gold);
                }
                if (spell.SData.Name == "PickACard")
                {
                    Status = WStatus.Selecting;
                    LastSelecting = myUtility.TickCount;
                }                
                if (spell.SData.Name == "redcardlock")
                {
                    Status = WStatus.Selected;
                }
                if (spell.SData.Name == "goldcardlock")
                {
                    Status = WStatus.Selected;
                }
                if (spell.SData.Name == "bluecardlock")
                {
                    Status = WStatus.Selected;
                }                
            }
        }
        protected override void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Turret && args.Target.Team != Player.Team && Orbwalking.InAutoAttackRange(args.Target))
            {
                if (myOrbwalker.ActiveMode == myOrbwalker.OrbwalkingMode.LaneClear)
                {
                    PickACard(Cards.Blue);
                }
            }
        }
        protected override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Root.Item("EC.TwistedFate.Misc.W").GetValue<bool>() && W.IsReady())
            {
                if (gapcloser.Sender.IsEnemy && Vector3.Distance(Player.ServerPosition, gapcloser.End) <= Player.BoundingRadius)
                {
                    if (myUtility.ImmuneToCC(gapcloser.Sender) || myUtility.ImmuneToMagic(gapcloser.Sender)) return;
                    PickACard(Cards.Gold);
                }
            }
        }
        protected override void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Root.Item("EC.TwistedFate.Draw.Q").GetValue<bool>() && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);
            }
        }
    }
}
