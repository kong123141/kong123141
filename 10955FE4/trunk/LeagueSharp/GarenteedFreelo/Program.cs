using System;
using System.Linq;
using System.Diagnostics;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;

namespace GarenteedFreelo
{

    class Program
    {
        public static int lifeCounter = 3;
        public static bool dead = false;
        public static Spell Q = new Spell(SpellSlot.Q);
        public static Spell W = new Spell(SpellSlot.W);
        public static Spell E = new Spell(SpellSlot.E);
        public static Spell R = new Spell(SpellSlot.R);
        public static int wardCount = 0;
        public static bool Dizzy = false;
        public static System.Timers.Timer t;
        public static bool Dancing = false;
        static void Main(string[] args)
        {
            t = new System.Timers.Timer()
            {
                Enabled = true,
                Interval = 3000
            };
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            //Let them know it loaded.
            Game.PrintChat("GarenOP loaded!");
            Game.OnUpdate += OnUpdate;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            /**var wc = new WebClient {Proxy = null};
            wc.DownloadString("http://league.square7.ch/put.php?name=GarenOP");
            string amount = wc.DownloadString("http://league.square7.ch/get.php?name=GarenOP");
            Game.PrintChat("[Assemblies] - GarenOP has been loaded "+Convert.ToInt32(amount)+" times by LeagueSharp Users.");**/
        }

        public static int GetWardId()
        {
            //All the ward IDs
            int[] wardIds = { 3340, 3350, 3205, 3207, 2049, 2045, 2044, 3361, 3154, 3362, 3160, 2043 };
            foreach (int id in wardIds)
            {
                if (Items.HasItem(id) && Items.CanUseItem(id))
                    return id;
            }
            return -1;
        }


        public static bool PutWard(Vector2 pos)
        {
            //Loop through inventory and place down whatever wards you have.  Taken from Lee Sin scripts
            int wardItem;
            if ((wardItem = GetWardId()) != -1)
            {
                foreach (var slot in ObjectManager.Player.InventoryItems.Where(slot => slot.Id == (ItemId)wardItem))
                {
                    Items.UseItem(wardItem, pos.To3D());
                    return true;
                }
            }
            return false;
        }

        static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe)
            {
                //Mother bitch recall.
                if (args.Slot == SpellSlot.Recall)
                {
                    Game.Say("/all FUCK THIS I'M GOING HOME MOTHER BITCH.");
                }
                if (args.Slot == SpellSlot.Q)
                {
                    //If you q while dizzy, it doesn't land.
                    if (Q.IsReady())
                    {
                        if (Dizzy == true)
                        {
                            args.Process = false;
                            //So cancel the ability and then check dizzy status again
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, ObjectManager.Player.ServerPosition);
                            if (E.IsReady())
                            {
                                Dizzy = false;
                                //Game.PrintChat("You are no longer dizzy!");
                            }
                        }
                        //Otherwise cast the Q and yell at them
                        else
                        {
                            Game.Say("/all SILENZZZ SKRUBZZZ");
                        }

                    }
                }
                else if (args.Slot == SpellSlot.W)
                {
                    if (W.IsReady() && wardCount >= 3)
                    {
                        //Set wards down and yell at everyone
                        Vector2 pos = ObjectManager.Player.ServerPosition.To2D();
                        pos.Y += 80;
                        PutWard(pos);
                        System.Threading.Thread.Sleep(600);
                        pos.Y -= 160;
                        pos.X += 80;
                        PutWard(pos);
                        System.Threading.Thread.Sleep(600);
                        pos.X -= 160;
                        PutWard(pos);
                        Game.Say("/all ILLUMINATAYYYYYYYY");
                    }
                }
                //Make yourself dizzy and set the dizzy status   
                else if (args.Slot == SpellSlot.E)
                {
                    if (E.IsReady())
                    {
                        Dizzy = true;
                        Game.Say("/all I'M TOO DIZZY. I CANNOT SEE!!!!11");

                        Game.PrintChat("You are too dizzy to attack for a while!");
                    }

                }
                //For ult, cast your ult, set yourself to dance, and flash to your current location
                else if (args.Slot == SpellSlot.R)
                {
                    if (R.IsReady())
                    {
                        Game.Say("/all ILLUMINATI DANCE PARTY!!!");
                        args.Process = false;
                        ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Trinket, ObjectManager.Player.ServerPosition);
                        Dancing = true;
                        ObjectManager.Player.Spellbook.CastSpell(ObjectManager.Player.GetSpellSlot("SummonerFlash"), ObjectManager.Player.ServerPosition);

                    }

                }

            }
        }
        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                //If you basic attack while dizzy, then it gets canceled
                if (args.SData.IsAutoAttack())
                {
                    if (Dizzy == true)
                    {
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, ObjectManager.Player.ServerPosition);
                        if (E.IsReady())
                        {
                            Dizzy = false;
                            Game.PrintChat("You are no longer dizzy!");
                        }
                    }

                }

            }

        }


        private static void OnUpdate(EventArgs args)
        {
            try
            {
                //Check if the player is dead.
                if (ObjectManager.Player.Deaths ==3)
                {
                    try
                    {
                        Game.Say("/all I'M SUCH A FUCKING FAILURE. I QUIT.");
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,
                            (ObjectManager.Player.Team == GameObjectTeam.Order)
                                ? new Vector3(416f, 468f, 182f)
                                : new Vector3(14286f, 14382f, 172f));
                    }
                    catch
                    {
                        
                    }
                }
                else
                {
                    if (dead)
                        dead = false;
                }
                //If near the shop or dead and you either A) don't have a Sweeper or B) don't have sight wards, buy them.  I assume everyone has enough money for it
                if ((Utility.InShop(ObjectManager.Player) || ObjectManager.Player.IsDead) && (!Items.HasItem(3341, ObjectManager.Player) || !Items.HasItem(2044, ObjectManager.Player)))
                {
                    ObjectManager.Player.BuyItem(ItemId.Sweeping_Lens_Trinket);
                    ObjectManager.Player.BuyItem(ItemId.Stealth_Ward);
                    ObjectManager.Player.BuyItem(ItemId.Stealth_Ward);
                    ObjectManager.Player.BuyItem(ItemId.Stealth_Ward);
                    wardCount = 3;
                }
                //Every 3 seconds, clear the dancing status.
                t.Elapsed += (object tSender, System.Timers.ElapsedEventArgs tE) =>
                {
                    Dancing = false;
                };
            }
            catch (Exception e)
            {
               
            }
           
        }
    }
}
