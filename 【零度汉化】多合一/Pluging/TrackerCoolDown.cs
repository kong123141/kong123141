namespace Pluging
{
    using System;
    using System.Collections;
    using LeagueSharp;
    using LeagueSharp.Common;
    using System.Linq;
    using SharpDX;
    using System.Drawing;
    using System.Collections.Generic;
    using Flowers_Utility.Properties;
    using SharpDX.Direct3D9;
    public class TrackerCoolDown
    {
        public static SharpDX.Direct3D9.Font Tahoma_13;
        public static Menu Menu;
        public static readonly SpellSlot[] Summoners = { SpellSlot.Summoner1, SpellSlot.Summoner2 };
        public static readonly SpellSlot[] SpellSlots = { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
        public static Bitmap Tracker_ExpHud = Resources.hud_exp;
        public static Bitmap Tracker_VanillaHud = Resources.hud_vanilla;
        private static float LastTick;
        private static List<PlayerWrapper> playersWrapper = new List<PlayerWrapper>();
        private static List<TrackerWrapper> TrackerWrappers = new List<TrackerWrapper>();
        public static List<HiddenObj> HiddenObjList = new List<HiddenObj>();
        public static Dictionary<string, Bitmap> summonerSpells = new Dictionary<string, Bitmap>()
        {
            { "itemsmiteaoe", Resources.itemsmiteaoe },
            { "s5_summonersmiteduel", Resources.s5_summonersmiteduel },
            { "s5_summonersmiteplayerganker", Resources.s5_summonersmiteplayerganker },
            { "s5_summonersmitequick", Resources.s5_summonersmitequick },
            { "snowballfollowupcast", Resources.snowballfollowupcast },
            { "summonerbarrier", Resources.summonerbarrier },
            { "summonerboost", Resources.summonerboost },
            { "summonerclairvoyance", Resources.summonerclairvoyance },
            { "summonerdot", Resources.summonerdot },
            { "summonerexhaust", Resources.summonerexhaust },
            { "summonerflash", Resources.summonerflash },
            { "summonerhaste", Resources.summonerhaste },
            { "summonerheal", Resources.summonerheal },
            { "summonermana", Resources.summonermana },
            { "summonerodingarrison", Resources.summonerodingarrison },
            { "summonerpororecall", Resources.summonerpororecall },
            { "summonerporothrow", Resources.summonerporothrow },
            { "summonerrevive", Resources.summonerrevive },
            { "summonersmite", Resources.summonersmite },
            { "summonersnowball", Resources.summonersnowball },
            { "summonerteleport", Resources.summonerteleport },
        };

        public static bool TrackEnemies => Menu.Item("TrackEnemies").GetValue<bool>();

        public static bool TrackAllies => Menu.Item("TrackAllies").GetValue<bool>();

        public static Bitmap TrackerHud => Tracker_VanillaHud;

        public TrackerCoolDown(Menu mainMenu)
        {
            Menu = mainMenu;

            Menu TrackerCoolDownMenu = new Menu("[FL] 眼位CD监控", "NightMoon");
            TrackerCoolDownMenu.AddItem(new MenuItem("EnableTrack", "启动CD监控").SetValue(true));
            TrackerCoolDownMenu.AddItem(new MenuItem("TrackAllies", "监测友军CD").SetValue(true));
            TrackerCoolDownMenu.AddItem(new MenuItem("TrackEnemies", "监测敌人CD").SetValue(true));
            TrackerCoolDownMenu.AddItem(new MenuItem("EnableTrackWard", "启动眼位监控").SetValue(true));

            Menu.AddSubMenu(TrackerCoolDownMenu);

            Tahoma_13 = new SharpDX.Direct3D9.Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 14, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });

            LoadList();
            LoadSprites();

            Game.OnUpdate += OnUpdate;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += OnEndScene;
        }

        private void OnEndScene(EventArgs args)
        {
            try
            {
                if (!Menu.Item("EnableTrackWard").GetValue<bool>())
                    return;

                foreach (var obj in HiddenObjList)
                {
                    if (obj.type == 1)
                    {
                        Utility.DrawCircle(obj.pos, 100, System.Drawing.Color.Yellow, 3, 20, true);
                    }

                    if (obj.type == 2)
                    {
                        Utility.DrawCircle(obj.pos, 100, System.Drawing.Color.HotPink, 3, 20, true);
                    }

                    if (obj.type == 3)
                    {
                        Utility.DrawCircle(obj.pos, 100, System.Drawing.Color.Orange, 3, 20, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Tracker.OnEndScene + " + ex);
            }
        }

        private void OnDraw(EventArgs args)
        {
            try
            {
                if (!Menu.Item("EnableTrackWard").GetValue<bool>())
                    return;

                var circleSize = 30;

                foreach (var obj in HiddenObjList)
                {
                    if (obj.type == 1)
                    {
                        DravTriangle(circleSize, obj.pos, System.Drawing.Color.Yellow);
                        DrawFontTextMap(Tahoma_13, "" + (int)(obj.endTime - Game.Time), obj.pos, SharpDX.Color.Yellow);
                    }

                    if (obj.type == 2)
                    {
                        DravTriangle(circleSize, obj.pos, System.Drawing.Color.HotPink);
                        DrawFontTextMap(Tahoma_13, "VW", obj.pos, SharpDX.Color.HotPink);
                    }
                    if (obj.type == 3)
                    {
                        DravTriangle(circleSize, obj.pos, System.Drawing.Color.Orange);
                        DrawFontTextMap(Tahoma_13, "! " + (int)(obj.endTime - Game.Time), obj.pos, SharpDX.Color.Orange);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Tracker.OnDraw + " + ex);
            }
        }

        private void OnDelete(GameObject sender, EventArgs args)
        {
            try
            {
                if (!sender.IsEnemy || sender.IsAlly || sender.Type != GameObjectType.obj_AI_Minion)
                    return;

                foreach (var obj in HiddenObjList)
                {
                    if (obj.pos == sender.Position)
                    {
                        HiddenObjList.Remove(obj);
                        return;
                    }
                    else if (obj.type == 3 && obj.pos.Distance(sender.Position) < 100)
                    {
                        HiddenObjList.Remove(obj);
                        return;
                    }
                    else if (obj.pos.Distance(sender.Position) < 400)
                    {
                        if (obj.type == 2 && sender.Name.ToLower() == "visionward")
                        {
                            HiddenObjList.Remove(obj);
                            return;
                        }
                        else if ((obj.type == 0 || obj.type == 1) && sender.Name.ToLower() == "sightward")
                        {
                            HiddenObjList.Remove(obj);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Tracker.OnDelete + " + ex);
            }
        }

        private void OnCreate(GameObject sender, EventArgs args)
        {
            try
            {
                if (!sender.IsEnemy || sender.IsAlly)
                    return;

                if (sender is MissileClient)
                {
                    var missile = (MissileClient)sender;

                    if (!missile.SpellCaster.IsVisible)
                    {
                        if ((missile.SData.Name == "BantamTrapShort" || missile.SData.Name == "BantamTrapBounceSpell") && !HiddenObjList.Exists(x => missile.EndPosition == x.pos))
                            AddWard("teemorcast", missile.EndPosition);
                    }
                }

                if (sender.Type == GameObjectType.obj_AI_Minion && (sender.Name.ToLower() == "visionward" || sender.Name.ToLower() == "sightward") && !HiddenObjList.Exists(x => x.pos.Distance(sender.Position) < 100))
                {
                    foreach (var obj in HiddenObjList)
                    {
                        if (obj.pos.Distance(sender.Position) < 400)
                        {
                            if (obj.type == 0)
                            {
                                HiddenObjList.Remove(obj);
                                return;
                            }
                        }
                    }

                    var dupa = (Obj_AI_Minion)sender;

                    if (dupa.Mana == 0)
                        HiddenObjList.Add(new HiddenObj() { type = 2, pos = sender.Position, endTime = float.MaxValue });
                    else
                        HiddenObjList.Add(new HiddenObj() { type = 1, pos = sender.Position, endTime = Game.Time + dupa.Mana });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Tracker.OnCreate + " + ex);
            }
        }

        private void CurrentDomain(object sender, EventArgs e)
        {
            try
            {
                foreach (var w in TrackerWrappers)
                {
                    w.Hud.Remove();

                    w.Summoner1.Remove();
                    w.Summoner2.Remove();

                    w.SummonerSpell1Rectangle.Remove();
                    w.SummonerSpell2Rectangle.Remove();

                    w.SummonerSpell1Text.Remove();
                    w.SummonerSpell2Text.Remove();

                    w.Spell1Rectangle.Remove();
                    w.Spell1Text.Remove();

                    w.Spell2Rectangle.Remove();
                    w.Spell2Text.Remove();

                    w.Spell3Rectangle.Remove();
                    w.Spell3Text.Remove();

                    w.Spell4Rectangle.Remove();
                    w.Spell4Text.Remove();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in TrackerCoolDown.ProcessExit + " + ex);
            }
        }

        private void OnUpdate(EventArgs args)
        {
            try
            {
                if (Environment.TickCount - LastTick < 250)
                {
                    return;
                }

                foreach (var w in TrackerWrappers)
                {
                    var hero = w.Hero;

                    if ((hero.IsAlly || hero.IsMe) && !TrackAllies)
                    {
                        continue;
                    }

                    if (hero.IsEnemy && !TrackEnemies)
                    {
                        continue;
                    }

                    foreach (var summonerSpell in Summoners)
                    {
                        var spellInstance = hero.Spellbook.GetSpell(summonerSpell);
                        var spellCooldown = spellInstance.CooldownExpires - Game.Time;

                        var widthDef = 13;
                        var widthCd = spellCooldown > 0 ? widthDef * (spellCooldown / spellInstance.Cooldown) : 0;

                        switch (summonerSpell)
                        {
                            case SpellSlot.Summoner1:
                                if (w.SummonerSpell1Rectangle != null)
                                {
                                    w.SummonerSpell1Rectangle.Visible = (int)widthCd != 0;
                                    w.SummonerSpell1Rectangle.Width = (int)widthCd;
                                }
                                break;
                            case SpellSlot.Summoner2:
                                if (w.SummonerSpell2Rectangle != null)
                                {
                                    w.SummonerSpell2Rectangle.Visible = (int)widthCd != 0;
                                    w.SummonerSpell2Rectangle.Width = (int)widthCd;
                                }
                                break;
                        }
                    }

                    foreach (var spell in SpellSlots)
                    {
                        var spellInstance = hero.Spellbook.GetSpell(spell);
                        var spellCooldown = spellInstance.CooldownExpires - Game.Time;
                        var widthDef = 23;
                        var widthCd = spellCooldown > 0 ? widthDef * (spellCooldown / spellInstance.Cooldown) : 0;

                        if (spellInstance.Level == 0)
                        {
                            widthCd = widthDef;
                        }

                        switch (spell)
                        {
                            case SpellSlot.Q:
                                if (w.Spell1Rectangle != null)
                                {
                                    w.Spell1Rectangle.Visible = (int)widthCd != 0;
                                    w.Spell1Rectangle.Crop(0, 0, (int)widthCd, Resources.CooldownSprite.Height);
                                }
                                break;
                            case SpellSlot.W:
                                if (w.Spell2Rectangle != null)
                                {
                                    w.Spell2Rectangle.Visible = (int)widthCd != 0;
                                    w.Spell2Rectangle.Crop(0, 0, (int)widthCd, Resources.CooldownSprite.Height);
                                }
                                break;
                            case SpellSlot.E:
                                if (w.Spell3Rectangle != null)
                                {
                                    w.Spell3Rectangle.Visible = (int)widthCd != 0;
                                    w.Spell3Rectangle.Crop(0, 0, (int)widthCd, Resources.CooldownSprite.Height);
                                }
                                break;
                            case SpellSlot.R:
                                if (w.Spell4Rectangle != null)
                                {
                                    w.Spell4Rectangle.Visible = (int)widthCd != 0;
                                    w.Spell4Rectangle.Crop(0, 0, (int)widthCd, Resources.CooldownSprite.Height);
                                }
                                break;
                        }
                    }
                }

                foreach (var obj in HiddenObjList)
                {
                    if (obj.endTime < Game.Time)
                    {
                        HiddenObjList.Remove(obj);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in TrackerCoolDown.OnUpdate + " + ex);
            }
        }

        private void LoadList()
        {
            try
            {
                foreach (var player in HeroManager.AllHeroes)
                {
                    var member = new PlayerWrapper
                    {
                        Hero = player, Summoners = new Tuple<string, string>(player.Spellbook.GetSpell(SpellSlot.Summoner1).Name, player.Spellbook.GetSpell(SpellSlot.Summoner2).Name)
                    };

                    playersWrapper.Add(member);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in TrackerCoolDown.LoadList + " + ex);
            }
        }

        private void LoadSprites()
        {
            try
            {
                foreach (var player in ObjectManager.Get<Obj_AI_Hero>().Where(h => !h.IsMe))
                {
                    var player_Ex = player;

                    var Summoner1Bitmap = summonerSpells[player_Ex.Spellbook.GetSpell(SpellSlot.Summoner1).Name];
                    var Summoner2Bitmap = summonerSpells[player_Ex.Spellbook.GetSpell(SpellSlot.Summoner2).Name];

                    Render.Sprite SummonerSpell1 = new Render.Sprite(Resources.empty, new Vector2());
                    Render.Sprite SummonerSpell2 = new Render.Sprite(Resources.empty, new Vector2());

                    var member = new TrackerWrapper();

                    var scale = 0.94f;
                    var offset = 8 * scale;
                    var offsetX = 0;

                    var Hudsprite = new Render.Sprite(TrackerHud, new Vector2(0, 0))
                    {
                        PositionUpdate = ()  => new Vector2(player_Ex.HPBarPosition.X - 14 * scale, player_Ex.HPBarPosition.Y + offset + 6 * scale),

                        VisibleCondition = sender  => player_Ex.IsHPBarRendered && (((player_Ex.IsAlly || player_Ex.IsMe) && TrackAllies) || (player_Ex.IsEnemy && TrackEnemies)), Scale = new Vector2(1.0f, 1.0f)
                    };

                    Hudsprite.Add(0);

                    if (Summoner1Bitmap != null)
                    {
                        SummonerSpell1 = new Render.Sprite(Summoner1Bitmap, new Vector2(0, 0))
                        {
                            PositionUpdate = () => new Vector2(player_Ex.HPBarPosition.X - 8 * scale, player_Ex.HPBarPosition.Y + offset + 8 * scale), VisibleCondition = sender => player_Ex.IsHPBarRendered && (((player_Ex.IsAlly || player_Ex.IsMe) && TrackAllies) || (player_Ex.IsEnemy && TrackEnemies)), Scale = new Vector2(1.0f, 1.0f)
                        };

                        SummonerSpell1.Add(0);

                        member.Summoner1 = SummonerSpell1;

                        var Summoner1Rectangle = new Render.Rectangle((int)player_Ex.HPBarPosition.X - 7, (int)player_Ex.HPBarPosition.Y + 8, 13, 13, new ColorBGRA(0, 0, 0, 175))
                        {
                            VisibleCondition = sender => player_Ex.IsHPBarRendered && (((player_Ex.IsAlly || player_Ex.IsMe) && TrackAllies) || (player_Ex.IsEnemy && TrackEnemies)), PositionUpdate = () => new Vector2((int)player_Ex.HPBarPosition.X - 7 * scale, (int)player_Ex.HPBarPosition.Y + offset + 8 * scale),
                        };

                        Summoner1Rectangle.Add(0);

                        member.SummonerSpell1Rectangle = Summoner1Rectangle;

                        var spellCooldown = player_Ex.Spellbook.GetSpell(SpellSlot.Summoner1).CooldownExpires - Game.Time;

                        var Summoner1Text = new Render.Text((int)player_Ex.HPBarPosition.X - 31, (int)player_Ex.HPBarPosition.Y + 6, ((int)spellCooldown).ToString(), 14, new ColorBGRA(255, 255, 255, 255))
                        {
                            TextUpdate = () => ((int)(player_Ex.Spellbook.GetSpell(SpellSlot.Summoner1).CooldownExpires - Game.Time) > 0 ? ((int)(player_Ex.Spellbook.GetSpell(SpellSlot.Summoner1).CooldownExpires - Game.Time)).ToString() : string.Empty), PositionUpdate = () => new Vector2((int)player_Ex.HPBarPosition.X - 31 * scale, (int)player_Ex.HPBarPosition.Y + offset + 6 * scale), VisibleCondition = sender => Menu.Item("EnableTrack").GetValue<bool>() && player_Ex.IsHPBarRendered && (player_Ex.Spellbook.GetSpell(SpellSlot.Summoner1).CooldownExpires - Game.Time) > 0 && (((player_Ex.IsAlly || player_Ex.IsMe) && TrackAllies) || (player_Ex.IsEnemy && TrackEnemies))
                        };

                        Summoner1Text.Add(0);

                        member.SummonerSpell1Text = Summoner1Text;
                    }

                    if (Summoner2Bitmap != null)
                    {
                        SummonerSpell2 = new Render.Sprite(Summoner2Bitmap, new Vector2(0, 0))
                        {
                            PositionUpdate = () => new Vector2(player_Ex.HPBarPosition.X - 8 * scale, player_Ex.HPBarPosition.Y + offset + 25 * scale), VisibleCondition = sender => player_Ex.IsHPBarRendered && (((player_Ex.IsAlly || player_Ex.IsMe) && TrackAllies) || (player_Ex.IsEnemy && TrackEnemies)), Scale = new Vector2(1.0f, 1.0f)
                        };

                        SummonerSpell2.Add(0);

                        member.Summoner2 = SummonerSpell2;

                        var Summoner2Rectangle = new Render.Rectangle((int)player_Ex.HPBarPosition.X - 7, (int)player_Ex.HPBarPosition.Y + 26, 13, 13, new ColorBGRA(0, 0, 0, 175))
                        {
                            VisibleCondition = sender => player_Ex.IsHPBarRendered && (((player_Ex.IsAlly || player_Ex.IsMe) && TrackAllies) || (player_Ex.IsEnemy && TrackEnemies)), PositionUpdate = () => new Vector2((int)player_Ex.HPBarPosition.X - 7 * scale, (int)player_Ex.HPBarPosition.Y + offset + 26 * scale),
                        };

                        Summoner2Rectangle.Add(0);

                        var spellCooldown = player_Ex.Spellbook.GetSpell(SpellSlot.Summoner2).CooldownExpires - Game.Time;

                        var Summoner2Text = new Render.Text((int)player_Ex.HPBarPosition.X - 31, (int)player_Ex.HPBarPosition.Y + 24, ((int)spellCooldown).ToString(), 14, new ColorBGRA(255, 255, 255, 255))
                        {
                            TextUpdate = () => ((int)(player_Ex.Spellbook.GetSpell(SpellSlot.Summoner2).CooldownExpires - Game.Time) > 0 ? ((int)(player_Ex.Spellbook.GetSpell(SpellSlot.Summoner2).CooldownExpires - Game.Time)).ToString() : string.Empty), PositionUpdate = () => new Vector2((int)player_Ex.HPBarPosition.X - 31 * scale, (int)player_Ex.HPBarPosition.Y + offset + 24 * scale), VisibleCondition = sender => Menu.Item("EnableTrack").GetValue<bool>() && player_Ex.IsHPBarRendered && (player_Ex.Spellbook.GetSpell(SpellSlot.Summoner2).CooldownExpires - Game.Time) > 0 && (((player_Ex.IsAlly || player_Ex.IsMe) && TrackAllies) || (player_Ex.IsEnemy && TrackEnemies))
                        };

                        Summoner2Text.Add(0);

                        member.SummonerSpell2Text = Summoner2Text;
                        member.SummonerSpell2Rectangle = Summoner2Rectangle;
                    }

                    var slot1 = SpellSlot.Q;

                    var Spell1Rectangle = new Render.Sprite(Resources.CooldownSprite, new Vector2((int)player_Ex.HPBarPosition.X + 13, (int)player_Ex.HPBarPosition.Y + 30))
                    {
                        VisibleCondition = sender => player_Ex.IsHPBarRendered && (((player_Ex.IsAlly || player_Ex.IsMe) && TrackAllies) || (player_Ex.IsEnemy && TrackEnemies)), PositionUpdate = () => new Vector2(player_Ex.HPBarPosition.X + offsetX + 13.2f * scale, (int)player_Ex.HPBarPosition.Y + offset + 28 * scale),
                    };

                    member.Spell1Rectangle = Spell1Rectangle;

                    Spell1Rectangle.Add(0);

                    var Spell1Text = new Render.Text((int)player_Ex.HPBarPosition.X + 16, (int)player_Ex.HPBarPosition.Y + 33, string.Empty, 14, new ColorBGRA(255, 255, 255, 255))
                    {
                        TextUpdate = () => ((player_Ex.Spellbook.GetSpell(slot1).CooldownExpires - Game.Time) > 0 ? (Truncate((player_Ex.Spellbook.GetSpell(slot1).CooldownExpires - Game.Time))) : string.Empty), PositionUpdate = () => new Vector2((int)player_Ex.HPBarPosition.X + offsetX + 16 * scale, (int)player_Ex.HPBarPosition.Y + offset + 33 * scale), VisibleCondition = sender => Menu.Item("EnableTrack").GetValue<bool>() && player_Ex.IsHPBarRendered && (player_Ex.Spellbook.GetSpell(slot1).CooldownExpires - Game.Time) > 0 && (((player_Ex.IsAlly || player_Ex.IsMe) && TrackAllies) || (player_Ex.IsEnemy && TrackEnemies))
                    };

                    member.Spell1Text = Spell1Text;

                    Spell1Text.Add(0);

                    var slot2 = SpellSlot.W;

                    var Spell2Rectangle = new Render.Sprite(Resources.CooldownSprite, new Vector2((int)player_Ex.HPBarPosition.X + 41, (int)player_Ex.HPBarPosition.Y + 30))
                    {
                        VisibleCondition = sender => player_Ex.IsHPBarRendered && (((player_Ex.IsAlly || player_Ex.IsMe) && TrackAllies) || (player_Ex.IsEnemy && TrackEnemies)), PositionUpdate = () => new Vector2(player_Ex.HPBarPosition.X + offsetX + 41f * scale, (int)player_Ex.HPBarPosition.Y + offset + 28 * scale),
                    };

                    member.Spell2Rectangle = Spell2Rectangle;

                    Spell2Rectangle.Add(0);

                    var Spell2Text = new Render.Text((int)player_Ex.HPBarPosition.X + 44, (int)player_Ex.HPBarPosition.Y + 33, string.Empty, 14, new ColorBGRA(255, 255, 255, 255))
                    {
                        TextUpdate = () => ((player_Ex.Spellbook.GetSpell(slot2).CooldownExpires - Game.Time) > 0 ? (Truncate((player_Ex.Spellbook.GetSpell(slot2).CooldownExpires - Game.Time))) : string.Empty), PositionUpdate = () => new Vector2((int)player_Ex.HPBarPosition.X + offsetX + 43.8f * scale, (int)player_Ex.HPBarPosition.Y + offset + 33 * scale), VisibleCondition = sender => Menu.Item("EnableTrack").GetValue<bool>() && player_Ex.IsHPBarRendered && (player_Ex.Spellbook.GetSpell(slot2).CooldownExpires - Game.Time) > 0 && (((player_Ex.IsAlly || player_Ex.IsMe) && TrackAllies) || (player_Ex.IsEnemy && TrackEnemies))
                    };

                    member.Spell2Text = Spell2Text;

                    Spell2Text.Add(0);

                    var slot3 = SpellSlot.E;

                    var Spell3Rectangle = new Render.Sprite(Resources.CooldownSprite, new Vector2((int)player_Ex.HPBarPosition.X + 41, (int)player_Ex.HPBarPosition.Y + 30))
                    {
                        VisibleCondition = sender => player_Ex.IsHPBarRendered && (((player_Ex.IsAlly || player_Ex.IsMe) && TrackAllies) || (player_Ex.IsEnemy && TrackEnemies)), PositionUpdate = () => new Vector2(player_Ex.HPBarPosition.X + offsetX + 69f * scale, (int)player_Ex.HPBarPosition.Y + offset + 28 * scale),
                    };

                    member.Spell3Rectangle = Spell3Rectangle;

                    Spell3Rectangle.Add(0);

                    var Spell3Text = new Render.Text((int)player_Ex.HPBarPosition.X + 44, (int)player_Ex.HPBarPosition.Y + 33, string.Empty, 14, new ColorBGRA(255, 255, 255, 255))
                    {
                        TextUpdate = () => ((player_Ex.Spellbook.GetSpell(slot3).CooldownExpires - Game.Time) > 0 ? (Truncate((player_Ex.Spellbook.GetSpell(slot3).CooldownExpires - Game.Time))) : string.Empty), PositionUpdate = () => new Vector2((int)player_Ex.HPBarPosition.X + offsetX + 73.8f * scale, (int)player_Ex.HPBarPosition.Y + offset + 33 * scale), VisibleCondition = sender => Menu.Item("EnableTrack").GetValue<bool>() && player_Ex.IsHPBarRendered && (player_Ex.Spellbook.GetSpell(slot3).CooldownExpires - Game.Time) > 0 && (((player_Ex.IsAlly || player_Ex.IsMe) && TrackAllies) || (player_Ex.IsEnemy && TrackEnemies))
                    };

                    member.Spell3Text = Spell3Text;

                    Spell3Text.Add(0);

                    var slot4 = SpellSlot.R;

                    var Spell4Rectangle = new Render.Sprite(Resources.CooldownSprite, new Vector2((int)player_Ex.HPBarPosition.X + 41, (int)player_Ex.HPBarPosition.Y + 30))
                    {
                        VisibleCondition = sender => player_Ex.IsHPBarRendered && (((player_Ex.IsAlly || player_Ex.IsMe) && TrackAllies) || (player_Ex.IsEnemy && TrackEnemies)), PositionUpdate = () => new Vector2(player_Ex.HPBarPosition.X + offsetX + 96f * scale, (int)player_Ex.HPBarPosition.Y + offset + 28 * scale),
                    };

                    member.Spell4Rectangle = Spell4Rectangle;

                    Spell4Rectangle.Add(0);

                    var Spell4Text = new Render.Text((int)player_Ex.HPBarPosition.X + 44, (int)player_Ex.HPBarPosition.Y + 33, string.Empty, 14, new ColorBGRA(255, 255, 255, 255))
                    {
                        TextUpdate = () => ((player_Ex.Spellbook.GetSpell(slot4).CooldownExpires - Game.Time) > 0 ? (Truncate((player_Ex.Spellbook.GetSpell(slot4).CooldownExpires - Game.Time))) : string.Empty), PositionUpdate = () => new Vector2((int)player_Ex.HPBarPosition.X + offsetX + 101f * scale, (int)player_Ex.HPBarPosition.Y + offset + 33 * scale), VisibleCondition = sender => Menu.Item("EnableTrack").GetValue<bool>() && player_Ex.IsHPBarRendered && (player_Ex.Spellbook.GetSpell(slot4).CooldownExpires - Game.Time) > 0 && (((player_Ex.IsAlly || player_Ex.IsMe) && TrackAllies) || (player_Ex.IsEnemy && TrackEnemies))
                    };

                    member.Spell4Text = Spell4Text;

                    Spell4Text.Add(0);

                    member.Hero = player_Ex;
                    member.Hud = Hudsprite;
                    member.Summoner1 = SummonerSpell1;
                    member.Summoner2 = SummonerSpell2;

                    TrackerWrappers.Add(member);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in TrackerCoolDown.LoadSprtes + " + ex);
            }
        }

        private static float GetXPPerLevel(int level)
        {
            return 80 + 100 * (level);
        }

        private static string Truncate(float s)
        {
            var s2 = Math.Ceiling(s).ToString();
            return s2;
        }

        public static void DravTriangle(float radius, Vector3 position, System.Drawing.Color color, float bold = 1)
        {
            var positionV2 = Drawing.WorldToScreen(position);

            Vector2 a = new Vector2(positionV2.X + radius, positionV2.Y + radius / 2);
            Vector2 b = new Vector2(positionV2.X - radius, positionV2.Y + radius / 2);
            Vector2 c = new Vector2(positionV2.X, positionV2.Y - radius);

            Drawing.DrawLine(a[0], a[1], b[0], b[1], bold, color);
            Drawing.DrawLine(b[0], b[1], c[0], c[1], bold, color);
            Drawing.DrawLine(c[0], c[1], a[0], a[1], bold, color);
        }

        private void AddWard(string name, Vector3 posCast)
        {
            switch (name)
            {
                //PINKS
                case "visionward":
                    HiddenObjList.Add(new HiddenObj() { type = 2, pos = posCast, endTime = float.MaxValue });
                    break;
                case "trinkettotemlvl3B":
                    HiddenObjList.Add(new HiddenObj() { type = 1, pos = posCast, endTime = Game.Time + 180 });
                    break;
                //SIGH WARD
                case "itemghostward":
                    HiddenObjList.Add(new HiddenObj() { type = 1, pos = posCast, endTime = Game.Time + 180 });
                    break;
                case "wrigglelantern":
                    HiddenObjList.Add(new HiddenObj() { type = 1, pos = posCast, endTime = Game.Time + 180 });
                    break;
                case "sightward":
                    HiddenObjList.Add(new HiddenObj() { type = 1, pos = posCast, endTime = Game.Time + 180 });
                    break;
                case "itemferalflare":
                    HiddenObjList.Add(new HiddenObj() { type = 1, pos = posCast, endTime = Game.Time + 180 });
                    break;
                //TRINKET
                case "trinkettotemlvl1":
                    HiddenObjList.Add(new HiddenObj() { type = 1, pos = posCast, endTime = Game.Time + 60 });
                    break;
                case "trinkettotemlvl2":
                    HiddenObjList.Add(new HiddenObj() { type = 1, pos = posCast, endTime = Game.Time + 120 });
                    break;
                case "trinkettotemlvl3":
                    HiddenObjList.Add(new HiddenObj() { type = 1, pos = posCast, endTime = Game.Time + 180 });
                    break;
                //others
                case "teemorcast":
                    HiddenObjList.Add(new HiddenObj() { type = 3, pos = posCast, endTime = Game.Time + 300 });
                    break;
                case "noxious trap":
                    HiddenObjList.Add(new HiddenObj() { type = 3, pos = posCast, endTime = Game.Time + 300 });
                    break;
                case "JackInTheBox":
                    HiddenObjList.Add(new HiddenObj() { type = 3, pos = posCast, endTime = Game.Time + 100 });
                    break;
                case "Jack In The Box":
                    HiddenObjList.Add(new HiddenObj() { type = 3, pos = posCast, endTime = Game.Time + 100 });
                    break;
            }
        }

        public static void DrawFontTextMap(SharpDX.Direct3D9.Font vFont, string vText, Vector3 Pos, ColorBGRA vColor)
        {
            var wts = Drawing.WorldToScreen(Pos);
            vFont.DrawText(null, vText, (int)wts[0], (int)wts[1], vColor);
        }
    }

    public class HiddenObj
    {
        public int type;
        //0 - missile
        //1 - normal
        //2 - pink
        //3 - teemo trap
        public float endTime { get; set; }
        public Vector3 pos { get; set; }
    }

    internal class TrackerWrapper
    {
        public Obj_AI_Hero Hero { get; set; }

        public Render.Sprite Hud { get; set; }

        public Render.Sprite Summoner1 { get; set; }

        public Render.Sprite Summoner2 { get; set; }

        public Render.Sprite Spell1Rectangle { get; set; }

        public Render.Sprite Spell2Rectangle { get; set; }

        public Render.Sprite Spell3Rectangle { get; set; }

        public Render.Sprite Spell4Rectangle { get; set; }

        public Render.Rectangle SummonerSpell1Rectangle { get; set; }

        public Render.Rectangle SummonerSpell2Rectangle { get; set; }

        public Render.Text SummonerSpell1Text { get; set; }

        public Render.Text SummonerSpell2Text { get; set; }

        public Render.Text Spell1Text { get; set; }

        public Render.Text Spell2Text { get; set; }

        public Render.Text Spell3Text { get; set; }

        public Render.Text Spell4Text { get; set; }
    }

    internal class PlayerWrapper
    {
        public Obj_AI_Hero Hero { get; set; }

        public Tuple<string, string> Summoners { get; set; }
    }
}