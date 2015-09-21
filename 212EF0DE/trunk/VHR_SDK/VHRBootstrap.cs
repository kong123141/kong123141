using VHR_SDK.Utility;

namespace VHR_SDK
{
    class VHRBootstrap
    {
        public static void Init()
        {
            MenuGenerator.SetUp();
            VHR.OnLoad();
        }
    }
}
