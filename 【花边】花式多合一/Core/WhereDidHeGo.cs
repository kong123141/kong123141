using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace 花边_花式多合一.Core
{
    class WhereDidHeGo
    {
        public static List<Tuple<int, Vector3>> StealthPoses = new List<Tuple<int, Vector3>>();
        public static List<Vector3> PingPoses = new List<Vector3>();
        public static List<Tuple<float, List<Vector2>>> StealthPaths = new List<Tuple<float, List<Vector2>>>();
        public static List<Tuple<int, string>> StealthSpells = new List<Tuple<int, string>>();
        public static List<_sdata> AntiStealthSpells = new List<_sdata>();
        public static Obj_AI_Hero Teemo;

        public struct _sdata
        {
            public string ChampionName;
            public SpellSlot Spell;
            public float SpellRange;
            public int StealthDetectionLevel;
            public bool SelfCast;
        }
        internal class Load
        {
            public Load()
            {
                try
                {
                    StealthSpells.Add(new Tuple<int, string>(3, "akalismokebomb"));
                    StealthSpells.Add(new Tuple<int, string>(1, "khazixr"));
                    StealthSpells.Add(new Tuple<int, string>(1, "khazixrlong"));
                    StealthSpells.Add(new Tuple<int, string>(3, "talonshadowassault"));
                    StealthSpells.Add(new Tuple<int, string>(1, "monkeykingdecoy"));
                    StealthSpells.Add(new Tuple<int, string>(1, "hideinshadows"));

                    AntiStealthSpells.Add(new _sdata { ChampionName = "caitlyn", Spell = SpellSlot.W, SpellRange = 800, StealthDetectionLevel = 1 });
                    AntiStealthSpells.Add(new _sdata { ChampionName = "kogmaw", Spell = SpellSlot.R, SpellRange = 1200, StealthDetectionLevel = 1 }); //range + level * 300
                    AntiStealthSpells.Add(new _sdata { ChampionName = "leesin", Spell = SpellSlot.Q, SpellRange = 1100, StealthDetectionLevel = 1 });
                    AntiStealthSpells.Add(new _sdata { ChampionName = "nidalee", Spell = SpellSlot.W, SpellRange = 900, StealthDetectionLevel = 1 });
                    AntiStealthSpells.Add(new _sdata { ChampionName = "nocturne", Spell = SpellSlot.Q, SpellRange = 1200, StealthDetectionLevel = 1 });
                    AntiStealthSpells.Add(new _sdata { ChampionName = "twistedfate", Spell = SpellSlot.R, SpellRange = 4000, StealthDetectionLevel = 3, SelfCast = true });
                    AntiStealthSpells.Add(new _sdata { ChampionName = "fizz", Spell = SpellSlot.W, SpellRange = 1275, StealthDetectionLevel = 2 });

                    Teemo = HeroManager.Enemies.FirstOrDefault(p => p.ChampionName.ToLower() == "teemo");

                    Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast; //for stealth spells
                    GameObject.OnCreate += Obj_AI_Base_OnCreate; //for lb passive & rengar ult
                    Drawing.OnDraw += Drawing_OnDraw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("WhereDidHeGo error occurred: '{0}'", ex);
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            lock (StealthPoses)
            {
                StealthPoses.RemoveAll(p => Utils.TickCount - p.Item1 > 4000);
                foreach (var p in StealthPoses)
                    if (InitializeMenu.Menu.Item("DRAWCIRCLE").GetValue<bool>())
                        Render.Circle.DrawCircle(p.Item2, 75, System.Drawing.Color.DarkRed);
            }

            lock (PingPoses)
            {
                foreach (var p in PingPoses)
                {
                    switch (InitializeMenu.Menu.Item("PINGMODE").GetValue<StringList>().SelectedIndex)
                    {
                        case 0:
                            {
                                for (int i = 0; i < InitializeMenu.Menu.Item("PINGCOUNT").GetValue<Slider>().Value; i++)
                                    Game.ShowPing(PingCategory.EnemyMissing, p, true);
                            }
                            break;
                        case 1:
                            {
                                for (int i = 0; i < InitializeMenu.Menu.Item("PINGCOUNT").GetValue<Slider>().Value; i++)
                                    Game.SendPing(PingCategory.EnemyMissing, p);
                            }
                            break;
                    }

                }

                PingPoses.Clear();
            }

            lock (StealthPaths)
            {
                if (InitializeMenu.Menu.Item("DRAWWAPOINTS").GetValue<bool>())
                {
                    foreach (var p in StealthPaths)
                    {
                        if (p.Item2.Count > 1)
                        {
                            for (int i = 0; i < p.Item2.Count - 1; i++)
                            {
                                Vector2 posFrom = Drawing.WorldToScreen(p.Item2[i].To3D());
                                Vector2 posTo = Drawing.WorldToScreen(p.Item2[i + 1].To3D());
                                Drawing.DrawLine(posFrom, posTo, 2, System.Drawing.Color.Aqua);
                            }

                            Vector2 pos = Drawing.WorldToScreen(p.Item2[p.Item2.Count - 1].To3D());
                            Drawing.DrawText(pos.X, pos.Y, System.Drawing.Color.Black, p.Item1.ToString("0.00")); //arrival time
                            Render.Circle.DrawCircle(p.Item2[p.Item2.Count - 1].To3D(), 75f, System.Drawing.Color.Aqua); //end circle
                        }
                    }
                }
            }

            if (Teemo != null && !Teemo.IsVisible)
                Render.Circle.DrawCircle(Teemo.Position, 75f, System.Drawing.Color.Green);

        }

        private static void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.IsEnemy)
            {
                if ((sender.Name.Contains("Rengar_Base_R_Alert") && ObjectManager.Player.HasBuff("rengarralertsound")) || sender.Name == "LeBlanc_Base_P_poof.troy")
                    AntiStealth.TryDeStealth(sender.Position, 3);
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsEnemy && InitializeMenu.Menu.Item("ENABLEWDHG").GetValue<bool>())
            {
                int level = 1;
                Vector3 pos = args.End;
                switch (args.SData.Name.ToLower())
                {
                    case "deceive":
                        {
                            if (args.Start.Distance(args.End) > 400)
                                pos = args.Start + (args.End - args.Start).Normalized() * 400;
                        }
                        break;
                    case "vaynetumble":
                        {
                            if (sender.HasBuff("VayneInquisition"))
                            {
                                pos = args.Start + (args.End - args.Start).Normalized() * 300;
                                level = 2;
                            }
                            else
                                return;
                        }
                        break;
                    case "summonerflash":
                        {
                            if (sender.IsVisible)
                                return;
                        }
                        break;
                    default:
                        {
                            if (!StealthSpells.Any(p => p.Item2 == args.SData.Name.ToLower()))
                                return;

                            level = StealthSpells.First(p => p.Item2 == args.SData.Name.ToLower()).Item1;
                        }
                        break;
                }

                lock (StealthPoses)
                    StealthPoses.Add(new Tuple<int, Vector3>(Utils.TickCount, pos));

                lock (PingPoses)
                    PingPoses.Add(pos);

                List<Vector2> path = sender.GetWaypoints();
                var pair = new Tuple<float, List<Vector2>>(path.PathLength() / sender.MoveSpeed, path);

                lock (StealthPaths)
                    StealthPaths.Add(pair);

                Utility.DelayAction.Add((int)(path.PathLength() / sender.MoveSpeed * 1000), () => StealthPaths.Remove(pair));

                AntiStealth.TryDeStealth(pos, level);
            }
        }

        public class AntiStealth
        {
            public static bool TryDeStealth(Vector3 pos = new Vector3(), int level = 1)
            {
                if (!TryConsumables(pos))
                    return TrySpells(pos, level);

                return true;
            }

            private static bool TryConsumables(Vector3 pos)
            {
                
                SpellSlot slot = SpellSlot.Unknown;

                //vision ward
                if (InitializeMenu.Menu.Item("USEVISIONWARD").GetValue<bool>())
                {
                    if (Items.CanUseItem(2043) && Items.HasItem(2043))
                        slot = ObjectManager.Player.GetSpellSlot("VisionWard");
                    else if ((Items.CanUseItem(3362) && Items.HasItem(3362)))
                        slot = SpellSlot.Trinket;
                }

                //oracle's lens
                if (InitializeMenu.Menu.Item("USEORACLESLENS").GetValue<bool>())
                {
                    if (Items.CanUseItem(3364) && Items.HasItem(3364))
                        slot = SpellSlot.Trinket;
                }

                //lightbringer active
                if (InitializeMenu.Menu.Item("USELIGHTBRINGER").GetValue<bool>())
                {
                    if (Items.CanUseItem(3185) && Items.HasItem(3185))
                    {
                        Items.UseItem(3185, pos);
                        return true;
                    }
                }

                //hextech sweeper
                if (InitializeMenu.Menu.Item("USEHEXTECHSWEEPER").GetValue<bool>())
                {
                    if (Items.CanUseItem(3187) && Items.HasItem(3185))
                    {
                        Items.UseItem(3187, pos);
                        return true;
                    }
                }

                //aram snowball
                if (InitializeMenu.Menu.Item("USESNOWBALL").GetValue<bool>())
                {
                    if (Game.MapId.Equals(GameMapId.HowlingAbyss))
                        slot = ObjectManager.Player.GetSpellSlot("summonersnowball");
                }

                if (slot != SpellSlot.Unknown)
                    ObjectManager.Player.Spellbook.CastSpell(slot, ObjectManager.Player.ServerPosition);

                return slot != SpellSlot.Unknown;
            }

            private static bool TrySpells(Vector3 pos, int level)
            {
                var spells = WhereDidHeGo.AntiStealthSpells.Where(p => p.ChampionName == ObjectManager.Player.ChampionName.ToLower());
                foreach (var spell in spells)
                {
                    if (InitializeMenu.Menu.Item(String.Format("USE{0}", spell.Spell.ToString())).GetValue<bool>() &&
                        InitializeMenu.Menu.Item(String.Format("DETECT{0}", spell.Spell.ToString())).GetValue<Slider>().Value >= level)
                    {
                        if (spell.Spell.IsReady())
                        {
                            if (spell.SelfCast)
                                ObjectManager.Player.Spellbook.CastSpell(spell.Spell);
                            else
                            {
                                if (!ObjectManager.Player.HasBuff("rengarralertsound") && pos.Distance(ObjectManager.Player.ServerPosition) <= spell.SpellRange)
                                    ObjectManager.Player.Spellbook.CastSpell(spell.Spell, pos);
                            }
                            return true;
                        }
                    }
                }

                return false;
            }
        }
    }
}
