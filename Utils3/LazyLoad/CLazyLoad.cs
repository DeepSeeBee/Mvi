using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Utils3.LazyLoad
{
   public static class CLazyLoad
   {
        public static T Get<T>(ref T aVar, Func<T> aLoad) where T : class
        {
            if (!(aVar is T))
            {
                aVar = aLoad();
            }
            return aVar;
        }
        public static T Get<T>(ref T? aVar, Func<T> aLoad) where T : struct
        {
            if (!(aVar.HasValue))
            {
                aVar = aLoad();
            }
            return aVar.Value;
        }

        public static void Load(this object aObject)
      { }

        public static bool RefEquals<T>(this T lhs, T rhs) where T :class
            => object.ReferenceEquals(lhs, rhs);
   }
}
