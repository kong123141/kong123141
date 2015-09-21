using System.IO;
using SharpDX.Multimedia;
using WaifuSharp.Enums;

namespace WaifuSharp.ResourceClasses
{
    class OnKillSound
    {
        public byte[] SoundStream { get; set; }

        public ResourcePriority SoundPriority { get; set; }

        public int MinWaifuLevel { get; set; }

        public bool PlayCondition
        {
            get { return true; }
        }

    }
}
