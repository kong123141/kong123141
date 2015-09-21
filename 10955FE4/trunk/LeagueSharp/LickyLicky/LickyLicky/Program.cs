using System;
using System.Collections.Generic;
using System.Drawing.Configuration;
using System.Linq;
using System.Windows.Forms;
using SharpDX;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using Menu = LeagueSharp.Common.Menu;
using MenuItem = LeagueSharp.Common.MenuItem;

namespace LickyLicky
{
    class Program
    {
        static Menu mainMenu = new Menu("Tahm", "Tahm", true);
        private static string[] allyNames;
        private static Obj_AI_Hero Player;
        private static string buffName = "TahmKenchPDebuffCounter";
        private static string tahmSlow = "tahmkenchwhasdevouredtarget";
        private static SwallowedTarget current = SwallowedTarget.None;
        private static bool usedWEnemy = false;
        private static bool usedWMinion = false;
        private static Spell Q, W, W2, E, R;

        enum SwallowedTarget
        {
            Enemy,
            Ally,
            Minion,
            None
        }
        static void Main(string[] args)
        {
            try
            {
                CustomEvents.Game.OnGameLoad += OnLoad;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void OnLoad(EventArgs args)
        {
            Player = LeagueSharp.ObjectManager.Player;
            if (!Player.CharData.BaseSkinName.ToLower().Contains("tahm"))
                return;
            Q = new Spell(SpellSlot.Q, 800);
            Q.SetSkillshot(.1f, 75, 2000, true, SkillshotType.SkillshotLine);
            W = new Spell(SpellSlot.W, 250);
            W2 = new Spell(SpellSlot.W, 900); //Not too sure on this value
            W2.SetSkillshot(.1f, 75, 900, true, SkillshotType.SkillshotLine); //Not sure on these values either.
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 4000);

            PopulateMenu();
            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            Interrupter2.OnInterruptableTarget += OnInterruptableSpell;
            Drawing.OnDraw += Drawing_OnDraw;

        }

        static void PopulateMenu()
        {
            Menu harassMenu = new Menu("Harass", "Harass");
            harassMenu.AddItem(new MenuItem("Harass", "Harass").SetValue(new KeyBind('C', KeyBindType.Press)));
            harassMenu.AddItem(new MenuItem("Use Q", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("Use W", "Use W").SetValue(true));
            harassMenu.AddItem(new MenuItem("Harass Mana Percent", "Harass Mana Percent").SetValue(new Slider(30)));


            Menu shieldMenu = new Menu("Shield", "Shield");
            shieldMenu.AddItem(new MenuItem("E Only Dangerous", "E Only Dangerous").SetValue(false));
            shieldMenu.AddItem(new MenuItem("E For Percent Of HP Damage", "E For Percent Of HP Damage").SetValue(new Slider(30)));
            shieldMenu.AddItem(new MenuItem("Devour Ally", "Devour Ally").SetValue(false));
            shieldMenu.AddItem(new MenuItem("Devour Ally at Percent HP", "Devour Ally at Percent HP").SetValue(new Slider(10)));

            Menu alliesMenu = new Menu("Allies", "Allies");
            Obj_AI_Hero[] allies = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsAlly && !x.IsMe).ToArray();
            allyNames = new string[allies.Count()];
            for (int i = 0; i < allies.Count(); i++)
            {
                allyNames[i] = allies[i].ChampionName;
                alliesMenu.AddItem(new MenuItem(allyNames[i], allyNames[i]).SetValue(true));
            }

            shieldMenu.AddSubMenu(alliesMenu);


            //Menu killStealMenu = new Menu("Killsteal", "Killsteal");
            mainMenu.AddItem(new MenuItem("Killsteal", "Killsteal").SetValue(true));


            Menu fleeMenu = new Menu("Flee", "Flee");
            fleeMenu.AddItem(new MenuItem("Flee", "Flee").SetValue(new KeyBind('Z', KeyBindType.Press)));
            fleeMenu.AddItem(new MenuItem("Use Q", "Use Q").SetValue(true));
            fleeMenu.AddItem(new MenuItem("Bring Ally", "Bring Ally").SetValue(true));


            Menu comboMenu = new Menu("Combo", "Combo");
            comboMenu.AddItem(new MenuItem("Combo", "Combo").SetValue(new KeyBind(' ', KeyBindType.Press)));
            comboMenu.AddItem(new MenuItem("Use Q", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("Use W", "Use W").SetValue(true));


            Menu miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("Interrupt With Q", "Interrupt With Q").SetValue(true));
            miscMenu.AddItem(new MenuItem("Interrupt with W", "Interrupt With W").SetValue(true));
            miscMenu.AddItem(new MenuItem("Default W Move Location", "Default W Move Location").SetValue(new StringList(new string[] { "Cursor Position", "Nearest Ally", "Turret" })) );


            Menu orbwalkingMenu = new Menu("Orbwalking", "Orbwalking");
            Orbwalking.Orbwalker walker = new Orbwalking.Orbwalker(orbwalkingMenu);

            Menu drawingMenu = new Menu("Drawing","Drawing");
            drawingMenu.AddItem(new MenuItem("Draw Q", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("Draw W", "Draw W").SetValue(true));
            drawingMenu.AddItem(new MenuItem("Draw Max W Movement", "Draw Max W Movement").SetValue(true));

            mainMenu.AddSubMenu(orbwalkingMenu);
            mainMenu.AddSubMenu(harassMenu);
            mainMenu.AddSubMenu(shieldMenu);
            //mainMenu.Add(killStealMenu);
            mainMenu.AddSubMenu(fleeMenu);
            mainMenu.AddSubMenu(comboMenu);
            mainMenu.AddSubMenu(miscMenu);
            mainMenu.AddSubMenu(drawingMenu);

            mainMenu.AddToMainMenu();
        }
        static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            //Arbitrary until I figure out how the Tahm W slow is found in code.
            if (current == SwallowedTarget.Enemy && Player.MoveSpeed < 150)
            {
                moveToPosition();
            }

            if (mainMenu.Item("Killsteal").IsActive())
            {
                try
                {
                    Obj_AI_Hero target =
                        ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(
                            x => x.Health <= Math.Max(Player.GetSpellDamage(x, SpellSlot.Q), Player.GetSpellDamage(x, SpellSlot.W)) && x.IsEnemy && x.Distance(Player) <= 650 && !x.IsDead);
                    Spell s = (Player.GetSpellDamage(target, SpellSlot.Q) > Player.GetSpellDamage(target, SpellSlot.W)) ? Q : W;
                    if (target != null)
                    {
                        if(s.Slot == SpellSlot.Q)
                        s.Cast(target);
                        else if(current==SwallowedTarget.None || current ==SwallowedTarget.Minion)
                        {
                            if (target.GetBuffCount(buffName)==3 && current==SwallowedTarget.None && target.Distance(Player)<=300f)
                                W.CastOnUnit(target);
                            else if(current==SwallowedTarget.None)
                                W.CastOnUnit(
                                    ObjectManager.Get<Obj_AI_Minion>()
                                        .Where(x => x.IsEnemy)
                                        .OrderBy(x => x.Distance(Player))
                                        .First());
                            else
                                W2.Cast(target);
                        }
                    }
                }
                catch { }
            }

            if (mainMenu.SubMenu("Combo").Item("Combo").IsActive())
            {
                Obj_AI_Hero target =
                    ObjectManager.Get<Obj_AI_Hero>().Where(x => x.Distance(Player) <= Q.Range && x.IsEnemy && x.IsTargetable && !x.IsDead)
                        .OrderByDescending(x => x.GetBuffCount(buffName))
                        .ThenBy(x => x.Distance(Player))
                        .FirstOrDefault();
                if (target != null)
                {
                    int buffCount = target.GetBuffCount(buffName);
                    switch (buffCount)
                    {
                        case -1:
                        case 1:
                        case 2:
                            if(mainMenu.SubMenu("Combo").Item("Use Q").IsActive())
                            Q.Cast(target);
                            if (target.Distance(Player) <= 200)
                                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                            break;
                        case 3:
                            if(mainMenu.SubMenu("Combo").Item("Use Q").IsActive())
                            Q.Cast(target);

                            if (current == SwallowedTarget.None && mainMenu.SubMenu("Combo").Item("Use W").IsActive())
                                W.CastOnUnit(target);
                            break;
                    }
                }
            }
            else if (mainMenu.SubMenu("Harass").Item("Harass").IsActive())
            {

                if (Player.ManaPercent >= mainMenu.SubMenu("Harass").Item("Harass Mana Percent").GetValue<Slider>().Value)
                {
                    Obj_AI_Hero target =
                        ObjectManager.Get<Obj_AI_Hero>().Where(x => x.Distance(Player) <= Q.Range && x.IsEnemy && x.IsTargetable && !x.IsDead)
                            .OrderByDescending(x => x.GetBuffCount(buffName))
                            .ThenBy(x => x.Distance(Player))
                            .FirstOrDefault();
                    if (target != null)
                    {
                        if (mainMenu.SubMenu("Harass").Item("Use Q").IsActive())
                        Q.Cast(target);
                        if (target.Distance(Player) <= 250 && target.GetBuffCount(buffName) == 3 && current == SwallowedTarget.None && mainMenu.SubMenu("Harass").Item("Use W").IsActive())
                            W.CastOnUnit(target);
                        else if (current == SwallowedTarget.None && mainMenu.SubMenu("Harass").Item("Use W").IsActive())
                        {
                            W.CastOnUnit(ObjectManager.Get<Obj_AI_Minion>().Where(x=> x.IsEnemy).OrderBy(x => x.Distance(Player)).First());
                        }
                        else if (current == SwallowedTarget.Minion && mainMenu.SubMenu("Harass").Item("Use W").IsActive())
                        {
                            W2.Cast(target.Position);
                        }
                    }
                }
            }
            else if (mainMenu.SubMenu("Flee").Item("Flee").IsActive())
            {

                try
                {
                    if (mainMenu.SubMenu("Flee").Item("Use Q").IsActive())
                    {
                        Obj_AI_Hero target =
                            ObjectManager.Get<Obj_AI_Hero>().Where(
                                x => x.Distance(Player) <= Q.Range && x.IsEnemy && x.IsTargetable && !x.IsDead)
                                .OrderByDescending(x => x.GetBuffCount(buffName))
                                .ThenBy(x => x.Distance(Player))
                                .FirstOrDefault();
                        if (target != null)
                            Q.Cast(target);
                    }
                }
                catch { }

                if (mainMenu.SubMenu("Flee").Item("Bring Ally").IsActive())
                {

                    try
                    {
                        var swallowAlly =
                        ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(
                            x => x.HealthPercent < mainMenu.SubMenu("Shield").Item("Devour Ally at Percent HP").GetValue<Slider>().Value
                                && x.IsAlly && Player.Distance(x) <= 500
                                && !x.IsDead);
                        if (current == SwallowedTarget.None)
                            W.CastOnUnit(swallowAlly);

                    }catch { }
                }
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            }

        }
        static void moveToPosition()
        {
            Vector3 point = new Vector3();
            switch (mainMenu.SubMenu("Misc").Item("Default W Move Location").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    point = Game.CursorPos;
                    break;
                case 1:
                    point = ObjectManager.Get<Obj_AI_Hero>().Where(x => !x.IsDead && x.IsAlly).OrderBy(x => x.Distance(Player)).FirstOrDefault().Position;
                    break;
                case 2:
                    point = ObjectManager.Get<Obj_AI_Turret>().Where(x => !x.IsDead && x.IsAlly).OrderBy(x => x.Distance(Player)).FirstOrDefault().Position;
                    break;
            }
            Orbwalking.MoveTo(point);
        }

        static void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            //Modified Kalista Soulbound code from Corey
            //Need to check in fountain otherwise recalls could make you swallow
            if (sender.Type == GameObjectType.obj_AI_Hero && sender.IsEnemy && !Player.InFountain())
            {
                try
                {
                    var swallowAlly =
                        ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(
                            x => x.HealthPercent < mainMenu.SubMenu("Shield").Item("Devour Ally at Percent HP").GetValue<Slider>().Value
                                && x.IsAlly && Player.Distance(x) <= 500
                                && !x.IsDead);
                    if (swallowAlly != null && current ==SwallowedTarget.None && W.IsReady())
                    {
                        W.CastOnUnit(swallowAlly);
                    }

                }
                catch { }
                Obj_AI_Hero enemy = (Obj_AI_Hero)sender;
                try
                {

                    SpellDataInst s = enemy.Spellbook.Spells.FirstOrDefault(x => x.SData.Name.Equals(args.SData.Name));
                    if (enemy.GetSpellDamage(Player, s.Slot) > Player.Health && mainMenu.SubMenu("Shield").Item("E Only Dangerous").IsActive())
                        E.Cast();
                    else if ((enemy.GetSpellDamage(Player, s.Slot)) / Player.MaxHealth <=
                             mainMenu.SubMenu("Shield").Item("E for Percent of HP Damage").GetValue<Slider>().Value)
                        E.Cast();
                }
                catch { }
            }
            else if (sender.IsMe)
            {
                SpellSlot s = Player.Spellbook.Spells.First(x => x.SData.Name == args.SData.Name).Slot;
                if (s.ToString().Equals("W"))
                {
                    if (args.Target.Type == GameObjectType.obj_AI_Hero)
                        current = (args.Target.IsAlly) ? SwallowedTarget.Ally : SwallowedTarget.Enemy;
                    else
                        current = SwallowedTarget.Minion;

                }
                else if(s.ToString().Equals("46"))
                    current = SwallowedTarget.None;
                
            }
        }

        static void OnInterruptableSpell(object enemy, Interrupter2.InterruptableTargetEventArgs args)
        {
            try
            {
                Obj_AI_Hero sender = (Obj_AI_Hero)enemy;
                if (sender.IsAlly)
                    return;
                float distance = sender.Distance(Player);
                if (sender.GetBuffCount(buffName) == 3)
                {
                    if (distance <= W.Range && mainMenu.SubMenu("Misc").Item("Interrupt With W").IsActive())
                        W.CastOnUnit(sender);
                    else if (distance <= Q.Range && mainMenu.SubMenu("misc").Item("Interrupt With Q").IsActive())
                        Q.Cast(sender);
                }
                else
                {
                    if (distance <= 250)
                        Player.IssueOrder(GameObjectOrder.AttackUnit, sender);
                    if (distance <= Q.Range)
                        Q.Cast(sender);
                }


            }
            catch{}
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;
            if(mainMenu.SubMenu("Drawing").Item("Draw Q").IsActive())
                Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Red);
            if (mainMenu.SubMenu("Drawing").Item("Draw W").IsActive())
            {
                if(current == SwallowedTarget.None)
                    Render.Circle.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Green);
                else if (current == SwallowedTarget.Minion)
                    Render.Circle.DrawCircle(Player.Position, W2.Range, System.Drawing.Color.Green);
            }
            if (mainMenu.SubMenu("Drawing").Item("Draw Max W Movement").IsActive() && current == SwallowedTarget.None)
                Render.Circle.DrawCircle(Player.Position, ((Player.MoveSpeed * .05f *.05f) +110) * 4, System.Drawing.Color.MediumSpringGreen);

        }
    }
}