using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Feature;
using CharlyBeck.Mvi.Sprites.Bumper;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Sprites.Quadrant
{

    internal abstract class CQuadrantTileDescriptor : CRootTileDescriptor
    {
        #region ctor
        internal CQuadrantTileDescriptor(CServiceLocatorNode aParent, CTileBuilder aTileBuilder) : base(aParent, aTileBuilder)
        {
        }
        #endregion
        protected override void OnBuild()
        {
            this.QuadrantSpriteData = new CQuadrantSpriteData(this, this.TileBuilder, this);
        }

        internal CQuadrantSpriteData QuadrantSpriteData { get; private set; }

        protected override void OnDraw()
        {
            base.OnDraw();
            this.QuadrantSpriteData.Draw();
        }
    }
    public sealed class CQuadrantSpriteData : CSpriteData
    {
        internal CQuadrantSpriteData(CServiceLocatorNode aParent, CTileBuilder aTileBuilder, CTileDescriptor aTileDescriptor) : base(aParent, aTileBuilder, aTileDescriptor)
        {
            this.QuadrantFeature = CFeature.Get(this.World, QuadrantFeatureDeclaration);
            this.Init();
            this.Build();
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();

        protected override void OnBuild()
        {
            var aTileBuilder = this.TileBuilder;
            var aTile = aTileBuilder.Tile;
            var aWorld = aTileBuilder.World;
            var aSize2 = aWorld.EdgeLen;

            this.Coordinates2 = this.GetWorldPos(aTile.AbsoluteCubeCoordinates);
            this.Size2 = aSize2;

            this.Center = new CVector3Dbl(this.Coordinates2.x + aSize2 / 2d, this.Coordinates2.y + aSize2 / 2d, this.Coordinates2.z + aSize2 / 2d);
            this.FrontBottomLeft2 = new CVector3Dbl(this.Coordinates2.x, this.Coordinates2.y, this.Coordinates2.z);
            this.FrontTopLeft2 = new CVector3Dbl(this.Coordinates2.x, this.Coordinates2.y + aSize2, this.Coordinates2.z);
            this.FrontTopRight2 = new CVector3Dbl(this.Coordinates2.x + aSize2, this.Coordinates2.y + aSize2, this.Coordinates2.z);
            this.FrontBottomRight2 = new CVector3Dbl(this.Coordinates2.x + aSize2, this.Coordinates2.y, this.Coordinates2.z);

            this.BackBottomLeft2 = new CVector3Dbl(this.Coordinates2.x, this.Coordinates2.y, this.Coordinates2.z + aSize2);
            this.BackTopLeft2 = new CVector3Dbl(this.Coordinates2.x, this.Coordinates2.y + aSize2, this.Coordinates2.z + aSize2);
            this.BackTopRight2 = new CVector3Dbl(this.Coordinates2.x + aSize2, this.Coordinates2.y + aSize2, this.Coordinates2.z + aSize2);
            this.BackBottomRight2 = new CVector3Dbl(this.Coordinates2.x + aSize2, this.Coordinates2.y, this.Coordinates2.z + aSize2);

            
        }

        #region Features
        [CFeatureDeclaration]
        internal static readonly CFeatureDeclaration QuadrantFeatureDeclaration = new CFeatureDeclaration(new Guid("4af89c99-2734-4c25-a8d5-9417a2d17b77"), "Quadrant");
        public readonly CFeature QuadrantFeature;
        #endregion
        public override CVector3Dbl WorldPos => this.GetWorldPos(this.AbsoluteCubeCoordinates);
        internal override ISprite NewSprite()
           => this.NewSprite<CQuadrantSpriteData>(this);
        internal override int ChangesCount => 0;

        public CVector3Dbl Coordinates2 { get; private set; }
        public double Size2 { get; private set; }
        public CVector3Dbl Center { get; private set; }
        public CVector3Dbl FrontBottomLeft2 { get; private set; }
        public CVector3Dbl FrontTopLeft2 { get; private set; }
        public CVector3Dbl FrontTopRight2 { get; private set; }
        public CVector3Dbl FrontBottomRight2 { get; private set; }
        public CVector3Dbl BackBottomLeft2 { get; private set; }
        public CVector3Dbl BackTopLeft2 { get; private set; }
        public CVector3Dbl BackTopRight2 { get; private set; }
        public CVector3Dbl BackBottomRight2 { get; private set; }

        public IEnumerable<CVector3Dbl> FrontSquare2
        {
            get
            {
                yield return this.FrontBottomLeft2;
                yield return this.FrontTopLeft2;
                yield return this.FrontTopRight2;
                yield return this.FrontBottomRight2;
            }
        }


        public IEnumerable<CVector3Dbl> BackSquare2
        {
            get
            {
                yield return this.BackBottomLeft2;
                yield return this.BackTopLeft2;
                yield return this.BackTopRight2;
                yield return this.BackBottomRight2;
            }
        }

        public IEnumerable<CVector3Dbl> TopSquare2
        {
            get
            {
                yield return this.FrontTopLeft2;
                yield return this.BackTopLeft2;
                yield return this.BackTopRight2;
                yield return this.FrontTopRight2;
            }
        }

        public IEnumerable<CVector3Dbl> BottomSquare2
        {
            get
            {
                yield return this.FrontBottomLeft2;
                yield return this.BackBottomLeft2;
                yield return this.BackBottomRight2;
                yield return this.FrontBottomRight2;
            }
        }

        public IEnumerable<CVector3Dbl> LeftSquare2
        {
            get
            {
                yield return this.FrontBottomLeft2;
                yield return this.FrontTopLeft2;
                yield return this.BackTopLeft2;
                yield return this.BackBottomLeft2;
            }
        }

        public IEnumerable<CVector3Dbl> RightSquare2
        {
            get
            {
                yield return this.FrontBottomRight2;
                yield return this.FrontTopRight2;
                yield return this.BackTopRight2;
                yield return this.BackBottomRight2;
            }
        }



        public IEnumerable<CVector3Dbl> Lines2
        {
            get
            {
                //yield return this.FrontBottomLeft;
                //yield return this.FrontBottomRight;
                //              yield return this.FrontBottomLeft;
                //                yield return this.FrontTopLeft;#

                foreach (var aPoint in this.FrontSquare2.PolyPointsToLines()) //.Subset(2,4))
                    yield return aPoint;
                foreach (var aPoint in this.BackSquare2.PolyPointsToLines())
                    yield return aPoint;
                foreach (var aPoint in this.LeftSquare2.PolyPointsToLines())
                    yield return aPoint;
                foreach (var aPoint in this.TopSquare2.PolyPointsToLines())
                    yield return aPoint;
                foreach (var aPoint in this.RightSquare2.PolyPointsToLines())
                    yield return aPoint;
                foreach (var aPoint in this.BottomSquare2.PolyPointsToLines())
                    yield return aPoint;

                //yield return this.FrontBottomLeft2;
                //yield return this.BackTopRight2;
                //yield return this.FrontBottomRight2;
                //yield return this.BackTopLeft2;
            }
        }
    }

    //public sealed class CBeyoundSpaceSpriteData : CSpriteData
    //{
    //    internal CBeyoundSpaceSpriteData(CServiceLocatorNode aParent, CTileBuilder aTileBuilder, CTileDescriptor aTileDescriptor) :  base(aParent, aTileBuilder, aTileDescriptor)
    //    {
    //        this.Init();
    //        this.Build();
    //    }
    //    public override T Throw<T>(Exception aException)
    //        => aException.Throw<T>();
    //    protected override void OnBuild()
    //    {
    //    }
    //    internal override ISprite NewSprite()
    //        => this.NewSprite(this);

    //    public override CVector3Dbl WorldPos => this.World.GetWorldPos(this.AbsoluteCubeCoordinates);
    //}

    //internal sealed class CBeyoundSpaceTileDescriptor : CQuadrantTileDescriptor
    //{
    //    #region ctor
    //    internal CBeyoundSpaceTileDescriptor(CServiceLocatorNode aParent, CTileBuilder aTileBuilder) : base(aParent, aTileBuilder)
    //    {
    //        this.BeyoundSpaceSprite = new CBeyoundSpaceSpriteData(this, aTileBuilder, this);
    //    }
    //    public override T Throw<T>(Exception aException)
    //        => throw aException;
    //    #endregion
    //    private readonly CBeyoundSpaceSpriteData BeyoundSpaceSprite;
    //    protected override void OnBuild()
    //    {
    //    }

    //}

}
