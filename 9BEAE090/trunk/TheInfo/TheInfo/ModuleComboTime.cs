using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace TheInfo
{
    class ModuleComboTime : IModule
    {
        private const int MaxCombotimeDistance = 2500;
        private ActivateHelper _activation;
        private readonly List<SpellSlot> _slots = new List<SpellSlot>();

        private Menu _comboTime;

        public void Initialize()
        {
            Game.OnUpdate += Tick;
        }

        public void InitializeMenu(Menu rootMenu)
        {
            _comboTime = new Menu("Combo Time", "Combo Time");

            var explanation = new Menu("Explanation", "Explanation");
            explanation.AddItem(new MenuItem("wow", "Converts all dmg to DPS"));
            explanation.AddItem(new MenuItem("such info", "and shows DPS time to kill"));

            var skills = new Menu("Skills", "Skills");
            skills.AddItem(RegisterSkillslotCalc(SpellSlot.Q, new MenuItem("Use Q", "Use Q").SetValue(true)));
            skills.AddItem(RegisterSkillslotCalc(SpellSlot.W, new MenuItem("Use W", "Use W").SetValue(true)));
            skills.AddItem(RegisterSkillslotCalc(SpellSlot.E, new MenuItem("Use E", "Use E").SetValue(true)));
            skills.AddItem(RegisterSkillslotCalc(SpellSlot.R, new MenuItem("Use R", "Use R").SetValue(true)));


            _comboTime.AddItem(new MenuItem("Use Autoattacks", "Use Autoattacks").SetValue(true));
            _comboTime.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));

            _comboTime.AddSubMenu(explanation);
            _activation = new ActivateHelper(_comboTime, "Activation");
            _comboTime.AddSubMenu(skills);
            rootMenu.AddSubMenu(_comboTime);
        }

        private void Tick(EventArgs args)
        {
            if (!_comboTime.Item("Enabled").GetValue<bool>())
                return;

            if (!_activation.GetActivated(() => HeroManager.AllHeroes.Count(hero => hero.Distance(ObjectManager.Player) < MaxCombotimeDistance) > HeroManager.AllHeroes.Count / 2f))
                return;

            foreach (var enemy in HeroManager.Enemies)
                if ((_activation.IsSmart && enemy.Distance(ObjectManager.Player) < MaxCombotimeDistance) || !_activation.IsSmart)
                    DrawDamage(ObjectManager.Player, enemy);



        }

        private void DrawDamage(Obj_AI_Hero sender, Obj_AI_Hero target)
        {
            if (!target.IsVisible || target.IsDead)
                return;
            var screenPos = Drawing.WorldToScreen(target.Position);
            if (_slots.Sum(slot => sender.GetSpell(slot).Cooldown <= 0 ?  sender.GetSpellDamage(target, slot) : 0) > target.Health)
                Drawing.DrawText(screenPos.X, screenPos.Y, Color.Red, "Combo");
            else
            {
                var dps = _slots.Sum(slot => sender.GetSpellDamage(target, slot) / sender.GetSpell(slot).Cooldown) + sender.TotalAttackDamage * (1 / sender.AttackDelay) * ((sender.Crit + 100) / 100f) * (100 / (100f + target.Armor));
           //     Console.WriteLine(dps + " / "+ sender.TotalAttackDamage * (1 / sender.AttackDelay) * ((sender.Crit + 100) / 100f) * (100 / (100f + target.Armor)));

                Drawing.DrawText(screenPos.X, screenPos.Y, Color.Red, "> " + string.Format("{0:0.0} s", target.Health / dps));
            }
        }

        private MenuItem RegisterSkillslotCalc(SpellSlot slot, MenuItem item)
        {
            item.ValueChanged += (sender, args) =>
            {
                if (args.GetNewValue<bool>())
                {
                    _slots.Add(slot);
                }
                else if (_slots.Contains(slot))
                {
                    _slots.Remove(slot);
                }
            };

            if (item.GetValue<bool>())
                _slots.Add(slot);
            return item;
        }
    }
}
