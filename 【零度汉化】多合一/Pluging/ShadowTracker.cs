namespace Pluging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using System.Drawing;
    using Color = System.Drawing.Color;

    public class ShadowTracker
    {
        public static Menu Menu;
        public static Obj_AI_Hero Player;
        public static int EnemyCnt;
        public static List<MovingTrackInfomation> TrackInfomationList = new List<MovingTrackInfomation>();
        public static List<StopTrackInfomation> StopTrackInfomationList = new List<StopTrackInfomation>();
        public static List<UsingTrackInfomation> UsingItemInfomationList = new List<UsingTrackInfomation>();
        public static MovingSkillInfomation[] MovingSkillInfoList = new MovingSkillInfomation[]
        {
            new MovingSkillInfomation { SpellName = "summonerflash", MaxRange = 450f },
            new MovingSkillInfomation { SpellName = "EzrealArcaneShift", MaxRange = 450f },
            new MovingSkillInfomation { SpellName = "Deceive", MaxRange = 450f }
        };
        public static StopSkillInfomation[] StopSkillInfoList = new StopSkillInfomation[]
        {
            new StopSkillInfomation { SpellName = "MonkeyKingDecoy", ExpireTime=1500 },
            new StopSkillInfomation { SpellName = "AkaliSmokeBomb", ExpireTime=8000 },
            new StopSkillInfomation { SpellName = "summonerteleport", ExpireTime=3500 }
        };
        public static PetSkillInfomation[] PetSkillInfoList = new PetSkillInfomation[]
        {
            new PetSkillInfomation { ChampionName = "Leblanc", ExpireTime = 8000 },
            new PetSkillInfomation { ChampionName = "Shaco", ExpireTime = 18000 },
            new PetSkillInfomation { ChampionName = "Mordekaiser", ExpireTime = 30000 }
        };
        public static UsingItemInfomation[] UsingItemInfoList = new UsingItemInfomation[]
        {
            new UsingItemInfomation { ItemName = "ZhonyasHourglass", ExpireTime = 2500 }
        };

        public ShadowTracker(Menu mainMenu)
        {
            Player = ObjectManager.Player;
            EnemyCnt = HeroManager.Enemies.Count;

            Menu = mainMenu;

            Menu ShadowTrackerMenu = new Menu("[FL] 真身显示", "ShadowTracker");

            ShadowTrackerMenu.AddItem(new MenuItem("Skill", "Skill").SetValue(true));
            ShadowTrackerMenu.AddItem(new MenuItem("Spell", "Spell").SetValue(true));
            ShadowTrackerMenu.AddItem(new MenuItem("Item", "Item").SetValue(true));

            Menu.AddSubMenu(ShadowTrackerMenu);

            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Drawing.OnDraw += OnDraw;
        }

        private void OnDraw(EventArgs args)
        {
            try
            {
                foreach (var item in TrackInfomationList.Where(x => x.InfoType == InfoType.MovingSkill && x.ExpireTime > Environment.TickCount))
                {
                    Render.Circle.DrawCircle(item.EndPosition, 30, Color.YellowGreen);

                    var StartScreenPos = Drawing.WorldToScreen(item.StartPosition);
                    var EndScreenPos = Drawing.WorldToScreen(item.EndPosition);

                    Drawing.DrawText(EndScreenPos.X, EndScreenPos.Y, Color.LightYellow, item.Sender.CharData.BaseSkinName);
                    Drawing.DrawLine(StartScreenPos, EndScreenPos, 2, Color.YellowGreen);
                }

                foreach (var item in StopTrackInfomationList.Where(x => x.InfoType == InfoType.StopSkill && x.ExpireTime > Environment.TickCount))
                {
                    Render.Circle.DrawCircle(item.Sender.Position, 100, Color.YellowGreen);

                    var TextPosition = Drawing.WorldToScreen(item.CastPosition);

                    Drawing.DrawText(TextPosition.X, TextPosition.Y, Color.LightYellow, item.Sender.CharData.BaseSkinName);
                    Drawing.DrawText(TextPosition.X - 20, TextPosition.Y + 15, Color.LawnGreen, (item.ExpireTime - Environment.TickCount).ToString());
                }

                foreach (var item in UsingItemInfomationList.Where(x => x.InfoType == InfoType.UsingItem && x.ExpireTime > Environment.TickCount))
                {
                    var TextPosition = Drawing.WorldToScreen(item.Sender.ServerPosition);

                    Drawing.DrawText(TextPosition.X - 20, TextPosition.Y + 15, Color.LawnGreen, (item.ExpireTime - Environment.TickCount).ToString());
                }

                foreach (var item in PetSkillInfoList)
                {
                    var enemy = HeroManager.Enemies.Find(x => x.ChampionName == item.ChampionName);

                    if (enemy != null && enemy.Pet != null && !enemy.Pet.IsDead)
                    {
                        Render.Circle.DrawCircle(enemy.Position, 50, Color.Red);

                        var TextPosition = Drawing.WorldToScreen(enemy.Position);

                        Drawing.DrawText(TextPosition.X - 20, TextPosition.Y + 30, Color.LawnGreen, "Here");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in ShadowTracker.OnDraw + " + ex);
            }
        }

        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            try
            {
                if (sender.IsEnemy)
                {
                    var MovingSkillInfo = MovingSkillInfoList.FirstOrDefault(x => x.SpellName == args.SData.Name);
                    var StopSkillInfo = StopSkillInfoList.FirstOrDefault(x => x.SpellName == args.SData.Name);
                    var UsingItemInfo = UsingItemInfoList.FirstOrDefault(x => x.ItemName == args.SData.Name);

                    if (MovingSkillInfo != null)
                    {
                        TrackInfomationList.Add(new MovingTrackInfomation
                        {
                            InfoType = InfoType.MovingSkill,
                            CastTime = Environment.TickCount,
                            Sender = sender,
                            ExpireTime = TickCount(3000),
                            StartPosition = args.Start,
                            EndPosition = args.Start.Extend(args.End, MovingSkillInfo.MaxRange)
                        });
                    }

                    if (StopSkillInfo != null)
                    {
                        Game.ShowPing(PingCategory.Danger, sender.Position);

                        StopTrackInfomationList.Add(new StopTrackInfomation
                        {
                            InfoType = InfoType.StopSkill,
                            Sender = sender,
                            CastPosition = args.End,
                            ExpireTime = TickCount(StopSkillInfo.ExpireTime)
                        });
                    }

                    if (UsingItemInfo != null)
                    {
                        UsingItemInfomationList.Add(new UsingTrackInfomation
                        {
                            InfoType = InfoType.UsingItem,
                            Sender = sender,
                            CastTime = Environment.TickCount,
                            ExpireTime = TickCount(UsingItemInfo.ExpireTime)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in ShadowTracker.OnProcessSpellCast + " + ex);
            }
        }

        private void OnUpdate(EventArgs args)
        {
            try
            {
                TrackInfomationList.RemoveAll(x => x.ExpireTime <= Environment.TickCount);
                StopTrackInfomationList.RemoveAll(x => x.ExpireTime <= Environment.TickCount);
                UsingItemInfomationList.RemoveAll(x => x.ExpireTime <= Environment.TickCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in ShadowTracker.OnUpdate + " + ex);
            }
        }

        private static int TickCount(int time)
        {
            return Environment.TickCount + time;
        }
    }

    public class UsingItemInfomation
    {
        public string ItemName;
        public int ExpireTime;
    }

    public class PetSkillInfomation
    {
        public string ChampionName;
        public int ExpireTime;
    }

    public class StopSkillInfomation
    {
        public string SpellName;
        public int ExpireTime;
    }

    public class MovingSkillInfomation
    {
        public string SpellName;
        public float MaxRange;
    }

    public class UsingTrackInfomation
    {
        public Obj_AI_Base Sender;
        public int CastTime;
        public InfoType InfoType;
        public int ExpireTime;
    }

    public class MovingTrackInfomation
    {
        public Obj_AI_Base Sender;
        public int CastTime;
        public InfoType InfoType;
        public int ExpireTime;
        public Vector3 StartPosition;
        public Vector3 EndPosition;
    }

    public class StopTrackInfomation
    {
        public Obj_AI_Base Sender;
        public Vector3 CastPosition;
        public InfoType InfoType;
        public int ExpireTime;
    }

    public enum InfoType
    {
        MovingSkill,
        StopSkill,
        UsingItem,
        PetStyle
    }
}