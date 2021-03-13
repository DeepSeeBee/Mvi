using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Sprites.Asteroid;
using CharlyBeck.Mvi.Sprites.Bumper;
using CharlyBeck.Mvi.Sprites.Crosshair;
using CharlyBeck.Mvi.Sprites.Explosion;
using CharlyBeck.Mvi.Sprites.Gem;
using CharlyBeck.Mvi.Sprites.GridLines;
using CharlyBeck.Mvi.Sprites.Shot;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvi.Models
{
    internal static class CColors
    {
        internal static readonly CVector3Dbl White = new CVector3Dbl(1, 1, 1);
        internal static readonly CVector3Dbl Black = new CVector3Dbl(0, 0, 0);
        internal static readonly CVector3Dbl Red = new CVector3Dbl(1, 0, 0);
        internal static readonly CVector3Dbl Green = new CVector3Dbl(0, 1, 0);
        internal static readonly CVector3Dbl Blue = new CVector3Dbl(0, 0, 1);
        internal static readonly CVector3Dbl Yellow = new CVector3Dbl(1, 1, 0);
    }


    internal sealed class CEnums<TEnum>
    {
        internal CEnums()
        {
            this.Fields = typeof(TEnum).GetEnumValues().OfType<TEnum>().ToArray();
        }
        internal readonly TEnum[] Fields;

    }
    public abstract class CModel : CServiceLocatorNode
    {
        internal CModel(CServiceLocatorNode aParent) :base(aParent)
        {
            this.World = this.ServiceContainer.GetService<CWorld>();
        }

        public readonly CWorld World;
    }


    public sealed class CModels : CServiceLocatorNode
    {
        #region ctor
        internal CModels(CServiceLocatorNode aParent) :base(aParent)
        {
            this.AsteroidModel = new CBumperModel(this);
            this.GridLinesModel = new CGridLinesModel(this);
            this.ShotModel = new CShotModel(this);
            this.CrosshairModel = new CCrosshairModel(this);
            this.ExplosionModel = new CExplosionModel(this);
            this.GemModel = new CGemModel(this);
        }
        #endregion
        public readonly CGridLinesModel GridLinesModel;
        public readonly CBumperModel AsteroidModel;
        public readonly CShotModel ShotModel;
        public readonly CCrosshairModel CrosshairModel;
        public readonly CExplosionModel ExplosionModel;
        public readonly CGemModel GemModel;

    }
    public abstract class CShapeScales<TShape>
    {
        public CShapeScales(int aMinScale, int aMaxScale)
        {
            this.MinScale = aMinScale;
            this.MaxScale = aMaxScale;
        }

        protected void Init()
        {
            var c = this.Count;
            var aShapes = new TShape[c];
            for (var i = 0; i < c; ++i)
            {
                var aScale = i + this.MinScale;
                aShapes[i] = this.NewShape(aScale);
            }
            this.Shapes = aShapes;
        }

        public readonly int MinScale;
        public readonly int MaxScale;
        internal int Count => this.MaxScale - this.MinScale;
        protected TShape[] Shapes;


        public TShape GetShapeByScale(int aScale) => this.Shapes[Math.Max(Math.Min(aScale, this.MaxScale-1), this.MinScale) - this.MinScale];


        protected abstract TShape NewShape(int aScale);
    }
}
