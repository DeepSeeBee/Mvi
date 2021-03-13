using CharlyBeck.Mvi.Extensions;
using CharlyBeck.Mvi.World;
using Mvi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Models
{
    using CColoredVertex = Tuple<CVector3Dbl, CVector3Dbl>;
    using CColor = CVector3Dbl;

    public class COctaeder
    {
        public COctaeder(double aW = 1.0d)
        {
            W = aW;
            P0 = new CVector3Dbl(Xc, Yb, Zc);
            P1 = new CVector3Dbl(Xl, Yc, Zf);
            P2 = new CVector3Dbl(Xr, Yc, Zf);
            P3 = new CVector3Dbl(Xr, Yc, Zb);
            P4 = new CVector3Dbl(Xl, Yc, Zb);
            P5 = new CVector3Dbl(Xc, Yt, Zc);
            Ps= new CVector3Dbl[] { P0, P1, P2, P3, P4, P5 };
            P0C = CColors.Red;
            P1C = CColors.Blue;
            P2C = CColors.Yellow;
            P3C = CColors.Blue;
            P4C = CColors.Yellow;
            P5C = CColors.Green;
            Colors = new CColor[] { P0C, P1C, P2C, P3C, P4C, P5C };
            Tis = new int[] { 0, 1, 2,
                                      0,2,3,
                                      0,3,4,
                                      0,4,1,
                                      1,5,2,
                                      2,5,3,
                                      3, 5, 4 ,
                                      4,5,1
                                        };
            var aLineListIndexes = new int[]
            {
                0,1,
                0,2,
                0,3,
                0,4,
                1,2,
                2,3,
                3,4,
                4,1,
                5,1,
                5,2,
                5,3,
                5,4
            };

            this.LineList = (from aIdx in aLineListIndexes select Ps[aIdx]).ToArray(); 
            this.ColoredTriangleList = (from aIdx in Enumerable.Range(0, TCount * 3) select Ps[Tis[aIdx]].ToColoredVertex(this.Colors[Tis[aIdx]])).ToArray();
            this.ColoredLineList = this.ColoredTriangleList.TriangleListToLineList().ToArray();
        }
        public double W;
        public double L => W / 2.0d;
        public double Xl => -L;
        public double Xc => 0;
        public double Xr => L;
        public double Yb => -L;
        public double Yc => 0;
        public double Yt => L;
        public double Zf => L;
        public double Zc => 0;
        public double Zb => -L;
        public CVector3Dbl P0;

        public CVector3Dbl P1;
        public CVector3Dbl P2;
        public CVector3Dbl P3;
        public CVector3Dbl P4;
        public CVector3Dbl P5;
        public CVector3Dbl[] Ps;
        public CColor P0C;
        public CColor P1C;
        public CColor P2C;
        public CColor P3C;
        public CColor P4C;
        public CColor P5C;
        public CColor[] Colors;
        public int[] Tis; // TriangleIndexes
        public const int PCount = 6;
        public const int TCount = 8;
        public readonly CColoredVertex[] ColoredTriangleList;
        public readonly CColoredVertex[] ColoredLineList;
        public readonly CVector3Dbl[] LineList;

    }
}
