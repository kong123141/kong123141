namespace Flowers_Utility.Common.Evade
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SharpDX;

    internal static class SkillshotDetector
    {
        public delegate void OnDeleteMissileH(Skillshot skillshot, MissileClient missile);
        public delegate void OnDetectSkillshotH(Skillshot skillshot);

        static SkillshotDetector()
        {
            Obj_AI_Base.OnProcessSpellCast += ObjAiHeroOnOnProcessSpellCast;
            GameObject.OnDelete += ObjSpellMissileOnOnDelete;
            GameObject.OnCreate += ObjSpellMissileOnOnCreate;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;

            if (Config.TestOnAllies && ObjectManager.Get<Obj_AI_Hero>().Count() == 1)
            {
                Game.OnWndProc += Game_OnWndProc;
            }
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == (uint)WindowsMessages.WM_KEYUP)
            {
                TriggerOnDetectSkillshot(DetectionType.ProcessSpell, SpellDatabase.GetByName("TestSkillShot"), Utils.TickCount, Pluging.Evade.PlayerPosition, Game.CursorPos.To2D(), ObjectManager.Player);
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var spellData = SpellDatabase.GetBySourceObjectName(sender.Name);

            if (spellData == null)
            {
                return;
            }

            if (Pluging.Evade.Menu.Item("Enabled" + spellData.MenuItemName) == null)
            {
                return;
            }

            TriggerOnDetectSkillshot(DetectionType.ProcessSpell, spellData, Utils.TickCount - Game.Ping / 2, sender.Position.To2D(), sender.Position.To2D(), HeroManager.AllHeroes.MinOrDefault(h => h.IsAlly ? 1 : 0));
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid || !Config.TestOnAllies && sender.Team == ObjectManager.Player.Team)
            {
                return;
            }

            for (var i = Pluging.Evade.DetectedSkillshots.Count - 1; i >= 0; i--)
            {
                var skillshot = Pluging.Evade.DetectedSkillshots[i];

                if (skillshot.SpellData.ToggleParticleName != "" && new Regex(skillshot.SpellData.ToggleParticleName).IsMatch(sender.Name))
                {
                    Pluging.Evade.DetectedSkillshots.RemoveAt(i);
                }
            }
        }

        private static void ObjSpellMissileOnOnCreate(GameObject sender, EventArgs args)
        {
            var missile = sender as MissileClient;

            if (missile == null || !missile.IsValid)
            {
                return;
            }

            var unit = missile.SpellCaster as Obj_AI_Hero;

            if (unit == null || !unit.IsValid || (unit.Team == ObjectManager.Player.Team && !Config.TestOnAllies))
            {
                return;
            }

            var spellData = SpellDatabase.GetByMissileName(missile.SData.Name);

            if (spellData == null)
            {
                return;
            }

            Utility.DelayAction.Add(0, delegate
            {
                ObjSpellMissionOnOnCreateDelayed(sender, args);
            });
        }

        private static void ObjSpellMissionOnOnCreateDelayed(GameObject sender, EventArgs args)
        {
            var missile = sender as MissileClient;

            if (missile == null || !missile.IsValid)
            {
                return;
            }

            var unit = missile.SpellCaster as Obj_AI_Hero;

            if (unit == null || !unit.IsValid || (unit.Team == ObjectManager.Player.Team && !Config.TestOnAllies))
            {
                return;
            }

            var spellData = SpellDatabase.GetByMissileName(missile.SData.Name);

            if (spellData == null)
            {
                return;
            }

            var missilePosition = missile.Position.To2D();
            var unitPosition = missile.StartPosition.To2D();
            var endPos = missile.EndPosition.To2D();
            var direction = (endPos - unitPosition).Normalized();
            if (unitPosition.Distance(endPos) > spellData.Range || spellData.FixedRange)
            {
                endPos = unitPosition + direction * spellData.Range;
            }

            if (spellData.ExtraRange != -1)
            {
                endPos = endPos + Math.Min(spellData.ExtraRange, spellData.Range - endPos.Distance(unitPosition)) * direction;
            }

            var castTime = Utils.TickCount - Game.Ping / 2 - (spellData.MissileDelayed ? 0 : spellData.Delay) - (int)(1000f * missilePosition.Distance(unitPosition) / spellData.MissileSpeed);

            TriggerOnDetectSkillshot(DetectionType.RecvPacket, spellData, castTime, unitPosition, endPos, unit);
        }

        private static void ObjSpellMissileOnOnDelete(GameObject sender, EventArgs args)
        {
            var missile = sender as MissileClient;

            if (missile == null || !missile.IsValid)
            {
                return;
            }

            var caster = missile.SpellCaster as Obj_AI_Hero;

            if (caster == null || !caster.IsValid || (caster.Team == ObjectManager.Player.Team && !Config.TestOnAllies))
            {
                return;
            }

            var spellName = missile.SData.Name;

            if (OnDeleteMissile != null)
            {
                foreach (var skillshot in Pluging.Evade.DetectedSkillshots)
                {
                    if (skillshot.SpellData.MissileSpellName == spellName && (skillshot.Unit.NetworkId == caster.NetworkId && (missile.EndPosition.To2D() - missile.StartPosition.To2D()).AngleBetween(skillshot.Direction) < 10) && skillshot.SpellData.CanBeRemoved)
                    {
                        OnDeleteMissile(skillshot, missile);

                        break;
                    }
                }
            }

            Pluging.Evade.DetectedSkillshots.RemoveAll(skillshot => (skillshot.SpellData.MissileSpellName == spellName || skillshot.SpellData.ExtraMissileNames.Contains(spellName)) && (skillshot.Unit.NetworkId == caster.NetworkId && ((missile.EndPosition.To2D() - missile.StartPosition.To2D()).AngleBetween(skillshot.Direction) < 10) && skillshot.SpellData.CanBeRemoved || skillshot.SpellData.ForceRemove)); // 
        }

        public static event OnDetectSkillshotH OnDetectSkillshot;
        public static event OnDeleteMissileH OnDeleteMissile;

        private static void TriggerOnDetectSkillshot(DetectionType detectionType, SpellData spellData, int startT, Vector2 start, Vector2 end, Obj_AI_Base unit)
        {
            var skillshot = new Skillshot(detectionType, spellData, startT, start, end, unit);

            if (OnDetectSkillshot != null)
            {
                OnDetectSkillshot(skillshot);
            }
        }

        private static void ObjAiHeroOnOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender == null || !sender.IsValid)
            {
                return;
            }

            if (Config.PrintSpellData && sender is Obj_AI_Hero)
            {
                Game.PrintChat(Utils.TickCount + " ProcessSpellCast: " + args.SData.Name);
                Console.WriteLine(Utils.TickCount + " ProcessSpellCast: " + args.SData.Name);
            }

            if (args.SData.Name == "dravenrdoublecast")
            {
                Pluging.Evade.DetectedSkillshots.RemoveAll(s => s.Unit.NetworkId == sender.NetworkId && s.SpellData.SpellName == "DravenRCast");
            }

            if (!sender.IsValid || sender.Team == ObjectManager.Player.Team && !Config.TestOnAllies)
            {
                return;
            }

            var spellData = SpellDatabase.GetByName(args.SData.Name);

            if (spellData == null)
            {
                return;
            }

            var startPos = new Vector2();

            if (spellData.FromObject != "")
            {
                foreach (var o in ObjectManager.Get<GameObject>())
                {
                    if (o.Name.Contains(spellData.FromObject))
                    {
                        startPos = o.Position.To2D();
                    }
                }
            }
            else
            {
                startPos = sender.ServerPosition.To2D();
            }

            if (spellData.FromObjects != null && spellData.FromObjects.Length > 0)
            {
                foreach (var obj in ObjectManager.Get<GameObject>())
                {
                    if (obj.IsEnemy && spellData.FromObjects.Contains(obj.Name))
                    {
                        var start = obj.Position.To2D();
                        var end = start + spellData.Range * (args.End.To2D() - obj.Position.To2D()).Normalized();

                        TriggerOnDetectSkillshot(DetectionType.ProcessSpell, spellData, Utils.TickCount - Game.Ping / 2, start, end, sender);
                    }
                }
            }

            if (!startPos.IsValid())
            {
                return;
            }

            var endPos = args.End.To2D();

            if (spellData.SpellName == "LucianQ" && args.Target != null && args.Target.NetworkId == ObjectManager.Player.NetworkId)
            {
                return;
            }

            var direction = (endPos - startPos).Normalized();

            if (startPos.Distance(endPos) > spellData.Range || spellData.FixedRange)
            {
                endPos = startPos + direction * spellData.Range;
            }

            if (spellData.ExtraRange != -1)
            {
                endPos = endPos + Math.Min(spellData.ExtraRange, spellData.Range - endPos.Distance(startPos)) * direction;
            }

            TriggerOnDetectSkillshot(DetectionType.ProcessSpell, spellData, Utils.TickCount - Game.Ping / 2, startPos, endPos, sender);
        }

        public static void GameOnOnGameProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == 0x3B)
            {
                var packet = new GamePacket(args.PacketData);

                packet.Position = 1;

                packet.ReadFloat();

                var missilePosition = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
                var unitPosition = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());

                packet.Position = packet.Size() - 119;

                var missileSpeed = packet.ReadFloat();

                packet.Position = 65;

                var endPos = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());

                packet.Position = 112;

                var id = packet.ReadByte();


                packet.Position = packet.Size() - 83;

                var unit = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(packet.ReadInteger());

                if ((!unit.IsValid || unit.Team == ObjectManager.Player.Team) && !Config.TestOnAllies)
                {
                    return;
                }

                var spellData = SpellDatabase.GetBySpeed(unit.ChampionName, (int)missileSpeed, id);

                if (spellData == null)
                {
                    return;
                }

                if (spellData.SpellName != "Laser")
                {
                    return;
                }

                var castTime = Utils.TickCount - Game.Ping / 2 - spellData.Delay - (int)(1000 * missilePosition.SwitchYZ().To2D().Distance(unitPosition.SwitchYZ()) / spellData.MissileSpeed);

                TriggerOnDetectSkillshot(DetectionType.RecvPacket, spellData, castTime, unitPosition.SwitchYZ().To2D(), endPos.SwitchYZ().To2D(), unit);
            }
        }
    }
}
