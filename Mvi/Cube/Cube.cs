using CharlyBeck.Utils3.Enumerables;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using CharlyBeck.Utils3.Strings;
using CharlyBeck.Mvi.World;
using System.Diagnostics;
using System.Threading;
using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Mvi.Internal;
using CDoubleRange = System.Tuple<double, double>;
using CIntegerRange = System.Tuple<int, int>;
using CharlyBeck.Mvi.Sprites.SolarSystem;
using CharlyBeck.Mvi.Sprites.Asteroid;
using CharlyBeck.Mvi.Value;
using Utils3.Asap;
using CharlyBeck.Mvi.Sprites.Bumper;
using CharlyBeck.Mvi.Sprites.Cube;
using CharlyBeck.Mvi.CubeMvi;

namespace CharlyBeck.Mvi.Cube
{
    using CTimeSpanRange = Tuple<TimeSpan, TimeSpan>;

    // using CMoveTile = Tuple<CCubePos, bool, CTileDataLoadProxy>;
    internal delegate CVector3Dbl CGetWorldPosByCubePosFunc(CCubePos aCubePos);

    internal sealed class CRandomGenerator : CBase
    {
        #region ctor
        internal CRandomGenerator(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Init();
        }
        public override T Throw<T>(Exception aException)
           => throw aException;
        #endregion
        private Random Random 
        { 
            get; 
            set; 
        }
        private bool IsPending => this.Random is object;

        internal CCubePos CubePos => throw new NotImplementedException();
        internal void Begin(UInt64 aSeed)
        {
            this.CheckNotPending();
            this.Random = new Random((int)aSeed);
        }
        internal void Begin()
            => this.Begin((UInt64)DateTime.Now.Ticks);
        internal void End()
        {
            this.CheckPending();
            this.Random = default;
        }
        private void CheckNotPending()
        {
            if (this.IsPending)
                throw new InvalidOperationException();
        }
        private void CheckPending()
        {
            if (!this.IsPending)
                throw new InvalidOperationException();

        }

        internal double NextFromDoubleRange(CDoubleRange r)
            => this.NextDouble(r.Item1, r.Item2);

        internal CVector3Dbl NextWorldPos()
           => new CVector3Dbl(this.NextDouble(), this.NextDouble(), this.NextDouble());
        internal CVector3Dbl NextDouble(CVector3Dbl aMultiplier)
           => this.NextWorldPos().Multiply(aMultiplier);
        internal double NextDouble()
            => this.Random.NextDouble();
        internal double NextDouble(double aMin, double aMax)
            => aMin + this.NextDouble()  * (aMax - aMin);
        internal double NextDouble(double aMultiplier)
           => this.NextDouble() * aMultiplier;
        internal int NextInteger(int aMin, int aMax)
        {
            int aNext;
            var d = this.NextDouble();
            var r = aMax - aMin + 1; // +1 weil fast nie genau 1.0 rauskommt.
            var i = (int)(d * (double)r);
            aNext = aMin + i;
            if (aNext > aMax)
                aNext = aMax; // für den fall, das tatsächlich genau 1.0 rausgekommen ist.
            return aNext;
        }
        internal bool NextBoolean()
           => this.NextBoolean(0.5d);
        internal bool NextBoolean(double aPropability)
           => this.NextDouble() > aPropability;
        internal T NextItem<T>(T[] aItems)
           => aItems[this.NextInteger(0, aItems.Length - 1)];
        internal T[] NextItems<T>(T[] aItems)
        {
            var aCount = (double)aItems.Length;
            var aPropability = 1.0d / aCount;
            return (from aItem in aItems where this.NextBoolean(aPropability) select aItem).ToArray();
        }

        internal TimeSpan NextFromTimeSpanRange(CTimeSpanRange aRange)
            => TimeSpan.FromMilliseconds(this.NextFromDoubleRange(new CDoubleRange(aRange.Item1.TotalMilliseconds, aRange.Item2.TotalMilliseconds)));

        internal T NextEnum<T>()
           => this.NextItem(typeof(T).GetEnumValues().Cast<T>().ToArray());

        internal T[] NextEnums<T>()
           => this.NextItems(typeof(T).GetEnumValues().Cast<T>().ToArray());

        internal CVector3Dbl NextVector3Dbl(double aXyz)
            => new CVector3Dbl(this.NextDouble(aXyz), this.NextDouble(aXyz), this.NextDouble(aXyz));

        internal Int64 NextInt64(Int64 aMin, Int64 aMax)
        { 
            // TODO-use 2x NextInteger + byte swapping to cover whole range
            var i = unchecked((uint)this.NextInteger(0, int.MaxValue));
            var range = unchecked((UInt64)(aMax - aMin));
            var r1 = unchecked((UInt64)i) % range;
            var r2 = aMin + unchecked((Int64) r1);
            return r2;
        }

        internal CCubePos NextCubePos()
            =>new CCubePos(this.NextInt64(CCubePos.Min.x, CCubePos.Max.x),
                           this.NextInt64(CCubePos.Min.y, CCubePos.Max.y),
                           this.NextInt64(CCubePos.Min.z, CCubePos.Max.z) );

    }

    internal abstract class CRootDimension : CDimension
    {
        internal CRootDimension(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CRootDimension>(() => this);
            return aServiceContainer;
        }
        internal abstract CDimension NewDimension(CServiceLocatorNode aParent, CDimPos aDimPos);
        #endregion        
    }
    
    public struct CDimPos
    {
        internal CDimPos(Int64? c0, Int64? c1)
        {
            this.c0 = c0;
            this.c1 = c1;
        }

        internal CDimPos(CDimPos aParent, Int64 c)
        {
            switch(aParent.Length)
            {
                case 0:
                    this.c0 = c;
                    this.c1 = default(Int64?);
                    break;
                case 1:
                    this.c0 = aParent.c0;
                    this.c1 = c;
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        internal readonly Int64? c0;
        internal readonly Int64? c1;
        internal int Length => this.c1.HasValue ? 2 : this.c0.HasValue ? 1 : 0;
    }
    internal struct CCubePosKey
    {
        internal CCubePosKey(CCubePos aCubePos, Int64 aEdgeLen)
        {
            this.CubePos = aCubePos;
            this.HashCode = aCubePos.CalcHashCode(aEdgeLen);
            this.EdgeLen = aEdgeLen;
        }

        private readonly CCubePos CubePos;
        private readonly int HashCode;
        private readonly Int64 EdgeLen;

        public override string ToString() => this.CubePos.ToString();
        public override int GetHashCode()
            => this.HashCode;
        public override bool Equals(object obj)
        {
            if (obj is CCubePosKey)
            {
                var rhs = (CCubePosKey)obj;
                return this.EdgeLen == rhs.EdgeLen
                    && this.CubePos == rhs.CubePos;
            }
            return false;
        }
    }
    public struct CCubePos
    {
        internal CCubePos(Int64 xyz) : this(xyz, xyz, xyz) { }
        internal CCubePos(Int64 x, Int64 y, Int64 z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        internal readonly Int64 x;
        internal readonly Int64 y;
        internal readonly Int64 z;


        internal static readonly CCubePos Min = new CCubePos(-CStaticParameters.Cube_Pos_Max);
        internal static readonly CCubePos Max = new CCubePos(CStaticParameters.Cube_Pos_Max);

        public override string ToString() => this.x.ToString() + "|" + this.y.ToString() + "|" + this.z.ToString() + "|";
        public static bool operator ==(CCubePos lhs, CCubePos rhs)
            => lhs.x == rhs.x
            && lhs.y == rhs.y
            && lhs.z == rhs.z
            ;

        public override bool Equals(object obj)
        {
            if (obj is CCubePos pos)
                return this == pos;
            return base.Equals(obj);
        }
        public override int GetHashCode()
            => throw new NotImplementedException(); // this.CalcHashCode(3);

        public static bool operator !=(CCubePos lhs, CCubePos rhs)
            => !(lhs == rhs);
        public static CCubePos operator* (CCubePos lhs, CCubePos rhs)
            => new CCubePos(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z);
        public static CCubePos operator /(CCubePos lhs, CCubePos rhs)
            => new CCubePos(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z);
        public static CCubePos operator +(CCubePos lhs, CCubePos rhs)
            => new CCubePos(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
        public static CCubePos operator -(CCubePos lhs, CCubePos rhs)
            => new CCubePos(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);

        internal static CCubePos Parse(string aText)
        {
            var aParts = (from aPart in aText.Split('|') select Int64.Parse(aPart)).ToArray();
            if(aParts.Length == 3)
            {
                return new CCubePos(aParts[0], aParts[1], aParts[2]);
            }
            throw new ArgumentException();
        }
        internal int CalcHashCode(Int64 aEdgeLen)
            => (int)((this.x + (this.y * aEdgeLen) + this.z * aEdgeLen * aEdgeLen));

        internal CVector3Dbl ToWorldPos()
            => new CVector3Dbl((double)this.x, (double)this.y, (double)this.z);

        internal UInt64 GetSeed(Int64 aCubeTileCount)
            => (UInt64)(this.x + (this.y * aCubeTileCount) + (this.z * aCubeTileCount * aCubeTileCount)); // TODO: Das gibt bei negativen coordinaten dieselben seeds wie bei positiven.
        internal CCubePosKey GetKey(Int64 aCubeDepth) => new CCubePosKey(this, aCubeDepth);
        //{
        //    var aSeed = default(Int64);
        //    var aLen = (Int64)aCoordinates.Length;
        //    for (Int64 aIdx = 0; aIdx < aLen; ++aIdx)
        //    {
        //        var aMul1 = Pow(aDepth, (Int64)aIdx);
        //        var aMul2 = aIdx == 0 ? 1 : aCoordinates[(int)(aIdx - 1)];
        //        var aOffset = (Int64)(aMul1 * aMul2);
        //        aSeed = aOffset + aCoordinates[(int)aIdx];
        //    }
        //    return aSeed;
        //}
    }

    public static class CCoordinatesExtensions
    {

        internal static Int64 Pow2(this Int64 aValue)
            => aValue * aValue;

        internal static Int64 Pow(this Int64 aBase, Int64 aExponent)
        {
            Int64 aResult = 1;
            for (Int64 aIdx = 0; aIdx < aExponent; ++aIdx)
            {
                aResult = (Int64)aResult * aBase;
            }
            return aResult;
        }
        internal static CCubePos Subtract(this CCubePos aLhs, CCubePos aRhs) => aLhs - aRhs;
        internal static CCubePos Add(this CCubePos aLhs, CCubePos aRhs) => aLhs + aRhs;
        internal static CVector3Dbl Add(this CVector3Dbl aLhs, CVector3Dbl aRhs) => aLhs + aRhs;
        internal static CVector3Dbl Multiply(this CVector3Dbl aLhs, CVector3Dbl aRhs) => aLhs * aRhs;
        internal static CVector3Dbl Divide(this CVector3Dbl aLhs, CVector3Dbl aRhs) => aLhs / aRhs;
        internal static CVector3Dbl Subtract(this CVector3Dbl aLhs, CVector3Dbl aRhs) => aLhs - aRhs;
        internal static CVector3Dbl Normalize(this CVector3Dbl v)
        {
            var len = Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
            return new CVector3Dbl(v.x / len, v.y / len, v.z / len);
        }
        internal static CVector3Dbl Abs(this CVector3Dbl v)
            => new CVector3Dbl(Math.Abs(v.x), Math.Abs(v.y), Math.Abs(v.z));
        internal static CVector3Dbl Sign(this CVector3Dbl v)
            => new CVector3Dbl(Math.Sign(v.x), Math.Sign(v.y), Math.Sign(v.z));

        internal static CCubePos GetCubeCoordinates(this CDimPos aDimensionCoordinates, CCubePos aCubeCoordinates, int aEdgeLength)
        {
            var aBaseX = aCubeCoordinates.x;
            var aBaseY = aCubeCoordinates.y;
            var aBaseZ = aCubeCoordinates.z;
            var aOffsetX = aDimensionCoordinates.c1;
            var aOffsetZ = aDimensionCoordinates.c0;
            var aX1 = aOffsetX % ((Int64)aEdgeLength);
            var aOffsetY = (aOffsetX - aX1) / ((Int64)aEdgeLength);
            var aX = aX1 + aBaseX;
            var aY = aBaseY + aOffsetY;
            var aZ = aBaseZ + aOffsetZ;
            return new CCubePos((Int64)aX, (Int64)aY, (Int64)aZ);
        }
        public static bool IsEqual(this CCubePos aLhs, CCubePos aRhs)
            => aLhs == aRhs;


        public static double ToRadians(this double degree)
            => (degree * Math.PI) / 180.0;
        public static double ToDegrees(this double radians)
            => radians * 180 / Math.PI;
    }

    internal delegate CQuadrant CNewQuadrantFunc(CServiceLocatorNode aParent);

    internal struct CQuadrantBuildArgs
    {
        internal CQuadrantBuildArgs(CRandomGenerator aRanomdGenerator, 
                                    CCubePos aTileCubePos, 
                                    CVector3Dbl aTileWorldPos, 
                                    Func<CSprite, CSpritePersistentData> aGetSpritePersistentDataFunc,
                                    Func <CSprite, int> aNewSpritePersistentId)
        {
            this.RandomGenerator = aRanomdGenerator;
            this.TileCubePos = aTileCubePos;
            this.TileWorldPos = aTileWorldPos;
            this.GetSpritePersistentDataFunc = aGetSpritePersistentDataFunc;
            this.NewSpritePersistentId = aNewSpritePersistentId;
        }
        internal readonly CRandomGenerator RandomGenerator;
        internal readonly CCubePos TileCubePos;
        internal readonly CVector3Dbl TileWorldPos;
        internal readonly Func<CSprite, int> NewSpritePersistentId;
        internal readonly Func<CSprite, CSpritePersistentData> GetSpritePersistentDataFunc;
    }

    internal abstract class CQuadrant  : CReuseable
    {
        internal CQuadrant(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Cube = this.ServiceContainer.GetService<CCube>();
        }
        internal readonly CCube Cube;
        internal CCubePos? TileCubePos { get; private set; }

        protected override void OnEndUse()
        {
            base.OnEndUse();

            this.TileCubePos = default;
            this.QuadrantPersistentDataM = default;
            this.LastSpritePersistentDataId = default;
        }

        internal virtual void Build(CQuadrantBuildArgs a)
        {
            this.ResetLAstSpritePersistentData();
            this.TileCubePos = a.TileCubePos;
        }

        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            //aServiceContainer.AddService<CGetSpritePersistentDataFunc>(() => new CGetSpritePersistentDataFunc(aSprite => this.QuadrantPersistentData.GetSpritePersistentData(aSprite)));
            return aServiceContainer;
        }
        #region Persistency
        private CQuadrantPersistentData QuadrantPersistentDataM;
        internal CQuadrantPersistentData QuadrantPersistentData => CLazyLoad.Get(ref this.QuadrantPersistentDataM, () => this.Cube.CubePersistentData.GetQuadrantPersistentData(this.TileCubePos.Value.GetKey(this.Cube.Depth)));
        private int? LastSpritePersistentDataId;
        internal int NewSpritePersistentDataId()
        {
            this.LastSpritePersistentDataId = this.LastSpritePersistentDataId.Value + 1;
            return this.LastSpritePersistentDataId.Value;
        }
        internal void ResetLAstSpritePersistentData()
        {
            this.LastSpritePersistentDataId = 0;
        } 
        #endregion
    }
    internal abstract class CDimension : CBase//, IDrawable
    {
        #region ctor
        internal CDimension(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        protected override void Init()
        {
            base.Init();
            this.RootDimension = this.ServiceContainer.GetService<CRootDimension>();
            this.ParentDimensionNullable = this.ParentServiceLocatorNode.ServiceContainer.GetServiceNullable<CDimension>();
        }
        #endregion
        #region SubDimensions
        internal abstract bool SubDimensionsIsDefined { get; }
        internal CDimension[] Dimensions { get; private set; }
        private bool AllocateSubDimensionsDone;
        internal void AllocateSubDimensions(int aSize)
        {
            if (this.AllocateSubDimensionsDone)
            {
                throw new InvalidOperationException();
            }
            else
            {
                 var aSubs = new CDimension[aSize];
                for (int aIdx = 0; aIdx < aSize; ++aIdx)
                {
                    var aDimPos = new CDimPos(this.DimensionCoordinates, aIdx); 
                    var aDimension = this.RootDimension.NewDimension(this, aDimPos);
                    aSubs[aIdx] = aDimension;
                }
                this.Dimensions = aSubs;
                this.Size = aSize;
                this.AllocateSubDimensionsDone = true;
            }
        }
        private void AllocateSubDimensions()
        {
            this.AllocateSubDimensions(this.AllocateSubDimensionsSize);
        }
        internal abstract int AllocateSubDimensionsSize { get; } 
        #endregion
        private int PositionM;
        internal int Position
        {
            get => this.PositionM;
            set
            {
                this.PositionM = value;
                this.UpdateCubePositions();
            }
        }
        internal virtual void UpdateCubePositions()
        {
            foreach (var aSubDimension in this.Dimensions)
            {
                aSubDimension.UpdateCubePositions();
            }
        }
        internal abstract int DimensionIdx { get; }
        internal int Size { get; private set; }
        internal IEnumerable<CDimension> DimensionPath
        {
            get
            {
                if (this.ParentDimensionIsDefined)
                    foreach (var aElement in this.ParentDimension.DimensionPath)
                        yield return aElement;
                yield return this;
            }
        }
        internal CRootDimension RootDimension { get; private set; }

        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CDimension>(() => this);
            return aServiceContainer;
        }
        #endregion
        #region IsLeaf
        internal virtual bool IsLeaf => false;
        internal abstract bool IsRoot { get; }
        #endregion
        #region Index
        private CDimPos? CoordinatesM;
        internal CDimPos DimensionCoordinates
        {
            get
            {
                if (this.CoordinatesM.HasValue)
                    return this.CoordinatesM.Value;
                return this.Throw<CDimPos>(new InvalidOperationException());
            }
            set
            {
                if (this.CoordinatesM.HasValue)
                    throw new InvalidOperationException();
                this.CoordinatesM = value;
            }
        }
        #endregion
        #region Allocate
        private bool AllocateDone;
        protected virtual void AllocateTemplate()
        {
            this.AllocateSubDimensions();
        }
        internal void Allocate()
        {
            if (this.AllocateDone)
            {
                throw new InvalidOperationException();
            }
            else
            {
                this.AllocateTemplate();
                this.AllocateDone = true; ;
            }
        }
        #endregion
        #region LeafDimensions
        internal bool IsLeafContainer => this.DimensionIdx == 1;
        internal virtual IEnumerable<CLeafDimension> GetLeafDimensions(bool aLoaded)
        {
            foreach (var aSubDimension in this.Dimensions)
                foreach (var aLeafDimension in aSubDimension.GetLeafDimensions(aLoaded))
                    yield return aLeafDimension;
        }
        internal IEnumerable<CLeafDimension> LoadedLeafDimensions => this.GetLeafDimensions(true);
        internal IEnumerable<CLeafDimension> LoadableLeafDimensions => this.GetLeafDimensions(false);
        #endregion
        #region ParentDimension
        private CDimension ParentDimensionNullable;
        internal CDimension ParentDimension
        {
            get
            {
                if (this.ParentDimensionNullable is object)
                    return this.ParentDimensionNullable;
                return this.Throw<CDimension>(new InvalidOperationException());
            }
        }
        internal bool ParentDimensionIsDefined => this.ParentDimensionNullable is object;
        #endregion
        #region Depth
        private Int64? DepthM;
        internal Int64 Depth => CLazyLoad.Get(ref this.DepthM, this.NewCubeDepth);
        private Int64 NewCubeDepth()
        {
            var aBorder = this.ServiceContainer.GetService<CNewBorderFunc>()();
            var aDepth = aBorder * 2 + 1;
            return aDepth;
        }
        #endregion
    }

    internal delegate Int64 CNewBorderFunc();

    internal abstract class CNodeDimension : CDimension
    {
        #region ctor
        internal CNodeDimension(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        protected override void Init()
        {
            base.Init();
        }
        #endregion
        internal override bool SubDimensionsIsDefined => true;
        internal override bool IsRoot => false;
    }


    internal abstract class CLeafDimension : CNodeDimension
    {
        #region ctor
        internal CLeafDimension(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        #endregion
        #region Leaf
        internal override bool IsLeaf => true;
        internal override bool SubDimensionsIsDefined => false;
        #endregion
        #region LeafDimensions
        internal override IEnumerable<CLeafDimension> GetLeafDimensions(bool aLoaded)
        {
            yield return this;
        }
        #endregion
    }

    internal sealed partial class CTile : CLeafDimension
    {
        #region ctor
        internal CTile(CServiceLocatorNode aParent) : base(aParent)
        {
            this.NewQuadrantFunc = this.ServiceContainer.GetService<CNewQuadrantFunc>();
            this.Cube = this.ServiceContainer.GetService<CCube>();
 

            this.Init();
        }
        internal static CTile New(CServiceLocatorNode aParent, CDimPos aCoordinates)
        {
            var aTile = new CTile(aParent)
            {
                DimensionCoordinates = aCoordinates
            };
            aTile.Allocate();
            return aTile;
        }
        protected override void Init()
        {
            base.Init();

        }
        public override T Throw<T>(Exception aException) => aException.Throw<T>();
        #endregion
        #region Cube
        internal CCube Cube { get; private set; }
        internal CCubePos RelativeCubeCoordinates
            => this.DimensionCoordinates.GetCubeCoordinates(this.Cube.NewCoords(0), (int)this.Depth);
        internal CCubePos TileCubePos
        => this.Cube.GetTileCubePos(this.RelativeCubeCoordinates);
        private Int64 GetIndex(CCubePos aCoord)
            => (aCoord.x + aCoord.y * this.Depth + aCoord.z * this.Depth * this.Depth);
        internal Int64 RelativeIndex =>
            this.GetIndex(this.RelativeCubeCoordinates);
        #endregion
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CTile>(() => this);
            return aServiceContainer;
        }
        #endregion
        internal bool ContainsAvatar => this.TileCubePos == this.Cube.CubePos;

        internal override int DimensionIdx => 0;
        internal override int AllocateSubDimensionsSize => 0;

        #region MultiverseCube
        private CWormholeCube MultiVerseCubeM;
        private CWormholeCube MultiVerseCube => CLazyLoad.Get(ref this.MultiVerseCubeM, () => this.ServiceContainer.GetService<CWormholeCube>());
        #endregion
        #region WorldPos
        internal CVector3Dbl WorldPos => this.MultiVerseCube.GetWorldPos(this.TileCubePos);
        #endregion
        #region Quadrant
        private CNewQuadrantFunc NewQuadrantFunc;
        private CQuadrant QuadrantM;
        internal CQuadrant Quadrant 
        {
            get => CLazyLoad.Get(ref this.QuadrantM, () => this.QuadrantM = this.NewQuadrantFunc(this.Cube));
            //set
            //{
            //    this.QuadrantM = value;
            //    this.BuildIsDone = false;
            //}
        }
        internal void SetQuadrant(CQuadrant aQuadrant, bool aDeallocateOld, bool aBuildIsDone)
        {
            if(!object.Equals(aQuadrant, this.QuadrantM))
            {
                if(this.QuadrantM is object)
                {
                    if(aDeallocateOld)
                    {
                        this.QuadrantM.Deallocate();
                    }
                    this.QuadrantM = default;
                }
                this.QuadrantM = aQuadrant;
                this.BuildIsDone = aBuildIsDone;
            }
        }
        internal bool BuildIsDone { get; private set; }
        internal void Build(UInt64 aSeed)
        {
            var aCube = this.Cube;
            var aRandomGenerator = aCube.RandomGenerator;
            aRandomGenerator.Begin(aSeed);
            try
            {
                var aTileCubePos = this.TileCubePos;
                var aTileWorldPos = this.GetWorldPosByCubePos(aTileCubePos);
                var aQuadrant = this.Quadrant;
                var aNewPersistentIdFunc = new Func<CSprite, int>(aSprite =>
                {
                    var aId = aQuadrant.NewSpritePersistentDataId();
                    return aId;
                });
                var aGetSpritePersistentDataFunc = new Func<CSprite, CSpritePersistentData>(aSprite => this.Quadrant.QuadrantPersistentData.GetSpritePersistentData(aSprite));
                var aQuadrantBuildArgs = new CQuadrantBuildArgs(aRandomGenerator, aTileCubePos, aTileWorldPos, aGetSpritePersistentDataFunc, aNewPersistentIdFunc);
                this.Quadrant.Build(aQuadrantBuildArgs);
            }
            finally
            {
                aRandomGenerator.End();
            }
            this.BuildIsDone = true;
        }
        #endregion
        #region GetWorldPosByCubePos
        private CGetWorldPosByCubePosFunc GetWorldPosByCubePosM;
        private CGetWorldPosByCubePosFunc GetWorldPosByCubePos => CLazyLoad.Get(ref this.GetWorldPosByCubePosM, () => 
        this.ServiceContainer.GetService<CGetWorldPosByCubePosFunc>());

        #endregion

    }

    internal sealed class CPlane : CNodeDimension
    {
        #region ctor
        internal CPlane(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Init();
        }
        internal static CPlane New(CServiceLocatorNode aParent, CDimPos aCoordinates)
        {
            var aPlane = new CPlane(aParent)
            {
                DimensionCoordinates = aCoordinates
            };
            aPlane.Allocate();
            return aPlane;
        }
        public override T Throw<T>(Exception aException) => aException.Throw<T>();
        #endregion
        internal override int DimensionIdx => 1;
        internal override int AllocateSubDimensionsSize => (int)this.Depth.Pow2();
    }

    internal sealed class CCube 
    : 
        CRootDimension
    ,   ICube
    {
        #region ctor
        private CCube(CServiceLocatorNode aParent) : base(aParent)
        {
            this.SpritePool = new CSpritePool(this);
            this.RandomGenerator = new CRandomGenerator(this);
            this.DimensionCoordinates = new CDimPos();
            this.CubePos = this.NewCubeCoordinates();
            this.Init();
        }
        private readonly CSpritePool SpritePool;

        private CCubePos NewCubeCoordinates()
            => new CCubePos(0);
        internal static CCube New(CServiceLocatorNode aParent)
        {
            var aCube = new CCube(aParent);
            aCube.Allocate();
            return aCube;
        }
        public override void Load()
        {
            base.Load();
            foreach (var aTile in this.LoadedLeafDimensions)
            {
                aTile.Load();
            }
        }
        #endregion
        #region ICube
         IEnumerable<CQuadrant> ICube.Quadrants => this.Quadrants;
        IEnumerable < CCubePos > ICube.CubePositions { get { yield return this.CubePos; } }
        void ICube.MoveTo(CCubePos aCubePos, bool aTranslateAvatarPos) => this.MoveTo(aCubePos, aTranslateAvatarPos);
        #endregion
        internal IEnumerable<CQuadrant> Quadrants => from aLeaf in this.GetLeafDimensions(true).OfType<CTile>() select aLeaf.Quadrant;
        internal override bool IsRoot => true;
        internal override int DimensionIdx => 2;
        internal override int AllocateSubDimensionsSize => (int)this.Depth;
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        internal readonly CRandomGenerator RandomGenerator;

        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CCube>(() => this);
            aServiceContainer.AddService<CSpritePool>(() => this.SpritePool);
            return aServiceContainer;
        }
        #endregion
        internal override CDimension NewDimension(CServiceLocatorNode aParent, CDimPos aDimPos)
        {
            switch (aDimPos.Length)
            { // TODO-ObjektPool verwenden.
                case 1:
                    return CPlane.New(aParent, aDimPos);
                case 2:
                    return CTile.New(aParent, aDimPos);
                default:
                    throw new ArgumentException();
            }
        }
        internal CCubePos CubePos { get; private set; }


        internal CCubePos GetTileCubePos(CCubePos aTileRelativeCubeCoordinates)
            => this.CubePos.Add(aTileRelativeCubeCoordinates);

        
        private void Build(CTile aTile)
        {
            var aSeed = this.GetRandomSeed(aTile);
            aTile.Build(aSeed);
        }
        #region Move
        internal CCubePos CenterOffset => this.NewCoords((this.Depth - 1) / 2);

        private IEnumerable<CTile> Tiles => this.GetLeafDimensions(false).OfType<CTile>();
        private Dictionary<CCubePosKey, CTile> NewMoveDic(Dictionary<CCubePosKey, CTile> aDic)
        {
            aDic.Clear();
            var aTiles = this.Tiles;
            foreach (var aTile in aTiles)
            {
                aDic.Add(aTile.TileCubePos.GetKey(this.Depth), aTile);
            }
            return aDic;
        }
        internal void MoveTo(CCubePos aCenteredOrAvatar, bool aTranslateAvatarPos)
        {
            var aNewCubePos = aTranslateAvatarPos
                         ? aCenteredOrAvatar.Subtract(this.CenterOffset)
                         : aCenteredOrAvatar
                         ;
            if (aNewCubePos.IsEqual(this.CubePos))
            {
                // no change
            }
            else
            {
                var aOptimize = false;

                if (!aOptimize)// TODO_OPT
                {
                    this.CubePos = aNewCubePos;
                    foreach(var aTile in this.Tiles)
                    {
                        aTile.SetQuadrant(default, true, false);
                    }

                    foreach (var aTile in this.Tiles)
                    {
                        this.Build(aTile); 
                    }
                }
                else
                {
                    var aDic1 = this.NewMoveDic(this.MoveDic1);
                    this.CubePos = aNewCubePos;
                    var aDic2 = this.NewMoveDic(this.MoveDic2); // TODO_OPT: Mapping in dict2 ist unnöti.
                    foreach(var aKvp in aDic2)
                    {
                        var aTile = aKvp.Value;
                        var aKey = aKvp.Key;

                        if(this.TileInitDone
                        && aDic1.ContainsKey(aKey))
                        {
                            var aOldTile = aDic1[aKey];
                            var aReuseBuildIsDone = aOldTile.BuildIsDone;
                            var aReuseQuadrant = aOldTile.Quadrant;
                            aOldTile.SetQuadrant(default, false, false);
                            var aOverwrittenQuadrant = aTile.Quadrant;
                            var aOverwrittenQuadrantBuildIsDone = aTile.BuildIsDone;
                            aTile.SetQuadrant(aReuseQuadrant, false, aReuseBuildIsDone);
                            aOldTile.SetQuadrant(aOverwrittenQuadrant, false, aOverwrittenQuadrantBuildIsDone);
                            if (!(aTile.Quadrant.TileCubePos is object)) // ?!?! TODO
                            {
                                System.Diagnostics.Debug.Assert(false);
                                this.Build(aTile);
                            }
                        }
                        else
                        {
                            this.Build(aTile);
                        }
                    }

                    this.TileInitDone = true;
                    var aTilesToBuild = this.Tiles.Where(aTile => !aTile.BuildIsDone);
                    foreach(var aTile in aTilesToBuild)
                    {
                        this.Build(aTile);
                        //Debug.Print(aTile.TileCubePos.ToString());
                    }
                }

                //#region InitQuadrants
                //private bool InitQuadrantsDone;
                //private void InitQuadrants()
                //{
                //    if(!this.InitQuadrantsDone)
                //    {
                //        foreach (var aTile in this.GetLeafDimensions(true).OfType<CTile>())
                //        {
                //            if (!aTile.BuildIsDone)
                //            {
                //                this.Build(aTile);
                //            }
                //        }
                //        this.InitQuadrantsDone = true;
                //    }
                //}
                //#endregion
                //private readonly Dictionary<CCubePosKey, Tuple<CCubePosKey, CTile, CTileDescriptor>> MoveDic = new Dictionary<CCubePosKey, Tuple<CCubePosKey, CTile, CTileDescriptor>>();
                //private readonly List<CQuadrant> MoveQuadrantList = new List<CQuadrant>();
                //private readonly List<CTile> MoveTileList = new List<CTile>();
                //private readonly Dictionary<CCubePosKey, Tuple<CCubePosKey, CTile, CQuadrant>> MoveDic = new Dictionary<CCubePosKey, Tuple<CCubePosKey, CTile, CQuadrant>>();
                //private readonly List<CTile> BuildTileList = new List<CTile>();
                //try
                //{
                //    this.MoveTileList.Clear();
                //    this.MoveQuadrantList.Clear();
                //    this.MoveDic.Clear();
                //    this.BuildTileList.Clear();

                //    var aDic = this.MoveDic;
                //    var aLeafs1 = (from aLeaf in this.LoadedLeafDimensions.OfType<CTile>() select aLeaf).ToArray();
                //    var aLeafs2 = (from aLeaf in aLeafs1 select new Tuple<CCubePosKey, CTile, CQuadrant>(aLeaf.TileCubePos.GetKey(this.Depth), aLeaf, aLeaf.Quadrant)).ToArray();
                //    foreach (var aLeaf in aLeafs2)
                //        aDic.Add(aLeaf.Item1, aLeaf);
                //    this.CubePos = aNewCubePos;
                //    foreach (var aLeaf in aLeafs1)
                //    {
                //        var aAbsIndex = aLeaf.TileCubePos;
                //        var aAbsIndexKey = new CCubePosKey(aAbsIndex, this.Depth);
                //        var aExists = aDic.ContainsKey(aAbsIndexKey);
                //        if (aExists)
                //        {
                //            var aOldData = aDic[aAbsIndexKey];
                //            var aQaudrant = aOldData.Item3;
                //            this.MoveQuadrantList.Add(aLeaf.Quadrant);
                //            this.MoveTileList.Add(aOldData.Item2);
                //            aLeaf.Quadrant = aQaudrant;
                //            this.Build(aLeaf);
                //        }
                //        else
                //        {
                //            this.BuildTileList.Add(aLeaf);
                //        }
                //    }
                //    foreach(var aBuildTile in this.BuildTileList)
                //    {
                //        this.Build(aBuildTile);
                //    }

                //    //if (false)
                //    //{
                //    //    foreach (var aTile in this.MoveTileList)
                //    //    {
                //    //        var aQuadrant = this.MoveQuadrantList[0];
                //    //        this.MoveQuadrantList.RemoveAt(0);
                //    //        aTile.Quadrant = aQuadrant;
                //    //        this.Build(aTile);
                //    //    }
                //    //}
                //}
                //finally
                //{
                //    this.MoveTileList.Clear();
                //    this.MoveQuadrantList.Clear();
                //    this.MoveDic.Clear();
                //}
                //this.InitQuadrants();
            }
        }


        // throw new NotImplementedException();
        //var aNewCubePos = aTranslateAvatarPos
        //             ? aCenteredOrAvatar.Subtract(this.CenterOffset)
        //             : aCenteredOrAvatar
        //             ;
        //if (aNewCubePos.IsEqual(this.CubePosAbs))
        //{
        //    // no change
        //}
        //else
        //{
        //    var aDic = this.MoveDic;
        //    aDic.Clear();
        //    var aLeafs1 = (from aLeaf in this.LoadedLeafDimensions.OfType<CTile>() select aLeaf).ToArray();
        //    var aLeafs2 = (from aLeaf in aLeafs1 select new Tuple<CCubePosKey, CTile, CTileDescriptor>(aLeaf.AbsoluteCubeCoordinates.GetKey(this.Depth), aLeaf, aLeaf.TileDescriptor)).ToArray();
        //    foreach (var aLeaf in aLeafs2)
        //        aDic.Add(aLeaf.Item1, aLeaf);
        //    this.CubePosAbs = aNewCubePos;
        //    foreach (var aLeaf in aLeafs1)
        //    {
        //        var aAbsIndex = aLeaf.AbsoluteCubeCoordinates;
        //        var aAbsIndexKey = new CCubePosKey(aAbsIndex, this.Depth);
        //        var aExists = aDic.ContainsKey(aAbsIndexKey);
        //        if (aExists)
        //        {
        //            var aOldData = aDic[aAbsIndexKey];
        //            var aTileDescriptor = aOldData.Item3;
        //            aLeaf.ReplaceTileData(aTileDescriptor)();
        //        }
        //        else
        //        {
        //            aLeaf.ReplaceTileData()();
        //        }
        //    }
        //}
        private readonly Dictionary<CCubePosKey, CTile> MoveDic1 = new Dictionary<CCubePosKey, CTile>();
        private readonly Dictionary<CCubePosKey, CTile> MoveDic2 = new Dictionary<CCubePosKey, CTile>();
        private bool TileInitDone;

        internal CCubePos NewCoords(Int64 aCoord)
            => new CCubePos(aCoord);

        internal UInt64 GetRandomSeed(CTile aTile)
            => aTile.TileCubePos.GetSeed(this.Depth);


        #endregion
        internal override bool SubDimensionsIsDefined => true;
        #region CubePersistentData
        private CCubePersistentData CubePersistentDataM;
        internal CCubePersistentData CubePersistentData => CLazyLoad.Get(ref this.CubePersistentDataM, () => this.ServiceContainer.GetService<CCubePersistentData>());
        #endregion
    }

    internal interface ICube
    {
        void MoveTo(CCubePos aCubePos, bool aTranslateAvatarPos);
        IEnumerable<CCubePos> CubePositions { get; }
        IEnumerable<CQuadrant> Quadrants { get; }
    }
    internal sealed class CWormholeCube 
    :
        CServiceLocatorNode
    {
        #region ctor
        internal CWormholeCube(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Cube = CCube.New(this);
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        #endregion
        internal readonly CCube Cube;
        internal bool Active;
        public IEnumerable<CQuadrant> Quadrants => this.Cube.Quadrants;
        internal IEnumerable<CCubePos> CubePositions { get { yield return this.Cube.CubePos; } }

        internal CCubePos CubePosOffset { get; set; }
        internal void MoveTo(CCubePos aCubePos, bool aTranslateToCenter)
            => this.Cube.MoveTo(aCubePos + this.CubePosOffset, aTranslateToCenter);

        internal CVector3Dbl GetWorldPos(CCubePos aCubePos)
            => (aCubePos - this.CubePosOffset).ToWorldPos() * this.World.EdgeLenAsPos;

        private CWorld WorldM;
        private CWorld World => CLazyLoad.Get(ref this.WorldM, () => this.ServiceContainer.GetService<CWorld>());
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);


        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CWormholeCube>(() => this);
            aServiceContainer.AddService<CGetWorldPosByCubePosFunc>(() => new CGetWorldPosByCubePosFunc(aCubePos => this.GetWorldPos(aCubePos)));
            return aServiceContainer;
        }
        #endregion
    }

    internal sealed class CWormholeCubes  
    : 
        CServiceLocatorNode
    ,   ICube
    {
        #region ctor
        internal CWormholeCubes(CServiceLocatorNode aParent) : base(aParent)
        {
            this.World = this.ServiceContainer.GetService<CWorld>();

            var aItems = new CWormholeCube[2];
            foreach(var aIdx in Enumerable.Range(0, aItems.Length))
            {
                aItems[aIdx] = new CWormholeCube(this);
            }
            aItems.First().Active = true;
            this.Items = aItems;
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        #endregion
        #region Cubes
        internal readonly CWormholeCube[] Items;
        internal IEnumerable<CWormholeCube> ActiveItems => from aCube in this.Items where aCube.Active select aCube;
        internal int CubeIndex { get; set; }
        #endregion
        #region World
        private readonly CWorld World;
        #endregion
        #region ICube

        IEnumerable<CQuadrant> ICube.Quadrants => from aItem in this.ActiveItems from aQuadrant in aItem.Quadrants select aQuadrant;

        IEnumerable < CCubePos > ICube.CubePositions => (from aItem in this.ActiveItems select aItem.CubePositions).Flatten();
        void ICube.MoveTo(CCubePos aCubePos, bool aTranslateToCenter) => this.MoveTo(aCubePos, aTranslateToCenter);
        #endregion

        private CWormholeCube this[int idx]
        {
            get => this.Items[idx];
            set => this.Items[idx] = value;
        }
        internal bool SecondIsActive
        {
            get => this[1].Active;
            set => this[1].Active = value;
        }
        internal void Swap(CBumperSprite aSource)
        {
            if (aSource.Cube.RefEquals<CCube>(this[0].Cube)
            && aSource.IsBelowSurface
            && !aSource.WarpIsActive)
            {
                aSource.WarpIsActive = true;
                this[1].CubePosOffset = aSource.TargetCubePos;
                this[1].MoveTo(new CCubePos(), true);
                this.World.OnWormholeEntered(aSource);
                //this[1].Active = true;
            }

            if(aSource.Cube.RefEquals<CCube>(this[0].Cube)
            && !aSource.IsBelowSurface
            && aSource.WarpIsActive)
            {
                var aFirst = this[0];
                var aSecond = this[1];
                aFirst.Active = false;
                aSecond.Active = true;
                this[0] = aSecond;
                this[1] = aFirst;
                aSource.WarpIsActive = false;
            }
        }
        internal void MoveTo(CCubePos aCubePos, bool aTranslateToCenter)
            => this[0].MoveTo(aCubePos, aTranslateToCenter);
    }

    internal static class CWorldPosToCubePos
    {
        private static bool HasDigits(double c)
            => ((double)(Int64)c) != c;

        private static Int64 GetCubePos(double wp)
        {
            var cp = wp >= 0 || !HasDigits(wp)
                   ? (Int64)wp
                   : ((Int64)wp) - 1
                   ;
            return cp;
        }
        internal static CCubePos GetCubePos(CVector3Dbl aWorldPos, CVector3Dbl aWorldEdgeLen)
        {
            var a1 = aWorldPos.Divide(aWorldEdgeLen);
            var a2 = new CCubePos(GetCubePos(a1.x), GetCubePos(a1.y), GetCubePos(a1.z));
            return a2;
        }
    }
}
