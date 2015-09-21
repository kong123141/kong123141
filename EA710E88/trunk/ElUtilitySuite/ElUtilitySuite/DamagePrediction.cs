namespace ElUtilitySuite
{
    using LeagueSharp;
    using LeagueSharp.Common;

    internal class DamagePrediction
    {
        #region Constructors and Destructors

        static DamagePrediction()
        {
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }

        #endregion

        #region Delegates

        public delegate void OnKillableDelegate(Obj_AI_Hero sender, Obj_AI_Hero target, SpellData SData);

        #endregion

        #region Public Events

        public static event OnKillableDelegate OnTargettedSpellWillKill;

        #endregion

        #region Methods

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!(sender is Obj_AI_Hero) || !(args.Target is Obj_AI_Hero))
            {
                return;
            }

            var senderHero = (Obj_AI_Hero)sender;
            var targetHero = (Obj_AI_Hero)args.Target;

            var predictedDamage = Orbwalking.IsAutoAttack(args.SData.Name)
                                      ? senderHero.GetAutoAttackDamage(targetHero, true)
                                      : senderHero.GetSpellDamage(targetHero, args.SData.Name);

            var predictedHealth = (int)(((Entry.Player.Health - predictedDamage) / Entry.Player.MaxHealth) * 100);

            if (predictedHealth <= InitializeMenu.Menu.Item("Heal.HP").GetValue<Slider>().Value)
            {
                if (predictedDamage > 30)
                {
                    // ReSharper disable once UseNullPropagation
                    if (OnTargettedSpellWillKill != null)
                    {
                        OnTargettedSpellWillKill(senderHero, targetHero, args.SData);
                    }
                }
            }
        }

        #endregion
    }
}