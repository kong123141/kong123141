using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace TheInfo
{
    class ModuleTeamfightOverview : IModule
    {
        private const float SmartHeroRange = 2000f;

        private Menu _teamfight, _drawingTf;
        private ActivateHelper _activate;

        public void Initialize()
        {
            Drawing.OnDraw += Draw;
        }

        public void InitializeMenu(LeagueSharp.Common.Menu rootMenu)
        {
            _teamfight = new Menu("Teamfight Overview", "teamfightoverview");
            _drawingTf = new Menu("Drawing", "Drawing");

            _drawingTf.AddItem(new MenuItem("Bar Ally", "Bar Ally").SetValue(true));
            _drawingTf.AddItem(new MenuItem("Bar Enemy", "Bar Enemy").SetValue(true));

/*            _drawingTf.AddItem(new MenuItem("Draw Mode", "Draw Mode").SetValue(new StringList(new[] { "Smart or Toggle", "Smart", "Toggle", "Always" })));
            _drawingTf.AddItem(new MenuItem("Only toggle mode:", "Only toggle mode:"));
            _drawingTf.AddItem(new MenuItem("Toggle Key", "Toggle Key").SetValue(new KeyBind(78, KeyBindType.Toggle)));
            */

            _teamfight.AddItem(new MenuItem("Health mode", "Health mode").SetValue(new StringList(new[] { "Health", "Effective Health", "% Health" })));
            _teamfight.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));

            _activate = new ActivateHelper(_teamfight, "Activation");

            _teamfight.AddSubMenu(_drawingTf);
            rootMenu.AddSubMenu(_teamfight);

        }


        private void Draw(EventArgs args)
        {
            if (!_teamfight.Item("Enabled").GetValue<bool>())
                return;

            if (!_activate.GetActivated(() => HeroManager.AllHeroes.Count(hero => hero.Distance(ObjectManager.Player) < SmartHeroRange) > HeroManager.AllHeroes.Count / 2f))
                return;

/*            var drawMode = _drawingTf.Item("Draw Mode").GetValue<StringList>().SelectedValue;

            if (drawMode == "Toggle" && !_drawingTf.Item("Toggle Key").GetValue<KeyBind>().Active)
                return;
            if (drawMode == "Smart" && HeroManager.AllHeroes.Count(hero => hero.Distance(ObjectManager.Player) < SmartHeroRange) <= HeroManager.AllHeroes.Count / 2f)
                return;
            if (drawMode == "Smart or Toggle" && !_drawingTf.Item("Toggle Key").GetValue<KeyBind>().Active && HeroManager.AllHeroes.Count(hero => hero.Distance(ObjectManager.Player) < SmartHeroRange) < HeroManager.AllHeroes.Count / 2f)
                return;*/
            
            var modHealthAlly = 0f;
            var modHealthEnemy = 0f;

            switch (_teamfight.Item("Health mode").GetValue<StringList>().SelectedValue)
            {
                case "Health":
                    modHealthEnemy = HeroManager.Enemies.Sum(hero => hero.Health) / HeroManager.Enemies.Sum(hero => hero.MaxHealth);
                    modHealthAlly = HeroManager.Allies.Sum(hero => hero.Health) / HeroManager.Allies.Sum(hero => hero.MaxHealth);
                    break;
                case "Effective Health":
                    modHealthEnemy = HeroManager.Enemies.Sum(hero => hero.Health + hero.Health * ((hero.Armor + hero.SpellBlock) / 200f)) / HeroManager.Enemies.Sum(hero => hero.MaxHealth + hero.MaxHealth * ((hero.Armor + hero.SpellBlock) / 200f));
                    modHealthAlly = HeroManager.Allies.Sum(hero => hero.Health + hero.Health * ((hero.Armor + hero.SpellBlock) / 200f)) / HeroManager.Allies.Sum(hero => hero.MaxHealth + hero.MaxHealth * ((hero.Armor + hero.SpellBlock) / 200f));
                    break;
                case "% Health":
                    modHealthAlly = HeroManager.Allies.Sum(hero => (1 / (float)HeroManager.Allies.Count) * (hero.Health / hero.MaxHealth));
                    modHealthEnemy = HeroManager.Enemies.Sum(hero => (1 / (float)HeroManager.Allies.Count) * (hero.Health / hero.MaxHealth));
                    break;
            }


            if (_drawingTf.Item("Bar Ally").GetValue<bool>())
            {
                Drawing.DrawLine(Drawing.Width / 3f, 30, Drawing.Width / 3f * 2f - (Drawing.Width / 3f) * (1 - modHealthAlly), 30, 10, Color.Green);
             //   Console.WriteLine(modHealthAlly);
            }

            if (_drawingTf.Item("Bar Enemy").GetValue<bool>())
                Drawing.DrawLine(Drawing.Width / 3f, 45, Drawing.Width / 3f * 2f - (Drawing.Width / 3f) * (1 - modHealthEnemy), 45, 10, Color.Red);

        }
    }
}
