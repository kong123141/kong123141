﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace OneKeyToWin_AIO_Sebby.Core
{
    class HiddenObj
    {
        public int type;
        //0 - missile
        //1 - normal
        //2 - pink
        //3 - teemo trap
        public float endTime { get; set; }
        public Vector3 pos { get; set; }
    }

    class OKTWward
    {
        public Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private Menu Config = Program.Config;
        private bool rengar = false;
        Obj_AI_Hero Vayne = null;
        private static Spell Q, W, E, R;

        public static List<HiddenObj> HiddenObjList = new List<HiddenObj>();

        private Items.Item
            VisionWard = new Items.Item(2043, 550f),
            OracleLens = new Items.Item(3364, 550f),
            WardN = new Items.Item(2044, 600f),
            TrinketN = new Items.Item(3340, 600f),
            SightStone = new Items.Item(2049, 600f),
            FarsightOrb = new Items.Item(3342, 4000f),
            ScryingOrb = new Items.Item(3363, 3500f);

        public void LoadOKTW()
        {
            Q = new Spell(SpellSlot.Q);
            E = new Spell(SpellSlot.E);
            W = new Spell(SpellSlot.W);
            R = new Spell(SpellSlot.R);

            Config.SubMenu("AutoWard OKTW©").AddItem(new MenuItem("AutoWard", "Auto Ward").SetValue(true));
            Config.SubMenu("AutoWard OKTW©").AddItem(new MenuItem("autoBuy", "Auto buy blue trinket after lvl 6").SetValue(true));
            Config.SubMenu("AutoWard OKTW©").AddItem(new MenuItem("AutoWardBlue", "Auto Blue Trinket").SetValue(true));
            Config.SubMenu("AutoWard OKTW©").AddItem(new MenuItem("AutoWardCombo", "Only combo mode").SetValue(true));
            Config.SubMenu("AutoWard OKTW©").AddItem(new MenuItem("AutoWardPink", "Auto VisionWard, OracleLens").SetValue(true));

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    if (hero.ChampionName == "Rengar")
                        rengar = true;
                    if (hero.ChampionName == "Vayne")
                        Vayne = hero;
                }
            }
            
            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            GameObject.OnCreate +=GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;

        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!Program.LagFree(0) || Player.IsRecalling() || Player.IsDead)
                return;

            foreach (var sender in ObjectManager.Get<Obj_AI_Base>().Where(Obj => Obj.IsEnemy && Obj.MaxHealth<6 && (Obj.Name == "SightWard" || Obj.Name == "VisionWard") ))
            {
                if (!HiddenObjList.Exists(x => x.pos.Distance(sender.Position) < 100) && (sender.Name.ToLower() == "visionward" || sender.Name.ToLower() == "sightward"))
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
                    if (sender.MaxHealth == 3)
                        AddWard("sightward", sender.Position);
                    if (sender.MaxHealth == 5)
                        AddWard("visionward", sender.Position);
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

            if (Config.Item("autoBuy").GetValue<bool>() && Player.InFountain() && !ScryingOrb.IsOwned() && Player.Level > 5)
                ObjectManager.Player.BuyItem(ItemId.Scrying_Orb_Trinket);

            if(rengar && Player.HasBuff("rengarralertsound"))
                CastVisionWards(Player.ServerPosition);
            
            if (Vayne != null && Vayne.IsValidTarget(1000) && Vayne.HasBuff("vaynetumblefade"))
                CastVisionWards(Vayne.ServerPosition);

            AutoWardLogic();
        }

        private void AutoWardLogic()
        {
            foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValid && !enemy.IsVisible && !enemy.IsDead))
            {
                var need = OKTWtracker.ChampionInfoList.Find(x => x.NetworkId == enemy.NetworkId);

                if (need == null || need.PredictedPos == null)
                    return;

                if (Player.ChampionName == "Quinn" && W.IsReady() && Game.Time - need.LastVisableTime > 0.5 && Game.Time - need.LastVisableTime < 4 && need.PredictedPos.Distance(Player.Position) < 1500 && Config.Item("autoW").GetValue<bool>())
                {
                    W.Cast();
                    return;
                }

                if (Player.ChampionName == "Ashe" && E.IsReady() && Player.Spellbook.GetSpell(SpellSlot.E).Ammo > 1 && Player.CountEnemiesInRange(800) == 0 && Game.Time - need.LastVisableTime > 3 && Game.Time - need.LastVisableTime < 1 && Config.Item("autoE").GetValue<bool>())
                {
                    if (need.PredictedPos.Distance(Player.Position) < 3000)
                    {
                        E.Cast(ObjectManager.Player.Position.Extend(need.PredictedPos, 5000));
                        return;
                    }
                }

                if (Player.ChampionName == "MissFortune" && E.IsReady() && Game.Time - need.LastVisableTime > 0.5 && Game.Time - need.LastVisableTime < 2 && Program.Combo && Player.Mana > 200f)
                {
                    if (need.PredictedPos.Distance(Player.Position) < 800)
                    {
                        E.Cast(ObjectManager.Player.Position.Extend(need.PredictedPos, 800));
                        return;
                    }
                }

                if (Player.ChampionName == "Caitlyn" && W.IsReady() && Game.Time - need.LastVisableTime < 2 && Player.Mana > 200f && !Player.IsWindingUp && Config.Item("bushW").GetValue<bool>())
                {
                    if (need.PredictedPos.Distance(Player.Position) < 800)
                    {
                        W.Cast(need.PredictedPos);
                        return;
                    }
                }

                if (Game.Time - need.LastVisableTime < 4)
                {
                    if (Config.Item("AutoWardCombo").GetValue<bool>() && !Config.Item("onlyUtility", true).GetValue<bool>() && !Program.Combo)
                        return;

                    if (NavMesh.IsWallOfGrass(need.PredictedPos, 0))
                    {
                        if (need.PredictedPos.Distance(Player.Position) < 600 && Config.Item("AutoWard").GetValue<bool>())
                        {
                            if (TrinketN.IsReady())
                            {
                                TrinketN.Cast(need.PredictedPos);
                                need.LastVisableTime = Game.Time - 5;
                            }
                            else if (SightStone.IsReady())
                            {
                                SightStone.Cast(need.PredictedPos);
                                need.LastVisableTime = Game.Time - 5;
                            }
                            else if (WardN.IsReady())
                            {
                                WardN.Cast(need.PredictedPos);
                                need.LastVisableTime = Game.Time - 5;
                            }
                        }

                        if (need.PredictedPos.Distance(Player.Position) < 1400 && Config.Item("AutoWardBlue").GetValue<bool>())
                        {
                            if (FarsightOrb.IsReady())
                            {
                                FarsightOrb.Cast(need.PredictedPos);
                                need.LastVisableTime = Game.Time - 5;
                            }
                            else if (ScryingOrb.IsReady())
                            {
                                ScryingOrb.Cast(need.PredictedPos);
                                need.LastVisableTime = Game.Time - 5;
                            }
                        }
                    }
                }
            } 
        }

        private void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.IsEnemy || sender.IsAlly )
                return;

            if (sender.Type == GameObjectType.MissileClient && (sender is MissileClient))
            {
                var missile = (MissileClient)sender;
                
                if ( !missile.SpellCaster.IsVisible)
                {
                    if (missile.SData.Name == "itemplacementmissile" && !HiddenObjList.Exists(x => missile.StartPosition.Extend(missile.EndPosition, 500).Distance(x.pos) < 500))
                        AddWard(missile.SData.Name.ToLower(), missile.StartPosition.Extend(missile.EndPosition, 500));

                    if ((missile.SData.Name == "BantamTrapShort" || missile.SData.Name == "BantamTrapBounceSpell") && !HiddenObjList.Exists(x => missile.EndPosition == x.pos))
                        AddWard("teemorcast", missile.EndPosition);
                }
            }

            if (sender.Type == GameObjectType.obj_AI_Minion && (sender.Name.ToLower() == "visionward" || sender.Name.ToLower() == "sightward") && !HiddenObjList.Exists(x => x.pos.Distance(sender.Position) < 100) )
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
                AddWard(sender.Name.ToLower(), sender.Position);
            }

            if (rengar &&  sender.Position.Distance(Player.Position) < 800)
            {
                switch (sender.Name)
                {
                    case "Rengar_LeapSound.troy":
                        CastVisionWards(sender.Position);
                        break;
                    case "Rengar_Base_R_Alert":
                        CastVisionWards(sender.Position);
                        break;
                }
            }
        }

        private void GameObject_OnDelete(GameObject sender, EventArgs args)
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
       
        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {

            if (sender.IsEnemy && !sender.IsMinion && sender is Obj_AI_Hero )
            {
                AddWard(args.SData.Name.ToLower(), args.End);

                if (sender.Distance(Player.Position) < 800)
                {
                    switch (args.SData.Name)
                    {
                        case "akalismokebomb":
                            CastVisionWards(sender.ServerPosition);
                            break;
                        case "deceive":
                            CastVisionWards(sender.ServerPosition);
                            break;
                        case "khazixr":
                            CastVisionWards(sender.ServerPosition);
                            break;
                        case "khazixrlong":
                            CastVisionWards(sender.ServerPosition);
                            break;
                        case "talonshadowassault":
                            CastVisionWards(sender.ServerPosition);
                            break;
                        case "monkeykingdecoy":
                            CastVisionWards(sender.ServerPosition);
                            break;
                        case "RengarR":
                            CastVisionWards(sender.ServerPosition);
                            break;
                        case "TwitchHideInShadows":
                            CastVisionWards(sender.ServerPosition);
                            break;
                    }
                }
            }
        }

        private void AddWard(string name, Vector3 posCast)
        {
            switch (name)
            {
                //OnCreateOBJ
                case "itemplacementmissile":
                    HiddenObjList.Add(new HiddenObj() { type = 0, pos = posCast, endTime = Game.Time + 180 });
                    break;
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

        private void CastVisionWards(Vector3 position)
        {
            if (Config.Item("AutoWardPink").GetValue<bool>())
            {
                if (OracleLens.IsReady())
                    OracleLens.Cast(Player.Position.Extend(position, OracleLens.Range));
                else if (VisionWard.IsReady())
                    VisionWard.Cast(Player.Position.Extend(position, VisionWard.Range));
            }
        }
    }
}
