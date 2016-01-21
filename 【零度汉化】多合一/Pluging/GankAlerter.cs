namespace Pluging
{
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class PreviewCircle
    {
        private const int Delay = 2;
        private float _lastChanged;
        private readonly Render.Circle _mapCircle;
        private int _radius;

        public PreviewCircle()
        {
            Drawing.OnEndScene += Drawing_OnEndScene;

            _mapCircle = new Render.Circle(ObjectManager.Player, 0, System.Drawing.Color.Red, 5);
            _mapCircle.Add(0);
            _mapCircle.VisibleCondition = sender => _lastChanged > 0 && Game.ClockTime - _lastChanged < Delay;
        }

        private void Drawing_OnEndScene(EventArgs args)
        {
            if (_lastChanged > 0 && Game.ClockTime - _lastChanged < Delay)
            {
                Utility.DrawCircle(ObjectManager.Player.Position, _radius, System.Drawing.Color.Red, 2, 30, true);
            }
        }

        public void SetRadius(int radius)
        {
            _radius = radius;
            _mapCircle.Radius = radius;
            _lastChanged = Game.ClockTime;
        }
    }

    internal class ChampionInfo
    {
        private static int index = 0;
        private readonly Obj_AI_Hero _hero;
        private event EventHandler OnEnterRange;
        private bool _visible;
        private float _distance;
        private float _lastEnter;
        private float _lineStart;
        private readonly Render.Line _line;

        public ChampionInfo(Obj_AI_Hero hero, bool ally)
        {
            index++;

            int textoffset = index * 50;

            _hero = hero;

            Render.Text text = new Render.Text(new Vector2(), _hero.ChampionName, 20, ally ? new Color { R = 205, G = 255, B = 205, A = 255 } : new Color { R = 255, G = 205, B = 205, A = 255 })
            {
                PositionUpdate = () => Drawing.WorldToScreen(ObjectManager.Player.Position.Extend(_hero.Position, 300 + textoffset)),

                VisibleCondition = delegate
                {
                    float dist = _hero.Distance(ObjectManager.Player.Position);
                    return GankAlerter.ShowChampionNames && !_hero.IsDead &&
                           Game.ClockTime - _lineStart < GankAlerter.LineDuration &&
                           (!_hero.IsVisible || !Render.OnScreen(Drawing.WorldToScreen(_hero.Position))) &&
                           dist < GankAlerter.Radius && dist > 300 + textoffset;
                },

                Centered = true,
                OutLined = true,
            };

            text.Add(1);

            _line = new Render.Line(new Vector2(), new Vector2(), 5, ally ? new Color { R = 0, G = 255, B = 0, A = 125 } : new Color { R = 255, G = 0, B = 0, A = 125 })
            {
                StartPositionUpdate = () => Drawing.WorldToScreen(ObjectManager.Player.Position),

                EndPositionUpdate = () => Drawing.WorldToScreen(_hero.Position),

                VisibleCondition =
                    delegate
                    {
                        return !_hero.IsDead && Game.ClockTime - _lineStart < GankAlerter.LineDuration &&
                               _hero.Distance(ObjectManager.Player.Position) < (GankAlerter.Radius + 1000);
                    }
            };

            _line.Add(0);

            Render.Line minimapLine = new Render.Line(new Vector2(), new Vector2(), 2, ally ? new Color { R = 0, G = 255, B = 0, A = 255 } : new Color { R = 255, G = 0, B = 0, A = 255 })
            {
                StartPositionUpdate = () => Drawing.WorldToMinimap(ObjectManager.Player.Position),

                EndPositionUpdate = () => Drawing.WorldToMinimap(_hero.Position),

                VisibleCondition =
                    delegate
                    {
                        return GankAlerter.DrawMinimapLines && !_hero.IsDead && Game.ClockTime - _lineStart < GankAlerter.LineDuration;
                    }
            };

            minimapLine.Add(0);

            Game.OnUpdate += Game_OnGameUpdate;
            OnEnterRange += ChampionInfo_OnEnterRange;
        }

        private void ChampionInfo_OnEnterRange(object sender, EventArgs e)
        {
            bool enabled = false;

            if (GankAlerter.EnemyJunglerOnly && _hero.IsEnemy)
            {
                enabled = IsJungler(_hero);
            }
            else if (GankAlerter.AllyJunglerOnly && _hero.IsAlly)
            {
                enabled = IsJungler(_hero);
            }
            else
            {
                enabled = GankAlerter.IsEnabled(_hero);
            }

            if (Game.ClockTime - _lastEnter > GankAlerter.Cooldown && enabled)
            {
                _lineStart = Game.ClockTime;

                if (GankAlerter.DangerPing && _hero.IsEnemy && !_hero.IsDead)
                {
                    Game.ShowPing(PingCategory.Danger, _hero, true);
                }
            }

            _lastEnter = Game.ClockTime;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            float newDistance = _hero.Distance(ObjectManager.Player);

            if (Game.ClockTime - _lineStart < GankAlerter.LineDuration)
            {
                float percentage = newDistance / GankAlerter.Radius;

                if (percentage <= 1)
                {
                    _line.Width = (int)(2 + (percentage * 8));
                }
            }

            if (newDistance < GankAlerter.Radius && _hero.IsVisible)
            {
                if (_distance >= GankAlerter.Radius || !_visible)
                {
                    if (OnEnterRange != null)
                    {
                        OnEnterRange(this, null);
                    }
                }
                else if (_distance < GankAlerter.Radius && _visible)
                {
                    _lastEnter = Game.ClockTime;
                }
            }

            _distance = newDistance;
            _visible = _hero.IsVisible;
        }

        private bool IsJungler(Obj_AI_Hero hero)
        {
            return hero.Spellbook.Spells.Any(spell => spell.Name.ToLower().Contains("smite"));
        }
    }

    public class GankAlerter
    {
        public static Menu Menu;
        public static MenuItem _sliderRadius;
        private static PreviewCircle _previewCircle;
        public static MenuItem _sliderCooldown;
        public static MenuItem _sliderLineDuration;
        public static MenuItem _enemyJunglerOnly;
        public static MenuItem _allyJunglerOnly;
        public static MenuItem _showChampionNames;
        public static MenuItem _drawMinimapLines;
        public static MenuItem _dangerPing;
        public static Menu _enemies;
        public static Menu _allies;
        private static IDictionary<int, ChampionInfo> _championInfoById = new Dictionary<int, ChampionInfo>();

        public GankAlerter(Menu mainMenu)
        {
            Menu = mainMenu;

            Menu GankAlerterMenu = new Menu("[FL] Gank提示", "GankAlerter");

            _sliderRadius = new MenuItem("range", "触发范围").SetValue(new Slider(3000, 500, 5000));
            _sliderRadius.ValueChanged += SliderRadiusValueChanged;

            _sliderCooldown = new MenuItem("cooldown", "触发时间间隔(s)").SetValue(new Slider(10, 0, 60));

            _sliderLineDuration = new MenuItem("lineduration", "线条持续时间(s)").SetValue(new Slider(10, 0, 20));

            _enemyJunglerOnly = new MenuItem("jungleronly", "仅提示有惩戒的敌人接近").SetValue(false);

            _allyJunglerOnly = new MenuItem("allyjungleronly", "只提示有惩戒的友军接近").SetValue(true);

            _showChampionNames = new MenuItem("shownames", "显示英雄名字").SetValue(true);

            _drawMinimapLines = new MenuItem("drawminimaplines", "小地图显示线条").SetValue(false);

            _dangerPing = new MenuItem("dangerping", "危险提示 (本地)").SetValue(false);

            _enemies = new Menu("敌人列表", "enemies");
            _enemies.AddItem(_enemyJunglerOnly);

            _allies = new Menu("友军列表", "allies");
            _allies.AddItem(_allyJunglerOnly);

            GankAlerterMenu.AddItem(_sliderRadius);
            GankAlerterMenu.AddItem(_sliderCooldown);
            GankAlerterMenu.AddItem(_sliderLineDuration);
            GankAlerterMenu.AddItem(_showChampionNames);
            GankAlerterMenu.AddItem(_drawMinimapLines);
            GankAlerterMenu.AddItem(_dangerPing);
            GankAlerterMenu.AddSubMenu(_enemies);
            GankAlerterMenu.AddSubMenu(_allies);

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.NetworkId != ObjectManager.Player.NetworkId)
                {
                    if (hero.IsEnemy)
                    {
                        _championInfoById[hero.NetworkId] = new ChampionInfo(hero, false);

                        _enemies.AddItem(new MenuItem("enemy" + hero.ChampionName, hero.ChampionName).SetValue(true));
                    }
                    else
                    {
                        _championInfoById[hero.NetworkId] = new ChampionInfo(hero, true);

                        _allies.AddItem(new MenuItem("ally" + hero.ChampionName, hero.ChampionName).SetValue(false));
                    }
                }
            }

            Menu.AddSubMenu(GankAlerterMenu);
        }

        public static bool IsEnabled(Obj_AI_Hero hero)
        {
            return hero.IsEnemy ? _enemies.Item("enemy" + hero.ChampionName).GetValue<bool>() : _allies.Item("ally" + hero.ChampionName).GetValue<bool>();
        }

        private static void SliderRadiusValueChanged(object sender, OnValueChangeEventArgs e)
        {
            _previewCircle.SetRadius(e.GetNewValue<Slider>().Value);
        }

        public static int Radius
        {
            get { return _sliderRadius.GetValue<Slider>().Value; }
        }

        public static int Cooldown
        {
            get { return _sliderCooldown.GetValue<Slider>().Value; }
        }

        public static bool DangerPing
        {
            get { return _dangerPing.GetValue<bool>(); }
        }

        public static int LineDuration
        {
            get { return _sliderLineDuration.GetValue<Slider>().Value; }
        }

        public static bool EnemyJunglerOnly
        {
            get { return _enemyJunglerOnly.GetValue<bool>(); }
        }

        public static bool AllyJunglerOnly
        {
            get { return _allyJunglerOnly.GetValue<bool>(); }
        }

        public static bool ShowChampionNames
        {
            get { return _showChampionNames.GetValue<bool>(); }
        }

        public static bool DrawMinimapLines
        {
            get { return _drawMinimapLines.GetValue<bool>(); }
        }
    }
}