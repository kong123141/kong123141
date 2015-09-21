using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Muramana
{
    class Program
    {
        public static int Muramana = 3042;
        private static Menu Menu;
        private static bool hasAttacked;
        private static float distance = 0f;
        private static Obj_AI_Hero target1;
        private static Dictionary<Obj_SpellMissile,Obj_AI_Hero> objList;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("Muramana Activator", "MMAct", true);
            Menu.AddItem(new MenuItem("useM", "Use Muramana Activator").SetValue(true));
            Game.PrintChat("Muramana Activator By DZ191 Loaded.");
            Orbwalking.OnAttack += OrbwalkingOnAtk;
            GameObject.OnCreate += Obj_SpellMissile_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }

        private static void OrbwalkingOnAtk(AttackableUnit unit, AttackableUnit target)
        {
            int Mur = Items.HasItem(Muramana) ? 3042 : 3043;
            if (ObjectManager.Get<Obj_AI_Hero>().Contains(target) && (Items.HasItem(Mur)) && (Menu.Item("useM").GetValue<bool>()) && (Items.CanUseItem(Mur)))
            {
                Items.UseItem(Mur);
                target1 = (Obj_AI_Hero)target;
            }
        }

        static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
 	        if (sender is Obj_SpellMissile && sender.IsValid)
            {
                var missile = (Obj_SpellMissile) sender;
                if(objList.ContainsKey(missile))
                {
                    
                    int Mur = Items.HasItem(Muramana) ? 3042 : 3043;
                    if (target1.IsValid && ObjectManager.Get<Obj_AI_Hero>().Contains(objList[missile]) && (Items.HasItem(Mur)) && (Items.CanUseItem(Mur)) && (Menu.Item("useM").GetValue<bool>()))
                    {
                        Items.UseItem(Mur);
                    }
                    objList.Remove(missile);
                }
            }
        }

        private static void Obj_SpellMissile_OnCreate(GameObject sender, EventArgs args)
        {
 	         if (sender is Obj_SpellMissile && sender.IsValid)
            {
                var missile = (Obj_SpellMissile) sender;
                if (missile.SpellCaster is Obj_AI_Hero && missile.SpellCaster.IsMe && missile.SpellCaster.IsValid &&
                    Orbwalking.IsAutoAttack(missile.SData.Name))
                {
                    objList.Add(missile, target1);
                    target1 = null;
                }
            }
        } 
    }
}
