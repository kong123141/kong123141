using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Media;
using System.Net.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows.Input;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.SDK.Core.UI.INotifications;
using SharpDX;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Events;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Wrappers;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

namespace TahmSDKench
{
    class Program
    {
        static Menu mainMenu = new Menu("Tahm", "Tahm", true);
        private static string[] allyNames;
        private static Obj_AI_Hero Player;
        private static string buffName = "TahmKenchPDebuffCounter";
        private static string tahmSlow = "tahmkenchwhasdevouredtarget";
        private static bool usedWEnemy = false;
        private static bool usedWMinion = false;
        private static Spell Q, W, W2, E, R;
        static void Main(string[] args)
        {
            try
            {
                Bootstrap.Init(null);

                Load.OnLoad += OnLoad;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void OnLoad(object sender, EventArgs args)
        {
            Player = LeagueSharp.ObjectManager.Player;
            if (!Player.CharData.BaseSkinName.ToLower().Contains("tahm"))
                return;
            Q = new Spell(SpellSlot.Q, 800);
            Q.SetSkillshot(.1f, 75, 2000, true, SkillshotType.SkillshotLine);
            W = new Spell(SpellSlot.W, 250);
            W2 = new Spell(SpellSlot.W, 700); //Not too sure on this value
            W2.SetSkillshot(true, SkillshotType.SkillshotLine);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 4000);

            Console.WriteLine("Spells done");
            PopulateMenu();
            Console.WriteLine("Populated menu");
            Game.OnUpdate += OnUpdate;
            Console.WriteLine("On Update added");
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            InterruptableSpell.OnInterruptableTarget += OnInterruptableSpell;
        }

        static void PopulateMenu()
        {
            Menu harassMenu = new Menu("Harass", "Harass");
            harassMenu.Add(new MenuKeyBind("Harass", "Harass", Keys.C, KeyBindType.Press));
            harassMenu.Add(new MenuBool("Use Q","Use Q", true));
            harassMenu.Add(new MenuBool("Use W", "Use W", true));
            harassMenu.Add(new MenuSlider("Harass Mana Percent", "Harass Mana Percent", 30, 0, 100));


            Menu shieldMenu = new Menu("Shield", "Shield");
            shieldMenu.Add(new MenuBool("E Only Dangerous", "E Only Dangerous", false));
            shieldMenu.Add(new MenuSlider("E For Percent Of HP Damage", "E For Percent Of HP Damage", 30));
            shieldMenu.Add(new MenuBool("Devour Ally", "Devour Ally"));
            shieldMenu.Add(new MenuSlider("Devour Ally at Percent HP", "Devour Ally at Percent HP",10));

            Menu alliesMenu = new Menu("Allies","Allies");
            Obj_AI_Hero[] allies = GameObjects.AllyHeroes.ToArray();
            allyNames = new string[allies.Count()];
            for (int i = 0; i < allies.Count(); i++)
            {
                allyNames[i] = allies[i].ChampionName;
                alliesMenu.Add(new MenuBool(allyNames[i], allyNames[i]));
            }

            shieldMenu.Add(alliesMenu);


            //Menu killStealMenu = new Menu("Killsteal", "Killsteal");
            mainMenu.Add(new MenuBool("Killsteal", "Killsteal"));


            Menu fleeMenu = new Menu("Flee", "Flee");
            fleeMenu.Add(new MenuKeyBind("Flee", "Flee", Keys.Z, KeyBindType.Press));
            fleeMenu.Add(new MenuBool("Use Q", "Use Q"));
            fleeMenu.Add(new MenuBool("Bring Ally", "Bring Ally"));


            Menu comboMenu = new Menu("Combo", "Combo");
            comboMenu.Add(new MenuKeyBind("Combo", "Combo", Keys.Space, KeyBindType.Press));
            comboMenu.Add(new MenuBool("Use Q", "Use Q"));
            comboMenu.Add(new MenuBool("Use W", "Use W"));


            Menu miscMenu = new Menu("Misc", "Misc");
            miscMenu.Add(new MenuBool("Interrupt With Q", "Interrupt With Q", true));
            miscMenu.Add(new MenuBool("Interrupt with W", "Interrupt With W", true));
            miscMenu.Add(new MenuList<String>("Default W Move Location", "Default W Move Location", new string[] { "Cursor Position", "Nearest Ally", "Turret" }));


            mainMenu.Add(harassMenu);
            mainMenu.Add(shieldMenu);
            //mainMenu.Add(killStealMenu);
            mainMenu.Add(fleeMenu);
            mainMenu.Add(comboMenu);
            mainMenu.Add(miscMenu);

            mainMenu.Attach();
        }
        static void OnUpdate(EventArgs args)
        {
            if(Player.IsDead)
                return;

            //Arbitrary until I figure out how the Tahm W slow is found in code.
            if (Player.HasBuff(tahmSlow) && Player.MoveSpeed<150)
            {
                moveToPosition();
                usedWEnemy = true;
            }
            else usedWEnemy = false;

            if (mainMenu["Killsteal"].GetValue<MenuBool>().Value)
            {
                try
                {
                    Obj_AI_Hero target =
                        GameObjects.EnemyHeroes.FirstOrDefault(
                            x => x.Health <= Player.GetSpellDamage(x, SpellSlot.Q) && x.Distance(Player) <= 650);
                    if (target != null)
                        Q.Cast(target);
                }catch{}
            }

            if (mainMenu["Combo"]["Combo"].GetValue<MenuKeyBind>().Active)
            {
                Obj_AI_Hero target =
                    GameObjects.EnemyHeroes.Where(x => x.Distance(Player) <= Q.Range && x.IsTargetable && !x.IsDead)
                        .OrderByDescending(x => x.GetBuffCount(buffName))
                        .ThenBy(x => x.Distance(Player))
                        .FirstOrDefault();
                if (target!=null)
                {
	                int buffCount = target.GetBuffCount(buffName);
	                switch (buffCount)
	                {
	                    case -1:
	                    case 1:
	                    case 2:
	                        Q.Cast(target);
	                        if (target.Distance(Player) <= 200)
	                            Player.IssueOrder(GameObjectOrder.AttackUnit, target);
	                        break;
	                    case 3:
	                        Q.CastIfHitchanceMinimum(target, HitChance.Medium);
	                        if(!usedWEnemy)
	                        W.CastOnUnit(target);
	                        break;
	                }
                }
            }
            else if (mainMenu["Harass"]["Harass"].GetValue<MenuKeyBind>().Active)
            {

                if (Player.ManaPercent >= mainMenu["Harass"]["Harass Mana Percent"].GetValue<MenuSlider>().Value)
                {
                    Obj_AI_Hero target =
                        GameObjects.EnemyHeroes.Where(x => x.Distance(Player) <= Q.Range && x.IsTargetable && !x.IsDead)
                            .OrderByDescending(x => x.GetBuffCount(buffName))
                            .ThenBy(x => x.Distance(Player))
                            .FirstOrDefault();
                    if (target != null)
                    {
                        Q.Cast(target);
                        if (target.Distance(Player) <= 250 && !usedWEnemy && !usedWMinion)
                            W.CastOnUnit(target);
                        else if (!usedWEnemy && !usedWMinion)
                        {
                            W.CastOnUnit(GameObjects.EnemyMinions.OrderBy(x => x.Distance(Player)).First());
                            usedWMinion = true;
                        }
                        else if (usedWMinion)
                        {
                            W2.Cast(target.Position);
                            usedWMinion = false;
                        }
                    }
                }
            }
            else if (mainMenu["Flee"]["Flee"].GetValue<MenuKeyBind>().Active)
            {

                try
                {
                    if (mainMenu["Flee"]["Use Q"].GetValue<MenuBool>().Value)
                    {
                        Obj_AI_Hero target =
                            GameObjects.EnemyHeroes.Where(
                                x => x.Distance(Player) <= Q.Range && x.IsTargetable && !x.IsDead)
                                .OrderByDescending(x => x.GetBuffCount(buffName))
                                .ThenBy(x => x.Distance(Player))
                                .FirstOrDefault();
                        if (target != null)
                            Q.Cast(target);
                    }
                }catch{}

                if (mainMenu["Flee"]["Bring Ally"].GetValue<MenuBool>().Value)
                {
                    
                try{
                    var swallowAlly =
                    GameObjects.AllyHeroes.FirstOrDefault(
                        x => x.HealthPercent < mainMenu["Shield"]["Devour Ally at Percent HP"].GetValue<MenuSlider>().Value
                            && Player.Distance(x) <= 500
                            && !x.IsDead);
                    if (swallowAlly != null)
                        W.Cast(swallowAlly);

                }catch { }
                }
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            }

        }
        static void moveToPosition()
        {
            Vector3 point = new Vector3();
            switch (mainMenu["Misc"]["Default W Move Location"].GetValue<MenuList>().Index)
            {
                case 0:
                    point = Game.CursorPos;
                    break;
                case 1:
                    point = GameObjects.AllyHeroes.Where(x=> !x.IsDead).OrderBy(x => x.Distance(Player)).FirstOrDefault().Position;
                    break;
                case 2:
                    point = GameObjects.AllyTurrets.Where(x => !x.IsDead).OrderBy(x => x.Distance(Player)).FirstOrDefault().Position;
                    break;
            }
            Orbwalker.MoveOrder(point);
        }

        static  void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            //Modified Kalista Soulbound code from Corey
            //Need to check in fountain otherwise recalls could make you swallow
            if (sender.Type == GameObjectType.obj_AI_Hero && sender.IsEnemy && !Player.InFountain())
            {
                try
                {

                var swallowAlly =
                    GameObjects.AllyHeroes.FirstOrDefault(
                        x => x.HealthPercent < mainMenu["Shield"]["Devour Ally at Percent HP"].GetValue<MenuSlider>().Value 
                            && Player.Distance(x) <= 500
                            && !x.IsDead);
                if (swallowAlly != null && !usedWEnemy && W.IsReady())
                {
                    W.CastOnUnit(swallowAlly);
                }

                }catch{}
                Obj_AI_Hero enemy = (Obj_AI_Hero) sender;
                try
                {

                    SpellDataInst s = enemy.Spellbook.Spells.FirstOrDefault(x => x.SData.Name.Equals(args.SData.Name));
                        if (enemy.GetSpellDamage(Player, s.Slot) > Player.Health && mainMenu["Shield"]["E Only Dangerous"].GetValue<MenuBool>().Value)
                            E.Cast();
                        else if ((enemy.GetSpellDamage(Player, s.Slot))/Player.MaxHealth <=
                                 mainMenu["Shield"]["E for Percent of HP Damage"].GetValue<MenuSlider>().Value)
                            E.Cast();
                }
                catch{}
            }
        }



        static void OnInterruptableSpell(object enemy, InterruptableSpell.InterruptableTargetEventArgs args)
        {
            try
            {
            Obj_AI_Hero sender = (Obj_AI_Hero) enemy;
            if (sender.IsAlly)
                return;
            float distance = sender.Distance(Player);
            if (sender.GetBuffCount(buffName) == 3)
            {
                if (distance <= W.Range && mainMenu["misc"]["Interrupt With W"].GetValue<MenuBool>().Value)
                    W.CastOnUnit(sender);
                else if (distance <= Q.Range && mainMenu["misc"]["Interrupt With Q"].GetValue<MenuBool>().Value)
                    Q.CastIfHitchanceMinimum(sender, HitChance.Medium);
            }
            else
            {
                if (distance <= 250)
                    Player.IssueOrder(GameObjectOrder.AttackUnit, sender);
                if (distance <= Q.Range)
                    Q.CastIfHitchanceMinimum(sender, HitChance.Medium);
            }


            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
