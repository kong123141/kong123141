
using LeagueSharp.Common;


namespace PewPewQuinn
{
    public class Program : PewPewQuinn

{
    public static void Main(string[] args)
    {
        CustomEvents.Game.OnGameLoad += OnLoad;
    }
}
}
        
