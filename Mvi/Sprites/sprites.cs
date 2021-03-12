using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Cube.Mvi;
using CharlyBeck.Mvi.CubeMvi;
using CharlyBeck.Mvi.Extensions;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Internal;
using CharlyBeck.Mvi.Sfx;
using CharlyBeck.Mvi.Sprites.Asteroid;
using CharlyBeck.Mvi.Sprites.Cube;
using CharlyBeck.Mvi.Sprites.Shot;
using CharlyBeck.Mvi.Sprites.SolarSystem;
using CharlyBeck.Mvi.World;
using CharlyBeck.Mvi.XnaExtensions;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.Reflection;
using CharlyBeck.Utils3.ServiceLocator;
using Microsoft.Xna.Framework;
using Mvi.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils3.Asap;

namespace CharlyBeck.Mvi.Sprites
{
    using CTranslateAndRotate = Tuple<CVector3Dbl, CVector3Dbl>;


    internal struct CSpriteBuildArgs
    {
        internal CSpriteBuildArgs(CQuadrantBuildArgs aQuadrantBuildArgs)
        {
            this.QuadrantBuildArgs = aQuadrantBuildArgs;
        }

        internal readonly CQuadrantBuildArgs QuadrantBuildArgs;

    }

    internal enum CCollisionSourceEnum
    {
        Shot,
        Avatar,
        Gem
    }

    public abstract class CSprite : CReuseable
    {
        internal CSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Facade = this.ServiceContainer.GetService<CFacade>();
            this.TileCubePos = new CCubePos(0);
            this.CollisionSourceIsEnabledArray = new bool[CollisionSourceCount];
        }

        protected override void Init()
        {
            base.Init();
            this.PlatformSprite = this.NewPlatformSprite();
        }


        internal CModels Models => this.World.Models;
        internal CSoundDirectoryEnum? DestroyedSound;
        internal bool DeallocateIsQueued;


        protected override void OnBeginUse()
        {
            base.OnBeginUse();
        }

        internal void Build(CQuadrantBuildArgs a)
        {
            var aSpriteBuildArgs = new CSpriteBuildArgs(a);
            this.Build(aSpriteBuildArgs);
        }

        internal virtual void Build(CSpriteBuildArgs a)
        {
            if (this.PersistencyEnabled)
            {
                this.PersistentId = a.QuadrantBuildArgs.NewSpritePersistentId(this);

            }
            this.SpritePersistentData = a.QuadrantBuildArgs.GetSpritePersistentDataFunc(this);
            //if (this.PersistentId.GetValueOrDefault(0) == 1)
            //{
            //    dynamic d = this.SpritePersistentData.ParentServiceLocatorNode;
            //    var s = d.CubePosKey.ToString();
            //    if(((object)s).ToString() == "0|0|0|")
            //    {
            //        System.Diagnostics.Debug.Assert(true);
            //    }
            //}
            this.TileCubePos = a.QuadrantBuildArgs.TileCubePos;
            this.TileWorldPos = this.GetWorldPos(a.QuadrantBuildArgs.TileCubePos);
            this.Reposition();
        }

        protected override void OnEndUse()
        {
            base.OnEndUse();

            this.WorldPos = default;
            this.IsNearestM = default;
            this.TileCubePos = default;
            this.TileWorldPos = default;
            this.Radius = default;
            this.ObjectIdM = default;
            this.AttractionToAvatarM = default;
            this.SpritePersistentData = default;
            this.PersistentId = default;
        }

        private CObjectId ObjectIdM;
        internal CObjectId ObjectId => CLazyLoad.Get(ref this.ObjectIdM, () => new CObjectId());
        internal bool PlaysFlybySound;
        internal virtual void Draw()
        {
            if (this.Visible)
                this.PlatformSprite.Draw();

        }
        internal CCubePos? TileCubePos { get; private set; }

        internal void OnShotHit(CShotSprite aShotSprite)
        {
            this.Destroyed = true;
            this.World.OnDestroyedByShot(this, aShotSprite);
        }

        public CVector3Dbl? TileWorldPos { get; private set; }
        internal IEnumerable<CTranslateAndRotate> TranslateAndRotates
        {
            get
            {
                yield return new CTranslateAndRotate(this.WorldPos.Value, new CVector3Dbl(0));
            }
        }
        #region WorldPos
        public CVector3Dbl? WorldPos;
        internal CVector3Dbl GetRandomWorldPos(CRandomGenerator aRandomGenerator)
            => this.GetWorldPos(this.TileCubePos.Value).Add(aRandomGenerator.NextDouble(this.World.EdgeLenAsPos));

        #endregion
        public double? Radius { get; set; }

        internal readonly CFacade Facade;
        internal CWorld World => this.Facade.World;
        #region PlatformSpriteEnum
        internal CPlatformSpriteEnum? PlattformSpriteEnum;
        internal CPlatformSprite NewPlatformSprite() => this.Facade.PlatformSpriteFactory.NewPlatformSprite(this);
        internal CPlatformSprite PlatformSprite; 
        #endregion
        internal void Reposition()
            => this.PlatformSprite.Reposition();
        internal virtual int ChangesCount => 0;
        internal virtual bool Visible => true             
            //&& !this.IsHitByShot 
            && !this.Destroyed
            ;

        private bool? IsNearestM;
        public bool IsNearest => CLazyLoad.Get(ref this.IsNearestM, () =>this.World.FrameInfo.SpritesOrderedByDistance.OfType<CSprite>().First().RefEquals<CSprite>(this));
        internal virtual void Update(CFrameInfo aFrameInfo) { }
        public double GetAlpha(CVector3Dbl aCameraPos)
        {
            var d = this.WorldPos.Value.GetDistance(aCameraPos);
            var dmax = (this.Cube.Depth -1) / 2; // ; ; // * ((this.World.Cube.EdgeLength - 1) / 2);
            var df = Math.Min(1d, Math.Max(0d, d / dmax));
            //var em = 1.0d;
            var a = 1d - df; // 1 - (Math.Exp(df * em) / Math.Pow( Math.E ,em));
            return a;
        }

        internal virtual void UpdateAvatarPos()
        {
            this.AvatarPos = this.World.AvatarWorldPos;
            this.DistanceToAvatarM = default;
            this.AvatarIsInCubeM = default;
            this.IsNearestM = default;
            this.WorldMatrix = Matrix.Identity;
            this.AttractionToAvatarM = default;
        }

        internal CVector3Dbl AvatarPos { get; set; }

        private double? DistanceToAvatarM;
        public double DistanceToAvatar { get=>CLazyLoad.Get(ref this.DistanceToAvatarM, () => this.AvatarPos.GetDistance(this.WorldPos.Value)); }
        private bool? AvatarIsInCubeM;
        public bool AvatarIsInQuadrant => CLazyLoad.Get(ref this.AvatarIsInCubeM, () => this.Cube.CubePos == this.TileCubePos.Value);
        internal CFrameInfo FrameInfo => this.World.FrameInfo;

        #region Cube
        private CCube CubeM;
        internal CCube Cube => CLazyLoad.Get(ref this.CubeM, () => this.ServiceContainer.GetService<CCube>());

        public Matrix WorldMatrix { get; protected set; }
        #endregion
        #region GetWorldPos
        private CGetWorldPosByCubePosFunc GetWorldPosByCubePosFuncM;
        private CGetWorldPosByCubePosFunc GetWorldPosByCubePosFunc => CLazyLoad.Get(ref this.GetWorldPosByCubePosFuncM, ()
              => this.ServiceContainer.GetService<CGetWorldPosByCubePosFunc>());
        internal CVector3Dbl GetWorldPos(CCubePos aCubePos)
            => this.GetWorldPosByCubePosFunc(aCubePos);
        #endregion
        #region Mass
        public bool MassIsDefined;
        private static readonly double QuadrantMass =1.0f;
        public virtual double? Mass => this.Radius * QuadrantMass;
        public bool AttractionIsDefined => this.MassIsDefined;
        private CVector3Dbl? AttractionToAvatarM;
        public CVector3Dbl AttractionToAvatar => CLazyLoad.Get(ref this.AttractionToAvatarM, this.NewAttractionToAvatar);
        public CVector3Dbl NewAttractionToAvatar()
        {
            if (this.Destroyed)
                return new CVector3Dbl(0);

            var aG= CStaticParameters.Gravity_G;
            var aAvatarMass = 1.0d;
            var mf = CStaticParameters.Gravity_MassMultiply;
            var aOrbMass = this.Mass.Value * mf;
            var aDistance = this.DistanceToAvatar;
            var aDistancePow2 = aDistance * aDistance;
            var aAttraction1 = ((aAvatarMass * aOrbMass) / aDistancePow2) * aG;
            var aRadius = this.Radius.Value;
            var aNoImpactDistance1 = CStaticParameters.Gravity_NoImpactDistance; // TODO_OPT
            var aNoImpactDistance2 = aNoImpactDistance1 + aRadius;
            var aNoImpactDistance = aNoImpactDistance2;
            var aAttractionImpact = aDistance < aRadius
                                  ? aDistance / aRadius
                                  : aDistance > aNoImpactDistance
                                  ? 0d
                                  : 1d - aDistance.F01_Map(aRadius, aNoImpactDistance, 0, 1)
                                  ;
            var aAttraction2 = aAttraction1 * aAttractionImpact;
            var aAttraction = aAttraction2;
            var aDistanceVec = this.WorldPos.Value - this.AvatarPos;
            var aAttractionVec = aDistanceVec * new CVector3Dbl(aAttraction);
            return aAttractionVec;
        }
        #endregion
        #region Destroyed
        private bool Destroyed 
        {
            get => this.SpritePersistentData is object
                  ? this.SpritePersistentData.Destroyed
                  :  false // ie. crosshairsprite doesn t get this.Build and has no perstistent data. maybe add a class hirarchy for that.
                  ;
            set =>this.SpritePersistentData.Destroyed = value;
        }

        #endregion
        #region Persistency
        internal bool PersistencyEnabled;

        private CSpritePersistentData SpritePersistentData;
        internal int? PersistentId;
        #endregion
        #region Collision
        public bool CollisionIsEnabled => this.CollisionSourceEnum.HasValue;
        internal CCollisionSourceEnum? CollisionSourceEnum = default(CCollisionSourceEnum?);
        internal virtual void Collide()
        {
            if(this.CollisionIsEnabled)
            {
                this.Collide(this.CanCollideWithTarget);
            }
        }
        internal virtual bool CanCollideWithTarget(CSprite aSprite)
            => this.CollisionSourceEnum.HasValue && aSprite.GetCanCollideWithSource(this.CollisionSourceEnum.Value);
        private void Collide(Func<CSprite, bool> aCanCollideWith)
            => this.Collide(this.FrameInfo.Sprites.Where(aCanCollideWith));
        private void Collide(IEnumerable<CSprite> aSprites)
        {
            var aSprite = this;
            var aOwnPos = this.WorldPos.Value;
            foreach (var aCollideWith in aSprites)
            {
                var aOtherPos = aCollideWith.WorldPos.Value;
                var aOtherRadius = aCollideWith.Radius;
                var aDistance = aOwnPos.GetDistance(aOtherPos);
                var aIsHit = aDistance < aOtherRadius;
                if (aIsHit)
                {
                    this.OnCollide(aCollideWith, aDistance);
                }
            }
        }
        protected virtual void OnCollide(CSprite aCollideWith, double aDistance)
        {

        }
        private static readonly int CollisionSourceCount = typeof(CCollisionSourceEnum).GetEnumMaxValue() + 1;
        private bool[] CollisionSourceIsEnabledArray;
        internal bool GetCanCollideWithSource(CCollisionSourceEnum aCollisionSource)
            => this.CollisionSourceIsEnabledArray[(int)aCollisionSource];
        internal bool SetCollisionIsEnabled(CCollisionSourceEnum aCollisionSource, bool aCollisionIsEnabled)
            => this.CollisionSourceIsEnabledArray[(int)aCollisionSource] = aCollisionIsEnabled;

        #endregion
    }


    public static class CExtensions
    {
        public static IEnumerable<CVector3Dbl> PolyPointsToLines(this IEnumerable<CVector3Dbl> aSquare)
        {
            var aFirst = aSquare.First();
            var aPrevious = aSquare.First();
            foreach (var aPoint in aSquare.Skip(1))
            {
                yield return aPrevious;
                yield return aPoint;
                aPrevious = aPoint;
            }
            yield return aPrevious;
            yield return aFirst;
        }
    }

    //internal sealed class CSpritePool : CServiceLocatorNode
    //{
    //    internal CSpritePool(CServiceLocatorNode aParent) : base(aParent)
    //    {
    //        this.SunPool.NewFunc = new Func<CSun>(() => new CSun(this));
    //        this.PlanetPool.NewFunc = new Func<CPlanet>(() => new CPlanet(this));
    //        this.MoonPool.NewFunc = new Func<CMoon>(() => new CMoon(this));
    //        this.AsteroidPool.NewFunc = new Func<CAsteroid>(() => new CAsteroid(this));
    //    }

    //    private bool ReserveIsDone;
    //    internal void Reserve()
    //    {
    //        bool aEnableReserve = false;
    //        if(!this.ReserveIsDone
    //        && aEnableReserve)
    //        {
    //            System.Diagnostics.Debugger.Break();
    //            this.AsteroidPool.Reserve(600);
    //            this.AsteroidPool.Locked = true;
    //            this.SunPool.Reserve(600);
    //            this.SunPool.Locked = true;
    //            this.PlanetPool.Reserve(600);
    //            this.PlanetPool.Locked = true;
    //            this.MoonPool.Reserve(600);
    //            this.MoonPool.Locked = true;
    //            this.ReserveIsDone = true;
    //        }
    //    }


    //    #region Sun
    //    private readonly CObjectPool<CSun> SunPool = new CObjectPool<CSun>();
    //    internal CSun AllocateSun()
    //    {
    //        this.Reserve();
    //        return this.SunPool.Allocate();
    //    }
    //    #endregion
    //    #region Planet
    //    private readonly CObjectPool<CPlanet> PlanetPool = new CObjectPool<CPlanet>();
    //    internal CPlanet NewPlanet()
    //    {
    //        this.Reserve();
    //        return this.PlanetPool.Allocate();
    //    }
    //    #endregion
    //    #region Moon
    //    private readonly CObjectPool<CMoon> MoonPool = new CObjectPool<CMoon>();
    //    internal CMoon NewMoon()
    //    {
    //        this.Reserve();
    //        return this.MoonPool.Allocate(); 
    //    }
    //    #endregion
    //    #region Asteroid
    //    private readonly CObjectPool<CAsteroid> AsteroidPool = new CObjectPool<CAsteroid>();
    //    internal CAsteroid NewAsteroid()
    //    {
    //        this.Reserve();
    //        return this.AsteroidPool.Allocate();
    //    }
    //    #endregion
    //}

    internal abstract class CSpriteManager : CServiceLocatorNode
    {
        internal CSpriteManager(CServiceLocatorNode aParent):base(aParent)
        {
        }

        internal abstract void UpdateAvatarPos();
        internal abstract void Update(CFrameInfo aFrameInfo);

        internal abstract IEnumerable<CSprite> BaseSprites { get; }
    }

    internal abstract class CSpriteManager<TSprite> : CSpriteManager where TSprite : CSprite
    {
        internal CSpriteManager(CServiceLocatorNode aParent): base(aParent)
        {
            this.World = this.ServiceContainer.GetService<CWorld>();
        }


        protected bool AddOnAllocate;

        internal readonly CWorld World;

        protected readonly List<TSprite> ActiveSprites = new List<TSprite>();

        internal IEnumerable<TSprite> Sprites => this.ActiveSprites;
        internal override IEnumerable<CSprite> BaseSprites => this.Sprites;

        protected void AddSprite(TSprite aSprite)
        {
            this.ActiveSprites.Add(aSprite);

        }

        internal void OnAllocate(TSprite aSprite)
        {
            if (this.AddOnAllocate)
            {
                this.AddSprite(aSprite);
            }
        }
        private void RemoveDeadSprites()
        {
            var aDeadShots = (from aTest in this.ActiveSprites where aTest.DeallocateIsQueued  select aTest).ToArray();
            foreach (var aDeadShot in aDeadShots)
            {
                aDeadShot.DeallocateIsQueued = false;
                aDeadShot.Deallocate();
            }
        }

        internal override void UpdateAvatarPos()
        {
            //this.RemoveDeadSprites();
            foreach (var aSprite in this.ActiveSprites)
            {
                if (aSprite.IsInUse)
                {
                    aSprite.UpdateAvatarPos();
                }
            }
        }

        internal override void Update(CFrameInfo aFrameInfo)
        {
            foreach (var aSprite in this.ActiveSprites)
            {
                aSprite.Update(aFrameInfo);
                aSprite.Collide();
            }
            this.RemoveDeadSprites();
        }
    }

    internal abstract class CMultiPoolSpriteManager<TSprite, TClassEnum> : CSpriteManager<TSprite> where TSprite : CSprite where TClassEnum : Enum
    {
        internal CMultiPoolSpriteManager(CServiceLocatorNode aParent): base(aParent)
        {
            this.MultiSpritePool = new CMultiSpritePool(this);
        }
        protected override void Init()
        {
            base.Init();
            this.MultiSpritePool.Init();
        }
        internal sealed class CMultiSpritePool : CMultiObjectPool
        {
            internal CMultiSpritePool(CMultiPoolSpriteManager<TSprite, TClassEnum> aMultiPoolSpriteManager)
            {
                this.MultiPoolSpriteManager = aMultiPoolSpriteManager;
            }
            internal void Init()
            {
                var aClassCount = this.MultiPoolSpriteManager.SpriteClassCount;
                this.AllocateObjectPool(aClassCount);
                for (var i = 0; i < aClassCount; ++i)
                {
                    this.SetNewFunc(i, this.MultiPoolSpriteManager.GetNewFunc((TClassEnum)(object)i));
                }
            }
            private readonly CMultiPoolSpriteManager<TSprite, TClassEnum> MultiPoolSpriteManager;
            protected override void OnDeallocating(CReuseable aReusable)
            {
                base.OnDeallocating(aReusable);
                this.MultiPoolSpriteManager.OnDeallocating((TSprite)aReusable);
            }
        }

        private void OnDeallocating(TSprite aSprite)
        {
            this.ActiveSprites.Remove(aSprite);
        }

        internal abstract CNewFunc GetNewFunc(TClassEnum aClassEnum);

        internal abstract int SpriteClassCount { get; }
        private readonly CMultiSpritePool MultiSpritePool;
        protected TSprite AllocateSprite(TClassEnum aClassEnum)
        {
            var aSprite = (TSprite)this.MultiSpritePool.Allocate((int)(object)aClassEnum);
            this.OnAllocate(aSprite);
            return aSprite;
        }
        
    }

    internal abstract class CSinglePoolSpriteManager<TSprite> : CSpriteManager<TSprite> where TSprite : CSprite
    {
        internal CSinglePoolSpriteManager(CServiceLocatorNode aParent) : base(aParent)
        {
            
            this.SpritePool = new CObjectPool<TSprite>();
            this.SpritePool.NewFunc = new Func<TSprite>(() => this.NewSprite());
            this.SpritePool.Deallocating += this.OnDeallocating;
        }

        protected void OnDeallocating(TSprite aSprite)
        {
            this.ActiveSprites.Remove(aSprite);
        }
        protected abstract TSprite NewSprite();

        private readonly CObjectPool<TSprite> SpritePool;
        protected TSprite AllocateSprite()
        {
            var aSprite = this.SpritePool.Allocate();
            if (this.AddOnAllocate)
                this.AddSprite(aSprite);
            return aSprite;
        }
    }

}
