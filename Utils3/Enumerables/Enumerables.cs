using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Utils3.Enumerables
{
    public static class Enumerables
    {
        public static int? FirstOrNull(this IEnumerable<int> aItems)
            => aItems.IsEmpty() ? default(int?) : aItems.First();

        public static IEnumerable<int> Range(this int i)
            => Enumerable.Range(0, i);
        public static bool ElementsAreEqual<T>(this T[] lhs, T[] rhs)
        {
            if(lhs.Length == rhs.Length)
            {
                for(var aIdx = 0; aIdx < lhs.Length; ++aIdx)
                {
                    if (!object.Equals(lhs[aIdx], rhs[aIdx]))
                        return false;
                }
                return true;
            }
            return false;
        }
        public static T[] Subset<T>(this IEnumerable<T> aEnumerable, int aStart, int aCount)
        {
            var aArray = new T[aCount];
            var aIdx = 0;
            foreach (var aItem in aEnumerable.Skip(aStart))
            {
                aArray[aIdx] = aItem;
                ++aIdx;
                if (aIdx == aCount)
                    return aArray;
            }
            throw new InvalidOperationException();
        }
        public static int IndexOf<T>(this IEnumerable<T> aEnumerable, T aFind)
        {
            return aEnumerable.IndexOfNullable(aFind).Value;
        }
        public static int? IndexOfNullable<T>(this IEnumerable<T> aEnumerable, T aFind)
        {
            var aIndex = default(int?);
            var aTestIndex = 0;
            foreach (var aItem in aEnumerable)
            {
                if (object.Equals(aFind, aItem))
                {
                    if (aIndex.HasValue)
                    {
                        throw new Exception("Item is duplicate");
                    }
                    else
                    {
                        aIndex = aTestIndex;
                    }
                }
                ++aTestIndex;
            }
            return aIndex;
        }
        public static bool IsEmpty<T>(this IEnumerable<T> aEnumberable)
        {
            var aEnumerator = aEnumberable.GetEnumerator();
            return !aEnumerator.MoveNext();
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> itemss)
            => from items in itemss from item in items select item;
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T[]> itemss)
            => (from items in itemss select items.AsEnumerable<T>()).Flatten();
    }
}
