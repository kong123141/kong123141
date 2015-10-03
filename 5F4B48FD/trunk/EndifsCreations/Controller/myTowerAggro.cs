#region Todo    
        //      for obwalker
        //      On Hold, button up summonerspells
        //      new api
        //      minion networkid
        //      minion is turret aggro
            /*
             * http://leagueoflegends.wikia.com/wiki/Turret
             * Towers deal bonus damage to melee and caster minions so that melee minions generally die to 2 tower attack and 1 champion auto attack, 
             * and ranged minions die to 1 tower attack and 2 champion auto attacks. 
             */
        //      do update neaarest ally tower
        //      ObjectManager.Get<Obj_AI_Base>().OrderBy(p => p.Distance(ObjectManager.Player.ServerPosition));
        //      1095 vision range
        //      775 attack range
#endregion Todo

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCreations.Controller
{
    internal class myTowerAggro 
    {
        static myTowerAggro()
        {
            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnDoCast += OnDoCast;
            Drawing.OnDraw += OnDraw;
        }
        private static Menu Menu;

        public static int TurretTargetID;
        //public static int TurretID;
        //public static double TurretDamage;

        public static void AddToMenu(Menu menu)
        {
            Menu = menu;
            var subs = new Menu("Tower Aggro", "Tower Aggro");
            {
                subs.AddItem(new MenuItem("EC.TA.Indicator", "Enable Indicator").SetValue(false));                
            }
            menu.AddSubMenu(subs);
        }

        private static void OnUpdate(EventArgs args)
        {
        }

        private static void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsAlly &&
                sender is Obj_AI_Turret &&
                args.Target is Obj_AI_Minion && args.SData.Name.Contains("BasicAttack") &&
                Vector3.Distance(ObjectManager.Player.ServerPosition, sender.ServerPosition) <= 1095)
            {                
                //myDevTools.DebugMode("It's a turret hitting minion");
                //myDevTools.DebugMode("args.Target.Name: " + args.Target.Name);
                //myDevTools.DebugMode("args.Target.NetworkId: " + args.Target.NetworkId);
                TurretTargetID = args.Target.NetworkId;
                //TurretID = sender.NetworkId;
                //var dmg = sender.GetSpellDamage((Obj_AI_Minion)args.Target, args.SData.Name);
                //myDevTools.DebugMode("dmg: " + dmg);
                //myDevTools.DebugMode("sender.AttackDelay: " + sender.AttackDelay);
                //TurretDamage = dmg;
            }
        }
        private static void OnDraw(EventArgs args)
        {
            //test indicator
            if (Menu.Item("EC.TA.Indicator").GetValue<bool>())
            {
                
                var minionList = MinionManager.GetMinions(ObjectManager.Player.Position, 1095);
                foreach (var minion in minionList.Where(minion => minion.IsValidTarget(1095)))
                {
                    if (minion.NetworkId == TurretTargetID)
                    {
                        Drawing.DrawText(minion.HPBarPosition.X + 70, minion.HPBarPosition.Y, Color.Cyan, "X");
                    }
                }
            }
        }
    }
}