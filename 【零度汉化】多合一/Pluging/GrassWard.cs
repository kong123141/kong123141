namespace Pluging
{
    using System;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;

    public class GrassWard
    {
        public static Menu Menu;
        public static int LastWardTime;

        public GrassWard(Menu mainMenu)
        {
            Menu = mainMenu;

            Menu GrassWardMenu = new Menu("[FL] 进草插眼", "GrassWard");

            GrassWardMenu.AddItem(new MenuItem("EnableGrassWard", "启动").SetValue(true));
            GrassWardMenu.AddItem(new MenuItem("GarssWardOnlyInCombo", "仅连招时插眼 ").SetValue(true));

            Menu.AddSubMenu(GrassWardMenu);

            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs args)
        {
            try
            {
                if (!Menu.Item("EnableGrassWard").GetValue<bool>())
                    return;

                var ramdom = WeightedRandom.Next(200, 700);

                var OnlyInCombo = Menu.Item("GarssWardOnlyInCombo").GetValue<bool>();

                var InCombo = Orbwalking.Orbwalker.Instances.Find(x => x.ActiveMode == Orbwalking.OrbwalkingMode.Combo);

                if (OnlyInCombo && (InCombo == null))
                    return;

                foreach (var e in HeroManager.Enemies.Where(e => !e.IsDead && e.Distance(ObjectManager.Player) < 1000))
                {
                    var path = e.GetWaypoints().LastOrDefault().To3D();

                    if (NavMesh.IsWallOfGrass(path, 1))
                    {
                        if (e.Distance(path) > 200)
                            return;

                        if (NavMesh.IsWallOfGrass(ObjectManager.Player.Position, 1) && ObjectManager.Player.Distance(path) < 200)
                            return;

                        if (ObjectManager.Player.Distance(path) < 500)
                        {
                            foreach(var obj in ObjectManager.Get<Obj_AI_Base>().Where(x => x.Name.ToLower().Contains("Ward") && x.IsAlly && x.Distance(path) < 300))
                            {
                                if (NavMesh.IsWallOfGrass(obj.Position, 1))
                                    return;
                            }

                            var wards = Items.GetWardSlot();

                            if(wards != null)
                            {
                                if (Environment.TickCount - LastWardTime > 1000)
                                {
                                    Utility.DelayAction.Add(ramdom, () =>
                                    {
                                        ObjectManager.Player.Spellbook.CastSpell(wards.SpellSlot, path);
                                    });

                                    LastWardTime = Environment.TickCount;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GrassWard.OnUpdate + " + ex);
            }
        }
    }
}