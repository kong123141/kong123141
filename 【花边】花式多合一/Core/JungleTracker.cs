using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using Vector2 = SharpDX.Vector2;

namespace 花边_花式多合一.Core
{
    class JungleTracker
    {

        #region static

        public static Utility.Map.MapType MapType { get; set; }
        public static Junglerrrr.Camp DragonCamp;
        public static Junglerrrr.Camp BaronCamp;
        public static List<int> OnAttackList;
        public static List<int> MissileHitList;
        public static List<int[]> OnCreateGrompList;
        public static List<int[]> OnCreateCampIconList;
        public static List<int[]> PossibleBaronList;
        public static List<int> PossibleDragonList;
        public static List<int> ObjectsList;
        public static int PossibleDragonTimer;
        public static int GuessNetworkId1 = 1;
        public static int GuessNetworkId2 = 1;
        public static int GuessDragonId = 1;
        public static int Seed1 = 3;
        public static int Seed2 = 2;
        public static float ClockTimeAdjust;
        public static int BiggestNetworkId;
        public static bool Timeronmap;
        public static bool Timeronminimap;
        public static int Circleradius;
        public static Color Colorattacking;
        public static Color Colortracked;
        public static Color Colordisengaged;
        public static Color Colordead;
        public static Color Colorguessed;
        public static int Circlewidth;
        public static ColorBGRA White;
        public static int[] HeroNetworkId = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static string[] BlockHeroes = { "Caitlyn", "Nidalee" };
        public static int[] SeedOrder = { 0, 1, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 1, 0, 0, 0, 1, 0, 0, 1, 0 };
        public static int[] CreateOrder = { 14, 15, 10, 9, 8, 13, 12, 11, 4, 3, 2, 7, 6, 5, 23, 22, 21, 20, 29, 28, 27, 26, 19, 18, 17, 16, 35, 34, 33, 32, 31, 30 };
        public static int[] IdOrder = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 0, 2, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static int UpdateTick;

        #endregion

        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if (!InitializeMenu.Menu.Item("JungleTrackerEnable").GetValue<bool>()) return;

                OnAttackList = new List<int>();
                MissileHitList = new List<int>();
                OnCreateGrompList = new List<int[]>();
                OnCreateCampIconList = new List<int[]>();
                PossibleBaronList = new List<int[]>();
                PossibleDragonList = new List<int>();
                ObjectsList = new List<int>();
                White = new ColorBGRA(255, 255, 255, 255);
                foreach (var camp in Junglerrrr.Camps.Where(camp => camp.MapType.ToString() == "SummonersRift"))
                {
                    if (camp.Name == "Dragon")
                    {
                        DragonCamp = camp;
                    }
                    else if (camp.Name == "Baron")
                    {
                        BaronCamp = camp;
                    }
                }
                foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.Name.Contains("SRU_") || x.Name.Contains("Sru_")))
                {
                    foreach (var camp in Junglerrrr.Camps.Where(camp => camp.MapType.ToString() == Game.MapId.ToString()))
                    {
                        foreach (var mob in camp.Mobs)
                        {
                            if (mob.Name.Contains(minion.Name) && !minion.IsDead && mob.NetworkId != minion.NetworkId)
                            {
                                mob.NetworkId = minion.NetworkId;

                                mob.LastChangeOnState = Environment.TickCount;
                                mob.Unit = minion;

                                if (!camp.IsRanged && camp.Mobs.Count > 1)
                                {
                                    mob.State = 6;
                                }
                                else
                                {
                                    mob.State = 5;
                                }

                                if (camp.Mobs.Count == 1)
                                {
                                    camp.State = mob.State;
                                    camp.LastChangeOnState = mob.LastChangeOnState;
                                }
                            }
                        }
                    }
                }
                if (Game.ClockTime > 450f)
                {
                    GuessNetworkId1 = 0;

                    GuessNetworkId2 = 0;
                }
                int c = 0;
                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
                {
                    HeroNetworkId[c] = hero.NetworkId;
                    c++;
                    if (hero.NetworkId > BiggestNetworkId)
                    {
                        BiggestNetworkId = hero.NetworkId;
                    }
                    if (!hero.IsAlly)
                    {
                        for (int i = 0; i <= 1; i++)
                        {
                            if (hero.ChampionName.Contains(BlockHeroes[i]))
                            {
                                GuessDragonId = 0;
                            }
                        }
                    }
                }
                Circleradius = InitializeMenu.Menu.Item("circleradius").GetValue<Slider>().Value;
                Colorattacking = InitializeMenu.Menu.Item("colorattacking").GetValue<Color>();
                Colortracked = InitializeMenu.Menu.Item("colortracked").GetValue<Color>();
                Colordisengaged = InitializeMenu.Menu.Item("colordisengaged").GetValue<Color>();
                Colordead = InitializeMenu.Menu.Item("colordead").GetValue<Color>();
                Colorguessed = InitializeMenu.Menu.Item("colorguessed").GetValue<Color>();
                Circlewidth = InitializeMenu.Menu.Item("circlewidth").GetValue<Slider>().Value;

                Drawing.OnEndScene += Drawing_OnEndScene;
                GameObject.OnCreate += GameObjectOnCreate;
                GameObject.OnDelete += GameObjectOnDelete;
                Game.OnUpdate += OnGameUpdate;
                Game.OnProcessPacket += OnProcessPacket;
            }
            catch (Exception ex)
            {
                Console.WriteLine("JungleTracker error occurred: '{0}'", ex);
            }
        }

        private static void OnProcessPacket(GamePacketEventArgs args)
        {
            short header = BitConverter.ToInt16(args.PacketData, 0);
            int length = BitConverter.ToString(args.PacketData, 0).Length;
            int networkID = BitConverter.ToInt32(args.PacketData, 2);

            if (header == 0)
            {
                return;
            }

            bool isMob = false;

            foreach (var camp in Junglerrrr.Camps.Where(camp => camp.MapType.ToString() == Game.MapId.ToString()))
            {
                //Do Stuff for each camp

                foreach (var mob in camp.Mobs.Where(mob => mob.NetworkId == networkID))
                {
                    //Do Stuff for each mob in a camp

                    isMob = true;

                    if (header == Packetssss.MonsterSkill.Header)
                    {
                        if (mob.Name.Contains("Crab"))
                        {
                            mob.State = 4;
                        }
                        else
                        {
                            mob.State = 1;
                        }
                        mob.LastChangeOnState = Environment.TickCount;
                    }

                    else if (header == Packetssss.Attack.Header)
                    {
                        mob.State = 1;
                        mob.LastChangeOnState = Environment.TickCount;
                    }

                    else if (header == Packetssss.MissileHit.Header)
                    {
                        mob.State = 1;
                        mob.LastChangeOnState = Environment.TickCount;
                    }

                    else if (header == Packetssss.Disengaged.Header)
                    {
                        if (mob.Name.Contains("Crab"))
                        {
                            if (mob.State == 0) mob.State = 5;    //check this again
                            else mob.State = 1;
                        }
                        if (!mob.Name.Contains("Crab") && !mob.Name.Contains("Spider"))
                        {
                            if (mob.State == 0) mob.State = 5;
                            else mob.State = 2;
                        }
                        mob.LastChangeOnState = Environment.TickCount;
                    }

                    if (mob.LastChangeOnState == Environment.TickCount && camp.Mobs.Count == 1)
                    {
                        camp.State = mob.State;
                        camp.LastChangeOnState = mob.LastChangeOnState;
                    }
                }
            }

            bool foundObj = false;

            foreach (var obj in ObjectManager.Get<GameObject>().ToList().Where(x => x.NetworkId == networkID))
            {
                foundObj = true;
            }

            if (Game.MapId.ToString() == "SummonersRift" &&
                !isMob && !foundObj &&
                networkID != DragonCamp.Mobs[0].NetworkId &&
                networkID != BaronCamp.Mobs[0].NetworkId &&
                networkID > BiggestNetworkId
                )
            {
                if (Packetssss.MissileHit.Header == header && Packetssss.MissileHit.Length == length)
                {
                    PossibleBaronList.Add(new int[] { networkID, (int)header, length, Environment.TickCount });

                    if ((PossibleBaronList.Count(item => item[0] == networkID && item[1] == Packetssss.MonsterSkill.Header && item[2] == Packetssss.MonsterSkill.Length) >= 1) &&
                    (PossibleBaronList.Count(item => item[0] == networkID && item[1] == Packetssss.MonsterSkill.Header && item[2] == Packetssss.MonsterSkill.Length2) >= 1))
                    {
                        BaronCamp.Mobs[0].State = 1;
                        BaronCamp.Mobs[0].LastChangeOnState = Environment.TickCount;
                        BaronCamp.Mobs[0].NetworkId = networkID;
                    }

                }
                else if (Packetssss.MonsterSkill.Header == header && Packetssss.MonsterSkill.Length == length)
                {
                    PossibleBaronList.Add(new int[] { networkID, (int)header, length, Environment.TickCount });

                    if ((PossibleBaronList.Count(item => item[0] == networkID && item[1] == Packetssss.MissileHit.Header && item[2] == Packetssss.MissileHit.Length) >= 1) &&
                    (PossibleBaronList.Count(item => item[0] == networkID && item[1] == Packetssss.MonsterSkill.Header && item[2] == Packetssss.MonsterSkill.Length2) >= 1))
                    {
                        BaronCamp.Mobs[0].State = 1;
                        BaronCamp.Mobs[0].LastChangeOnState = Environment.TickCount;
                        BaronCamp.Mobs[0].NetworkId = networkID;
                    }
                }
                else if (Packetssss.MonsterSkill.Header == header && Packetssss.MonsterSkill.Length2 == length)
                {
                    PossibleBaronList.Add(new int[] { networkID, (int)header, length, Environment.TickCount });

                    if ((PossibleBaronList.Count(item => item[0] == networkID && item[1] == Packetssss.MissileHit.Header && item[2] == Packetssss.MissileHit.Length) >= 1) &&
                    (PossibleBaronList.Count(item => item[0] == networkID && item[1] == Packetssss.MonsterSkill.Header && item[2] == Packetssss.MonsterSkill.Length) >= 1))
                    {
                        BaronCamp.Mobs[0].State = 1;
                        BaronCamp.Mobs[0].LastChangeOnState = Environment.TickCount;
                        BaronCamp.Mobs[0].NetworkId = networkID;
                    }
                }
            }

            if (Environment.TickCount <= PossibleDragonTimer + 5000)
            {
                foreach (var id in PossibleDragonList.ToList().Where(id => id == networkID))
                {
                    try
                    {
                        PossibleDragonList.RemoveAll(x => x == networkID);
                    }
                    catch (Exception)
                    {
                        //ignored
                    }
                }
            }
            else
            {
                if (PossibleDragonList.Count() == 1)
                {
                    DragonCamp.Mobs[0].State = 1;
                    DragonCamp.Mobs[0].LastChangeOnState = Environment.TickCount;
                    DragonCamp.Mobs[0].NetworkId = PossibleDragonList[0];
                }
                try
                {
                    PossibleDragonList.Clear();
                }
                catch (Exception)
                {
                    //ignored
                }
            }


            if (header == Packetssss.MonsterSkill.Header &&
                Game.MapId.ToString() == "SummonersRift" &&
                !isMob && !foundObj &&
                networkID != DragonCamp.Mobs[0].NetworkId &&
                networkID != BaronCamp.Mobs[0].NetworkId &&
                networkID > BiggestNetworkId &&
                GuessDragonId == 1)
            {
                if (!ObjectsList.Contains(networkID))
                {
                    PossibleDragonList.Add(networkID);
                    PossibleDragonTimer = Environment.TickCount;
                }
            }

            if (header == Packetssss.CreateGromp.Header && Game.MapId.ToString() == "SummonersRift")  //Gromp Created
            {
                if (length == Packetssss.CreateGromp.Length)
                {
                    foreach (var camp in Junglerrrr.Camps.Where(camp => camp.Name == "Gromp"))
                    {
                        foreach (var mob in camp.Mobs.Where(mob => mob.Name.Contains("SRU_Gromp13.1.1")))
                        {
                            mob.NetworkId = BitConverter.ToInt32(args.PacketData, 2);
                            mob.State = 3;
                            mob.LastChangeOnState = Environment.TickCount;
                            camp.State = mob.State;
                            camp.LastChangeOnState = mob.LastChangeOnState;
                        }
                    }

                    if (Game.ClockTime - 111f < 90 && ClockTimeAdjust == 0)
                    {
                        ClockTimeAdjust = Game.ClockTime - 111f;
                        DragonCamp.Mobs[0].State = 0;
                        DragonCamp.RespawnTime = Environment.TickCount + 39000;
                        DragonCamp.State = 0;
                        BiggestNetworkId = BitConverter.ToInt32(args.PacketData, 2);
                    }
                }
                else if (length == Packetssss.CreateGromp.Length2)
                {
                    foreach (var camp in Junglerrrr.Camps.Where(camp => camp.Name == "Gromp"))
                    {
                        foreach (var mob in camp.Mobs.Where(mob => mob.Name.Contains("SRU_Gromp14.1.1")))
                        {
                            mob.NetworkId = BitConverter.ToInt32(args.PacketData, 2);
                            mob.State = 3;
                            mob.LastChangeOnState = Environment.TickCount;
                            camp.State = mob.State;
                            camp.LastChangeOnState = mob.LastChangeOnState;
                        }
                    }
                }
            }

            if (!ObjectsList.Contains(networkID) && (header != Packetssss.MonsterSkill.Header || length != Packetssss.MonsterSkill.Length))
            {
                ObjectsList.Add(networkID);
            }
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (Environment.TickCount > UpdateTick + InitializeMenu.Menu.Item("updatetick").GetValue<Slider>().Value)
            {
                var enemy = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget());

                foreach (var camp in Junglerrrr.Camps.Where(camp => camp.MapType.ToString() == Game.MapId.ToString()))
                {
                    #region Update States

                    int mobCount = 0;
                    bool firstMob = true;
                    int visibleMobsCount = 0;
                    int rangedMobsCount = 0;
                    int deadRangedMobsCount = 0;

                    foreach (var mob in camp.Mobs)
                    {
                        //Do Stuff for each mob in a camp

                        try
                        {
                            if (mob.Unit != null && mob.Unit.IsVisible)
                            {
                                visibleMobsCount++;
                            }
                        }
                        catch (Exception)
                        {
                            //ignored
                        }


                        if (mob.IsRanged)
                        {
                            rangedMobsCount++;

                            if (mob.JustDied)
                            {
                                deadRangedMobsCount++;
                            }
                        }

                        bool visible = false;

                        mobCount += 1;

                        int guessedTimetoDead = 3000;

                        if (camp.Name == "Dragon")
                        {
                            if (Game.ClockTime - ClockTimeAdjust < 420f) guessedTimetoDead = 60000;
                            else if (Game.ClockTime - ClockTimeAdjust < 820f) guessedTimetoDead = 40000;
                            else guessedTimetoDead = 15000;
                        }

                        if (camp.Name == "Baron")
                        {
                            guessedTimetoDead = 5000;
                        }


                        switch (mob.State)
                        {
                            case 1:
                                if ((Environment.TickCount - mob.LastChangeOnState) >= guessedTimetoDead && camp.Name != "Crab")
                                {
                                    if (camp.Name == "Dragon")
                                    {
                                        try
                                        {
                                            if (mob.Unit != null && !mob.Unit.IsVisible && enemy == null)
                                            {
                                                mob.State = 4;
                                                mob.LastChangeOnState = Environment.TickCount - 2000;
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            //ignored
                                        }
                                    }
                                    else if (camp.Name == "Baron")
                                    {
                                        mob.State = 5;
                                        mob.LastChangeOnState = Environment.TickCount - 2000;
                                    }
                                    else
                                    {
                                        mob.State = 4;
                                        mob.LastChangeOnState = Environment.TickCount - 2000;
                                    }
                                }

                                if ((Environment.TickCount - mob.LastChangeOnState >= 10000 && camp.Name == "Crab"))
                                {
                                    mob.State = 3;
                                    mob.LastChangeOnState = Environment.TickCount;
                                }
                                break;
                            case 2:
                                if (Environment.TickCount - mob.LastChangeOnState >= 4000)
                                {
                                    if (!camp.IsRanged && camp.Mobs.Count > 1)
                                    {
                                        mob.State = 6;
                                    }
                                    else
                                    {
                                        mob.State = 5;
                                    }
                                    mob.LastChangeOnState = Environment.TickCount;
                                }
                                break;
                            case 4:
                                if (Environment.TickCount - mob.LastChangeOnState >= 5000)
                                {
                                    mob.State = 7;
                                    mob.JustDied = true;
                                }
                                break;
                            case 5:
                                if (Environment.TickCount - mob.LastChangeOnState >= 45000)
                                {
                                    mob.State = 6;
                                }
                                if (mob.Unit != null && mob.Unit.IsVisible && !mob.Unit.IsDead)
                                {
                                    mob.State = 3;
                                }
                                break;
                            case 6:
                                if (mob.Unit != null && mob.Unit.IsVisible && !mob.Unit.IsDead)
                                {
                                    mob.State = 3;
                                }
                                break;
                            default:
                                break;
                        }

                        if (mob.Unit != null && mob.Unit.IsVisible && !mob.Unit.IsDead)
                        {
                            visible = true;
                        }

                        if ((mob.State == 7 || mob.State == 4) && visible) //check again
                        {
                            mob.State = 3;
                            mob.LastChangeOnState = Environment.TickCount;
                            mob.JustDied = false;
                        }

                        if (camp.Mobs.Count == 1)
                        {
                            camp.State = mob.State;
                            camp.LastChangeOnState = mob.LastChangeOnState;
                        }

                        if (camp.IsRanged && camp.Mobs.Count > 1 && mob.State > 0)
                        {
                            if (visible)
                            {
                                if (firstMob)
                                {
                                    camp.State = mob.State;
                                    camp.LastChangeOnState = mob.LastChangeOnState;
                                    firstMob = false;
                                }
                                else if (!firstMob)
                                {
                                    if (mob.State < camp.State)
                                    {
                                        camp.State = mob.State;
                                    }
                                    if (mob.LastChangeOnState > camp.LastChangeOnState)
                                    {
                                        camp.LastChangeOnState = mob.LastChangeOnState;
                                    }
                                }

                                if (!mob.IsRanged)
                                {
                                    camp.LastChangeOnState = Environment.TickCount;
                                    camp.RespawnTime = (camp.LastChangeOnState + camp.RespawnTimer * 1000);
                                }
                            }
                            else
                            {
                                if (firstMob)
                                {
                                    if (mob.IsRanged)
                                    {
                                        camp.State = mob.State;
                                        firstMob = false;
                                    }
                                    camp.LastChangeOnState = mob.LastChangeOnState;
                                }
                                else if (!firstMob)
                                {
                                    if (mob.State < camp.State && mob.IsRanged)
                                    {
                                        camp.State = mob.State;
                                    }
                                    if (mob.LastChangeOnState > camp.LastChangeOnState)
                                    {
                                        camp.LastChangeOnState = mob.LastChangeOnState;
                                    }
                                }
                            }
                        }
                        else if (!camp.IsRanged && camp.Mobs.Count > 1 && mob.State > 0)
                        {
                            if (firstMob)
                            {
                                camp.State = mob.State;
                                camp.LastChangeOnState = mob.LastChangeOnState;
                                firstMob = false;
                            }
                            else
                            {
                                if (mob.State < camp.State)
                                {
                                    camp.State = mob.State;
                                }
                                if (mob.LastChangeOnState > camp.LastChangeOnState)
                                {
                                    camp.LastChangeOnState = mob.LastChangeOnState;
                                }
                            }
                            if (visible)
                            {
                                camp.LastChangeOnState = Environment.TickCount;
                                camp.RespawnTime = (camp.LastChangeOnState + camp.RespawnTimer * 1000);
                            }
                        }

                        if (visible && camp.RespawnTime > Environment.TickCount)
                        {
                            camp.RespawnTime = (Environment.TickCount + camp.RespawnTimer * 1000);
                        }
                    }


                    //Do Stuff for each camp

                    if (camp.State == 7)
                    {
                        int mobsJustDiedCount = 0;

                        for (int i = 0; i < mobCount; i++)
                        {
                            try
                            {
                                if (camp.Mobs[i].JustDied)
                                {
                                    mobsJustDiedCount++;
                                }
                            }
                            catch (Exception)
                            {
                                //ignored
                            }

                        }

                        if (mobsJustDiedCount == mobCount)
                        {
                            camp.RespawnTime = (camp.LastChangeOnState + camp.RespawnTimer * 1000);

                            for (int i = 0; i < mobCount; i++)
                            {
                                try
                                {
                                    camp.Mobs[i].JustDied = false;
                                }
                                catch (Exception)
                                {
                                    //ignored
                                }
                            }
                        }
                    }

                    if (camp.IsRanged && visibleMobsCount == 0 && rangedMobsCount == deadRangedMobsCount)
                    {
                        camp.RespawnTime = (camp.LastChangeOnState + camp.RespawnTimer * 1000);

                        for (int i = 0; i < mobCount; i++)
                        {
                            try
                            {
                                camp.Mobs[i].JustDied = false;
                            }
                            catch (Exception)
                            {
                                //ignored
                            }

                        }
                    }

                    if (camp.Name == "Baron" && PossibleBaronList.Count >= 1 && camp.State >= 1 && camp.State <= 3)
                    {
                        try
                        {
                            PossibleBaronList.Clear();
                        }
                        catch (Exception)
                        {
                            //ignored
                        }
                    }

                    #endregion

                    #region Guess Blue/Red NetworkID

                    if (GuessNetworkId1 == 1 && camp.Name == "Blue" && camp.Team.ToString().Contains("Order") && visibleMobsCount == camp.Mobs.Count &&
                        camp.Mobs[0].NetworkId != 0 && camp.Mobs[1].NetworkId != 0 && camp.Mobs[2].NetworkId != 0)
                    {
                        Seed1 = (camp.Mobs[1].NetworkId - camp.Mobs[0].NetworkId);
                        Seed2 = (camp.Mobs[2].NetworkId - camp.Mobs[1].NetworkId);

                        int id = 0;

                        for (int c = 0; c <= 31; c++)
                        {
                            int order = CreateOrder[c];

                            if (c == 2)
                            {
                                id += Seed1;
                                id += Seed2;
                            }
                            else
                            {
                                if (SeedOrder[c] == 1) id += Seed1;
                                else id += Seed2;
                            }

                            IdOrder[order] = id;
                        }

                        foreach (var camp2 in Junglerrrr.Camps.Where(camp2 => camp2.MapType.ToString() == Game.MapId.ToString() && camp2.Name == "Blue" && !camp2.Team.ToString().Contains("Order")))
                        {
                            for (int j = 5; j <= 7; j++)
                            {
                                if (IdOrder[j] == 0) continue;
                                int i = 0;
                                switch (j)
                                {
                                    case 5:
                                        i = 2;
                                        break;
                                    case 6:
                                        i = 1;
                                        break;
                                    case 7:
                                        i = 0;
                                        break;
                                    default:
                                        break;
                                }

                                if (camp2.Mobs[i].NetworkId == 0)
                                {
                                    if (IdOrder[j] < IdOrder[4])
                                    {
                                        camp2.Mobs[i].NetworkId = camp.Mobs[0].NetworkId - ((IdOrder[4] - IdOrder[j]));
                                        camp2.Mobs[i].State = 5;
                                        camp2.Mobs[i].LastChangeOnState = Environment.TickCount;
                                    }
                                    else if (IdOrder[j] > IdOrder[4])
                                    {
                                        camp2.Mobs[i].NetworkId = camp.Mobs[0].NetworkId + ((IdOrder[j] - IdOrder[4]));
                                        camp2.Mobs[i].State = 5;
                                        camp2.Mobs[i].LastChangeOnState = Environment.TickCount;
                                    }
                                }
                            }
                        }
                        GuessNetworkId1 = 0;

                    }
                    else if (GuessNetworkId1 == 1 && camp.Name == "Blue" && !camp.Team.ToString().Contains("Order") && visibleMobsCount == camp.Mobs.Count &&
                        camp.Mobs[0].NetworkId != 0 && camp.Mobs[1].NetworkId != 0 && camp.Mobs[2].NetworkId != 0)
                    {
                        Seed1 = (camp.Mobs[1].NetworkId - camp.Mobs[0].NetworkId);
                        Seed2 = (camp.Mobs[2].NetworkId - camp.Mobs[1].NetworkId);

                        //Console.WriteLine("Seed1:" + Seed1 + "  Seed2:" + Seed2);

                        int id = 0;

                        for (int c = 0; c <= 31; c++)
                        {
                            int order = CreateOrder[c];

                            if (c == 2)
                            {
                                id += Seed1;
                                id += Seed2;
                            }
                            else
                            {
                                if (SeedOrder[c] == 1) id += Seed1;
                                else id += Seed2;
                            }

                            IdOrder[order] = id;
                        }

                        foreach (var camp2 in Junglerrrr.Camps.Where(camp2 => camp2.MapType.ToString() == Game.MapId.ToString() && camp2.Name == "Blue" && camp2.Team.ToString().Contains("Order")))
                        {
                            for (int j = 2; j <= 4; j++)
                            {
                                if (IdOrder[j] == 0) continue;
                                int i = 0;
                                switch (j)
                                {
                                    case 2:
                                        i = 2;
                                        break;
                                    case 3:
                                        i = 1;
                                        break;
                                    case 4:
                                        i = 0;
                                        break;
                                    default:
                                        break;
                                }

                                if (camp2.Mobs[i].NetworkId == 0)
                                {
                                    if (IdOrder[j] < IdOrder[7])
                                    {
                                        camp2.Mobs[i].NetworkId = camp.Mobs[0].NetworkId - ((IdOrder[7] - IdOrder[j]));
                                        camp2.Mobs[i].State = 5;
                                        camp2.Mobs[i].LastChangeOnState = Environment.TickCount;
                                    }
                                    else if (IdOrder[j] > IdOrder[7])
                                    {
                                        camp2.Mobs[i].NetworkId = camp.Mobs[0].NetworkId + ((IdOrder[j] - IdOrder[7]));
                                        camp2.Mobs[i].State = 5;
                                        camp2.Mobs[i].LastChangeOnState = Environment.TickCount;
                                    }
                                    //Console.WriteLine("NetworkID[" + j + "]:" + NetworkID[j] + " and Name: " + NameToCompare[j]);
                                }
                            }
                        }
                        GuessNetworkId1 = 0;
                    }

                    else if (GuessNetworkId1 == 1 && camp.Name == "Red" && camp.Team.ToString().Contains("Order") && visibleMobsCount == camp.Mobs.Count &&
                    camp.Mobs[0].NetworkId != 0 && camp.Mobs[1].NetworkId != 0 && camp.Mobs[2].NetworkId != 0)
                    {
                        Seed1 = (camp.Mobs[1].NetworkId - camp.Mobs[0].NetworkId);
                        Seed2 = (camp.Mobs[2].NetworkId - camp.Mobs[1].NetworkId);

                        //Console.WriteLine("Seed1:" + Seed1 + "  Seed2:" + Seed2);

                        int id = 0;

                        for (int c = 0; c <= 31; c++)
                        {
                            int order = CreateOrder[c];

                            if (c == 2)
                            {
                                id += Seed1;
                                id += Seed2;
                            }
                            else
                            {
                                if (SeedOrder[c] == 1) id += Seed1;
                                else id += Seed2;
                            }

                            IdOrder[order] = id;
                        }

                        foreach (var camp2 in Junglerrrr.Camps.Where(camp2 => camp2.MapType.ToString() == Game.MapId.ToString() && camp2.Name == "Red" && !camp2.Team.ToString().Contains("Order")))
                        {
                            for (int j = 11; j <= 13; j++)
                            {
                                if (IdOrder[j] == 0) continue;
                                int i = 0;
                                switch (j)
                                {
                                    case 11:
                                        i = 2;
                                        break;
                                    case 12:
                                        i = 1;
                                        break;
                                    case 13:
                                        i = 0;
                                        break;
                                    default:
                                        break;
                                }

                                if (camp2.Mobs[i].NetworkId == 0)
                                {
                                    if (IdOrder[j] < IdOrder[10])
                                    {
                                        camp2.Mobs[i].NetworkId = camp.Mobs[0].NetworkId - ((IdOrder[10] - IdOrder[j]));
                                        camp2.Mobs[i].State = 5;
                                        camp2.Mobs[i].LastChangeOnState = Environment.TickCount;
                                    }
                                    else if (IdOrder[j] > IdOrder[10])
                                    {
                                        camp2.Mobs[i].NetworkId = camp.Mobs[0].NetworkId + ((IdOrder[j] - IdOrder[10]));
                                        camp2.Mobs[i].State = 5;
                                        camp2.Mobs[i].LastChangeOnState = Environment.TickCount;
                                    }
                                    //Console.WriteLine("NetworkID[" + j + "]:" + NetworkID[j] + " and Name: " + NameToCompare[j]);
                                }
                            }
                        }
                        GuessNetworkId1 = 0;

                    }
                    else if (GuessNetworkId1 == 1 && camp.Name == "Red" && !camp.Team.ToString().Contains("Order") && visibleMobsCount == camp.Mobs.Count &&
                        camp.Mobs[0].NetworkId != 0 && camp.Mobs[1].NetworkId != 0 && camp.Mobs[2].NetworkId != 0)
                    {
                        Seed1 = (camp.Mobs[1].NetworkId - camp.Mobs[0].NetworkId);
                        Seed2 = (camp.Mobs[2].NetworkId - camp.Mobs[1].NetworkId);

                        //Console.WriteLine("Seed1:" + Seed1 + "  Seed2:" + Seed2);

                        int id = 0;

                        for (int c = 0; c <= 31; c++)
                        {
                            int order = CreateOrder[c];

                            if (c == 2)
                            {
                                id += Seed1;
                                id += Seed2;
                            }
                            else
                            {
                                if (SeedOrder[c] == 1) id += Seed1;
                                else id += Seed2;
                            }

                            IdOrder[order] = id;
                        }

                        foreach (var camp2 in Junglerrrr.Camps.Where(camp2 => camp2.MapType.ToString() == Game.MapId.ToString() && camp2.Name == "Red" && camp2.Team.ToString().Contains("Order")))
                        {
                            for (int j = 8; j <= 10; j++)
                            {
                                if (IdOrder[j] == 0) continue;
                                int i = 0;
                                switch (j)
                                {
                                    case 8:
                                        i = 2;
                                        break;
                                    case 9:
                                        i = 1;
                                        break;
                                    case 10:
                                        i = 0;
                                        break;
                                    default:
                                        break;
                                }

                                if (camp2.Mobs[i].NetworkId == 0)
                                {
                                    if (IdOrder[j] < IdOrder[13])
                                    {
                                        camp2.Mobs[i].NetworkId = camp.Mobs[0].NetworkId - ((IdOrder[13] - IdOrder[j]));
                                        camp2.Mobs[i].State = 5;
                                        camp2.Mobs[i].LastChangeOnState = Environment.TickCount;
                                    }
                                    else if (IdOrder[j] > IdOrder[13])
                                    {
                                        camp2.Mobs[i].NetworkId = camp.Mobs[0].NetworkId + ((IdOrder[j] - IdOrder[13]));
                                        camp2.Mobs[i].State = 5;
                                        camp2.Mobs[i].LastChangeOnState = Environment.TickCount;
                                    }
                                }
                            }
                        }
                        GuessNetworkId1 = 0;
                    }

                    #endregion

                    #region Ping

                    if (camp.State != 1 && !camp.ShouldPing)
                    {
                        camp.ShouldPing = true;
                    }

                    if (camp.State == 1 && camp.ShouldPing)
                    {
                        if (camp.Name == "Baron" && ((InitializeMenu.Menu.Item("pingfow").GetValue<bool>() && visibleMobsCount == 0) || !InitializeMenu.Menu.Item("pingfow").GetValue<bool>()) &&
                            ((InitializeMenu.Menu.Item("pingfow").GetValue<bool>() && visibleMobsCount == 0) || !InitializeMenu.Menu.Item("pingfow").GetValue<bool>()) &&
                            ((InitializeMenu.Menu.Item("pingscreen").GetValue<bool>() && !camp.Position.IsOnScreen()) || !InitializeMenu.Menu.Item("pingscreen").GetValue<bool>()) &&
                            InitializeMenu.Menu.Item("pingbaron").GetValue<bool>() && Environment.TickCount - camp.LastPing >= (InitializeMenu.Menu.Item("pingdelay").GetValue<Slider>().Value * 1000))
                        {
                            Game.ShowPing(PingCategory.Danger, DragonCamp.Position, true);
                            camp.LastPing = Environment.TickCount;
                        }
                        else if (camp.Name == "Dragon" && ((InitializeMenu.Menu.Item("pingfow").GetValue<bool>() && visibleMobsCount == 0) || !InitializeMenu.Menu.Item("pingfow").GetValue<bool>()) &&
                                ((InitializeMenu.Menu.Item("pingfow").GetValue<bool>() && visibleMobsCount == 0) || !InitializeMenu.Menu.Item("pingfow").GetValue<bool>()) &&
                                ((InitializeMenu.Menu.Item("pingscreen").GetValue<bool>() && !camp.Position.IsOnScreen()) || !InitializeMenu.Menu.Item("pingscreen").GetValue<bool>()) &&
                                InitializeMenu.Menu.Item("pingdragon").GetValue<bool>() && Environment.TickCount - camp.LastPing >= (InitializeMenu.Menu.Item("pingdelay").GetValue<Slider>().Value * 1000))
                        {
                            Game.ShowPing(PingCategory.Danger, DragonCamp.Position, true);
                            camp.LastPing = Environment.TickCount;
                        }
                        else
                        {
                            if (((InitializeMenu.Menu.Item("pingfow").GetValue<bool>() && visibleMobsCount == 0) || !InitializeMenu.Menu.Item("pingfow").GetValue<bool>()) &&
                                ((InitializeMenu.Menu.Item("pingscreen").GetValue<bool>() && !camp.Position.IsOnScreen()) || !InitializeMenu.Menu.Item("pingscreen").GetValue<bool>()) &&
                                 InitializeMenu.Menu.Item("pingsmall").GetValue<bool>() && Environment.TickCount - camp.LastPing >= (InitializeMenu.Menu.Item("pingdelay").GetValue<Slider>().Value * 1000))
                            {
                                Game.ShowPing(PingCategory.Normal, camp.Position, true);
                                camp.LastPing = Environment.TickCount;
                            }
                        }
                        camp.ShouldPing = false;
                    }
                    #endregion
                }

                #region Static Menu Update

                Circleradius = InitializeMenu.Menu.Item("circleradius").GetValue<Slider>().Value;
                Colorattacking = InitializeMenu.Menu.Item("colorattacking").GetValue<Color>();
                Colortracked = InitializeMenu.Menu.Item("colortracked").GetValue<Color>();
                Colordisengaged = InitializeMenu.Menu.Item("colordisengaged").GetValue<Color>();
                Colordead = InitializeMenu.Menu.Item("colordead").GetValue<Color>();
                Colorguessed = InitializeMenu.Menu.Item("colorguessed").GetValue<Color>();
                Circlewidth = InitializeMenu.Menu.Item("circlewidth").GetValue<Slider>().Value;

                #endregion

                foreach (var obj in PossibleBaronList.ToList().Where(item => Environment.TickCount >= item[3] + 20000))
                {
                    try
                    {
                        PossibleBaronList.Remove(obj);
                    }
                    catch (Exception)
                    {
                        //ignored
                    }
                }

                UpdateTick = Environment.TickCount;
            }
        }

        private static void GameObjectOnDelete(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Minion) || sender.Team != GameObjectTeam.Neutral)
            {
                return;
            }

            var minion = (Obj_AI_Minion)sender;

            foreach (var camp in Junglerrrr.Camps.Where(camp => camp.MapType.ToString() == Game.MapId.ToString()))
            {
                //Do Stuff for each camp

                foreach (var mob in camp.Mobs.Where(mob => mob.Name == minion.Name))
                {
                    //Do Stuff for each mob in a camp

                    mob.LastChangeOnState = Environment.TickCount - 3000;
                    mob.Unit = null;
                    if (mob.State != 7)
                    {
                        mob.State = 7;
                        mob.JustDied = true;
                    }

                    if (camp.Mobs.Count == 1)
                    {
                        camp.State = mob.State;
                        camp.LastChangeOnState = mob.LastChangeOnState;
                    }
                }
            }

        }

        private static void GameObjectOnCreate(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Minion) || sender.Team != GameObjectTeam.Neutral)
            {
                return;
            }

            var minion = (Obj_AI_Minion)sender;

            foreach (var camp in Junglerrrr.Camps.Where(camp => camp.MapType.ToString() == Game.MapId.ToString()))
            {
                //Do Stuff for each camp

                foreach (var mob in camp.Mobs.Where(mob => mob.Name == minion.Name))
                {
                    //Do Stuff for each mob in a camp

                    mob.NetworkId = minion.NetworkId;
                    mob.LastChangeOnState = Environment.TickCount;
                    mob.Unit = minion;
                    if (!minion.IsDead)
                    {
                        mob.State = 3;
                        mob.JustDied = false;
                    }
                    else
                    {
                        mob.State = 7;
                        mob.JustDied = true;
                    }

                    if (camp.Mobs.Count == 1)
                    {
                        camp.State = mob.State;
                        camp.LastChangeOnState = mob.LastChangeOnState;
                    }

                    if (mob.Name.Contains("Baron") && PossibleBaronList.Count >= 1)
                    {
                        try
                        {
                            PossibleBaronList.Clear();
                        }
                        catch (Exception)
                        {
                            //ignored
                        }
                    }
                }
            }
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            foreach (var camp in Junglerrrr.Camps.Where(camp => camp.MapType.ToString() == Game.MapId.ToString()))
            {
                //Do Stuff for each camp

                #region Minimap Circles

                if (camp.State == 1)
                {
                    Utility.DrawCircle(camp.Position, Circleradius, Colorattacking, Circlewidth + 1, 30, true);
                }
                else if (camp.State == 2)
                {
                    Utility.DrawCircle(camp.Position, Circleradius, Colordisengaged, Circlewidth + 1, 30, true);
                }
                else if (camp.State == 3 && (camp.IsRanged || (camp.Name == "Dragon" || camp.Name == "Crab" || camp.Name == "Spider")))
                {
                    Utility.DrawCircle(camp.Position, Circleradius, Colortracked, Circlewidth, 30, true);
                }
                else if (camp.State == 4)
                {
                    Utility.DrawCircle(camp.Position, Circleradius, Colordead, Circlewidth, 30, true);
                }
                else if (camp.State == 5)
                {
                    Utility.DrawCircle(camp.Position, Circleradius, Colorguessed, Circlewidth, 30, true);
                }

                #endregion
            }
        }

    }

    public class Junglerrrr
    {
        public static List<Camp> Camps;

        static Junglerrrr()
        {
            try
            {
                Camps = new List<Camp>
                {
                    // Order: Blue
                    new Camp("Blue",
                        115, 300, new Vector3(3872f, 7900f, 51f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("SRU_Blue1.1.1"),
                                new Mob("SRU_BlueMini1.1.2", true),
                                new Mob("SRU_BlueMini21.1.3", true)
                            }),
                        Utility.Map.MapType.SummonersRift,
                        GameObjectTeam.Order,
                        Color.Cyan, new Timers(new Vector2(0,0),new Vector2(0,0)), true),
                    //Order: Wolves
                    new Camp("Wolves",
                        115, 100, new Vector3(3825f, 6491f, 52f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("SRU_Murkwolf2.1.1"),
                                new Mob("SRU_MurkwolfMini2.1.2"),
                                new Mob("SRU_MurkwolfMini2.1.3")
                            }),
                        Utility.Map.MapType.SummonersRift,
                        GameObjectTeam.Order,
                        Color.White, new Timers(new Vector2(0,0),new Vector2(0,0))),
                    //Order: Raptor
                    new Camp("Raptor",
                        115, 100, new Vector3(6954f, 5458f, 53f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("SRU_Razorbeak3.1.1", true),
                                new Mob("SRU_RazorbeakMini3.1.2"),
                                new Mob("SRU_RazorbeakMini3.1.3"),
                                new Mob("SRU_RazorbeakMini3.1.4")
                            }),
                        Utility.Map.MapType.SummonersRift,
                        GameObjectTeam.Order,
                        Color.Salmon, new Timers(new Vector2(0,0),new Vector2(0,0)), true),
                    //Order: Red
                    new Camp("Red",
                        115, 300, new Vector3(7862f, 4111f, 54f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("SRU_Red4.1.1"),
                                new Mob("SRU_RedMini4.1.2", true),
                                new Mob("SRU_RedMini4.1.3", true)
                            }),
                        Utility.Map.MapType.SummonersRift,
                        GameObjectTeam.Order,
                        Color.Red, new Timers(new Vector2(0,0),new Vector2(0,0)), true),
                        
                    //Order: Krug
                    new Camp("Krug",
                        115, 100, new Vector3(8381f, 2711f, 51f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("SRU_Krug5.1.2"),
                                new Mob("SRU_KrugMini5.1.1")
                            }),
                        Utility.Map.MapType.SummonersRift,
                        GameObjectTeam.Order,
                        Color.White, new Timers(new Vector2(0,0),new Vector2(0,0))),
                    //Order: Gromp
                    new Camp("Gromp",
                        115, 100, new Vector3(2091f, 8428f, 52f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("SRU_Gromp13.1.1", true)
                            }),
                        Utility.Map.MapType.SummonersRift,
                        GameObjectTeam.Order,
                        Color.Green, new Timers(new Vector2(0,0),new Vector2(0,0)), true),
                    //Chaos: Blue
                    new Camp("Blue",
                        115, 300, new Vector3(10930f, 6992f, 52f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("SRU_Blue7.1.1"),
                                new Mob("SRU_BlueMini7.1.2", true),
                                new Mob("SRU_BlueMini27.1.3", true)
                            }),
                        Utility.Map.MapType.SummonersRift,
                        GameObjectTeam.Chaos,
                        Color.Cyan, new Timers(new Vector2(0,0),new Vector2(0,0)), true),
                    //Chaos: Wolves
                    new Camp("Wolves",
                        115, 100, new Vector3(10957f, 8350f, 62f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("SRU_Murkwolf8.1.1"),
                                new Mob("SRU_MurkwolfMini8.1.2"),
                                new Mob("SRU_MurkwolfMini8.1.3")
                            }),
                        Utility.Map.MapType.SummonersRift,
                        GameObjectTeam.Chaos,
                        Color.White, new Timers(new Vector2(0,0),new Vector2(0,0))),
                    //Chaos: Raptor
                    new Camp("Raptor",
                        115, 100, new Vector3(7857f, 9471f, 52f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("SRU_Razorbeak9.1.1", true),
                                new Mob("SRU_RazorbeakMini9.1.2"),
                                new Mob("SRU_RazorbeakMini9.1.3"),
                                new Mob("SRU_RazorbeakMini9.1.4")
                            }),
                        Utility.Map.MapType.SummonersRift,
                        GameObjectTeam.Chaos,
                        Color.Salmon, new Timers(new Vector2(0,0),new Vector2(0,0)), true),
                    //Chaos: Red
                    new Camp("Red",
                        115, 300, new Vector3(7017f, 10775f, 56f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("SRU_Red10.1.1"),
                                new Mob("SRU_RedMini10.1.2", true),
                                new Mob("SRU_RedMini10.1.3", true)
                            }),
                        Utility.Map.MapType.SummonersRift,
                        GameObjectTeam.Chaos,
                        Color.Red, new Timers(new Vector2(0,0),new Vector2(0,0)), true),
                    //Chaos: Krug
                    new Camp("Krug",
                        115, 100, new Vector3(6449f, 12117f, 56f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("SRU_Krug11.1.2"),
                                new Mob("SRU_KrugMini11.1.1")
                            }),
                        Utility.Map.MapType.SummonersRift,
                        GameObjectTeam.Chaos,
                        Color.White, new Timers(new Vector2(0,0),new Vector2(0,0))),
                    //Chaos: Gromp
                    new Camp("Gromp",
                        115, 100, new Vector3(12703f, 6444f, 52f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("SRU_Gromp14.1.1", true)
                            }),
                        Utility.Map.MapType.SummonersRift,
                        GameObjectTeam.Chaos,
                        Color.Green, new Timers(new Vector2(0,0),new Vector2(0,0)), true),
                    //Neutral: Dragon
                    new Camp("Dragon",
                        150, 360, new Vector3(9866f, 4414f, -71f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("SRU_Dragon6.1.1")
                            }),
                        Utility.Map.MapType.SummonersRift,
                        GameObjectTeam.Neutral,
                        Color.Orange, new Timers(new Vector2(0,0),new Vector2(0,0))),
                    //Neutral: Baron
                    new Camp("Baron",
                        120, 420, new Vector3(5007f, 10471f, -71f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("SRU_Baron12.1.1", true, null, 0)
                            }),
                        Utility.Map.MapType.SummonersRift,
                        GameObjectTeam.Neutral,
                        Color.DarkOrchid, new Timers(new Vector2(0,0),new Vector2(0,0)), true,  8),
                    //Dragon: Crab
                    new Camp("Crab",
                        150, 180, new Vector3(10508f, 5271f, -62f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("Sru_Crab15.1.1")
                            }),
                        Utility.Map.MapType.SummonersRift,
                        GameObjectTeam.Neutral,
                        Color.PaleGreen, new Timers(new Vector2(0,0),new Vector2(0,0))),
                    //Baron: Crab
                    new Camp("Crab",
                        150, 180, new Vector3(4418f, 9664f, -69f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("Sru_Crab16.1.1")
                            }),
                        Utility.Map.MapType.SummonersRift,
                        GameObjectTeam.Neutral,
                        Color.PaleGreen, new Timers(new Vector2(0,0),new Vector2(0,0))),
                    //Order: Wraiths
                    new Camp("Wraiths",
                        95, 75, new Vector3(4373f, 5843f, -107f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("TT_NWraith1.1.1", true),
                                new Mob("TT_NWraith21.1.2", true),
                                new Mob("TT_NWraith21.1.3", true)
                            }),
                        Utility.Map.MapType.TwistedTreeline,
                        GameObjectTeam.Order,
                        Color.White, new Timers(new Vector2(0,0),new Vector2(0,0)), true),
                    //Order: Golems
                    new Camp("Golems",
                        95, 75, new Vector3(5107f, 7986f, -108f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("TT_NGolem2.1.1"),
                                new Mob("TT_NGolem22.1.2")
                            }),
                        Utility.Map.MapType.TwistedTreeline,
                        GameObjectTeam.Order,
                        Color.White, new Timers(new Vector2(0,0),new Vector2(0,0))),
                    //Order: Wolves
                    new Camp("Wolves",
                        95, 75, new Vector3(6078f, 6094f, -99f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("TT_NWolf3.1.1"),
                                new Mob("TT_NWolf23.1.2"),
                                new Mob("TT_NWolf23.1.3")
                            }),
                         Utility.Map.MapType.TwistedTreeline,
                         GameObjectTeam.Order,
                        Color.White, new Timers(new Vector2(0,0),new Vector2(0,0))),
                    //Chaos: Wraiths
                    new Camp("Wraiths",
                        95, 75, new Vector3(11026f, 5806f, -107f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("TT_NWraith4.1.1", true),
                                new Mob("TT_NWraith24.1.2", true),
                                new Mob("TT_NWraith24.1.3", true)
                            }),
                        Utility.Map.MapType.TwistedTreeline,
                        GameObjectTeam.Chaos,
                        Color.White, new Timers(new Vector2(0,0),new Vector2(0,0)), true),
                    //Chaos: Golems
                    new Camp("Golems",
                        95, 75, new Vector3(10277f, 8038f, -109f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("TT_NGolem5.1.1"),
                                new Mob("TT_NGolem25.1.2")
                            }),
                        Utility.Map.MapType.TwistedTreeline,
                        GameObjectTeam.Chaos,
                        Color.White, new Timers(new Vector2(0,0),new Vector2(0,0))),
                    //Chaos: Wolves
                    new Camp("Wolves",
                        95, 75, new Vector3(9294f, 6085f, -97f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("TT_NWolf6.1.1"),
                                new Mob("TT_NWolf26.1.2"),
                                new Mob("TT_NWolf26.1.3") }),
                         Utility.Map.MapType.TwistedTreeline,
                         GameObjectTeam.Chaos,
                        Color.White, new Timers(new Vector2(0,0),new Vector2(0,0))),
                    //Neutral: Spider
                    new Camp("Spider",
                        600, 360, new Vector3(7738f, 10080f, -62f),
                        new List<Mob>(
                            new[]
                            {
                                new Mob("TT_Spiderboss8.1.1")
                            }),
                        Utility.Map.MapType.TwistedTreeline,
                        GameObjectTeam.Neutral,
                        Color.DarkOrchid, new Timers(new Vector2(0,0),new Vector2(0,0)), true)
                };
            }
            catch (Exception)
            {
                Camps = new List<Camp>();
            }
        }

        public class Camp
        {
            public Camp(string name,
                float spawnTime,
                int respawnTimer,
                Vector3 position,
                List<Mob> mobs,
                Utility.Map.MapType mapType,
                GameObjectTeam team,
                Color colour,
                Timers timer,
                bool isRanged = false,
                int state = 0,
                int respawnTime = 0,
                int lastChangeOnState = 0,
                bool shouldping = true,
                int lastPing = 0)
            {
                Name = name;
                SpawnTime = spawnTime;
                RespawnTimer = respawnTimer;
                Position = position;
                MapPosition = Drawing.WorldToScreen(Position);
                MinimapPosition = Drawing.WorldToMinimap(Position);
                Mobs = mobs;
                MapType = mapType;
                Team = team;
                Colour = colour;
                IsRanged = isRanged;
                State = state;
                RespawnTime = respawnTime;
                LastChangeOnState = lastChangeOnState;
                Timer = timer;
                ShouldPing = shouldping;
                LastPing = lastPing;
            }

            public string Name { get; set; }
            public float SpawnTime { get; set; }
            public int RespawnTimer { get; set; }
            public Vector3 Position { get; set; }
            public Vector2 MinimapPosition { get; set; }
            public Vector2 MapPosition { get; set; }
            public List<Mob> Mobs { get; set; }
            public Utility.Map.MapType MapType { get; set; }
            public GameObjectTeam Team { get; set; }
            public Color Colour { get; set; }
            public bool IsRanged { get; set; }
            public int State { get; set; }
            public int RespawnTime { get; set; }
            public int LastChangeOnState { get; set; }
            public Timers Timer { get; set; }
            public bool ShouldPing { get; set; }
            public int LastPing { get; set; }

        }

        public class Mob
        {
            public Mob(string name, bool isRanged = false, Obj_AI_Minion unit = null, int state = 0, int networkId = 0, int lastChangeOnState = 0, bool justDied = false)
            {
                Name = name;
                IsRanged = isRanged;
                Unit = unit;
                State = state;
                NetworkId = networkId;
                LastChangeOnState = lastChangeOnState;
                JustDied = justDied;
            }

            public Obj_AI_Minion Unit { get; set; }
            public string Name { get; set; }
            public bool IsRanged { get; set; }
            public int State { get; set; }
            public int NetworkId { get; set; }
            public int LastChangeOnState { get; set; }
            public bool JustDied { get; set; }
        }

        public class Timers
        {
            public Timers(Vector2 position, Vector2 minimapPosition, string textOnMap = "", string textOnMinimap = "")
            {
                TextOnMap = textOnMap;
                TextOnMinimap = textOnMinimap;
                Position = position;
                MinimapPosition = minimapPosition;
            }

            public string TextOnMap { get; set; }
            public string TextOnMinimap { get; set; }
            public Vector2 Position { get; set; }
            public Vector2 MinimapPosition { get; set; }
        }
    }

    public class Packetssss
    {
        public static OnAttack Attack;
        public static OnMissileHit MissileHit;
        public static OnDisengaged Disengaged;
        public static OnMonsterSkill MonsterSkill;
        public static OnCreateGromp CreateGromp;
        public static OnCreateCampIcon CreateCampIcon;

        static Packetssss()
        {
            try
            {
                Attack = new OnAttack();
                MissileHit = new OnMissileHit();
                Disengaged = new OnDisengaged();
                MonsterSkill = new OnMonsterSkill();
                CreateGromp = new OnCreateGromp();
                CreateCampIcon = new OnCreateCampIcon();
            }
            catch (Exception)
            {
                //ignored
            }
        }

        public class OnAttack
        {
            public OnAttack(int header = 0, int length = 71)
            {
                Length = length;
                Header = header;
            }
            public int Header { get; set; }
            public int Length { get; set; }
        }

        public class OnMissileHit
        {
            public OnMissileHit(int header = 0, int length = 35)
            {
                Length = length;
                Header = header;
            }
            public int Header { get; set; }
            public int Length { get; set; }
        }

        public class OnDisengaged
        {
            public OnDisengaged(int header = 0, int length = 68)
            {
                Length = length;
                Header = header;
            }
            public int Header { get; set; }
            public int Length { get; set; }
        }

        public class OnMonsterSkill
        {
            public OnMonsterSkill(int header = 0, int length = 47, int length2 = 68)
            {
                Length = length;
                Length2 = length2;
                Header = header;
            }
            public int Header { get; set; }
            public int Length { get; set; }
            public int Length2 { get; set; }
        }

        public class OnCreateGromp
        {
            public OnCreateGromp(int header = 0, int length = 302, int length2 = 311)
            {
                Length = length;
                Length2 = length2;
                Header = header;
            }
            public int Header { get; set; }
            public int Length { get; set; }
            public int Length2 { get; set; }
        }

        public class OnCreateCampIcon
        {
            public OnCreateCampIcon(int header = 0, int length = 74, int length2 = 86, int length3 = 83, int length4 = 62, int length5 = 71)
            {
                Length = length;
                Length2 = length2;
                Length3 = length3;
                Length4 = length4;
                Length5 = length5;
                Header = header;
            }
            public int Header { get; set; }
            public int Length { get; set; }
            public int Length2 { get; set; }
            public int Length3 { get; set; }
            public int Length4 { get; set; }
            public int Length5 { get; set; }
        }
    }
}
