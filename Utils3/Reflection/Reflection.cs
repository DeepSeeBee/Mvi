using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Utils3.Reflection
{
   using System.Reflection;

    public static class CReflectionExtensions
    {
        public static T GetCustomAttribute<T>(this Enum aEnum) where T : Attribute
            => aEnum.GetType().GetField(aEnum.ToString()).GetCustomAttribute<T>();

        public static int GetEnumMaxValue(this Type aEnumType)
            => Enum.GetValues(aEnumType).Cast<Enum>().Select(aEn=>(int)(object)aEn).OrderBy(e=>e).Last();
        public static Tuple<TEnum, TAttribute>[] GetEnumAttributes<TEnum, TAttribute>(this Type aEnumType) where TAttribute : Attribute
        {
            var aCount = aEnumType.GetEnumMaxValue() + 1;
            var aItems = new Tuple<TEnum, TAttribute>[aCount];
            for(var i = 0; i< aCount; ++i)
            {
                var aEnum = (Enum)(object)i;
                aItems[i] = new Tuple<TEnum, TAttribute>((TEnum)(object)aEnum, aEnum.GetCustomAttribute<TAttribute>());
            }
            return aItems;
        }
    }
}
