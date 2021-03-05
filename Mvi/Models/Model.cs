using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Sprites.Asteroid;
using CharlyBeck.Mvi.Sprites.Cube;
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
            this.World = this.ServiceContainer.GetService<CWorld>();
            this.AsteroidModel = new CAsteroidModel(this);
            this.CubeModel = new CCubeModel(this);
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();        
        #endregion
        private readonly CWorld World;
        public readonly CAsteroidModel AsteroidModel;
        public readonly CCubeModel CubeModel;
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
