using System.Collections;
using System.Linq.Expressions;
using System.Security.Cryptography;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Bard_My_Friend
{
    internal class Program
    {
        public static Menu RootMenu;
        public static Obj_AI_Hero Player;
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Vector3> chimeLocs;
        public static Vector3 destination;
        public static Boolean onWay = false;
        public static int WayPointCounter = 0;
        public static Spell Q = new Spell(SpellSlot.Q, 950f);
        public static Spell W = new Spell(SpellSlot.W, 1000f);
        public static Spell E = new Spell(SpellSlot.E, 900f);
        public static Spell R = new Spell(SpellSlot.R, 3400f);
        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        #region Game Overrides
        private static void Game_OnGameLoad(EventArgs args)
        {
                Player = ObjectManager.Player;

                if (Player.ChampionName != "Bard") return;
                RootMenu = new Menu("Bard: My Friend", "Bard", true);

                Game.PrintChat("Bard My Friend loaded");


                #region MenuPopulation

                Menu tsMenu = new Menu("Target Selector", "Target Selector");
                TargetSelector.AddToMenu(tsMenu);
                RootMenu.AddSubMenu(tsMenu);

                RootMenu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
                Orbwalker = new Orbwalking.Orbwalker(RootMenu.SubMenu(("Orbwalking")));
                RootMenu.AddItem(new MenuItem("Block last hit", "Block last hit")).SetValue(true);

                RootMenu.AddSubMenu(new Menu("Harass", "Harass"));
                RootMenu.SubMenu("Harass")
                    .AddItem(new MenuItem("Harass", "Harass"))
                    .SetValue(new KeyBind('C', KeyBindType.Press));
                RootMenu.SubMenu("Harass")
                    .AddItem(new MenuItem("Mana Level", "Mana Level"))
                    .SetValue(new Slider(30, 0, 100));
                RootMenu.SubMenu("Harass").AddItem(new MenuItem("Use Q", "Use Q")).SetValue(true);
                RootMenu.SubMenu("Harass")
                    .AddItem(new MenuItem("Use Q only when Stun", "Use Q only when Stun"))
                    .SetValue(false);
                RootMenu.SubMenu("Harass").AddItem(new MenuItem("Q Hitchance", "Q Hitchance")).SetValue(new StringList(
                new[]
                {
                    HitChance.Low.ToString(),
                    HitChance.Medium.ToString(),
                    HitChance.High.ToString(),
                    HitChance.VeryHigh.ToString()
                }));

                RootMenu.AddSubMenu(new Menu("Healing", "Healing"));
                RootMenu.SubMenu("Healing").AddItem(new MenuItem("Enabled", "Enabled")).SetValue(true);
                RootMenu.SubMenu("Healing")
                    .AddItem(new MenuItem("Health Level", "Health Level"))
                    .SetValue(new Slider(30, 0, 100));
                RootMenu.SubMenu("Healing")
                    .AddItem(new MenuItem("Mana Level", "Mana Level"))
                    .SetValue(new Slider(30, 0, 100));

                RootMenu.AddSubMenu(new Menu("Combo", "Combo"));
                RootMenu.SubMenu("Combo")
                    .AddItem(new MenuItem("Combo", "Combo"))
                    .SetValue(new KeyBind(' ', KeyBindType.Press));
                RootMenu.SubMenu("Combo").AddItem(new MenuItem("Use Q", "Use Q")).SetValue(true);
                RootMenu.SubMenu("Combo").AddItem(new MenuItem("Q Hitchance", "Q Hitchance")).SetValue(new StringList(
                   new[]
                {
                    HitChance.Low.ToString(),
                    HitChance.Medium.ToString(),
                    HitChance.High.ToString(),
                    HitChance.VeryHigh.ToString()
                }));
                RootMenu.SubMenu("Combo").AddItem(new MenuItem("Use W", "Use W")).SetValue(true);
                RootMenu.SubMenu("Combo").AddItem(new MenuItem("Use Ultimate", "Use Ultimate")).SetValue(true);
                RootMenu.SubMenu("Combo").AddSubMenu(new Menu("Ultimate", "Ultimate"));
                RootMenu.SubMenu("Combo")
                    .SubMenu("Ultimate")
                    .AddItem(new MenuItem("Minimum Ult Range", "Minimum Ult Range"))
                    .SetValue(new Slider(1500, 0, 3400));
                RootMenu.SubMenu("Combo")
                    .SubMenu("Ultimate")
                    .AddItem(new MenuItem("Minimum Enemy Health", "Minimum Enemy Health"))
                    .SetValue(new Slider(20, 0, 100));

                //RootMenu.SubMenu("Combo").AddItem(new MenuItem("Use E (Experimental)", "Use E")).SetValue(true);

                RootMenu.AddSubMenu(new Menu("Flee", "Flee"));
                RootMenu.SubMenu("Flee")
                    .AddItem(new MenuItem("Run Away", "Run Away").SetValue(new KeyBind('Z', KeyBindType.Press)));


                RootMenu.AddSubMenu(new Menu("Monster Freeze", "Monster Freeze"));
                RootMenu.SubMenu("Monster Freeze").AddItem(new MenuItem("Freeze Dragon", "Freeze Dragon").SetValue(true));
                RootMenu.SubMenu("Monster Freeze").AddItem(new MenuItem("Freeze Baron", "Freeze Baron").SetValue(true));
                //RootMenu.AddSubMenu(new Menu("Get Objects", "Get Objects"));
                //RootMenu.SubMenu("Get Objects")
                //    .AddItem(new MenuItem("Active", "Active").SetValue(new KeyBind('P', KeyBindType.Press)));

                RootMenu.AddSubMenu(new Menu("Chimes", "Chimes"));
                RootMenu.SubMenu("Chimes")
                    .AddItem(new MenuItem("Collect Now", "Collect Now").SetValue(new KeyBind('N', KeyBindType.Toggle)));

                RootMenu.AddToMainMenu();

                #endregion

                Game.OnUpdate += Game_OnGameUpdate;
                Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
                //We're subscribing to both events since we only need to update chime locations if an object is created or deleted.
                //TODO: Implement the oncreate and ondelete to get a cache of chimes for less lag and accesses.
                //GameObject.OnCreate += GameObject_OnCreateObject;
                //GameObject.OnDelete += GameObject_OnCreateObject;
                chimeLocs = new List<Vector3>();
                Q.SetSkillshot(.5f, 50, 1500, false, SkillshotType.SkillshotLine);
                W.SetSkillshot(.5f, 100, 1000, false, SkillshotType.SkillshotCircle);
                R.SetSkillshot(.5f, 350, 1500, false, SkillshotType.SkillshotCircle);
        }

        private static HitChance GetHitChance(string submenu, string chanceItem)
        {
            var hc = RootMenu.SubMenu(submenu).Item(chanceItem).GetValue<StringList>();
            switch (hc.SList[hc.SelectedIndex])
            {
                case "Low":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High":
                    return HitChance.High;
                case "Very High":
                    return HitChance.VeryHigh;
            }
            return HitChance.High;
        }

        private static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            //Block AA's when it's a minion that will die when you AA the last hit is active and no allies are nearby
            if (args.Target.Type == GameObjectType.obj_AI_Minion && args.Target.Health < Player.GetAutoAttackDamage((Obj_AI_Base)args.Target) && RootMenu.Item("Block last hit").IsActive() && Player.CountAlliesInRange(1000) > 1)
            {
                args.Process = false;
            }
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (RootMenu.SubMenu("Chimes").Item("Collect Now").IsActive())
            {
                int count = GetChimeLocs().Count();
                var tempDest = GetNextPoint(GetChimeLocs());
                if (count != 0 && tempDest != destination)
                {
                    destination = tempDest;
                    Player.IssueOrder(GameObjectOrder.MoveTo, destination);
                    if (Player.Position.Equals(destination))
                    {
                        destination = GetNextPoint(GetChimeLocs());
                        if (destination.Equals(Player.Position))
                            count = 0;
                    }
                }
                if (count == 0)
                {
                    RootMenu.SubMenu("Chimes").Item("Collect Now").SetValue(new KeyBind('N', KeyBindType.Toggle));
                }
            }
            if (RootMenu.SubMenu("Healing").Item("Enabled").IsActive())
                Heal();
            FreezeDragon();
            if(RootMenu.SubMenu("Combo").Item("Combo").IsActive())
                Combo();
            if (RootMenu.SubMenu("Flee").Item("Run Away").IsActive())
            {
                W.Cast(Player.Position);
                Flee();
            }
            if (RootMenu.SubMenu("Harass").Item("Harass").IsActive())
                Harass();

        }
        #endregion
        #region User-Created Methods
        public static Vector3 GetNextPoint(Vector3[] positions)
        {
            if(positions.Count()==0)
                return new Vector3();
            var minimum = float.MaxValue;
            var start = Player.Position;
            Vector3 tempMin = new Vector3();
            foreach (Vector3 chime in positions)
            {
                var path = ObjectManager.Player.GetPath(start, chime);
                var lastPoint = start;
                var d = 0f;
                d = DistanceFromArray(path);
                if (d < minimum)
                {
                    minimum = d;
                    tempMin = chime;
                }
            }
            return tempMin;
        }
        public static Boolean CompareVec3(Vector3 first, Vector3 second)
        {
            return (Math.Abs(first.X - second.X) < 2 && Math.Abs(first.Y - second.Y) < 2 &&
                    Math.Abs(first.Z - second.Z) < 2);
        }

        public static float DistanceFromArray(Vector3[] array)
        {
            var start = array[0];
            float distance = 0;
            for (int i = 1; i < array.Length; i++)
            {
                distance += start.Distance(array[i]);
                start = array[i];
            }
            return distance;
        }


        public static Vector3[] GetChimeLocs()
        {
            List<Vector3> locations = new List<Vector3>();
            GameObject[] objects = LeagueSharp.ObjectManager.Get<GameObject>().ToArray();
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i].Name.ToLower().Equals("bardchimeminion"))
                {
                    locations.Add(objects[i].Position);
                }
            }
            return locations.ToArray();
        }
        #endregion
        #region Combat Methods


        public static void Harass()
        {
            //TODO: Add mana manager.
            if (Player.ManaPercent >= RootMenu.SubMenu("Harass").Item("Mana Level").GetValue<Slider>().Value &&Q.IsReady())
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (target.IsValidTarget())
                {
                    if (RootMenu.SubMenu("Harass").Item("Use Q only when Stun").IsActive())
                        Stun(target);
                    else
                        Q.CastIfHitchanceEquals(target, GetHitChance("Harass", "Q Hitchance"));
                }
            }
        }

        public static void Heal()
        {
            if (!Player.IsRecalling())
            {
                //First and foremost, heal yourself.
                //From there, check your allies and see if they're in range and have less than 35% health.
                //If they do, cast it on them.
                //TODO: Change the values to be loaded from the menu.
                //TODO: Add mana manager.
                if (W.IsReady() && Player.ManaPercent >= RootMenu.SubMenu("Healing").Item("Mana Level").GetValue<Slider>().Value)
                {
                    if (Player.HealthPercent <= RootMenu.SubMenu("Healing").Item("Health Level").GetValue<Slider>().Value )
                        W.Cast(Player.Position);
                    else
                    {
                        foreach (
                            Obj_AI_Hero friendlies in ObjectManager.Get<Obj_AI_Hero>().Where(allies => !allies.IsEnemy && !allies.IsDead))
                        {
                            if (Vector3.Distance(Player.Position, friendlies.Position) < 1000f &&
                                friendlies.HealthPercent <= RootMenu.SubMenu("Healing").Item("Health Level").GetValue<Slider>().Value)
                                W.Cast(friendlies.Position);
                        }
                    }
                }
            }
        }
        public static void FreezeDragon()
        {
            Obj_AI_Minion[] objects = LeagueSharp.ObjectManager.Get<Obj_AI_Minion>().ToArray();
            for (int i = 0; i < objects.Length; i++)
            {
                if ((((objects[i].Name.ToLower().Contains("dragon") && RootMenu.SubMenu("Monster Freeze").Item("Freeze Dragon").IsActive()) || 
                    (objects[i].Name.ToLower().Contains("baron")  && RootMenu.SubMenu("Monster Freeze").Item("Freeze Baron").IsActive()) 
                    && objects[i].Health < 2500)))
                {
                    if (Vector3.Distance(objects[i].Position, Player.Position) < 3400f)
                    {
                        int playercount = 0;
                        int enemycount = 0;
                        foreach (Obj_AI_Hero players in ObjectManager.Get<Obj_AI_Hero>())
                        {
                            if (Vector3.Distance(objects[i].Position, players.Position) <= 650f)
                                if (players.IsEnemy)
                                    enemycount++;
                                else if(!players.IsMe)
                                    playercount++;
                        }
                        if(playercount==0 && enemycount!=0)
                            R.Cast(objects[i]);
                    }
                }
            }
        }

        public static void Combo()
        {
            Obj_AI_Hero target;
            if (Q.IsReady())
            {
                target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                Stun(target);
                if (Q.IsReady())
                    Q.CastIfHitchanceEquals(target, GetHitChance("Combo", "Q Hitchance"));

            }
            int playercount = 0;
            int enemycount = 0;
            target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            foreach (Obj_AI_Hero players in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (Vector3.Distance(target.Position, players.Position) <= 350f)
                    if (players.IsEnemy)
                        enemycount++;
                    else if (!players.IsMe)
                        playercount++;
            }
            //Only ult within user defined range.
            if (playercount == 0 && enemycount != 0 && Vector3.Distance(target.Position, Player.Position) < 3400f &&
                Vector3.Distance(target.Position, Player.Position) > RootMenu.SubMenu("Combo").SubMenu("Ultimate").Item("Minimum Ult Range").GetValue<Slider>().Value &&
                !target.UnderTurret() && target.HealthPercent >= RootMenu.SubMenu("Combo").SubMenu("Ultimate").Item("Minimum Enemy Health").GetValue<Slider>().Value
                && RootMenu.SubMenu("Combo").Item("Use Ultimate").IsActive())

                R.CastIfHitchanceEquals(target, HitChance.High);
        }
        private static void Stun(Obj_AI_Hero target)
        {
            //TODO: Add mana manager.
            var prediction = Q.GetPrediction(target);

            var direction = (Player.ServerPosition - prediction.UnitPosition).Normalized();
            var endOfQ = (Q.Range) * direction;
            //Modified Vayne condemn logic from DZ191
            for (int i = 0; i < 30; i++)
            {
                var checkPoint = prediction.UnitPosition.Extend(Player.ServerPosition, -Q.Range / 30 * i);
                var j4Flag = IsJ4FlagThere(checkPoint, target);
                if (checkPoint.IsWall() || prediction.CollisionObjects.Count == 1 || j4Flag)
                {
                    Q.CastIfHitchanceEquals(target, HitChance.High);
                }
            }
        }
        //Credits to DZ191
        private static bool IsJ4FlagThere(Vector3 position, Obj_AI_Hero target)
        {
            return ObjectManager.Get<Obj_AI_Base>().Any(m => m.Distance(position) <= target.BoundingRadius && m.Name == "Beacon");
        }

        public static void Flee()
        {
            //For fleeing, cast your Q to slow them down. Then AA, since your AA usually has a slow from Meeps
            //Then cast W on yourself to speed yourself up.
            //TODO: Check if you have a meep before doing AA.
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (Q.IsReady() && target.IsValidTarget())
                Q.Cast(target);
            target = TargetSelector.GetTarget(Player.AttackRange, TargetSelector.DamageType.Physical);
            if (Player.CanAttack)
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);

            if (W.IsReady())
            {
                W.Cast(Game.CursorPos);
            }

        }
        #endregion
    }
}
