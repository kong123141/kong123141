using System;
namespace VHR_SDK.Interfaces
{
    interface IVHRModule
    {
        bool ShouldBeLoaded();

        String GetModuleName();

        void OnLoad();

        bool ShouldRun();

        void Run();
    }
}
