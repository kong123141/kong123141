using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using LeagueSharp;
using LeagueSharp.Common;
using SPrediction;
using SharpDX;
using Color = System.Drawing.Color;

namespace AimSharp
{
    class Program
    {
        public static List<Spell> SpellList = new List<Spell>();
        private static Obj_AI_Hero Player = ObjectManager.Player;
        public static Menu Config;

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        static void Main(string[] args)
        {

            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            foreach (var spell in SpellDatabase.Spells)
            {
                if (spell.ChampionName == Player.CharData.BaseSkinName)
                {
                    if (spell.Slot == SpellSlot.Q)
                    {
                        Q = new Spell(spell.Slot, spell.Range);
                        Q.SetSkillshot(spell.Delay / 1000, spell.Radius, spell.MissileSpeed, spell.CanBeRemoved, spell.Type);
                        SpellList.Add(Q);
                    }
                    if (spell.Slot == SpellSlot.W)
                    {
                        W = new Spell(spell.Slot, spell.Range);
                        W.SetSkillshot(spell.Delay / 1000, spell.Radius, spell.MissileSpeed, spell.CanBeRemoved, spell.Type);
                        SpellList.Add(W);
                    }
                    if (spell.Slot == SpellSlot.E)
                    {
                        E = new Spell(spell.Slot, spell.Range);
                        E.SetSkillshot(spell.Delay / 1000, spell.Radius, spell.MissileSpeed, spell.CanBeRemoved, spell.Type);
                        SpellList.Add(E);
                    }
                    if (spell.Slot == SpellSlot.R)
                    {
                        R = new Spell(spell.Slot, spell.Range);
                        R.SetSkillshot(spell.Delay / 1000, spell.Radius, spell.MissileSpeed, spell.CanBeRemoved, spell.Type);
                        SpellList.Add(R);
                    }
                }

            }

            Config = new Menu("Aim#", "aimSharp", true);

            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.AddItem(new MenuItem("qCombo", "Use Q").SetValue(new KeyBind('U', KeyBindType.Press)));
                comboMenu.AddItem(new MenuItem("wCombo", "Use W").SetValue(new KeyBind('I', KeyBindType.Press)));
                comboMenu.AddItem(new MenuItem("eCombo", "Use E").SetValue(new KeyBind('O', KeyBindType.Press)));
                comboMenu.AddItem(new MenuItem("rCombo", "Use R").SetValue(new KeyBind('P', KeyBindType.Press)));
                Config.AddSubMenu(comboMenu);
            }
            var drawMenu = new Menu("Draw Settings", "Draw Settings");
            {
                drawMenu.AddItem(new MenuItem("qDraw", "Q Range").SetValue(new Circle(true, Color.White)));
                drawMenu.AddItem(new MenuItem("wDraw", "W Range").SetValue(new Circle(true, Color.Silver)));
                drawMenu.AddItem(new MenuItem("eDraw", "E Range").SetValue(new Circle(true, Color.Yellow)));
                drawMenu.AddItem(new MenuItem("rDraw", "R Range").SetValue(new Circle(true, Color.Gold)));
                Config.AddSubMenu(drawMenu);
            }

            Config.AddItem(new MenuItem("CRHITCHANCE", "Hit Chance").SetValue<StringList>(new StringList(Utility.HitchanceNameArray, 2)));
            Config.AddToMainMenu();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            SPrediction.Prediction.Initialize(Config);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem1 = Config.Item("qDraw").GetValue<Circle>();
            var menuItem2 = Config.Item("wDraw").GetValue<Circle>();
            var menuItem3 = Config.Item("eDraw").GetValue<Circle>();
            var menuItem4 = Config.Item("rDraw").GetValue<Circle>();

            if (menuItem1.Active && Q.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z), Q.Range, menuItem1.Color, 5);
            }
            if (menuItem2.Active && W.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z), W.Range, menuItem2.Color, 5);
            }
            if (menuItem3.Active && E.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z), E.Range, menuItem3.Color, 5);
            }
            if (menuItem4.Active && R.IsReady())
            {
                Render.Circle.DrawCircle(new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z), R.Range, menuItem4.Color, 5);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Config.Item("qCombo").GetValue<KeyBind>().Active)
            {
                comboQ();
            }
            if (Config.Item("wCombo").GetValue<KeyBind>().Active)
            {
                comboW();
            }
            if (Config.Item("eCombo").GetValue<KeyBind>().Active)
            {
                comboE();
            }
            if (Config.Item("rCombo").GetValue<KeyBind>().Active)
            {
                comboR();
            }
        }
        private static void comboQ()
        {
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && x.IsValidTarget(Q.Range)))
            {

                HitChance hCance = Utility.HitchanceArray[Config.Item("CRHITCHANCE").GetValue<StringList>().SelectedIndex];
                Q.SPredictionCast(enemy,hCance);
            }
        }
        private static void comboW()
        {
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && x.IsValidTarget(W.Range)))
            {
                HitChance hCance = Utility.HitchanceArray[Config.Item("CRHITCHANCE").GetValue<StringList>().SelectedIndex];
                W.SPredictionCast(enemy, hCance);
            }
        }
        private static void comboE()
        {
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && x.IsValidTarget(E.Range)))
            {
                HitChance hCance = Utility.HitchanceArray[Config.Item("CRHITCHANCE").GetValue<StringList>().SelectedIndex];
                E.SPredictionCast(enemy, hCance);
            }
        }
        private static void comboR()
        {
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && x.IsValidTarget(R.Range)))
            {
                HitChance hCance = Utility.HitchanceArray[Config.Item("CRHITCHANCE").GetValue<StringList>().SelectedIndex];
                R.SPredictionCast(enemy, hCance);
            }
        }
    }
}
