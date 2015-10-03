namespace ElSmite
{
    using ElSmite.Plugins;

    internal class Base
    {
        #region Methods

        internal static void Load(string champName)
        {
            switch (champName)
            {
                case "LeeSin":
                    LeeSin.Load();
                    break;

                case "Nunu":
                    Nunu.Load();
                    break;

                case "Olaf":
                    Olaf.Load();
                    break;

                case "Elise":
                    Elise.Load();
                    break;

                case "Nidalee":
                    Nidalee.Load();
                    break;

                case "Chogath":
                    Chogath.Load();
                    break;

                case "Pantheon":
                    Pantheon.Load();
                    break;

                case "MonkeyKing":
                    MonkeyKing.Load();
                    break;

                case "Fizz":
                    Fizz.Load();
                    break;
            }
        }

        #endregion
    }
}