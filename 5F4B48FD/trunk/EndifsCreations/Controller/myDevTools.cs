#region Notes
    //      Merged with Developer#  https://github.com/myo/Experimental/
#endregion Notes

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace EndifsCreations.Controller
{
    internal static class myDevTools
    {
        static myDevTools()
        {
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnBuffAdd += OnBuffAdd;
            Obj_AI_Base.OnBuffRemove += OnBuffRemove;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        } 

        private static Menu Menu;
        private static bool Once;

        private static List<Obj_AI_Hero> ChampionsList = new List<Obj_AI_Hero>();
        private static List<Obj_AI_Hero> ChampionsNearMouse = new List<Obj_AI_Hero>();

        private static List<GameObject> GameObjectsList = new List<GameObject>();
        private static List<GameObject> GameObjectsNearMouse = new List<GameObject>();

        private static int LastOrder;

        public static void AddToMenu(Menu menu)
        {
            Menu = menu;
            var subs = new Menu("Dev Tools", "DevTools");
            {
                var Blacklist = new Menu("Blacklist", "Blacklist");
                {
                    foreach (var ally in HeroManager.Allies.Where(x => !x.IsMe))
                    {
                        Blacklist.SubMenu("Allies").AddItem(new MenuItem("EC.DevTools.Ignore." + ally.NetworkId, ally.CharData.BaseSkinName).SetValue(false));
                    }
                    foreach (var enemy in HeroManager.Enemies)
                    {
                        Blacklist.SubMenu("Enemy").AddItem(new MenuItem("EC.DevTools.Ignore." + enemy.NetworkId, enemy.CharData.BaseSkinName).SetValue(false));
                    }                    
                }
                subs.AddSubMenu(Blacklist);

                var mouseover = new Menu("Mouse Over", "MouseOver");
                {
                    mouseover.AddItem(new MenuItem("EC.DevTools.ObjectCheck", "Object Check").SetValue(false));
                    mouseover.AddItem(new MenuItem("EC.DevTools.BuffCheck", "Buff Check").SetValue(false));
                    mouseover.AddItem(new MenuItem("EC.DevTools.SpellsCheck", "Spells Check").SetValue(false));
                }
                subs.AddSubMenu(mouseover);
                var capture = new Menu("Capture to Console", "Capture");
                {
                    capture.AddItem(new MenuItem("EC.DevTools.DebugMode", "Debug Mode").SetValue(false));
                    capture.AddItem(new MenuItem("EC.DevTools.SpellsProcessCheck", "Spells Process (Self)").SetValue(false));
                    capture.AddItem(new MenuItem("EC.DevTools.OthersSpellsProcessCheck", "Spells Process (Others)").SetValue(false));
                    capture.AddItem(new MenuItem("EC.DevTools.SelfBuffAddBuffRemove", "Buff Add/Remove (Self)").SetValue(false));
                    capture.AddItem(new MenuItem("EC.DevTools.OthersBuffAddBuffRemove", "Buff Add/Remove (Others)").SetValue(false));
                }
                subs.AddSubMenu(capture);
            }
            menu.AddSubMenu(subs);
        }

        public static void DebugMode(String text)
        {
            if (Menu.Item("EC.DevTools.DebugMode").GetValue<bool>())
            {
                Console.WriteLine("<Debug> " + text);
            }
        }

        private static void WriteBuffs(List<Obj_AI_Hero> list)
        {
            if (Once) return;
            foreach (var t in list)
            {
                if (!t.IsValid) return;
                var X = Drawing.WorldToScreen(t.Position).X;
                var Y = Drawing.WorldToScreen(t.Position).Y;
                Drawing.DrawText(X, Y, Color.Cyan, t.CharData.BaseSkinName);
                if (t.Buffs.Any())
                {
                    for (var i = 0; i < t.Buffs.Count(); i += 1)
                    {
                        Console.WriteLine("-----" + t.CharData.BaseSkinName + "-----");
                        Console.WriteLine("Name: " + t.Buffs[i].Name);
                        Console.WriteLine("DisplayName: " + t.Buffs[i].DisplayName);
                        Console.WriteLine("Count: " + t.Buffs[i].Count);
                        Console.WriteLine("Caster: " + t.Buffs[i].Caster);
                        Console.WriteLine("------------------------------");
                    }
                }
            }
            Once = true;
        }

        private static void SpellsCheck()
        {
            if (Once) return;
            foreach (var x in ObjectManager.Player.Spellbook.Spells)
            {
                var s = x.SData;
                Console.WriteLine("SpellSlot: " + x.Slot + " || TargettingType: " + s.TargettingType);
                Console.WriteLine("Range: " + s.CastRange + " || Width: " + s.LineWidth + " || Radius: " + s.CastRadius);
                Console.WriteLine("CastTime: " + s.SpellCastTime + " || Delay: " + s.DelayCastOffsetPercent + " || Missile Speed: " + s.MissileSpeed);
                Console.WriteLine("------------------------------");
            }
            Once = true;
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Menu.Item("EC.DevTools.BuffCheck").GetValue<bool>())
            {
                if (myUtility.TickCount - LastOrder > 1000)
                {
                    ChampionsList = ObjectManager.Get<Obj_AI_Hero>().ToList();
                    ChampionsNearMouse = ChampionsList.Where(x => x.Position.Distance(Game.CursorPos) < 150).ToList();
                    LastOrder = myUtility.TickCount;
                }
            }
            if (Menu.Item("EC.DevTools.SpellsCheck").GetValue<bool>())
            {
                SpellsCheck();
            }
            else if (!Menu.Item("EC.DevTools.SpellsCheck").GetValue<bool>() && Once)
            {
                Once = false;
            }
            if (Menu.Item("EC.DevTools.ObjectCheck").GetValue<bool>())
            {
                if (myUtility.TickCount - LastOrder > 1000)
                {
                    GameObjectsList = ObjectManager.Get<GameObject>().ToList();
                    GameObjectsNearMouse = GameObjectsList.Where(
                        o => 
                            o.Position.Distance(Game.CursorPos) < 150 &&
                            o.Name != "missile" && 
                            !(o is Obj_Turret) &&
                            !(o is Obj_AI_Hero) && 
                            !(o is Obj_LampBulb) && 
                            !(o is Obj_SpellMissile) && 
                            !(o is GrassObject) && 
                            !(o is DrawFX) && 
                            !(o is LevelPropSpawnerPoint) && 
                            !(o is Obj_GeneralParticleEmitter) && 
                            !o.Name.Contains("MoveTo")).ToList();
                    LastOrder = myUtility.TickCount;
                }
            }
        }
        private static void OnDraw(EventArgs args)
        {
            if (Menu.Item("EC.DevTools.BuffCheck").GetValue<bool>())
            {
                foreach (var t in ChampionsNearMouse.Where(x => !Menu.Item("EC.DevTools.Ignore." + x.CharData.BaseSkinName).GetValue<bool>()))
                {
                    if (!t.IsValid) return;
                    var X = Drawing.WorldToScreen(t.Position).X;
                    var Y = Drawing.WorldToScreen(t.Position).Y;
                    Drawing.DrawText(X, Y, Color.Cyan, t.CharData.BaseSkinName);
                    if (t.Buffs.Any())
                    {
                        for (var i = 0; i < t.Buffs.Count() * 10; i += 11)
                        {
                            Drawing.DrawText(X, (Y + 10 + i), Color.Cyan, "N: " + t.Buffs[i / 11].Name + " C: " + t.Buffs[i / 11].Count);
                        }
                    }
                }
            }
            if (Menu.Item("EC.DevTools.ObjectCheck").GetValue<bool>())
            {
                foreach (var obj in GameObjectsNearMouse)
                {
                    if (!obj.IsValid) return;
                    var X = Drawing.WorldToScreen(obj.Position).X;
                    var Y = Drawing.WorldToScreen(obj.Position).Y;
                    Drawing.DrawText(X, Y, Color.Lime, obj.Name);
                    Drawing.DrawText(X, Y + 10 + 1, Color.Lime, obj.Type.ToString());
                    Drawing.DrawText(X, Y + 20 + 2, Color.Lime, "NetworkID: " + obj.NetworkId);
                    Drawing.DrawText(X, Y + 30 + 3, Color.Lime, obj.Position.ToString());
                }
            }
        }
        private static void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (Menu.Item("EC.DevTools.SpellsProcessCheck").GetValue<bool>())
            {
                if (unit.IsMe && !spell.SData.IsAutoAttack())
                {
                    Console.WriteLine("------------------------------");
                    Console.WriteLine("TargettingType: " + spell.SData.TargettingType.ToString());
                    Console.WriteLine("SData.Name: " + spell.SData.Name);
                    Console.WriteLine("SData.CastRange: " + spell.SData.CastRange);
                    Console.WriteLine("SData.SpellCastTime: " + spell.SData.SpellCastTime);
                    Console.WriteLine("SData.DelayCastOffsetPercent: " + spell.SData.DelayCastOffsetPercent);
                    Console.WriteLine("SData.MissileSpeed: " + spell.SData.MissileSpeed);
                    Console.WriteLine("SData.MissileAccel: " + spell.SData.MissileAccel);
                    Console.WriteLine("SData.LineWidth: " + spell.SData.LineWidth);
                    Console.WriteLine("SData.CastRadius: " + spell.SData.CastRadius);
                    if (spell.Target != null)
                    {
                        Console.WriteLine("spell.Target: " + spell.Target.Name);
                    }
                    
                }
            }
            if (Menu.Item("EC.DevTools.OthersSpellsProcessCheck").GetValue<bool>())
            {
                if (unit is Obj_AI_Hero && !unit.IsMe && !spell.SData.IsAutoAttack() && !Menu.Item("EC.DevTools.Ignore." + unit.NetworkId).GetValue<bool>())
                {                   
                    Console.WriteLine("------------------------------");
                    Console.WriteLine("Caster: " + unit.CharData.BaseSkinName);
                    Console.WriteLine("SData.Name: " + spell.SData.Name);
                    
                    Console.WriteLine("SData.CastRange: " + spell.SData.CastRange);
                    Console.WriteLine("SData.SpellCastTime: " + spell.SData.SpellCastTime);
                    Console.WriteLine("SData.DelayCastOffsetPercent: " + spell.SData.DelayCastOffsetPercent);
                    Console.WriteLine("SData.MissileSpeed: " + spell.SData.MissileSpeed);
                    Console.WriteLine("SData.MissileAccel: " + spell.SData.MissileAccel);
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location))
                    {
                        Console.WriteLine("Type.Location");
                        if (spell.SData.LineWidth > 0)
                        {
                            Console.WriteLine("SData.LineWidth: " + spell.SData.LineWidth);
                        }
                    }
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Location2))
                    {
                        Console.WriteLine("Type.Location2");
                        if (spell.SData.LineWidth > 0)
                        {
                            Console.WriteLine("SData.LineWidth: " + spell.SData.LineWidth);
                        }
                        if (spell.SData.CastRadius > 0)
                        {
                            Console.WriteLine("SData.LineWidth: " + spell.SData.CastRadius);
                        }
                    }
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.LocationAoe))
                    {
                        Console.WriteLine("Type.LocationAoe");                        
                        Console.WriteLine("SData.CastRadius: " + spell.SData.CastRadius);
                        if (spell.SData.HaveAfterEffect)
                        {
                            Console.WriteLine(spell.SData.AfterEffectName);     
                        }
                    }
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.LocationVector))
                    {
                        Console.WriteLine("Type.LocationVector"); 
                        if (spell.SData.LineWidth > 0)
                        {
                            Console.WriteLine("SData.LineWidth: " + spell.SData.LineWidth);
                        }                  
                    }                                       
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Cone))
                    {
                        Console.WriteLine("Type.Cone");
                        
                    }
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Self))
                    {
                        Console.WriteLine("Type.Self");
                    }
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAoe))
                    {
                        Console.WriteLine("Type.SelfAoe");
                    }
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.SelfAndUnit))
                    {
                        Console.WriteLine("Type.SelfAndUnit");
                        if (spell.Target != null)
                        {
                            Console.WriteLine("spell.Target: " + spell.Target.Name);    
                        }
                    }
                    if (spell.SData.TargettingType.Equals(SpellDataTargetType.Unit))
                    {
                        Console.WriteLine("Type.Unit");
                        if (spell.Target != null)
                        {
                            Console.WriteLine("spell.Target: " + spell.Target.Name);
                        }
                    }
                }  
            }            
        }
        private static void OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            if (Menu.Item("EC.DevTools.SelfBuffAddBuffRemove").GetValue<bool>())
            {
                if (sender.IsMe)
                {
                    Console.WriteLine("[Self] Buff Added: " + args.Buff.Name);
                }
            }
            if (Menu.Item("EC.DevTools.OthersBuffAddBuffRemove").GetValue<bool>())
            {
                if (sender is Obj_AI_Hero && !sender.IsMe && !Menu.Item("EC.DevTools.Ignore." + sender.NetworkId).GetValue<bool>())
                {
                    Console.WriteLine("[" + sender.Name + "] " + " Buff Added: " + args.Buff.Name);
                }
            }
        }
        private static void OnBuffRemove(Obj_AI_Base sender, Obj_AI_BaseBuffRemoveEventArgs args)
        {
            if (Menu.Item("EC.DevTools.SelfBuffAddBuffRemove").GetValue<bool>())
            {
                if (sender.IsMe)
                {
                    Console.WriteLine("[Self] Buff Removed: " + args.Buff.Name);
                }
            }
            if (Menu.Item("EC.DevTools.OthersBuffAddBuffRemove").GetValue<bool>())
            {
                if (sender is Obj_AI_Hero && !sender.IsMe && !Menu.Item("EC.DevTools.Ignore." + sender.NetworkId).GetValue<bool>())
                {
                    Console.WriteLine("[" + sender.Name + "] " + " Buff Removed: " + args.Buff.Name);
                }
            }
        }
    }
}