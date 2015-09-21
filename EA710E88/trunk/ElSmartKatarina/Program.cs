using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;
using Color = System.Drawing.Color;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace ElKatarina
{
    internal enum Spells
    {
        Q,
        W,
        E,
        R
    }

    internal static class Program
    {
        private static Orbwalking.Orbwalker _orbwalker;
        private static SpellSlot _igniteSlot;
        private static Menu _config;
        private static Obj_AI_Hero _player = ObjectManager.Player;
        private static long _lastECast;
        private static int _lastPlaced;
        private static int _lastNotification = 0;
        private static Vector3 _lastWardPos;
        private static bool _isChanneling;
        private static float _rStart = 0;


        private static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            { Spells.Q, new Spell(SpellSlot.Q, 675)},
            { Spells.W, new Spell(SpellSlot.W, 375)},
            { Spells.E, new Spell(SpellSlot.E, 700)},
            { Spells.R, new Spell(SpellSlot.R, 550)}
        };

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        #region OnLoad
        private static void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Katarina")
                return;
            
            _igniteSlot = _player.GetSpellSlot("summonerdot");

            spells[Spells.R].SetCharged("KatarinaR", "KatarinaR", 550, 550, 1.0f);

            Drawing.OnDraw += Drawings;
            MenuLoad();
            Game.OnUpdate +=OnUpdate;
            Obj_AI_Hero.OnIssueOrder += Obj_AI_Hero_OnIssueOrder;
            GameObject.OnCreate += GameObject_OnCreate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Orbwalking.BeforeAttack += BeforeAttack;
            Notifications.AddNotification("SmartKatarina by Jouza - jQuery", 5000);
        }
        #endregion

        #region HasRBuff
        private static bool HasRBuff()
        {
            return _player.HasBuff("KatarinaR") || _player.IsChannelingImportantSpell() || _player.HasBuff("katarinarsound", true);
        }
        #endregion

        #region BeforeAttack
        private static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe)
            {
                args.Process = !_player.HasBuff("KatarinaR");
            }
        }
        #endregion

        #region OnUpdate
        private static void OnUpdate(EventArgs args)
        {
            if (HasRBuff())
            {
                _orbwalker.SetAttack(false);
                _orbwalker.SetMovement(false);
            }
            else
            {
                _orbwalker.SetAttack(true);
                _orbwalker.SetMovement(true);
            }

            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Laneclear();
                    JungleClear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Farm();
                    break;
            }

            KillSteal();

            /*var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                return;

            if (target)*/

            var wardjump = _config.Item("wardjumpkey").GetValue<KeyBind>().Active;
            if (wardjump)
                DoWardJump();

            var showNotifications = _config.Item("ElKatarina.misc.Notifications").GetValue<bool>();

            if (spells[Spells.R].IsReady() && showNotifications && Environment.TickCount - _lastNotification > 5000)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget() && GetComboDamage(h) > h.Health))
                {
                    ShowNotification(enemy.ChampionName + ": is killable", Color.White, 4000);
                    _lastNotification = Environment.TickCount;
                }
            }

            var autoHarass = _config.Item("ElKatarina.AutoHarass.Activated", true).GetValue<KeyBind>().Active;
            if (autoHarass)
                OnAutoHarass();
        }
        #endregion

        #region AutoHarass
        private static void OnAutoHarass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValid || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                return;

            var useQ = _config.Item("ElKatarina.AutoHarass.Q").GetValue<bool>();
            var useW = _config.Item("ElKatarina.AutoHarass.W").GetValue<bool>();

            if (spells[Spells.Q].IsReady() && target.IsValidTarget() && useQ)
            {
                spells[Spells.Q].Cast(target);
            };

            if (spells[Spells.W].IsReady() && target.IsValidTarget(spells[Spells.W].Range) && useW)
            {
                spells[Spells.W].Cast();
            }
        }
        #endregion

        #region GameObject_OnCreate
        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!spells[Spells.E].IsReady() || !(sender is Obj_AI_Minion) || Environment.TickCount >= _lastPlaced + 300)
                return;

            if (Environment.TickCount >= _lastPlaced + 300) return;
            var ward = (Obj_AI_Minion)sender;

            if (ward.Name.ToLower().Contains("ward") && ward.Distance(_lastWardPos) < 500)
            {
                spells[Spells.E].Cast(ward);
            }
        }
        #endregion

        #region UseItems

        private static void UseItems(Obj_AI_Base target)
        {
            var useHextech = _config.Item("ElKatarina.Items.hextech").GetValue<bool>();
            if (useHextech)
            {
                var cutlass = ItemData.Bilgewater_Cutlass.GetItem();
                var hextech = ItemData.Hextech_Gunblade.GetItem();

                if (cutlass.IsReady() && cutlass.IsOwned(_player) && cutlass.IsInRange(target))
                    cutlass.Cast(target);

                if (hextech.IsReady() && hextech.IsOwned(_player) && hextech.IsInRange(target))
                    hextech.Cast(target);
            }
        }

        #endregion

        #region WardSlot
        private static InventorySlot GetBestWardSlot()
        {
            InventorySlot slot = Items.GetWardSlot();
            if (slot == default(InventorySlot)) return null;
            return slot;
        }
        #endregion

        #region Wardjump
        private static void DoWardJump()
        {
            if (Environment.TickCount <= _lastPlaced + 3000 || !spells[Spells.E].IsReady())
                return;

            Vector3 cursorPos = Game.CursorPos;
            Vector3 myPos = _player.ServerPosition;
            Vector3 delta = cursorPos - myPos;

            delta.Normalize();

            Vector3 wardPosition = myPos + delta * (600 - 5);
            InventorySlot invSlot = GetBestWardSlot();

            if (invSlot == null)
                return;

            Items.UseItem((int)invSlot.Id, wardPosition);
            _lastWardPos = wardPosition;
            _lastPlaced = Environment.TickCount;

            spells[Spells.E].Cast();
        }

        #endregion

        #region Humanizer
        private static void CastE(Obj_AI_Base unit)
        {
            var playLegit = _config.Item("playLegit").GetValue<bool>();
            var legitCastDelay = _config.Item("legitCastDelay").GetValue<Slider>().Value;

            if (playLegit)
            {
                if (Environment.TickCount > _lastECast + legitCastDelay)
                {
                    spells[Spells.E].CastOnUnit(unit);
                    _lastECast = Environment.TickCount;
                }
            }
            else
            {
                spells[Spells.E].CastOnUnit(unit);
                _lastECast = Environment.TickCount;
            }
        }
        #endregion

        #region Killsteal
        private static void KillSteal()
        {
            //&& !HasRBuff()
            if (_config.Item("KillSteal").GetValue<bool>())
            {
                foreach (
                    Obj_AI_Hero hero in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                hero =>
                                    ObjectManager.Player.Distance(hero.ServerPosition) <= spells[Spells.E].Range && !hero.IsMe &&
                                    hero.IsValidTarget() && hero.IsEnemy && !hero.IsInvulnerable))
                {
                    var qdmg = spells[Spells.Q].GetDamage(hero);
                    var wdmg = spells[Spells.W].GetDamage(hero);
                    var edmg = spells[Spells.E].GetDamage(hero);
                    var markDmg = _player.CalcDamage(hero, Damage.DamageType.Magical, _player.FlatMagicDamageMod * 0.15 + _player.Level * 15);
                    float ignitedmg;

                    //Ignite Damage
                    if (_igniteSlot != SpellSlot.Unknown)
                    {
                        ignitedmg = (float) _player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
                    }
                    else
                    {
                        ignitedmg = 0f;
                    }

                    if (hero.HasBuff("katarinaqmark") && hero.Health - wdmg - markDmg < 0 && spells[Spells.W].IsReady() &&
                        spells[Spells.W].IsInRange(hero))
                    {
                        spells[Spells.W].Cast();
                    }

                    if (hero.Health - ignitedmg < 0 && _igniteSlot.IsReady())
                    {
                        _player.Spellbook.CastSpell(_igniteSlot, hero);
                    }
                    

                    if (hero.Health - edmg < 0 && spells[Spells.E].IsReady())
                    {
                        spells[Spells.E].Cast(hero);
                    }
                 
                    if (hero.Health - qdmg < 0 && spells[Spells.Q].IsReady() && spells[Spells.Q].IsInRange(hero))
                    {
                        spells[Spells.Q].Cast(hero);
                    }

                    if (hero.Health - edmg - wdmg < 0 && spells[Spells.E].IsReady() && spells[Spells.W].IsReady())
                    {
                        CastE(hero);
                        spells[Spells.W].Cast();
                    }

                    if (hero.Health - edmg - qdmg < 0 && spells[Spells.E].IsReady() && spells[Spells.Q].IsReady())
                    {
                        CastE(hero);
                        spells[Spells.Q].Cast(hero);
                    }

                    if (hero.Health - edmg - wdmg - qdmg < 0 && spells[Spells.E].IsReady() && spells[Spells.Q].IsReady() && spells[Spells.W].IsReady())
                    {
                        CastE(hero);
                        spells[Spells.Q].Cast(hero);
                        spells[Spells.W].Cast();
                    }

                    if (hero.Health - edmg - wdmg - qdmg - markDmg < 0 && spells[Spells.E].IsReady() && spells[Spells.Q].IsReady() && spells[Spells.W].IsReady())
                    {
                        CastE(hero);
                        spells[Spells.Q].Cast(hero);
                        spells[Spells.W].Cast();
                    }

                    if (hero.Health - edmg - wdmg - qdmg - ignitedmg < 0 && spells[Spells.E].IsReady() && spells[Spells.Q].IsReady() && spells[Spells.W].IsReady() &&
                        _igniteSlot.IsReady())
                    {
                        CastE(hero);
                        spells[Spells.Q].Cast(hero);
                        spells[Spells.W].Cast();
                        _player.Spellbook.CastSpell(_igniteSlot, hero);
                    }
                }

                foreach (
                    Obj_AI_Base target in
                        ObjectManager.Get<Obj_AI_Base>()
                            .Where(
                                target =>
                                    ObjectManager.Player.Distance(target.ServerPosition) <= spells[Spells.E].Range && !target.IsMe &&
                                    target.IsTargetable && !target.IsInvulnerable))
                {
                    foreach (
                        Obj_AI_Hero focus in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(
                                    focus =>
                                        focus.Distance(focus.ServerPosition) <= spells[Spells.Q].Range && focus.IsEnemy && !focus.IsMe &&
                                        !focus.IsInvulnerable && focus.IsValidTarget()))
                    {
                        //Variables
                        var qdmg = spells[Spells.Q].GetDamage(focus);
                        var wdmg = spells[Spells.W].GetDamage(focus);
                        float ignitedmg;

                        //Ignite Damage
                        if (_igniteSlot != SpellSlot.Unknown)
                        {
                            ignitedmg =
                                (float) _player.GetSummonerSpellDamage(focus, Damage.SummonerSpell.Ignite);
                        }
                        else
                        {
                            ignitedmg = 0f;
                        }

                        //Mark Damage
                        var markDmg = _player.CalcDamage(focus, Damage.DamageType.Magical, _player.FlatMagicDamageMod * 0.15 + _player.Level * 15);

                        //Q
                        if (focus.Health - qdmg < 0 && spells[Spells.E].IsReady() && spells[Spells.Q].IsReady() &&
                            focus.Distance(target.ServerPosition) <= spells[Spells.Q].Range)
                        {
                            CastE(target);
                            spells[Spells.Q].Cast(focus);
                        }
                        // Q + W
                        if (focus.Distance(target.ServerPosition) <= spells[Spells.W].Range && focus.Health - qdmg - wdmg < 0 &&
                            spells[Spells.E].IsReady() && spells[Spells.Q].IsReady())
                        {
                            CastE(target);
                            spells[Spells.Q].Cast(focus);
                            spells[Spells.W].Cast();
                        }
                        // Q + W + Mark
                        if (focus.Distance(target.ServerPosition) <= spells[Spells.W].Range && focus.Health - qdmg - wdmg - markDmg < 0 &&
                            spells[Spells.E].IsReady() && spells[Spells.Q].IsReady() && spells[Spells.W].IsReady())
                        {
                            CastE(target);
                            spells[Spells.Q].Cast(focus);
                            spells[Spells.W].Cast();
                        }
                        // Q + Ignite
                        if (focus.Distance(target.ServerPosition) <= 600 && focus.Health - qdmg - ignitedmg < 0 &&
                            spells[Spells.E].IsReady() && spells[Spells.Q].IsReady() && _igniteSlot.IsReady())
                        {
                            CastE(target);
                            spells[Spells.Q].Cast(focus);
                            _player.Spellbook.CastSpell(_igniteSlot, focus);
                        }
                        // Q + W + Ignite
                        if (focus.Distance(target.ServerPosition) <= spells[Spells.W].Range &&
                            focus.Health - qdmg - wdmg - ignitedmg < 0 && spells[Spells.E].IsReady() && spells[Spells.Q].IsReady() && spells[Spells.W].IsReady() &&
                            _igniteSlot.IsReady())
                        {
                            CastE(target);
                            spells[Spells.Q].Cast(focus);
                            spells[Spells.W].Cast();
                            _player.Spellbook.CastSpell(_igniteSlot, focus);
                        }
                    }
                }
            }
        }
        #endregion

        #region Combo
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;

            UseItems(target);

            var rdmg = spells[Spells.R].GetDamage(target, 1);

            if (spells[Spells.Q].IsInRange(target))
            {
                if (spells[Spells.Q].IsReady())
                {
                    spells[Spells.Q].Cast(target);
                }
                if (spells[Spells.E].IsReady())
                {
                    CastE(target);
                }
            }
            else
            {
                if (spells[Spells.E].IsReady())
                {
                    CastE(target);
                }
                if (spells[Spells.Q].IsReady())
                {
                    spells[Spells.Q].Cast(target);
                }
            }

            if (spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(target))
            {
                spells[Spells.W].Cast();
                return;
            }

            //Smart R
            if (_config.Item("smartR").GetValue<bool>())
            {
                if (spells[Spells.R].IsReady() && target.Health - rdmg < 0 && !spells[Spells.E].IsReady())
                {
                    _orbwalker.SetMovement(false);
                    _orbwalker.SetAttack(false);
                    spells[Spells.R].Cast();

                    _rStart = Environment.TickCount;
                    Console.WriteLine("CAST ULT 1");
                }
            }
            else if (spells[Spells.R].IsReady() && !spells[Spells.E].IsReady())
            {
                _orbwalker.SetMovement(false);
                _orbwalker.SetAttack(false);
                spells[Spells.R].Cast();

                _rStart = Environment.TickCount;
            }
        }
        #endregion

        #region Harass
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;

            var menuItem = _config.Item("hMode").GetValue<StringList>().SelectedIndex; 

            switch (menuItem)
            {
                case 0: //1st mode: Q only
                    if (spells[Spells.Q].IsReady())
                    {
                        spells[Spells.Q].CastOnUnit(target);
                    }
                    break;
                case 1: //2nd mode: Q and W
                    if (spells[Spells.Q].IsReady() && spells[Spells.W].IsReady())
                    {
                        spells[Spells.Q].Cast(target);
                        if (spells[Spells.W].IsInRange(target))
                        {
                            spells[Spells.W].Cast();
                        }
                    }
                    break;
                case 2: //3rd mode: Q, E and W
                    if (spells[Spells.Q].IsReady() && spells[Spells.W].IsReady() && spells[Spells.E].IsReady())
                    {
                        spells[Spells.Q].Cast(target);
                        CastE(target);
                        spells[Spells.W].Cast();
                    }
                    break;
            }
        }

        #endregion

        #region LaneClear
        private static void Laneclear()
        {
            var useQ = _config.Item("qFarm").GetValue<bool>();
            var useW = _config.Item("qFarm").GetValue<bool>();
            var useE = _config.Item("eFarm").GetValue<bool>();

            var minions = MinionManager.GetMinions(_player.ServerPosition, spells[Spells.Q].Range);
            if (minions.Count <= 0)
                return;

            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spells[Spells.Q].Range, MinionTypes.All, MinionTeam.Neutral,
               MinionOrderTypes.MaxHealth);

            if (spells[Spells.Q].IsReady() && useQ)
            {
                foreach (
                     var minion in
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(
                                minion =>
                                    minion.IsValidTarget() && minion.IsEnemy &&
                                    minion.Distance(_player.ServerPosition) < spells[Spells.E].Range))
                {
                        spells[Spells.Q].CastOnUnit(minion);
                    return;
                }
            }

            if (useW && spells[Spells.W].IsReady() && spells[Spells.W].IsInRange(minions.FirstOrDefault())) //check
            {
                if (minions.Count > 2)
                {
                    var farmLocation = spells[Spells.W].GetCircularFarmLocation(minions);
                    spells[Spells.W].Cast(farmLocation.Position);
                }
            }

            foreach (var minion in
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        minion =>
                            minion.IsValidTarget() && minion.IsEnemy &&
                            minion.Distance(_player.ServerPosition) < spells[Spells.E].Range))
            {
                var edmg = spells[Spells.E].GetDamage(minion);

                if (minion.Health - edmg <= 0 && minion.Distance(_player.ServerPosition) <= spells[Spells.E].Range &&
                    spells[Spells.E].IsReady() && (_config.Item("eFarm").GetValue<bool>()))
                {
                    CastE(minion);
                }
            }

           /* if (useE && spells[Spells.E].IsReady())
            {
                foreach (
                    var minion in
                        allMinions.Where(
                            minion => minion.IsValidTarget()))
                {
                    //spells[Spells.E].CastOnUnit(minion);
                    CastE(minion);
                }
            }*/
        }
        #endregion

        #region Lasthit
        private static void Farm()
        {
            foreach (var minion in
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        minion =>
                            minion.IsValidTarget() && minion.IsEnemy &&
                            minion.Distance(_player.ServerPosition) < spells[Spells.E].Range))
            {
                var qdmg = spells[Spells.Q].GetDamage(minion);
                var wdmg = spells[Spells.W].GetDamage(minion);
                var edmg = spells[Spells.E].GetDamage(minion);
                var markDmg = _player.CalcDamage(
                    minion, Damage.DamageType.Magical, _player.FlatMagicDamageMod * 0.15 + _player.Level * 15);

                //Killable with Q
                if (minion.Health - qdmg <= 0 && minion.Distance(_player.ServerPosition) <= spells[Spells.Q].Range &&
                    spells[Spells.Q].IsReady() && (_config.Item("wFarm").GetValue<bool>()))
                {
                    spells[Spells.Q].CastOnUnit(minion);
                }

                //Killable with W
                if (minion.Health - wdmg <= 0 && minion.Distance(_player.ServerPosition) <= spells[Spells.W].Range &&
                    spells[Spells.W].IsReady() && (_config.Item("wFarm").GetValue<bool>()))
                {
                    spells[Spells.Q].Cast();
                }

                //Killable with E
                if (minion.Health - edmg <= 0 && minion.Distance(_player.ServerPosition) <= spells[Spells.E].Range &&
                    spells[Spells.E].IsReady() && (_config.Item("eFarm").GetValue<bool>()))
                {
                    CastE(minion);
                }

                //Killable with Q and W
                if (minion.Health - wdmg - qdmg <= 0 &&
                    minion.Distance(_player.ServerPosition) <= spells[Spells.W].Range && spells[Spells.Q].IsReady() &&
                    spells[Spells.W].IsReady() && (_config.Item("qFarm").GetValue<bool>()) &&
                    (_config.Item("wFarm").GetValue<bool>()))
                {
                    spells[Spells.Q].Cast(minion);
                    spells[Spells.W].Cast();
                }

                //Killable with Q, W and Mark
                if (minion.Health - wdmg - qdmg - markDmg <= 0 &&
                    minion.Distance(_player.ServerPosition) <= spells[Spells.W].Range && spells[Spells.Q].IsReady() &&
                    spells[Spells.W].IsReady() && (_config.Item("qFarm").GetValue<bool>()) &&
                    (_config.Item("wFarm").GetValue<bool>()))
                {
                    spells[Spells.Q].Cast(minion);
                    spells[Spells.W].Cast();
                }

                //Killable with Q, W, E and Mark
                if (minion.Health - wdmg - qdmg - markDmg - edmg <= 0 &&
                    minion.Distance(_player.ServerPosition) <= spells[Spells.W].Range && spells[Spells.E].IsReady() &&
                    spells[Spells.Q].IsReady() && spells[Spells.W].IsReady() && (_config.Item("qFarm").GetValue<bool>()) &&
                    (_config.Item("wFarm").GetValue<bool>()) && (_config.Item("eFarm").GetValue<bool>()))
                {
                    CastE(minion);
                    spells[Spells.Q].Cast(minion);
                    spells[Spells.W].Cast();
                }
            }
        }
        #endregion

        #region Obj_AI_Base_OnProcessSpellCast
        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || args.SData.Name != "KatarinaR" || !_player.HasBuff("katarinarsound", true))
                return;

            _isChanneling = true;
            _orbwalker.SetMovement(false);
            _orbwalker.SetAttack(false);
            Utility.DelayAction.Add(1, () => _isChanneling = false);
        }
        #endregion

        #region Obj_AI_Hero_OnIssueOrder
        private static void Obj_AI_Hero_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (sender.IsMe && Environment.TickCount < _rStart + 300)
            {
                args.Process = false;
            }
        }
        #endregion

        #region ComboDamage
        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (spells[Spells.Q].IsReady())
            {
                damage += spells[Spells.Q].GetDamage(enemy);
            }

            if (spells[Spells.W].IsReady())
            {
                damage += spells[Spells.W].GetDamage(enemy);
            }

            if (spells[Spells.E].IsReady())
            {
                damage += spells[Spells.E].GetDamage(enemy);
            }

            if (spells[Spells.R].IsReady())
            {
                damage += spells[Spells.R].GetDamage(enemy);
            }

            if (_igniteSlot == SpellSlot.Unknown || _player.Spellbook.CanUseSpell(_igniteSlot) != SpellState.Ready)
            {
                damage += (float)_player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return damage;
        }

        #endregion

        #region JungleClear
        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(
                _player.ServerPosition, spells[Spells.W].Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count <= 0)
            {
                return;
            }

            var mob = mobs[0];
            if (mob == null)
            {
                return;
            }

            if (_config.Item("qJungle").GetValue<bool>() && spells[Spells.Q].IsReady())
            {
                spells[Spells.Q].CastOnUnit(mob);
            }

            if (_config.Item("wJungle").GetValue<bool>() && spells[Spells.W].IsReady())
            {
                spells[Spells.W].CastOnUnit(mob);
            }

            if (_config.Item("eJungle").GetValue<bool>() && spells[Spells.E].IsReady())
            {
                spells[Spells.E].CastOnUnit(mob);
            }
        }

        #endregion

        #region Notifications 

        private static void ShowNotification(string message, Color color, int duration = -1, bool dispose = true)
        {
            Notifications.AddNotification(new Notification(message, duration, dispose).SetTextColor(color));
        }

        #endregion

        #region Menu

        private static void MenuLoad()
        {
            _config = new Menu("ElKatarina", "Katarina", true);

            //Orbwalker Menu
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Target Selector Menu
            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            _config.AddSubMenu(tsMenu);

            _config.AddSubMenu(new Menu("Smart Combo", "combo"));
            _config.SubMenu("combo").AddItem(new MenuItem("smartR", "Use Smart R").SetValue(true));
            _config.SubMenu("combo").AddItem(new MenuItem("wjCombo", "Use WardJump in Combo").SetValue(true));

            _config.AddSubMenu(new Menu("Harass", "harass"));
            _config.SubMenu("harass").AddItem(new MenuItem("hMode", "Harass Mode: ").SetValue(new StringList(new[] { "Q only", "Q+W", "Q+E+W" })));

            _config.SubMenu("harass").SubMenu("AutoHarass settings").AddItem(new MenuItem("ElKatarina.AutoHarass.Activated", "Auto harass", true).SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle)));
            _config.SubMenu("harass").SubMenu("AutoHarass settings").AddItem(new MenuItem("ElKatarina.AutoHarass.Q", "Use Q").SetValue(true));
            _config.SubMenu("harass").SubMenu("AutoHarass settings").AddItem(new MenuItem("ElKatarina.AutoHarass.W", "Use W").SetValue(true));

            _config.AddSubMenu(new Menu("Farming", "farm"));
            _config.SubMenu("farm").AddItem(new MenuItem("smartFarm", "Use Smart Farm").SetValue(true));
            _config.SubMenu("farm").AddItem(new MenuItem("qFarm", "Use Q").SetValue(true));
            _config.SubMenu("farm").AddItem(new MenuItem("wFarm", "Use W").SetValue(true));
            _config.SubMenu("farm").AddItem(new MenuItem("eFarm", "Use E").SetValue(true));

            _config.AddSubMenu(new Menu("Jungle Clear", "jungle"));
            _config.SubMenu("jungle").AddItem(new MenuItem("qJungle", "Use Q").SetValue(true));
            _config.SubMenu("jungle").AddItem(new MenuItem("wJungle", "Use W").SetValue(true));
            _config.SubMenu("jungle").AddItem(new MenuItem("eJungle", "Use E").SetValue(true));

            _config.AddSubMenu(new Menu("Killsteal", "KillSteal"));
            _config.SubMenu("KillSteal").AddItem(new MenuItem("KillSteal", "Smart").SetValue(true));
            _config.SubMenu("KillSteal").AddItem(new MenuItem("jumpsS", "Use E").SetValue(true));

            _config.AddSubMenu(new Menu("Items", "Items"));
            _config.SubMenu("Items").AddItem(new MenuItem("ElKatarina.Items.hextech", "Use Hextech Gunblade").SetValue(true));

            _config.AddSubMenu(new Menu("Draw", "drawing"));
            _config.SubMenu("drawing").AddItem(new MenuItem("mDraw", "Disable all drawings").SetValue(false));
            _config.SubMenu("drawing").AddItem(new MenuItem("Target", "Highlight Target").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 0))));
            _config.SubMenu("drawing").AddItem(new MenuItem("QDraw", "Draw Q").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            _config.SubMenu("drawing").AddItem(new MenuItem("WDraw", "Draw W").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            _config.SubMenu("drawing").AddItem(new MenuItem("EDraw", "Draw E").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            _config.SubMenu("drawing").AddItem(new MenuItem("RDraw", "Draw R").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));

            var dmgAfterE = new MenuItem("ElKatarina.DrawComboDamage", "Draw combo damage").SetValue(true);
            var drawFill = new MenuItem("ElKatarina.DrawColour", "Fill colour", true).SetValue(new Circle(true, Color.FromArgb(204, 255, 0, 1)));
            _config.SubMenu("drawing").AddItem(drawFill);
            _config.SubMenu("drawing").AddItem(dmgAfterE);

            DrawDamage.DamageToUnit = GetComboDamage;
            DrawDamage.Enabled = dmgAfterE.GetValue<bool>();
            DrawDamage.Fill = drawFill.GetValue<Circle>().Active;
            DrawDamage.FillColor = drawFill.GetValue<Circle>().Color;

            dmgAfterE.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                DrawDamage.Enabled = eventArgs.GetNewValue<bool>();
            };

            drawFill.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                DrawDamage.Fill = eventArgs.GetNewValue<Circle>().Active;
                DrawDamage.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };

            //Misc Menu
            _config.AddSubMenu(new Menu("Misc", "misc"));
            _config.SubMenu("misc").AddItem(new MenuItem("playLegit", "Legit E").SetValue(false));
            _config.SubMenu("misc").AddItem(new MenuItem("legitCastDelay", "Legit E Delay").SetValue(new Slider(1000, 0, 2000)));
            _config.SubMenu("misc").AddItem(new MenuItem("ElKatarina.misc.Notifications", "Use notifications").SetValue(true));

            //Wardjump Menu
            _config.AddSubMenu(new Menu("WardJump Settings", "wardjump"));
            _config.SubMenu("wardjump").AddItem(new MenuItem("wardjumpkey", "WardJump key").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));

            if (_igniteSlot != SpellSlot.Unknown)
            {
                _config.SubMenu("misc").AddItem(new MenuItem("autoIgnite", "Auto ignite when killable").SetValue(true));
            }

            var credits = new Menu("Credits", "jQuery");
            credits.AddItem(new MenuItem("ElKatarina.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElKatarina.Email", "info@zavox.nl"));
            _config.AddSubMenu(credits);

            _config.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _config.AddItem(new MenuItem("422442fsaafsf", "Version: 1.0.0.7"));
            _config.AddItem(new MenuItem("fsasfafsfsafsa", "Made By Jouza - jQuery "));


            _config.AddToMainMenu();
        }

        #endregion

        #region Drawings
        private static void Drawings(EventArgs args)
        {
            var drawOff = _config.Item("mDraw").GetValue<bool>();
            var drawQ = _config.Item("QDraw").GetValue<Circle>();
            var drawW = _config.Item("WDraw").GetValue<Circle>();
            var drawE = _config.Item("EDraw").GetValue<Circle>();
            var drawR = _config.Item("RDraw").GetValue<Circle>();

            if (drawOff)
                return;

            if (drawQ.Active)
                if (spells[Spells.Q].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.Q].Range, Color.White);

            if (drawW.Active)
                if (spells[Spells.W].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.W].Range, Color.White);

            if (drawE.Active)
                if (spells[Spells.E].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.E].Range, Color.White);

            if (drawR.Active)
                if (spells[Spells.R].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spells[Spells.R].Range, Color.White);

            var target = TargetSelector.GetTarget(spells[Spells.Q].Range, TargetSelector.DamageType.Magical);
            if (_config.Item("Target").GetValue<Circle>().Active && target != null)
            {
                Render.Circle.DrawCircle(target.Position, 50, _config.Item("Target").GetValue<Circle>().Color);
            }
        }
        #endregion
    }
}
