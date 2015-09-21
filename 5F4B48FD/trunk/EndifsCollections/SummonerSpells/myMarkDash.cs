using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCollections.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.SummonerSpells
{
    class myMarkDash
    {
        private static SpellSlot SnowballSlot;
        private static Spell Snowball;
        private static Menu tools;
        private static IEnumerable<Obj_AI_Hero> AllEnemies;
        private static Obj_AI_Hero target;
        public myMarkDash()
        {
            SnowballSlot = ObjectManager.Player.GetSpellSlot("summonersnowball");
            if (HaveSnowball)
            {
                Snowball = new Spell(SnowballSlot, 1600);
                Snowball.SetSkillshot(.50f, 100f, 1600, true, SkillshotType.SkillshotLine);
            }
            if (MapSupported)
            {
                AllEnemies = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy);
                Game.OnUpdate += OnUpdate;
                Drawing.OnDraw += OnDraw;
            }
        }
        public static void AddToMenu(Menu Tools)
        {
            if (MapSupported)
            {
                tools = Tools;
                var subs = new Menu("Mark/Dash", "myMarkDash");
                {
                    subs.AddItem(new MenuItem("msb_active", "Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                    subs.AddItem(new MenuItem("msb_mbool", "Use Mark").SetValue(false));
                    subs.AddItem(new MenuItem("msb_dbool", "Use Dash").SetValue(false));
                    subs.AddItem(new MenuItem("msb_draw", "Draw Range").SetValue(false));
                }
                Tools.AddSubMenu(subs);
            }
        }
        private static bool IsReady
        {
            get
            {
                return HaveSnowball && ObjectManager.Player.Spellbook.CanUseSpell(SnowballSlot) == SpellState.Ready;
            }
        }
        public static bool HaveSnowball
        {
            get
            {
                return SnowballSlot == SpellSlot.Summoner1 || SnowballSlot == SpellSlot.Summoner2;
            }
        }
        private static string State
        {
            get
            {
                return Snowball.Instance.Name.Equals("snowballfollowupcast") ? "Dash" : "Mark";
            }
        }
        private static bool MapSupported
        {
            get { return Game.MapId == GameMapId.HowlingAbyss; }
        }

        void OnUpdate(EventArgs args)
        {
            if (HaveSnowball && MapSupported)
            {
                target =
                    TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget(Snowball.Range)
                    ? TargetSelector.GetSelectedTarget()
                    : AllEnemies.Where(x => x.IsValid<Obj_AI_Hero>() && x.IsVisible && Vector3.Distance(ObjectManager.Player.ServerPosition, x.ServerPosition) <= Snowball.Range)
                    .OrderByDescending(x => myRePriority.ResortDB(x.ChampionName))
                    .ThenBy(i => i.Health)
                    .FirstOrDefault();
                if (tools.Item("msb_active").GetValue<KeyBind>().Active && IsReady)
                {
                    switch (State)
                    {
                        case "Dash":
                            if (tools.Item("msb_dbool").GetValue<bool>())
                            {
                                Snowball.Cast();
                            }
                            break;
                        case "Mark":
                            if (tools.Item("msb_mbool").GetValue<bool>())
                            {
                                Snowball.Cast(target);
                            }
                            break;
                    }
                }
            }
        }
        void OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead) return;
            if (tools.Item("msb_draw").GetValue<bool>() && IsReady)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 1600, Color.Yellow, 7);                
            }
        }
    }
}
