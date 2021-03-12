using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Mvi.Sprites.Asteroid;
using CharlyBeck.Mvi.Sprites.Cube;
using CharlyBeck.Mvi.Sprites.SolarSystem;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Cube.Mvi
{

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
        //internal virtual void Update(CVector3Dbl aAvatarPos)
        //{
        //    foreach (var aSprite in this.Sprites)
        //        aSprite.Update(aAvatarPos);
        //}
        //internal virtual void Update(CFrameInfo aFrameInfo)
        //{ 
        //    foreach(var aSprite in this.Sprites)
        //        aSprite.Update(aFrameInfo);
        //}
    }

    internal sealed class CSpaceSwitchQuadrant : CSpaceQuadrant
    {
        #region ctor
        internal CSpaceSwitchQuadrant(CServiceLocatorNode aParent):base(aParent)
        {
            this.CubeQuadrant = new CCubeQuadrant(this);
            this.AsteroidsQuadrant = new CAsteroids(this);
            this.SolarSystemQuadrant = new CSolarSystems(this);
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
        internal readonly CCubeQuadrant CubeQuadrant;
        internal readonly CAsteroids AsteroidsQuadrant;
        internal readonly CSolarSystems SolarSystemQuadrant;

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
}
