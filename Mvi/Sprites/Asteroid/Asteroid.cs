using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Cube.Mvi;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Models;
using CharlyBeck.Mvi.Sprites.Bumper;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.ServiceLocator;
using Mvi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils3.Asap;

namespace CharlyBeck.Mvi.Sprites.Asteroid
{
    using CDoubleRange = System.Tuple<double, double>;

    public sealed class CAsteroid : CBumperSprite
    {
        internal CAsteroid(CServiceLocatorNode aParent) : base(aParent)
        {
            this.PlaysFlybySound = true;
            this.DestroyedSound = Sfx.CSoundDirectoryEnum.Audio_Destroyed_Moon;
        }

        internal override CVector3Dbl GenerateOriginalWorldPos(CRandomGenerator aRandomGenerator)
            => this.GenerateDefaultWorldPos(aRandomGenerator);

        internal override CDoubleRange AsteroidRadiusMax => this.World.DefaultAsteroidRadiusMax;
        public override string CategoryName => "Asteroid";
        internal override CPlatformSpriteEnum PlattformSpriteEnum => CPlatformSpriteEnum.Bumper;
    }
    internal sealed class CAsteroidsQuadrant : CSpaceQuadrant
    {
        #region ctor
        public CAsteroidsQuadrant(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        protected override void OnEndUse()
        {
            base.OnEndUse();
            this.Asteroids.DeallocateItems();
        }
        #endregion

        internal override void Build(CQuadrantBuildArgs aQuadrantBuildArgs)
        {
            var aWorld = this.World;
            var aRandomGenerator = aQuadrantBuildArgs.RandomGenerator;
            var aAsteroidCount = aRandomGenerator.NextInteger(aWorld.TileAsteroidCountMin, aWorld.TileAsteroidCountMax);
            var aAsteroids = new CBumperSprite[aAsteroidCount];
            this.Asteroids.DeallocateItems();
            for (var aIdx = 0; aIdx < aAsteroidCount; ++aIdx)
            {
                var aAsteroid = this.SpritePool.NewAsteroid();
                aAsteroid.Build(aQuadrantBuildArgs);
                aAsteroids[aIdx] = aAsteroid;
            }
            this.Asteroids = aAsteroids;

        }
        private CBumperSprite[] Asteroids;

        public override IEnumerable<CSprite> Sprites => this.Asteroids;
    }
}
