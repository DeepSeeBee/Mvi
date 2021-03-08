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
            this.World = this.ServiceContainer.GetService<CWorld>();
            this.SpritePool = this.ServiceContainer.GetService<CSpritePool>();
        }
        #endregion
        internal readonly CSpritePool SpritePool;
        internal readonly CWorld World;
        public abstract IEnumerable<CSprite> Sprites { get; }
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
            this.AsteroidsQuadrant = new CAsteroidsQuadrant(this);
            this.SolarSystemQuadrant = new CSolarSystemQuadrant(this);
            var aItems = new List<CSpaceQuadrant>();
            if (CStaticParameters.Quadrant_Asteroids)
                aItems.Add(this.AsteroidsQuadrant);
            if (CStaticParameters.Quadrant_SolarSystem)
                aItems.Add(this.SolarSystemQuadrant);

            var aQuadrants = aItems.ToArray();
            this.RandomSpaceQuadrants = aQuadrants;
        }
        #endregion
        internal readonly CCubeQuadrant CubeQuadrant;
        internal readonly CAsteroidsQuadrant AsteroidsQuadrant;
        internal readonly CSolarSystemQuadrant SolarSystemQuadrant;

        private CSpaceQuadrant[] RandomSpaceQuadrants;
        private CSpaceQuadrant RandomSpaceQuadrant;

        internal override void Build(CQuadrantBuildArgs a)
        {
            base.Build(a);

            var aRandomGenerator = a.RandomGenerator;
            this.RandomSpaceQuadrant = aRandomGenerator.NextItem(this.RandomSpaceQuadrants);
            this.RandomSpaceQuadrant.Build(a);
            this.CubeQuadrant.Build(a);
        }
        public override IEnumerable<CSprite> Sprites 
            => this.RandomSpaceQuadrant.Sprites.Concat(this.CubeQuadrant.Sprites);

    }
}
