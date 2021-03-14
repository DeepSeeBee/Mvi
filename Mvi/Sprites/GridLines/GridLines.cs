using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Cube.Mvi;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Value;
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

namespace CharlyBeck.Mvi.Sprites.GridLines
{
    public sealed class CGridLinesModel : CModel
    {
        internal CGridLinesModel(CServiceLocatorNode aParent) : base(aParent)
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

    public sealed class CGridLinesSprite : CSprite
    {
        internal CGridLinesSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.QuadrantValue = CValue.GetStaticValue<CBoolValue>(this.World, QuadrantValueDeclaration);
            this.PlattformSpriteEnum = CPlatformSpriteEnum.GridLines;

            this.Init();
        }

        internal override void Build(CSpriteBuildArgs a)
        {
            base.Build(a);

            this.WorldPos = this.GetWorldPos(this.TileCubePos.Value);
        }
        public CGridLinesModel CubeModel => this.World.Models.GridLinesModel;

        #region Values
        [CMemberDeclaration]
        internal static readonly CBoolDeclaration QuadrantValueDeclaration = new CBoolDeclaration
            ( CValueEnum.Global_GridLines, new Guid("4af89c99-2734-4c25-a8d5-9417a2d17b77"), true, CStaticParameters.Value_QuadrantGridLines);
        public readonly CBoolValue QuadrantValue;
        internal override bool Visible => base.Visible && this.QuadrantValue.Value;
        #endregion    
    }

    internal sealed class CGridLinesQuadrantContent : CQuadrantContent
    {
        #region ctor
        internal CGridLinesQuadrantContent(CServiceLocatorNode aParent) :base(aParent)
        {
        }
        #endregion

        internal override void AllocateStaticSprites()
        {
            base.AllocateStaticSprites();
            this.GridLinesSprite = this.SolarSystemSpriteManager.AllocateGridLinesSpriteNullable();
            this.SpritesM = this.GridLinesSprite is object
                          ? new CSprite[] { this.GridLinesSprite }
                          : Array.Empty<CSprite>()
                          ;
        }

        private CGridLinesSprite GridLinesSprite;
        private CSprite[] SpritesM;
        public override IEnumerable<CSprite> Sprites => this.SpritesM;

        internal override void Build(CQuadrantBuildArgs a)
        {
            this.GridLinesSprite.Build(a);
        }

    }

    internal sealed class CCubeSpriteManager  : CSinglePoolSpriteManager<CGridLinesSprite>
    {
        internal CCubeSpriteManager(CServiceLocatorNode aParent):base(aParent)
        {
            this.Init();
        }
        protected override void Init()
        {
            base.Init();

            var aEdgeLen = CStaticParameters.Cube_Size * 2 + 1;
            var aCubeQuadrantCount = aEdgeLen * aEdgeLen * aEdgeLen;
            var aQuadrantCount = aCubeQuadrantCount * CStaticParameters.Cube_Count;
            var aLock = true;
            this.Reserve(aQuadrantCount, aLock);
        }
        protected override CGridLinesSprite NewSprite()
            => new CGridLinesSprite(this);

        internal CGridLinesSprite AllocateCubeNullable()
            => this.AllocateSpriteNullable();
    }

}
