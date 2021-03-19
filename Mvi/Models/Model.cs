using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Sprites.Asteroid;
using CharlyBeck.Mvi.Sprites.Bumper;
using CharlyBeck.Mvi.Sprites.Crosshair;
using CharlyBeck.Mvi.Sprites.Explosion;
using CharlyBeck.Mvi.Sprites.Gem;
using CharlyBeck.Mvi.Sprites.GemSlot;
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
    public static class CColors
    {
        internal static readonly CVector3Dbl C_White = new CVector3Dbl(1, 1, 1);
        internal static readonly CVector3Dbl C_Black = new CVector3Dbl(0, 0, 0);
        internal static readonly CVector3Dbl C_Red = new CVector3Dbl(1, 0, 0);
        internal static readonly CVector3Dbl C_Green = new CVector3Dbl(0, 1, 0);
        internal static readonly CVector3Dbl C_Blue = new CVector3Dbl(0, 0, 1);
        internal static readonly CVector3Dbl C_Yellow = new CVector3Dbl(1, 1, 0);
        internal static readonly CVector3Dbl C_Orange = new CVector3Dbl(1d, 0.5d, 0.25d);
        internal static readonly CVector3Dbl C_LightBlue = new CVector3Dbl(0.25d, 1d, 1d);
        internal static readonly CVector3Dbl C_Purple = new CVector3Dbl(1d, 0.25d, 1d);
        internal static readonly CVector3Dbl C_DarkGrey = new CVector3Dbl(0.4d, 0.4d, 0.4d);
        internal static readonly CVector3Dbl C_DarkestGrey = new CVector3Dbl(0.1d, 0.1d, 0.1d);
        
        public static CVector3Dbl Shot_Canon => C_Yellow;
        public static CVector3Dbl Shot_GuidedMissile => C_Orange;
        public static CVector3Dbl Shot_NuclearMissile => C_Red;
        public static CVector3Dbl Shot_Drill => C_LightBlue;
        public static CVector3Dbl Shot_KruskalScanner => C_Purple;

        public static CVector3Dbl QuadrantGridGray => C_DarkestGrey;
        public static CVector3Dbl OrbitGray => C_DarkGrey;
        public static CVector3Dbl Crosshair => C_White;
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
            this.GemControlsModel = new CGemControlsModel(this);
        }
        #endregion
        public readonly CGridLinesModel GridLinesModel;
        public readonly CBumperModel AsteroidModel;
        public readonly CShotModel ShotModel;
        public readonly CCrosshairModel CrosshairModel;
        public readonly CExplosionModel ExplosionModel;
        public readonly CGemModel GemModel;
        public readonly CGemControlsModel GemControlsModel;
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
