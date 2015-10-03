namespace ElUtilitySuite
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    internal static class Offensive
    {
        #region Static Fields

        public static bool CanManamune;

        #endregion

        #region Public Methods and Operators

        public static void Load()
        {
            try
            {
                Game.OnUpdate += OnUpdate;
                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        #endregion

        #region Methods

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && (Items.HasItem(3042) || Items.HasItem(3043)))
            {
                if (Entry.Player.GetSpellSlot(args.SData.Name) == SpellSlot.Unknown
                    && (InitializeMenu.Menu.Item("usecombo").GetValue<KeyBind>().Active
                        || args.Target.Type == Entry.Player.Type))
                {
                    CanManamune = true;
                }

                else
                {
                    Utility.DelayAction.Add(400, () => CanManamune = false);
                }
            }
        }

        private static void OffensiveItemManager()
        {
            if (InitializeMenu.Menu.Item("useMuramana").GetValue<bool>())
            {
                if (CanManamune)
                {
                    if (InitializeMenu.Menu.Item("muraMode").GetValue<StringList>().SelectedIndex != 1
                        || InitializeMenu.Menu.Item("usecombo").GetValue<KeyBind>().Active)
                    {
                        var manamune = Entry.Player.GetSpellSlot("Muramana");
                        if (manamune != SpellSlot.Unknown && !Entry.Player.HasBuff("Muramana"))
                        {
                            if (Entry.Player.Mana / Entry.Player.MaxMana * 100
                                > InitializeMenu.Menu.Item("useMuramanaMana").GetValue<Slider>().Value)
                            {
                                Entry.Player.Spellbook.CastSpell(manamune);
                            }

                            Utility.DelayAction.Add(400, () => CanManamune = false);
                        }
                    }
                }

                if (!CanManamune && !InitializeMenu.Menu.Item("usecombo").GetValue<KeyBind>().Active)
                {
                    var manamune = Entry.Player.GetSpellSlot("Muramana");
                    if (manamune != SpellSlot.Unknown && Entry.Player.HasBuff("Muramana"))
                    {
                        Entry.Player.Spellbook.CastSpell(manamune);
                    }
                }
            }

            if (InitializeMenu.Menu.Item("usecombo").GetValue<KeyBind>().Active)
            {
                UseItem("Frostclaim", 3092, 850f, true);
                UseItem("Youmuus", 3142, 650f);
                UseItem("Hydra", 3077, 250f);
                UseItem("Hydra", 3074, 250f);
                UseItem("Hextech", 3146, 700f, true);
                UseItem("Cutlass", 3144, 450f, true);
                UseItem("Botrk", 3153, 450f, true);
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Entry.Player.IsDead)
            {
                return;
            }

            try
            {
                OffensiveItemManager();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: '{0}'", e);
            }
        }

        private static void UseItem(string name, int itemId, float range, bool targeted = false)
        {
            if (!Items.HasItem(itemId) || !Items.CanUseItem(itemId))
            {
                return;
            }

            if (!InitializeMenu.Menu.Item("use" + name).GetValue<bool>())
            {
                return;
            }

            Obj_AI_Hero target = null;

            foreach (var targ in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(hero => hero.IsValidTarget(2000))
                    .OrderByDescending(hero => hero.Distance(Game.CursorPos)))
            {
                target = targ;
            }

            if (target.IsValidTarget(range))
            {
                if (target != null)
                {
                    var eHealthPercent = (int)((target.Health / target.MaxHealth) * 100);
                    var aHealthPercent = (int)((Entry.Player.Health / target.MaxHealth) * 100);

                    if (eHealthPercent <= InitializeMenu.Menu.Item("use" + name + "Pct").GetValue<Slider>().Value
                        && InitializeMenu.Menu.Item("ouseOn" + target.SkinName).GetValue<bool>())
                    {
                        if (targeted && itemId == 3092)
                        {
                            var pi = new PredictionInput
                                         {
                                             Aoe = true, Collision = false, Delay = 0.0f, From = Entry.Player.Position,
                                             Radius = 250f, Range = 850f, Speed = 1500f, Unit = target,
                                             Type = SkillshotType.SkillshotCircle
                                         };

                            var po = Prediction.GetPrediction(pi);
                            if (po.Hitchance >= HitChance.Medium)
                            {
                                Items.UseItem(itemId, po.CastPosition);
                            }
                        }

                        else if (targeted)
                        {
                            Items.UseItem(itemId, target);
                        }

                        else
                        {
                            Items.UseItem(itemId);
                        }
                    }

                    else if (aHealthPercent <= InitializeMenu.Menu.Item("use" + name + "Me").GetValue<Slider>().Value
                             && InitializeMenu.Menu.Item("ouseOn" + target.SkinName).GetValue<bool>())
                    {
                        if (targeted)
                        {
                            Items.UseItem(itemId, target);
                        }
                        else
                        {
                            Items.UseItem(itemId);
                        }
                    }
                }
            }
        }

        #endregion
    }
}