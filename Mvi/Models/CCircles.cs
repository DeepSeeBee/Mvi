using CharlyBeck.Mvi.Extensions;
using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Mvi.World;
using Mvi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Models
{
    public sealed class CCircle
    {
        public CCircle(double r, int aSegmentCount)
        {
            var aDots = new CVector3Dbl[aSegmentCount];
            var aSegmentAngle = Math.PI * 2d / aSegmentCount;
            for(var i = 0; i < aSegmentCount; ++i)
            {
                var rad = aSegmentAngle * i;
                var x = Math.Cos(rad);
                var y = 0;
                var z = Math.Sin(rad);
                aDots[i] = new CVector3Dbl(x, y, z);
            }
            this.Dots = aDots;
            this.LineList = aDots.DotsToPolygonLineList().ToArray(); ;
        }

        public readonly CVector3Dbl[] Dots;
        public readonly CVector3Dbl[] LineList;
    }


    public sealed class CCircles : CShapeScales<CCircle>
    {
        public CCircles(int aMinSegmentCount, int aMaxSegmentCount) : base(aMinSegmentCount, aMaxSegmentCount)
        {
            this.Init();
        }
        protected override CCircle NewShape(int aScale)
            => new CCircle(1d, aScale);

        public int GetScaleByRadius(double r) => (int)r * this.MaxScale;
        public CCircle GetShapeByRadius(double r) => this.GetShapeByScale(this.GetScaleByRadius(r));
    }
}
