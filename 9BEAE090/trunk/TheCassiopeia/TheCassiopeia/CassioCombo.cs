using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TheCassiopeia.Commons;
using TheCassiopeia.Commons.ComboSystem;
using Collision = LeagueSharp.Common.Collision;

namespace TheCassiopeia
{
    class CassioCombo : ComboProvider
    {
        public bool AutoInCombo;
        public MenuItem AssistedUltMenu;
        private CassR _r;
        private CassQ _q;
        private CassE _e;
        public bool BlockBadUlts;
        public bool EnablePoisonTargetSelection;
        private float _assistedUltTime;
        public MenuItem LanepressureMenu;
        private bool _gaveAutoWarning;
        private Vector3 _castPosition;
        private Vector3 _objectPosition;
        private float _objectTime;
        public MenuItem BurstMode;
        public bool IgniteInBurstMode;
        public bool OnlyIgniteWhenNoE;

        public CassioCombo(float targetSelectorRange, IEnumerable<Skill> skills, Orbwalking.Orbwalker orbwalker)
            : base(targetSelectorRange, skills, orbwalker)
        {
        }

        public CassioCombo(float targetSelectorRange, Orbwalking.Orbwalker orbwalker, params Skill[] skills)
            : base(targetSelectorRange, orbwalker, skills)
        {
        }

        public override void Initialize()
        {
            _r = GetSkill<CassR>();
            _q = GetSkill<CassQ>();
            _e = GetSkill<CassE>();
            Spellbook.OnCastSpell += OnCastSpell;
            Obj_AI_Base.OnProcessSpellCast += OnSpellCast;
            Orbwalking.BeforeAttack += OrbwalkerBeforeAutoAttack;
            GameObject.OnCreate += OnCreateGameObject;
            base.Initialize();
        }

        private void OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "CassiopeiaNoxiousBlast")
                _castPosition = args.End;
        }

        private void OnCreateGameObject(GameObject sender, EventArgs args)
        {
            if (sender.Name == "CassNoxious_tar.troy")
            {
                _objectTime = 0f;
            }

            if (sender.Name == "Cassiopeia_Base_Q_Hit_Green.troy" && sender.Position.Distance(_castPosition) < 10 && _q.FastCombo)
            {
                _objectPosition = sender.Position;
                _objectTime = Game.Time;
            }
        }

        private void OrbwalkerBeforeAutoAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Target.IsValidTarget() && Target.IsPoisoned())
                args.Process = AutoInCombo;
        }

        private void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {


            if (Game.Time - _assistedUltTime < _r.Delay)
            {
                _assistedUltTime = 0f;
                return;
            }

            if (AssistedUltMenu.IsActive())
            {
                args.Process = false;
                return;
            }

            if (!BlockBadUlts) return;

            if (sender.Owner.IsMe && args.Slot == SpellSlot.R && HeroManager.Enemies.All(enemy => !enemy.IsValidTarget(_r.Range) || !_r.WillHit(enemy, args.StartPosition)))
            {
                args.Process = false;
            }
        }

        private void CastAssistedUlt()
        {
            if (!_r.CanCast(Target)) return;
            var pred = _r.GetPrediction(Target);
            if (pred.Hitchance >= Commons.Prediction.HitChance.Low)
            {
                _assistedUltTime = Game.Time;
                _r.Cast(pred.CastPosition);
            }
        }

        protected override void OnUpdate(Orbwalking.OrbwalkingMode mode)
        {
            if (AssistedUltMenu != null && AssistedUltMenu.GetValue<KeyBind>().Active)
                CastAssistedUlt();

            if (Game.Time - _objectTime < 0.5f)
                foreach (var enemy in HeroManager.Enemies)
                    if (enemy.IsValidTarget() && enemy.ServerPosition.Distance(_objectPosition) < _q.Instance.SData.CastRadius + enemy.BoundingRadius - (enemy.MoveSpeed * (0.5f - (Game.Time - _objectTime))))
                        SetMarked(enemy, 0.25f);


            if (mode == Orbwalking.OrbwalkingMode.LaneClear && LanepressureMenu.IsActive())
                mode = Orbwalking.OrbwalkingMode.Mixed;



            base.OnUpdate(mode);

            if (mode == Orbwalking.OrbwalkingMode.Combo && IgniteInBurstMode && BurstMode.IsActive() && Target.IsValidTarget(600) && ObjectManager.Player.CalcDamage(Target, Damage.DamageType.True, ObjectManager.Player.GetIgniteDamage()) > Target.Health + Target.HPRegenRate * 5 && (_e.Instance.CooldownExpires > Game.Time + 0.5f || !OnlyIgniteWhenNoE))
            {
                var ignite = ObjectManager.Player.Spellbook.Spells.FirstOrDefault(spell => spell.Name == "summonerdot");
                if (ignite != null && ignite.IsReady())
                    ObjectManager.Player.Spellbook.CastSpell(ignite.Slot, Target);
            }

            if (!_gaveAutoWarning && Game.Time > 30 * 60f)
            {
                _gaveAutoWarning = true;
                Notifications.AddNotification(new Notification("Tipp: Disable AA in combo\nfor better lategame kiting!", 6000) { BorderColor = new SharpDX.Color(154, 205, 50) });
            }
        }

        public override bool ShouldBeDead(Obj_AI_Base target, float additionalSpellDamage = 0f)
        {
            return base.ShouldBeDead(target, GetRemainingCassDamage(target));
        }

        public float GetRemainingCassDamage(Obj_AI_Base target)
        {
            var buff = target.GetBuff("cassiopeianoxiousblastpoison");
            float damage = 0;
            if (buff != null)
                damage += (float)(((int)(buff.EndTime - Game.Time)) * (ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q) / 3));

            buff = target.GetBuff("cassiopeiamiasmapoison");
            if (buff != null)
            {
                damage += (float)(((int)(buff.EndTime - Game.Time)) * (ObjectManager.Player.GetSpellDamage(target, SpellSlot.W)));
            }

            return damage;
        }

        protected override Obj_AI_Hero SelectTarget()
        {
            var target = base.SelectTarget();
            var valid = target.IsValidTarget();
            bool invulnerable = false;
            if (valid)
                invulnerable = TargetSelector.IsInvulnerable(target, TargetSelector.DamageType.Magical);

            if (!valid || target.IsBehindWindWall() || invulnerable)
            {
                var newTarget = HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(TargetRange) && !enemy.IsBehindWindWall() && !TargetSelector.IsInvulnerable(enemy, TargetSelector.DamageType.Magical)).MaxOrDefault(TargetSelector.GetPriority);
                if (newTarget != null)
                    target = newTarget;
            }

            if (EnablePoisonTargetSelection && target.IsValidTarget() && !target.IsPoisoned())
            {
                var newTarget = HeroManager.Enemies.Where(enemy => enemy.IsValidTarget(TargetRange) && !enemy.IsBehindWindWall() && enemy.IsPoisoned() && !TargetSelector.IsInvulnerable(enemy, TargetSelector.DamageType.Magical)).MaxOrDefault(TargetSelector.GetPriority);
                if (newTarget != null && TargetSelector.GetPriority(target) - TargetSelector.GetPriority(newTarget) < 0.5f)
                    return newTarget;
            }
            return target;
        }
    }
}
