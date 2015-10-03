#region
using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Marksman.Utils;
#endregion

namespace Marksman.Champions
{
    using System.Collections.Generic;
    using SharpDX;
    using Color = System.Drawing.Color;

    class DangerousSpells
    {
        public string ChampionName { get; private set; }
        public SpellSlot SpellSlot { get; private set; }

        public DangerousSpells(string championName, SpellSlot spellSlot)
        {
            this.ChampionName = championName;
            this.SpellSlot = spellSlot;
        }
    }

    internal class Sivir : Champion
    {
        public static Spell Q;
        public Spell E;
        public Spell W;
        private Menu _menuSupportedSpells;
        public static List<DangerousSpells> DangerousList = new List<DangerousSpells>();

        public Sivir()
        {
            Q = new Spell(SpellSlot.Q, 1220);
            Q.SetSkillshot(0.25f, 90f, 1350f, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 593);

            E = new Spell(SpellSlot.E);

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;

            DangerousList.Add(new DangerousSpells("darius", SpellSlot.R));
            DangerousList.Add(new DangerousSpells("fiddlesticks", SpellSlot.Q));
            DangerousList.Add(new DangerousSpells("garen", SpellSlot.R));
            DangerousList.Add(new DangerousSpells("leesin", SpellSlot.R));
            DangerousList.Add(new DangerousSpells("nautilius", SpellSlot.R));
            DangerousList.Add(new DangerousSpells("skarner", SpellSlot.R));
            DangerousList.Add(new DangerousSpells("syndra", SpellSlot.R));
            DangerousList.Add(new DangerousSpells("warwick", SpellSlot.R));
            DangerousList.Add(new DangerousSpells("warwick", SpellSlot.R));
            DangerousList.Add(new DangerousSpells("zed", SpellSlot.R));
            DangerousList.Add(new DangerousSpells("tristana", SpellSlot.R));

            Utils.Utils.PrintMessage("Sivir loaded.");
//            Utils.Utils.PrintMessage("Sivir E Support Loaded! Please check the Marksman Menu for her E Spell");
        }

        public void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy && sender is Obj_AI_Hero && args.Target.IsMe && this.E.IsReady())
            {
                foreach (var c in DangerousList.Where(c => ((Obj_AI_Hero)sender).ChampionName.ToLower() == c.ChampionName))
                {
                    if (args.SData.Name == ((Obj_AI_Hero)sender).GetSpell(c.SpellSlot).Name)
                    {
                        this.E.Cast();
                    }
                }
            }

            if (((Obj_AI_Hero)sender).ChampionName.ToLower() == "kalista" && args.SData.Name == ((Obj_AI_Hero)sender).GetSpell(SpellSlot.E).Name)
            {
                var bCount = ObjectManager.Player.Buffs.Count(b => b.Name.Contains("kalistaexpungemarker"));
                if (bCount > 0)
                    this.E.Cast();
            }
            
            if (((Obj_AI_Hero)sender).ChampionName.ToLower() == "vayne" && args.SData.Name == ((Obj_AI_Hero)sender).GetSpell(SpellSlot.E).Name)
            {
                for (var i = 1; i < 8; i++)
                {
                    var championBehind = ObjectManager.Player.Position
                                         + Vector3.Normalize(
                                             ((Obj_AI_Hero)sender).ServerPosition - ObjectManager.Player.Position)
                                         * (-i * 50);
                    if (championBehind.IsWall())
                    {
                        this.E.Cast();
                    }
                }
            }     
        }

        public override void Game_OnGameUpdate(EventArgs args)
        {
            if (GetValue<bool>("AutoQ"))
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (Q.IsReady() && t.IsValidTarget())
                {
                    if ((t.HasBuffOfType(BuffType.Slow) || t.HasBuffOfType(BuffType.Stun) ||
                         t.HasBuffOfType(BuffType.Snare) || t.HasBuffOfType(BuffType.Fear) ||
                         t.HasBuffOfType(BuffType.Taunt)))
                    {
                        CastQ();
                    }
                }
            }

            if (ComboActive || HarassActive)
            {
                var useQ = GetValue<bool>("UseQ" + (ComboActive ? "C" : "H"));

                if (Q.IsReady() && useQ)
                {
                    var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                    if (t != null)
                    {
                        CastQ();
                    }
                }
            }
        }

        public override void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var t = target as Obj_AI_Hero;
            if (t != null && (ComboActive || HarassActive) && unit.IsMe)
            {
                var useQ = GetValue<bool>("UseQ" + (ComboActive ? "C" : "H"));
                var useW = GetValue<bool>("UseW" + (ComboActive ? "C" : "H"));

                if (W.IsReady() && useW)
                {
                    W.Cast();
                }
                else if (Q.IsReady() && useQ)
                {
                    CastQ();
                }
            }
        }

        private static void CastQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (t.IsValidTarget() && Q.IsReady() &&
                ObjectManager.Player.Distance(t.ServerPosition) <= Q.Range)
            {
                var Qpredict = Q.GetPrediction(t);
                var hithere = Qpredict.CastPosition.Extend(ObjectManager.Player.Position, -140);

                var Hitchance = HitChance.High;
                if (ObjectManager.Player.Distance(t) >= 850)
                    Hitchance = HitChance.VeryHigh;
                else if (ObjectManager.Player.Distance(t) < 850 && ObjectManager.Player.Distance(t) > 600)
                    Hitchance = HitChance.High;
                else
                    Hitchance = HitChance.Medium;
                if (Qpredict.Hitchance >= Hitchance)
                    Q.Cast(hithere);
            }
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            Spell[] spellList = { Q };
            foreach (var spell in spellList)
            {
                var menuItem = GetValue<Circle>("Draw" + spell.Slot);
                if (menuItem.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }
        }

        public override bool ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQC" + Id, "Use Q").SetValue(true));
            config.AddItem(new MenuItem("UseWC" + Id, "Use W").SetValue(true));
            return true;
        }

        public override bool HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseQH" + Id, "Use Q").SetValue(false));
            config.AddItem(new MenuItem("UseWH" + Id, "Use W").SetValue(false));
            return true;
        }

        public override bool MiscMenu(Menu config)
        {
            config.AddItem(new MenuItem("AutoQ" + Id, "Auto Q on Stun/Slow/Fear/Taunt/Snare").SetValue(true));
            return true;
        }

        public override bool DrawingMenu(Menu config)
        {
            config.AddItem(
                new MenuItem("DrawQ" + Id, "Q range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            return true;
        }

        public override bool ExtrasMenu(Menu config)
        {
            return true;
        }

        public override bool LaneClearMenu(Menu config)
        {
            return true;
        }

        private void Game_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.Type == GameObjectType.obj_AI_Hero && sender.IsEnemy)
            {
                foreach (var spell in
                    _menuSupportedSpells.Items.SelectMany(
                        t =>
                            SpellList.BuffList.Where(
                                xSpell => xSpell.CanBlockWith.ToList().Contains(BlockableSpells.SivirE))
                                .Where(
                                    spell => t.Name == args.SData.Name && t.Name == spell.BuffName && t.GetValue<bool>()))
                    )
                {
                    switch (spell.SkillType)
                    {
                        case SkillShotType.SkillshotCircle:
                            if (ObjectManager.Player.Distance(args.End) <= 250f)
                            {
                                if (E.IsReady())
                                    E.Cast();
                            }
                            break;

                        case SkillShotType.SkillshotLine:
                            if (ObjectManager.Player.Distance(args.End) <= 100f)
                            {
                                if (E.IsReady())
                                    E.Cast();
                            }
                            break;

                        case SkillShotType.SkillshotUnknown:
                            if (ObjectManager.Player.Distance(args.End) <= 500f ||
                                ObjectManager.Player.Distance(sender.Position) <= 500)
                            {
                                if (E.IsReady())
                                    E.Cast();
                            }
                            break;
                    }
                }
            }
        }
    }
}
