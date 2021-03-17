using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Utils3.DateTimes
{
    public static class CDateTimeExtensions
    {
        public static TimeSpan Min(this TimeSpan lhs, TimeSpan rhs)
            => lhs.CompareTo(rhs) <= 0 ? lhs : rhs;
        public static TimeSpan Max(this TimeSpan lhs, TimeSpan rhs)
            => lhs.CompareTo(rhs) >= 0 ? lhs : rhs;
    }
}
