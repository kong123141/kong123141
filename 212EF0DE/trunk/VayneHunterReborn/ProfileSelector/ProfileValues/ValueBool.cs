using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VayneHunter_Reborn.ProfileSelector.ProfileValues
{
    class ValueBool : IValue
    {
        public static bool Value;
        public ValueBool(bool val)
        {
            Value = val;
        }

        public bool GetValue()
        {
            return Value;
        }
    }
}
