using System;
using System.Collections.Generic;
using EndifsCreations.Controller;
using EndifsCreations.Tools;
using EndifsCreations.SummonerSpells;
using LeagueSharp;
using LeagueSharp.Common;

namespace EndifsCreations
{
    class PluginData
    {
        //Spells
        protected Spell Q { get; set; }
        protected Spell W { get; set; }
        protected Spell E { get; set; }
        protected Spell R { get; set; }

        //Spells 2
        protected Spell Q2 { get; set; }
        protected Spell W2 { get; set; }
        protected Spell E2 { get; set; }
        protected Spell R2 { get; set; }

        //Spells 3
        protected Spell Q3 { get; set; }

        protected static Obj_AI_Hero Player = ObjectManager.Player;
        protected static Obj_AI_Hero Target, LastTarget, LockedTarget, TempTarget;

        protected int LastOrder, LastAA, LastSpell;
        protected int LastQ, LastW, LastE, LastR;
        protected int LastQ2, LastW2, LastE2, LastR2;

        protected readonly List<Spell> SpellList = new List<Spell>();

        //Summoner Spells
        protected SpellSlot FlashSlot = ObjectManager.Player.GetSpellSlot("summonerflash");
        protected SpellSlot SmiteSlot = ObjectManager.Player.GetSpellSlot("summonersmite");
        protected SpellSlot IgniteSlot = ObjectManager.Player.GetSpellSlot("summonerdot");
        protected SpellSlot BarrierSlot = ObjectManager.Player.GetSpellSlot("summonerbarrier");
        protected SpellSlot HealSlot = ObjectManager.Player.GetSpellSlot("summonerheal");

        public static Menu config, plugins;
     
        protected PluginData()
        {
            InitializeSharedMenu();
            InitializeEvents();   
            InitializeTools();
        }

        private void InitializeTools()
        {
            new myHumazier();
            new myRePriority();
            //new myMarkDash();
        }
        
        private void InitializeSharedMenu()
        {
            config = new Menu("Endif's " + Player.ChampionName, Player.ChampionName, true);
            var myorb = new Menu("myOrbwalker", "myOrbwalker");
            {
                myOrbwalker.AddToMenu(myorb);
                config.AddSubMenu(myorb);
            }
            var ts = new Menu("Target Selector", "Target Selector");
            {
                TargetSelector.AddToMenu(ts);
                config.AddSubMenu(ts);
            }
            var di = new Menu("Damage Indicator", "Damage Indicator");
            {
                myDamageIndicator.AddToMenu(di);
                config.AddSubMenu(di);
            }
            myHumazier.AddToMenu(myorb);
            config.AddToMainMenu();
            plugins = new Menu("Endif's Plugins", "EndifsPlugins", true);
            if (SmiteSlot != SpellSlot.Unknown || IgniteSlot != SpellSlot.Unknown || BarrierSlot != SpellSlot.Unknown || HealSlot != SpellSlot.Unknown)
            {
                mySummonerSpell.AddToMenu(plugins);
            }
            myDevTools.AddToMenu(plugins);            
            plugins.AddToMainMenu();
        }
        private void InitializeEvents()
        {            
            Game.OnUpdate += OnUpdate;
            Game.OnWndProc += OnWndProc;
            Game.OnProcessPacket += OnProcessPacket;
            Game.OnSendPacket += OnSendPacket;        
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += OnEndScene;
            Spellbook.OnCastSpell += OnCastSpell;
            Spellbook.OnStopCast += OnStopCast;
            myOrbwalker.BeforeAttack += OnBeforeAttack;
            myOrbwalker.AfterAttack += OnAfterAttack;
            myOrbwalker.OnNonKillableMinion += OnNonKillableMinion;
            CustomEvents.Unit.OnDash += OnDash;    
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += OnInterruptableTarget;
            Obj_AI_Base.OnNewPath += OnNewPath;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Obj_AI_Hero.OnPlayAnimation += OnPlayAnimation;
            Obj_AI_Base.OnIssueOrder += OnIssueOrder;
            Obj_AI_Base.OnTeleport += OnTeleport;
            Obj_AI_Base.OnCreate += OnCreate;
            Obj_AI_Base.OnDelete += OnDelete;            
            Obj_AI_Base.OnDamage += OnDamage;
            Obj_AI_Base.OnBuffAdd += OnBuffAdd;
            Obj_AI_Base.OnBuffRemove += OnBuffRemove;         
            myCustomEvents.ProcessDamageBuffer += ProcessDamageBuffer;
        }
        protected virtual void OnDash(Obj_AI_Base sender, Dash.DashItem args) { }        
        protected virtual void OnProcessPacket(GamePacketEventArgs args) { }
        protected virtual void OnSendPacket(GamePacketEventArgs args) { }
        protected virtual void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) { }
        protected virtual void OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args) { }
        protected virtual void OnEnemyGapcloser(ActiveGapcloser gapcloser) { }
        protected virtual void OnUpdate(EventArgs args) { }
        protected virtual void OnBeforeAttack(myOrbwalker.BeforeAttackEventArgs args) { }
        protected virtual void OnAfterAttack(AttackableUnit unit, AttackableUnit target) { }
        protected virtual void OnNonKillableMinion(AttackableUnit minion) { }
        protected virtual void OnLoad(EventArgs args) { }
        protected virtual void OnDraw(EventArgs args) { }     
        protected virtual void OnEndScene(EventArgs args) { }
        protected virtual void OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args) { }
        protected virtual void OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args) { }
        protected virtual void OnWndProc(WndEventArgs args) { }
        protected virtual void OnTeleport(GameObject sender, GameObjectTeleportEventArgs args) { }        
        protected virtual void OnCreate(GameObject sender, EventArgs args) { }
        protected virtual void OnDelete(GameObject sender, EventArgs args) { }
        protected virtual void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args) { }
        protected virtual void OnStopCast(Spellbook sender, SpellbookStopCastEventArgs args) { }
        protected virtual void OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args) { }
        protected virtual void OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args) { }
        protected virtual void OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args) { }
        protected virtual void OnBuffRemove(Obj_AI_Base sender, Obj_AI_BaseBuffRemoveEventArgs args) { }
        protected virtual void ProcessDamageBuffer(Obj_AI_Base sender, Obj_AI_Hero target, SpellData spell, myCustomEvents.DamageTriggerType type) { }
    }
}
