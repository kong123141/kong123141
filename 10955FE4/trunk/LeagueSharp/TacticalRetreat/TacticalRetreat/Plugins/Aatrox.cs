using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace TacticalRetreat.Plugins
{
    internal class Aatrox
    {
        private static Menu rootMenu = new Menu("TacticalRetreat", "OKTF", true);
        private static Vector3 escapePoint = new Vector3();
        //Thanks Blacky for these values!
        private static readonly Vector3 PurpleSpawn = new Vector3(14286f, 14382f, 172f);
        private static readonly Vector3 BlueSpawn = new Vector3(416f, 468f, 182f);
        private static Obj_AI_Hero Player = ObjectManager.Player;

        private enum EscapeType
        {
            Not,
            Enemy,
            Self,
			Dash
        }

        private static readonly Dictionary<Spell, EscapeType> spells = new Dictionary<Spell, EscapeType>
        {
            {new Spell(SpellSlot.Q, 650), EscapeType.Dash},
            {new Spell(SpellSlot.W, 0), EscapeType.Not},
            {new Spell(SpellSlot.E, 1000), EscapeType.Enemy},
            {new Spell(SpellSlot.R, 550), EscapeType.Not}
        };

        public static void Load()
        {
            LoadMenus();
            Game.OnUpdate += OnUpdate;
        }

        private static void LoadMenus()
        {
            rootMenu.AddSubMenu(new Menu("Aatrox", "Aatrox"));
            rootMenu.AddSubMenu(new Menu("Escape Position", "Escape Position"));
            rootMenu.SubMenu("Escape Position")
                .AddItem(
                    new MenuItem("Escape Position", "Escape Position").SetValue(
                        new StringList(new[] {"Nearest Ally", "Fountain", "Cursor"}, 1)));
            rootMenu.AddSubMenu(new Menu("Keys", "Keys"));
            rootMenu.SubMenu("Keys").AddItem(new MenuItem("Flee", "Flee").SetValue(new KeyBind('Z', KeyBindType.Press)));
        }

        private static void OnUpdate(EventArgs args)
        {
            if (!rootMenu.SubMenu("Keys").Item("Flee").IsActive())
                return;
            switch (rootMenu.SubMenu("Escape Position").Item("Escape Position").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    NearestAlly();
                    break;
                case 1:
                    escapePoint = (Player.Team == GameObjectTeam.Order) ? BlueSpawn : PurpleSpawn;
                    break;
                case 2:
                    escapePoint = Game.CursorPos;
                    break;
            }
            Player.IssueOrder(GameObjectOrder.MoveTo, escapePoint);
            EscapeSpells();
            Player.IssueOrder(GameObjectOrder.MoveTo, escapePoint);
        }

        private static void EscapeSpells()
        {
            foreach (KeyValuePair<Spell, EscapeType> spell in spells.Where(s => s.Value != EscapeType.Not))
            {
                if (spell.Key.IsReady())
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, escapePoint);
                    if (spell.Value == EscapeType.Dash)
                    {
                        Vector3 pos = Player.Position.Extend(Player.Direction, spell.Key.Range);
                        spell.Key.Cast(pos);
                    }
                    else if (spell.Value == EscapeType.Self)
                    {
                        spell.Key.Cast(Player.Position);
                    }
                    else
                    {

                        Obj_AI_Hero target = TargetSelector.GetTarget(spell.Key.Range, TargetSelector.DamageType.Magical);
                        if (spell.Key.IsReady() && target.IsValidTarget())
                            spell.Key.Cast(target);
                    }
                }
            }
        }

        #region Ally Finding

        private static void NearestAlly()
        {
            Obj_AI_Hero[] allies =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(ally => !ally.IsEnemy)
                    .Where(ally => !ally.IsDead)
                    .Where(ally => !ally.IsMe)
                    .ToArray();
            List<Vector3> allyPos = new List<Vector3>();
            foreach (Obj_AI_Hero ally in allies)
                allyPos.Add(ally.Position);
            escapePoint = NearestAlly(allyPos.ToArray());
        }

        private static Vector3 NearestAlly(Vector3[] allyPositions)
        {
            if (allyPositions.Count() == 0)
                return new Vector3();
            var minimum = float.MaxValue;
            var start = Player.Position;
            Vector3 tempMin = new Vector3();
            foreach (Vector3 ally in allyPositions)
            {
                var path = ObjectManager.Player.GetPath(start, ally);
                var lastPoint = start;
                var d = 0f;
                d = DistanceFromArray(path);
                if (d < minimum)
                {
                    minimum = d;
                    tempMin = ally;
                }
            }
            return tempMin;
        }

        public static float DistanceFromArray(Vector3[] array)
        {
            var start = array[0];
            float distance = 0;
            for (int i = 1; i < array.Length; i++)
            {
                distance += start.Distance(array[i]);
                start = array[i];
            }
            return distance;
        }

        #endregion
    }
}
