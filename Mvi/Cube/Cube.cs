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

namespace CharlyBeck.Mvi.Cube
{
    using CMoveTile = Tuple<CCubePos, bool, CTileDataLoadProxy>;


    internal abstract class CRandomGenerator : CBase
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
        internal void Begin(CTile aTile)
        {
            this.CheckNotPending();
            var aSeed = aTile.Cube.GetRandomSeed(aTile);
            this.Random = new Random((int)aSeed);
        }

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

        internal double NextDouble(CDoubleRange r)
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
            if (aNext > aMin)
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


        internal T NextEnum<T>()
           => this.NextItem(typeof(T).GetEnumValues().Cast<T>().ToArray());

        internal T[] NextEnums<T>()
           => this.NextItems(typeof(T).GetEnumValues().Cast<T>().ToArray());

        internal CVector3Dbl NextVector3Dbl(double aXyz)
            => new CVector3Dbl(this.NextDouble(aXyz), this.NextDouble(aXyz), this.NextDouble(aXyz));
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
        internal CCubePosKey GetKey(Int64 aEdgeLen) => new CCubePosKey(this, aEdgeLen);
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

    public sealed class CCoordinates<T> : IEnumerable<T> // TODO_OPT
    {
        internal CCoordinates() : this(new T[] { })
        {
        }
        public CCoordinates(params T[] aIds)
        {
            this.Ids = aIds;
        }
        internal static CCoordinates<T> NewN(T aValue, int aSize)
        {
            var aIds = new T[aSize];
            for (var aIdx = 0; aIdx < aSize; ++aIdx)
            {
                aIds[aIdx] = aValue;
            }
            return new CCoordinates<T>(aIds);
        }

        internal CCoordinates<T1> To<T1>(Func<T, T1> aConvert)
            => new CCoordinates<T1>((from aCoord in this select aConvert(aCoord)).ToArray());

        public override int GetHashCode()
            => this[0].GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is CCoordinates<T>)
            {
                var rhs = (CCoordinates<T>)obj;
                return this.Ids.ElementsAreEqual<T>(rhs.Ids);
            }
            return false;
        }

        internal CCoordinates(CCoordinates<T> aParent, T aId)
        {
            this.Ids = aParent.Ids.Concat(new T[] { aId }).ToArray();
        }
        internal CCoordinates(CCoordinates<T> aRhs)
        {
            this.Ids = aRhs.Ids.ToArray();
        }

        public T this[int aIdx]
        {
            get => this.Ids[aIdx];
            set => this.Ids[aIdx] = value;
        }

        internal readonly T[] Ids;

        internal int Length => this.Ids.Length;
        internal int CommonLength(CCoordinates<T> aRhs)
           => this.Length; // TODO

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
           => this.Ids.Cast<T>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
           => this.Ids.GetEnumerator();
        public override string ToString()
           => "(" + (from aId in this select aId.ToString()).JoinString("|") + ")";

    }


    internal static class CCoordinatesExtensions
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


    internal abstract class CDimension : CBase, IDrawable
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
        public override void Load()
        {
            base.Load();
            foreach (var aDim in this.LoadedSubDimensions)
                aDim.Load();

            if (this.HasData)
            {
                var aWait = true;
                this.DimensionDataLoadProxy.LoadAsyncable(aWait);
            }
        }
        void IDrawable.Unload()
        {
            if (this.HasData)
            {
                this.DimensionDataLoadProxy.Unload();
            }
        }
        #endregion
        #region SubDimensions
        internal abstract bool SubDimensionsIsDefined { get; }
        internal CDimensionLoadProxy[] DimensionLoadProxys { get; private set; }

        internal IEnumerable<CDimension> GetSubDimensions(bool aLoaded)
           => from aSubDimensionLoadProxy in this.DimensionLoadProxys select aSubDimensionLoadProxy.GetLoadable(aLoaded);
        internal IEnumerable<CDimension> LoadedSubDimensions => this.GetSubDimensions(true);
        internal IEnumerable<CDimension> LoadeableSubDimensions => this.GetSubDimensions(false);
        private bool AllocateSubDimensionsDone;
        internal void AllocateSubDimensions(int aSize)
        {
            if (this.AllocateSubDimensionsDone)
            {
                throw new InvalidOperationException();
            }
            else
            {
                var aSubs = new CDimensionLoadProxy[aSize];
                for (int aIdx = 0; aIdx < aSize; ++aIdx)
                {
                    var aSubCoordinates = new CDimPos(this.DimensionCoordinates, aIdx); // new CCoordinates<Int64>(this.DimensionCoordinates, (Int64)aIdx);
                    aSubs[aIdx] = this.NewSubDimensionLoadProxy(aSubCoordinates);
                }
                this.DimensionLoadProxys = aSubs;
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
        #region Draw
        internal virtual void Draw()
        {
            foreach (var aDim in this.LoadeableSubDimensions)
                aDim.Draw();
            if (this.HasData)
            {
                this.DimensionDataLoadProxy.Draw();
            }
        }
        void IDrawable.Draw() => this.Draw();
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
            foreach (var aSubDimension in this.LoadedSubDimensions)
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

        internal CDimensionLoadProxy NewSubDimensionLoadProxy(CDimPos aDimPos)
           => new CDimensionLoadProxy(this, aDimPos);

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
        #region Load
        internal void BeginLoadAsyncable()
        {
            foreach (var aDimensionLoadProxy in this.DimensionLoadProxys)
            {
                aDimensionLoadProxy.BeginLoadAsyncable();
            }
            if (this.HasData)
            {
                this.DimensionDataLoadProxy.BeginLoadAsyncable();
            }
        }
        internal void WaitLoaded()
        {
            foreach (var aDimensionLoadProxy in this.DimensionLoadProxys)
            {
                aDimensionLoadProxy.WaitUntilLoaded();
            }
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
        #region Data
        internal virtual bool HasData { get; }
        internal virtual CLoadProxy DimensionDataLoadProxy => this.Throw<CLoadProxy>(new NotImplementedException());
        #endregion
        #region LeafDimensions
        internal bool IsLeafContainer => this.DimensionIdx == 1;
        internal virtual IEnumerable<CLeafDimension> GetLeafDimensions(bool aLoaded)
        {
            foreach (var aSubDimension in this.GetSubDimensions(aLoaded))
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

    internal sealed class CLoadingDimension : CDimension
    {
        #region ctor
        internal CLoadingDimension(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Init();
        }
        protected override void Init()
        {
            base.Init();
        }
        public override T Throw<T>(Exception aException)
           => aException.Throw<T>();
        #endregion
        #region DimensionIdx
        internal override int DimensionIdx => this.ParentDimension.DimensionIdx - 1;
        #endregion
        internal override bool SubDimensionsIsDefined => false;
        internal override bool IsRoot => false;
        internal override int AllocateSubDimensionsSize => 0;
    }

    internal interface IDrawable
    {
        void Draw();
        void Unload();
    }


    internal delegate void CWaitAction();

    internal abstract class CWorker : CServiceLocatorNode
    {
        internal CWorker(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Thread = new Thread(this.Run);
            this.Thread.Priority = ThreadPriority.Lowest;
            this.Thread.Start();
        }
        private Thread Thread;
        private readonly List<CWorkItem> WorkItems = new List<CWorkItem>();
        private void CheckNotRunning()
        { if (this.IsRunning) throw new InvalidOperationException(); }

        private void Enqueue(CWorkItem aWorkItem)
        {
            this.Wait();
            this.CheckNotRunning();
            this.WorkItems.Add(aWorkItem);               
        }

        private readonly AutoResetEvent DoWorkEvent = new AutoResetEvent(false);
        private bool StopRequested;
        private void Run()
        {
            while(!this.StopRequested)
            {
                this.DoWorkEvent.WaitOne();
                lock(this.WorkingSyncObject) // Race Condition
                {
                    foreach (var aItem in this.WorkItems)
                        aItem.DoWork();
                    this.WorkItems.Clear();
                    this.IsRunning = false;
                }
            }
        }


        internal CWaitAction Enqueue(Action aDoWorkAction)
        {
            this.Wait();
            var aWorkItem = new CWorkItem(this, aDoWorkAction);
            this.Enqueue(aWorkItem);
            return new CWaitAction(aWorkItem.Wait);
        }

        private sealed class CWorkItem
        {
            internal CWorkItem(CWorker aWorker, Action aDoWorkAction)
            {
                this.Worker = aWorker;
                this.DoWorkAction = aDoWorkAction;
            }
            private readonly AutoResetEvent FinishedEvent = new AutoResetEvent(false);
            private readonly CWorker Worker;
            private readonly Action DoWorkAction;
            internal void DoWork()
            {
                this.DoWorkAction();
                this.FinishedEvent.Set(); // RaceCondition2
            }
            internal void Wait()
            {
                if(!this.Worker.IsRunning)
                {
                    this.Worker.Start();
                    Thread.Sleep(20); // RaceCondition2
                }
                this.FinishedEvent.WaitOne();
            }
        }

        private bool IsRunning;
        private readonly object WorkingSyncObject = new object();

        private void Wait()
        {
            lock(this.WorkingSyncObject) // Race Condition
            {
            }
        }
        internal void Start()
        {
            this.Wait();
            this.IsRunning = true;
            this.DoWorkEvent.Set();            
        }

        internal void Stop()
        {
            this.StopRequested = true;
            this.Start();
        }
    }

    internal sealed class CTileDataLoadProxyWorker : CWorker
    {
        internal CTileDataLoadProxyWorker(CServiceLocatorNode aParent): base(aParent)
        {
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
    }

    internal abstract class CLoadProxy : CBase
    {
        #region ctor
        internal CLoadProxy(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        protected override void Init()
        {
            base.Init();
            this.ParentDimension = this.ServiceContainer.GetService<CDimension>();
            this.RootDimension = this.ServiceContainer.GetService<CRootDimension>();
        }
        internal void Unload()
        {
            this.Dispose();
        }
        internal bool IsDisposed { get; private set; }
        internal void Dispose()
        {
            if (this.IsDisposed)
                throw new ObjectDisposedException(this.ToString());
            if (this.IsLoading
           || this.IsLoaded)
                this.OnUnload();
            this.IsDisposed = true;
        }
        protected abstract void OnUnload();
        #endregion
        #region Services
        internal CDimension ParentDimension { get; private set; }
        internal CRootDimension RootDimension { get; private set; }
        #endregion
        #region Load
        public void LoadAsyncable(bool aWait)
        {
            if (!this.IsLoaded
            && !this.IsLoading) // hack
            {
                this.BeginLoadAsyncable();
                if (aWait)
                    this.WaitUntilLoaded();
            }
        }
        internal abstract void LoadTemplate();
        public override void Load()
        {
            base.Load();

            if (this.IsLoaded)
            {
               // Hack... Eigentlich:
                // this.Throw(new InvalidOperationException());
            }
            else if (!this.IsLoading)
            {
                this.Throw(new InvalidOperationException());
            }
            else
            {
                this.LoadTemplate();
                this.IsLoading = false;
                this.WaitAction = default;
                this.IsLoaded = true;
            }
        }
        public bool IsLoaded { get; protected set; }
        internal void WaitUntilLoaded()
        {
            if (this.IsLoading)
            {
                this.WaitAction();
            }
        }
        internal abstract bool LoadAsyncIsEnabled { get; }
        internal void BeginLoadAsyncable()
        {
            if (this.IsLoaded)
            {
                this.Throw(new InvalidOperationException());
            }
            else if (this.IsLoading)
            {
                this.Throw(new InvalidOperationException());
            }
            else if (this.LoadAsyncIsEnabled)
            {
                this.IsLoading = true;
                this.WaitAction = this.Worker.Enqueue(delegate () { this.Load(); });
                //if (!DisableTask)
                //{
                //    var aTask = Task.Factory.StartNew(delegate () { (); });
                //    this.LoadAsyncResult = aTask; // TODO-RaceCondition
                //}
            }
            else
            {
                this.IsLoading = true;
                this.Load();
            }
        }
        //private static bool DisableTask = false;
        //private IAsyncResult LoadAsyncResult;
        internal bool IsLoading { get; private set; }
        #endregion
        #region Draw
        internal virtual void Draw()
        {
        }
        #endregion
        #region Worker
        private CWorker WorkerM;
        private CWorker Worker => CLazyLoad.Get(ref this.WorkerM, () => this.NewWorker());
        internal abstract CWorker NewWorker();
        private CWaitAction WaitAction;
        #endregion
    }
    internal abstract class CLoadProxy<TData, TLoadingData, TLoadedData> : CLoadProxy
       where TData : IDrawable
       where TLoadingData : TData
       where TLoadedData : TData
    {
        #region ctor
        internal CLoadProxy(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        protected override void OnUnload()
        {
            this.Loaded.Unload();
            this.IsLoaded = false;
            this.Loaded = default;
        }
        #endregion
        #region Load
        internal abstract TLoadedData NewLoaded();
        internal override void LoadTemplate()
        {
            this.LoadedNullable = this.NewLoaded();
        }
        internal abstract TLoadingData Loading { get; }
        private TLoadedData LoadedNullable;
        private TLoadedData PrivateLoaded
        {
            get
            {
                if (this.LoadedNullable is object)
                    return this.LoadedNullable;
                return this.Throw<TLoadedData>(new InvalidOperationException());
            }
        }
        internal TData LoadedOrLoading => this.IsLoading ? (TData)this.Loading : (TData)this.PrivateLoaded;
        public TLoadedData Loaded
        {
            get
            {
                if (this.IsLoaded)
                    return this.PrivateLoaded;
                else if (!this.IsLoading)
                    this.BeginLoadAsyncable();
                this.WaitUntilLoaded();
                return this.PrivateLoaded;
            }
            protected set
            {
                if (this.IsLoading)
                    throw new InvalidOperationException();
                else if (this.IsLoaded)
                    throw new InvalidOperationException();
                this.LoadedNullable = value;
                this.IsLoaded = true;
            }
        }
        internal TData GetLoadable(bool aLoaded)
           => aLoaded || this.IsLoaded ? (TData)this.Loaded : (TData)this.Loading;
        #endregion
        #region Draw
        internal override void Draw()
        {
            base.Draw();

            if (this.IsLoaded)
            {
                this.Loaded.Draw();
            }
            else
            {
                this.Loading.Draw();
            }
        }
        #endregion
    }

    internal sealed class CDimensionLoadProxy : CLoadProxy<CDimension, CLoadingDimension, CDimension>
    {
        #region ctor
        internal CDimensionLoadProxy(CServiceLocatorNode aParent, CDimPos aDimPos) : base(aParent)
        {
            this.DimPos = aDimPos;
            this.Init();
        }
        protected override void Init()
        {
            base.Init();
            this.LoadingDimension = new CLoadingDimension(this);
        }
        public override T Throw<T>(Exception aException)
           => throw aException;
        #endregion
        #region Loading
        internal override CWorker NewWorker()=> throw new NotImplementedException();
        internal override bool LoadAsyncIsEnabled => false;
        private CLoadingDimension LoadingDimension { get; set; }
        internal override CLoadingDimension Loading => this.LoadingDimension;
        #endregion
        internal override CDimension NewLoaded()
        {
            var aLoadedDimension = this.RootDimension.NewDimension(this, this.DimPos);
            aLoadedDimension.BeginLoadAsyncable();
            return aLoadedDimension;
        }
        internal CDimPos DimPos;
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

    internal abstract class CDimensionData : CBase, IDrawable
    {
        #region ctor
        internal CDimensionData(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        #endregion
        #region Draw
        internal virtual void Draw()
        {
        }
        void IDrawable.Draw() => this.Draw();
        internal abstract void Unload();
        void IDrawable.Unload()
            => this.Unload();
        #endregion
    }

    internal interface ILoadableData { }

    internal abstract class CDimensionDataLoadProxy<TData, TLoadingData, TLoadedData>
    :
       CLoadProxy<TData, TLoadingData, TLoadedData>
       where TData : IDrawable
       where TLoadingData : TData
       where TLoadedData : TData
    {
        #region ctor
        internal CDimensionDataLoadProxy(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        protected override void Init()
        {
            base.Init();
        }
        public override T Throw<T>(Exception aException)
           => aException.Throw<T>();
        #endregion
    }

    internal sealed class CSpriteRegistry : CServiceLocatorNode, IEnumerable<CSpriteData>
    {
        internal CSpriteRegistry(CServiceLocatorNode aParent) : base(aParent)
        {

        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        internal void Unload()
        {
            foreach (var aSpriteData in this.SpriteDatas)
            {
                aSpriteData.Unload();
            }
        }
        private readonly List<CSpriteData> SpriteDatas = new List<CSpriteData>();
        internal void Add(CSpriteData aSpriteData)
            => this.SpriteDatas.Add(aSpriteData);
        public IEnumerator<CSpriteData> GetEnumerator() => this.SpriteDatas.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    internal abstract partial class CTileDescriptor : CBuildable
    {
        internal CTileDescriptor(CServiceLocatorNode aParent, CTileBuilder aTileBuilder) : base(aParent, aTileBuilder)
        {
            this.AbsoluteCubeCoordinates = aTileBuilder.Tile.AbsoluteCubeCoordinates;
        }

        internal readonly CCubePos AbsoluteCubeCoordinates;

        #region SubTileDescriptors
        internal virtual IEnumerable<CTileDescriptor> SubTileDescriptors => Array.Empty<CTileDescriptor>();
        #endregion
        #region SpriteRegistry
        private CSpriteRegistry OwnSpriteRegistryM;
        private CSpriteRegistry OwnSpriteRegistry => CLazyLoad.Get(ref this.OwnSpriteRegistryM, () => new CSpriteRegistry(this));
        #endregion
        internal virtual bool OwnSpriteRegistryIsDefined => false;    
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CSpriteRegistry>(() => this.OwnSpriteRegistryIsDefined ? this.OwnSpriteRegistry : base.ServiceContainer.GetService<CSpriteRegistry>());
            return aServiceContainer;
        }
        #endregion
        internal override void OnUnload()
        {
            base.OnUnload();
            if(this.OwnSpriteRegistryIsDefined)
            {
                this.OwnSpriteRegistry.Unload();
            }
        }
    }


    internal abstract class CTileData : CDimensionData
    {
        #region ctor
        internal CTileData(CServiceLocatorNode aParent) : base(aParent)
        {

        }
        #endregion
    }

    internal sealed class CLoadingTileData : CTileData
    {
        #region ctor
        internal CLoadingTileData(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        public override T Throw<T>(Exception aException)
           => aException.Throw<T>();
        #endregion
        internal override void Unload()
        {
            throw new NotImplementedException();
        }
    }
    internal sealed class CLoadedTileData : CTileData
    {
        #region ctor
        private CLoadedTileData(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Init();
        }

        internal static CLoadedTileData New(CServiceLocatorNode aParent, CTileDescriptor aTileDescriptor)
        {
            var aLoadedTileData = new CLoadedTileData(aParent);
            aLoadedTileData.TileDescriptor = aTileDescriptor;
            return aLoadedTileData;
        }
        public override T Throw<T>(Exception aException)
           => aException.Throw<T>();
        internal override void Unload()
        {
            this.TileDescriptor.Unload();
            this.TileDescriptor = default;
        }
        #endregion

        internal CTileDescriptor TileDescriptor { get; private set; }
        internal CCubePos AbsoluteCubeCoordinates => this.TileDescriptor.AbsoluteCubeCoordinates;

        internal override void Draw()
        {
            base.Draw();
            this.TileDescriptor.Draw();
        }

    }
    internal sealed partial class CTileDataLoadProxy : CDimensionDataLoadProxy<CTileData, CLoadingTileData, CLoadedTileData>
    {
        #region ctor
        private CTileDataLoadProxy(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Init();
        }
        internal static CTileDataLoadProxy New(CServiceLocatorNode aParent, CLoadedTileData aLoadedTileData)
        {
            var aTileDataLoadProxy = new CTileDataLoadProxy(aParent);
            aTileDataLoadProxy.Loaded = aLoadedTileData;
            return aTileDataLoadProxy;
        }
        internal static CTileDataLoadProxy New(CServiceLocatorNode aParent)
        {
            var aTileDataLoadProxy = new CTileDataLoadProxy(aParent);
            return aTileDataLoadProxy;
        }
        protected override void Init()
        {
            base.Init();
            this.LoadingTileData = new CLoadingTileData(this);
        }
        #endregion
        #region Load
        internal override bool LoadAsyncIsEnabled => false;
        internal override CWorker NewWorker() => this.ServiceContainer.GetService<CTileDataLoadProxyWorker>();
        private CLoadingTileData LoadingTileData { get; set; }
        internal override CLoadingTileData Loading => this.LoadingTileData;
        #endregion
    }

    internal sealed partial class CTile : CLeafDimension
    {
        #region ctor
        internal CTile(CServiceLocatorNode aParent) : base(aParent)
        {
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

            this.Cube = this.ServiceContainer.GetService<CCube>();

            this.TileDataLoadProxy = this.NewTileDataLoadProxy();
        }
        public override T Throw<T>(Exception aException) => aException.Throw<T>();
        #endregion
        #region Cube
        internal CCube Cube { get; private set; }
        internal CCubePos RelativeCubeCoordinates
            => this.DimensionCoordinates.GetCubeCoordinates(this.Cube.NewCoords(0), (int)this.Depth);
        internal CCubePos AbsoluteCubeCoordinates
        => this.Cube.TileAbsoluteCoordinates(this.RelativeCubeCoordinates);
        private Int64 GetIndex(CCubePos aCoord)
            => (aCoord.x + aCoord.y * this.Depth + aCoord.z * this.Depth * this.Depth);
        internal Int64 RelativeIndex =>
            this.GetIndex(this.RelativeCubeCoordinates);
        #endregion
        #region TileData
        private CLoadedTileData NewLoadedTileData(CTileDescriptor aTileDescriptor)
           => CLoadedTileData.New(this, aTileDescriptor);
        internal CTileDataLoadProxy NewTileDataLoadProxy()
           => CTileDataLoadProxy.New(this);
        internal CTileDataLoadProxy NewTileDataLoadProxy(CTileDescriptor aTileDescriptor)
            => this.NewTileDataLoadProxy(this.NewLoadedTileData(aTileDescriptor));
        internal CTileDataLoadProxy NewTileDataLoadProxy(CLoadedTileData aTileData)
           => CTileDataLoadProxy.New(this, aTileData);
        internal override bool HasData => true;
        internal CTileDataLoadProxy TileDataLoadProxy { get; set; }

        private void UnloadTileDataLoadProxy()
        {
            this.TileDataLoadProxy.Unload();
            this.TileDataLoadProxy = default;
        }
        internal Action ReplaceTileData()
        {
            //this.UnloadTileDataLoadProxy();
            //var aTileDataLoadProxy = this.NewTileDataLoadProxy();
            //this.TileDataLoadProxy = aTileDataLoadProxy;
            //aTileDataLoadProxy.BeginLoadAsyncable();
            this.UnloadTileDataLoadProxy();
            var aTileDataLoadProxy = this.NewTileDataLoadProxy();
            aTileDataLoadProxy.BeginLoadAsyncable();
            return new Action(delegate () 
            {                
                this.TileDataLoadProxy = aTileDataLoadProxy;                
            });
        }       
        internal Action ReplaceTileData(CTileDescriptor aTileDescriptor)
        {
            this.UnloadTileDataLoadProxy();
            var aTileDataLoadProxy = this.NewTileDataLoadProxy(aTileDescriptor);
            return new Action(delegate ()
            {
             this.TileDataLoadProxy = aTileDataLoadProxy;
            });
        }
        internal CTileData TileData => this.TileDataLoadProxy.Loaded;
        internal CTileDescriptor TileDescriptor => this.TileDataLoadProxy.Loaded.TileDescriptor;
        internal override CLoadProxy DimensionDataLoadProxy => this.TileDataLoadProxy;
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
        internal override int DimensionIdx => 0;
        internal override int AllocateSubDimensionsSize => 0;

        internal CCubePosKey CubePosKey => this.AbsoluteCubeCoordinates.GetKey(this.Cube.Depth);
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
            this.DimensionCoordinates = new CDimPos();
            this.CubePos = this.NewCubeCoordinates();
            this.World = this.ServiceContainer.GetService<CWorld>();
            this.Init();
        }
        private CCubePos NewCubeCoordinates()
            => new CCubePos(0);
        internal static CCube New(CServiceLocatorNode aParent)
        {
            var aCube = new CCube(aParent);
            aCube.Allocate();
            return aCube;
        }
        protected override void Init()
        {
            base.Init();
        }
        public override void Load()
        {
            base.Load();
            foreach (var aTile in this.LoadedLeafDimensions)
            {
                aTile.Load();
            }
        }
        public override T Throw<T>(Exception aException)
        => aException.Throw<T>();
        #endregion
        #region ICube
        void ICube.Draw() => this.Draw();
        CVector3Dbl ICube.WorldPos { set => this.WorldPos = value; }
        void ICube.Update(CVector3Dbl aAvatarPos) => this.Update(aAvatarPos);
        void ICube.Update(CFrameInfo aFrameInfo) => this.Update(aFrameInfo);
        IEnumerable<CSpriteData> ICube.SpriteDatas => this.SpriteDatas;
        IEnumerable<CCubePos> ICube.CubePositions { get { yield return this.CubePos; } }
        #endregion
        internal IEnumerable<CSpriteData> SpriteDatas => from aTile in this.Tiles from aSpriteData in aTile.TileDataLoadProxy.Loaded.TileDescriptor.SpriteRegistry select aSpriteData;
        internal void Update(CVector3Dbl aAvatarPos)
        {
            var aSpriteDatas = this.SpriteDatas;
            foreach (var aSpriteData in aSpriteDatas)
            {
                aSpriteData.UpdateBeforeFrameInfo(aAvatarPos);
            }
        }
        internal void Update(CFrameInfo aFrameInfo)
        {
            var aSpriteDatas = this.SpriteDatas;
            foreach (var aSpriteData in aSpriteDatas)
            {
                aSpriteData.UpdateAfteFrameInfo(aFrameInfo);
            }
        }


        private readonly CWorld World;
        //internal override Int64 Depth => 3u;

        internal override bool IsRoot => true;
        internal override int DimensionIdx => 2;
        internal override int AllocateSubDimensionsSize => (int)this.Depth;
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CCube>(() => this);
            aServiceContainer.AddService<CTileDataLoadProxyWorker>(() => this.LoadProxyWorker);
            return aServiceContainer;
        }
        #endregion
        internal override CDimension NewDimension(CServiceLocatorNode aParent, CDimPos aDimPos)
        {
            switch (aDimPos.Length)
            {
                case 1:
                    return CPlane.New(aParent, aDimPos);
                case 2:
                    return CTile.New(aParent, aDimPos);
                default:
                    throw new ArgumentException();
            }
        }
        internal CCubePos CubePos { get; private set; }
        internal CTile Tile => (from aTile in this.Tiles where aTile.AbsoluteCubeCoordinates == this.CubePos select aTile).First(); // TODO_OPT


        internal CCubePos TileAbsoluteCoordinates(CCubePos aTileRelativeCubeCoordinates)
            => this.CubePos.Add(aTileRelativeCubeCoordinates);
        
        #region Move
        internal CCubePos CenterOffset => this.NewCoords((this.Depth - 1) / 2);

        private readonly Dictionary<CCubePosKey, Tuple<CCubePosKey, CTile, CTileDescriptor>> MoveDic = new Dictionary<CCubePosKey, Tuple<CCubePosKey, CTile, CTileDescriptor>>();
        
        private void MoveToCubeCoordinates(CCubePos aNewCubePos)
        {
            var aSw = new Stopwatch();

            aSw.Restart();
            var aDic = this.MoveDic;
            aDic.Clear();

            var aLeafs1 = (from aLeaf in this.LoadedLeafDimensions.OfType<CTile>() select aLeaf).ToArray();
            var aLeafs2 = (from aLeaf in aLeafs1 select new Tuple<CCubePosKey, CTile, CTileDescriptor>(aLeaf.AbsoluteCubeCoordinates.GetKey(this.Depth), aLeaf, aLeaf.TileDescriptor)).ToArray();
            foreach (var aLeaf in aLeafs2)
                aDic.Add(aLeaf.Item1, aLeaf);
            this.CubePos = aNewCubePos;

            aSw.Stop();
            Debug.Print("Prepare took " + aSw.ElapsedMilliseconds.ToString() + " ms.");

            foreach (var aLeaf in aLeafs1)
            {
                var aAbsIndex = aLeaf.AbsoluteCubeCoordinates;
                var aAbsIndexKey = new CCubePosKey(aAbsIndex, this.Depth);
                var aExists = aDic.ContainsKey(aAbsIndexKey);
                if (aExists)
                {
                    var aOldData = aDic[aAbsIndexKey];
                    var aTileDescriptor = aOldData.Item3;
                    aLeaf.ReplaceTileData(aTileDescriptor)();
                }
                else
                {
                    aLeaf.ReplaceTileData()();
                }
            }
        }

        internal void MoveToCubeCoordinatesOnDemand(CCubePos aAvatarPos)
        {
            var aCubePos = aAvatarPos.Subtract(this.CenterOffset);
            if (aCubePos.IsEqual(this.CubePos))
            {
                // no change
            }
            //else if (!this.World.AvatarAbsCoordIsValid(aNewCubeCoordinates))
            //{
            //    // Einfach durchfliegen... muss anderweitig behandelt werden.
            //}
            else
            {
                System.Diagnostics.Debug.Print("AvatarPos at: " + aAvatarPos.ToString());
                System.Diagnostics.Debug.Print("CubePos at: " + aCubePos.ToString());
                var aStopWatch = new Stopwatch();
                aStopWatch.Start();
                this.MoveToCubeCoordinates(aCubePos);
                aStopWatch.Stop();
                Debug.Print("MoveToCubeCoordinates took " + aStopWatch.ElapsedMilliseconds);
            }
        }

        internal CCubePos NewCoords(Int64 aCoord)
            => new CCubePos(aCoord);

        internal UInt64 GetRandomSeed(CTile aTile)
            => aTile.AbsoluteCubeCoordinates.GetSeed(this.Depth);

        internal CCubePos GetCubePos(CVector3Dbl aWorldPos)
            => aWorldPos.Divide(this.World.EdgeLenAsPos).ToCubePos();
        #endregion
        internal override bool SubDimensionsIsDefined => true;

        #region LoadProxyWorker
        private CTileDataLoadProxyWorker LoadProxyWorkerM;
        private CTileDataLoadProxyWorker LoadProxyWorker => CLazyLoad.Get(ref this.LoadProxyWorkerM, () => new CTileDataLoadProxyWorker(this));

        public IEnumerable<CTile> Tiles => this.LoadedLeafDimensions.OfType<CTile>();

        public CVector3Dbl WorldPos {  set => this.MoveToCubeCoordinatesOnDemand(this.GetCubePos(value)); }
        #endregion
    }

    internal interface ICube
    {
        CVector3Dbl WorldPos { set; }
        void Draw();

        void Update(CVector3Dbl aAvatarPos);
        void Update(CFrameInfo aFrameInfo);
        IEnumerable<CSpriteData> SpriteDatas { get; }
        IEnumerable<CCubePos> CubePositions { get; }

    }
    internal sealed class CMultiverseCube 
    :
        CServiceLocatorNode
    {
        #region ctor
        internal CMultiverseCube(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Cube = CCube.New(aParent);
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        #endregion
        internal readonly CCube Cube;
        internal bool Active;
        internal CVector3Dbl WorldPos { set => this.Cube.WorldPos = value; }
        internal void Draw() => this.Cube.Draw();
        internal void Update(CVector3Dbl aAvatarPos) => this.Cube.Update(aAvatarPos);
        internal void Update(CFrameInfo aFrameInfo) => this.Cube.Update(aFrameInfo);
        internal IEnumerable<CSpriteData> SpriteDatas => this.Cube.SpriteDatas;
        internal IEnumerable<CCubePos> CubePositions { get { yield return this.Cube.CubePos; } }

    }

    internal sealed class CMultiverseCubes  
    : 
        CServiceLocatorNode
    , ICube
    {
        #region ctor
        internal CMultiverseCubes(CServiceLocatorNode aParent) : base(aParent)
        {
            var aItems = new CMultiverseCube[2];
            foreach(var aIdx in Enumerable.Range(0, aItems.Length))
            {
                aItems[aIdx] = new CMultiverseCube(this);
            }
            this.Items = aItems;
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        #endregion
        #region Cubes
        internal readonly CMultiverseCube[] Items;
        internal IEnumerable<CMultiverseCube> ActiveItems => from aCube in this.Items where aCube.Active select aCube;

        internal int CubeIndex { get; set; }

        internal CVector3Dbl WorldPos
        {
            set
            {
                foreach (var aItem in this.ActiveItems)
                    aItem.WorldPos = value;
            }
        }
        #endregion
        internal void Draw()
        {
            foreach (var aItem in this.ActiveItems)
                aItem.Draw();
        }
        #region ICube
        CVector3Dbl ICube.WorldPos { set => this.WorldPos = value; }
        void ICube.Draw() => this.Draw();
        void ICube.Update(CVector3Dbl aAvatarPos)
        {
            foreach (var aCube in this.ActiveItems)
                aCube.Update(aAvatarPos);
        }
        void ICube.Update(CFrameInfo aFrameInfo)
        {
            foreach (var aCube in this.ActiveItems)
                aCube.Update(aFrameInfo);
        }
        IEnumerable<CSpriteData> ICube.SpriteDatas => from aItem in this.ActiveItems from aSpriteData in aItem.SpriteDatas select aSpriteData;
        IEnumerable<CCubePos> ICube.CubePositions => (from aItem in this.ActiveItems select aItem.CubePositions).Flatten();
        #endregion

    }
}
