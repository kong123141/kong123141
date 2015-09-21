using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreshHunter.Utility
{
    internal enum QStates
    {
        Q1, Q2, NotReady, Unknown
    }

    internal enum EMode
    {
        Pull, Push
    }

    internal enum LanternMode
    {
        LowestHealth, Closest
    }
}
