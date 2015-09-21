namespace ElUtilitySuite
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using ItemData = LeagueSharp.Common.Data.ItemData;

    public static class ProtectYourself
    {
        #region Static Fields

        private static Spell gapcloseSpell;

        private static Obj_AI_Hero rengarObj;

        #endregion

        #region Public Methods and Operators

        public static void Load()
        {
            try
            {
                gapcloseSpell = Championspells();
                GameObject.OnCreate += OnCreateObject;
                Game.OnUpdate += OnUpdate;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion

        #region Methods

        private static void AntiRengar()
        {
            if (!SupportedChampions())
            {
                return;
            }

            if (rengarObj.ChampionName == "Rengar")
            {
                if (rengarObj.IsValidTarget(gapcloseSpell.Range) && gapcloseSpell.IsReady()
                    && rengarObj.Distance(Entry.Player) <= gapcloseSpell.Range)
                {
                    gapcloseSpell.Cast(rengarObj);
                }
            }
        }

        private static Spell Championspells()
        {
            switch (Entry.Player.ChampionName)
            {
                case "Vayne":
                    return new Spell(SpellSlot.E, 550);
                case "Tristana":
                    return new Spell(SpellSlot.R, 550);
                case "Draven":
                    return new Spell(SpellSlot.E, 1100);
                case "Leesin":
                    return new Spell(SpellSlot.R, 375);
                case "Janna":
                    return new Spell(SpellSlot.R, 550);
                case "Fiddlesticks":
                    return new Spell(SpellSlot.Q, 575);
                case "Ashe":
                    return new Spell(SpellSlot.R, 20000); //global, amk.
                case "Braum":
                    return new Spell(SpellSlot.R, 1200);
                case "Thresh":
                    return new Spell(SpellSlot.E, 400);
                case "Urgot":
                    return new Spell(SpellSlot.R, 700);
            }

            return null;
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Rengar_LeapSound.troy" && sender.IsEnemy)
            {
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            hero =>
                            hero.IsValidTarget(1500) && hero.ChampionName == "Rengar" && !hero.IsVisible && !hero.IsDead)
                    )
                {
                    rengarObj = enemy;
                }
            }
            if (rengarObj != null && Entry.Player.Distance(rengarObj, true) < 1000 * 1000
                && InitializeMenu.Menu.Item("Protect.Rengar").GetValue<bool>())
            {
                AntiRengar();
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base enemy, GameObjectProcessSpellCastEventArgs args)
        {
            try
            {
                if (enemy.Type == GameObjectType.obj_AI_Hero && enemy.IsEnemy)
                {
                    if (args.SData.Name == "akalismokebomb")
                    {
                        if (InitializeMenu.Menu.Item("Protect.Akali").GetValue<bool>())
                        {
                            var pinkTrinket = ItemData.Greater_Vision_Totem_Trinket.GetItem();
                            var pinkWard = ItemData.Vision_Ward.GetItem();
                            var oracleLens = ItemData.Oracles_Lens_Trinket.GetItem();

                            if (InitializeMenu.Menu.Item("usecombo").GetValue<KeyBind>().Active)
                            {
                                var akaliPinkHp = InitializeMenu.Menu.Item("Protect.Akali.HP").GetValue<Slider>().Value;
                                var akaliHp = (int)((enemy.Health / enemy.MaxHealth) * 100);

                                if (akaliHp <= akaliPinkHp)
                                {
                                    var wardPos = args.End;
                                    if (wardPos.Distance(Entry.Player.Position) > 600)
                                    {
                                        wardPos = wardPos.Extend(Entry.Player.Position, 220);
                                        if (Entry.Player.Distance(wardPos) <= 600)
                                        {
                                            return;
                                        }
                                    }

                                    if (pinkTrinket.IsOwned(Entry.Player) && pinkTrinket.IsReady()
                                        && InitializeMenu.Menu.Item("Protect.Akali.Trinket").GetValue<bool>())
                                    {
                                        pinkTrinket.Cast(wardPos);
                                    }

                                    if (oracleLens.IsOwned(Entry.Player) && oracleLens.IsReady()
                                        && InitializeMenu.Menu.Item("Protect.Akali.Sweeping").GetValue<bool>())
                                    {
                                        oracleLens.Cast(wardPos);
                                    }

                                    if (pinkWard.IsOwned(Entry.Player) && pinkWard.IsReady()
                                        && InitializeMenu.Menu.Item("Protect.Akali.PinkWard").GetValue<bool>())
                                    {
                                        pinkWard.Cast(wardPos);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        //rengarralertsound RengarRBuff
        private static void OnUpdate(EventArgs args)
        {
            if (Entry.Player.IsDead)
            {
                return;
            }

            try
            {
                var oracleLens = ItemData.Oracles_Lens_Trinket.GetItem();

                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(hero => hero.IsValidTarget(1500) && hero.ChampionName == "Rengar"))
                {
                    if (enemy.HasBuff("rengarralertsound") || enemy.HasBuff("RengarRBuff"))
                    {
                        if (oracleLens.IsOwned(Entry.Player) && oracleLens.IsReady()
                            && InitializeMenu.Menu.Item("Protect.Rengar.Lens").GetValue<bool>())
                        {
                            oracleLens.Cast(enemy.ServerPosition);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        private static bool SupportedChampions()
        {
            return Entry.Player.ChampionName == "Vayne" || Entry.Player.ChampionName == "Tristana"
                   || Entry.Player.ChampionName == "Draven" || Entry.Player.ChampionName == "Ashe"
                   || Entry.Player.ChampionName == "Leesin" || Entry.Player.ChampionName == "Janna"
                   || Entry.Player.ChampionName == "Fiddlesticks" || Entry.Player.ChampionName == "Braum"
                   || Entry.Player.ChampionName == "Thresh" || Entry.Player.ChampionName == "Urgot";
        }

        #endregion
    }
}