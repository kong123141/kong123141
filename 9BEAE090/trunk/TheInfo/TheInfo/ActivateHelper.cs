using System;
using LeagueSharp.Common;

namespace TheInfo
{
    class ActivateHelper
    {
        public bool IsSmart { get; private set; }
        private Menu _menu;

        public ActivateHelper(Menu menu, string subMenuName)
        {
            var newMenu = new Menu(subMenuName, subMenuName);
            menu.AddSubMenu(newMenu);
            Init(newMenu);
        }

        public ActivateHelper(Menu menu)
        {
            Init(menu);
        }

        private void Init(Menu menu)
        {
            var modeItem = new MenuItem("Mode", "Mode").SetValue(new StringList(new[] {"Smart or Toggle", "Smart", "Toggle", "Always"}));
            modeItem.ValueChanged += (sender, args) => { IsSmart = args.GetNewValue<StringList>().SelectedValue.Contains("Smart"); };
            menu.AddItem(modeItem);
            menu.AddItem(new MenuItem("Only toggle mode:", "Only toggle mode:"));
            menu.AddItem(new MenuItem("Toggle Key", "Toggle Key").SetValue(new KeyBind(78, KeyBindType.Toggle)));
            _menu = menu;
        }


        public bool GetActivated(Func<bool> smartFunction)
        {
            var drawMode = _menu.Item("Mode").GetValue<StringList>().SelectedValue;

            return (drawMode != "Smart" || smartFunction()) && (drawMode != "Smart or Toggle" || _menu.Item("Toggle Key").GetValue<KeyBind>().Active || smartFunction()) && (drawMode != "Toggle" || _menu.Item("Toggle Key").GetValue<KeyBind>().Active);
        }

        public string GetMode()
        {
            return _menu.Item("Mode").GetValue<StringList>().SelectedValue;
        }
    }
}
