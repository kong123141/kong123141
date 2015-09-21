using System;
using VHR_SDK.Interfaces;
using VHR_SDK.Utility;

namespace VHR_SDK.Modules
{
    class TestModule : IVHRModule
    {
        public bool ShouldBeLoaded()
        {
            return false;
        }

        public string GetModuleName()
        {
            return "Test Module";
        }

        public void OnLoad()
        {
            VHRDebug.WriteDebug("Test Module loaded successfully!");
        }

        public bool ShouldRun()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            throw new NotImplementedException();
        }
    }
}
