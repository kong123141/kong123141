/**
 * 
 * Assembly Skeleton by GoldenGates, please leave some sort of credit in the code or elsewhere if you used this if possible!
 * Good luck!
 * 
 **/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace AssemblySkeleton {
    class Program {

        #region Declaration
        static Spell Q, W, E, R;
        static SpellSlot IgniteSlot;
        static Items.Item HealthPot;
        static Items.Item ManaPot;
        static Orbwalking.Orbwalker Orbwalker;
        static Menu Menu;
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static string CHAMPION_NAME = "MY_CHAMPION_NAME";
        static List<SpellSlot> AUTO_LEVEL_SEQUENCE = new List<SpellSlot>() { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.Q, SpellSlot.Q, SpellSlot.R, SpellSlot.Q, SpellSlot.W, SpellSlot.Q, SpellSlot.W, SpellSlot.R, SpellSlot.W, SpellSlot.W, SpellSlot.E, SpellSlot.E, SpellSlot.R, SpellSlot.E, SpellSlot.E }; //Assuming R>Q>W>E
        #endregion

        static void Game_OnGameLoad(EventArgs args) {
            if (Player.ChampionName != CHAMPION_NAME)
                return;

            /**EDIT SPELL VALUES BASED ON YOUR CHAMPION**/
            #region Spells
            Q = new Spell(SpellSlot.Q, 800);
            Q.SetSkillshot(0.4f, 40f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W = new Spell(SpellSlot.W);
            W.SetSkillshot(0.6f, (float)(80 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);
            E = new Spell(SpellSlot.E);
            E.SetSkillshot(0.25f, 50f, 1700f, false, SkillshotType.SkillshotLine);
            R = new Spell(SpellSlot.R);
            R.SetTargetted(.401f, 2000);
            #endregion
            /**EDIT SPELL VALUES BASED ON YOUR CHAMPION**/

            #region Items
            IgniteSlot = Player.GetSpellSlot("summonerdot");
            HealthPot = new Items.Item(2003, 0);
            ManaPot = new Items.Item(2004, 0);
            #endregion

            /**EDIT MENU BASED ON YOUR LIKING**/
            #region Menu
            Menu = new Menu("MENU TITLE HERE", Player.ChampionName, true);

            Menu OrbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(OrbwalkerMenu);

            Menu TargetSelectorMenu = Menu.AddSubMenu(new Menu("Target Selector", "TargetSelector"));
            TargetSelector.AddToMenu(TargetSelectorMenu);

            Menu.AddItem(new MenuItem("AutoLevel", "Auto Level").SetValue<bool>(true));

            Menu ComboMenu = Menu.AddSubMenu(new Menu("Combo", "Combo"));
            ComboMenu.AddItem(new MenuItem("ComboUseQ", "Use Q").SetValue(true));
            ComboMenu.AddItem(new MenuItem("ComboUseW", "Use W").SetValue(true));
            ComboMenu.AddItem(new MenuItem("ComboUseE", "Use E").SetValue(true));
            ComboMenu.AddItem(new MenuItem("ComboUseR", "Use R").SetValue(true));
            ComboMenu.AddItem(new MenuItem("ComboUseIgnite", "Use Ignite").SetValue(true));

            Menu HarassMenu = Menu.AddSubMenu(new Menu("Harass", "Harass"));
            HarassMenu.AddItem(new MenuItem("HarassUseQ", "Use Q").SetValue(true));
            HarassMenu.AddItem(new MenuItem("HarassUseW", "Use W").SetValue(true));
            HarassMenu.AddItem(new MenuItem("HarassUseE", "Use E").SetValue(true));
            HarassMenu.AddItem(new MenuItem("HarassManaManager", "Mana Manager (%)").SetValue(new Slider(40, 1, 100)));

            Menu LaneClearMenu = Menu.AddSubMenu(new Menu("Lane Clear", "LaneClear"));
            LaneClearMenu.AddItem(new MenuItem("LaneClearUseQ", "Use Q").SetValue(true));
            LaneClearMenu.AddItem(new MenuItem("LaneClearUseW", "Use W").SetValue(true));
            LaneClearMenu.AddItem(new MenuItem("LaneClearUseE", "Use E").SetValue(true));
            LaneClearMenu.AddItem(new MenuItem("LaneClearManaManager", "Mana Manager (%)").SetValue(new Slider(40, 1, 100)));

            Menu ItemsMenu = Menu.AddSubMenu(new Menu("Items", "Items"));
            ItemsMenu.AddItem(new MenuItem("ItemUseHealthPot", "Use Health Potion").SetValue(true));
            ItemsMenu.AddItem(new MenuItem("ItemHealthManager", "Activate at Health (%)").SetValue(new Slider(30, 1, 100)));
            ItemsMenu.AddItem(new MenuItem("ItemUseManaPot", "Use Mana Potion").SetValue(true));
            ItemsMenu.AddItem(new MenuItem("ItemManaManager", "Activate at Mana (%)").SetValue(new Slider(30, 1, 100)));

            Menu DrawingMenu = Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            DrawingMenu.AddItem(new MenuItem("drawAA", "Draw AA Range").SetValue(true));
            DrawingMenu.AddItem(new MenuItem("DrawQ", "Draw Q Range").SetValue(true));
            DrawingMenu.AddItem(new MenuItem("DrawW", "Draw W Range").SetValue(true));
            DrawingMenu.AddItem(new MenuItem("DrawE", "Draw E Range").SetValue(true));
            DrawingMenu.AddItem(new MenuItem("DrawR", "Draw R Range").SetValue(true));

            Menu.AddToMainMenu();
            #endregion
            /**EDIT MENU BASED ON YOUR LIKING**/

            /**EDIT SUBSCRIPTIONS BASED ON YOUR LIKING**/
            #region Subscriptions
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Game.OnProcessPacket += Game_OnProcessPacket;
            Game.OnSendPacket += Game_OnSendPacket;
            #endregion
            /**EDIT SUBSCRIPTIONS BASED ON YOUR LIKING**/
        }

        static void Game_OnSendPacket(GamePacketEventArgs args) {
            //Runs when package is sent
            //Not used often in scrub assemblies

        }

        static void Game_OnProcessPacket(GamePacketEventArgs args) {
            //Run when a packet is processed
            //Not used often in scrub assemblies
        }

        static void Game_OnUpdate(EventArgs args) {
            if (Player.IsDead)
                return;

            Checks();

            switch (Orbwalker.ActiveMode) {
                case Orbwalking.OrbwalkingMode.Combo:
                    /**YOUR COMBO LOGIC GOES HERE**/
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (Player.ManaPercent > Menu.Item("HarassManaManager").GetValue<Slider>().Value) {
                        /**YOUR HARASS LOGIC GOES HERE**/
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (Player.ManaPercent > Menu.Item("LaneClearManaManager").GetValue<Slider>().Value) {
                        /**YOUR LANE CLEAR LOGIC GOES HERE**/
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    /**YOUR LAST HIT LOGIC GOES HERE (Eg. Cast if 2 or more die, etc.)**/
                    break;
                case Orbwalking.OrbwalkingMode.None:
                    /**YOUR 'OTHER' LOGIC WHEN NOTHING IS HELD GOES HERE**/
                    break;
            }
        }

        static void Checks() {
            if (Menu.Item("AutoLevel").GetValue<bool>()) {
                AutoLevel.Enable();
                AutoLevel.UpdateSequence(AUTO_LEVEL_SEQUENCE);
            } else {
                AutoLevel.Disable();
            }
            if (Menu.Item("ItemUseHealthPot").GetValue<bool>() && Menu.Item("ItemHealthManager").GetValue<Slider>().Value > Player.HealthPercent) {
                if (Items.HasItem(HealthPot.Id) && Items.CanUseItem(HealthPot.Id) && !Player.HasBuff("RegenerationPotion")) {
                    HealthPot.Cast();
                }
            }
            if (Menu.Item("ItemUseManaPot").GetValue<bool>() && Menu.Item("ItemManaManager").GetValue<Slider>().Value > Player.HealthPercent) {
                if (Items.HasItem(ManaPot.Id) && Items.CanUseItem(ManaPot.Id)) { //Mana Pot Check Needed, :cat_lazy:
                    ManaPot.Cast();
                }
            }
        }

        static void Drawing_OnDraw(EventArgs args) {
            /**EDIT FROM, RADIUS, AND COLOR VALUES**/
            if (Menu.Item("DrawAA").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, Player.AttackRange, Color.Blue);
            if (Menu.Item("DrawQ").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.Blue);
            if (Menu.Item("DrawW").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.Blue);
            if (Menu.Item("DrawE").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.Blue);
            if (Menu.Item("DrawR").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Blue);
            /**EDIT FROM, RADIUS, AND COLOR VALUES**/
        }

        static void Main(string[] args) {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
    }
}
