﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ExoCondemn
{
    class Program
    {
        private static SpellSlot FlashSlot;
        private static Spell Condemn;
        private static Menu AssemblyMenu;
        private static Vector3 MyPosition2;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Vayne")
            {
                return;
            }
            OnLoad();
        }

        private static void OnLoad()
        {
            AssemblyMenu = new Menu("ExoCondemn - Flash E","dz191.exocondemn", true);

            AssemblyMenu.AddItem(
                new MenuItem("dz191.exocondemn.pushdistance", "Push Distance").SetValue(
                    new Slider(400, 370, 465)));

            AssemblyMenu.AddItem(
                new MenuItem("dz191.exocondemn.execute", "Do Flash Condemn!").SetValue(
                    new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));

            AssemblyMenu.AddItem(
                new MenuItem("dz191.exocondemn.onlyone", "Only for 1v1").SetValue(true));

            AssemblyMenu.AddToMainMenu();
            

            Condemn = new Spell(SpellSlot.E, 590f);
            LoadFlash();
            Condemn.SetTargetted(0.25f, 2000f);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += OnDraw;
        }

        private static void OnDraw(EventArgs args)
        {
            if (MyPosition2 != Vector3.Zero)
            {
                Render.Circle.DrawCircle(MyPosition2, 65, System.Drawing.Color.Red);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {

            var cPos = Game.CursorPos;
            for (int i = 65; i < 425; i += (int)ObjectManager.Player.BoundingRadius)
            {
                var extended = ObjectManager.Player.ServerPosition.Extend(cPos, i);
                var possibleUnit = CondemnCheck(extended);
                if (possibleUnit != null && !extended.UnderTurret(true) && !extended.IsWall() && (extended.Distance(ObjectManager.Player.ServerPosition, true) > ObjectManager.Player.ServerPosition.Distance(possibleUnit.ServerPosition, true)))
                {
                    MyPosition2 = extended;
                }
            }

            if (AssemblyMenu.Item("dz191.exocondemn.execute").GetValue<KeyBind>().Active && (Condemn.IsReady() && ObjectManager.Player.GetSpell(FlashSlot).State == SpellState.Ready))
            {
                if (CondemnCheck(ObjectManager.Player.ServerPosition) != null)
                {
                    return;
                }
                if (AssemblyMenu.Item("dz191.exocondemn.onlyone").GetValue<bool>() &&
                    ObjectManager.Player.CountEnemiesInRange(1200f) > 1)
                {
                    return;
                }

                var myPosition = Vector3.Zero;
                Obj_AI_Hero myUnit = null;

                var cursorPos = Game.CursorPos;
                for (int i = 65; i < 425; i += (int) ObjectManager.Player.BoundingRadius)
                {
                    var extended = ObjectManager.Player.ServerPosition.Extend(cursorPos, i);
                    var possibleUnit = CondemnCheck(extended);
                    if (possibleUnit != null && !extended.UnderTurret(true) && !extended.IsWall() && (extended.Distance(ObjectManager.Player.ServerPosition, true) > ObjectManager.Player.ServerPosition.Distance(possibleUnit.ServerPosition, true)))
                    {
                        myPosition = extended;
                        myUnit = possibleUnit;
                    }
                }

                if (myPosition != Vector3.Zero)
                {
                    Condemn.CastOnUnit(myUnit);

                    Utility.DelayAction.Add((int)(250 + Game.Ping/2f +25), () =>
                    {
                        ObjectManager.Player.Spellbook.CastSpell(FlashSlot, myPosition);
                    });
                }
            }
        }

        private static Obj_AI_Hero CondemnCheck(Vector3 fromPosition)
        {
            var HeroList = HeroManager.Enemies.Where(
                                    h =>
                                        h.IsValidTarget(Condemn.Range) &&
                                        !h.HasBuffOfType(BuffType.SpellShield) &&
                                        !h.HasBuffOfType(BuffType.SpellImmunity));
            foreach (var Hero in HeroList)
            {
                var ePred = Condemn.GetPrediction(Hero);
                int pushDist = AssemblyMenu.Item("dz191.exocondemn.pushdistance").GetValue<Slider>().Value;
                for (int i = 0; i < pushDist; i += (int)Hero.BoundingRadius)
                {
                    Vector3 loc3 = ePred.UnitPosition.To2D().Extend(fromPosition.To2D(), -i).To3D();
                    if (loc3.IsWall())
                    {
                        return Hero;
                    }
                }
            }
            return null;
        }

        private static void LoadFlash()
        {
            var testSlot = ObjectManager.Player.GetSpellSlot("summonerflash");
            if (testSlot != SpellSlot.Unknown)
            {
                Console.WriteLine("Flash Slot: {0}", testSlot);
                FlashSlot = testSlot;
            }
            else
            {
                Console.WriteLine("Error loading Flash! Not found!");
            }
        }
    }
}
