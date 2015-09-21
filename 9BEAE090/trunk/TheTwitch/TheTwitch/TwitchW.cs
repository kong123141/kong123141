using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheTwitch.Commons.ComboSystem;

namespace TheTwitch
{
    class TwitchW : Skill
    {
        public int MinFarmMinions;
        public Circle DrawRange;
        private bool _afterAttack;
        public bool IsAreaOfEffect;
        public bool NotDuringR;
        public int HarassAfterStacks;
        public int ComboAfterStacks;

        public TwitchW(SpellSlot slot)
            : base(slot)
        {
            SetSkillshot(0.25f, 275f, 1400f, false, SkillshotType.SkillshotCircle);
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            HarassEnabled = false;
        }

        private void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                _afterAttack = true;
            }
        }

        public override void Update(Orbwalking.OrbwalkingMode mode, ComboProvider combo, Obj_AI_Hero target)
        {
            base.Update(mode, combo, target);
            if (_afterAttack) _afterAttack = false;
        }

        public override void Execute(Obj_AI_Hero target)
        {
            if (((target.GetBuffCount("twitchdeadlyvenom") >= ComboAfterStacks || ComboAfterStacks == 0) && _afterAttack || !Orbwalking.InAutoAttackRange(target)) && (!NotDuringR || !ObjectManager.Player.HasBuff("TwitchFullAutomatic")))
            {
                Cast(target, aoe: IsAreaOfEffect);
            }
        }
        public override void Harass(Obj_AI_Hero target)
        {
            if (((target.GetBuffCount("twitchdeadlyvenom") >= HarassAfterStacks || HarassAfterStacks == 0) && _afterAttack || !Orbwalking.InAutoAttackRange(target)) && (!NotDuringR || !ObjectManager.Player.HasBuff("TwitchFullAutomatic")))
                Cast(target, aoe: IsAreaOfEffect);
        }

        public override void Draw()
        {
            if (DrawRange.Active)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 950, DrawRange.Color);
        }

        public override void LaneClear()
        {
            var location = GetCircularFarmLocation(MinionManager.GetMinions(950, MinionTypes.All, MinionTeam.NotAlly));
            if (location.MinionsHit >= MinFarmMinions)
                Cast(location.Position);
        }

        public override void Gapcloser(ComboProvider combo, ActiveGapcloser gapcloser)
        {
            Cast(gapcloser.Sender, true);
        }

        public override int GetPriority()
        {
            return 3;
        }
    }
}
