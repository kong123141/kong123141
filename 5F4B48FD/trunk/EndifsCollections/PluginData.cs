using System;
using System.Collections.Generic;
using EndifsCollections.Controller;
using EndifsCollections.Tools;
using EndifsCollections.SummonerSpells;
using LeagueSharp;
using LeagueSharp.Common;

namespace EndifsCollections
{
    class PluginData
    {
        public Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

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
       
        protected readonly List<Spell> SpellList = new List<Spell>();

        //Summoner Spells
        protected SpellSlot FlashSlot = ObjectManager.Player.GetSpellSlot("SummonerFlash");

        public static Menu config;
        public static Menu plugins;
     
        protected PluginData()
        {
            InitializeSharedMenu();
            InitializeEvents();   
            InitializeTools();
        }
        private void InitializeTools()
        {
            new myRePriority();
            new mySmiter();
            new myMarkDash();
            new myIgniter();
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
            config.AddToMainMenu();

            plugins = new Menu("Endif's Plugins", "EndifsPlugins", true);
            var ss = new Menu("Summoner Spells", "Summoner Spells");
            {
                mySmiter.AddToMenu(ss);
                myMarkDash.AddToMenu(ss);
                myIgniter.AddToMenu(ss);
                plugins.AddSubMenu(ss);
            }
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
        protected virtual void OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args) { }
        protected virtual void OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args) { }
    }
}
