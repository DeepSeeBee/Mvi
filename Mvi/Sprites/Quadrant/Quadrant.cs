using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Mvi.Sprites.Asteroid;
using CharlyBeck.Mvi.Sprites.GridLines;
using CharlyBeck.Mvi.Sprites.SolarSystem;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils3.Asap;

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
            this.SolarSystemSpriteManagerM = new CSolarSystemSpriteManager(this);
        }
        #endregion

        internal readonly CSolarSystemSpriteManager SolarSystemSpriteManagerM;
        internal override CSolarSystemSpriteManager SolarSystemSpriteManager => this.SolarSystemSpriteManagerM;

        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CSolarSystemSpriteManager>(() => this.SolarSystemSpriteManager);
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

        private CSolarSystemSpriteManager SolarSystemSpriteManagerM;
        internal virtual CSolarSystemSpriteManager SolarSystemSpriteManager => CLazyLoad.Get(ref this.SolarSystemSpriteManagerM, () => this.ServiceContainer.GetService<CSolarSystemSpriteManager>());

        private CWorldSpriteManagers WorldSpriteManagersM;
        internal CWorldSpriteManagers WorldSpriteManagers => CLazyLoad.Get(ref this.WorldSpriteManagersM, () => this.ServiceContainer.GetService<CWorldSpriteManagers>());
        internal virtual void DeallocateContent()
        {
        }
        internal virtual void AllocateStaticSprites()
        {

        }
    }

}
