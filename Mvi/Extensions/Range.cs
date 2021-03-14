using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Extensions
{
    using CDoubleRange = System.Tuple<double, double>;
    using CTimeSpanRange = System.Tuple<TimeSpan, TimeSpan>;
    public static class CRangeExtensions
    {
        public static TimeSpan GetInRangeTimespan(this CTimeSpanRange r, double f)
            => TimeSpan.FromMilliseconds(r.Item1.TotalMilliseconds
                                    + (r.Item2.TotalMilliseconds - r.Item1.TotalMilliseconds) * f);
        public static double GetInRangeDouble(this CDoubleRange r, double f)
            => r.Item1 + (r.Item2 - r.Item1) * f;
    }
}
