using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace EndifsCollections.Tools
{
    class myRePriority
    {
        public List<Obj_AI_Hero> Heroes;
        public List<Obj_AI_Hero> Enemies;
        private Menu tools;

        public myRePriority()
        {
            Heroes = ObjectManager.Get<Obj_AI_Hero>().ToList();
            Enemies = Heroes.Where(x => x.IsEnemy && !x.IsMe).ToList();           
            foreach (var enemy in Enemies)
            {
                TargetSelector.SetPriority(enemy, ResortDB(enemy.ChampionName));
            }
        }
        public static int ResortDB(string championName)
        {           
            string[] TopTank =
            {
                 "Dr. Mundo","Garen", "Gnar","Hecarim", "Shen", "Sion", "Renekton",   
            };
            string[] TopOffTank =
            {
                 "Aatrox", "Darius","Irelia", "Jax","Malphite", "Maokai","Nasus", "Olaf", "Poppy", "Riven", "Shyvana","Singed", "Trundle",
                 "Yorick"
            };
            string[] TopCarry =
            {
                 "Fiora","Gangplank","Kayle", "Rumble", "Tryndamere"
            };
            string[] JungleTank =
            {
                 "Amumu","Gragas","Lee Sin", "Nunu","Sejuani","Skarner", "Volibear", "Zac",  "Vi","Udyr",  "RekSai"
            };
            string[] JungleOffTank =
            {
                 "Lee Sin","Jarvan IV", "Pantheon",  "Warwick", "MonkeyKing", "XinZhao"
            };
            string[] JungleCarry =
            {
                 "Elise", "Evelynn", "Fiddlesticks", "Kha'Zix", "MasterYi", "Nidalee","Nocturne", "Rammus", "Rengar", "Shaco"
            };
            string[] Mid =
            {
                 "Ahri", "Anivia","Azir", "Brand","Cassiopeia","Akali", "Diana", "Heimerdinger", "Fizz","Jayce", "Kassadin",
                 "Karthus", "Katarina","Kennen",  "LeBlanc","Lissandra", "Lux", "Malzahar","Mordekaiser",  "Orianna",  "Ryze","Syndra",
                 "Swain", "Vladimir",  "Veigar","Talon", "TwistedFate",  "VelKoz", "Viktor", "Xerath", "Zed", "Ziggs","Yasuo", "Ekko"
            };
            string[] MidTank =
            {
                 "Cho'Gath"
            };
            string[] SupportTank =
            {
                  "Alistar", "Blitzcrank", "Braum", "Leona", "Nautilus", "Taric", "Thresh"
            };
            string[] SupportCarry =
            {
                 "Annie", "Karma", "Lulu", "Morgana", "Sona", "Zyra"
            };
            string[] Support =
            {
                 "Bard", "Janna", "Nami", "Soraka", "Zilean"
            };
            string[] ADC =
            {
                  "Caitlyn", "Corki", "Draven", "Ashe", "Ezreal", "Graves", "Jinx", "Kalista", "KogMaw", "Lucian", "MissFortune",
                  "Quinn", "Sivir","Teemo", "Tristana", "Twitch", "Varus", "Vayne", "Urgot", 
            };
            string[] TankCarry =
            {
                  "Urgot", 
            };
            if (TopTank.Contains(championName) || JungleOffTank.Contains(championName) || MidTank.Contains(championName))
            {
                return 1;
            }
            if (TopOffTank.Contains(championName) || JungleTank.Contains(championName) || SupportTank.Contains(championName))
            {
                return 2;
            }
            if (TopCarry.Contains(championName) || JungleCarry.Contains(championName) || Support.Contains(championName))
            {
                return 3;
            }
            if (Mid.Contains(championName) || SupportCarry.Contains(championName) || TankCarry.Contains(championName))
            {
                return 4;
            }
            if (ADC.Contains(championName))
            {
                return 5;
            }
            return 1;
        }
    }      
}

