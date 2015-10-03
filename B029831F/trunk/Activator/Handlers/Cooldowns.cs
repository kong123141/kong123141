#region Copyright © 2015 Kurisu Solutions
// All rights are reserved. Transmission or reproduction in part or whole,
// any form or by any means, mechanical, electronical or otherwise, is prohibited
// without the prior written consent of the copyright owner.
// 
// Document:	Handlers/Cooldowns.cs
// Date:		22/09/2015
// Author:		Robin Kurisu
#endregion

using LeagueSharp;

namespace Activator.Handlers
{
    class Cooldowns
    {
        public static void Load()
        {
            Obj_AI_Base.OnProcessSpellCast += HeroOnProcessSpellCast;
        }

        private static void HeroOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {

        }
    }
}
