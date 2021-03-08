using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Cube.Mvi;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Feature;
using CharlyBeck.Mvi.Sprites.Asteroid;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.ServiceLocator;
using Mvi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Sprites.Cube
{
    public sealed class CCubeModel : CModel
    {
        internal CCubeModel(CServiceLocatorNode aParent) : base(aParent)
        {

            this.FrontSquare = new CVector3Dbl[] { this.FrontBottomLeft, this.FrontTopLeft, this.FrontTopRight, this.FrontBottomRight };
            this.BackSquare = new CVector3Dbl[] { this.BackBottomLeft, this.BackTopLeft, this.BackTopRight, this.BackBottomRight };
            this.TopSquare = new CVector3Dbl[] { this.FrontTopLeft, this.BackTopLeft, this.BackTopRight, this.FrontTopRight };
            this.BottomSquare = new CVector3Dbl[] {this.FrontBottomLeft,this.BackBottomLeft,this.BackBottomRight,this.FrontBottomRight };
            this.LeftSquare = new CVector3Dbl[] { this.FrontBottomLeft, this.FrontTopLeft, this.BackTopLeft, this.BackBottomLeft };
            this.RightSquare = new CVector3Dbl[] {this.FrontBottomRight,this.FrontTopRight,this.BackTopRight,this.BackBottomRight };

            this.FrontSquareLines = this.FrontSquare.PolyPointsToLines().ToArray();
            this.BackSquareLines = this.BackSquare.PolyPointsToLines().ToArray();
            this.TopSquareLines = this.TopSquare.PolyPointsToLines().ToArray();
            this.BottomSquareLines = this.BottomSquare.PolyPointsToLines().ToArray();
            this.LeftSquareLines = this.LeftSquare.PolyPointsToLines().ToArray();
            this.RightSquareLines = this.RightSquare.PolyPointsToLines().ToArray();

            this.LineList = Array.Empty<CVector3Dbl>()
                            .Concat(this.FrontSquareLines)
                            .Concat(this.BackSquareLines)
                            .Concat(this.TopSquareLines)
                            .Concat(this.BottomSquareLines)
                            .Concat(this.LeftSquareLines)
                            .Concat(this.RightSquareLines)
                            .ToArray()
                            ;
        }

        public readonly CVector3Dbl Center = new CVector3Dbl(0.5d, 0.5d, 0.5d);
        public readonly CVector3Dbl FrontBottomLeft = new CVector3Dbl(0, 0, 0);
        public readonly CVector3Dbl FrontTopLeft = new CVector3Dbl(0, 1d, 0);
        public readonly CVector3Dbl FrontTopRight = new CVector3Dbl(1d, 1d, 0);

        public readonly CVector3Dbl FrontBottomRight = new CVector3Dbl(1d, 0, 0);
        public readonly CVector3Dbl BackBottomLeft = new CVector3Dbl(0, 0, 1d);
        public readonly CVector3Dbl BackTopLeft = new CVector3Dbl(0, 1d, 1d);
        public readonly CVector3Dbl BackTopRight = new CVector3Dbl(1d, 1d, 1d);
        public readonly CVector3Dbl BackBottomRight = new CVector3Dbl(1d, 0, 1d);
        public readonly CVector3Dbl[] FrontSquare;
        public readonly CVector3Dbl[] BackSquare;
        public readonly CVector3Dbl[] LeftSquare;
        public readonly CVector3Dbl[] RightSquare;
        public readonly CVector3Dbl[] BottomSquare;
        public readonly CVector3Dbl[] TopSquare;
        public readonly CVector3Dbl[] FrontSquareLines;
        public readonly CVector3Dbl[] BackSquareLines;
        public readonly CVector3Dbl[] LeftSquareLines;
        public readonly CVector3Dbl[] RightSquareLines;
        public readonly CVector3Dbl[] BottomSquareLines;
        public readonly CVector3Dbl[] TopSquareLines;
        public readonly CVector3Dbl[] LineList;

    }

    public sealed class CCubeSprite : CSprite
    {
        internal CCubeSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.QuadrantFeature = CFeature.Get(this.World, QuadrantFeatureDeclaration);
        }

        internal override void Build(CSpriteBuildArgs a)
        {
            base.Build(a);
        }
        internal override CPlatformSpriteEnum PlattformSpriteEnum => CPlatformSpriteEnum.Cube;
        public CCubeModel CubeModel => this.World.Models.CubeModel;

        #region Features
        [CFeatureDeclaration]
        internal static readonly CFeatureDeclaration QuadrantFeatureDeclaration = new CFeatureDeclaration(new Guid("4af89c99-2734-4c25-a8d5-9417a2d17b77"), "Quadrant.GridLines", CStaticParameters.Feature_QuadrantGridLines);
        public readonly CFeature QuadrantFeature;
        internal override bool Visible => base.Visible && this.QuadrantFeature.Enabled;
        #endregion
        public override CVector3Dbl WorldPos => this.GetWorldPos(this.TileCubePos.Value);
    
    }

    internal sealed class CCubeQuadrant : CSpaceQuadrant
    {
        #region ctor
        internal CCubeQuadrant(CServiceLocatorNode aParent) :base(aParent)
        {
            this.CubeSprite = new CCubeSprite(aParent);
            this.SpritesM = new CSprite[] { this.CubeSprite };
        }
        #endregion

        private readonly CCubeSprite CubeSprite;
        private readonly CSprite[] SpritesM;
        public override IEnumerable<CSprite> Sprites => this.SpritesM;

        internal override void Build(CQuadrantBuildArgs a)
        {
            base.Build(a);
            this.CubeSprite.Build(a);
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
