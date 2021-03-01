using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.World;
using Mvi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharlyBeck.Mvi.Models;
using CharlyBeck.Mvi.Extensions;
using CharlyBeck.Utils3.ServiceLocator;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Mvi.Sprites.Quadrant;

using CColoredVertex = System.Tuple<CharlyBeck.Mvi.World.CVector3Dbl, CharlyBeck.Mvi.World.CVector3Dbl>;
using CDoubleRange = System.Tuple<double, double>;
using CIntegerRange = System.Tuple<int, int>;

namespace CharlyBeck.Mvi.Sprites.Bumper
{


    public sealed class CBumperModel : CModel
    {
        internal CBumperModel(CServiceLocatorNode aParent):base(aParent)
        {
            this.Sphere = new CSphere(4, 1.0f, true);
            var aLen = this.World.SphereScaleCount; // 33;
            this.Spheres = (from aIdx in Enumerable.Range(0, aLen) select new CSphere(aIdx + 1, 1.0d, true)).ToArray();
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        public readonly COctaeder Octaeder = new COctaeder(0.01);
        public readonly CSphere Sphere;
        public readonly CSphere[] Spheres;
        public int GetSphereIdx(double aDistanceToSurface)
        {
            var aBase = 4;
            var aStartScaleAt= 0.1; // TODO -  in cworld ablegen 
            var aMaxScaleAt = 0.001;
            if (aDistanceToSurface > aStartScaleAt)
                return aBase;
            else if(aDistanceToSurface < 0d)
            {
                return this.Spheres.Length - 1;
            }
            else
            {
                var d = Math.Max(aMaxScaleAt, aDistanceToSurface);
                var r = aStartScaleAt - aMaxScaleAt;
                var f = 1 - (d - aMaxScaleAt) / r;
                var i = (int)(((double)this.Spheres.Length-1) * f);
                return Math.Max(aBase, i);
            }
        }
    }

    public sealed class CDefaultBumperSpriteData : CBumperSpriteData
    {
        internal CDefaultBumperSpriteData(CServiceLocatorNode aParent, CTileBuilder aTileBuilder, CTileDescriptor aTileDescriptor) : base(aParent, aTileBuilder, aTileDescriptor)
        {
            this.Init();
            this.Build();
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();

        protected override CVector3Dbl GenerateOriginalWorldPos()
            => this.GenerateDefaultWorldPos();

        internal override CDoubleRange BumperRadiusMax => this.World.DefaultBumperQuadrantBumperRadiusMax;
        public override string CategoryName => "Bumper";
    }

    public abstract class CBumperSpriteData : CSpriteData
    {
        private static readonly CEnums<CAccelerateEnum> AllAccelerateEnums = new CEnums<CAccelerateEnum>();

        internal CBumperSpriteData(CServiceLocatorNode aParent, CTileBuilder aTileBuilder, CTileDescriptor aTileDescriptor) : base(aParent, aTileBuilder, aTileDescriptor)
        {
            this.Id = ++NewId;
        }
        private static ulong NewId;
        internal readonly ulong Id;
        internal abstract CDoubleRange BumperRadiusMax { get; }
        protected override void OnBuild()
        {

            var aTileBuilder = this.TileBuilder;
            var aWorldGenerator = aTileBuilder.WorldGenerator;
            var aWorld = aTileBuilder.World;
            var aTile = aTileBuilder.Tile;
            var aTileAbsoluteCubeCoordinates = aTile.AbsoluteCubeCoordinates;
            this.OriginalWorldPos = this.GenerateOriginalWorldPos();
            //this.Coordinates2 = aWorld.GetWorldPos(aTileAbsoluteCubeCoordinates)
            //var aRadiusMax = aWorldGenerator.NextItem(this.BumperRadiusMax);
            this.Radius = this.BuildRadius(); // aWorldGenerator.NextDouble(aRadiusMax);
            this.Color = aWorldGenerator.NextWorldPos();
            this.AccelerateEnums = aWorldGenerator.NextItems<CAccelerateEnum>(AllAccelerateEnums.Fields);
            this.GravityIsEnabled = this.AccelerateEnums.Contains(CAccelerateEnum.Gravity);
            this.GravityRadius = aWorldGenerator.NextDouble(aWorld.BumperGravityRadiusMax);
            this.GravityStrength = aWorldGenerator.NextDouble(aWorld.BumperGravityStrengthMax);
            this.GravityRepulsive = aWorldGenerator.NextBoolean();
            this.AccelerateIsEnabled = this.AccelerateEnums.Contains(CAccelerateEnum.Accelerate);
            this.AccelerateHasVector = aWorldGenerator.NextBoolean();
            this.AccelerateVector = aWorldGenerator.NextWorldPos();
            this.AccelerateStrength = aWorldGenerator.NextDouble(1.0d);
            this.AccelerateIsRepulsive = aWorldGenerator.NextBoolean();
        }
        internal virtual double BuildRadius()
            => this.WorldGenerator.NextDouble(this.BumperRadiusMax);

        protected abstract CVector3Dbl GenerateOriginalWorldPos();
        internal CVector3Dbl GenerateDefaultWorldPos()
            => this.World.GetWorldPos(this.Tile.AbsoluteCubeCoordinates).Add(this.WorldGenerator.NextDouble(this.World.EdgeLenAsPos));
        internal override ISprite NewSprite()
           => this.Facade.NewSprite(this);
        internal override int ChangesCount => (int)CChangeEnum._Count;

        public override CVector3Dbl WorldPos => this.OriginalWorldPos;
        internal CVector3Dbl OriginalWorldPos { get; private set; }

        public double Radius { get; private set; }
        public CVector3Dbl Color { get; private set; }
        internal CAccelerateEnum[] AccelerateEnums { get; private set; }
        public bool GravityIsEnabled { get; private set; }
        public double GravityRadius { get; private set; }
        public double GravityStrength { get; private set; }
        public bool GravityRepulsive { get; private set; }
        public bool AccelerateIsEnabled { get; private set; }
        public bool AccelerateHasVector { get; private set; }
        public CVector3Dbl AccelerateVector { get; private set; }
        public double AccelerateStrength { get; private set; }
        public bool AccelerateIsRepulsive { get; private set; }

        public CBumperModel BumperModel => this.World.Models.BumperModel;

        internal enum CChangeEnum
        {
            _Count
        }

        public abstract string CategoryName {get;}
        public string VmCategoryName => this.CategoryName;

        internal enum CAccelerateEnum
        {
            Gravity,
            Accelerate
        }

        internal override bool ModelIsDefined => true;

        public double AvatarDistanceToSurface { get; private set; }
        public bool IsBelowSurface { get; set; }
        public bool IsNearestBumperToAvatar { get; set; }

        internal override CModel NewModel() => this.World.Models.BumperModel;

        internal override void UpdateBeforeFrameInfo(CVector3Dbl aAvatarPos)
        {
            base.UpdateBeforeFrameInfo(aAvatarPos);

            this.AvatarDistanceToSurface = this.DistanceToAvatar - this.Radius;

                
            this.IsBelowSurface =  this.DistanceToAvatar < this.Radius;
        }
        internal override void UpdateAfteFrameInfo(CFrameInfo aFrameInfo)
        {
            base.UpdateAfteFrameInfo(aFrameInfo);

            this.IsNearestBumperToAvatar = aFrameInfo.NearestBumperIsDefined
                                        && aFrameInfo.NearestBumper.RefEquals<CBumperSpriteData>(this);
        }
    }


    internal sealed class CBumpersTileDescriptor : CQuadrantTileDescriptor
    {
        #region ctor
        public CBumpersTileDescriptor(CServiceLocatorNode aParent, CTileBuilder aTileBuilder) : base(aParent, aTileBuilder)
        {
            this.Init();
            this.Build();
        }
        public override T Throw<T>(Exception aException)
           => throw aException;
        #endregion

        protected override void OnBuild()
        {
            base.OnBuild();
            var aTileBuilder = this.TileBuilder;
            var aWorldGenerator = aTileBuilder.WorldGenerator;
            var aWorld = aTileBuilder.World;
            var aBumperCount = aWorldGenerator.NextInteger(aWorld.TileBumperCountMin, aWorld.TileBumperCountMax);
            var aBumpers = new CBumperSpriteData[aBumperCount];
            for (var aIdx = 0; aIdx < aBumperCount; ++aIdx)
                aBumpers[aIdx] = new CDefaultBumperSpriteData(this, aTileBuilder, this);
            this.Bumpers = aBumpers;
        }
        protected override void OnDraw()
        {
            base.OnDraw();

            foreach (var aBumper in this.Bumpers)
                aBumper.Draw();
        }
        internal CBumperSpriteData[] Bumpers;
    }
}
