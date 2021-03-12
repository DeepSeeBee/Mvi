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
            this.MassIsDefined = true;
            this.DestroyedSound = Sfx.CSoundDirectoryEnum.Audio_Destroyed_Moon;
            this.AsteroidRadiusMax = this.World.DefaultAsteroidRadiusMax;
            this.CategoryName = "Asteroid";

            this.Init();
        }

        protected override void OnBeginUse()
        {
            base.OnBeginUse();
        }

        protected override void OnEndUse()
        {
            base.OnEndUse();

            this.WorldPos = default;
        }


        internal override void Build(CSpriteBuildArgs a)
        {
            base.Build(a);
            this.WorldPos = this.GetRandomWorldPos(a.QuadrantBuildArgs.RandomGenerator);
        }
    }
    internal sealed class CAsteroidsQuadrantContent : CQuadrantContent
    {
        #region ctor
        public CAsteroidsQuadrantContent(CServiceLocatorNode aParent) : base(aParent)
        {
        }

        protected override void Init()
        {
            base.Init();
        }
        protected override void OnEndUse()
        {
            base.OnEndUse();
            this.Asteroids.DeallocateItems();
        }
        internal override void DeallocateContent()
        {
            base.DeallocateContent();
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
                var aAsteroid = this.SolarSystemSpriteManager.AllocateAsteroidNullable();
                if (aAsteroid is object)
                {
                    aAsteroid.Build(aQuadrantBuildArgs);
                }
                aAsteroids[aIdx] = aAsteroid;
            }
            this.Asteroids = aAsteroids;

        }
        private CBumperSprite[] Asteroids;

        public override IEnumerable<CSprite> Sprites => this.Asteroids;
    }
}
