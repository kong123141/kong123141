
namespace Pluging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;
    using Color = System.Drawing.Color;
    using GamePath = System.Collections.Generic.List<SharpDX.Vector2>;
    using Flowers_Utility.Common.Evade.Pathfinding;
    using Flowers_Utility.Common.Evade;
    using Flowers_Utility.Common.Evade.Evade;

    internal class Evade
    {
        private static bool _evading;
        private static Vector2 _evadePoint;
        public static bool NoSolutionFound = false;
        public static Vector2 EvadeToPoint = new Vector2();
        private static bool _followPath = false;
        public static Menu Menu;
        public static SpellList<Skillshot> DetectedSkillshots = new SpellList<Skillshot>();
        public static string PlayerChampionName;
        public static int LastWardJumpAttempt = 0;
        public static Vector2 PreviousTickPosition = new Vector2();
        public static Vector2 PlayerPosition = new Vector2();
        private static readonly Random RandomN = new Random();
        private static int LastSentMovePacketT = 0;
        private static int LastSentMovePacketT2 = 0;
        private static int LastSMovePacketT = 0;

        public static bool Keepfollowing { get; set; }

        public static bool FollowPath
        {
            get
            {
                return _followPath;
            }

            set
            {
                _followPath = value;
                if (!_followPath)
                {
                    PathFollower.Stop();
                }
            }
        }

        public static bool Evading
        {
            get { return _evading; }
            set
            {
                if (value == true)
                {
                    LastSentMovePacketT = 0;
                    ObjectManager.Player.SendMovePacket(EvadePoint);
                }

                if (value == false)
                {
                    FollowPath = true;
                    Keepfollowing = true;
                }

                _evading = value;
            }
        }

        public static Vector2 EvadePoint
        {
            get { return _evadePoint; }
            set
            {
                if (value.IsValid())
                {
                    ObjectManager.Player.SendMovePacket(value);
                }
                _evadePoint = value;
            }
        }


        public Evade(Menu mainMenu)
        {
            Menu = mainMenu;

            Menu EvadeMenu = new Menu("[FL] 躲避插件", "Evade");

            var evadeSpells = new Menu("Evade spells", "evadeSpells");
            foreach (var spell in EvadeSpellDatabase.Spells)
            {
                var subMenu = new Menu(spell.Name, spell.Name);
                subMenu.AddItem(new MenuItem("DangerLevel" + spell.Name, "Danger level").SetValue(new Slider(spell.DangerLevel, 5, 1)));
                if (spell.IsTargetted && spell.ValidTargets.Contains(SpellValidTargets.AllyWards))
                    subMenu.AddItem(new MenuItem("WardJump" + spell.Name, "WardJump").SetValue(true));
                subMenu.AddItem(new MenuItem("Enabled" + spell.Name, "Enabled").SetValue(true));
                evadeSpells.AddSubMenu(subMenu);
            }
            EvadeMenu.AddSubMenu(evadeSpells);

            var skillShots = new Menu("Skillshots", "Skillshots");
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
                if (hero.Team != ObjectManager.Player.Team || Flowers_Utility.Common.Evade.Config.TestOnAllies)
                    foreach (var spell in SpellDatabase.Spells)
                        if (string.Equals(spell.ChampionName, hero.ChampionName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            var subMenu = new Menu(spell.MenuItemName, spell.MenuItemName);

                            subMenu.AddItem(new MenuItem("DangerLevel" + spell.MenuItemName, "Danger level").SetValue(new Slider(spell.DangerValue, 5, 1)));
                            subMenu.AddItem(new MenuItem("IsDangerous" + spell.MenuItemName, "Is Dangerous").SetValue(spell.IsDangerous));
                            subMenu.AddItem(new MenuItem("Draw" + spell.MenuItemName, "Draw").SetValue(true));
                            subMenu.AddItem(new MenuItem("Enabled" + spell.MenuItemName, "Enabled").SetValue(!spell.DisabledByDefault));
                            skillShots.AddSubMenu(subMenu);
                        }
            EvadeMenu.AddSubMenu(skillShots);

            var shielding = new Menu("Ally shielding", "Shielding");
            foreach (var ally in ObjectManager.Get<Obj_AI_Hero>())
                if (ally.IsAlly && !ally.IsMe)
                    shielding.AddItem(new MenuItem("shield" + ally.ChampionName, "Shield " + ally.ChampionName).SetValue(true));
            EvadeMenu.AddSubMenu(shielding);

            var collision = new Menu("Collision", "Collision");
            collision.AddItem(new MenuItem("MinionCollision", "Minion collision").SetValue(false));
            collision.AddItem(new MenuItem("HeroCollision", "Hero collision").SetValue(false));
            collision.AddItem(new MenuItem("YasuoCollision", "Yasuo wall collision").SetValue(true));
            collision.AddItem(new MenuItem("EnableCollision", "Enabled").SetValue(true));
            EvadeMenu.AddSubMenu(collision);

            var drawings = new Menu("Drawings", "Drawings");
            drawings.AddItem(new MenuItem("EnabledColor", "Enabled spell color").SetValue(Color.White));
            drawings.AddItem(new MenuItem("DisabledColor", "Disabled spell color").SetValue(Color.Red));
            drawings.AddItem(new MenuItem("MissileColor", "Missile color").SetValue(Color.LimeGreen));
            drawings.AddItem(new MenuItem("Border", "Border Width").SetValue(new Slider(1, 5, 1)));
            drawings.AddItem(new MenuItem("EnableDrawings", "Enabled").SetValue(true));
            EvadeMenu.AddSubMenu(drawings);

            var misc = new Menu("Misc", "Misc");
            misc.AddItem(new MenuItem("BlockSpells", "Block spells while evading").SetValue(new StringList(new[] { "No", "Only dangerous", "Always" }, 1)));
            misc.AddItem(new MenuItem("DisableFow", "Disable fog of war dodging").SetValue(false));
            misc.AddItem(new MenuItem("ShowEvadeStatus", "Show Evade Status").SetValue(false));
            if (ObjectManager.Player.CharData.BaseSkinName == "Olaf")
                misc.AddItem(new MenuItem("DisableEvadeForOlafR", "Automatic disable Evade when Olaf's ulti is active!").SetValue(true));
            EvadeMenu.AddSubMenu(misc);

            EvadeMenu.AddItem(new MenuItem("Enabled", "Enabled").SetValue(new KeyBind("K".ToCharArray()[0], KeyBindType.Toggle, true))).Permashow(true, "Evade");
            EvadeMenu.AddItem(new MenuItem("OnlyDangerous", "Dodge only dangerous").SetValue(new KeyBind(32, KeyBindType.Press))).Permashow();

            Menu.AddSubMenu(EvadeMenu);

            PlayerChampionName = ObjectManager.Player.ChampionName;

            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnIssueOrder += OnIssueOrder;
            Spellbook.OnCastSpell += OnCastSpell;
            SkillshotDetector.OnDetectSkillshot += OnDetectSkillshot;
            SkillshotDetector.OnDeleteMissile += OnDeleteMissile;
            Drawing.OnDraw += OnDraw;
            DetectedSkillshots.OnAdd += OnAdd;
            Flowers_Utility.Common.Evade.Collision.Init();
        }

        private void OnAdd(object sender, EventArgs e)
        {
            try
            {
                Evading = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Evade.OnAdd + " + ex);
            }
        }

        private void OnDraw(EventArgs args)
        {
            try
            {
                if (!Menu.Item("EnableDrawings").GetValue<bool>())
                {
                    return;
                }

                if (Menu.Item("ShowEvadeStatus").GetValue<bool>())
                {
                    var heropos = Drawing.WorldToScreen(ObjectManager.Player.Position);

                    if (Menu.Item("Enabled").GetValue<KeyBind>().Active)
                    {
                        Drawing.DrawText(heropos.X, heropos.Y, Color.Red, "Evade: ON");
                    }
                }

                var Border = Menu.Item("Border").GetValue<Slider>().Value;
                var missileColor = Menu.Item("MissileColor").GetValue<Color>();

                foreach (var skillshot in DetectedSkillshots)
                {
                    skillshot.Draw((skillshot.Evade() && Menu.Item("Enabled").GetValue<KeyBind>().Active) ? Menu.Item("EnabledColor").GetValue<Color>() : Menu.Item("DisabledColor").GetValue<Color>(), missileColor, Border);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Evade.OnDraw + " + ex);
            }
        }

        private void OnDeleteMissile(Skillshot skillshot, MissileClient missile)
        {
            try
            {
                if (skillshot.SpellData.SpellName == "VelkozQ")
                {
                    var spellData = SpellDatabase.GetByName("VelkozQSplit");
                    var direction = skillshot.Direction.Perpendicular();

                    if (DetectedSkillshots.Count(s => s.SpellData.SpellName == "VelkozQSplit") == 0)
                    {
                        for (var i = -1; i <= 1; i = i + 2)
                        {
                            var skillshotToAdd = new Skillshot(DetectionType.ProcessSpell, spellData, Flowers_Utility.Common.Evade.Utils.TickCount, missile.Position.To2D(), missile.Position.To2D() + i * direction * spellData.Range, skillshot.Unit);
                            DetectedSkillshots.Add(skillshotToAdd);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Evade.OnDeleteMissile + " + ex);
            }
        }

        private void OnDetectSkillshot(Skillshot skillshot)
        {
            try
            {
                var alreadyAdded = false;

                if (Menu.Item("DisableFow").GetValue<bool>() && !skillshot.Unit.IsVisible)
                {
                    return;
                }

                foreach (var item in DetectedSkillshots)
                {
                    if (item.SpellData.SpellName == skillshot.SpellData.SpellName && (item.Unit.NetworkId == skillshot.Unit.NetworkId && (skillshot.Direction).AngleBetween(item.Direction) < 5 && (skillshot.Start.Distance(item.Start) < 100 || skillshot.SpellData.FromObjects.Length == 0)))
                    {
                        alreadyAdded = true;
                    }
                }

                if (skillshot.Unit.Team == ObjectManager.Player.Team && !Flowers_Utility.Common.Evade.Config.TestOnAllies)
                {
                    return;
                }

                if (skillshot.Start.Distance(PlayerPosition) > (skillshot.SpellData.Range + skillshot.SpellData.Radius + 1000) * 1.5)
                {
                    return;
                }

                if (!alreadyAdded || skillshot.SpellData.DontCheckForDuplicates)
                {
                    if (skillshot.DetectionType == DetectionType.ProcessSpell)
                    {
                        if (skillshot.SpellData.MultipleNumber != -1)
                        {
                            var originalDirection = skillshot.Direction;

                            for (var i = -(skillshot.SpellData.MultipleNumber - 1) / 2; i <= (skillshot.SpellData.MultipleNumber - 1) / 2; i++)
                            {
                                var end = skillshot.Start + skillshot.SpellData.Range * originalDirection.Rotated(skillshot.SpellData.MultipleAngle * i);
                                var skillshotToAdd = new Skillshot(skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end, skillshot.Unit);

                                DetectedSkillshots.Add(skillshotToAdd);
                            }

                            return;
                        }

                        if (skillshot.SpellData.SpellName == "UFSlash")
                        {
                            skillshot.SpellData.MissileSpeed = 1600 + (int)skillshot.Unit.MoveSpeed;
                        }

                        if (skillshot.SpellData.SpellName == "SionR")
                        {
                            skillshot.SpellData.MissileSpeed = (int)skillshot.Unit.MoveSpeed;
                        }

                        if (skillshot.SpellData.Invert)
                        {
                            var newDirection = -(skillshot.End - skillshot.Start).Normalized();
                            var end = skillshot.Start + newDirection * skillshot.Start.Distance(skillshot.End);
                            var skillshotToAdd = new Skillshot(skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end, skillshot.Unit);

                            DetectedSkillshots.Add(skillshotToAdd);

                            return;
                        }

                        if (skillshot.SpellData.Centered)
                        {
                            var start = skillshot.Start - skillshot.Direction * skillshot.SpellData.Range;
                            var end = skillshot.Start + skillshot.Direction * skillshot.SpellData.Range;
                            var skillshotToAdd = new Skillshot(skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end, skillshot.Unit);

                            DetectedSkillshots.Add(skillshotToAdd);

                            return;
                        }

                        if (skillshot.SpellData.SpellName == "SyndraE" || skillshot.SpellData.SpellName == "syndrae5")
                        {
                            var angle = 60;
                            var edge1 = (skillshot.End - skillshot.Unit.ServerPosition.To2D()).Rotated(-angle / 2 * (float)Math.PI / 180);
                            var edge2 = edge1.Rotated(angle * (float)Math.PI / 180);
                            var positions = new List<Vector2>();
                            var explodingQ = DetectedSkillshots.FirstOrDefault(s => s.SpellData.SpellName == "SyndraQ");

                            if (explodingQ != null)
                            {
                                positions.Add(explodingQ.End);
                            }

                            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>())
                            {
                                if (minion.Name == "Seed" && !minion.IsDead && (minion.Team != ObjectManager.Player.Team || Flowers_Utility.Common.Evade.Config.TestOnAllies))
                                {
                                    positions.Add(minion.ServerPosition.To2D());
                                }
                            }

                            foreach (var position in positions)
                            {
                                var v = position - skillshot.Unit.ServerPosition.To2D();

                                if (edge1.CrossProduct(v) > 0 && v.CrossProduct(edge2) > 0 && position.Distance(skillshot.Unit) < 800)
                                {
                                    var start = position;
                                    var end = skillshot.Unit.ServerPosition.To2D().Extend(position, skillshot.Unit.Distance(position) > 200 ? 1300 : 1000);
                                    var startTime = skillshot.StartTick;

                                    startTime += (int)(150 + skillshot.Unit.Distance(position) / 2.5f);

                                    var skillshotToAdd = new Skillshot(skillshot.DetectionType, skillshot.SpellData, startTime, start, end, skillshot.Unit);

                                    DetectedSkillshots.Add(skillshotToAdd);
                                }
                            }
                            return;
                        }

                        if (skillshot.SpellData.SpellName == "AlZaharCalloftheVoid")
                        {
                            var start = skillshot.End - skillshot.Direction.Perpendicular() * 400;
                            var end = skillshot.End + skillshot.Direction.Perpendicular() * 400;
                            var skillshotToAdd = new Skillshot(skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end, skillshot.Unit);

                            DetectedSkillshots.Add(skillshotToAdd);

                            return;
                        }

                        if (skillshot.SpellData.SpellName == "DianaArc")
                        {
                            var skillshotToAdd = new Skillshot(skillshot.DetectionType, SpellDatabase.GetByName("DianaArcArc"), skillshot.StartTick, skillshot.Start, skillshot.End, skillshot.Unit);

                            DetectedSkillshots.Add(skillshotToAdd);
                        }

                        if (skillshot.SpellData.SpellName == "ZiggsQ")
                        {
                            var d1 = skillshot.Start.Distance(skillshot.End);
                            var d2 = d1 * 0.4f;
                            var d3 = d2 * 0.69f;

                            var bounce1SpellData = SpellDatabase.GetByName("ZiggsQBounce1");
                            var bounce2SpellData = SpellDatabase.GetByName("ZiggsQBounce2");

                            var bounce1Pos = skillshot.End + skillshot.Direction * d2;
                            var bounce2Pos = bounce1Pos + skillshot.Direction * d3;

                            bounce1SpellData.Delay = (int)(skillshot.SpellData.Delay + d1 * 1000f / skillshot.SpellData.MissileSpeed + 500);
                            bounce2SpellData.Delay = (int)(bounce1SpellData.Delay + d2 * 1000f / bounce1SpellData.MissileSpeed + 500);

                            var bounce1 = new Skillshot(skillshot.DetectionType, bounce1SpellData, skillshot.StartTick, skillshot.End, bounce1Pos, skillshot.Unit);
                            var bounce2 = new Skillshot(skillshot.DetectionType, bounce2SpellData, skillshot.StartTick, bounce1Pos, bounce2Pos, skillshot.Unit);

                            DetectedSkillshots.Add(bounce1);
                            DetectedSkillshots.Add(bounce2);
                        }

                        if (skillshot.SpellData.SpellName == "ZiggsR")
                        {
                            skillshot.SpellData.Delay = (int)(1500 + 1500 * skillshot.End.Distance(skillshot.Start) / skillshot.SpellData.Range);
                        }

                        if (skillshot.SpellData.SpellName == "JarvanIVDragonStrike")
                        {
                            var endPos = new Vector2();

                            foreach (var s in DetectedSkillshots)
                            {
                                if (s.Unit.NetworkId == skillshot.Unit.NetworkId && s.SpellData.Slot == SpellSlot.E)
                                {
                                    var extendedE = new Skillshot(skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, skillshot.End + skillshot.Direction * 100, skillshot.Unit);

                                    if (!extendedE.IsSafe(s.End))
                                    {
                                        endPos = s.End;
                                    }
                                    break;
                                }
                            }

                            foreach (var m in ObjectManager.Get<Obj_AI_Minion>())
                            {
                                if (m.CharData.BaseSkinName == "jarvanivstandard" && m.Team == skillshot.Unit.Team)
                                {

                                    var extendedE = new Skillshot(skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, skillshot.End + skillshot.Direction * 100, skillshot.Unit);
                                    if (!extendedE.IsSafe(m.Position.To2D()))
                                    {
                                        endPos = m.Position.To2D();
                                    }
                                    break;
                                }
                            }

                            if (endPos.IsValid())
                            {
                                skillshot = new Skillshot(DetectionType.ProcessSpell, SpellDatabase.GetByName("JarvanIVEQ"), Flowers_Utility.Common.Evade.Utils.TickCount, skillshot.Start, endPos, skillshot.Unit);
                                skillshot.End = endPos + 200 * (endPos - skillshot.Start).Normalized();
                                skillshot.Direction = (skillshot.End - skillshot.Start).Normalized();
                            }
                        }
                    }

                    if (skillshot.SpellData.SpellName == "OriannasQ")
                    {
                        var skillshotToAdd = new Skillshot(skillshot.DetectionType, SpellDatabase.GetByName("OriannaQend"), skillshot.StartTick, skillshot.Start, skillshot.End, skillshot.Unit);

                        DetectedSkillshots.Add(skillshotToAdd);
                    }

                    if (skillshot.SpellData.DisableFowDetection && skillshot.DetectionType == DetectionType.RecvPacket)
                    {
                        return;
                    }

                    DetectedSkillshots.Add(skillshot);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Evade.OnDetectSkillshot + " + ex);
            }
        }

        private void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            try
            {
                if (sender.Owner.IsValid && sender.Owner.IsMe)
                {
                    if (args.Slot == SpellSlot.Recall)
                    {
                        EvadeToPoint = new Vector2();
                    }

                    if (Evading)
                    {
                        var blockLevel = Menu.Item("BlockSpells").GetValue<StringList>().SelectedIndex;

                        if (blockLevel == 0)
                        {
                            return;
                        }

                        var isDangerous = false;

                        foreach (var skillshot in DetectedSkillshots)
                        {
                            if (skillshot.Evade() && skillshot.IsDanger(PlayerPosition))
                            {
                                isDangerous = skillshot.GetValue<bool>("IsDangerous");

                                if (isDangerous)
                                {
                                    break;
                                }
                            }
                        }

                        if (blockLevel == 1 && !isDangerous)
                        {
                            return;
                        }

                        args.Process = !SpellBlocker.ShouldBlock(args.Slot);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Evade.OnCastSpell + " + ex);
            }
        }

        private void OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            try
            {
                if (!sender.IsMe)
                {
                    return;
                }

                if (NoSolutionFound)
                {
                    return;
                }

                if (!Menu.Item("Enabled").GetValue<KeyBind>().Active)
                {
                    return;
                }

                if (EvadeSpellDatabase.Spells.Any(evadeSpell => evadeSpell.Name == "Walking" && !evadeSpell.Enabled))
                {
                    return;
                }

                if (ObjectManager.Player.IsSpellShielded())
                {
                    return;
                }

                if (PlayerChampionName == "Olaf" && Menu.Item("DisableEvadeForOlafR").GetValue<bool>() && ObjectManager.Player.HasBuff("OlafRagnarok"))
                {
                    return;
                }

                if (args.Order == GameObjectOrder.MoveTo || args.Order == GameObjectOrder.AttackTo)
                {
                    EvadeToPoint.X = args.TargetPosition.X;
                    EvadeToPoint.Y = args.TargetPosition.Y;
                    Keepfollowing = false;
                    FollowPath = false;
                }
                else
                {
                    EvadeToPoint.X = 0;
                    EvadeToPoint.Y = 0;
                }

                var myPath = ObjectManager.Player.GetPath(new Vector3(args.TargetPosition.X, args.TargetPosition.Y, ObjectManager.Player.ServerPosition.Z)).To2DList();
                var safeResult = IsSafe(PlayerPosition);

                if (Evading || !safeResult.IsSafe)
                {
                    var rcSafePath = IsSafePath(myPath, Flowers_Utility.Common.Evade.Config.EvadingRouteChangeTimeOffset);
                    if (args.Order == GameObjectOrder.MoveTo)
                    {
                        if (Evading && Flowers_Utility.Common.Evade.Utils.TickCount - Flowers_Utility.Common.Evade.Config.LastEvadePointChangeT > Flowers_Utility.Common.Evade.Config.EvadePointChangeInterval)
                        {
                            var points = Evader.GetEvadePoints(-1, 0, false, true);
                            if (points.Count > 0)
                            {
                                var to = new Vector2(args.TargetPosition.X, args.TargetPosition.Y);
                                EvadePoint = to.Closest(points);
                                Evading = true;
                                Flowers_Utility.Common.Evade.Config.LastEvadePointChangeT = Flowers_Utility.Common.Evade.Utils.TickCount;
                            }
                        }

                        if (rcSafePath.IsSafe && IsSafe(myPath[myPath.Count - 1]).IsSafe && args.Order == GameObjectOrder.MoveTo)
                        {
                            EvadePoint = myPath[myPath.Count - 1];
                            Evading = true;
                        }
                    }

                    args.Process = false;
                    return;
                }

                var safePath = IsSafePath(myPath, Flowers_Utility.Common.Evade.Config.CrossingTimeOffset);

                if (!safePath.IsSafe && args.Order != GameObjectOrder.AttackUnit)
                {
                    if (safePath.Intersection.Valid)
                    {
                        if (ObjectManager.Player.Distance(safePath.Intersection.Point) > 75)
                        {
                            ObjectManager.Player.SendMovePacket(safePath.Intersection.Point);
                        }
                    }
                    FollowPath = true;
                    args.Process = false;
                }
                else if (safePath.IsSafe && args.Order != GameObjectOrder.AttackUnit)
                {
                    FollowPath = false;
                }

                //AutoAttacks.
                if (!safePath.IsSafe && args.Order == GameObjectOrder.AttackUnit)
                {
                    var target = args.Target;
                    if (target != null && target.IsValid<Obj_AI_Base>() && target.IsVisible)
                    {
                        //Out of attack range.
                        if (PlayerPosition.Distance(((Obj_AI_Base)target).ServerPosition) >
                            ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius +
                            target.BoundingRadius)
                        {
                            if (safePath.Intersection.Valid)
                            {
                                ObjectManager.Player.SendMovePacket(safePath.Intersection.Point);
                            }
                            args.Process = false;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Evade.OnIssueOrder + " + ex);
            }
        }

        private void OnUpdate(EventArgs args)
        {
            try
            {
                PlayerPosition = ObjectManager.Player.ServerPosition.To2D();

                if (PreviousTickPosition.IsValid() && PlayerPosition.Distance(PreviousTickPosition) > 200)
                {
                    Evading = false;
                }

                PreviousTickPosition = PlayerPosition;

                DetectedSkillshots.RemoveAll(skillshot => !skillshot.IsActive());

                foreach (var skillshot in DetectedSkillshots)
                {
                    skillshot.Game_OnGameUpdate();
                }

                if (!Menu.Item("Enabled").GetValue<KeyBind>().Active)
                {
                    Evading = false;
                    EvadeToPoint = Vector2.Zero;
                    PathFollower.Stop();
                    return;
                }

                if (PlayerChampionName == "Olaf" && Menu.Item("DisableEvadeForOlafR").GetValue<bool>() && ObjectManager.Player.HasBuff("OlafRagnarok"))
                {
                    Evading = false;
                    EvadeToPoint = Vector2.Zero;
                    PathFollower.Stop();
                    return;
                }

                if (ObjectManager.Player.IsDead)
                {
                    Evading = false;
                    EvadeToPoint = Vector2.Zero;
                    PathFollower.Stop();
                    return;
                }

                if (ObjectManager.Player.IsCastingInterruptableSpell(true))
                {
                    Evading = false;
                    EvadeToPoint = Vector2.Zero;
                    PathFollower.Stop();
                    return;
                }

                if (ObjectManager.Player.IsWindingUp && !Orbwalking.IsAutoAttack(ObjectManager.Player.LastCastedSpellName()))
                {
                    Evading = false;
                    return;
                }

                if (Flowers_Utility.Common.Evade.Utils.ImmobileTime(ObjectManager.Player) - Flowers_Utility.Common.Evade.Utils.TickCount > Game.Ping / 2 + 70)
                {
                    Evading = false;
                    return;
                }

                if (ObjectManager.Player.IsDashing())
                {
                    Evading = false;
                    return;
                }

                if (PlayerChampionName == "Sion" && ObjectManager.Player.HasBuff("SionR"))
                {
                    PathFollower.Stop();
                    return;
                }

                foreach (var ally in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (ally.IsValidTarget(1000, false))
                    {
                        var shieldAlly = Menu.Item("shield" + ally.ChampionName);
                        if (shieldAlly != null && shieldAlly.GetValue<bool>())
                        {
                            var allySafeResult = IsSafe(ally.ServerPosition.To2D());

                            if (!allySafeResult.IsSafe)
                            {
                                var dangerLevel = 0;

                                foreach (var skillshot in allySafeResult.SkillshotList)
                                {
                                    dangerLevel = Math.Max(dangerLevel, skillshot.GetValue<Slider>("DangerLevel").Value);
                                }

                                foreach (var evadeSpell in EvadeSpellDatabase.Spells)
                                {
                                    if (evadeSpell.IsShield && evadeSpell.CanShieldAllies && ally.Distance(ObjectManager.Player) < evadeSpell.MaxRange && dangerLevel >= evadeSpell.DangerLevel && ObjectManager.Player.Spellbook.CanUseSpell(evadeSpell.Slot) == SpellState.Ready && IsAboutToHit(ally, evadeSpell.Delay))
                                    {
                                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, ally);
                                    }
                                }
                            }
                        }
                    }
                }

                if (ObjectManager.Player.IsSpellShielded())
                {
                    PathFollower.Stop();
                    return;
                }

                if (NoSolutionFound)
                {
                    PathFollower.Stop();
                }

                var currentPath = ObjectManager.Player.GetWaypoints();
                var safeResult = IsSafe(PlayerPosition);
                var safePath = IsSafePath(currentPath, 100);

                if (FollowPath && !NoSolutionFound && (Keepfollowing || !Evading) && EvadeToPoint.IsValid() && safeResult.IsSafe)
                {
                    if (EvadeSpellDatabase.Spells.Any(evadeSpell => evadeSpell.Name == "Walking" && evadeSpell.Enabled))
                    {
                        if (Flowers_Utility.Common.Evade.Utils.TickCount - LastSentMovePacketT2 > 300)
                        {
                            var candidate = Pathfinding.PathFind(PlayerPosition, EvadeToPoint);
                            PathFollower.Follow(candidate);
                            LastSentMovePacketT2 = Flowers_Utility.Common.Evade.Utils.TickCount;
                        }
                    }
                }
                else
                {
                    FollowPath = false;
                }

                NoSolutionFound = false;

                if (Evading && IsSafe(EvadePoint).IsSafe)
                {
                    if (safeResult.IsSafe)
                    {
                        Evading = false;
                    }
                    else
                    {
                        if (Flowers_Utility.Common.Evade.Utils.TickCount - LastSentMovePacketT > 1000 / 15)
                        {
                            LastSentMovePacketT = Flowers_Utility.Common.Evade.Utils.TickCount;
                            ObjectManager.Player.SendMovePacket(EvadePoint);
                        }
                        return;
                    }
                }
                else if (Evading)
                {
                    Evading = false;
                }

                if (!safePath.IsSafe)
                {
                    if (!safeResult.IsSafe)
                    {
                        TryToEvade(safeResult.SkillshotList, EvadeToPoint.IsValid() ? EvadeToPoint : Game.CursorPos.To2D());
                    }
                    //Outside the danger polygon.
                    else
                    {
                        FollowPath = true;
                        if (EvadeSpellDatabase.Spells.Any(evadeSpell => evadeSpell.Name == "Walking" && evadeSpell.Enabled))
                        {
                            ObjectManager.Player.SendMovePacket(safePath.Intersection.Point);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Evade.OnUpdate + " + ex);
            }
        }

        public static IsSafeResult IsSafe(Vector2 point)
        {
            var result = new IsSafeResult();

            result.SkillshotList = new List<Skillshot>();

            foreach (var skillshot in DetectedSkillshots)
            {
                if (skillshot.Evade() && skillshot.IsDanger(point))
                {
                    result.SkillshotList.Add(skillshot);
                }
            }

            result.IsSafe = (result.SkillshotList.Count == 0);

            return result;
        }

        public static SafePathResult IsSafePath(GamePath path, int timeOffset, int speed = -1, int delay = 0, Obj_AI_Base unit = null)
        {
            var IsSafe = true;
            var intersections = new List<FoundIntersection>();
            var intersection = new FoundIntersection();

            foreach (var skillshot in DetectedSkillshots)
            {
                if (skillshot.Evade())
                {
                    var sResult = skillshot.IsSafePath(path, timeOffset, speed, delay, unit);

                    IsSafe = (IsSafe) ? sResult.IsSafe : false;

                    if (sResult.Intersection.Valid)
                    {
                        intersections.Add(sResult.Intersection);
                    }
                }
            }

            if (!IsSafe)
            {
                var intersetion = intersections.MinOrDefault(o => o.Distance);

                return new SafePathResult(false, intersetion.Valid ? intersetion : intersection);
            }

            return new SafePathResult(true, intersection);
        }

        public static bool IsSafeToBlink(Vector2 point, int timeOffset, int delay)
        {
            foreach (var skillshot in DetectedSkillshots)
            {
                if (skillshot.Evade())
                {
                    if (!skillshot.IsSafeToBlink(point, timeOffset, delay))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool IsAboutToHit(Obj_AI_Base unit, int time)
        {
            time += 150;

            foreach (var skillshot in DetectedSkillshots)
            {
                if (skillshot.Evade())
                {
                    if (skillshot.IsAboutToHit(time, unit))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void TryToEvade(List<Skillshot> HitBy, Vector2 to)
        {
            var dangerLevel = 0;

            foreach (var skillshot in HitBy)
            {
                dangerLevel = Math.Max(dangerLevel, skillshot.GetValue<Slider>("DangerLevel").Value);
            }

            foreach (var evadeSpell in EvadeSpellDatabase.Spells)
            {
                if (evadeSpell.Enabled && evadeSpell.DangerLevel <= dangerLevel)
                {
                    if (evadeSpell.IsSpellShield && ObjectManager.Player.Spellbook.CanUseSpell(evadeSpell.Slot) == SpellState.Ready)
                    {
                        if (IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                        {
                            ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, ObjectManager.Player);
                        }

                        NoSolutionFound = true;
                        return;
                    }

                    if (evadeSpell.Name == "Walking")
                    {
                        var points = Evader.GetEvadePoints();

                        if (points.Count > 0)
                        {
                            EvadePoint = to.Closest(points);

                            var nEvadePoint = EvadePoint.Extend(PlayerPosition, -100);

                            if (IsSafePath(ObjectManager.Player.GetPath(nEvadePoint.To3D()).To2DList(), Flowers_Utility.Common.Evade.Config.EvadingSecondTimeOffset, (int)ObjectManager.Player.MoveSpeed, 100).IsSafe)
                            {
                                EvadePoint = nEvadePoint;
                            }

                            Evading = true;

                            return;
                        }
                    }

                    if (evadeSpell.IsReady())
                    {
                        if (evadeSpell.IsMovementSpeedBuff)
                        {
                            var points = Evader.GetEvadePoints((int)evadeSpell.MoveSpeedTotalAmount());

                            if (points.Count > 0)
                            {
                                EvadePoint = to.Closest(points);

                                Evading = true;

                                if (evadeSpell.IsSummonerSpell)
                                {
                                    ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, ObjectManager.Player);
                                }
                                else
                                {
                                    ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, ObjectManager.Player);
                                }

                                return;
                            }
                        }

                        if (evadeSpell.IsDash)
                        {
                            if (evadeSpell.IsTargetted)
                            {
                                var targets = Evader.GetEvadeTargets(evadeSpell.ValidTargets, evadeSpell.Speed, evadeSpell.Delay, evadeSpell.MaxRange, false, false);

                                if (targets.Count > 0)
                                {
                                    var closestTarget = Flowers_Utility.Common.Evade.Utils.Closest(targets, to);

                                    EvadePoint = closestTarget.ServerPosition.To2D();

                                    Evading = true;

                                    if (evadeSpell.IsSummonerSpell)
                                    {
                                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, closestTarget);
                                    }
                                    else
                                    {
                                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, closestTarget);
                                    }

                                    return;
                                }
                                if (Flowers_Utility.Common.Evade.Utils.TickCount - LastWardJumpAttempt < 250)
                                {
                                    NoSolutionFound = true;

                                    return;
                                }

                                if (evadeSpell.IsTargetted && evadeSpell.ValidTargets.Contains(SpellValidTargets.AllyWards) && Menu.Item("WardJump" + evadeSpell.Name).GetValue<bool>())
                                {
                                    var wardSlot = Items.GetWardSlot();

                                    if (wardSlot != null)
                                    {
                                        var points = Evader.GetEvadePoints(evadeSpell.Speed, evadeSpell.Delay, false);

                                        points.RemoveAll(item => item.Distance(ObjectManager.Player.ServerPosition) > 600);

                                        if (points.Count > 0)
                                        {
                                            for (var i = 0; i < points.Count; i++)
                                            {
                                                var k = (int)(600 - PlayerPosition.Distance(points[i]));

                                                k = k - new Random(Flowers_Utility.Common.Evade.Utils.TickCount).Next(k);

                                                var extended = points[i] + k * (points[i] - PlayerPosition).Normalized();

                                                if (IsSafe(extended).IsSafe)
                                                {
                                                    points[i] = extended;
                                                }
                                            }

                                            var ePoint = to.Closest(points);

                                            ObjectManager.Player.Spellbook.CastSpell(wardSlot.SpellSlot, ePoint.To3D());

                                            LastWardJumpAttempt = Flowers_Utility.Common.Evade.Utils.TickCount;

                                            NoSolutionFound = true;

                                            return;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var points = Evader.GetEvadePoints(evadeSpell.Speed, evadeSpell.Delay, false);

                                points.RemoveAll(item => item.Distance(ObjectManager.Player.ServerPosition) > evadeSpell.MaxRange);

                                if (evadeSpell.FixedRange)
                                {
                                    for (var i = 0; i < points.Count; i++)
                                    {
                                        points[i] = PlayerPosition.Extend(points[i], evadeSpell.MaxRange);
                                    }

                                    for (var i = points.Count - 1; i > 0; i--)
                                    {
                                        if (!IsSafe(points[i]).IsSafe)
                                        {
                                            points.RemoveAt(i);
                                        }
                                    }
                                }
                                else
                                {
                                    for (var i = 0; i < points.Count; i++)
                                    {
                                        var k = (int)(evadeSpell.MaxRange - PlayerPosition.Distance(points[i]));

                                        k -= Math.Max(RandomN.Next(k) - 100, 0);

                                        var extended = points[i] + k * (points[i] - PlayerPosition).Normalized();

                                        if (IsSafe(extended).IsSafe)
                                        {
                                            points[i] = extended;
                                        }
                                    }
                                }

                                if (points.Count > 0)
                                {
                                    EvadePoint = to.Closest(points);

                                    Evading = true;

                                    if (!evadeSpell.Invert)
                                    {
                                        if (evadeSpell.RequiresPreMove)
                                        {
                                            ObjectManager.Player.SendMovePacket(EvadePoint);

                                            var theSpell = evadeSpell;

                                            Utility.DelayAction.Add(Game.Ping / 2 + 100,
                                                delegate
                                                {
                                                    ObjectManager.Player.Spellbook.CastSpell(theSpell.Slot, EvadePoint.To3D());
                                                });
                                        }
                                        else
                                        {
                                            ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, EvadePoint.To3D());
                                        }
                                    }
                                    else
                                    {
                                        var castPoint = PlayerPosition - (EvadePoint - PlayerPosition);

                                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, castPoint.To3D());
                                    }

                                    return;
                                }
                            }
                        }

                        if (evadeSpell.IsBlink)
                        {
                            if (evadeSpell.IsTargetted)
                            {
                                var targets = Evader.GetEvadeTargets(evadeSpell.ValidTargets, int.MaxValue, evadeSpell.Delay, evadeSpell.MaxRange, true, false);

                                if (targets.Count > 0)
                                {
                                    if (IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                                    {
                                        var closestTarget = Flowers_Utility.Common.Evade.Utils.Closest(targets, to);

                                        EvadePoint = closestTarget.ServerPosition.To2D();

                                        Evading = true;

                                        if (evadeSpell.IsSummonerSpell)
                                        {
                                            ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, closestTarget);
                                        }
                                        else
                                        {
                                            ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, closestTarget);
                                        }
                                    }

                                    NoSolutionFound = true;

                                    return;
                                }
                                if (Flowers_Utility.Common.Evade.Utils.TickCount - LastWardJumpAttempt < 250)
                                {
                                    NoSolutionFound = true;

                                    return;
                                }

                                if (evadeSpell.IsTargetted && evadeSpell.ValidTargets.Contains(SpellValidTargets.AllyWards) && Menu.Item("WardJump" + evadeSpell.Name).GetValue<bool>())
                                {
                                    var wardSlot = Items.GetWardSlot();

                                    if (wardSlot != null)
                                    {
                                        var points = Evader.GetEvadePoints(int.MaxValue, evadeSpell.Delay, true);

                                        points.RemoveAll(item => item.Distance(ObjectManager.Player.ServerPosition) > 600);

                                        if (points.Count > 0)
                                        {
                                            for (var i = 0; i < points.Count; i++)
                                            {
                                                var k = (int)(600 - PlayerPosition.Distance(points[i]));

                                                k = k - new Random(Flowers_Utility.Common.Evade.Utils.TickCount).Next(k);

                                                var extended = points[i] +  k * (points[i] - PlayerPosition).Normalized();

                                                if (IsSafe(extended).IsSafe)
                                                {
                                                    points[i] = extended;
                                                }
                                            }

                                            var ePoint = to.Closest(points);

                                            ObjectManager.Player.Spellbook.CastSpell(wardSlot.SpellSlot, ePoint.To3D());

                                            LastWardJumpAttempt = Flowers_Utility.Common.Evade.Utils.TickCount;

                                            NoSolutionFound = true;

                                            return;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var points = Evader.GetEvadePoints(int.MaxValue, evadeSpell.Delay, true);

                                points.RemoveAll(item => item.Distance(ObjectManager.Player.ServerPosition) > evadeSpell.MaxRange);

                                for (var i = 0; i < points.Count; i++)
                                {
                                    var k = (int)(evadeSpell.MaxRange - PlayerPosition.Distance(points[i]));

                                    k = k - new Random(Flowers_Utility.Common.Evade.Utils.TickCount).Next(k);

                                    var extended = points[i] +  k * (points[i] - PlayerPosition).Normalized();

                                    if (IsSafe(extended).IsSafe)
                                    {
                                        points[i] = extended;
                                    }
                                }

                                if (points.Count > 0)
                                {
                                    if (IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                                    {
                                        EvadePoint = to.Closest(points);

                                        Evading = true;

                                        if (evadeSpell.IsSummonerSpell)
                                        {
                                            ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, EvadePoint.To3D());
                                        }
                                        else
                                        {
                                            ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, EvadePoint.To3D());
                                        }
                                    }

                                    NoSolutionFound = true;

                                    return;
                                }
                            }
                        }

                        if (evadeSpell.IsInvulnerability)
                        {
                            if (evadeSpell.IsTargetted)
                            {
                                var targets = Evader.GetEvadeTargets(evadeSpell.ValidTargets, int.MaxValue, 0, evadeSpell.MaxRange, true, false, true);

                                if (targets.Count > 0)
                                {
                                    if (IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                                    {
                                        var closestTarget = Flowers_Utility.Common.Evade.Utils.Closest(targets, to);

                                        EvadePoint = closestTarget.ServerPosition.To2D();

                                        Evading = true;

                                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, closestTarget);
                                    }

                                    NoSolutionFound = true;

                                    return;
                                }
                            }
                            else
                            {
                                if (IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                                {
                                    if (evadeSpell.SelfCast)
                                    {
                                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot);
                                    }
                                    else
                                    {
                                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, ObjectManager.Player.ServerPosition);
                                    }
                                }
                            }

                            NoSolutionFound = true;

                            return;
                        }
                    }

                    if (evadeSpell.Name == "Zhonyas" && (Items.CanUseItem("ZhonyasHourglass")))
                    {
                        if (IsAboutToHit(ObjectManager.Player, 100))
                        {
                            Items.UseItem("ZhonyasHourglass");
                        }

                        NoSolutionFound = true;

                        return;
                    }

                    if (evadeSpell.IsShield &&
                        ObjectManager.Player.Spellbook.CanUseSpell(evadeSpell.Slot) == SpellState.Ready)
                    {
                        if (IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                        {
                            ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, ObjectManager.Player);
                        }

                        NoSolutionFound = true;

                        return;
                    }
                }
            }

            NoSolutionFound = true;
        }

        public struct IsSafeResult
        {
            public bool IsSafe;
            public List<Skillshot> SkillshotList;
        }
    }
}