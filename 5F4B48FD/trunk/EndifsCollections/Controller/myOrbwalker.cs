using System;
using System.Collections.Generic;
using System.Linq;
using EndifsCollections.SummonerSpells;
using EndifsCollections.Tools;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace EndifsCollections.Controller
{
    internal class myOrbwalker
    {
        public delegate void AfterAttackEvenH(Obj_AI_Base unit, Obj_AI_Base target);

        public delegate void BeforeAttackEvenH(BeforeAttackEventArgs args);

        public delegate void OnAttackEvenH(Obj_AI_Base unit, Obj_AI_Base target);

        public delegate void OnNonKillableMinionH(AttackableUnit minion);

        public delegate void OnTargetChangeH(Obj_AI_Base oldTarget, Obj_AI_Base newTarget);

        public enum OrbwalkingMode
        {
            Combo,
            Harass,
            LaneClear,
            JungleClear,
            Hybrid,
            Lasthit,
            Flee,
            Custom,
            None
        }

        //private const float LaneClearWaitTimeMod = TimeMod;

        private static readonly string[] AttackResets =
        {
            "blindingdart", "khazixq", "khazixqlong", "sonaq",
            "pantheonew", "pantheoneq", "dariusnoxiantacticsonh", "fioraflurry", "garenq", "hecarimrapidslash",
            "jaxempowertwo", "jaycehypercharge", "leonashieldofdaybreak", "luciane", "lucianq", "monkeykingdoubleattack",
            "mordekaisermaceofspades", "nasusq", "nautiluspiercinggaze", "netherblade", "parley", "poppydevastatingblow",
            "powerfist", "renektonpreexecute",  "shyvanadoubleattack", "sivirw", "takedown",
            "talonnoxiandiplomacy", "trundletrollsmash", "vaynetumble", "vie", "volibearq", "xenzhaocombotarget",
            "yorickspectral","shyvanadoubleattack","rengarq", "rengarqemp"
        };

        private static readonly string[] NoAttacks =
        {
            "jarvanivcataclysmattack", "monkeykingdoubleattack", "zyragraspingplantattack", "zyragraspingplantattack2", "zyragraspingplantattackfire", "zyragraspingplantattack2fire",
            "elisespiderlingbasicattack", "heimertyellowbasicattack", "heimertyellowbasicattack2", "heimertbluebasicattack",
            "annietibbersbasicattack", "annietibbersbasicattack2", "yorickdecayedghoulbasicattack", "yorickravenousghoulbasicattack",
            "yorickspectralghoulbasicattack", "malzaharvoidlingbasicattack", "malzaharvoidlingbasicattack2", "malzaharvoidlingbasicattack3"
        };

        private static readonly string[] Attacks =
        {
            "caitlynheadshotmissile", "frostarrow", "garenslash2",
            "kennenmegaproc", "lucianpassiveattack", "masteryidoublestrike", "quinnwenhanced", "renektonexecute",
            "renektonsuperexecute", "rengarnewpassivebuffdash", "trundleq", "xenzhaothrust", "xenzhaothrust2",
            "xenzhaothrust3", "sivirwattackbounce",
        };
       
        private static readonly string[] NoResets = { "Kalista" };
        private static readonly string[] IgnorePassive = { "Thresh" };

        private static Menu Menu;
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        
        private static Obj_AI_Hero ForcedTarget;
        private static Vector3 CustomPoint;        
        private static int WindUpDuration;
        private static int ResetDelayDuration;
        private static Obj_AI_Minion PrevMinion;

        private static  IEnumerable<Obj_AI_Hero> AllEnemies;
        
        private static bool AttackBool = true;
        private static bool MovementBool = true;
        private static bool DisableNextAttack;
        private static float LastAACastDelay;
        private static float LastAADelay;
        private static int LastAATick;
        private static int LastCastTime;
        private static Obj_AI_Base ProcessSpellLastTarget;
        private static int LastMovement;
        private static bool MissileLaunched;

        private static bool ComboBool
        {
            get { return Menu.Item("Combo_Bool").GetValue<bool>(); }
        }

        private static bool HarassBool
        {
            get { return Menu.Item("Harass_Bool").GetValue<bool>(); }
        }

        private static bool LaneClearBool
        {
            get { return Menu.Item("LaneClear_Bool").GetValue<bool>(); }
        }

        private static bool HybridBool
        {
            get { return Menu.Item("Hybrid_Bool").GetValue<bool>(); }
        }

        private static bool LastHitBool
        {
            get { return Menu.Item("LastHit_Bool").GetValue<bool>(); }
        }

        private static bool JungleClearBool
        {
            get { return Menu.Item("JungleClear_Bool").GetValue<bool>(); }
        }

        private static bool CustomModeBool
        {
            get { return Menu.Item("CustomMode_Bool").GetValue<bool>(); }
        }

        private static bool FleeBool
        {
            get { return Menu.Item("Flee_Bool").GetValue<bool>(); }
        }

        private static bool JungleMove
        {
            get { return Menu.Item("Melee_JungleMoveInAA").GetValue<bool>(); }
        }

        private static bool ComboMagnet
        {
            get { return Menu.Item("Combo_magnet").GetValue<bool>(); }
        }

        private static int FarmDelay
        {
            get { return Menu.Item("Misc_Farmdelay").GetValue<Slider>().Value; }
        }

        private static float LaneClearWaitTimeMod
        {
            get { return Menu.Item("LaneClear_timemod").GetValue<Slider>().Value; }
        }

        private static bool MissileCheck
        {
            get { return Menu.Item("Misc_Missilecheck").GetValue<bool>(); }
        }

        public static OrbwalkingMode ActiveMode
        {
            get
            {
                if (Menu.Item("Combo_Key").GetValue<KeyBind>().Active && ComboBool)
                {
                    return OrbwalkingMode.Combo;
                }
                if (Menu.Item("Harass_Key").GetValue<KeyBind>().Active && HarassBool)
                {
                    return OrbwalkingMode.Harass;
                }
                if (Menu.Item("LaneClear_Key").GetValue<KeyBind>().Active && LaneClearBool)
                {
                    return OrbwalkingMode.LaneClear;
                }
                if (Menu.Item("Hybrid_Key").GetValue<KeyBind>().Active && HybridBool)
                {
                    return OrbwalkingMode.Hybrid;
                }
                if (Menu.Item("JungleClear_Key").GetValue<KeyBind>().Active && JungleClearBool)
                {
                    return OrbwalkingMode.JungleClear;
                }
                if (Menu.Item("LastHit_Key").GetValue<KeyBind>().Active && LastHitBool)
                {
                    return OrbwalkingMode.Lasthit;
                }
                if (Menu.Item("CustomMode_Key").GetValue<KeyBind>().Active && CustomModeBool)
                {
                    return OrbwalkingMode.Custom;
                }
                if (Menu.Item("Flee_Key").GetValue<KeyBind>().Active && FleeBool)
                {
                    return OrbwalkingMode.Flee;
                }
                return OrbwalkingMode.None;
            }
        }

        public static event BeforeAttackEvenH BeforeAttack;
        public static event OnTargetChangeH OnTargetChange;
        public static event AfterAttackEvenH AfterAttack;
        public static event OnAttackEvenH OnAttack;
        public static event OnNonKillableMinionH OnNonKillableMinion;

        private static void FireBeforeAttack(Obj_AI_Base target)
        {
            if (BeforeAttack != null)
            {
                BeforeAttack(new BeforeAttackEventArgs { Target = target });
            }
            else
            {
                DisableNextAttack = false;
            }
        }

        private static void FireOnTargetSwitch(Obj_AI_Base newTarget)
        {
            //if (OnTargetChange != null && (ProcessSpellLastTarget == null || ProcessSpellLastTarget.NetworkId != newTarget.NetworkId))
            if (OnTargetChange != null && (ProcessSpellLastTarget == null || ProcessSpellLastTarget != newTarget))
            {
                OnTargetChange(ProcessSpellLastTarget, newTarget);
            }
        }

        private static void FireAfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (AfterAttack != null && target.IsValidTarget())
            {
                AfterAttack(unit, target);
            }
        }

        private static void FireOnAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (OnAttack != null)
            {
                OnAttack(unit, target);
            }
        }

        private static void FireOnNonKillableMinion(AttackableUnit minion)
        {
            if (OnNonKillableMinion != null)
            {
                OnNonKillableMinion(minion);
            }
        }

        public static bool IsAutoAttackReset(string name)
        {
            return AttackResets.Contains(name.ToLower());
        }

        public static bool IsMelee(Obj_AI_Base unit)
        {
            return unit.CombatType == GameObjectCombatType.Melee;
        }

        public static bool IsAutoAttack(string name)
        {
            return (name.ToLower().Contains("attack") && !NoAttacks.Contains(name.ToLower())) ||
                   Attacks.Contains(name.ToLower());
        }

        public static float GetRealAutoAttackRange(Obj_AI_Base source = null, Obj_AI_Base target = null)
        {
            if (source == null)
            {
                source = Player;
            }
            var result = (source.AttackRange + source.BoundingRadius);
            if (target != null && target.IsValidTarget())
            {
                result += target.BoundingRadius;
            }
            return result;
        }

        public static bool InFindRange(Obj_AI_Base target)
        {
            if (!target.IsValidTarget())
            {
                return false;
            }
            var myRange = GetRealAutoAttackRange(Player, target);

            return Vector2.DistanceSquared(
                    (target is Obj_AI_Base) ? ((Obj_AI_Base)target).ServerPosition.To2D() : target.Position.To2D(),
                    Player.ServerPosition.To2D()) <= myRange * myRange;
        }

        private static float MyProjectileSpeed()
        {
            return IsMelee(Player) ? float.MaxValue : Player.BasicAttack.MissileSpeed;
        }

        public static bool CanAttack()
        {
            if (LastAATick < myUtility.TickCount || LastCastTime < myUtility.TickCount)
            {
                var AADelay = Math.Max(LastAADelay, Player.AttackDelay * 1000);
                return myUtility.TickCount + Game.Ping / 2 >= LastAATick + AADelay && AttackBool;
            }
            return false;
        }

        public static bool CanMove()
        {
            if (!MovementBool) return false;
            if (MissileLaunched && MissileCheck)
            {
                return true;
            }
            var extraWindup = GetWindUp();
            if (LastAATick < myUtility.TickCount)
            {
                var CastDelay = Math.Max(LastAACastDelay, Player.AttackCastDelay * 1000);
                return NoResets.Contains(Player.ChampionName)
                    ? (myUtility.TickCount - LastAATick > 300)
                    : (myUtility.TickCount + Game.Ping / 2 >= LastAATick + CastDelay + extraWindup);
            }
            return false;
        }

        private static void MoveTo(Vector3 position)
        {
            var delay = Menu.Item("Misc_Humanizer").GetValue<Slider>().Value;
            if (myUtility.TickCount - LastMovement < delay)
            {
                return;
            }
            if (!CanMove() || !IsAllowedToMove())
            {
                return;
            }
            if (Vector3.Distance(Game.CursorPos, Player.ServerPosition) < 500)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, position);
                LastMovement = myUtility.TickCount;
            }
            else
            {
                var point = Player.ServerPosition + 500 * (position - Player.ServerPosition).Normalized();
                Player.IssueOrder(GameObjectOrder.MoveTo, point);
                //Player.IssueOrder(GameObjectOrder.MoveTo, myUtility.RandomPos(1, 100, 300, point));               
                LastMovement = myUtility.TickCount;
            }
        }

        private static void Orbwalk(Vector3 goalPosition, Obj_AI_Base target)
        {
            if (target.IsValidTarget() && CanAttack() && IsAllowedToAttack())
            {
                DisableNextAttack = false;
                FireBeforeAttack(target);
                if (!DisableNextAttack)
                {
                    if (ActiveMode != OrbwalkingMode.Combo)
                    {
                        foreach (var obj in
                            ObjectManager.Get<Obj_Building>()
                                .Where(
                                    obj =>
                                        obj.Position.Distance(Player.Position) <=
                                        GetRealAutoAttackRange() + obj.BoundingRadius / 2 && obj.IsTargetable
                                        && (obj is Obj_HQ || obj is Obj_BarracksDampener)
                                        ))
                        {
                            Player.IssueOrder(GameObjectOrder.AttackTo, obj.Position);
                            LastAATick = myUtility.TickCount + (int)(ObjectManager.Player.AttackCastDelay * 1000f);
                            return;
                        }
                    }
                    if (!NoResets.Contains(Player.ChampionName))
                    {
                        LastAACastDelay = Player.AttackCastDelay * 1000;
                        LastAADelay = Player.AttackDelay * 1000;
                        MissileLaunched = false;
                    }
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                    LastAATick = myUtility.TickCount + Game.Ping / 2;
                    ProcessSpellLastTarget = target;
                    return;
                }
            }
            if (!CanMove() || !IsAllowedToMove())
            {
                return;
            }
            if (CanMove())
            {
                MoveTo(goalPosition);
            }
        }

        private static void SpellbookOnStopCast(Spellbook spellbook, SpellbookStopCastEventArgs args)
        {
            if (spellbook.Owner.IsValid && spellbook.Owner.IsMe && args.DestroyMissile && args.StopAnimation)
            {
                Utility.DelayAction.Add(GetResetDelay(), ResetAutoAttackTimer);
            }
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender is MissileClient && sender.IsValid)
            {
                var missile = (MissileClient)sender;
                if (missile.SpellCaster.Name == Player.Name && IsAutoAttack(missile.SData.Name))
                {
                    MissileLaunched = true;
                }
            }
        }

        private static void OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (IsAutoAttackReset(spell.SData.Name) && unit.IsMe)
            {
                Utility.DelayAction.Add(GetResetDelay(), ResetAutoAttackTimer);
            }
            if (!IsAutoAttack(spell.SData.Name))
            {
                if (unit.IsMe)
                {
                    var ct = (unit.Spellbook.CastTime - Game.Time) * 1000;
                    if (ct > 0)
                    {
                        LastCastTime = myUtility.TickCount + (int)ct;
                    }
                }
                return;
            }
            if (unit.IsMe && spell.Target is Obj_AI_Base)
            {
                LastAATick = myUtility.TickCount;
                MissileLaunched = false;
                var target = (Obj_AI_Base)spell.Target;
                if (target.IsValid)
                {
                    FireOnTargetSwitch(target);
                    ProcessSpellLastTarget = target;
                }
                if (unit.IsMelee())
                {
                    Utility.DelayAction.Add((int)(unit.AttackCastDelay * 1000 + 50), () => FireAfterAttack(unit, ProcessSpellLastTarget));
                }
            }
            FireOnAttack(unit, ProcessSpellLastTarget);
        }

        public class BeforeAttackEventArgs
        {
            private bool _process = true;
            public Obj_AI_Base Target;
            public Obj_AI_Base Unit = ObjectManager.Player;

            public bool Process
            {
                get { return _process; }
                set
                {
                    DisableNextAttack = !value;
                    _process = value;
                }
            }
        }

        public static void AddToMenu(Menu menu)
        {
            Menu = menu;
            var menuModes = new Menu("Modes", "Modes");
            {
                var modeCombo = new Menu("Combo", "Modes_Combo");
                modeCombo.AddItem(new MenuItem("Combo_Bool", "Enabled").SetValue(true));
                modeCombo.AddItem(
                    new MenuItem("Combo_Key", "Key").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
                modeCombo.AddItem(new MenuItem("Combo_move", "Movement").SetValue(true));
                modeCombo.AddItem(new MenuItem("Combo_attack", "Attack").SetValue(true));
                modeCombo.AddItem(new MenuItem("Combo_magnet", "Magnetic (Melee)").SetValue(true));
                menuModes.AddSubMenu(modeCombo);

                var modeHarass = new Menu("Harass", "Modes_Harass");
                modeHarass.AddItem(new MenuItem("Harass_Bool", "Enabled").SetValue(true));
                modeHarass.AddItem(
                    new MenuItem("Harass_Key", "Key").SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));
                modeHarass.AddItem(new MenuItem("Harass_move", "Movement").SetValue(true));
                modeHarass.AddItem(new MenuItem("Harass_attack", "Auto Attack").SetValue(true));
                modeHarass.AddItem(new MenuItem("Harass_Lasthit", "Last Hit Minions").SetValue(true));
                menuModes.AddSubMenu(modeHarass);

                var modeLaneClear = new Menu("Lane Clear", "Modes_LaneClear");
                modeLaneClear.AddItem(new MenuItem("LaneClear_Bool", "Enabled").SetValue(true));
                modeLaneClear.AddItem(new MenuItem("LaneClear_Key", "Key").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                modeLaneClear.AddItem(new MenuItem("LaneClear_move", "Movement").SetValue(true));
                modeLaneClear.AddItem(new MenuItem("LaneClear_attack", "Auto Attack").SetValue(true));
                modeLaneClear.AddItem(new MenuItem("LaneClear_timemod", "Time Mod").SetValue(new Slider(2, 1, 5)));
                menuModes.AddSubMenu(modeLaneClear);

                var modeHybrid = new Menu("Hybrid", "Modes_Hybrid");
                modeHybrid.AddItem(new MenuItem("Hybrid_Bool", "Enabled").SetValue(false));
                modeHybrid.AddItem(
                    new MenuItem("Hybrid_Key", "Key").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
                modeHybrid.AddItem(new MenuItem("Hybrid_move", "Movement").SetValue(true));
                modeHybrid.AddItem(new MenuItem("Hybrid_attack", "Attack").SetValue(true));
                menuModes.AddSubMenu(modeHybrid);

                var modeLasthit = new Menu("Last Hit", "Modes_LastHit");
                modeLasthit.AddItem(new MenuItem("LastHit_Bool", "Enabled").SetValue(true));
                modeLasthit.AddItem(
                    new MenuItem("LastHit_Key", "Key").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                modeLasthit.AddItem(new MenuItem("LastHit_move", "Movement").SetValue(true));
                modeLasthit.AddItem(new MenuItem("LastHit_attack", "Auto Attack").SetValue(true));
                menuModes.AddSubMenu(modeLasthit);

                var modeJungleClear = new Menu("Jungle Clear", "Modes_JungleClear");
                modeJungleClear.AddItem(new MenuItem("JungleClear_Bool", "Enabled").SetValue(true));
                modeJungleClear.AddItem(
                    new MenuItem("JungleClear_Key", "Key").SetValue(
                        new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                modeJungleClear.AddItem(new MenuItem("JungleClear_move", "Movement").SetValue(true));
                modeJungleClear.AddItem(new MenuItem("JungleClear_attack", "Attack").SetValue(true));
                menuModes.AddSubMenu(modeJungleClear);

                var modeCustomMode = new Menu("Custom Key", "Modes_Custom");
                modeCustomMode.AddItem(new MenuItem("CustomMode_Bool", "Enabled").SetValue(false));
                modeCustomMode.AddItem(
                    new MenuItem("CustomMode_Key", "Key").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                modeCustomMode.AddItem(new MenuItem("CustomMode_move", "Movement").SetValue(true));
                modeCustomMode.AddItem(new MenuItem("CustomMode_attack", "Attack").SetValue(true));

                menuModes.AddSubMenu(modeCustomMode);

                var modeFlee = new Menu("Flee", "Modes_Flee");
                modeFlee.AddItem(new MenuItem("Flee_Bool", "Enabled").SetValue(false));
                modeFlee.AddItem(new MenuItem("Flee_Key", "Key").SetValue(new KeyBind(32, KeyBindType.Press)));
                menuModes.AddSubMenu(modeFlee);
            }
            menu.AddSubMenu(menuModes);

            var menuBestTarget = new Menu("Target settings", "Target");
            menuBestTarget.AddItem(new MenuItem("ImmuneCheck", "(Combo) Check Physical Immunity").SetValue(true));

            menu.AddSubMenu(menuBestTarget);

            var menuMelee = new Menu("Melee", "Melee");            
            menuMelee.AddItem(new MenuItem("Melee_JungleMoveInAA", "(Jungle) Movement while AA").SetValue(false));
            menu.AddSubMenu(menuMelee);

            var menuMisc = new Menu("Misc", "Misc");
            menuMisc.AddItem(new MenuItem("Misc_ExtraWindUp", "Extra Winduptime").SetValue(new Slider(0, 0, 400)));
            menuMisc.AddItem(new MenuItem("Misc_AutoWindUp", "Autoset Windup").SetValue(false));

            menuMisc.AddItem(new MenuItem("Misc_Humanizer", "Movement Delayer").SetValue(new Slider(0, 0, 400)));
            menuMisc.AddItem(new MenuItem("Misc_Missilecheck", "Missile Check").SetValue(true));
            menuMisc.AddItem(new MenuItem("Misc_Farmdelay", "Farm Delay").SetValue(new Slider(0, 0, 400)));
            menuMisc.AddItem(new MenuItem("Misc_AAReset", "AA Reset Delay").SetValue(new Slider(0, 0, 400)));

            menuMisc.AddItem(new MenuItem("Misc_AllMovementDisabled", "Disable All Movement").SetValue(false));
            menuMisc.AddItem(new MenuItem("Misc_AllAttackDisabled", "Disable All Attacks").SetValue(false));
            menu.AddSubMenu(menuMisc);

            var menuDrawing = new Menu("Drawing", "Draw");
            menuDrawing.AddItem(new MenuItem("Draw_Lasthit", "Minion Lasthit").SetValue(new Circle(true, Color.Red)));
            menuDrawing.AddItem(
                new MenuItem("Draw_nearKill", "Minion nearKill").SetValue(new Circle(true, Color.Yellow)));
            menu.AddSubMenu(menuDrawing);

            AllEnemies = ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy);     

            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;            
            Spellbook.OnStopCast += SpellbookOnStopCast;
            GameObject.OnCreate += OnCreate;
        }

        private static void OnUpdate(EventArgs args)
        {
            CheckAutoWindUp();
            if (ActiveMode == OrbwalkingMode.None)
            {
                CustomPointReset();
                return;
            }
            if (Player.IsCastingInterruptableSpell(true))
            {
                return;
            }
            if (MenuGUI.IsChatOpen)
            {
                return;
            }
            var target = GetTarget();
            if (ComboMagnet && Player.IsMelee())
            {
                Orbwalk(CustomPoint != new Vector3() ? CustomPoint : Game.CursorPos, target);
            }
            else
            {
                Orbwalk(Game.CursorPos, target);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
           
            if (Menu.Item("Draw_Lasthit").GetValue<Circle>().Active ||
                Menu.Item("Draw_nearKill").GetValue<Circle>().Active)
            {
                var minionList = MinionManager.GetMinions(
                    Player.Position, GetRealAutoAttackRange(), MinionTypes.All, MinionTeam.Enemy,
                    MinionOrderTypes.MaxHealth);
                foreach (var minion in minionList.Where(minion => minion.IsValidTarget(GetRealAutoAttackRange() + 500)))
                {
                    if (Menu.Item("Draw_Lasthit").GetValue<Circle>().Active &&
                        minion.Health <= Player.GetAutoAttackDamage(minion, true))
                    {
                        Render.Circle.DrawCircle(
                            minion.Position, minion.BoundingRadius, Menu.Item("Draw_Lasthit").GetValue<Circle>().Color);
                    }
                    else if (Menu.Item("Draw_nearKill").GetValue<Circle>().Active &&
                             minion.Health < Player.GetAutoAttackDamage(minion, true) * 2)
                    {
                        Render.Circle.DrawCircle(
                            minion.Position, minion.BoundingRadius,
                            Menu.Item("Draw_nearKill").GetValue<Circle>().Color);
                    }
                }
            }
        }

        private static bool IsAllowedToMove()
        {
            if (!MovementBool)
            {
                return false;
            }
            if (Menu.Item("Misc_AllMovementDisabled").GetValue<bool>())
            {
                return false;
            }
            if (ActiveMode == OrbwalkingMode.Combo && !Menu.Item("Combo_move").GetValue<bool>() && ComboBool)
            {
                return false;
            }
            if (ActiveMode == OrbwalkingMode.Harass && !Menu.Item("Harass_move").GetValue<bool>() && HarassBool)
            {
                return false;
            }
            if (ActiveMode == OrbwalkingMode.LaneClear && !Menu.Item("LaneClear_move").GetValue<bool>() && LaneClearBool)
            {
                return false;
            }
            if (ActiveMode == OrbwalkingMode.JungleClear && !Menu.Item("JungleClear_move").GetValue<bool>() &&
                JungleClearBool)
            {
                return false;
            }
            if (ActiveMode == OrbwalkingMode.Hybrid && !Menu.Item("Hybrid_move").GetValue<bool>() && HybridBool)
            {
                return false;
            }
            if (ActiveMode == OrbwalkingMode.Custom && !Menu.Item("CustomMode_move").GetValue<bool>() && CustomModeBool)
            {
                return false;
            }
            return ActiveMode != OrbwalkingMode.Lasthit || (Menu.Item("LastHit_move").GetValue<bool>() && LastHitBool);
        }

        private static bool IsAllowedToAttack()
        {
            if (!AttackBool)
            {
                return false;
            }
            if (Menu.Item("Misc_AllAttackDisabled").GetValue<bool>())
            {
                return false;
            }
            if (ActiveMode == OrbwalkingMode.Combo && !Menu.Item("Combo_attack").GetValue<bool>() && ComboBool)
            {
                return false;
            }
            if (ActiveMode == OrbwalkingMode.Harass && !Menu.Item("Harass_attack").GetValue<bool>() && HarassBool)
            {
                return false;
            }
            if (ActiveMode == OrbwalkingMode.LaneClear && !Menu.Item("LaneClear_attack").GetValue<bool>() &&
                LaneClearBool)
            {
                return false;
            }
            if (ActiveMode == OrbwalkingMode.JungleClear && !Menu.Item("JungleClear_attack").GetValue<bool>() &&
                JungleClearBool)
            {
                return false;
            }
            if (ActiveMode == OrbwalkingMode.Hybrid && !Menu.Item("Hybrid_attack").GetValue<bool>() &&
                HybridBool)
            {
                return false;
            }
            if (ActiveMode == OrbwalkingMode.Custom && !Menu.Item("CustomMode_attack").GetValue<bool>() &&
                CustomModeBool)
            {
                return false;
            }
            return ActiveMode != OrbwalkingMode.Lasthit || (Menu.Item("LastHit_attack").GetValue<bool>() && LastHitBool);
        }

        public static Obj_AI_Base GetTarget()
        {
            Obj_AI_Base result;
            Obj_AI_Hero TempTarget = null;
            if (
                ObjectManager.Get<Obj_Building>()
                    .Any(
                        obj =>
                            obj.Position.Distance(Player.Position) <= GetRealAutoAttackRange() + obj.BoundingRadius / 2 &&
                            obj.IsTargetable && obj is Obj_HQ))
            {
                return null;
            }
            
            if (ActiveMode == OrbwalkingMode.Combo)
            {
                var AllValids = AllEnemies.Where(x => x.IsVisible && Orbwalking.InAutoAttackRange(x));
                if (TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget())
                {
                    TempTarget = TargetSelector.GetSelectedTarget();
                }
                else
                {
                    if (ImmuneCheck())
                    {
                        var tt = AllValids.Where(x => x.IsValid<Obj_AI_Hero>() && !myUtility.ImmuneToPhysical(x)).OrderByDescending(x => myRePriority.ResortDB(x.ChampionName)).ThenBy(i => i.Health / Player.GetAutoAttackDamage(i)).FirstOrDefault();
                        if (tt != null)
                        {
                            TempTarget = tt;
                        }
                    }
                    else
                    {
                        var t2 = AllValids.Where(x => x.IsValid<Obj_AI_Hero>()).OrderByDescending(x => myRePriority.ResortDB(x.ChampionName)).ThenBy(i => i.Health / Player.GetAutoAttackDamage(i)).FirstOrDefault();
                        if (t2 != null)
                        {
                            TempTarget = t2;
                        }
                    }
                }
                if (TempTarget != null && TempTarget.IsValidTarget())
                {
                    if (Player.IsMelee())
                    {
                        CustomPointMode(TempTarget);
                        return TempTarget;
                    }
                    return TempTarget;
                }
                TempTarget = null;
            }
            if (ActiveMode == OrbwalkingMode.Harass || ActiveMode == OrbwalkingMode.Hybrid)
            {
                var AllValids = AllEnemies.Where(x => x.IsVisible && Orbwalking.InAutoAttackRange(x));
                if (TargetSelector.GetSelectedTarget() != null && TargetSelector.GetSelectedTarget().IsValidTarget())
                {
                    TempTarget = TargetSelector.GetSelectedTarget();
                }
                else
                {
                    var t3 = AllValids.Where(x => x.IsValid<Obj_AI_Hero>()).OrderByDescending(x => myRePriority.ResortDB(x.ChampionName)).ThenBy(i => i.Health / Player.GetAutoAttackDamage(i)).FirstOrDefault();
                    if (t3 != null)
                    {
                        TempTarget = t3;
                    }                    
                }
                if (TempTarget != null && TempTarget.IsValidTarget())
                {
                    if (Player.UnderTurret(true) && TempTarget.UnderTurret(true))
                    {
                        return null;
                    }
                    return TempTarget;
                }
            }
            if (ActiveMode == OrbwalkingMode.Custom && CustomModeBool)
            {
                if (ForcedTarget != null && ForcedTarget.IsValidTarget())
                {
                    return ForcedTarget;
                }
                ForcedTarget = null;
            }
            if (ActiveMode == OrbwalkingMode.Lasthit || ActiveMode == OrbwalkingMode.LaneClear || ActiveMode == OrbwalkingMode.Hybrid ||                
                (ActiveMode == OrbwalkingMode.Harass && Menu.Item("Harass_Lasthit").GetValue<bool>()))
            {
                if (mySmiter.MapSupported && mySmiter.CanSmiteMinions)
                {
                    var superseige = myUtility.GetLargeMinions(GetRealAutoAttackRange()).OrderByDescending(i => i.MaxHealth).FirstOrDefault();
                    mySmiter.Smites(superseige);
                }
                foreach (var minion in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            minion =>
                                minion.IsValidTarget() && InFindRange(minion) &&
                                minion.Health < 
                                2 * 
                                (Player.BaseAttackDamage + Player.FlatPhysicalDamageMod)))
                {
                    var t = (int)(Player.AttackCastDelay * 1000) - 100 + Game.Ping / 2 + 1000 * (int)Vector3.Distance(Player.ServerPosition, minion.ServerPosition) / MyProjectileSpeed();
                    var predHealth = HealthPrediction.GetHealthPrediction(minion, (int)Math.Abs(t), FarmDelay);
                    if (minion.Team != GameObjectTeam.Neutral && MinionManager.IsMinion(minion, false))
                    {                        
                        if (predHealth < 0)
                        {
                            FireOnNonKillableMinion(minion);
                        }
                        if (IgnorePassive.Contains(Player.ChampionName))
                        {
                            if (predHealth > 0 && predHealth <= Player.GetAutoAttackDamage(minion))
                            {
                                return minion;
                            }
                        }
                        else
                        {
                            if (predHealth > 0 && predHealth <= Player.GetAutoAttackDamage(minion, true))
                            {
                                return minion;
                            }
                        }
                    }
                }
            }
            if (ActiveMode == OrbwalkingMode.JungleClear)
            {                
                var largemobs =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            x =>
                                x.Team == GameObjectTeam.Neutral &&
                                InFindRange(x) &&
                                x.IsValidTarget() &&
                                myUtility.LargeNeutral.Contains(x.BaseSkinName) && 
                                !(x.BaseSkinName.Contains("SRU_") && x.BaseSkinName.Contains("Mini")) &&
                                !(x.BaseSkinName.Contains("TT_") && x.BaseSkinName.Contains("2"))
                                ).MaxOrDefault(mob => mob.MaxHealth);
                var mobs =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                        x =>
                            x.Team == GameObjectTeam.Neutral &&
                            InFindRange(x) &&
                            x.IsValidTarget())
                        .MaxOrDefault(mob => mob.MaxHealth);
                if (largemobs != null && largemobs.IsValidTarget())
                {
                    if (mySmiter.MapSupported && mySmiter.CanSmiteMonster) mySmiter.Smites(largemobs);
                    if (IsMelee(Player) && !JungleMove) SetMovement(false);
                    return largemobs;
                }
                if (mobs != null && mobs.IsValidTarget())
                {
                    if (IsMelee(Player) && !JungleMove)
                    {
                        SetMovement(true);
                    }
                    return mobs;
                }
                if (IsMelee(Player) && !JungleMove)
                {
                    SetMovement(true);
                }
            }
            if (ActiveMode == OrbwalkingMode.LaneClear || ActiveMode == OrbwalkingMode.Hybrid)
            {
                foreach (var turret in
                    ObjectManager.Get<Obj_AI_Turret>()
                        .Where(turret => turret.IsEnemy && turret.IsValidTarget(GetRealAutoAttackRange(Player, turret)))
                    )
                {
                    return turret;
                }
            }
            if (ActiveMode == OrbwalkingMode.LaneClear || ActiveMode == OrbwalkingMode.Hybrid)
            {
                if (!ShouldWait())
                {
                    if (PrevMinion.IsValidTarget() && InFindRange(PrevMinion))
                    {
                        var predHealth = HealthPrediction.LaneClearHealthPrediction(
                            PrevMinion, (int)((Player.AttackDelay * 1000) * LaneClearWaitTimeMod), FarmDelay);
                        if (predHealth >= 2 * Player.GetAutoAttackDamage(PrevMinion) ||
                            Math.Abs(predHealth - PrevMinion.Health) < float.Epsilon)
                        {
                            return PrevMinion;
                        }
                    }
                    result = (from minion in
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(minion => minion.Team != GameObjectTeam.Neutral && minion.IsValidTarget() && InFindRange(minion))
                        let predHealth =
                            HealthPrediction.LaneClearHealthPrediction(
                                minion, (int) ((Player.AttackDelay * 1000) * LaneClearWaitTimeMod), FarmDelay)
                        where
                            predHealth >= 2 * Player.GetAutoAttackDamage(minion) ||
                            Math.Abs(predHealth - minion.Health) < float.Epsilon
                        select minion).MaxOrDefault(m => m.Health);

                    if (result != null)
                    {
                        PrevMinion = (Obj_AI_Minion)result;
                    }
                }
            }
            if (ShouldWait())
            {
                return null;
            }
            return null;
        }

        private static bool ShouldWait()
        {
            return
                ObjectManager.Get<Obj_AI_Minion>()
                    .Any(
                        minion =>
                            minion.IsValidTarget() && minion.Team != GameObjectTeam.Neutral && InFindRange(minion) &&
                            HealthPrediction.LaneClearHealthPrediction(
                                minion, (int) ((Player.AttackDelay * 1000) * LaneClearWaitTimeMod), FarmDelay) <=
                            Player.GetAutoAttackDamage(minion));
        }       

        public static bool IsWaiting()
        {
            return ShouldWait();
        }

        public static void ResetAutoAttackTimer()
        {
            LastAATick = 0;
        }

        private static bool ImmuneCheck()
        {
            return Menu.Item("ImmuneCheck").GetValue<bool>();
        }      

        private static int LastWindUpCheck;

        private static void CheckAutoWindUp()
        {
            if (!Menu.Item("Misc_AutoWindUp").GetValue<bool>())
            {
                return;
            }
            if (WindUpDuration > 0)
            {
                Menu.Item("Misc_ExtraWindUp").SetValue(WindUpDuration);
                Menu.Item("Misc_AutoWindUp").SetValue(false);
                return;
            }
            if (myUtility.TickCount - LastWindUpCheck > 10000)
            {
                var additional = 0;
                if (Game.Ping >= 100)
                {
                    additional = Game.Ping / 100 * 10;
                }
                else if (Game.Ping > 40 && Game.Ping < 100)
                {
                    additional = Game.Ping / 100 * 20;
                }
                else if (Game.Ping <= 40)
                {
                    additional = +10;
                }
                var windUp = Game.Ping + additional;
                if (windUp < 40)
                {
                    windUp = 40;
                }
                Menu.Item("Misc_ExtraWindUp").SetValue(windUp < 200 ? new Slider(windUp, 200, 0) : new Slider(200, 200, 0));
                LastWindUpCheck = myUtility.TickCount;
            }
        }

        public static int GetCurrentWindupTime()
        {
            return WindUpDuration > 0 ? WindUpDuration : Menu.Item("Misc_ExtraWindUp").GetValue<Slider>().Value;
        }
     
        public static void SetMovement(bool value)
        {
            MovementBool = value;
        }

        public static bool GetAttack()
        {
            return AttackBool;
        }

        public static void SetAttack(bool value)
        {
            AttackBool = value;
        }

        public static void SetBoth(bool value)
        {
            AttackBool = value;
            MovementBool = value;
        }

        public static void SetForcedTarget(Obj_AI_Hero target)
        {
            ForcedTarget = target;
        }

        public static void UnlockTarget()
        {
            ForcedTarget = null;
        }

        public static bool GetMovement()
        {
            return MovementBool;
        }

        public static void SetWindUp(int x)
        {
            WindUpDuration = x;
        }

        public static void SetResetDelay(int x)
        {
            ResetDelayDuration = x;
        }

        public static int GetWindUp()
        {
            return WindUpDuration > 0 ? WindUpDuration : Menu.Item("Misc_ExtraWindUp").GetValue<Slider>().Value;
        }

        public static int GetResetDelay()
        {
            return ResetDelayDuration > 0 ? ResetDelayDuration : Menu.Item("Misc_AAReset").GetValue<Slider>().Value;
        }

        public static void CustomPointReset()
        {
            if (Player.IsMelee() && CustomPoint != new Vector3())
            {
                Menu.Item("Combo_move").SetValue(true);
                CustomPoint = new Vector3();
            }
        }

        private static void CustomPointMode(Obj_AI_Hero target)
        {

            if (ComboMagnet && CanMove() &&
                target != null &&
                target.IsValidTarget() &&
                !target.IsDead &&
                !target.IsZombie &&
                !target.IsInvulnerable &&
                Vector3.Distance(Game.CursorPos, target.ServerPosition) < 500f &&
                Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) < 500f &&
                myUtility.IsFacing(Player, Game.CursorPos) && myUtility.IsFacing(Player, target.ServerPosition) 
                )
            {
                Menu.Item("Combo_move").SetValue(false);
                if (target.IsMoving)
                {
                    if (Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) >
                        Vector3.Distance(ObjectManager.Player.Position.Shorten(target.Position, 10f), target.ServerPosition))
                    {
                        CustomPoint = target.Position;
                    }
                    else
                    {
                        CustomPoint = ObjectManager.Player.Position.Shorten(target.Position, 10f);
                    }
                }
                else if (Vector3.Distance(ObjectManager.Player.ServerPosition, target.ServerPosition) > 10f)
                {
                    CustomPoint = target.Position;
                }
                else
                {
                    CustomPoint = ObjectManager.Player.Position.Shorten(target.Position, 10f);
                }
                return;
            }
            if (ActiveMode == OrbwalkingMode.Combo && (target == null || !target.IsValidTarget() || target.IsDead || target.IsZombie || target.IsInvulnerable))
            {
                CustomPointReset();
            }
            CustomPointReset();
        }
        public static bool Active()
        {
            return ActiveMode == OrbwalkingMode.Combo ||
                ActiveMode == OrbwalkingMode.Custom ||
                ActiveMode == OrbwalkingMode.Harass ||
                ActiveMode == OrbwalkingMode.Hybrid ||
                ActiveMode == OrbwalkingMode.JungleClear ||
                ActiveMode == OrbwalkingMode.LaneClear ||
                ActiveMode == OrbwalkingMode.Lasthit ||
                ActiveMode == OrbwalkingMode.Flee;
        }
    }
}
