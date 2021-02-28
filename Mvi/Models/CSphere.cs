using CharlyBeck.Mvi.Extensions;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.Enumerables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Models
{
    public sealed class CSphere
    {
        public CSphere(int c, double r = 1.0d, bool aSurfaceOnly = true)
        {
            var l = r / (double)c;
            var cnt = 2 * c + 1;
            var ys = new CVector3Dbl[cnt][][];

            for (var sy = 0; sy < cnt; ++sy)
            {
                var yf1 = ((double)sy) / ((double)cnt -1); // System.Diagnostics.Debug.Print(yf.ToString());
                var xsf = Math.Sin(Math.PI *  yf1);
                var yf = 1d - Math.Sin(Math.PI  * (yf1 + 0.5d) );// System.Diagnostics.Debug.Print(yf.ToString());
                var y2 =r - r * yf;
                var y = y2;//System.Diagnostics.Debug.Print(y.ToString());
                var xs = new CVector3Dbl[aSurfaceOnly ? 2 : cnt][];
                if(aSurfaceOnly)
                {
                    xs[0] = CalcBelt(cnt, c, -c, xsf,y,r);
                    xs[1] = CalcBelt(cnt, c, c, xsf, y,r);
                }
                else
                {
                    for (var sx = -c; sx <= c; ++sx)
                    {
                        xs[sx + c] = CalcBelt(cnt, c, sx, xsf,y,r);
                    }
                }
                ys[sy] = xs;
            }
            this.Slices = ys;
            this.Dots = (from d0 in this.Slices from d1 in d0 from d2 in d1 select d2).ToArray();
            this.OutterDots = (from d0 in this.Slices select (from d1 in d0.First() select d1).ToArray()).ToArray();
            this.HorizontalOutterPolygonLineList = (from aDots in this.OutterDots
                                          select aDots.DotsToPolygonLineList()).Flatten().ToArray();
            {
                var ds = this.OutterDots;
                this.TriangleStrips = (
                                       from i1 in Enumerable.Range(0, ds.Length - 1)
                                       from i2 in Enumerable.Range(0, ds[i1].Length - 1)
                                       select new CVector3Dbl[] { ds[i1 + 1][i2], ds[i1][i2], ds[i1][i2 + 1] }
                                      )
                                      .Concat(
                                       from i1 in Enumerable.Range(0, ds.Length - 1).Reverse() 
                                       from i2 in Enumerable.Range(0, ds[i1].Length - 1).Reverse()
                                       select new CVector3Dbl[] { ds[i1][i2+1], ds[i1+1][i2+1], ds[i1+1][i2] }

                                      )



                                      .Flatten().ToArray();
            }
        }

        private CVector3Dbl[] CalcBelt(int cnt, int c, int sx, double xsf, double y, double r)
        {
            var xf1 = ((double)sx + c) / ((double)cnt - 1);
            var xf2 = Math.Sin(Math.PI * xf1);
            var xf = r * ((double)sx * 2) / (double)(c * 2) * xsf;
            //var xf = r * xf1 * xf2;
            var rs = new CVector3Dbl[cnt];
            for (var sr = 0; sr < cnt; ++sr)
            {
                var fkt = ((double)sr) / (double)(cnt - 1);
                var x = xf * Math.Sin(Math.PI * 2.0d * (fkt + 0.25d));
                var z = xf * Math.Sin(Math.PI * 2.0d * fkt);
                var p = new CVector3Dbl(x, y, z);
                rs[sr] = p;
            }
            return rs;
        }
        internal CVector3Dbl[][][] Slices;
        public readonly CVector3Dbl[] Dots;
        public readonly CVector3Dbl[][] OutterDots;
        public readonly CVector3Dbl[] HorizontalOutterPolygonLineList;
        public readonly CVector3Dbl[] TriangleStrips;

    }
}
