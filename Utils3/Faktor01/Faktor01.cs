﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Utils3.Faktor01
{
    public static class CFaktor01
    {
        public static double F01_GetFactor(this double d, double aMin, double aMax)
        {
            if (d < aMin)
                return 0d;
            else if (d > aMax)
                return 1d;
            else
                return (d - aMin) / (aMax - aMin);
        }
        public static double F01_Map(this double d, double aSourceMin, double aSourceMax, double aTargetMin, double aTargetMax)
        {
            var f = d.F01_GetFactor(aSourceMin, aSourceMax);
            var r = aTargetMin + f * (aTargetMax - aTargetMin);
            return r;
        }

        public static float F01_Map(this float f, 
                                    float aSourceMin, 
                                    float aSourceMax, 
                                    float aTargetMin, 
                                    float aTargetMax)
            => (float)((double)f).F01_Map((double)aSourceMin, (double)aSourceMax, (double)aTargetMin, (double)aTargetMax);


    }
}
