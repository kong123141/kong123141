using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;

namespace TheKalista.Commons.Items
{
    class MercuralScimitar : Qss
    {
        public override string GetDisplayName()
        {
            return "Mercurial Scimitar";
        }

        public override void Use(Obj_AI_Base target)
        {
            if (ObjectManager.Player.Spellbook.Spells.Any(spell => spell.Name == "itemmercurial"))
                ObjectManager.Player.Spellbook.CastSpell(ObjectManager.Player.Spellbook.Spells.First(spell => spell.Name == "itemmercurial").Slot);
            Console.WriteLine("lol");

        }
    }
}
