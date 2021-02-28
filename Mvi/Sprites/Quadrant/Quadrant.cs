using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Feature;
using CharlyBeck.Mvi.Sprites.Bumper;
using CharlyBeck.Mvi.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Sprites.Quadrant
{

    internal abstract class CQuadrantTileDescriptor : CWorldTileDescriptor
    {
        #region ctor
        internal CQuadrantTileDescriptor(CTileBuilder aTileBuilder) : base(aTileBuilder)
        {
            this.QuadrantSpriteData = new CQuadrantSpriteData(aTileBuilder, this);
        }
        #endregion

        internal readonly CQuadrantSpriteData QuadrantSpriteData;

        internal override void Draw()
        {
            base.Draw();
            this.QuadrantSpriteData.Draw();
        }
    }
    public sealed class CQuadrantSpriteData : CSpriteData
    {
        internal CQuadrantSpriteData(CTileBuilder aTileBuilder, CTileDescriptor aTileDescriptor) : base(aTileBuilder, aTileDescriptor)
        {
            var aTile = aTileBuilder.Tile;
            var aWorld = aTileBuilder.World;
            var aSize2 = aWorld.EdgeLen;

            this.Coordinates2 = aWorld.GetWorldPos(aTile);
            this.Size2 = aSize2;

            this.FrontBottomLeft2 = new CVector3Dbl(this.Coordinates2.x, this.Coordinates2.y, this.Coordinates2.z);
            this.FrontTopLeft2 = new CVector3Dbl(this.Coordinates2.x, this.Coordinates2.y + aSize2, this.Coordinates2.z);
            this.FrontTopRight2 = new CVector3Dbl(this.Coordinates2.x + aSize2, this.Coordinates2.y + aSize2, this.Coordinates2.z);
            this.FrontBottomRight2 = new CVector3Dbl(this.Coordinates2.x + aSize2, this.Coordinates2.y, this.Coordinates2.z);

            this.BackBottomLeft2 = new CVector3Dbl(this.Coordinates2.x, this.Coordinates2.y, this.Coordinates2.z + aSize2);
            this.BackTopLeft2 = new CVector3Dbl(this.Coordinates2.x, this.Coordinates2.y + aSize2, this.Coordinates2.z + aSize2);
            this.BackTopRight2 = new CVector3Dbl(this.Coordinates2.x + aSize2, this.Coordinates2.y + aSize2, this.Coordinates2.z + aSize2);
            this.BackBottomRight2 = new CVector3Dbl(this.Coordinates2.x + aSize2, this.Coordinates2.y, this.Coordinates2.z + aSize2);

            this.QuadrantFeature = CFeature.Get(this.World, QuadrantFeatureDeclaration);

            this.Init();
        }

        #region Features
        [CFeatureDeclaration]
        internal static readonly CFeatureDeclaration QuadrantFeatureDeclaration = new CFeatureDeclaration(new Guid("4af89c99-2734-4c25-a8d5-9417a2d17b77"), "Quadrant");
        public readonly CFeature QuadrantFeature;
        #endregion
        public override CVector3Dbl WorldPos => this.World.GetWorldPos(this.AbsoluteCubeCoordinates);
        internal override ISprite NewSprite()
           => this.NewSprite<CQuadrantSpriteData>(this);
        internal override int ChangesCount => 0;

        public readonly CVector3Dbl Coordinates2;
        public readonly double Size2;

        public readonly CVector3Dbl FrontBottomLeft2;
        public readonly CVector3Dbl FrontTopLeft2;
        public readonly CVector3Dbl FrontTopRight2;
        public readonly CVector3Dbl FrontBottomRight2;
        public readonly CVector3Dbl BackBottomLeft2;
        public readonly CVector3Dbl BackTopLeft2;
        public readonly CVector3Dbl BackTopRight2;
        public readonly CVector3Dbl BackBottomRight2;

        internal override bool Visible => this.World.QuadrantGridLines;
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

    public sealed class CBeyoundSpaceSpriteData : CSpriteData
    {
        internal CBeyoundSpaceSpriteData(CTileBuilder aTileBuilder, CTileDescriptor aTileDescriptor) : base(aTileBuilder, aTileDescriptor)
        {
            this.Init();
        }
        internal override ISprite NewSprite()
            => this.NewSprite(this);

        public override CVector3Dbl WorldPos => this.World.GetWorldPos(this.AbsoluteCubeCoordinates);
    }

    internal sealed class CBeyoundSpaceTileDescriptor : CQuadrantTileDescriptor
    {
        #region ctor
        internal CBeyoundSpaceTileDescriptor(CTileBuilder aTileBuilder) : base(aTileBuilder)
        {
            this.BeyoundSpaceSprite = new CBeyoundSpaceSpriteData(aTileBuilder, this);
        }
        #endregion
        private readonly CBeyoundSpaceSpriteData BeyoundSpaceSprite;

    }

    internal sealed class CInSpaceTileDescriptor : CQuadrantTileDescriptor
    {
        #region ctor
        internal CInSpaceTileDescriptor(CTileBuilder aTileBuilder) : base(aTileBuilder)
        {
            var aWorldGenerator = aTileBuilder.WorldGenerator;
            var aWorld = aTileBuilder.World;
            var aBumperCount = aWorldGenerator.NextInteger(aWorld.TileBumperCountMin, aWorld.TileBumperCountMax);
            var aBumpers = new CBumperSpriteData[aBumperCount];
            for (var aIdx = 0; aIdx < aBumperCount; ++aIdx)
                aBumpers[aIdx] = new CBumperSpriteData(aTileBuilder, this);
            this.Bumpers = aBumpers;
        }
        internal override void Draw()
        {
            base.Draw();

            foreach (var aBumper in this.Bumpers)
                aBumper.Draw();
        }
        internal CBumperSpriteData[] Bumpers;
        #endregion
    }
}
