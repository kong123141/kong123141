namespace Pluging
{
    using Flowers_Utility.Common;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using SharpDX.Direct3D9;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    public class JungleTime
    {
        public static Menu Menu;
        public static readonly List<JungleCamp> _jungleCamps = new List<JungleCamp>();
        private static Font _mapFont;
        private static Font _miniMapFont;
        public static int _nextTime;

        public JungleTime(Menu mainMenu)
        {
            Menu = mainMenu;

            Menu JungleTimeMenu = new Menu("[FL] 打野计时", "JungleTime");
            JungleTimeMenu.AddItem(new MenuItem("Timer", "大地图显示").SetValue(true));
            JungleTimeMenu.AddItem(new MenuItem("Timer1", "小地图显示").SetValue(true));
            JungleTimeMenu.AddItem(new MenuItem("JungleTimerFormat", "格式:").SetValue(new StringList(new[] { "分:秒", "秒" })));

            Menu.AddSubMenu(JungleTimeMenu);

            AddJungleCamps();

            _mapFont = new Font(Drawing.Direct3DDevice, new System.Drawing.Font("Times New Roman", 20));
            _miniMapFont = new Font(Drawing.Direct3DDevice, new System.Drawing.Font("Times New Roman", 9));

            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += OnEndScene;
        }

        private void OnEndScene(EventArgs args)
        {
            try
            {
                if (!Menu.Item("Timer1").GetValue<bool>())
                    return;

                foreach (JungleCamp jungleCamp in _jungleCamps.Where(camp => camp.NextRespawnTime > 0))
                {
                    int timeClock = jungleCamp.NextRespawnTime - (int)Game.ClockTime;

                    string time = Menu.Item("JungleTimerFormat").GetValue<StringList>().SelectedIndex == 0 ? Helper.JungleFormatTime(timeClock) : timeClock.ToString(CultureInfo.InvariantCulture);

                    Vector2 pos = Drawing.WorldToMinimap(jungleCamp.Position);

                    if (timeClock >= 150)
                    {
                        Helper.JungleTimeText(_miniMapFont, time, (int)pos.X, (int)pos.Y - 8, Color.White);
                    }

                    if (timeClock < 150 && timeClock >= 100)
                    {
                        Helper.JungleTimeText(_miniMapFont, time, (int)pos.X, (int)pos.Y - 8, Color.LimeGreen);
                    }

                    if (timeClock < 100 && timeClock >= 50)
                    {
                        Helper.JungleTimeText(_miniMapFont, time, (int)pos.X, (int)pos.Y - 8, Color.Orange);
                    }

                    if (timeClock < 50)
                    {
                        Helper.JungleTimeText(_miniMapFont, time, (int)pos.X, (int)pos.Y - 8, Color.Red);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in JungleTime.OnEndScece + " + ex);
            }
        }

        private void OnDraw(EventArgs args)
        {
            try
            {
                if (!Menu.Item("Timer").GetValue<bool>())
                    return;

                foreach (JungleCamp jungleCamp in _jungleCamps.Where(camp => camp.NextRespawnTime > 0))
                {
                    int timeClock = jungleCamp.NextRespawnTime - (int)Game.ClockTime;

                    string time = Menu.Item("JungleTimerFormat").GetValue<StringList>().SelectedIndex == 0 ? Helper.JungleFormatTime(timeClock) : timeClock.ToString(CultureInfo.InvariantCulture);

                    Vector2 pos = Drawing.WorldToScreen(jungleCamp.Position);

                    if (timeClock >= 150)
                    {
                        Helper.JungleTimeText(_mapFont, time, (int)pos.X, (int)pos.Y - 8, Color.White);
                    }

                    if (timeClock < 150 && timeClock >= 100)
                    {
                        Helper.JungleTimeText(_mapFont, time, (int)pos.X, (int)pos.Y - 8, Color.LimeGreen);
                    }

                    if (timeClock < 100 && timeClock >= 50)
                    {
                        Helper.JungleTimeText(_mapFont, time, (int)pos.X, (int)pos.Y - 8, Color.Orange);
                    }

                    if (timeClock < 50)
                    {
                        Helper.JungleTimeText(_mapFont, time, (int)pos.X, (int)pos.Y - 8, Color.Red);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in JungleTime.OnDraw + " + ex);
            }
        }

        private void OnUpdate(EventArgs args)
        {
            try
            {
                if ((int)Game.ClockTime - _nextTime >= 0)
                {
                    _nextTime = (int)Game.ClockTime + 1;

                    IEnumerable<Obj_AI_Base> minions = ObjectManager.Get<Obj_AI_Base>().Where(minion => !minion.IsDead && minion.IsValid && minion.Name.ToUpper().StartsWith("SRU"));
                    IEnumerable<JungleCamp> junglesAlive = _jungleCamps.Where(jungle => !jungle.IsDead && jungle.Names.Any(s => minions.Where(minion => minion.Name == s).Select(minion => minion.Name).FirstOrDefault() != null));

                    foreach (JungleCamp jungle in junglesAlive)
                    {
                        jungle.Visibled = true;
                    }

                    IEnumerable<JungleCamp> junglesDead = _jungleCamps.Where(jungle => !jungle.IsDead && jungle.Visibled && jungle.Names.All(s => minions.Where(minion => minion.Name == s).Select(minion => minion.Name).FirstOrDefault() == null));

                    foreach (JungleCamp jungle in junglesDead)
                    {
                        jungle.IsDead = true;
                        jungle.Visibled = false;
                        jungle.NextRespawnTime = (int)Game.ClockTime + jungle.RespawnTime;
                    }

                    foreach (JungleCamp jungleCamp in _jungleCamps.Where(jungleCamp => (jungleCamp.NextRespawnTime - (int)Game.ClockTime) <= 0))
                    {
                        jungleCamp.IsDead = false;
                        jungleCamp.NextRespawnTime = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in JungleTime.OnUpdate + " + ex);
            }
        }

        private void AddJungleCamps()
        {
            try
            {
                _jungleCamps.Add(new JungleCamp("SRU_Blue", 300, new Vector3(3871.489f, 7901.054f, 51.90324f), new[] { "SRU_Blue1.1.1", "SRU_BlueMini1.1.2", "SRU_BlueMini21.1.3" }));
                _jungleCamps.Add(new JungleCamp("SRU_Murkwolf", 100, new Vector3(3780.628f, 6443.984f, 52.4632f), new[] { "SRU_Murkwolf2.1.1", "SRU_MurkwolfMini2.1.2", "SRU_MurkwolfMini2.1.3" }));
                _jungleCamps.Add(new JungleCamp("SRU_Razorbeak", 100, new Vector3(6823.895f, 5457.756f, 53.12784f), new[] { "SRU_Razorbeak3.1.1", "SRU_RazorbeakMini3.1.2", "SRU_RazorbeakMini3.1.3", "SRU_RazorbeakMini3.1.4" }));
                _jungleCamps.Add(new JungleCamp("SRU_Red", 300, new Vector3(7862f, 4112f, 53.71951f), new[] { "SRU_Red4.1.1", "SRU_RedMini4.1.2", "SRU_RedMini4.1.3" }));
                _jungleCamps.Add(new JungleCamp("SRU_Krug", 100, new Vector3(8532.471f, 2737.948f, 50.58278f), new[] { "SRU_Krug5.1.2", "SRU_KrugMini5.1.1" }));
                _jungleCamps.Add(new JungleCamp("SRU_Dragon", 360, new Vector3(9866.148f, 4414.014f, -71.2406f), new[] { "SRU_Dragon6.1.1" }));
                _jungleCamps.Add(new JungleCamp("SRU_Blue", 300, new Vector3(10931.73f, 6990.844f, 51.72291f), new[] { "SRU_Blue7.1.1", "SRU_BlueMini7.1.2", "SRU_BlueMini27.1.3" }));
                _jungleCamps.Add(new JungleCamp("SRU_Murkwolf", 100, new Vector3(11008f, 8386f, 62.13136f), new[] { "SRU_Murkwolf8.1.1", "SRU_MurkwolfMini8.1.2", "SRU_MurkwolfMini8.1.3" }));
                _jungleCamps.Add(new JungleCamp("SRU_Razorbeak", 100, new Vector3(7986.997f, 9471.389f, 52.34794f), new[] { "SRU_Razorbeak9.1.1", "SRU_RazorbeakMini9.1.2", "SRU_RazorbeakMini9.1.3", "SRU_RazorbeakMini9.1.4" }));
                _jungleCamps.Add(new JungleCamp("SRU_Red", 300, new Vector3(7016.869f, 10775.55f, 56.00922f), new[] { "SRU_Red10.1.1", "SRU_RedMini10.1.2", "SRU_RedMini10.1.3" }));
                _jungleCamps.Add(new JungleCamp("SRU_Krug", 100, new Vector3(6317.092f, 12146.46f, 56.4768f), new[] { "SRU_Krug11.1.2", "SRU_KrugMini11.1.1" }));
                _jungleCamps.Add(new JungleCamp("SRU_Baron", 420, new Vector3(5007.124f, 10471.45f, -71.2406f), new[] { "SRU_Baron12.1.1" }));
                _jungleCamps.Add(new JungleCamp("SRU_Gromp", 100, new Vector3(2090.628f, 8427.984f, 51.77725f), new[] { "SRU_Gromp13.1.1" }));
                _jungleCamps.Add(new JungleCamp("SRU_Gromp", 100, new Vector3(12702f, 6444f, 51.69143f), new[] { "SRU_Gromp14.1.1" }));
                _jungleCamps.Add(new JungleCamp("Sru_Crab", 180, new Vector3(10500f, 5170f, -62.8102f), new[] { "Sru_Crab15.1.1" }));
                _jungleCamps.Add(new JungleCamp("Sru_Crab", 180, new Vector3(4400f, 9600f, -66.53082f), new[] { "Sru_Crab16.1.1" }));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in JungleTime.AddJungleCamps + " + ex);
            }
        }

        public class JungleCamp
        {
            public bool IsDead;
            public string Name;
            public string[] Names;
            public int NextRespawnTime;
            public Vector3 Position;
            public int RespawnTime;
            public bool Visibled;

            public JungleCamp(string name, int respawnTime, Vector3 position, string[] names)
            {
                Name = name;
                RespawnTime = respawnTime;
                Position = position;
                Names = names;
                IsDead = false;
                Visibled = false;
            }
        }
    }
}