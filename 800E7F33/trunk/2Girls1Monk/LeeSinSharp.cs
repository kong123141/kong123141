/*
 

                                                    __                        .-'''-.                             
       .-''-.                    .---.         ...-'  |`.                    '   _    \                           
     .' .-.  )         .--.      |   |         |      |  |  __  __   ___   /   /` '.   \   _..._       .          
    / .'  / /    .--./)|__|      |   |         ....   |  | |  |/  `.'   `..   |     \  ' .'     '.   .'|          
   (_/   / /    /.''\\ .--.-,.--.|   |           -|   |  | |   .-.  .-.   |   '      |  .   .-.   ..'  |          
        / /    | |  | ||  |  .-. |   |            |   |  | |  |  |  |  |  \    \     / /|  '   '  <    |          
       / /      \`-' / |  | |  | |   |      _  ...'   `--' |  |  |  |  |  |`.   ` ..' / |  |   |  ||   | ____     
      . '       /("'`  |  | |  | |   |    .' | |         |`|  |  |  |  |  |   '-...-'`  |  |   |  ||   | \ .'     
     / /    _.-'\ '---.|  | |  '-|   |   .   | ` --------\ |  |  |  |  |  |             |  |   |  ||   |/  .      
   .' '  _.'.-'' /'""'.|__| |    |   | .'.'| |//`---------'|__|  |__|  |__|             |  |   |  ||    /\  \     
  /  /.-'_.'    ||     || | |    '---.'.'.-'  /                                         |  |   |  ||   |  \  \    
 /    _.'       \'. __//  |_|        .'   \_.'                                          |  |   |  |'    \  \  \   
( _.-'           `'---'                                                                 '--'   '--'------'  '---' 

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using SharpDX;

/*
 * ToDo:
 * 
 * */


namespace two_girls_one_monk
{
    internal class LeeSinSharp
    {
        public static string[] testSpells = { "RelicSmallLantern", "RelicLantern", "SightWard", "wrigglelantern", "ItemGhostWard", "VisionWard",
                                     "BantamTrap", "JackInTheBox","CaitlynYordleTrap", "Bushwhack"};


        

        public const string CharName = "LeeSin";

        public static Menu Config;
        
        public static Map map;

        public static bool CastQAgain;

        public static int passiveStacks;
        
        public static float passiveTimer;

        public static bool q2Done = false;
        
        public static float q2Timer;

        public static bool waitingForQ2 = false;

        public static Obj_AI_Hero target;

        public static string[] SmiteName = { "summonersmite", "s5_summonersmiteplayerganker", "s5_summonersmitequick", "s5_summonersmiteduel", "itemsmiteaoe" };
        
        private static readonly string[] spells =
        {
            "BlindMonkQOne", "BlindMonkWOne", "BlindMonkEOne", "blindmonkwtwo", "blindmonkqtwo", "blindmonketwo", "BlindMonkRKick"
        };

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe && passiveStacks > 0)
            {
                passiveStacks = passiveStacks - 1;
            }
        }
        public LeeSinSharp()
        {
            /* CallBAcks */
            CustomEvents.Game.OnGameLoad += onLoad;

        }

        private static void onLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != CharName) return;
            map = new Map();

            Game.PrintChat("<font color=\"#e61515\">2Girls1Monk -<font color=\"#FFFFFF\"> by spawny Successfully Loaded.</font>");

            try
            {
                //Menu
                Config = new Menu("LeeSin", "LeeSin", true);
                var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
                
                //TargetSelector
                TargetSelector.AddToMenu(targetSelectorMenu);
                Config.AddSubMenu(targetSelectorMenu);

                // Orbwalker
                Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
                LeeSin.orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));
                
                //Laneclear
                Config.AddSubMenu(new Menu("Laneclear", "Laneclear"));
                Config.SubMenu("Laneclear").AddItem(new MenuItem("UseQClear", "Use Q")).SetValue(true);
                Config.SubMenu("Laneclear").AddItem(new MenuItem("UseWClear", "Use W")).SetValue(true);
                Config.SubMenu("Laneclear").AddItem(new MenuItem("UseEClear", "Use E")).SetValue(true);
                Config.SubMenu("Laneclear").AddItem(new MenuItem("ActiveClear", "Farm!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press, false)));
                
                //C-C-C-Combo
                Config.AddSubMenu(new Menu("Combo", "Combo"));
                Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
                Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
                Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);
                Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true);
                Config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
                Config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo1", "Combo2!").SetValue((new KeyBind("Z".ToCharArray()[0], KeyBindType.Press, false))));
                
                //Harass
                Config.AddSubMenu(new Menu("Harass", "Harass"));
                Config.SubMenu("Harass").AddItem(new MenuItem("ActiveHarass", "Harass!").SetValue((new KeyBind("C".ToCharArray()[0], KeyBindType.Press, false))));

                //Insec
                Config.AddSubMenu(new Menu("Insec", "Insec"));
                Config.SubMenu("Insec").AddItem(new MenuItem("ActiveInsec", "Insec!").SetValue((new KeyBind("T".ToCharArray()[0], KeyBindType.Press, false))));
                
                //KS
                Config.AddSubMenu(new Menu("KillSteal", "KillSteal"));
                Config.SubMenu("KillSteal").AddItem(new MenuItem("UseR", "R killsteal")).SetValue(true);

                //Wardjump
                Config.AddSubMenu(new Menu("WardJump", "WardJump"));
                Config.SubMenu("WardJump").AddItem(new MenuItem("ActiveWard", "WardJump!").SetValue((new KeyBind("G".ToCharArray()[0], KeyBindType.Press, false))));
                
                //Drawings
                Config.AddSubMenu(new Menu("Drawings", "Drawings"));
                Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
                Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
                Config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
                Config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
                Config.SubMenu("Drawings").AddItem(new MenuItem("DrawInsec", "Draw Insec")).SetValue(true);
                Config.SubMenu("Drawings").AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
                Config.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));
                Config.AddToMainMenu();
                Drawing.OnDraw += onDraw;
                Game.OnUpdate += OnGameUpdate;

                GameObject.OnCreate += OnCreateObject;
                GameObject.OnDelete += OnDeleteObject;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;

                LeeSin.setSkillShots();
            }
            catch
            {
            }

        }

        private static void OnGameUpdate(EventArgs args)
        {
            LeeSin.loaidraw();
            LeeSin.CastR_kill();
            if (passiveTimer <= Environment.TickCount) passiveStacks = 0;
            if (q2Timer <= Environment.TickCount) q2Done = false;

            target = TargetSelector.GetTarget(1500, TargetSelector.DamageType.Physical);
            LeeSin.checkLock(target);
            LeeSin.orbwalker.SetAttack(true);
            if (Config.Item("ActiveWard").GetValue<KeyBind>().Active)
            {
                LeeSin.wardJump(Game.CursorPos.To2D());
            }

            if (Config.Item("ActiveHarass").GetValue<KeyBind>().Active)
            {
                LeeSin.doHarass();
            }

            if (Config.Item("ActiveClear").GetValue<KeyBind>().Active)
            {
                Laneclear.Clear();
                Laneclear.Jgclear();
            }
            
            if (Config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                LeeSin.combo();
            }
            if (Config.Item("ActiveCombo1").GetValue<KeyBind>().Active)
            {
                LeeSin.combo2();

            }
            if (Config.Item("ActiveInsec").GetValue<KeyBind>().Active)
            {
                LeeSin.insecOrbwalk(LeeSin.LockedTarget);
                LeeSin.useinsec();
            }

            //if (LeeSin.orbwalker.ActiveMode.ToString() == "LaneClear")
            //{
                
            //}
        }

        private static void onDraw(EventArgs args)
        {
            if (Config.Item("DrawQ").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 1000, System.Drawing.Color.Gray,
                    Config.Item("CircleThickness").GetValue<Slider>().Value);
                    //Config.Item("CircleQuality").GetValue<Slider>().Value);
            }
            if (Config.Item("DrawW").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 700, System.Drawing.Color.Gray,
                    Config.Item("CircleThickness").GetValue<Slider>().Value);
                    //Config.Item("CircleQuality").GetValue<Slider>().Value);
            }
            if (Config.Item("DrawE").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 350, System.Drawing.Color.Gray,
                    Config.Item("CircleThickness").GetValue<Slider>().Value);
                    //Config.Item("CircleQuality").GetValue<Slider>().Value);
            }
            if (Config.Item("DrawR").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 375, System.Drawing.Color.Gray,
                    Config.Item("CircleThickness").GetValue<Slider>().Value);
                    //Config.Item("CircleQuality").GetValue<Slider>().Value);
            }
            if (Config.Item("DrawInsec").GetValue<bool>() && LeeSin.R.IsReady())
            {
                if (!LeeSin.loaidraw())
                {
                    Vector2 heroPos = Drawing.WorldToScreen(LeeSin.LockedTarget.Position);
                    Vector2 diempos = Drawing.WorldToScreen(LeeSin.getward1(LeeSin.LockedTarget));
                    Drawing.DrawLine(heroPos[0], heroPos[1], diempos[0], diempos[1], 1, System.Drawing.Color.White);
                }
                else
                {
                    Vector2 heroPos = Drawing.WorldToScreen(LeeSin.LockedTarget.Position);
                    Vector2 diempos = Drawing.WorldToScreen(LeeSin.getward3(LeeSin.LockedTarget));
                    Drawing.DrawLine(heroPos[0], heroPos[1], diempos[0], diempos[1], 1, System.Drawing.Color.White);
                }
            }
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Missile") || sender.Name.Contains("Minion"))
                return;
        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_GeneralParticleEmitter))
                return;
            if (sender.Name.Contains("blindMonk_Q_resonatingStrike") && waitingForQ2)
            {
                waitingForQ2 = false;
                q2Done = true;
                q2Timer = Environment.TickCount + 800;
            }
        }

        public static void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (testSpells.ToList().Contains(args.SData.Name))
            {
                if (!sender.IsMe) return;
                if (spells.Contains(args.SData.Name))
                {
                    passiveStacks = 2;
                    passiveTimer = Environment.TickCount + 3000;
                }
                if (args.SData.Name == "BlindMonkQOne")
                {
                    CastQAgain = false;
                    Utility.DelayAction.Add(2900, () =>
                    {
                        CastQAgain = true;
                    });
                }
                LeeSin.testSpellCast = args.End.To2D();
                Polygon pol;
                if ((pol = map.getInWhichPolygon(args.End.To2D())) != null)
                {
                    LeeSin.testSpellProj = pol.getProjOnPolygon(args.End.To2D());
                }
            }
        }




    }
}
