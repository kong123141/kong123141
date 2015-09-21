#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Pantheon
{
    internal class Program
    {
        public const string ChampionName = "Pantheon";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;
        private static bool usedSpell, shennBuffActive;
        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell E;
        public static Spell W;
        public static Spell R;
        private static SpellSlot IgniteSlot;
        private static readonly Items.Item Tiamat = new Items.Item(3077, 450);
        //Menu
        public static Menu Config;
        public static Menu MenuExtras;
        public static Menu MenuTargetedItems;
        public static Menu MenuNonTargetedItems;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.CharData.BaseSkinName != "Pantheon")
                return;

            Q = new Spell(SpellSlot.Q, 620f);
            W = new Spell(SpellSlot.W, 620f);
            E = new Spell(SpellSlot.E, 640f);
            R = new Spell(SpellSlot.R, 2000f);

            Q.SetTargetted(0.2f, 1700f);
            W.SetTargetted(0.2f, 1700f);
            E.SetSkillshot(0.25f, 15f*2*(float) Math.PI/180, 2000f, false, SkillshotType.SkillshotCone);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            Config = new Menu("xQx | Pantheon", "Pantheon", true);

            var targetSelectorMenu = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            // Combo
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            {
                Config.SubMenu("Combo")
                    .AddItem(
                        new MenuItem("ComboActive", "Combo!").SetValue(
                            new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));
            }

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            {
                Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
                Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
                Config.SubMenu("Harass")
                    .AddItem(new MenuItem("HarassMana", "Min. Mana Percent: ").SetValue(new Slider(50, 100, 0)));
                Config.SubMenu("Harass")
                    .AddItem(
                        new MenuItem("HarassActiveT", "Harass (toggle)!").SetValue(new KeyBind("H".ToCharArray()[0],
                            KeyBindType.Toggle)));
                Config.SubMenu("Harass")
                    .AddItem(
                        new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0],
                            KeyBindType.Press)));
            }

            Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClear", "Use Q").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseELaneClear", "Use E").SetValue(false));
            Config.SubMenu("LaneClear")
                .AddItem(new MenuItem("LaneClearMana", "Min. Mana Percent: ").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("LaneClear")
                .AddItem(
                    new MenuItem("LaneClearActive", "LaneClear!").SetValue(
                        new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            // Jungling Farm
            Config.AddSubMenu(new Menu("JungleFarm", "JungleFarm"));
            {
                Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJungleFarm", "Use Q").SetValue(true));
                Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJungleFarm", "Use E").SetValue(false));
                Config.SubMenu("JungleFarm")
                    .AddItem(new MenuItem("JungleFarmMana", "Min. Mana Percent: ").SetValue(new Slider(50, 100, 0)));
                Config.SubMenu("JungleFarm")
                    .AddItem(
                        new MenuItem("JungleFarmActive", "JungleFarm!").SetValue(
                            new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            }

            MenuExtras = new Menu("Extras", "Extras");
            {
                Config.AddSubMenu(MenuExtras);
                MenuExtras.AddItem(new MenuItem("InterruptSpells", "Interrupt Spells").SetValue(true));
                MenuExtras.AddItem(new MenuItem("AutoLevelUp", "Auto Level Up").SetValue(true));
            }

            var menuUseItems = new Menu("Use Items", "menuUseItems");
            {
                Config.SubMenu("Extras").AddSubMenu(menuUseItems);

                // Extras -> Use Items -> Targeted Items
                MenuTargetedItems = new Menu("Targeted Items", "menuTargetItems");
                {
                    menuUseItems.AddSubMenu(MenuTargetedItems);
                    MenuTargetedItems.AddItem(new MenuItem("item3153", "Blade of the Ruined King").SetValue(true));
                    MenuTargetedItems.AddItem(new MenuItem("item3144", "Bilgewater Cutlass").SetValue(true));
                    MenuTargetedItems.AddItem(new MenuItem("item3146", "Hextech Gunblade").SetValue(true));
                    MenuTargetedItems.AddItem(new MenuItem("item3184", "Entropy ").SetValue(true));
                }
                // Extras -> Use Items -> AOE Items
                MenuNonTargetedItems = new Menu("AOE Items", "menuNonTargetedItems");
                {
                    menuUseItems.AddSubMenu(MenuNonTargetedItems);
                    MenuNonTargetedItems.AddItem(new MenuItem("item3180", "Odyn's Veil").SetValue(true));
                    MenuNonTargetedItems.AddItem(new MenuItem("item3143", "Randuin's Omen").SetValue(true));
                    MenuNonTargetedItems.AddItem(new MenuItem("item3131", "Sword of the Divine").SetValue(true));
                    MenuNonTargetedItems.AddItem(new MenuItem("item3074", "Ravenous Hydra").SetValue(true));
                    MenuNonTargetedItems.AddItem(new MenuItem("item3077", "Tiamat ").SetValue(true));
                    MenuNonTargetedItems.AddItem(new MenuItem("item3142", "Youmuu's Ghostblade").SetValue(true));
                }
            }
            // Drawing
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            {
                Config.SubMenu("Drawings")
                    .AddItem(
                        new MenuItem("QRange", "Q Range").SetValue(
                            new Circle(false, Color.FromArgb(255, 255, 255, 255))));
                Config.SubMenu("Drawings")
                    .AddItem(
                        new MenuItem("WRange", "W Range").SetValue(
                            new Circle(false, Color.FromArgb(255, 255, 255, 255))));
                Config.SubMenu("Drawings")
                    .AddItem(
                        new MenuItem("ERange", "E Range").SetValue(
                            new Circle(false, Color.FromArgb(255, 255, 255, 255))));
                Config.SubMenu("Drawings")
                    .AddItem(
                        new MenuItem("RRange", "R Range").SetValue(
                            new Circle(false, Color.FromArgb(255, 255, 255, 255))));
                Config.SubMenu("Drawings")
                    .AddItem(
                        new MenuItem("RRange2", "R Range (minimap)").SetValue(new Circle(true,
                            Color.FromArgb(255, 255, 255, 255))));

                var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Damage After Combo").SetValue(true);
                Config.SubMenu("Drawings").AddItem(dmgAfterComboItem);

                Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
                Utility.HpBarDamageIndicator.Enabled = true;
            }

            new PotionManager();
            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += DrawingOnOnEndScene;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;

            Notifications.AddNotification(string.Format("{0} Loaded", ChampionName), 4000);
        }

        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs arg)
        {
            if (!sender.IsMe)
                return;

            if (arg.SData.Name.ToLower().Contains("pantheonq") || arg.SData.Name.ToLower().Contains("pantheonw"))
            {
                usedSpell = true;
            }
            else if (arg.SData.Name.ToLower().Contains("pantheone") || Player.HasBuff("sound", true))
            {
                usedSpell = true;
            }
            else if (arg.SData.Name.ToLower().Contains("attack"))
            {
                usedSpell = false;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active && spell.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color, 1);
            }

            Render.Circle.DrawCircle(Player.Position, 30f, Color.Red, 1, true);
        }

        private static void DrawingOnOnEndScene(EventArgs args)
        {
            var rCircle2 = Config.Item("RRange2").GetValue<Circle>();
            if (rCircle2.Active)
            {
                Utility.DrawCircle(ObjectManager.Player.Position, 5500, rCircle2.Color, 1, 23, true);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            shennBuffActive = Player.HasBuff("Sheen", true);

            if (!Orbwalking.CanMove(100))
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }

            if (Config.Item("HarassActive").GetValue<KeyBind>().Active ||
               (Config.Item("HarassActiveT").GetValue<KeyBind>().Active && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo))
            {
                var existsMana = Config.Item("HarassMana").GetValue<Slider>().Value;
                if (Player.ManaPercent >= existsMana)
                    Harass();
            }

            if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
            {
                var existsMana = Config.Item("LaneClearMana").GetValue<Slider>().Value;
                if (Player.ManaPercent >= existsMana)
                    LaneClear();
            }

            if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
            {
                var existsMana = Config.Item("JungleFarmMana").GetValue<Slider>().Value;
                if (Player.ManaPercent >= existsMana)
                    JungleFarm();
            }
        }

        private static float GetComboDamage(Obj_AI_Hero t)
        {
            var fComboDamage = 0d;

            if (Q.IsReady())
                fComboDamage += Player.GetSpellDamage(t, SpellSlot.Q);

            if (W.IsReady())
                fComboDamage += Player.GetSpellDamage(t, SpellSlot.W);

            if (E.IsReady())
                fComboDamage += Player.GetSpellDamage(t, SpellSlot.E);

            if (R.IsReady())
                fComboDamage += Player.GetSpellDamage(t, SpellSlot.R);

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                fComboDamage += Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite);

            return (float) fComboDamage;
        }

        private static void Combo()
        {
            Obj_AI_Hero t;
            t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (!t.IsValidTarget())
                return;

            //if (t.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + 65) && (shennBuffActive || usedSpell))
            if (t.IsValidTarget(Orbwalking.GetRealAutoAttackRange(null) + 65) && (shennBuffActive))
                return;

            if (W.IsReady())
            {
                W.CastOnUnit(t);
            }
            else if (Q.IsReady())
            {
                Q.CastOnUnit(t);
            }
            else if (E.IsReady() && !Player.HasBuff("sound", true) && !Q.IsReady() && !W.IsReady())
            {
                E.Cast(t.Position);
            }

            UseItems(t);

            if (IgniteSlot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
                Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite) > t.Health)
            {
                Player.Spellbook.CastSpell(IgniteSlot, t);
            }
        }

        private static void Harass()
        {
            var useQ = Config.Item("UseQHarass").GetValue<bool>() && Q.IsReady();
            var useE = Config.Item("UseEHarass").GetValue<bool>() && E.IsReady();

            Obj_AI_Hero t;

            if (useQ)
            {
                t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                    Q.CastOnUnit(t);
            }

            if (useE && !Player.HasBuff("sound", true) && !Q.IsReady() && !W.IsReady())
            {
                t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    E.Cast(t.Position);
                }
            }
        }

        private static void JungleFarm()
        {
            var useQ = Config.Item("UseQJungleFarm").GetValue<bool>();
            var useE = Config.Item("UseEJungleFarm").GetValue<bool>();

            var mobs = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral,
                MinionOrderTypes.MaxHealth);

            if (mobs.Count <= 0)
                return;

            var mob = mobs[0];
            if (useQ && Q.IsReady() && mobs.Count >= 1)
                Q.CastOnUnit(mob);

            if (useE && E.IsReady() && mobs.Count >= 2 &&
                (LastCastedSpell.LastCastPacketSent.Slot != SpellSlot.E ||
                 Environment.TickCount - LastCastedSpell.LastCastPacketSent.Tick > 150))
            {
                E.Cast(mob.Position);
            }

            if (Tiamat.IsReady() && Config.Item("JungleFarmUseTiamat").GetValue<bool>())
            {
                if (mobs.Count >= 2)
                    Tiamat.Cast(Player);
            }
        }

        private static void LaneClear()
        {
            var useQ = Config.Item("UseQLaneClear").GetValue<bool>() && Q.IsReady();
            var useE = Config.Item("UseELaneClear").GetValue<bool>() && E.IsReady();

            if (useQ)
            {
                var vMinions = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range);
                foreach (var minions in
                    vMinions.Where(
                        minions => minions.Health < ObjectManager.Player.GetSpellDamage(minions, SpellSlot.Q)))
                    Q.Cast(minions);
            }

            if (useE)
            {
                var rangedMinionsE = MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition, E.Range + E.Width + 30, MinionTypes.Ranged);

                var locE = W.GetCircularFarmLocation(rangedMinionsE, W.Width*0.75f);
                if (locE.MinionsHit >= 3 && E.IsInRange(locE.Position.To3D()))
                {
                    E.Cast(locE.Position);
                }
            }
        }

        private static float GetComboDamage(Obj_AI_Base vTarget)
        {
            var fComboDamage = 0d;

            if (Q.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.Q);

            if (E.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.E);

            if (Items.CanUseItem(3128))
                fComboDamage += Player.GetItemDamage(vTarget, Damage.DamageItems.Botrk);

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                fComboDamage += Player.GetSummonerSpellDamage(vTarget, Damage.SummonerSpell.Ignite);

            return (float) fComboDamage;
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base vTarget, InterruptableSpell args)
        {
            var interruptSpells = Config.Item("InterruptSpells").GetValue<KeyBind>().Active;
            if (!interruptSpells)
                return;

            if (Player.Distance(vTarget) < Orbwalking.GetRealAutoAttackRange(ObjectManager.Player))
            {
                if (W.IsReady())
                    W.Cast();
            }
        }

        private static InventorySlot GetInventorySlot(int id)
        {
            return
                ObjectManager.Player.InventoryItems.FirstOrDefault(
                    item =>
                        (item.Id == (ItemId) id && item.Stacks >= 1) || (item.Id == (ItemId) id && item.Charges >= 1));
        }

        public static void UseItems(Obj_AI_Hero vTarget)
        {
            if (vTarget == null)
                return;

            foreach (var itemID in from menuItem in MenuTargetedItems.Items
                let useItem = MenuTargetedItems.Item(menuItem.Name).GetValue<bool>()
                where useItem
                select Convert.ToInt16(menuItem.Name.Substring(4, 4))
                into itemId
                where Items.HasItem(itemId) && Items.CanUseItem(itemId) && GetInventorySlot(itemId) != null
                select itemId)
            {
                Items.UseItem(itemID, vTarget);
            }

            foreach (var itemID in from menuItem in MenuNonTargetedItems.Items
                let useItem = MenuNonTargetedItems.Item(menuItem.Name).GetValue<bool>()
                where useItem
                select Convert.ToInt16(menuItem.Name.Substring(4, 4))
                into itemId
                where Items.HasItem(itemId) && Items.CanUseItem(itemId) && GetInventorySlot(itemId) != null
                select itemId)
            {
                if (Player.Distance(vTarget) <= 350)
                    Items.UseItem(itemID);
            }
        }
    }
}
