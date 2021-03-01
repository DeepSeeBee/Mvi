using CharlyBeck.Mvi.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Extensions
{
    using CColoredVertexDbl = Tuple<CVector3Dbl, CVector3Dbl>;

    public static class CExtensions
    {
        public static double AvoidNan(this double d)
            => double.IsNaN(d) ? 0 : d;
        public static CColoredVertexDbl ToColoredVertex(this CVector3Dbl v, CVector3Dbl c)
            => new CColoredVertexDbl(v, c);


        public static IEnumerable<T> TriangleListToLineList<T>(this T[] vs)
        {
            foreach(var idx in Enumerable.Range(0, vs.Length / 3))
            {
                yield return vs[idx * 3 + 0];
                yield return vs[idx * 3 + 1];
                yield return vs[idx * 3 + 1];
                yield return vs[idx * 3 + 2];
                yield return vs[idx * 3 + 2];
                yield return vs[idx * 3 + 0];
            }
        }

        public static IEnumerable<T> DotsToLineList<T>(this T[] vs, bool aPolygon)
        {
            foreach (var p in (from i in Enumerable.Range(0, vs.Length - 1) 
                               select new Tuple<T, T>(vs[i], vs[i + 1]))
                               .Concat(aPolygon && vs.Length > 0 ? new Tuple<T, T>[] { new Tuple<T, T>(vs.Last(), vs.First()) } : Array.Empty<Tuple<T, T>>()))
            {
                yield return p.Item1;
                yield return p.Item2;
            }
        }
        public static IEnumerable<T> DotsToLineList<T>(this T[] vs)
            => vs.DotsToLineList(false);
        public static IEnumerable<T> DotsToPolygonLineList<T>(this T[] vs)
            => vs.DotsToLineList(true);



        //internal static double GetLength(this CVector3Dbl aPoint) // Not tested, https://www.engineeringtoolbox.com/distance-relationship-between-two-points-d_1854.html
        //    => Math.Sqrt((aPoint.x * aPoint.x) + (aPoint.y * aPoint.y) + (aPoint.z * aPoint.z));

        //internal static CVector3Dbl MakeLongerDelta(this CVector3Dbl aVector, float aLength) // Not tested, https://www.freemathhelp.com/forum/threads/extend-length-of-line-in-3d-space-find-new-end-point.125160/
        //     => (new CVector3Dbl(aLength) / new CVector3Dbl(aVector.GetLength())) * aVector;

        //internal static double GetDistance(this CVector3Dbl v1, CVector3Dbl v2)
        //    => v1.Max(v2).Subtract(v1.Min(v2)).GetLength();
        internal static CVector3Dbl Min(this CVector3Dbl lhs, CVector3Dbl rhs)
            => new CVector3Dbl(Math.Min(lhs.x, rhs.x), Math.Min(lhs.y, rhs.y), Math.Min(lhs.z, rhs.z));
        internal static CVector3Dbl Max(this CVector3Dbl lhs, CVector3Dbl rhs)
            => new CVector3Dbl(Math.Max(lhs.x, rhs.x), Math.Max(lhs.y, rhs.y), Math.Max(lhs.z, rhs.z));

        internal static CVector3Dbl Subtract(this CVector3Dbl lhs, CVector3Dbl rhs)
            => lhs - rhs;
        internal static CVector3Dbl Divide(this CVector3Dbl lhs, CVector3Dbl rhs)
            => lhs / rhs;

        internal static double DotProduct(this CVector3Dbl lhs, CVector3Dbl rhs)
            => lhs.x * rhs.x + lhs.y * rhs.y + lhs.z + rhs.z;
    }


}
