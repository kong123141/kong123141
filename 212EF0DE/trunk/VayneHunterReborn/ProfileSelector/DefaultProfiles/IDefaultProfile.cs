﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VayneHunter_Reborn.ProfileSelector.DefaultProfiles
{
    interface IDefaultProfile
    {
        ProfileSettings GetProfileSettings();
    }
}
