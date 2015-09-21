#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.Threading.Tasks;
using LeagueSharp.Common.Data;
using System.Text;
using SharpDX;
using Color = System.Drawing.Color;
using Collision = LeagueSharp.Common.Collision;

#endregion

namespace LeeSin
{
    class Program
    {
        private const string ChampionName = "LeeSin";
        private static List<Spell> SpellList = new List<Spell>();
        private static Spell _q, _w, _e, _r;
        private static Orbwalking.Orbwalker _orbwalker;
        private static Menu _config;
        public static Menu TargetSelectorMenu;
        private static Obj_AI_Hero _player;
        private static Obj_AI_Base insobj;
        private static SpellSlot _igniteSlot;
        private static SpellSlot _flashSlot;
        private static SpellSlot _smitedmgSlot;
        private static SpellSlot _smitehpSlot;
        private static Items.Item _tiamat, _hydra, _blade, _bilge, _rand, _lotis, _youmuu; 
        public static Vector3 WardCastPosition;
        private static Vector3 insdirec;
        private static Vector3 insecpos;
        private static Vector3 movepoint;
        private static Vector3 jumppoint;
        private static Vector3 wpos;
        private static Vector3 wallcheck;
        private static Vector3 firstpos;
        private static int canmove=1;
        private static int instypecheck;
        private static float inscount;
        private static float counttime;
        private static float qcasttime;
        private static float q2casttime;
        private static float wcasttime;
        private static float ecasttime;
        private static float casttime;
        private static bool walljump;
        private static bool checker;
        private static int bCount;
 


        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                _player = ObjectManager.Player;
                if (ObjectManager.Player.BaseSkinName != ChampionName) return;
               
                
                _q = new Spell(SpellSlot.Q, 1100f);
                _w = new Spell(SpellSlot.W, 700f);
                _e = new Spell(SpellSlot.E, 330f);
                _r = new Spell(SpellSlot.R, 375f);

                _q.SetSkillshot(0.25f, 65f, 1800f, true, SkillshotType.SkillshotLine);

                _bilge = new Items.Item(3144, 475f);
                _blade = new Items.Item(3153, 425f);
                _hydra = new Items.Item(3074, 250f);
                _tiamat = new Items.Item(3077, 250f);
                _rand = new Items.Item(3143, 490f);
                _lotis = new Items.Item(3190, 590f);
                _youmuu = new Items.Item(3142, 10);
                
                _igniteSlot = _player.GetSpellSlot("SummonerDot");
                _flashSlot = _player.GetSpellSlot("SummonerFlash");
                _smitedmgSlot = _player.GetSpellSlot(SmitetypeDmg());
                _smitehpSlot = _player.GetSpellSlot(SmitetypeHp());




                _config = new Menu("Lee Is Back", "Lee Is Back", true);

                TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
                TargetSelector.AddToMenu(TargetSelectorMenu);
                _config.AddSubMenu(TargetSelectorMenu);

                _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
                _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));


                _config.AddSubMenu(new Menu("Combo", "Combo"));
                _config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
                _config.SubMenu("Combo").AddItem(new MenuItem("UseIgnitecombo", "Use Ignite(rush for it)")).SetValue(true);
                _config.SubMenu("Combo").AddItem(new MenuItem("UseSmitecombo", "Use Smite(rush for it)")).SetValue(true);
                _config.SubMenu("Combo").AddItem(new MenuItem("UseWcombo", "Use W")).SetValue(false);
                
                _config.AddSubMenu(new Menu("Insec", "Insec"));
                _config.SubMenu("Insec").AddItem(new MenuItem("insc", "insec active")).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press));
                _config.SubMenu("Insec").AddItem(new MenuItem("minins", "insec using minions?")).SetValue(false);
                _config.SubMenu("Insec").AddItem(new MenuItem("fins", "flash if no wards")).SetValue(true);

                _config.AddSubMenu(new Menu("Harass", "Harass"));
                _config.SubMenu("Harass")
                    .AddItem(
                        new MenuItem("ActiveHarass", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                _config.SubMenu("Harass").AddItem(new MenuItem("UseItemsharass", "Use Tiamat/Hydra")).SetValue(true);
                _config.SubMenu("Harass").AddItem(new MenuItem("UseEHar", "Use E")).SetValue(true);
                _config.SubMenu("Harass").AddItem(new MenuItem("UseQ1Har", "Use Q1 Harass")).SetValue(true);
                _config.SubMenu("Harass").AddItem(new MenuItem("UseQ2Har", "Use Q2 Harass")).SetValue(true);


                _config.AddSubMenu(new Menu("items", "items"));
                _config.SubMenu("items").AddSubMenu(new Menu("Offensive", "Offensive"));
                _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Youmuu", "Use Youmuu's")).SetValue(true);
                _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Tiamat", "Use Tiamat")).SetValue(true);
                _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Hydra", "Use Hydra")).SetValue(true);
                _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Bilge", "Use Bilge")).SetValue(true);
                _config.SubMenu("items")
                    .SubMenu("Offensive")
                    .AddItem(new MenuItem("BilgeEnemyhp", "If Enemy Hp <").SetValue(new Slider(85, 1, 100)));
                _config.SubMenu("items")
                    .SubMenu("Offensive")
                    .AddItem(new MenuItem("Bilgemyhp", "Or your Hp < ").SetValue(new Slider(85, 1, 100)));
                _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Blade", "Use Blade")).SetValue(true);
                _config.SubMenu("items")
                    .SubMenu("Offensive")
                    .AddItem(new MenuItem("BladeEnemyhp", "If Enemy Hp <").SetValue(new Slider(85, 1, 100)));
                _config.SubMenu("items")
                    .SubMenu("Offensive")
                    .AddItem(new MenuItem("Blademyhp", "Or Your  Hp <").SetValue(new Slider(85, 1, 100)));
                _config.SubMenu("items").AddSubMenu(new Menu("Deffensive", "Deffensive"));
                _config.SubMenu("items")
                    .SubMenu("Deffensive")
                    .AddItem(new MenuItem("Omen", "Use Randuin Omen"))
                    .SetValue(true);
                _config.SubMenu("items")
                    .SubMenu("Deffensive")
                    .AddItem(new MenuItem("Omenenemys", "Randuin if enemys>").SetValue(new Slider(2, 1, 5)));
                _config.SubMenu("items")
                    .SubMenu("Deffensive")
                    .AddItem(new MenuItem("lotis", "Use Iron Solari"))
                    .SetValue(true);
                _config.SubMenu("items")
                    .SubMenu("Deffensive")
                    .AddItem(new MenuItem("lotisminhp", "Solari if Ally Hp<").SetValue(new Slider(35, 1, 100)));

                //Farm
                _config.AddSubMenu(new Menu("Farm", "Farm"));
                _config.SubMenu("Farm").AddSubMenu(new Menu("LaneFarm", "LaneFarm"));
                _config.SubMenu("Farm")
                    .SubMenu("LaneFarm")
                    .AddItem(new MenuItem("UseItemslane", "Use Hydra/Tiamat"))
                    .SetValue(true);
                _config.SubMenu("Farm").SubMenu("LaneFarm").AddItem(new MenuItem("UseQL", "Q LaneClear")).SetValue(true);
                _config.SubMenu("Farm").SubMenu("LaneFarm").AddItem(new MenuItem("UseEL", "E LaneClear")).SetValue(true);
                _config.SubMenu("Farm")
                    .SubMenu("LaneFarm")
                    .AddItem(
                        new MenuItem("Activelane", "Lane clear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

                _config.SubMenu("Farm").AddSubMenu(new Menu("LastHit", "LastHit"));
                _config.SubMenu("Farm").SubMenu("LastHit").AddItem(new MenuItem("UseQLH", "Q LastHit")).SetValue(true);
                _config.SubMenu("Farm")
                    .SubMenu("LastHit")
                    .AddItem(
                        new MenuItem("Activelast", "LastHit!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

                _config.SubMenu("Farm").AddSubMenu(new Menu("Jungle", "Jungle"));
                _config.SubMenu("Farm")
                    .SubMenu("Jungle")
                    .AddItem(new MenuItem("UseItemsjungle", "Use Hydra/Tiamat"))
                    .SetValue(true);
                _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("UseQJ", "Q Jungle")).SetValue(true);
                _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("UseWJ", "W Jungle")).SetValue(true);
                _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("UseEJ", "E Jungle")).SetValue(true);
                _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("PriW", "W>E? (off E>W)")).SetValue(true);
                _config.SubMenu("Farm")
                    .SubMenu("Jungle")
                    .AddItem(
                        new MenuItem("Activejungle", "Jungle!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

                //Misc
                _config.AddSubMenu(new Menu("Misc", "Misc"));
                _config.SubMenu("Misc").AddItem(new MenuItem("UseIgnitekill", "Use Ignite KillSteal")).SetValue(true);
                _config.SubMenu("Misc").AddItem(new MenuItem("UseEM", "Use E KillSteal")).SetValue(true);
                _config.SubMenu("Misc").AddItem(new MenuItem("wjump", "ward jump")).SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press));
                _config.SubMenu("Misc").AddItem(new MenuItem("wjmax", "ward jump max range?")).SetValue(false);





                //Drawings
                _config.AddSubMenu(new Menu("Drawings", "Drawings"));
                _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
                _config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
                _config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
                _config.SubMenu("Drawings").AddItem(new MenuItem("damagetest", "Damage Text")).SetValue(true);
                _config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
                _config.SubMenu("Drawings")
                    .AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
                _config.SubMenu("Drawings")
                    .AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));
                _config.AddToMainMenu();
                new AssassinManager();
                new DamageIndicator();

                DamageIndicator.DamageToUnit = ComboDamage;

                Drawing.OnDraw += Drawing_OnDraw;
                Game.OnUpdate += Game_OnUpdate;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
                Game.OnWndProc += OnWndProc;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Game.PrintChat("Error something went wrong");
            }
        }
   
                    
        private static void Game_OnUpdate(EventArgs args)
        {

            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {          
                Combo(GetEnemy);
                
            }
            if (_config.Item("wjump").GetValue<KeyBind>().Active)
            {
                wjumpflee();
            }
            if (_config.Item("ActiveHarass").GetValue<KeyBind>().Active)
            {
                Harass(GetEnemy);

            }
            if (_config.Item("insc").GetValue<KeyBind>().Active)
            {
                Insec(GetEnemy);

            }
            if (_config.Item("Activejungle").GetValue<KeyBind>().Active)
            {
                JungleClear();
            }
            if (_config.Item("Activelane").GetValue<KeyBind>().Active)
            {
                LaneClear();
            }
            if (_config.Item("Activelast").GetValue<KeyBind>().Active)
            {
                LastHit();
            }
            

            
 

        }

        private static void OnWndProc(WndEventArgs args)
        {
            if (args.Msg == 515 || args.Msg == 513)
            {
                if (args.Msg == 515)
                {
                    insdirec = Game.CursorPos;
                    instypecheck = 1;
                }
                var boohoo = ObjectManager.Get<Obj_AI_Base>()
                         .OrderBy(obj => obj.Distance(_player.ServerPosition))
                         .FirstOrDefault(
                             obj =>
                                 obj.IsAlly && !obj.IsMe && !obj.IsMinion &&
                                  Game.CursorPos.Distance(obj.ServerPosition) <= 150);

                if (args.Msg == 513 && boohoo != null)
                {
                    insobj = boohoo;
                    instypecheck = 2;
                }
            }

        }

        private static void OnProcessSpell (Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                casttime = Environment.TickCount;
            }
         
            if (sender.IsAlly || !sender.Type.Equals(GameObjectType.obj_AI_Hero) ||
                (((Obj_AI_Hero)sender).ChampionName != "MonkeyKing" && ((Obj_AI_Hero)sender).ChampionName != "Akali") ||
                sender.Position.Distance(_player.ServerPosition) >= 330  ||
                !_e.IsReady())
            {
                return;
            }
            if (args.SData.Name == "MonkeyKingDecoy" || args.SData.Name == "AkaliSmokeBomb")
            {
                _e.Cast();
            }
        }
        private static float ComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            if (_igniteSlot != SpellSlot.Unknown &&_player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            if (_smitedmgSlot != SpellSlot.Unknown && _player.Spellbook.CanUseSpell(_smitedmgSlot) == SpellState.Ready)
                damage += 20 + 8 * _player.Level;
            if (Items.HasItem(3077) && Items.CanUseItem(3077))
                damage += _player.GetItemDamage(enemy, Damage.DamageItems.Tiamat);
            if (Items.HasItem(3074) && Items.CanUseItem(3074))
                damage += _player.GetItemDamage(enemy, Damage.DamageItems.Hydra);
            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += _player.GetItemDamage(enemy, Damage.DamageItems.Botrk);
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                damage += _player.GetItemDamage(enemy, Damage.DamageItems.Bilgewater);
            if (QStage == QCastStage.First)
                damage += _player.GetSpellDamage(enemy, SpellSlot.Q)*2;
            if (EStage == ECastStage.First)
                damage += _player.GetSpellDamage(enemy, SpellSlot.E);
            if (_r.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.R);

            return (float)damage;
        }
        private static bool Passive()
        {
            if (_player.HasBuff("blindmonkpassive_cosmetic"))
            {
                return true;
            }
            else
                return false;
        }

        private static void Combo(Obj_AI_Hero t)
        {


            if (_config.Item("UseIgnitecombo").GetValue<bool>() && _igniteSlot != SpellSlot.Unknown &&
                _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
            {
                if (ComboDamage(t) > t.Health)
                {
                    _player.Spellbook.CastSpell(_igniteSlot, t);
                }
            }
            if (_config.Item("UseWcombo").GetValue<bool>() && t.Distance(_player.Position) <= Orbwalking.GetRealAutoAttackRange(_player))
            {
                if (WStage == WCastStage.First || !Passive())
                    CastSelfW();
                if (WStage == WCastStage.Second && (!Passive() || Environment.TickCount > wcasttime + 2500))
                    _w.Cast();
            }
            if (_config.Item("UseSmitecombo").GetValue<bool>() && _smitedmgSlot != SpellSlot.Unknown &&
                _player.Spellbook.CanUseSpell(_smitedmgSlot) == SpellState.Ready)
            {
                if (ComboDamage(t) > t.Health)
                {
                    _player.Spellbook.CastSpell(_smitedmgSlot, t);
                }
            }
            if (ComboDamage(t) > t.Health && t.Distance(_player.Position) < 950 && _r.IsReady())
            {
                if (t.Distance(_player.Position) > 500)
                    WardJump(t.Position, true, true, true);
                if (t.Distance(_player.Position) <= 375 && (t.HasBuff("BlindMonkQOne") || t.HasBuff("blindmonkqonechaos") || _player.GetSpellDamage(t, SpellSlot.R) > t.Health))
                    _r.CastOnUnit(t);
            }

            if (t.IsValidTarget() && _q.IsReady())
            {
                CastQ1(t);
                if (t.HasBuff("BlindMonkQOne") || t.HasBuff("blindmonkqonechaos") && (ComboDamage(t) > t.Health || t.Distance(_player.Position) > 350 || Environment.TickCount > qcasttime + 2500))
                    _q.Cast();
            }

            CastECombo();
            UseItemes(t);

        }
        private static void Harass(Obj_AI_Hero t)
        {
            if (t == null) return;

            var jumpObject =ObjectManager.Get<Obj_AI_Base>()
                .OrderBy(obj => obj.Distance(firstpos))
                .FirstOrDefault(obj =>
                    obj.IsAlly && !obj.IsMe &&
                    !(obj.Name.IndexOf("turret", StringComparison.InvariantCultureIgnoreCase) >= 0) &&
                    obj.Distance(t.Position)< 550);
         
            if (_config.Item("UseEHar").GetValue<bool>())
                CastECombo();
            if (_config.Item("UseQ1Har").GetValue<bool>())
                CastQ1(t);
            if (_config.Item("UseQ2Har").GetValue<bool>() && (t.HasBuff("BlindMonkQOne") || t.HasBuff("blindmonkqonechaos")) && jumpObject != null&& WStage==WCastStage.First)
            {
                _q.Cast();
                q2casttime = Environment.TickCount;
            }
            if (_player.Distance(t.Position) < 300 && !_q.IsReady()&& q2casttime+2500>Environment.TickCount&&Environment.TickCount>q2casttime+500)
                CastW(jumpObject);

            var useItemsH = _config.Item("UseItemsharass").GetValue<bool>();

            if (useItemsH && _tiamat.IsReady() && t.Distance(_player.Position) < _tiamat.Range)
            {
                _tiamat.Cast();
            }
            if (useItemsH && _hydra.IsReady() && t.Distance(_player.Position) < _hydra.Range)
            {
                _hydra.Cast();
            }

        }

        public static void WardJump(Vector3 pos, bool useWard = true, bool checkObjects = true, bool fullRange = false)
        {
            if (WStage!= WCastStage.First)
            {
                return;
            }
            pos = fullRange ? _player.ServerPosition.To2D().Extend(pos.To2D(), 600).To3D() : pos;
            WardCastPosition = NavMesh.GetCollisionFlags(pos).HasFlag(CollisionFlags.Wall)
                ? _player.GetPath(pos).Last()
                : pos;
            var jumpObject =
                ObjectManager.Get<Obj_AI_Base>()
                    .OrderBy(obj => obj.Distance(_player.ServerPosition))
                    .FirstOrDefault(
                        obj =>
                            obj.IsAlly && !obj.IsMe &&
                            (!(obj.Name.IndexOf("turret", StringComparison.InvariantCultureIgnoreCase) >= 0) &&
                             Vector3.DistanceSquared(pos, obj.ServerPosition) <= 150 * 150));
            if (jumpObject != null && checkObjects && WStage == WCastStage.First)
            {
                CastW(jumpObject);
                return;
            }
            if (!useWard)
            {
                return;
            }

            if (Items.GetWardSlot() == null || Items.GetWardSlot().Stacks == 0)
            {
                return;
            }
            placeward(WardCastPosition);
        }
         private static void placeward(Vector3 castpos)
        {
            if (WStage != WCastStage.First)
            {
                return;
            }
            var ward = Items.GetWardSlot();
            _player.Spellbook.CastSpell(ward.SpellSlot, castpos);
          
            
        }

        private static void wjumpflee()
        {
            if (WStage != WCastStage.First)
                _player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            else
            {
                if (_config.Item("wjmax").GetValue<bool>())
                {
                    _player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                    WardJump(Game.CursorPos, true, true, true);
                }
                else if (_player.Distance(Game.CursorPos) >= 700 || walljump == true)
                {

                    if (Game.CursorPos.Distance(wallcheck) > 50)
                    {
                        walljump = false;
                        checker=false;
                        for (var i = 0; i < 40; i++)
                        {
                            var p = Game.CursorPos.Extend(_player.Position, 10 * i);
                            if (NavMesh.GetCollisionFlags(p).HasFlag(CollisionFlags.Wall))
                            {
                                jumppoint = p;
                                wallcheck = Game.CursorPos;
                                walljump = true;
                                break;


                            }
                        }


                        if (walljump == true)
                        {
                            foreach (
                              var qPosition in
                                GetPossibleJumpPositions(jumppoint)
                                .OrderBy(qPosition => qPosition.Distance(jumppoint)))
                                {
                                if (_player.Position.Distance(qPosition) < _player.Position.Distance(jumppoint))
                                {
                                    movepoint = qPosition;
                                    if (movepoint.Distance(jumppoint) > 600)
                                        wpos = movepoint.Extend(jumppoint, 595);
                                    else
                                        wpos = jumppoint;

                                    break;
                                }
                                if (qPosition == null)
                                    movepoint = jumppoint;
                                checker = true;
                                break;
                            }


                        }
                    }
                    var jumpObj = ObjectManager.Get<Obj_AI_Base>()
                         .OrderBy(obj => obj.Distance(_player.ServerPosition))
                         .FirstOrDefault(obj => obj.IsAlly && !obj.IsMe && obj.Distance(movepoint) <= 700 &&
                             (!(obj.Name.IndexOf("turret", StringComparison.InvariantCultureIgnoreCase) >= 0) &&
                             obj.Distance(jumppoint) <= 200));



                    if (walljump == false || movepoint.Distance(Game.CursorPos) > _player.Distance(Game.CursorPos) + 150)
                    {
                        movepoint = Game.CursorPos;
                        jumppoint = Game.CursorPos;
                        
                    }
                    if (jumpObj == null && Items.GetWardSlot() != null && Items.GetWardSlot().Stacks != 0)
                        placeward(wpos);
                    if (_player.Position.Distance(jumppoint) <= 700 && jumpObj != null)
                    {
                        CastW(jumpObj);
                        walljump = false;
                    }


                    _player.IssueOrder(GameObjectOrder.MoveTo, movepoint);
                }
                else
                    WardJump(jumppoint, true, true, false);

            }

        }

        private static IEnumerable<Vector3> GetPossibleJumpPositions(Vector3 pos)
        {
            var pointList = new List<Vector3>();

            for (var j = 680; j >= 50; j -= 50)
            {
                var offset = (int)(2 * Math.PI * j / 50);

                for (var i = 0; i <= offset; i++)
                {
                    var angle = i * Math.PI * 2 / offset;
                    var point = new Vector3((float)(pos.X + j * Math.Cos(angle)),
                        (float)(pos.Y - j * Math.Sin(angle)),
                        pos.Z);

                    if (!NavMesh.GetCollisionFlags(point).HasFlag(CollisionFlags.Wall)&&point.Distance(_player.Position)<pos.Distance(_player.Position)-400&&
                        point.Distance(pos.Extend(_player.Position, 600)) <= 250)
                        pointList.Add(point);
                }
            }

            return pointList;
        }
        private static void Insec (Obj_AI_Hero t)
         {
             if (insobj != null&& instypecheck==2)
                 insdirec = insobj.Position;
             if (canmove==1)
             {
                 _player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
             }

             if (!_r.IsReady() ||((Items.GetWardSlot() == null || Items.GetWardSlot().Stacks == 0 || WStage != WCastStage.First) && _player.Spellbook.CanUseSpell(_flashSlot) == SpellState.Cooldown))
             {
                 canmove = 1;
                 return;
             }


            insecpos = t.ServerPosition.Extend(insdirec, -300);
            if ((_player.ServerPosition.Distance(insecpos) > 600 || inscount + 500 > Environment.TickCount) && t != null && t.IsValidTarget() && QStage == QCastStage.First)
            {
                var qpred = _q.GetPrediction(t);
                if (qpred.Hitchance >= HitChance.Medium)
                _q.Cast(t);
                if (qpred.Hitchance == HitChance.Collision && _config.Item("minins").GetValue<bool>())
                {
                    var enemyqtry = ObjectManager.Get<Obj_AI_Base>().Where(enemyq => (enemyq.IsValidTarget()||(enemyq.IsMinion&&enemyq.IsEnemy)) && enemyq.Distance(insecpos)<500);
                    foreach( 
                        var enemyhit in enemyqtry.OrderBy(enemyhit=>enemyhit.Distance(insecpos)))
                        {

                            if (_q.GetPrediction(enemyhit).Hitchance >= HitChance.Medium && enemyhit.Distance(insecpos) < 500 && _player.GetSpellDamage(enemyhit, SpellSlot.Q)<enemyhit.Health)
                            _q.Cast(enemyhit);
                    }
                }
            }
            if (QStage == QCastStage.Second)
            {
                var enemy = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(unit => unit.IsEnemy && (unit.HasBuff("BlindMonkQOne") || unit.HasBuff("blindmonkqonechaos")));
                if (enemy.Position.Distance(insecpos) < 550)
                {
                    _q.Cast();
                    canmove = 0;
                }
            }

            if (_player.Position.Distance(insecpos) < 600)
            {
                if ((Items.GetWardSlot() == null || Items.GetWardSlot().Stacks == 0 || WStage != WCastStage.First) && _config.Item("fins").GetValue<bool>()&&
                    _player.Spellbook.CanUseSpell(_flashSlot) == SpellState.Ready&&_player.Position.Distance(t.Position)<_r.Range&& Environment.TickCount>counttime+3000)
                {
                    _r.CastOnUnit(t);
                    Utility.DelayAction.Add(Game.Ping + 125, () => _player.Spellbook.CastSpell(_flashSlot, insecpos));
                    canmove = 0;
                }
                else
                WardJump(insecpos, true, true, false);
                counttime = Environment.TickCount;
                canmove = 0;
            }
            if (t.ServerPosition.Distance(insdirec)+100 < _player.Position.Distance(insdirec) && WStage != WCastStage.First)
            {
                _r.CastOnUnit(t);
                inscount = Environment.TickCount;
                canmove = 1;
            }

        }
       
        static Obj_AI_Hero GetEnemy
        {
            get
            {
                var assassinRange = TargetSelectorMenu.Item("AssassinSearchRange").GetValue<Slider>().Value;

                var vEnemy = ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        enemy =>
                            enemy.Team != ObjectManager.Player.Team && !enemy.IsDead && enemy.IsVisible &&
                            TargetSelectorMenu.Item("Assassin" + enemy.ChampionName) != null &&
                            TargetSelectorMenu.Item("Assassin" + enemy.ChampionName).GetValue<bool>() &&
                            ObjectManager.Player.Distance(enemy.ServerPosition) < assassinRange);

                if (TargetSelectorMenu.Item("AssassinSelectOption").GetValue<StringList>().SelectedIndex == 1)
                {
                    vEnemy = (from vEn in vEnemy select vEn).OrderByDescending(vEn => vEn.MaxHealth);
                }

                Obj_AI_Hero[] objAiHeroes = vEnemy as Obj_AI_Hero[] ?? vEnemy.ToArray();

                Obj_AI_Hero t = !objAiHeroes.Any()
                    ? TargetSelector.GetTarget(1400, TargetSelector.DamageType.Magical)
                    : objAiHeroes[0];

                return t;

            }

        }

        private static QCastStage QStage
        {
            get
            {
                if (!_q.IsReady()) return QCastStage.Cooldown;

                return (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name == "BlindMonkQOne"
                    ? QCastStage.First
                    : QCastStage.Second);

            }
        }
         private static ECastStage EStage
        {
            get
            {
                if (!_e.IsReady()) return ECastStage.Cooldown;

                return (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Name == "BlindMonkEOne"
                    ? ECastStage.First
                    : ECastStage.Second);

            }
        }
        private static WCastStage  WStage
        {
            get
            {
                if (!_w.IsReady()) return WCastStage.Cooldown;

                return (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "blindmonkwtwo"
                    ? WCastStage.Second
                    : WCastStage.First);

            }
        }


        internal enum QCastStage
        {
            First,
            Second,
            Cooldown
        }

        internal enum ECastStage
        {
            First,
            Second,
            Cooldown
        }

        internal enum WCastStage
        {
            First,
            Second,
            Cooldown
        }
        private static void CastSelfW()
        {
            if (500 >= Environment.TickCount - wcasttime || WStage != WCastStage.First ) return;

                _w.Cast();
            wcasttime=Environment.TickCount;
            
        }
        private static void CastW(Obj_AI_Base obj)
        {
            if (500 >= Environment.TickCount - wcasttime || WStage != WCastStage.First) return;

            _w.CastOnUnit(obj);
            wcasttime = Environment.TickCount;

        }

        private static void CastECombo()
        {
            if (!_e.IsReady()) return;
            if (ObjectManager.Get<Obj_AI_Hero>()
                .Count(
                    hero =>
                        hero.IsValidTarget() &&
                        hero.Distance(ObjectManager.Player.ServerPosition) <= _e.Range)> 0)
            {
                CastE1();
            }
            if (EStage == ECastStage.Second && ((Environment.TickCount > casttime + 200 && !Passive()) || Environment.TickCount > ecasttime + 2700))
                _e.Cast();
        }
        private static void CastE1()
        {
            if (500 >= Environment.TickCount - ecasttime || EStage != ECastStage.First) return;
            _e.Cast();
            ecasttime = Environment.TickCount;
        }

        private static void CastQ1(Obj_AI_Base target)
        {
            if (QStage != QCastStage.First) return;
            var qpred = _q.GetPrediction(target);
            if (qpred.Hitchance >= HitChance.Medium && qpred.CastPosition.Distance(_player.ServerPosition) < 1100)
            {
                _q.Cast(target);
                firstpos = _player.Position;
                qcasttime = Environment.TickCount;
            }
        }

        private static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        private static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        private static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        private static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };

        private static string SmitetypeDmg()
        {
            if (SmiteBlue.Any(a => Items.HasItem(a)))
            {
                return "s5_summonersmiteplayerganker";
            }
            if (SmiteRed.Any(a => Items.HasItem(a)))
            {
                return "s5_summonersmiteduel";

            }
            return "summonersmite";
        }
        private static string SmitetypeHp()
        {
            if (SmitePurple.Any(a => Items.HasItem(a)))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }
        private static void UseItemes(Obj_AI_Hero target)
        {
            var iBilge = _config.Item("Bilge").GetValue<bool>();
            var iBilgeEnemyhp = target.Health <=
                                (target.MaxHealth * (_config.Item("BilgeEnemyhp").GetValue<Slider>().Value) / 100);
            var iBilgemyhp = _player.Health <=
                             (_player.MaxHealth * (_config.Item("Bilgemyhp").GetValue<Slider>().Value) / 100);
            var iBlade = _config.Item("Blade").GetValue<bool>();
            var iBladeEnemyhp = target.Health <=
                                (target.MaxHealth * (_config.Item("BladeEnemyhp").GetValue<Slider>().Value) / 100);
            var iBlademyhp = _player.Health <=
                             (_player.MaxHealth * (_config.Item("Blademyhp").GetValue<Slider>().Value) / 100);
            var iOmen = _config.Item("Omen").GetValue<bool>();
            var iOmenenemys = ObjectManager.Get<Obj_AI_Hero>().Count(hero => hero.IsValidTarget(450)) >=
                              _config.Item("Omenenemys").GetValue<Slider>().Value;
            var iTiamat = _config.Item("Tiamat").GetValue<bool>();
            var iHydra = _config.Item("Hydra").GetValue<bool>();
            var ilotis = _config.Item("lotis").GetValue<bool>();
            var iYoumuu = _config.Item("Youmuu").GetValue<bool>();


            if (_player.Distance(target.ServerPosition) <= 450 && iBilge && (iBilgeEnemyhp || iBilgemyhp) && _bilge.IsReady())
            {
                _bilge.Cast(target);

            }
            if (_player.Distance(target.ServerPosition) <= 450 && iBlade && (iBladeEnemyhp || iBlademyhp) && _blade.IsReady())
            {
                _blade.Cast(target);

            }
            if (_player.Distance(target.ServerPosition) <= 300 && iTiamat && _tiamat.IsReady())
            {
                _tiamat.Cast();

            }
            if (_player.Distance(target.ServerPosition) <= 300 && iHydra && _hydra.IsReady())
            {
                _hydra.Cast();

            }
            if (iOmenenemys && iOmen && _rand.IsReady())
            {
                _rand.Cast();

            }
            if (ilotis)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly || hero.IsMe))
                {
                    if (hero.Health <= (hero.MaxHealth * (_config.Item("lotisminhp").GetValue<Slider>().Value) / 100) &&
                        hero.Distance(_player.ServerPosition) <= _lotis.Range && _lotis.IsReady())
                        _lotis.Cast();
                }
            }
            if (_player.Distance(target.ServerPosition) <= 350 && iYoumuu && _youmuu.IsReady())
            {
                _youmuu.Cast();

            }
        }

        private static void LaneClear()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range);
            var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _e.Range);
            var useItemsl = _config.Item("UseItemslane").GetValue<bool>();
            var useQl = _config.Item("UseQL").GetValue<bool>();
            var useEl = _config.Item("UseEL").GetValue<bool>();
            if (allMinionsQ.Count == 0)
                return;
            if (EStage == ECastStage.Second && ((Environment.TickCount > casttime + 200 && !Passive()) || Environment.TickCount > ecasttime + 2700))
                _e.Cast();
            if (QStage == QCastStage.Second && (Environment.TickCount > qcasttime + 2700 || Environment.TickCount > casttime + 200 && !Passive()))
                _q.Cast();

            foreach (var minion in allMinionsQ)
            {
                if (!Orbwalking.InAutoAttackRange(minion) &&useQl&&
                    minion.Health < _player.GetSpellDamage(minion, SpellSlot.Q)*0.70)
                    _q.Cast(minion);
                else if (Orbwalking.InAutoAttackRange(minion) && useQl&&
                    minion.Health > _player.GetSpellDamage(minion, SpellSlot.Q) * 2)
                    CastQ1(minion);
            }
             
            

            if (_e.IsReady() && useEl)
            {
                if (allMinionsE.Count > 2)
                {
                    CastE1();
                }
                else
                    foreach (var minion in allMinionsE)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                            minion.Health < 0.90 * _player.GetSpellDamage(minion, SpellSlot.E))
                            CastE1();
            }
            if (useItemsl && _tiamat.IsReady() && allMinionsE.Count > 2)
            {
                _tiamat.Cast();
            }
            if (useItemsl && _hydra.IsReady() && allMinionsE.Count > 2)
            {
                _hydra.Cast();
            }
        }

        private static void LastHit()
        {
            var allMinionsQ = MinionManager.GetMinions(_player.ServerPosition, _q.Range, MinionTypes.All);
            var useQ = _config.Item("UseQLH").GetValue<bool>();
            foreach (var minion in allMinionsQ)
            {
                if (QStage == QCastStage.First && useQ &&_player.Distance(minion.ServerPosition) < _q.Range &&
                    minion.Health < 0.90 * _player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    CastQ1(minion);
                }
            }
        }
        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var useItemsJ = _config.Item("UseItemsjungle").GetValue<bool>();
            var useQ = _config.Item("UseQJ").GetValue<bool>();
            var useW = _config.Item("UseWJ").GetValue<bool>();
            var useE = _config.Item("UseEJ").GetValue<bool>();
            

            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                    
                if (useItemsJ && _tiamat.IsReady() && _player.Distance(mob.ServerPosition) < _tiamat.Range)
                {
                    _tiamat.Cast();
                }
                if (useItemsJ && _hydra.IsReady() && _player.Distance(mob.ServerPosition) < _hydra.Range)
                {
                    _hydra.Cast();
                }
                if (QStage == QCastStage.Second && (mob.Health < _q.GetDamage(mob) && ((mob.HasBuff("BlindMonkQOne") || mob.HasBuff("blindmonkqonechaos"))) || Environment.TickCount > qcasttime + 2700 || ((Environment.TickCount > casttime + 200 && !Passive()))))
                    _q.Cast();
                if (WStage == WCastStage.Second && ((Environment.TickCount > casttime + 200 && !Passive()) || Environment.TickCount > wcasttime + 2700))
                    _w.Cast();
                if (EStage == ECastStage.Second && ((Environment.TickCount > casttime + 200 && !Passive()) || Environment.TickCount > ecasttime + 2700))
                    _e.Cast();
                if (!Passive() && useQ && _q.IsReady() && Environment.TickCount > casttime + 200 || mob.Health < _q.GetDamage(mob)*2)
                    CastQ1(mob);
                else if (!Passive() && _config.Item("PriW").GetValue<bool>() && useW && _w.IsReady()&& Environment.TickCount>casttime+200)
                    CastSelfW();
                else if (!Passive() && useE && _e.IsReady() && mob.Distance(_player.Position) < _e.Range && Environment.TickCount > casttime + 200 || mob.Health < _e.GetDamage(mob))
                    CastE1();

            }
        }
        private static void KillSteal()
        {
            var enemyVisible =
                        ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget() && _player.Distance(enemy.ServerPosition) <= 600).FirstOrDefault();

            {
                if (_player.GetSummonerSpellDamage(enemyVisible, Damage.SummonerSpell.Ignite) > enemyVisible.Health &&_igniteSlot != SpellSlot.Unknown &&
                _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                {
                    _player.Spellbook.CastSpell(_igniteSlot, enemyVisible);
                }
            }


            if (_e.IsReady() && _config.Item("UseEM").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Physical);
                if (_e.GetDamage(t) > t.Health && _player.Distance(t.ServerPosition) <= _e.Range )
                {
                    _e.Cast();
                }
            }
        }




        private static void Drawing_OnDraw(EventArgs args)
        
        {
            if (checker)
            {
                Drawing.DrawText(Drawing.WorldToScreen(jumppoint)[0] + 50,
                                Drawing.WorldToScreen(jumppoint)[1] + 40, Color.Red,
                                "NOT JUMPABLE");
            }
            if (_config.Item("wjump").GetValue<KeyBind>().Active)
            {
                Render.Circle.DrawCircle(jumppoint, 50, System.Drawing.Color.Red);
                Render.Circle.DrawCircle(movepoint, 50, System.Drawing.Color.White);
            }

            if (_config.Item("insc").GetValue<KeyBind>().Active)
            {

                Render.Circle.DrawCircle(insecpos, 75, System.Drawing.Color.Blue);
                Render.Circle.DrawCircle(insdirec, 100, System.Drawing.Color.Green);
            }
            

            if (_config.Item("damagetest").GetValue<bool>())
            {
                foreach (
                    var enemyVisible in
                        ObjectManager.Get<Obj_AI_Hero>().Where(enemyVisible => enemyVisible.IsValidTarget()))

                    if (ComboDamage(enemyVisible) > enemyVisible.Health)
                    {
                        Drawing.DrawText(Drawing.WorldToScreen(enemyVisible.Position)[0] + 50,
                            Drawing.WorldToScreen(enemyVisible.Position)[1] - 40, Color.Red,
                            "Combo=Rekt");
                    }

            }


            if (_config.Item("CircleLag").GetValue<bool>())
            {
                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.Blue);
                }
                if (_config.Item("DrawE").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.White);
                }
                if (_config.Item("DrawR").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _r.Range, System.Drawing.Color.Blue);
                }
            }
            else
            {
                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _q.Range, System.Drawing.Color.White);
                }
                if (_config.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _e.Range, System.Drawing.Color.White);
                }

                if (_config.Item("DrawR").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _r.Range, System.Drawing.Color.White);
                }
            }

        }


    }
}
