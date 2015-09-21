using LeagueSharp.Common;

namespace TheKalista.Commons
{
    public interface ISummonerSpell
    {
        string GetDisplayName();
        void Initialize(Menu menu);
        void Update();
        bool IsAvailable();
    }
}
