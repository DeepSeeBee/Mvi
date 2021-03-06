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
    }
}
