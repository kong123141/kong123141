﻿using LeagueSharp;
using LeagueSharp.Common;

namespace TRUStinMarksman.Utils.Items
{
    internal class _3184 : Item
    {
        internal override int Id
        {
            get { return 3184; }
        }

        internal override string Name
        {
            get { return "Entropy"; }
        }

        public override void Use()
        {
            var target = TRUStinMarksman.Orbwalker.GetTarget();

            if (!target.IsValid<Obj_AI_Hero>())
            {
                return;
            }

            var targetHero = (Obj_AI_Hero) target;

            if (targetHero.IsValidTarget())
            {
                LeagueSharp.Common.Items.UseItem(Id, TRUStinMarksman.Player);
            }
        }
    }
}