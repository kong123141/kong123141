using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TheKalista.Commons.ComboSystem;

namespace TheKalista
{
    class KalistaW : Skill
    {
        public readonly Vector3 BaronPosition = new Vector3(4944, 10388, -712406f);
        public readonly Vector3 DragonPosition = new Vector3(9918f, 4474f, -71.2406f);

        public KalistaW(SpellSlot slot, float range, TargetSelector.DamageType damageType) : base(slot, range, damageType)
        {
        }

        public KalistaW(SpellSlot slot) : base(slot)
        {
        }

        public override void Execute(Obj_AI_Hero target) { }

        public override int GetPriority()
        {
            return 1;
        }
    }
}
