using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D9;
using LeagueSharp;
using LeagueSharp.Common;
using Font = SharpDX.Direct3D9.Font;
using Rectangle = SharpDX.Rectangle;
using Color = System.Drawing.Color;
using SharpColor = SharpDX.Color;

namespace HikiCarry_Vayne_Masterrace
{
    class Program
    {
        public const string ChampionName = "Vayne";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Menu Config;
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static readonly Vector2 midPos = new Vector2(6707.485f, 8802.744f);
        private static readonly Vector2 dragPos = new Vector2(11514, 4462);
        private static SpellSlot Flash;
        private static int pushDistance = 0;

        private static Font hikiFont;

        public static string[] Marksman = { "Kalista", "Jinx", "Lucian", "Quinn", "Draven",  "Varus", "Graves", "Vayne", 
                                            "Caitlyn","Urgot", "Ezreal", "KogMaw", "Ashe", "MissFortune", "Tristana", "Teemo", 
                                            "Sivir","Twitch", "Corki"};
        public static string[] qssBuff = { "Charm", "Flee", "Snare", "Polymorph", "Stun", "Suppression", "Taunt"};



        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.CharData.BaseSkinName != ChampionName)
            {
                return;
            }
            hikiFont = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 15, Weight = FontWeight.Bold, Quality = FontQuality.Antialiased });
            
            Q = new Spell(SpellSlot.Q, 300f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 550f);
            E.SetTargetted(0.25f, 1600f);
            R = new Spell(SpellSlot.R);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            Config = new Menu("HikiCarry - Vayne", "HikiCarry - Vayne", true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("Orbwalker Settings", "Orbwalker Settings"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker Settings"));

            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.AddItem(new MenuItem("qCombo", "Use Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("eCombo", "Use E").SetValue(true));
                comboMenu.AddItem(new MenuItem("rCombo", "Use R").SetValue(true));
                comboMenu.AddItem(new MenuItem("rComboxEnemy", "R on x Enemy").SetValue(new Slider(3, 0, 10)));
                Config.AddSubMenu(comboMenu);
            }
            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.AddItem(new MenuItem("qHarass", "Use Q").SetValue(true));
                harassMenu.AddItem(new MenuItem("eHarass", "Use E").SetValue(true));
                harassMenu.AddItem(new MenuItem("manaHarass", "Harass Mana Percent").SetValue(new Slider(30, 1, 100)));
                Config.AddSubMenu(harassMenu);
            }
            var miscMenu = new Menu("Misc Settings", "Misc Settings");
            {
                var scryingMenu = new Menu("Scrying Orb Settings", "Scrying Orb Settings");
                {
                    scryingMenu.AddItem(new MenuItem("bT", "Auto Scrying Orb Buy!").SetValue(true));
                    scryingMenu.AddItem(new MenuItem("bluetrinketlevel", "Scrying Orb Buy Level").SetValue(new Slider(6, 0, 18)));
                    scryingMenu.AddSubMenu(miscMenu);
                }
                var ultiBroker = new Menu("Ulti Broker", "Ulti Broker");
                {
                    ultiBroker.AddItem(new MenuItem("broke.ulti", "Broke Ulti").SetValue(true));
                    ultiBroker.AddItem(new MenuItem("katarina.ulti", "Katarina R").SetValue(true));
                    ultiBroker.AddItem(new MenuItem("missfortune.ulti", "Miss Fortune R").SetValue(true));
                    ultiBroker.AddSubMenu(miscMenu);
                }
                miscMenu.AddItem(new MenuItem("agapcloser", "Anti-Gapcloser Active!").SetValue(true));
                miscMenu.AddItem(new MenuItem("ainterrupt", "Auto Interrupt Active!").SetValue(true));
                Config.AddSubMenu(miscMenu);
            }
            var combatMenu = new Menu("Combat Settings", "Combat Settings");
            {
                var gravesMenu = new Menu("Graves Settings", "Graves Settings");
                {
                    gravesMenu.AddItem(new MenuItem("combatGraves", "Combat Graves").SetValue(true));
                    gravesMenu.AddItem(new MenuItem("positioning", "Wall Condemn Method").SetValue(new StringList(new[] { "Repositioning with Flash", "Repositioning with Q" })));
                    gravesMenu.AddItem(new MenuItem("qCombat", "Use Q").SetValue(true));
                    gravesMenu.AddItem(new MenuItem("eCombat", "Use E").SetValue(true));
                    gravesMenu.AddItem(new MenuItem("fCombat", "Use Flash").SetValue(true));
                    combatMenu.AddSubMenu(gravesMenu);
                }
                combatMenu.AddItem(new MenuItem("combatmode", "Combat Mode").SetValue(true));
                Config.AddSubMenu(combatMenu);
            }
            var itemMenu = new Menu("Item Settings", "Item Settings");
            {
                var botrk = new Menu("BOTRK Settings", "BOTRK Settings");
                {
                    botrk.AddItem(new MenuItem("useBOTRK", "Use BOTRK").SetValue(true));
                    botrk.AddItem(new MenuItem("myhp", "Use if my HP < %").SetValue(new Slider(20, 0, 100)));
                    botrk.AddItem(new MenuItem("theirhp", "Use if enemy HP < %").SetValue(new Slider(20, 0, 100)));
                    itemMenu.AddSubMenu(botrk);
                }
                var qssMenu = new Menu("QSS Settings", "QSS Settings");
                {
                    botrk.AddItem(new MenuItem("use.qss", "Use QSS").SetValue(true));
                    for (int i = 0; i < 7; i++)
                    {
                        qssMenu.AddItem(new MenuItem(qssBuff[i], qssBuff[i]).SetValue(true));
                    }
                    itemMenu.AddSubMenu(qssMenu);
                }

                var ghostBlade = new Menu("GhostBlade Settings", "GhostBlade Settings");
                {
                    ghostBlade.AddItem(new MenuItem("ghostblade", "Use GhostBlade").SetValue(true));
                    itemMenu.AddSubMenu(ghostBlade);
                }
                Config.AddSubMenu(itemMenu);
            }
            var drawMenu = new Menu("Draw Settings", "Draw Settings");
            {
                drawMenu.AddItem(new MenuItem("qDraw", "Q Range").SetValue(new Circle(true, Color.White)));
                drawMenu.AddItem(new MenuItem("eDraw", "E Range").SetValue(new Circle(true, Color.Yellow)));
                drawMenu.AddItem(new MenuItem("aaRangeDraw", "AA Range").SetValue(new Circle(true, Color.Gold)));
                drawMenu.AddItem(new MenuItem("eCrash", "E Crash Prediction").SetValue(true));
                drawMenu.AddItem(new MenuItem("enemyadc.waypoint", "Enemy ADC Waypoint").SetValue(true));
                Config.AddSubMenu(drawMenu);
            }

            Config.AddItem(new MenuItem("masterracec0mb0", "                      HikiCarry Masterrace Mode"));
            Config.AddItem(new MenuItem("condemnMethod", "Condemn Method").SetValue(new StringList(new[] { "HIKIGAYA", "ASUNA" })));
            Config.AddItem(new MenuItem("cType", "Combo Type").SetValue(new StringList(new[] { "BURST", "NORMAL" })));
            Config.AddItem(new MenuItem("hType", "Harass Type").SetValue(new StringList(new[] { "2 SILVER STACK + Q", "2 SILVER STACK + E" })));
            Config.AddItem(new MenuItem("whitelistcondemn", "                          Condemn Whitelist"));
            
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(o => o.IsEnemy))
            {
                Config.AddItem(new MenuItem("condemnset." + enemy.CharData.BaseSkinName, string.Format("Condemn: {0}", enemy.CharData.BaseSkinName)).SetValue(true));
            
            }
            Config.AddItem(new MenuItem("condemnDis", "                           Condemn Distance"));
            Config.AddItem(new MenuItem("condemn.distance", "Condemn Push Distance").SetValue(new Slider(425,1,425)));

            var drawDamageMenu = new MenuItem("RushDrawEDamage", "W Damage").SetValue(true);
            var drawFill = new MenuItem("RushDrawEDamageFill", "W Damage Fill").SetValue(new Circle(true, Color.Yellow));
            
            drawMenu.SubMenu("Damage Draws").AddItem(drawDamageMenu);
            drawMenu.SubMenu("Damage Draws").AddItem(drawFill);
            DamageIndicator.DamageToUnit = GetComboDamage;
            DamageIndicator.Enabled = drawDamageMenu.GetValue<bool>();
            DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
            DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;

            drawDamageMenu.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };
            drawFill.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };
            Config.AddToMainMenu();
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Drawing.OnDraw += Drawing_OnDraw;
            pushDistance = Config.Item("condemn.distance").GetValue<Slider>().Value;
        }
        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var tar = (Obj_AI_Hero)target;
            if (Config.Item("useBOTRK").GetValue<bool>() && ((tar.Health / tar.MaxHealth) < Config.Item("theirhp").GetValue<Slider>().Value) && ((ObjectManager.Player.Health / ObjectManager.Player.MaxHealth) < Config.Item("myhp").GetValue<Slider>().Value))
            {
                if (Items.CanUseItem(3153))
                {
                    Items.UseItem(3153, tar);
                }
            }
            if (Config.Item("ghostblade").GetValue<bool>())
            {
                if (Items.CanUseItem(3142))
                {
                    Items.UseItem(3142);
                }
            }

        }
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("agapcloser").GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(1000))
                {
                    Render.Circle.DrawCircle(gapcloser.Sender.Position, gapcloser.Sender.BoundingRadius, Color.Gold, 5);
                    var targetpos = Drawing.WorldToScreen(gapcloser.Sender.Position);
                    Drawing.DrawText(targetpos[0] - 40, targetpos[1] + 20, Color.Gold, "Gapcloser");
                }
                if (E.CanCast(gapcloser.Sender))
                {
                    E.Cast(gapcloser.Sender);
                }
            }
        }
        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Config.Item("ainterrupt").GetValue<bool>())
            {
                if (sender.IsValidTarget(1000))
                {
                    Render.Circle.DrawCircle(sender.Position, sender.BoundingRadius, Color.Gold, 5);
                    var targetpos = Drawing.WorldToScreen(sender.Position);
                    Drawing.DrawText(targetpos[0] - 40, targetpos[1] + 20, Color.Gold, "Interrupt");
                }
                if (E.CanCast(sender))
                {
                    E.Cast(sender);
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            var qDraw = Config.Item("qDraw").GetValue<Circle>();
            var eDraw = Config.Item("eDraw").GetValue<Circle>();
            var eCrash = Config.Item("eCrash").GetValue<bool>();
            var wpDraw = Config.Item("enemyadc.waypoint").GetValue<bool>();
            var aaDraw = Config.Item("aaRangeDraw").GetValue<Circle>();

            if (qDraw.Active && Q.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, qDraw.Color);
            }
            if (eDraw.Active && E.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, eDraw.Color);
            }
            if (aaDraw.Active)
            {
                Render.Circle.DrawCircle(Player.Position, 550, aaDraw.Color);
            }
            if (eCrash)
            {
                foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(1100)))
                {
                    var EPred = E.GetPrediction(En);
                    int pushDist = 425;
                    for (int i = 0; i < pushDist; i += (int)En.BoundingRadius)
                    {
                        Vector3 loc3 = EPred.UnitPosition.To2D().Extend(Player.Position.To2D(), -i).To3D();
                        if (loc3.IsWall())
                        {
                            Render.Circle.DrawCircle(loc3, En.BoundingRadius, Color.Yellow);
                        }
                    }
                }
            }
            if (wpDraw)
            {
                wayPoint();
            }
            
            
        }
        static bool isAllyFountain(Vector3 Position)
        {
            float fountainRange = 750;
            var map = Utility.Map.GetMap();
            if (map != null && map.Type == Utility.Map.MapType.SummonersRift)
            {
                fountainRange = 1050;
            }
            return
                ObjectManager.Get<GameObject>().Where(spawnPoint => spawnPoint is Obj_SpawnPoint && spawnPoint.IsAlly).Any(spawnPoint => Vector2.Distance(Position.To2D(), spawnPoint.Position.To2D()) < fountainRange);
        }
        private static void hikiCondemn(Obj_AI_Hero target) 
        {
            if (Vector3.Distance(Player.ServerPosition, target.ServerPosition) < E.Range)
            {
               
                if (Player.ServerPosition.Extend(target.ServerPosition, pushDistance).IsWall())
                {
                    E.CastOnUnit(target);
                }
            }
        }
        private static void Combo()
        {
            var condemnDistance = Config.Item("condemn.distance").GetValue<Slider>().Value;
            switch (Config.Item("cType").GetValue<StringList>().SelectedIndex)
            {
               case 0:
                    if (Q.IsReady() && Config.Item("qCombo").GetValue<bool>())
            {
                foreach (Obj_AI_Hero qTarget in HeroManager.Enemies.Where(x => x.IsValidTarget(550)))
                {
                    if (qTarget.Buffs.Any(buff => buff.Name == "vaynesilvereddebuff" && buff.Count == 2))
                    {
                        Q.Cast(Game.CursorPos);
                        if (Config.Item("ghostblade").GetValue<bool>() && Items.CanUseItem(3142))
                        {
                                Items.UseItem(3142);
                        }
                    }
                }
            }
            if (E.IsReady() && Config.Item("eCombo").GetValue<bool>())
            {
                switch (Config.Item("condemnMethod").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        foreach (var enemy in HeroManager.Enemies.Where(hero => hero.IsValidTarget(E.Range) && !hero.HasBuffOfType(BuffType.SpellShield) && !hero.HasBuffOfType(BuffType.SpellImmunity)
                            && Config.Item("condemnset." + hero.CharData.BaseSkinName).GetValue<bool>()))
                        {
                            hikiCondemn(enemy);
                        }
                        break;
                    case 1:
                        foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(E.Range) && !hero.HasBuffOfType(BuffType.SpellShield) && !hero.HasBuffOfType(BuffType.SpellImmunity)
                             && Config.Item("condemnset." + hero.CharData.BaseSkinName).GetValue<bool>()))
                        {
                            var EPred = E.GetPrediction(En);
                            int pushDist = pushDistance;
                            var FinalPosition = EPred.UnitPosition.To2D().Extend(Player.ServerPosition.To2D(), -pushDist).To3D();

                            for (int i = 1; i < pushDist; i += (int)En.BoundingRadius)
                            {
                                Vector3 loc3 = EPred.UnitPosition.To2D().Extend(Player.ServerPosition.To2D(), -i).To3D();

                                if (loc3.IsWall() || isAllyFountain(FinalPosition))
                                    E.Cast(En);
                            }
                        }
                        break;
                }
            }
            if (R.IsReady() && Config.Item("rCombo").GetValue<bool>() && Player.CountEnemiesInRange(1000) >= Config.Item("rComboxEnemy").GetValue<Slider>().Value)
            {
                R.Cast();
            }

            break;
                case 1:
            if (Q.IsReady() && Config.Item("qCombo").GetValue<bool>())
            {
                foreach (Obj_AI_Hero qTarget in HeroManager.Enemies.Where(x => x.IsValidTarget(550)))
                {
                    Q.Cast(Game.CursorPos);
                }
            }

            if (E.IsReady() && Config.Item("eCombo").GetValue<bool>())
            {
                foreach (var En in HeroManager.Enemies.Where(hero => hero.IsValidTarget(E.Range) && !hero.HasBuffOfType(BuffType.SpellShield) && !hero.HasBuffOfType(BuffType.SpellImmunity)))
                {
                    var EPred = E.GetPrediction(En);
                    int pushDist = 425;
                    var FinalPosition = EPred.UnitPosition.To2D().Extend(Player.ServerPosition.To2D(), -pushDist).To3D();

                    for (int i = 1; i < pushDist; i += (int)En.BoundingRadius)
                    {
                        Vector3 loc3 = EPred.UnitPosition.To2D().Extend(Player.ServerPosition.To2D(), -i).To3D();

                        if (loc3.IsWall() || isAllyFountain(FinalPosition))
                            E.Cast(En);
                    }
                }
            }
            if (R.IsReady() && Config.Item("rCombo").GetValue<bool>() && Player.CountEnemiesInRange(1000) >= Config.Item("rComboxEnemy").GetValue<Slider>().Value)
            {
                R.Cast();
            }

            break;
            }
        }
        public static void DrawText(Font font, String text, int posX, int posY, Color color)
        {
            Rectangle rec = font.MeasureText(null, text, FontDrawFlags.Center);
            font.DrawText(null, text, posX + 1 + rec.X, posY + 1, SharpColor.White);
        }
        private static void wayPoint() // SOON TM
        {
            foreach (Obj_AI_Hero enemy in
               ObjectManager.Get<Obj_AI_Hero>()
                   .Where(enemy => enemy.IsEnemy && enemy.IsVisible && Player.Distance(enemy.Position) < 1000 && !enemy.IsDead && enemy.IsValid && enemy.IsMoving && Marksman.Contains(enemy.CharData.BaseSkinName.ToString())))
                    {
                        List<Vector2> wpoints = enemy.GetWaypoints();
                        int nPoint = wpoints.Count - 1;
                        int lPoint = wpoints.Count - 2;
                        float timer = 0;
                        for (int i = 0; i < nPoint; i++)
                        {
                            Vector3 bPoint = wpoints[i].To3D();
                            Vector3 ePoint = wpoints[i + 1].To3D();
                            timer += bPoint.Distance(ePoint) / ObjectManager.Player.MoveSpeed;
                            Vector2 p1 = Drawing.WorldToScreen(bPoint);
                            Vector2 p2 = Drawing.WorldToScreen(ePoint);
                            if (i != lPoint)
                            {
                                Drawing.DrawLine(p1[0], p1[1], p2[0], p2[1], 2, Color.White);
                            }
                            else
                            {
                                float r = 25 / p2.Distance(p1);
                                var enp = new Vector2(r * p1.X + (1 - r) * p2.X, r * p1.Y + (1 - r) * p2.Y);
                                Drawing.DrawLine(p1[0], p1[1], enp[0], enp[1], 2, Color.White);
                                Render.Circle.DrawCircle(ePoint, 50, Color.Black);
                                Render.Circle.DrawCircle(ePoint, 50, Color.FromArgb(50, Color.Gold), -2);
                                DrawText(hikiFont, timer.ToString("F"), (int)p2[0], (int)p2[1] - 10, Color.White);
                            }
                        }
                    }
        }
        private static IEnumerable<Vector3> CondemnPosition()
        {
            var pointList = new List<Vector3>();
            var j = 300;
            var offset = (int)(2 * Math.PI * j / 100);
            for (var i = 0; i <= offset; i++)
            {
                var angle = i * Math.PI * 2 / offset;
                var point = new Vector3((float)(ObjectManager.Player.Position.X + j * Math.Cos(angle)),
                    (float)(ObjectManager.Player.Position.Y - j * Math.Sin(angle)),
                    ObjectManager.Player.Position.Z);

                if (!NavMesh.GetCollisionFlags(point).HasFlag(CollisionFlags.Wall))
                    pointList.Add(point);
            }
            return pointList;
        }
        private static void combatGraves()
        {
            if (Config.Item("combatgraves").GetValue<bool>() && Q.IsReady() && E.IsReady())
            {
                var pushDistance = 400;
                Flash = Player.GetSpellSlot("SummonerFlash");
                float range1000 = Player.CountEnemiesInRange(1000);
                float range750  = Player.CountEnemiesInRange(750);
                float range650  = Player.CountEnemiesInRange(750);


                switch (Config.Item("positioning").GetValue<StringList>().SelectedIndex)
                {
                    case 0:
                        foreach (var vsGraves in HeroManager.Enemies.Where(h => h.IsValidTarget(1000) && h.ChampionName == "Graves"))
                        {
                            if (range1000 == 1 && vsGraves.ChampionName == "Graves")
                            {
                                if (R.IsReady() && Config.Item("rCombat").GetValue<bool>() && range750 == 1)
                                {
                                    if (vsGraves.IsValidTarget(650) && range650 == 1)
                                    {
                                        R.Cast();
                                    }
                                }
                                if (Config.Item("fCombat").GetValue<bool>() && Config.Item("eCombat").GetValue<bool>() && Flash.IsReady() && vsGraves.IsValidTarget(E.Range))
                                {
                                    foreach (
                             var qPosition in
                               CondemnPosition()
                               .OrderBy(qPosition => qPosition.Distance(vsGraves.ServerPosition)))
                                    {
                                        if (qPosition.Distance(vsGraves.Position) < E.Range)
                                            E.UpdateSourcePosition(qPosition, qPosition);
                                        var targetPosition = E.GetPrediction(vsGraves).CastPosition;
                                        var finalPosition = targetPosition.Extend(qPosition, 400);
                                        if (finalPosition.IsWall())
                                        {
                                            Player.Spellbook.CastSpell(Flash, qPosition);

                                        }
                                        if (E.IsReady())
                                        {
                                            E.Cast(vsGraves);
                                        }

                                    }
                                }
                                if (Q.IsReady() && Config.Item("qCombat").GetValue<bool>())
                                {
                                    if (vsGraves.IsValidTarget(Q.Range))
                                    {
                                        Q.Cast(Game.CursorPos);
                                    }
                                }

                            }
                        }
                        break;
                    case 1:
                        foreach (var vsGraves in HeroManager.Enemies.Where(h => h.IsValidTarget(1000) && h.ChampionName == "Graves"))
                        {
                            if (range1000 == 1 && vsGraves.ChampionName == "Graves")
                            {
                                if (R.IsReady() && Config.Item("rCombat").GetValue<bool>() && range750 == 1)
                                {
                                    if (vsGraves.IsValidTarget(650) && range650 == 1)
                                    {
                                        R.Cast();
                                    }
                                }
                                if (Config.Item("fCombat").GetValue<bool>() && Config.Item("eCombat").GetValue<bool>() && Flash.IsReady() && vsGraves.IsValidTarget(E.Range))
                                {
                                    foreach (
                                var qPosition in
                                  CondemnPosition()
                                  .OrderBy(qPosition => qPosition.Distance(vsGraves.ServerPosition)))
                                    {
                                        if (qPosition.Distance(vsGraves.Position) < E.Range)
                                            E.UpdateSourcePosition(qPosition, qPosition);
                                        var targetPosition = E.GetPrediction(vsGraves).CastPosition;
                                        var finalPosition = targetPosition.Extend(qPosition, -pushDistance);
                                        if (finalPosition.IsWall())
                                        {
                                            Q.Cast(qPosition);

                                        }
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Harass();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                //soon(tm)
            }
            if (Config.Item("bT").GetValue<bool>())
            {
                BlueOrb();
            }
            if (Config.Item("combatmode").GetValue<bool>())
            {
                if (Config.Item("combatGraves").GetValue<bool>())
                {
                    combatGraves();
                }
            }
            if (Config.Item("use.qss").GetValue<bool>())
            {
                qssSupport();
            }
        }
        private static void BlueOrb()
        {
            if (Config.Item("bT").GetValue<bool>())
            {
                if (Player.Level >= Config.Item("bluetrinketlevel").GetValue<Slider>().Value && Player.InShop() && !(Items.HasItem(3342) || Items.HasItem(3363)))
                {
                    Player.BuyItem(ItemId.Scrying_Orb_Trinket);
                }
            }
        }
        private static void Harass()
        {
            switch (Config.Item("hType").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    if (ObjectManager.Player.ManaPercent > Config.Item("manaHarass").GetValue<Slider>().Value)
                    {
                        if (Q.IsReady() && Config.Item("qHarass").GetValue<bool>())
                        {
                            foreach (Obj_AI_Hero qTarget in HeroManager.Enemies.Where(x => x.IsValidTarget(550)))
                            {
                                if (qTarget.Buffs.Any(buff => buff.Name == "vaynesilvereddebuff" && buff.Count == 2))
                                {
                                    Q.Cast(Game.CursorPos);
                                }
                            }
                        }
                    }
                    
                    break;
                case 1:
                    if (ObjectManager.Player.ManaPercent > Config.Item("manaHarass").GetValue<Slider>().Value)
                    {
                        if (E.IsReady() && Config.Item("eHarass").GetValue<bool>())
                        {
                            foreach (Obj_AI_Hero eTarget in HeroManager.Enemies.Where(x => x.IsValidTarget(E.Range)))
                            {
                                if (eTarget.Buffs.Any(buff => buff.Name == "vaynesilvereddebuff" && buff.Count == 2))
                                {
                                    E.Cast(eTarget);
                                }
                            }
                        }
                    }
                    
                    break;
            }
        }
        private static float GetComboDamage(Obj_AI_Hero hero)
        {
            float damage = 0;
            foreach (var Buff in hero.Buffs)
            {
                if (Buff.Name == "vaynesilvereddebuff" && Buff.Count == 2)
                {
                    return W.GetDamage(hero);
                }
            }

            if (damage > hero.Health)
            {
                var yx = Drawing.WorldToScreen(hero.Position);
                Drawing.DrawText(yx[0], yx[1], System.Drawing.Color.White, "BASIC ATTACK = KILL");
            }
            return 0;
        }
        private static void channelBroke()
        {
            var brokeUlti = Config.Item("broke.ulti").GetValue<bool>();
            if (brokeUlti)
            {
                var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical, true);
                var kataUlti = Config.Item("katarina.ulti").GetValue<bool>();
                var missUlti = Config.Item("missfortune.ulti").GetValue<bool>();

                if (eTarget.HasBuff("KatarinaR") || eTarget.HasBuff("katarinarsound") && Player.Distance(eTarget.Position) <= E.Range)
                {
                    E.Cast(eTarget);
                }
                if (eTarget.HasBuff("missfortunebulletsound") || eTarget.HasBuff("MissFortuneBulletTime") && Player.Distance(eTarget.Position) <= E.Range)
                {
                    E.Cast(eTarget);
                }
            }
        }
        private static void qssSupport()
        {
            for (int i = 0; i < 7; i++)
            {
                if (Player.HasBuff("BuffType." + qssBuff[i]) && Config.Item("BuffType." + qssBuff[i]).GetValue<bool>())
                {
                    if (Items.CanUseItem(3140) && Items.HasItem(3140))
                    {
                        Items.UseItem(3140);
                    }
                    if (Items.CanUseItem(3139) && Items.HasItem(3137))
                    {
                        Items.UseItem(3139);
                    }
                }
            }
        }
    }
}
