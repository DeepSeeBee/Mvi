using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Sprites.Bumper;
using CharlyBeck.Mvi.Sprites.Quadrant;
using CharlyBeck.Mvi.World;
using CharlyBeck.Mvi.XnaExtensions;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Sprites.SolarSystem
{

    public abstract class COrb : CBumperSpriteData
    {
        internal COrb(CServiceLocatorNode aParent, CTileBuilder aTileBuilder, CTileDescriptor aTileDescriptor) : base(aParent, aTileBuilder, aTileDescriptor)
        {

        }
        internal override double[] BumperRadiusMax => this.World.SolarSystemPlanetRadiusMax;

    }

    public abstract class CTrabant : COrb
    {
        internal CTrabant(CServiceLocatorNode aParent, CTileBuilder aTileBuilder, CTileDescriptor aTileDescriptor, double aOrbit) : base(aParent, aTileBuilder, aTileDescriptor)
        {          
            this.Orbit = aOrbit;
        }

        internal readonly double Orbit; 
        internal TimeSpan SolarSystemYear { get; private set; }
        internal double OrbitStartAngle { get; private set; }
        internal double OrbitCurrentAngle => (this.World.GameTime.TotalGameTime.TotalSeconds * Math.PI * 2d / this.SolarSystemYear.TotalSeconds) + this.OrbitStartAngle;
        internal abstract COrb ParentOrb { get; }
        protected override CVector3Dbl GenerateOriginalWorldPos()
            => (this.ParentOrb.WorldPos.ToVector3() + new Vector3((float)this.Orbit, 0, 0))
                .RotateY(this.ParentOrb.WorldPos.ToVector3(), (float)this.OrbitStartAngle).ToVector3Dbl();
        public override CVector3Dbl WorldPos
                => (this.ParentOrb.WorldPos.ToVector3() + new Vector3((float)this.Orbit, 0, 0))
                    .RotateY(this.ParentOrb.WorldPos.ToVector3(), (float)this.OrbitCurrentAngle).ToVector3Dbl();

        protected override void OnBuild()
        {
            base.OnBuild();

            this.OrbitStartAngle = (this.WorldGenerator.NextDouble() * Math.PI * 2d).ToRadians();
            this.SolarSystemYear =  TimeSpan.FromSeconds(this.WorldGenerator.NextDouble(40d, 90d ) * this.Orbit);
        }


        public override Matrix WorldMatrix
        {
            get
            {
                /*
                var m1 = this.ParentOrbIsDefined
                       ? this.ParentOrb.WorldMatrix
                       : Matrix.Identity;
                var m2 = Matrix.CreateTranslation((float)this.Radius);
                var m3 = m1 * m2;
                */
                return Matrix.Identity;
              //  throw new NotImplementedException();

            }
        }
    }

    public sealed class CPlanet : CTrabant
    {
        internal CPlanet(CServiceLocatorNode aParent, CTileBuilder aTileBuilder, CTileDescriptor aTileDescriptor, double aOrbit) : base(aParent, aTileBuilder, aTileDescriptor, aOrbit)
        {
            this.Sun = this.ServiceContainer.GetService<CSun>();
            this.Init();
            this.Build();
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        private readonly CSun Sun;
        internal override COrb ParentOrb => this.Sun;

    }

    public sealed class CMoon : CTrabant
    {
        internal CMoon(CServiceLocatorNode aParent, CTileBuilder aTileBuilder, CTileDescriptor aTileDescriptor, double aOrbit) : base(aParent, aTileBuilder, aTileDescriptor, aOrbit)
        {
            this.Planet = this.ServiceContainer.GetService<CPlanet>();

            this.Init();
            this.Build();

        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        private CPlanet Planet;
        internal override COrb ParentOrb => this.Planet;
        internal override double[] BumperRadiusMax => base.BumperRadiusMax;
    }


    internal sealed class CSun : COrb
    {
        internal CSun(CServiceLocatorNode aParent, CTileBuilder aTileBuilder, CQuadrantTileDescriptor aTileDescriptor) : base(aParent, aTileBuilder, aTileDescriptor)
        {
            this.QuadrantTileDescriptor = aTileDescriptor;
            this.Init();
            this.Build();
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        private readonly CQuadrantTileDescriptor QuadrantTileDescriptor;
        protected override CVector3Dbl GenerateOriginalWorldPos()
            => this.QuadrantTileDescriptor.QuadrantSpriteData.Center;
        internal Vector3 OrbitAxis => new Vector3(1, 1, 1);
        internal override double[] BumperRadiusMax => this.World.SolarSystemSunRadiusMax;
    }

    internal sealed class CSolarSystem : CQuadrantTileDescriptor
    {
        #region ctor
        public CSolarSystem(CServiceLocatorNode aParent, CTileBuilder aTileBuilder) : base(aParent, aTileBuilder)
        {
            this.Init();
            this.Build();
        }
        public override T Throw<T>(Exception aException)
           => throw aException;
        #endregion


        protected override void OnDraw()
        {
            base.OnDraw();

            this.Sun.Draw();

            foreach(var aPlanet in this.Planets)
            {
                aPlanet.Draw();
            }
        }
        protected override void OnBuild()
        {
            base.OnBuild();

            var aTileBuilder = this.TileBuilder;
            this.Sun = new CSun(this, this.TileBuilder, this);

            var aWorldGenerator = aTileBuilder.WorldGenerator;
            var aWorld = aTileBuilder.World;
            var aPlanetCount = aWorldGenerator.NextInteger(aWorld.TileBumperCountMin, aWorld.TileBumperCountMax);
            //aPlanetCount = 0;
            var aPlanets = new CPlanet[aPlanetCount];
            var aSolarSystemRadius = aWorldGenerator.NextDouble(aWorld.SolarSystemMinRadius, aWorld.SolarSystemMaxRadius);
            var aSunRadius = this.Sun.Radius;
            var aOrbitStartRadius = aSunRadius + aSunRadius * 0.75;
            var aOrbitEndRadius = aSolarSystemRadius;
            var aPlanetOrbitRange = (aOrbitEndRadius - aOrbitStartRadius) / aPlanetCount;
           // var aPlanetOrbit
            for (var aIdx = 0; aIdx < aPlanetCount; ++aIdx)
            {
                var aOrbit1 = ((double)aIdx + 1) * aPlanetOrbitRange;
                //var aOrbit2 = aOrbit1 + aOrbitStartRadius;
                //var aOrbit3 = aOrbit2 + aWorldGenerator.NextDouble() * aPlanetOrbitRange;
                var aOrbit = aOrbit1;
                aPlanets[aIdx] = new CPlanet(this, aTileBuilder, this, aOrbit);
            }
            this.Planets = aPlanets;
        }
        internal CPlanet[] Planets { get; private set; }
        private CSun Sun;
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CSun>(() => this.Sun);
            return aServiceContainer;
        }
        #endregion
    }
}
