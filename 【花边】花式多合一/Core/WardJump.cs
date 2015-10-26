using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace 花边_花式多合一.Core
{
    enum ChamptionName
    {
        LeeSin = 0,
        Jax = 1,
        Katarina = 2
    }

    class WardJump
    {
        public static ChamptionName name;
        public static Spell Q_Jax, W_LeeSin, E_Katarina;
        public static Vector3 wardPosition;
        public static bool jumped;

        internal static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                if (!InitializeMenu.Menu.Item("WardJumpEnable").GetValue<bool>()) return;

                if (Huabian.Player.ChampionName == "Jax")
                {
                    Q_Jax = new Spell(SpellSlot.Q, 700);
                    name = ChamptionName.Jax;
                }
                else if (Huabian.Player.ChampionName == "LeeSin")
                {
                    W_LeeSin = new Spell(SpellSlot.W, 700);
                    name = ChamptionName.LeeSin;
                }
                else if (Huabian.Player.ChampionName == "Katarina")
                {
                    E_Katarina = new Spell(SpellSlot.E, 700);
                    name = ChamptionName.Katarina;
                }
                else
                {

                }

                Game.OnUpdate += Game_OnGameUpdate;
                Drawing.OnDraw += Drawing_OnDraw;

            }
            catch (Exception ex)
            {
                Console.WriteLine("WardJump error occurred: '{0}'", ex);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (InitializeMenu.Menu.Item("DrawingsWardJump").IsActive())
            {
                if (ChamptionName.Jax == name)
                {
                    Drawing.DrawCircle(Huabian.Player.Position, 700, System.Drawing.Color.Green);
                }
                else if (ChamptionName.LeeSin == name)
                {
                    Drawing.DrawCircle(Huabian.Player.Position, 700, System.Drawing.Color.Green);
                }
                else if (ChamptionName.Katarina == name)
                {
                    Drawing.DrawCircle(Huabian.Player.Position, 700, System.Drawing.Color.Green);
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (InitializeMenu.Menu.Item("wardjumpKey").GetValue<KeyBind>().Active)
            {
                if (ChamptionName.Jax == name)
                {
                    WardJumpJax();
                }
                else if (ChamptionName.LeeSin == name)
                {
                    WardJumpLeeSin();
                }
                else if (ChamptionName.Katarina == name)
                {
                    WardJumpKatarina();
                }
            }
        }

        private static void WardJumpKatarina()
        {
            Huabian.Player.IssueOrder(GameObjectOrder.MoveTo, Huabian.Player.Position.Extend(Game.CursorPos, 150));
            if (E_Katarina.IsReady())
            {
                wardPosition = Game.CursorPos;
                Obj_AI_Minion Wards;
                if (Game.CursorPos.Distance(Huabian.Player.Position) <= 700)
                {
                    Wards = ObjectManager.Get<Obj_AI_Minion>().Where(ward => ward.Distance(Game.CursorPos) < 150 && !ward.IsDead).FirstOrDefault();
                }
                else
                {
                    Vector3 cursorPos = Game.CursorPos;
                    Vector3 myPos = Huabian.Player.ServerPosition;
                    Vector3 delta = cursorPos - myPos;
                    delta.Normalize();
                    wardPosition = myPos + delta * (600 - 5);
                    Wards = ObjectManager.Get<Obj_AI_Minion>().Where(ward => ward.Distance(wardPosition) < 150 && !ward.IsDead).FirstOrDefault();
                }
                if (Wards == null)
                {
                    if (!wardPosition.IsWall())
                    {
                        InventorySlot invSlot = Items.GetWardSlot();
                        Items.UseItem((int)invSlot.Id, wardPosition);
                        jumped = true;
                    }
                }

                else if (E_Katarina.CastOnUnit(Wards))
                {
                    jumped = false;
                }
            }

        }

        private static void WardJumpLeeSin()
        {
            Huabian.Player.IssueOrder(GameObjectOrder.MoveTo, Huabian.Player.Position.Extend(Game.CursorPos, 150));

            if (W_LeeSin.IsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "BlindMonkWOne")
            {
                wardPosition = Game.CursorPos;
                Obj_AI_Minion Wards;
                if (Game.CursorPos.Distance(Huabian.Player.Position) <= 700)
                {
                    Wards = ObjectManager.Get<Obj_AI_Minion>().Where(ward => ward.Distance(Game.CursorPos) < 150 && !ward.IsDead).FirstOrDefault();
                }
                else
                {
                    Vector3 cursorPos = Game.CursorPos;
                    Vector3 myPos = Huabian.Player.ServerPosition;
                    Vector3 delta = cursorPos - myPos;
                    delta.Normalize();
                    wardPosition = myPos + delta * (600 - 5);
                    Wards = ObjectManager.Get<Obj_AI_Minion>().Where(ward => ward.Distance(wardPosition) < 150 && !ward.IsDead).FirstOrDefault();
                }
                if (Wards == null)
                {
                    if (!wardPosition.IsWall())
                    {
                        InventorySlot invSlot = Items.GetWardSlot();
                        Items.UseItem((int)invSlot.Id, wardPosition);
                        jumped = true;
                    }
                }

                else
                    if (W_LeeSin.CastOnUnit(Wards))
                {
                    jumped = false;
                }
            }
        }

        private static void WardJumpJax()
        {
            Huabian.Player.IssueOrder(GameObjectOrder.MoveTo, Huabian.Player.Position.Extend(Game.CursorPos, 150));

            if (Q_Jax.IsReady())
            {
                wardPosition = Game.CursorPos;
                Obj_AI_Minion Wards;
                if (Game.CursorPos.Distance(Huabian.Player.Position) <= 700)
                {
                    Wards = ObjectManager.Get<Obj_AI_Minion>().Where(ward => ward.Distance(Game.CursorPos) < 150 && !ward.IsDead).FirstOrDefault();
                }
                else
                {
                    Vector3 cursorPos = Game.CursorPos;
                    Vector3 myPos = Huabian.Player.ServerPosition;
                    Vector3 delta = cursorPos - myPos;
                    delta.Normalize();
                    wardPosition = myPos + delta * (600 - 5);
                    Wards = ObjectManager.Get<Obj_AI_Minion>().Where(ward => ward.Distance(wardPosition) < 150 && !ward.IsDead).FirstOrDefault();
                }
                if (Wards == null)
                {
                    if (!wardPosition.IsWall())
                    {
                        InventorySlot invSlot = Items.GetWardSlot();
                        Items.UseItem((int)invSlot.Id, wardPosition);
                        jumped = true;
                    }
                }

                else
                    if (Q_Jax.CastOnUnit(Wards))
                {
                    jumped = false;
                }
            }
        }
    }
}
