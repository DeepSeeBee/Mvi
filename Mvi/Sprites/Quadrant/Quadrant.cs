using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Mvi.Sprites.Asteroid;
using CharlyBeck.Mvi.Sprites.GridLines;
using CharlyBeck.Mvi.Sprites.SolarSystem;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.Reflection;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharlyBeck.Utils3.Asap;

namespace CharlyBeck.Mvi.Cube.Mvi
{
    internal abstract class CQuadrant : CQuadrantContent
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

        internal override void Build(CQuadrantBuildArgs a)
        {
            this.ResetLAstSpritePersistentData();
            this.TileCubePos = a.TileCubePos;
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

    internal abstract class CSpaceQuadrant : CQuadrant
    {
        #region ctor
        internal CSpaceQuadrant(CServiceLocatorNode aParent) : base(aParent)
        {
            this.SolarSystemSpriteManagerM = new CQuadrantSpriteManager(this);
        }
        #endregion

        internal readonly CQuadrantSpriteManager SolarSystemSpriteManagerM;
        internal override CQuadrantSpriteManager SolarSystemSpriteManager => this.SolarSystemSpriteManagerM;

        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CQuadrantSpriteManager>(() => this.SolarSystemSpriteManager);
            return aServiceContainer;
        }
        #endregion
    }

    internal sealed class CSpaceSwitchQuadrant : CSpaceQuadrant
    {
        #region ctor
        internal CSpaceSwitchQuadrant(CServiceLocatorNode aParent):base(aParent)
        {
            this.CubeQuadrant = new CGridLinesQuadrantContent(this);
            this.AsteroidsQuadrant = new CAsteroidsQuadrantContent(this);
            this.SolarSystemQuadrant = new CSolarSystemQuadrantContent(this);
            var aItems = new List<CQuadrantContent>();
            if (CStaticParameters.Quadrant_Asteroids)
                aItems.Add(this.AsteroidsQuadrant);
            if (CStaticParameters.Quadrant_SolarSystem)
                aItems.Add(this.SolarSystemQuadrant);

            var aQuadrants = aItems.ToArray();
            this.QuadrantContents = aQuadrants;
        }
        protected override void OnEndUse()
        {
            base.OnEndUse();

            this.CubeQuadrant.DeallocateContent();
            this.AsteroidsQuadrant.DeallocateContent();
            this.SolarSystemQuadrant.DeallocateContent();
        }
        #endregion
        internal readonly CGridLinesQuadrantContent CubeQuadrant;
        internal readonly CAsteroidsQuadrantContent AsteroidsQuadrant;
        internal readonly CSolarSystemQuadrantContent SolarSystemQuadrant;

        private CQuadrantContent[] QuadrantContents;
        private CQuadrantContent QuadrantContent;

        internal override void Build(CQuadrantBuildArgs a)
        {
            base.Build(a);

            var aRandomGenerator = a.RandomGenerator;
            this.QuadrantContent = aRandomGenerator.NextItem(this.QuadrantContents);
            this.QuadrantContent.Build(a);
            this.CubeQuadrant.Build(a);
        }   
        public override IEnumerable<CSprite> Sprites 
            => this.QuadrantContent is object
            ? this.QuadrantContent.Sprites.Concat(this.CubeQuadrant.Sprites)
            : Array.Empty<CSprite>()
            ;

    }

    internal abstract class CQuadrantContent : CReuseable
    {
        internal CQuadrantContent(CServiceLocatorNode aParent) : base(aParent)
        {
            this.World = this.ServiceContainer.GetService<CWorld>();

            this.WorldSpriteManagers.RootAllocateStaticSprites += this.AllocateStaticSprites;
        }

        internal readonly CWorld World;
        public abstract IEnumerable<CSprite> Sprites { get; }

        internal abstract void Build(CQuadrantBuildArgs aQuadrantBuildArgs);

        private CQuadrantSpriteManager SolarSystemSpriteManagerM;
        internal virtual CQuadrantSpriteManager SolarSystemSpriteManager => CLazyLoad.Get(ref this.SolarSystemSpriteManagerM, () => this.ServiceContainer.GetService<CQuadrantSpriteManager>());

        private CWorldSpriteManagers WorldSpriteManagersM;
        internal CWorldSpriteManagers WorldSpriteManagers => CLazyLoad.Get(ref this.WorldSpriteManagersM, () => this.ServiceContainer.GetService<CWorldSpriteManagers>());
        internal virtual void DeallocateContent()
        {
        }
        internal virtual void AllocateStaticSprites()
        {

        }
    }
    internal sealed class CQuadrantSpriteManager : CMultiPoolSpriteManager<CSprite, CSolarSystemSpriteEnum>
    {
        internal CQuadrantSpriteManager(CServiceLocatorNode aParent) : base(aParent)
        {
            this.NoOutOfMemoryException = true;
            this.AddOnAllocate = true;
            this.Init();
        }

        protected override void Init()
        {
            base.Init();
            var aLock = true;
            this.Reserve(CSolarSystemSpriteEnum.GridLines, 1, aLock);
            this.Reserve(CSolarSystemSpriteEnum.Asteroid, this.World.TileAsteroidCountMax, aLock);
            this.Reserve(CSolarSystemSpriteEnum.Sun, 1, aLock);
            this.Reserve(CSolarSystemSpriteEnum.Planet, CStaticParameters.SunTrabantCountMax, aLock);
            this.Reserve(CSolarSystemSpriteEnum.Moon, CStaticParameters.SunTrabantCountMax * this.World.PlanetMoonCountRange.Item2, aLock);
        }

        internal override int SpriteClassCount => typeof(CSolarSystemSpriteEnum).GetEnumMaxValue() + 1;
        internal override CNewFunc GetNewFunc(CSolarSystemSpriteEnum aClassEnum)
        {
            switch (aClassEnum)
            {
                case CSolarSystemSpriteEnum.GridLines: return new CNewFunc(() => new CGridLinesSprite(this));
                case CSolarSystemSpriteEnum.Asteroid: return new CNewFunc(() => new CAsteroid(this));
                case CSolarSystemSpriteEnum.Sun: return new CNewFunc(() => new CSun(this));
                case CSolarSystemSpriteEnum.Planet: return new CNewFunc(() => new CPlanet(this));
                case CSolarSystemSpriteEnum.Moon: return new CNewFunc(() => new CMoon(this));
                default:
                    throw new InvalidOperationException();
            }
        }

        internal CAsteroid AllocateAsteroidNullable()
            => (CAsteroid)this.AllocateSpriteNullable(CSolarSystemSpriteEnum.Asteroid);
        internal CSun AllocateSun()
            => (CSun)this.AllocateSpriteNullable(CSolarSystemSpriteEnum.Sun);
        internal CPlanet AllocatePlanetNullable()
            => (CPlanet)this.AllocateSpriteNullable(CSolarSystemSpriteEnum.Planet);
        internal CMoon AllocateMoonNullable()
            => (CMoon)this.AllocateSpriteNullable(CSolarSystemSpriteEnum.Moon);
        internal CGridLinesSprite AllocateGridLinesSpriteNullable()
            => (CGridLinesSprite)this.AllocateSpriteNullable(CSolarSystemSpriteEnum.GridLines);
    }

    //internal sealed class CQuadrantSpriteManagers : CServiceLocatorNode
    //{
    //    internal CQuadrantSpriteManagers(CServiceLocatorNode aParent) : base(aParent)
    //    {
    //        this.QuadrantSpriteManager = new CQuadrantSpriteManager(this);
    //    }

    //    internal readonly CQuadrantSpriteManager QuadrantSpriteManager;

    //}
}
