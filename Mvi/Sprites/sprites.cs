using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Cube.Mvi;
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



    public abstract class CSprite : CReuseable // CTileDescriptorBuildable
    {
        internal CSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Facade = this.ServiceContainer.GetService<CFacade>();
            this.PlatformSprite = this.NewPlatformSprite();
            this.TileCubePos = new CCubePos(0);
        }

        private CSpritePool SpritePoolM;
        internal CSpritePool SpritePool => CLazyLoad.Get(ref this.SpritePoolM, () => this.ServiceContainer.GetService<CSpritePool>());

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
            this.TileCubePos = a.QuadrantBuildArgs.TileCubePos;
            this.TileWorldPos = this.GetWorldPos(a.QuadrantBuildArgs.TileCubePos);
            this.Reposition();
        }

        protected override void OnEndUse()
        {
            base.OnEndUse();

            this.IsNearestM = default;
            this.TileCubePos = default;
            this.TileWorldPos = default;
            this.HitGameTimeTotal = default;
            this.Radius = default;
            this.ObjectIdM = default;
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

        internal void OnHit(CShotSprite aShotSprite)
        {
            this.World.OnDestroyed(this, aShotSprite);
        }

        public CVector3Dbl? TileWorldPos { get; private set; }
        internal IEnumerable<CTranslateAndRotate> TranslateAndRotates
        {
            get
            {
                yield return new CTranslateAndRotate(this.WorldPos, new CVector3Dbl(0));
            }
        }
        public abstract CVector3Dbl WorldPos { get; }
        public double? Radius { get; set; }

        internal readonly CFacade Facade;
        internal CWorld World => this.Facade.World;
        #region PlatformSpriteEnum
        internal abstract CPlatformSpriteEnum PlattformSpriteEnum { get; }
        internal CPlatformSprite NewPlatformSprite() => this.Facade.PlatformSpriteFactory.NewPlatformSprite(this);
        internal CPlatformSprite PlatformSprite { get; private set; }
        #endregion
        internal void Reposition()
            => this.PlatformSprite.Reposition();
        internal virtual int ChangesCount => 0;
        internal virtual bool Visible => !this.IsHit;

        private bool? IsNearestM;
        public bool IsNearest => CLazyLoad.Get(ref this.IsNearestM, () =>this.World.FrameInfo.SpriteDatasOrderedByDistance.OfType<CSprite>().First().RefEquals<CSprite>(this));
        internal virtual void Update(CFrameInfo aFrameInfo) { }
        public double GetAlpha(CVector3Dbl aCameraPos)
        {
            var d = this.WorldPos.GetDistance(aCameraPos);
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
        }

        internal CVector3Dbl? AvatarPos { get; private set; }

        private double? DistanceToAvatarM;
        public double DistanceToAvatar { get=>CLazyLoad.Get(ref this.DistanceToAvatarM, () => this.AvatarPos.Value.GetDistance(this.WorldPos)); }
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
        #region Hitable
        internal bool IsHitable;
        internal TimeSpan? HitGameTimeTotal;
        internal bool IsHit => this.HitGameTimeTotal.HasValue;
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

    internal sealed class CSpritePool : CMultiObjectPool
    {
        internal CSpritePool(CServiceLocatorNode aParent)
        {
            this.ServiceLocatorNode = aParent;
            this.SunPool.NewFunc = new Func<CSun>(() => new CSun(this.ServiceLocatorNode));
            this.PlanetPool.NewFunc = new Func<CPlanet>(() => new CPlanet(this.ServiceLocatorNode));
            this.MoonPool.NewFunc = new Func<CMoon>(() => new CMoon(this.ServiceLocatorNode));
            this.AsteroidPool.NewFunc = new Func<CAsteroid>(() => new CAsteroid(this.ServiceLocatorNode));
        }

        private bool ReserveIsDone;
        internal void Reserve()
        {
            bool aEnableReserve = false;
            if(!this.ReserveIsDone
            && aEnableReserve)
            {
                System.Diagnostics.Debugger.Break();
                this.AsteroidPool.Reserve(600);
                this.AsteroidPool.Locked = true;
                this.SunPool.Reserve(600);
                this.SunPool.Locked = true;
                this.PlanetPool.Reserve(600);
                this.PlanetPool.Locked = true;
                this.MoonPool.Reserve(600);
                this.MoonPool.Locked = true;
                this.ReserveIsDone = true;
            }
        }

        private readonly CServiceLocatorNode ServiceLocatorNode;

        #region Sun
        private readonly CObjectPool<CSun> SunPool = new CObjectPool<CSun>();
        internal CSun AllocateSun()
        {
            this.Reserve();
            return this.SunPool.Allocate();
        }
        #endregion
        #region Planet
        private readonly CObjectPool<CPlanet> PlanetPool = new CObjectPool<CPlanet>();
        internal CPlanet NewPlanet()
        {
            this.Reserve();
            return this.PlanetPool.Allocate();
        }
        #endregion
        #region Moon
        private readonly CObjectPool<CMoon> MoonPool = new CObjectPool<CMoon>();
        internal CMoon NewMoon()
        {
            this.Reserve();
            return this.MoonPool.Allocate(); 
        }
        #endregion
        #region Asteroid
        private readonly CObjectPool<CAsteroid> AsteroidPool = new CObjectPool<CAsteroid>();
        internal CAsteroid NewAsteroid()
        {
            this.Reserve();
            return this.AsteroidPool.Allocate();
        }
        #endregion
    }

    internal abstract class CSpriteManager<TSprite> : CServiceLocatorNode where TSprite : CSprite
    {
        internal CSpriteManager(CServiceLocatorNode aParent) : base(aParent)
        {
            this.World = this.ServiceContainer.GetService<CWorld>();
            this.SpritePool = new CObjectPool<TSprite>();
            this.SpritePool.NewFunc = new Func<TSprite>(() => this.NewSprite());
        }

        protected abstract TSprite NewSprite();

        internal readonly CWorld World;
        private CObjectPool<TSprite> SpritePool;
        private readonly List<TSprite> ActiveSprites = new List<TSprite>();

        internal IEnumerable<TSprite> Sprites => this.ActiveSprites;

        protected void AddSprite(TSprite aSprite)
        {
            this.ActiveSprites.Add(aSprite);

        }
        protected TSprite AllocateSprite()
            => this.SpritePool.Allocate();

        private void RemoveDeadSprites()
        {
            var aDeadShots = (from aTest in this.ActiveSprites where aTest.DeallocateIsQueued select aTest).ToArray();
            foreach (var aDeadShot in aDeadShots)
            {
                aDeadShot.DeallocateIsQueued = false;
                this.ActiveSprites.Remove(aDeadShot);
            }
        }

        internal void UpdateAvatarPos()
        {
            this.RemoveDeadSprites();

            foreach (var aSprite in this.ActiveSprites)
                aSprite.UpdateAvatarPos();;
        }

        internal void Update(CFrameInfo aFrameInfo)
        {

            foreach (var aSprite in this.ActiveSprites)
                aSprite.Update(aFrameInfo);

        }
    }

}
