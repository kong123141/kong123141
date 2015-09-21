using System;
namespace ThreshHunter.Interfaces
{
    interface ITHModule
    {
        bool ShouldBeLoaded();

        String GetModuleName();

        void OnLoad();

        bool ShouldRun();

        void Run();
    }
}
