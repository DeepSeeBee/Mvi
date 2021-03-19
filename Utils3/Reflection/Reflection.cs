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

        public static T[] NewEnumLookup<TEnum, T>(this Type aEnumType, Func<TEnum, T> aGetLookupTarget)
            => aEnumType.GetEnumValues().Cast<TEnum>().OrderBy(e => (int)(object)e).Select(e => aGetLookupTarget(e)).ToArray();
        public static bool GetCustomAttributeIsDefined<T>(this Enum aEnum) where T : Attribute
            => aEnum.GetType().GetField(aEnum.ToString()).IsDefined(typeof(T), false);

        public static T GetCustomAttribute<T>(this Enum aEnum) where T : Attribute
            => aEnum.GetType().GetField(aEnum.ToString()).GetCustomAttribute<T>();
        public static T GetCustomAttributeNullable<T>(this Enum aEnum) where T : Attribute
            => aEnum.GetCustomAttributeIsDefined<T>() ? aEnum.GetCustomAttribute<T>() : default;
        public static int GetEnumMaxValue(this Type aEnumType)
            => Enum.GetValues(aEnumType).Cast<Enum>().Select(aEn=>(int)(object)aEn).OrderBy(e=>e).Last();
        public static Tuple<TEnum, TAttribute>[] GetEnumAttributes<TEnum, TAttribute>(this Type aEnumType) where TAttribute : Attribute
        {
            var aEnumValues = aEnumType.GetEnumValues().Cast<TEnum>().ToArray();
            var aCount = aEnumValues.Length;
            var aItems = new Tuple<TEnum, TAttribute>[aCount];
            for(var i = 0; i< aCount; ++i)
            {
                var aEnum = aEnumValues[i];
                aItems[i] = new Tuple<TEnum, TAttribute>((TEnum)(object)aEnum, ((Enum)(object)aEnum).GetCustomAttribute<TAttribute>());
            }
            return aItems;
        }
    }
}
