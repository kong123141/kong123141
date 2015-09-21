using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace TheKalista.Commons.Items
{
    class Qss : IActivateableItem
    {
        private bool _blind, _stun, _fear, _snare, _polymorph, _silence, _charm, _exhaust, _ignite, _sleep, _taunt, _noAliW;
        private Slider _minDuration;
        private bool _supress;

        public virtual string GetDisplayName()
        {
            return "QSS";
        }

        public void Initialize(Menu menu, ItemManager itemManager)
        {
            menu.AddMItem("Min duration in ms", new Slider(1000, 0, 3000), (sender, args) => _minDuration = args.GetNewValue<Slider>());
            menu.AddMItem("1000 ms = 1 sec");
            var typeMenu = menu.CreateSubmenu("Use on");
            typeMenu.AddMItem("Blind", false, (sender, args) => _blind = args.GetNewValue<bool>());
            typeMenu.AddMItem("Silence", false, (sender, args) => _silence = args.GetNewValue<bool>());

            typeMenu.AddMItem("Stun", true, (sender, args) => _stun = args.GetNewValue<bool>());
            typeMenu.AddMItem("Surpress", true, (sender, args) => _supress = args.GetNewValue<bool>());
            typeMenu.AddMItem("Fear", true, (sender, args) => _fear = args.GetNewValue<bool>());
            typeMenu.AddMItem("Snare", true, (sender, args) => _snare = args.GetNewValue<bool>());
            typeMenu.AddMItem("Polymorph", true, (sender, args) => _polymorph = args.GetNewValue<bool>());
            typeMenu.AddMItem("Charm", true, (sender, args) => _charm = args.GetNewValue<bool>());
            typeMenu.AddMItem("Sleep", true, (sender, args) => _sleep = args.GetNewValue<bool>());
            typeMenu.AddMItem("Taunt", true, (sender, args) => _taunt = args.GetNewValue<bool>());
            var miscMenu = menu.CreateSubmenu("Misc");
            miscMenu.AddMItem("Don't use on Alistar W (is a stun)", true, (sender, args) => _noAliW = args.GetNewValue<bool>());
            miscMenu.AddMItem("Use on killable Ignite", true, (sender, args) => _ignite = args.GetNewValue<bool>());
            miscMenu.AddMItem("Use on Exhaust", true, (sender, args) => _exhaust = args.GetNewValue<bool>());


        }

        public void Update(Obj_AI_Hero target)
        {
            foreach (var buff in ObjectManager.Player.Buffs)
            {
                if (buff.Type == BuffType.Blind && _blind || buff.Type == BuffType.Stun && _stun || buff.Type == BuffType.Fear && _fear || buff.Type == BuffType.Snare && _snare || buff.Type == BuffType.Polymorph && _polymorph || buff.Type == BuffType.Silence && _silence || buff.Type == BuffType.Charm && _charm ||
                    buff.Type == BuffType.Sleep && _sleep || buff.Type == BuffType.Taunt && _taunt || buff.Type == BuffType.Suppression && _supress)
                {
                    //Console.WriteLine((buff.EndTime - Game.Time) + "buff.EndTime - Game.Time > _minDuration.Value / 1000f" + _minDuration.Value / 1000f + " spell:" + buff.Type + " caster: " + buff.Caster.Name);

                    if (buff.Caster.Type == GameObjectType.obj_AI_Hero && ((Obj_AI_Hero)buff.Caster).ChampionName == "Alistar" && _noAliW) continue;

                    if (buff.EndTime - Game.Time > _minDuration.Value / 1000f)
                        Use(target);
                }

                if (_ignite && buff.Name == "summonerdot" && ObjectManager.Player.GetRemainingIgniteDamage() > ObjectManager.Player.Health)
                    Use(target);

                if (_exhaust && buff.Name == "summonerexhaust")
                    Use(target);
            }
        }

        public virtual void Use(Obj_AI_Base target)
        {
            if (ObjectManager.Player.Spellbook.Spells.Any(spell => spell.Name == "QuicksilverSash"))
                ObjectManager.Player.Spellbook.CastSpell(ObjectManager.Player.Spellbook.Spells.First(spell => spell.Name == "QuicksilverSash").Slot);
        }

        public int GetRange()
        {
            return 0;
        }

        public TargetSelector.DamageType GetDamageType()
        {
            return TargetSelector.DamageType.True;
        }
    }
}
