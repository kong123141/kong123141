namespace LeagueSharp.Common
{
    using System;
    using System.Linq;
    using SharpDX;
    using NightMoon = LeagueSharp.Hacks;

    internal class Flowers
    {
        //public static bool duramk = false;
        //public static float gameTime1 = 0;
        public static Obj_AI_Hero Player;
        public static readonly Menu FlowersMenu = new Menu("Flowers Utility", "Flowers Utility");
        public static Vector3 LastEndPoint;
        public static float LastOrderTime, LastTime, DeltaTick = 0.15f;
        public static bool Attacking;
        public static Random random = new Random();
        public static bool FakerClickEnable => FlowersMenu.Item("Enable").GetValue<bool>();
        public static int FakerClickMode => FlowersMenu.Item("ClickMode").GetValue<StringList>().SelectedIndex;
        public static bool FakerClickKeyEnable => (FlowersMenu.Item("Key 1").GetValue<KeyBind>().Active || FlowersMenu.Item("Key 2").GetValue<KeyBind>().Active || FlowersMenu.Item("Key 3").GetValue<KeyBind>().Active || FlowersMenu.Item("Key 4").GetValue<KeyBind>().Active);

        internal static void Initialize()
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {

            try
            {
                Player = ObjectManager.Player;

                LoadInitializeMenu();
                LoadUtility();
                LoadEvents();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Flowers_OnGameLoad " + ex);
            }

        }

        private static void LoadEvents()
        {
            try
            {
                Orbwalking.BeforeAttack += BeforeAttack;
                Orbwalking.AfterAttack += AfterAttack;
                Obj_AI_Base.OnIssueOrder += OnIssueOrder;
                Obj_AI_Base.OnNewPath += OnNewPath;
                Spellbook.OnCastSpell += OnCastSpell;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Flowers_LoadEvents " + ex);
            }
        }

        private static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            try
            {
                var e = args.Target;

                if (e != null)
                    if (e.Position.Distance(Player.Position) >= 5f)
                        ShowClick(args.Target.Position, ClickType.Attack);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Flowers_OnCastSpell " + ex);
            }
        }

        private static void OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            try
            {
                if (sender.IsMe)
                    if (LastTime + DeltaTick < Game.Time)
                        if (args.Path.LastOrDefault() != LastEndPoint)
                            if (args.Path.LastOrDefault().Distance(Player.ServerPosition) >= 5f)
                                if (FakerClickEnable)
                                    if (FakerClickMode == 1 || (FakerClickMode == 2 && FakerClickKeyEnable))
                                    {
                                        LastEndPoint = args.Path.LastOrDefault();

                                        if (!Attacking)
                                        {
                                            ShowClick(Game.CursorPos, ClickType.Move);
                                        }
                                        else
                                        {
                                            ShowClick(Game.CursorPos, ClickType.Attack);
                                        }

                                        LastTime = Game.Time;
                                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Flowers_OnCastSpell " + ex);
            }
        }

        private static void OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            try
            {
                if (sender.IsMe)
                    if (args.Order == GameObjectOrder.MoveTo || args.Order == GameObjectOrder.AttackUnit || args.Order == GameObjectOrder.AttackTo)
                        if (LastOrderTime + random.NextFloat(DeltaTick, DeltaTick + 0.2f) < Game.Time)
                            if (FakerClickEnable)
                                if (FakerClickMode == 0)
                                {
                                    var Vector = args.TargetPosition;
                                    Vector.Z = Player.Position.Z;

                                    if (args.Order == GameObjectOrder.AttackUnit || args.Order == GameObjectOrder.AttackTo)
                                    {
                                        ShowClick(RandomizePosition(Vector), ClickType.Attack);
                                    }
                                    else
                                    {
                                        ShowClick(Vector, ClickType.Move);
                                    }

                                    LastOrderTime = Game.Time;
                                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Flowers_OnIssueOrder " + ex);
            }
        }

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            try
            {
                Attacking = false;

                var e = target as Obj_AI_Hero;

                if (e != null)
                    if (unit.IsMe)
                        ShowClick(RandomizePosition(e.Position), ClickType.Move);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Flowers_AfterAttack " + ex);
            }
        }

        private static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            try
            {
                if (FakerClickMode == 1 || (FakerClickMode == 2 && FakerClickKeyEnable))
                {
                    ShowClick(RandomizePosition(args.Target.Position), ClickType.Attack);
                    Attacking = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Flowers_BeforeAttack " + ex);
            }
        }

        private static void ShowClick(Vector3 Pos, ClickType Type)
        {
            try
            {
                if (FakerClickEnable)
                    Hud.ShowClick(Type, Pos);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Flowers_ShowClick " + ex);
            }
        }

        private static Vector3 RandomizePosition(Vector3 input)
        {
            if (random.Next(2) == 0)
            {
                input.X += random.Next(100);
            }
            else
            {
                input.Y += random.Next(100);
            }

            return input;
        }

        private static void LoadUtility()
        {
            try
            {
                FlowersMenu.Item("Disable Drawing").ValueChanged += (sender, e) =>
                {
                    NightMoon.DisableDrawings = e.GetNewValue<KeyBind>().Active;
                };

                FlowersMenu.Item("disable say").ValueChanged += (sender, e) =>
                {
                    NightMoon.DisableSay = e.GetNewValue<bool>();
                };

                if (FlowersMenu.Item("SaySomething").GetValue<bool>())
                {
                    Utility.DelayAction.Add(1000, () => {
                        Game.PrintChat("銆€");
                        Game.PrintChat("銆€");
                        Game.PrintChat("銆€");
                        Game.PrintChat("銆€");
                        Game.PrintChat("銆€");
                        Game.PrintChat("銆€");
                        Game.PrintChat("銆€");
                        Game.PrintChat("銆€");
                        Game.PrintChat("<font color=\"#FFA042\"><b>杈撳嚭/help鑾峰彇鍛戒护鍒楄〃</b></font>");
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Flowers_LoadUtility " + ex);
            }
        }

        private static void LoadInitializeMenu()
        {
            try
            {
                FlowersMenu.AddItem(new MenuItem("fakeclicks", "--------- FakeClicks"));
                FlowersMenu.AddItem(new MenuItem("Enable", "Enable").SetValue(false));
                FlowersMenu.AddItem(new MenuItem("ClickMode", "Click Mode")).SetValue(new StringList(new[] { "Evade, No Cursor Position", "Cursor Position, No Evade", "Press Key" }));
                FlowersMenu.AddItem(new MenuItem("Key 1", "PressKey 1").SetValue(new KeyBind('X', KeyBindType.Press)));
                FlowersMenu.AddItem(new MenuItem("Key 2", "PressKey 2").SetValue(new KeyBind('C', KeyBindType.Press)));
                FlowersMenu.AddItem(new MenuItem("Key 3", "PressKey 3").SetValue(new KeyBind('V', KeyBindType.Press)));
                FlowersMenu.AddItem(new MenuItem("Key 4", "PressKey 4").SetValue(new KeyBind(32, KeyBindType.Press)));

                FlowersMenu.AddItem(new MenuItem("Explore Utility", "--------- Explore Utility"));
                FlowersMenu.AddItem(new MenuItem("Disable Drawing", "Screen display [I]").SetValue(new KeyBind('I', KeyBindType.Toggle)));
                FlowersMenu.AddItem(new MenuItem("zoom hack", "Infinite horizon [danger]").SetValue(false));
                FlowersMenu.AddItem(new MenuItem("disable say", "Ban said the script").SetValue(true));
                //FlowersMenu.AddItem(new MenuItem("Tower Ranges", "Show enemy tower range").SetValue(false));
                FlowersMenu.AddItem(new MenuItem("SaySomething", "Stop script loading information").SetValue(false));
                FlowersMenu.AddItem(new MenuItem("SaySomething1", "By huabian"));
                CommonMenu.Instance.AddSubMenu(FlowersMenu);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Flowers_Menu " + ex);
            }
        }
    }
}
