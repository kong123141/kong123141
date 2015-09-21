using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TheKalista.Commons;
using TheKalista.Commons.Debug;
using TheKalista.Commons.Items;
using TheKalista.Commons.SummonerSpells;
using Color = System.Drawing.Color;

namespace TheKalista
{
    class Kalista
    {
        public static Obj_AI_Hero Soulbound;
        public static Obj_AI_Minion Baron, Dragon;

        static void Main()
        {

            CustomEvents.Game.OnGameLoad += (eArgs) =>
            {
                DevAssistant.Init(); //todo: remove
                if (ObjectManager.Player.ChampionName != "Kalista") return;
                
                var alwaysStealBaronAndDrake = false;

                var mainMenu = new Menu("The Kalista", "TheKalista", true);
                mainMenu.AddToMainMenu();

                var orbwalkerMenu = mainMenu.CreateSubmenu("Orbwalker");
                var targetselectorMenu = mainMenu.CreateSubmenu("Target Selector");
                var comboMenu = mainMenu.CreateSubmenu("Combo");
                var ultMenu = comboMenu.CreateSubmenu("R Settings");
                var harassMenu = mainMenu.CreateSubmenu("Harass");
                var laneclearMenu = mainMenu.CreateSubmenu("Laneclear");
                var lasthitMenu = mainMenu.CreateSubmenu("Lasthit / Steal");
                var itemsMenu = mainMenu.CreateSubmenu("Items");
                var summonersMenu = mainMenu.CreateSubmenu("Summoners");
                var miscMenu = mainMenu.CreateSubmenu("Misc");
                var autoLevelSpells = mainMenu.CreateSubmenu("Auto Level Spells");
                var drawingsMenu = mainMenu.CreateSubmenu("Drawings");

                var q = new KalistaQ(SpellSlot.Q, 1200, TargetSelector.DamageType.Physical);
                var w = new KalistaW(SpellSlot.W);
                var e = new KalistaE(SpellSlot.E, 1000, TargetSelector.DamageType.Physical);
                var r = new KalistaR(SpellSlot.R, 1500, TargetSelector.DamageType.Physical);

                var orbwalker = new KalistaWalker(orbwalkerMenu);
                KalistaTargetSelector.AddToMenu(targetselectorMenu, e);
                orbwalkerMenu.Item("ExtraWindup").DontSave();
                orbwalkerMenu.Item("ExtraWindup").SetValue(new Slider(0, 0, 1));
                orbwalkerMenu.AddMItem("(You don't need extra windup on Kalista)").FontColor = new SharpDX.Color(218, 165, 32);

                var provider = new KalistaCombo(2100, orbwalker, q, w, e, r);
                provider.Initialize();

                provider.CreateBasicMenu(null, null, null, null, null, null, null, drawingsMenu);
                provider.CreateComboMenu(comboMenu, SpellSlot.W);
                provider.CreateLaneclearMenu(harassMenu, true, SpellSlot.W);
                provider.CreateLaneclearMenu(laneclearMenu, true, SpellSlot.W);
                //provider.CreateLasthitMenu(lasthitMenu, true, SpellSlot.Q, SpellSlot.E);
                provider.CreateItemsMenu(itemsMenu, new BilgewaterCutlass(), new Botrk(), new YoumusBlade(), new Qss(), new MercuralScimitar());
                provider.CreateSummonersMenu(summonersMenu, new Cleanse(), new Heal());
                provider.CreateAutoLevelMenu(autoLevelSpells, Commons.ComboSystem.ComboProvider.SpellOrder.RWEQ, Commons.ComboSystem.ComboProvider.SpellOrder.REQW);

                comboMenu.AddMItem("Focus", new StringList(new[] { "Most Damage", "More Damage", "Damage + Mobility", "More Mobility", "Most Mobility" }), val => e.MobilityType = val.SelectedIndex);

                if (HeroManager.Allies.Any(ally => ally.ChampionName == "Blitzcrank"))
                {
                    ultMenu.AddMItem("Balista", true, val => r.Balista = val);
                    ultMenu.AddMItem("Balista min. range", new Slider(700, 100, 1450), val => r.BalistaDistance = val.Value); //vales from iKalista
                }

                ultMenu.AddMItem("Ally Health %", new Slider(15), val => r.AllyHealth = val.Value);
                ultMenu.AddMItem("My Health %", new Slider(35), val => r.MyHealth = val.Value);
                ultMenu.AddMItem("Smart peel", true, val => r.SmartPeel = val);
                //ultMenu.AddMItem("Possible hit count", new Slider(4, 0, 6), val => r.HitCount = val.Value);
                ultMenu.AddMItem("(Will ult if any condition is met)").FontColor = new SharpDX.Color(218, 165, 32); // Color.Goldenrod 218 165 32

                lasthitMenu.AddMItem("E minions it would miss", true, val => e.FarmAssist = val);
                lasthitMenu.AddMItem("Min Mana %", new Slider(50), val => e.FarmAssistMana = val.Value);
                lasthitMenu.AddMItem("Always steal Baron and Drake", true, val => alwaysStealBaronAndDrake = val);
                lasthitMenu.AddMItem("Jungle steal in Mixed/Lasthit/Clear", true, val => e.StealJungle = val);

                laneclearMenu.AddMItem("Min. E kills", new Slider(4, 1, 12), val => e.MinLaneclear = val.Value);
                laneclearMenu.AddMItem("Min. Q kills", new Slider(3, 1, 12), val => q.MinLaneclear = val.Value);



                miscMenu.AddMItem("Stuck W Baron", new KeyBind("T"[0], KeyBindType.Press), val =>
                {
                    if (val.Active)
                    {
                        if (w.BaronPosition.Distance(ObjectManager.Player.Position) < 5000)
                            w.Cast(w.BaronPosition);
                        else
                        {
                            Notifications.AddNotification("Not in range!", 3);
                        }
                    }
                });

                miscMenu.AddMItem("Stuck W Dragon", new KeyBind("Y"[0], KeyBindType.Press), val =>
                {
                    if (val.Active)
                    {
                        if (w.DragonPosition.Distance(ObjectManager.Player.Position) < 5000)
                            w.Cast(w.DragonPosition);
                        else
                        {
                            Notifications.AddNotification("Not in range!", 3);
                        }
                    }
                });
                miscMenu.AddMItem("E damage reduction", new Slider(20), val => e.Reduction = val.Value);


                var autobuyblue = miscMenu.AddMItem("Autobuy: Blue Trinket", true);
                var autobuyblueLevel = miscMenu.AddMItem("Autobuy: Blue Trinket: Level", new Slider(10, 1, 18));
                var autoUseBlueTrinket = true;
                miscMenu.AddMItem("Use blue trinket", true, val => autoUseBlueTrinket = val);

                AttackableUnit.OnLeaveTeamVisiblity += (sender, args) =>
                {
                    if (autoUseBlueTrinket && sender.Type == GameObjectType.obj_AI_Hero && sender.IsEnemy && !sender.IsDead && !((Obj_AI_Hero)sender).UnderTurret() && sender.Position.Distance(ObjectManager.Player.Position, true) < 500 * 500 && provider.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    {
                        var orb = ObjectManager.Player.Spellbook.Spells.FirstOrDefault(spell => spell.Name.ToLower().Contains("trinketorb"));
                        if (orb != null)
                            ObjectManager.Player.Spellbook.CastSpell(orb.Slot, sender.Position);
                        var trinket = ObjectManager.Player.Spellbook.Spells.FirstOrDefault(spell => spell.Name.ToLower().Contains("trinkettotem"));
                        if(trinket != null)
                            ObjectManager.Player.Spellbook.CastSpell(trinket.Slot, sender.Position);
                    }
                };
                //itemmercurial & Mercurial_Scimitar / / ItemMercurial
                //QuicksilverSash & Quicksilver_Sash / / QuicksilverSash
                //drawingsMenu.AddMItem("E progress", new StringList(new[] {"AAs to kill", "% to kill"}), list => e.DrawCount = list.SelectedIndex == 0);

                Drawing.OnDraw += _ =>
                {

                    //Drawing.DrawText(200, 200, Color.Red, e.Instance.GetState().ToString());
                    //Profiler.DrawSections(700, 200);
                };

                Game.OnUpdate += _ =>
                {
                    Profiler.Clear(); //Todo: remove

                    if (alwaysStealBaronAndDrake || provider.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None)
                    {

                        if (Baron.IsValidTarget(e.Range) && Baron.Health < Baron.MaxHealth)
                        {
                            var damage = e.GetDamage(Baron);
                            if (ObjectManager.Player.HasBuff("barontarget"))
                                damage *= 0.5f;
                            if (damage > Baron.Health) e.Cast();
                        }

                        if (Dragon.IsValidTarget(e.Range) && Dragon.Health < Dragon.MaxHealth)
                        {
                            var damage = e.GetDamage(Dragon);
                            if (ObjectManager.Player.HasBuff("s5test_dragonslayerbuff"))
                                damage *= (1 - (.07f * ObjectManager.Player.GetBuffCount("s5test_dragonslayerbuff")));
                            if (damage > Dragon.Health) e.Cast();
                        }
                    }

                    provider.Update();

                    if (!TickLimiter.Limit(1000)) return;

                    if (Soulbound == null)
                        Soulbound = HeroManager.Allies.FirstOrDefault(ally => ally.HasBuff("kalistacoopstrikeally"));

                    if (Baron == null || Dragon != null)
                    {
                        Baron = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(min => min.Name.StartsWith("SRU_Baron"));
                        Dragon = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(min => min.Name.StartsWith("SRU_Dragon"));
                    }

                    if (autobuyblue.IsActive() && ObjectManager.Player.Level >= autobuyblueLevel.GetValue<Slider>().Value && ObjectManager.Player.InFountain() && ObjectManager.Player.InventoryItems.Any(item => item.Id == ItemId.Warding_Totem_Trinket))
                    {
                        ObjectManager.Player.BuyItem(ItemId.Scrying_Orb_Trinket);
                    }

                };

                //var pos = ObjectManager.Player.Position;
                //Drawing.OnDraw += _ =>
                //{
                //    Drawing.DrawText(600, 600, Color.Red, ObjectManager.Player.Position.Distance(pos).ToString());
                //};
            };
        }




    }
}
