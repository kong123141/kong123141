using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.Collections.Generic;

namespace 花边_花式多合一.Core
{
    class AutoZhongya
    {
        private static readonly Items.Item Zhonya = new Items.Item(3157, 0);
        private static readonly Items.Item Seraph = new Items.Item(3040, 0);
        private static bool delayingzhonya;

        public static bool zhonyaready()
        {
            return Zhonya.IsReady();
        }

        public static bool seraphready()
        {
            return Seraph.IsReady();
        }


        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if (!InitializeMenu.Menu.Item("AutoZhongya").GetValue<bool>()) return;
                Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
                Game.OnUpdate += Game_OnUpdate;
            }
            catch (Exception ex)
            {
                Console.WriteLine("AutoZhongyas error occurred: '{0}'", ex);
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Huabian.Player.IsDead || (!zhonyaready() && !seraphready()) || !(sender is Obj_AI_Hero) || sender.IsAlly || !args.Target.IsMe || args.SData.IsAutoAttack() || sender.IsMe)
            {
                return;
            }
            DangerousSpells.Data Spellinfo = null;
            try
            {
                Spellinfo = DangerousSpells.GetByName(args.SData.Name);
            }
            catch (Exception e)
            {
                Console.WriteLine(e + e.StackTrace);
            }


            if (Spellinfo != null)
            {
                Console.WriteLine(Spellinfo.DisplayName);
                if (InitializeMenu.Menu.Item("Enabled" + Spellinfo.DisplayName).GetValue<bool>())
                {
                    Game.PrintChat("Attempting to Zhonya: " + args.SData.Name);
                    var delay = Spellinfo.BaseDelay * 1000;
                    if (zhonyaready())
                    {
                        Utility.DelayAction.Add((int)delay, () => Zhonya.Cast());
                        return;
                    }
                    if (seraphready() && InitializeMenu.Menu.Item("enableseraph").GetValue<bool>())
                    {
                        Utility.DelayAction.Add((int)delay, () => Seraph.Cast());
                    }
                    return;
                }
            }

            if (InitializeMenu.Menu.Item("enablehpzhonya").GetValue<bool>() && (zhonyaready() || seraphready()))
            {
                var calcdmg = sender.GetSpellDamage(Huabian.Player, args.SData.Name);
                var remaininghealth = Huabian.Player.Health - calcdmg;
                var slidervalue = InitializeMenu.Menu.Item("minspelldmg").GetValue<Slider>().Value / 100f;
                var hptozhonya = InitializeMenu.Menu.Item("hptozhonya").GetValue<Slider>().Value;
                var remaininghealthslider = InitializeMenu.Menu.Item("remaininghealth").GetValue<Slider>().Value / 100f;
                if ((calcdmg / Huabian.Player.Health) >= slidervalue || Huabian.Player.HealthPercent <= hptozhonya || remaininghealth <= remaininghealthslider * Huabian.Player.Health)
                {
                    Console.WriteLine("Attempting to Zhonya because incoming spell costs " + calcdmg / Huabian.Player.Health
                        + " of our health.");
                    if (zhonyaready())
                    {
                        Zhonya.Cast();
                        return;
                    }
                    if (seraphready() && InitializeMenu.Menu.Item("enableseraph").GetValue<bool>())
                    {
                        Seraph.Cast();
                    }
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (InitializeMenu.Menu.Item("enablehpzhonya").GetValue<bool>() && zhonyaready())
            {
                BuffDetector();
                if (Huabian.Player.Health < Huabian.Player.MaxHealth * 0.35 && Huabian.Player.CountEnemiesInRange(300) >= 1 && (!SpellSlot.Q.IsReady() || !SpellSlot.W.IsReady() || !SpellSlot.E.IsReady() || !SpellSlot.R.IsReady()) && Huabian.Player.Mana < Huabian.Player.MaxMana * 80)
                {
                    Zhonya.Cast();
                }
            }
        }

        private static void BuffDetector()
        {
            foreach (var buff in Huabian.Player.Buffs)
            {
                var isbadbuff = DangerousBuffs.ScaryBuffs.ContainsKey(buff.Name);

                if (isbadbuff)
                {
                    var bufftime = DangerousBuffs.ScaryBuffs[buff.Name];
                    if (zhonyaready())
                    {
                        if (bufftime.Equals(0))
                        {
                            Zhonya.Cast();
                            return;
                        }
                        delayingzhonya = true;
                        Utility.DelayAction.Add(
                            (int)bufftime, () =>
                            {
                                Zhonya.Cast();
                                delayingzhonya = false;
                            });
                        return;
                    }

                    if (seraphready() && InitializeMenu.Menu.Item("enableseraph").GetValue<bool>() && !delayingzhonya)
                    {
                        if (bufftime.Equals(0))
                        {
                            Seraph.Cast();
                            return;
                        }
                        Utility.DelayAction.Add((int)bufftime, () => Seraph.Cast());
                    }
                }
            }
        }

        class DangerousBuffs
        {
            public static Dictionary<String, float> ScaryBuffs = new Dictionary<String, float>();

            static DangerousBuffs()
            {
                ScaryBuffs.Add("KarthusFallenOne", 2200);
                ScaryBuffs.Add("monkeykingspinknockup", 0);
                ScaryBuffs.Add("zedultexecute", 0);
                ScaryBuffs.Add("fizzmarinerdoombomb", 0);
                ScaryBuffs.Add("missfortunebulletsound", 0);
            }
        }

        public static class DangerousSpells
        {
            public static List<Data> AvoidableSpells = new List<Data>();

            public class Data
            {
                public String Name { get; set; }
                public String DisplayName { get; set; }
                public String Source { get; set; }
                public String RequiredBuff { get; set; }
                public double BaseDelay { get; set; }
                public double SSDelay { get; set; } // Spellshield delay
                public bool Buffrequirement { get; set; }
                public bool Zhonyable { get; set; }
                public bool SpellShieldable { get; set; }

            }



            static DangerousSpells()
            {
                // List of spells that we can avoid using Zhonya or Spellshields

                #region Azir Ultimate

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "AzirR",
                        DisplayName = "Azir R",
                        Source = "Azir",
                        BaseDelay = 0,
                        SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = true
                    });

                #endregion Azir Ultimate

                #region Karthus Ultimate

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "FallenOne",
                        DisplayName = "Karthus R",
                        Source = "Karthus",
                        BaseDelay = 2.5,
                        SSDelay = 2.5,
                        Zhonyable = true,
                        SpellShieldable = true
                    });

                #endregion Karthus Ultimate

                #region Vi Ultimate

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "ViR",
                        DisplayName = "Vi R",
                        Source = "Vi",
                        BaseDelay = 0.2,
                        SSDelay = 1.0,
                        Zhonyable = true,
                        SpellShieldable = true
                    });

                #endregion Vi Ultimate

                #region SyndraR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "SyndraR",
                        DisplayName = "Syndra R",
                        Source = "Syndra",
                        BaseDelay = 0,
                        SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = true
                    });

                #endregion SyndraR

                #region VeigarR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "VeigarPrimordialBurst",
                        DisplayName = "Veigar R",
                        Source = "Veigar",
                        BaseDelay = 0.1,
                        SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = true
                    });

                #endregion VeigarR

                #region MorganaR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "SoulShackles",
                        DisplayName = "Morgana R",
                        Source = "Morgana",
                        BaseDelay = 2.4,
                        SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = true
                    });

                #endregion MorganaR


                #region MalzaharR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "AlZaharNetherGrasp",
                        DisplayName = "Malzahar R",
                        Source = "Malzahar",
                        BaseDelay = 0,
                        SSDelay = 0,
                        Zhonyable = false, //well true but he gets reset so no point
                    SpellShieldable = true
                    });

                #endregion MalzaharR

                #region VladR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "VladimirHemoplague",
                        DisplayName = "Vlad R",
                        Source = "Vladimir",
                        BaseDelay = 2.5, //Zhonya lasts 2.5 seconds 
                    SSDelay = 5,
                        Zhonyable = true,
                        SpellShieldable = true
                    });

                #endregion VladR

                #region CaitlynR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "CaitlynAceintheHole",
                        DisplayName = "Cait R",
                        Source = "Caitlyn",
                        BaseDelay = 0.2, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0.9,
                        Zhonyable = true,
                        SpellShieldable = true
                    });

                #endregion CaitlynR

                #region VelkozR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "VelkozR",
                        DisplayName = "Velkoz R",
                        Source = "VelKoz",
                        BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = false
                    });

                #endregion VelkozR

                #region AniviaR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "GlacialStorm",
                        DisplayName = "Anivia R",
                        Source = "Anivia",
                        BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = false,
                        Buffrequirement = true,
                        RequiredBuff = "aniviaslowdebuffnamehere"
                    });

                #endregion AniviaR

                #region BrandR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "BrandWildfire",
                        DisplayName = "Brand R",
                        Source = "Brand",
                        BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = false,
                        Buffrequirement = true,
                        RequiredBuff = "aniviaslowdebuffnamehere"
                    });

                #endregion BrandR

                #region NunuR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "AbsoluteZero",
                        DisplayName = "Nunu R",
                        Source = "Nunu",
                        BaseDelay = 2.5, //Zhonya lasts 2.5 seconds 
                    SSDelay = 2.8,
                        Zhonyable = true,
                        SpellShieldable = false,
                        Buffrequirement = true,
                        RequiredBuff = "aniviaslowdebuffnamehere"
                    });

                #endregion NunuR

                #region ZyraR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "ZyraBrambleZone",
                        DisplayName = "Zyra R",
                        Source = "Zyra",
                        BaseDelay = 1.85, //Zhonya lasts 2.5 seconds 
                    SSDelay = 1.9,
                        Zhonyable = true,
                        SpellShieldable = true,
                        Buffrequirement = true,
                        RequiredBuff = "aniviaslowdebuffnamehere"
                    });

                #endregion ZyraR

                #region RumbleR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "RumbleCarpetBomb",
                        DisplayName = "Rumble R",
                        Source = "Rumble",
                        BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = false,
                        Buffrequirement = true,
                        RequiredBuff = "aniviaslowdebuffnamehere"
                    });

                #endregion RumbleR

                #region LuxR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "LuxMaliceCannon",
                        DisplayName = "Lux R",
                        Source = "Lux",
                        BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0.1,
                        Zhonyable = true,
                        SpellShieldable = true,
                        Buffrequirement = false,
                        RequiredBuff = "aniviaslowdebuffnamehere"
                    });

                #endregion LuxR

                #region LissandraR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "LissandraR",
                        DisplayName = "Lissandra R",
                        Source = "Lissandra",
                        BaseDelay = 0.1, //Zhonya lasts 2.5 seconds 
                    SSDelay = 1,
                        Zhonyable = true,
                        SpellShieldable = true,
                        Buffrequirement = false,
                        RequiredBuff = "aniviaslowdebuffnamehere"
                    });

                #endregion LissandraR

                #region KennenR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "KennenShurikenStorm",
                        DisplayName = "Kennen R",
                        Source = "Kennen",
                        BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = true,
                        Buffrequirement = false,
                        RequiredBuff = "aniviaslowdebuffnamehere"
                    });

                #endregion KennenR

                #region FiddleR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "Crowstorm",
                        DisplayName = "Fiddle R",
                        Source = "Fiddlesticks",
                        BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = false,
                        Buffrequirement = false,
                        RequiredBuff = "aniviaslowdebuffnamehere"
                    });

                #endregion FiddleR

                #region FioraR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "FioraDanceStrike",
                        DisplayName = "Fiora R",
                        Source = "Fiora",
                        BaseDelay = 0.2, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = false,
                        Buffrequirement = false,
                        RequiredBuff = "aniviaslowdebuffnamehere"
                    });

                #endregion FioraR

                #region FioraR2

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "FioraDance",
                        DisplayName = "Fiora R2",
                        Source = "Fiora",
                        BaseDelay = 0.2, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = false,
                        Buffrequirement = false,
                        RequiredBuff = "aniviaslowdebuffnamehere"
                    });

                #endregion FioraR2

                #region WukongR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "MonkeyKingSpinToWin",
                        DisplayName = "Wukong R",
                        Source = "MonkeyKing",
                        BaseDelay = 0, //Zhonya lasts 2.5 seconds 
                    SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = false,
                        Buffrequirement = false,
                        RequiredBuff = "aniviaslowdebuffnamehere"
                    });

                #endregion WukongR

                #region ZedR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "zedult",
                        DisplayName = "Zed R",
                        Source = "Zed",
                        BaseDelay = 1.5, //Zed untargettable for 0.75 seconds
                    SSDelay = 2.8,
                        Zhonyable = true,
                        SpellShieldable = true,
                        Buffrequirement = false,
                        RequiredBuff = "aniviaslowdebuffnamehere"
                    });

                #endregion ZedR

                #region GarenR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "GarenR",
                        DisplayName = "Garen R",
                        Source = "Garen",
                        BaseDelay = 0.1, //Zed untargettable for 0.75 seconds
                    SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = true,
                        Buffrequirement = false,
                        RequiredBuff = "aniviaslowdebuffnamehere"
                    });

                #endregion GarenR

                #region NautilusR

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "nautilusgrandline",
                        DisplayName = "Nautilus R",
                        Source = "Nautilus",
                        BaseDelay = 0.1, //Zed untargettable for 0.75 seconds
                    SSDelay = 0.1,
                        Zhonyable = true,
                        SpellShieldable = true,
                        Buffrequirement = false,
                        RequiredBuff = "aniviaslowdebuffnamehere"
                    });

                #endregion NautilusR


                #region Jarvan R

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "JarvanIVCataclysm",
                        DisplayName = "Jarvan R",
                        Source = "JarvanIV",
                        BaseDelay = 0, //Zed untargettable for 0.75 seconds
                    SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = false,
                        Buffrequirement = false,
                        RequiredBuff = "aniviaslowdebuffnamehere"
                    });

                #endregion Jarvan R


                #region Sejuani R

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "SejuaniGlacialPrisonStart",
                        DisplayName = "Sejuani R",
                        Source = "Sejuani",
                        BaseDelay = 0.1, //Zed untargettable for 0.75 seconds
                    SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = true,
                        Buffrequirement = false,
                        RequiredBuff = "aniviaslowdebuffnamehere"
                    });

                #endregion Sejuani R

                #region Katarina R

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "KatarinaR",
                        DisplayName = "Katarina R",
                        Source = "Katarina",
                        BaseDelay = 0, //Zed untargettable for 0.75 seconds
                    SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = false,
                        Buffrequirement = false,
                        RequiredBuff = "katarinarsound"
                    });

                #endregion Katarina R

                #region Urgot R

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "urgotswap2",
                        DisplayName = "Urgot R",
                        Source = "Urgot",
                        BaseDelay = 0, //Zed untargettable for 0.75 seconds
                    SSDelay = 0,
                        Zhonyable = true,
                        SpellShieldable = false,
                        Buffrequirement = false,
                        RequiredBuff = "" //urgot ult debuff name
                });

                #endregion Urgot R

                #region Nocturne R

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "NocturneParanoia",
                        DisplayName = "Nocturne R",
                        Source = "Nocturne",
                        BaseDelay = 0.15,
                        SSDelay = 1.0,
                        Zhonyable = true,
                        SpellShieldable = false,
                        Buffrequirement = false,
                        RequiredBuff = "" //maybe nocturne shroud debuffname
                });

                #endregion Nocturne R

                #region Yasuo

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "YasuoRKnockUpComboW",
                        DisplayName = "Yasuo R",
                        Source = "Yasuo",
                        BaseDelay = 0,
                        SSDelay = 1.0,
                        Zhonyable = true,
                        SpellShieldable = false,
                        Buffrequirement = false,
                        RequiredBuff = ""
                    });

                #endregion Yasuo

                #region Orianna

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "OrianaDetonateCommand",
                        DisplayName = "Orianna R",
                        Source = "Orianna",
                        BaseDelay = 0,
                        SSDelay = 1.0,
                        Zhonyable = true,
                        SpellShieldable = false,
                        Buffrequirement = false,
                        RequiredBuff = ""
                    });

                #endregion Orianna

                #region Riven

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "RivenFengShuiEngine",
                        DisplayName = "Riven R",
                        Source = "Riven",
                        BaseDelay = 0,
                        SSDelay = 1.0,
                        Zhonyable = true,
                        SpellShieldable = false,
                        Buffrequirement = false,
                        RequiredBuff = ""
                    });

                #endregion Riven

                #region Velkoz

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "VelkozR",
                        DisplayName = "Velkoz R",
                        Source = "Velkoz",
                        BaseDelay = 0,
                        SSDelay = 1.0,
                        Zhonyable = true,
                        SpellShieldable = false,
                        Buffrequirement = false,
                        RequiredBuff = ""
                    });

                #endregion Velkoz

                #region Velkoz

                AvoidableSpells.Add(
                    new Data
                    {
                        Name = "ViktorChaosStorm",
                        DisplayName = "Viktor R",
                        Source = "Viktor",
                        BaseDelay = 0,
                        SSDelay = 1.0,
                        Zhonyable = true,
                        SpellShieldable = false,
                        Buffrequirement = false,
                        RequiredBuff = ""
                    });

                #endregion Velkoz



            }

            public static Data GetByName(string spellName)
            {
                spellName = spellName.ToLower();
                foreach (var Data in AvoidableSpells)
                {
                    if (Data.Name.ToLower() == spellName)
                    {
                        return Data;
                    }
                }
                return null;
            }


            public static Data GetByName2(string spellName)
            {
                return AvoidableSpells.FirstOrDefault(spell => String.Equals(spell.Name, spellName, StringComparison.CurrentCultureIgnoreCase));
            }
        }
    }
}
