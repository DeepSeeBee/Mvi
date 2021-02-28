using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using CharlyBeck.Mvi.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using CharlyBeck.Utils3.Enumerables;
using Mvi.Models;
using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Mvi.Internal;
using CharlyBeck.Mvi.Sprites.Quadrant;
using CharlyBeck.Mvi.Sprites.Bumper;
using Microsoft.Xna.Framework;

namespace CharlyBeck.Mvi.Cube
{
    partial class CTileDataLoadProxy
    {
        #region TileBuilder
        private CTileBuilder TileBuilderM;
        internal CTileBuilder TileBuilder => CLazyLoad.Get(ref this.TileBuilderM, () => new CTileBuilder(this));
        internal override CLoadedTileData NewLoaded()
              => this.TileBuilder.NewLoadedTileData(this);
        #endregion
    }
}

namespace CharlyBeck.Mvi.World
{
    public struct CVector3Dbl
    {
        public CVector3Dbl(double xyz) : this(xyz, xyz, xyz) { }
        public CVector3Dbl(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public readonly double x;
        public readonly double y;
        public readonly double z;
        public override string ToString() => this.x.ToString() + "|" + this.y.ToString() + "|" + this.z.ToString() + "|";

        internal double GetLength()
            => (float)Math.Sqrt((this.x * this.x) + (this.y * this.y) + (this.z * this.z));

        internal static CVector3Dbl Min(CVector3Dbl lhs, CVector3Dbl rhs)
            => new CVector3Dbl(Math.Min(lhs.x, rhs.x),
                             Math.Min(lhs.y, rhs.y),
                             Math.Min(lhs.z, rhs.z));
        internal static CVector3Dbl Max(CVector3Dbl lhs, CVector3Dbl rhs)
            => new CVector3Dbl(Math.Max(lhs.x, rhs.x),
                             Math.Max(lhs.y, rhs.y),
                             Math.Max(lhs.z, rhs.z));

        internal double GetDistance(CVector3Dbl rhs)
            => (Max(this, rhs) - Min(this, rhs)).GetLength();

        public static bool operator ==(CVector3Dbl lhs, CVector3Dbl rhs)
            => lhs.x == rhs.x
            && lhs.y == rhs.y
            && lhs.z == rhs.z
            ;

        public override bool Equals(object obj)
        {
            if (obj is CVector3Dbl pos)
                return this == pos;
            return base.Equals(obj);
        }
        public override int GetHashCode()
            => throw new NotImplementedException(); // this.CalcHashCode(3);

        public static bool operator !=(CVector3Dbl lhs, CVector3Dbl rhs)
            => !(lhs == rhs);
        public static CVector3Dbl operator *(CVector3Dbl lhs, CVector3Dbl rhs)
            => new CVector3Dbl(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z);
        public static CVector3Dbl operator /(CVector3Dbl lhs, CVector3Dbl rhs)
            => new CVector3Dbl(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z);
        public static CVector3Dbl operator +(CVector3Dbl lhs, CVector3Dbl rhs)
            => new CVector3Dbl(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
        public static CVector3Dbl operator -(CVector3Dbl lhs, CVector3Dbl rhs)
            => new CVector3Dbl(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);

        internal static CVector3Dbl Parse(string aText)
        {
            var aParts = (from aPart in aText.Split('|') select double.Parse(aPart)).ToArray();
            if (aParts.Length == 3)
            {
                return new CVector3Dbl(aParts[0], aParts[1], aParts[2]);
            }
            throw new ArgumentException();
        }
        internal int CalcHashCode(double aEdgeLen)
            => (int)((this.x + (this.y * aEdgeLen) + this.z * aEdgeLen * aEdgeLen));

        internal CCubePos ToCubePos()
            => new CCubePos((Int64)this.x, (Int64)this.y, (Int64)this.z);

        public double GetRadiansXy()
            => Math.Atan(this.y / this.x);

        public double GetRadiansXz()
            => Math.Atan(this.z / this.x);

        public double GetRadiansYz()
            => Math.Atan(this.y / this.z);



    }

}

namespace CharlyBeck.Mvi.World
{

    internal sealed class CWorldGenerator : CRandomGenerator
    {
        #region ctor
        internal CWorldGenerator(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Init();
        }
        public override T Throw<T>(Exception aException)
           => aException.Throw<T>();


        #endregion
    }
    internal sealed class CTileBuilder : CServiceLocatorNode
    {
        #region ctor
        internal CTileBuilder(CServiceLocatorNode aParent) : base(aParent)
        {
            this.WorldGenerator = new CWorldGenerator(this);
            this.Facade = this.ServiceContainer.GetService<CFacade>();
            this.Init();
        }
        public override T Throw<T>(Exception aException)
           => aException.Throw<T>();
        #endregion
        #region Tile
        private CTile TileM;
        internal CTile Tile => CLazyLoad.Get(ref this.TileM, () => this.ServiceContainer.GetService<CTile>());
        #endregion
        #region TileDescriptor
        internal CTileDescriptor NewTileDescriptor() => CRootTileDescriptor.New(this, this);

        internal CLoadedTileData NewLoadedTileData(CTileDataLoadProxy aTileDataLoadProxy)
        {
          //  this.WorldGenerator.Reset(this.Tile);
           return CLoadedTileData.New(this, this.NewTileDescriptor());
        }
        #endregion
        #region World
        private CWorld WorldM;
        internal CWorld World => CLazyLoad.Get(ref this.WorldM, () => this.ServiceContainer.GetService<CWorld>());
        #endregion
        #region WorldGenerator
        internal readonly CWorldGenerator WorldGenerator;
        #endregion
        #region Facade
        internal CFacade Facade { get; private set; }
        #endregion
    }

    public sealed class CWorld : CServiceLocatorNode
    {
        #region ctor
        internal CWorld(CServiceLocatorNode aParent) : base(aParent)
        {
            this.CubeBorder = 1;
            this.EdgeLen = 1.0d;
            this.EdgeLenAsPos = new CVector3Dbl(this.EdgeLen);
            this.TileBumperCountMin = 5;
            this.TileBumperCountMax = 10;

            //this.TileBumperCountMin = 1;
            //this.TileBumperCountMax = 5;

            this.DefaultBumperQuadrantBumperRadiusMax = new double[] { 
                0.1d, 
                0.01d,
                0.001d,
                0.0001d,
                0.00001d
                }.OrderBy(nr=>nr).ToArray(); // Aufsteigend!
            this.SolarSystemPlanetRadiusMax = new double[] {
                0.1d,
                0.01d,
                0.001d,
                }.OrderBy(nr => nr).ToArray(); // Aufsteigend!
            this.SolarSystemSunRadiusMax = new double[] {
                0.1d,
                }.OrderBy(nr => nr).ToArray(); // Aufsteigend!
            this.BumperGravityRadiusMax = 1.0d;
            this.BumperGravityStrengthMax = 1.0d;
            this.NearBumperSpeedMin = 0.0001;
            this.NearBumperSpeedForRadius0 = 0.001;
            this.SphereScaleCount = 25;
            this.SolarSystemMaxRadius = 0.6;
            this.SolarSystemMinRadius = this.SolarSystemMaxRadius * 0.75;
        }
        public override void Load()
        {
            base.Load();
            this.Cube.Load();
        }
        public override T Throw<T>(Exception aException)
           => aException.Throw<T>();

        //internal readonly Int64 MaxInSpaceCubeCoord;
        //internal readonly Int64 MinInSpaceCubeCoord;
        //internal readonly Int64 MaxCubeCoord;
        ////internal readonly Int64 MinCubeCoord;
        //internal readonly Int64 MinAvatarCoord;
        //internal readonly Int64 MaxAvatarCoord;

        //internal double MinWorldCoord => this.MinCubeCoord * this.TileEdgeLen;
        //internal double MaxWorldCoord => this.MaxCubeCoord * this.TileEdgeLen;

        internal bool IsBeyound(CTile aTile)
        {
            return false;
            //var aCubeCoordinates = aTile.AbsoluteCubeCoordinates;
            //var aIsBorder = (from aCoordinate in aCubeCoordinates
            //                 select aCoordinate <  this.MinInSpaceCubeCoord
            //                 || aCoordinate >  this.MaxInSpaceCubeCoord).Contains(true);
            //return aIsBorder;
        }
        #endregion
        #region Cube
        private CCube CubeM;
        internal CCube Cube => CLazyLoad.Get(ref this.CubeM, () => CCube.New(this));
        #endregion
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CWorld>(() => this);
            aServiceContainer.AddService<CNewBorderFunc>(() => new CNewBorderFunc(()=> this.CubeBorder));
            return aServiceContainer;
        }
        #endregion
        #region Draw
        internal void Draw()
        {
            this.Cube.Draw();
        }
        #endregion
        #region Models
        private CModels ModelsM;
        internal CModels Models => CLazyLoad.Get(ref this.ModelsM, () => new CModels(this));
        #endregion
        public readonly double EdgeLen;
        internal readonly CVector3Dbl EdgeLenAsPos;

        internal CVector3Dbl GetWorldPos(CTile aTile)
           => this.GetWorldPos(aTile.AbsoluteCubeCoordinates);

        internal CVector3Dbl NewCoords(double xyz)
            => new CVector3Dbl(xyz);

        internal CVector3Dbl GetWorldPos(CCubePos aCubeCoordinates)
           => this.GetWorldPos2(aCubeCoordinates); // .Subtract(this.CubeCenterOffset)

        internal CVector3Dbl GetWorldPos2(CCubePos aCubeCoordinates)
            => aCubeCoordinates.ToWorldPos() * this.EdgeLenAsPos;

        public CCubePos GetCubePos(CVector3Dbl aWorldPos)
            => aWorldPos.Divide(this.EdgeLenAsPos).ToCubePos();
        //internal CCubePos CubeCenterOffset
        //    => CCubePos.NewN((this.Cube.Depth - 1) / 2, (int)this.Cube.Depth);
        ////    => new CCubePos((Int64)aWorldPos[0], (Int64)aWorldPos[1], (Int64)aWorldPos[2]); //.Add(this.CubeCenterOffset);

        internal readonly double NearBumperSpeedForRadius0;
        internal readonly int TileBumperCountMin;
        internal readonly int TileBumperCountMax;
        internal readonly double[] DefaultBumperQuadrantBumperRadiusMax;
        internal readonly double[] SolarSystemPlanetRadiusMax;
        internal readonly double[] SolarSystemSunRadiusMax;

        internal readonly double BumperGravityRadiusMax;
        internal readonly double BumperGravityStrengthMax;
        internal readonly double NearBumperSpeedMin;
        internal readonly double SolarSystemMaxRadius;
        internal readonly double SolarSystemMinRadius;
        internal readonly Int64 CubeBorder;

        //internal CWorldPos GetQuadrantPosition(CTile aTile)

        //public bool GetCubeCoordinatesIsDefined(CWorldPos aWorldCoordinates)
        //    => throw new NotImplementedException();
        //=> aWorldCoordinates.x >= this.MinWorldCoord && aWorldCoordinates[0] <= this.MaxWorldCoord
        //&& aWorldCoordinates.y >= this.MinWorldCoord && aWorldCoordinates[1] <= this.MaxWorldCoord
        //&& aWorldCoordinates.z >= this.MinWorldCoord && aWorldCoordinates[2] <= this.MaxWorldCoord;


        public CFrameInfo FrameInfo { get; internal set; }
        public int SphereScaleCount;
        public GameTime GameTime { get; private set; }
        public void Update(CVector3Dbl aAvatarPos, GameTime aGameTime)
        {
            this.GameTime = aGameTime;
            var aSpriteDatas = from aTile in this.Cube.Tiles from aSpriteData in aTile.TileDataLoadProxy.Loaded.TileDescriptor.SpriteRegistry select aSpriteData;
            foreach (var aSpriteData in aSpriteDatas)
            {
                aSpriteData.UpdateBeforeFrameInfo(aAvatarPos);
            }

            this.FrameInfo = new CFrameInfo(this, aAvatarPos);

            foreach(var aSpriteData in this.FrameInfo.SpriteDatas)
            {
                aSpriteData.UpdateAfteFrameInfo(this.FrameInfo);
            }
        }


    }
}


namespace CharlyBeck.Mvi.World
{
    using CSpriteDistance = Tuple<CSpriteData, double>;
    public struct  CFrameInfo
    {
        internal CFrameInfo(CWorld aWorld, CVector3Dbl aWorldPos)
        {
            this.World = default;
            this.WorldPos = default;
            this.SpriteDatas = default;
            this.SpriteDistances = default;
            this.NearestBumperSpriteDataAndDistance = default;
            //this.NearestBumperDistanceToSurface = default;
            //this.NearestBumperIsEntered = default;
            this.NearPlanetSpeedM = default;

            this.World = aWorld;
            this.WorldPos = aWorldPos;
            this.SpriteDatas = (from aTile in aWorld.Cube.Tiles from aSpriteData in aTile.TileDataLoadProxy.Loaded.TileDescriptor.SpriteRegistry select aSpriteData).ToArray();
            this.SpriteDistances = (from aSpriteData in this.SpriteDatas.OfType<CBumperSpriteData>() select new Tuple<CSpriteData, double>(aSpriteData, aSpriteData.DistanceToAvatar)).ToArray().OrderBy(aDist=>aDist.Item2).ToArray();
            this.NearestBumperSpriteDataAndDistance = ( from aSpriteAndDistance in this.SpriteDistances where (aSpriteAndDistance.Item1 is CBumperSpriteData) select new Tuple<CBumperSpriteData, double>((CBumperSpriteData)aSpriteAndDistance.Item1, aSpriteAndDistance.Item2)).FirstOrDefault();
            //this.NearestBumperDistanceToSurface = this.NearestBumperIsDefined ? (this.NearestBumper.AvatarDistanceToSurface) : double.NaN;
            //this.NearestBumperIsEntered = this.NearestBumperIsDefined ? this.NearestBumper.IsBelowSurface : false; // this.NearestBumperDistance < this.NearestBumper.Radius : false;
            //this.NearPlanetSpeed = this.NewNearPlanetSpeed();
        }

        public readonly CWorld World;
        public readonly CVector3Dbl WorldPos;
        public readonly CSpriteData[] SpriteDatas;
        public readonly CSpriteDistance[] SpriteDistances;
        public readonly Tuple<CBumperSpriteData, double> NearestBumperSpriteDataAndDistance;
        public CBumperSpriteData NearestBumper => this.NearestBumperSpriteDataAndDistance.Item1;
       // public double NearestBumperDistance => this.NearestBumperSpriteDataAndDistance.Item2;

       // public readonly bool NearestBumperIsEntered;
       // public readonly double NearestBumperDistanceToSurface;       
        public bool NearestBumperIsDefined => this.NearestBumperSpriteDataAndDistance.Item1 is object;

        public double NearPlanetSpeed { get => CLazyLoad.Get(ref this.NearPlanetSpeedM, this.NewNearPlanetSpeed); }
        public double? NearPlanetSpeedM;
        private double NewNearPlanetSpeed()
        {
            if (!this.NearestBumperIsDefined)
            {
                return 1d; // Strange...
            }
            //else if (this.NearestBumperIsEntered)
            //{
            //    return this.World.NearBumperSpeedMin;
            //}
            //else
            {
                var aDistance =  Math.Abs(this.NearestBumper.AvatarDistanceToSurface);
                var rm = this.World.DefaultBumperQuadrantBumperRadiusMax.Last();
                var rf =  (this.NearestBumper.Radius / rm);
                var s1 = (1d - rf) * this.World.NearBumperSpeedForRadius0;
                var s2 = aDistance;
                var s3 = (s1 + s2) / 2d; // s1 + s2;
                var s = Math.Max(this.World.NearBumperSpeedMin, s3);
                return s;
                //var aDistance = this.NearestBumperDistanceToSurface;
                //var aMinDistance = this.World.NearBumperSpeedMinDistance;
                //var aMaxDistance = this.World.NearBumperSpeedMaxDistance;
                //var aEffectiveDistance = Math.Min(aMaxDistance, Math.Max(aMinDistance, aDistance));
                //var aDistanceForFkt = aEffectiveDistance - aMinDistance;
                //var aDistanceRange1 = aMaxDistance - aMinDistance;
                //var aDistanceFkt = aDistanceForFkt / aDistanceRange1;
                //var aMinSpeed = this.World.NearBumperSpeedMaxDistance;
                //var aDistanceRange2 = 1.0d - this.World.NearBumperSpeedMin;
                //var aSpeed1 = aDistanceFkt * aDistanceRange2; // + this.World.NearBumperSpeedMin;
                //var aExp = (1/aDistance);
                //var aSpeed2 = Exp(aSpeed1, aExp, 0); // Math.Exp(aSpeed1 * aExp) /Math.Pow(Math.E, aExp);
                //return aSpeed2;
            }
        }
        private double Exp(double nr, double mul, int rec)
            => rec < 0 ? nr : Exp(Math.Exp(nr * mul) / Math.Pow(Math.E, mul), mul, rec - 1); 
    }


}