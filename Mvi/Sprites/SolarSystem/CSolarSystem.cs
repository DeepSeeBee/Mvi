using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Extensions;
using CharlyBeck.Mvi.Feature;
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
    using CDoubleRange = Tuple<double, double>;
    using CIntegerRange = Tuple<int, int>;
    using COrbit = Tuple<CVector3Dbl, CVector3Dbl, double, double>; // OrbitAngles, OrbitCenter, OrbitRadius, OrbitCurrentRadians


    public abstract class COrb : CBumperSpriteData
    {
        #region ctor
        internal COrb(CServiceLocatorNode aParent, CTileBuilder aTileBuilder, CTileDescriptor aTileDescriptor) : base(aParent, aTileBuilder, aTileDescriptor)
        {
            this.SolarSystem = this.ServiceContainer.GetService<CSolarSystem>();
        }
        #endregion
        internal override CDoubleRange BumperRadiusMax => this.World.PlanetRadiusMax;

        internal virtual CVector3Dbl GeneratedOrbitPlaneSlope { get; set; }
        internal CVector3Dbl OrbitPlaneSlopeCur => this.OrbitPlaneSlopeFeature.Enabled ? this.GeneratedOrbitPlaneSlope : new CVector3Dbl(0d);

        #region Trabants
        internal virtual double TrabantPropability => 1d;
        internal virtual CIntegerRange TrabantCountRange => new CIntegerRange(0,0);
        internal virtual CTrabant NewTrabant(double aOrbitRadius) => this.Throw<CTrabant>(new NotImplementedException());
        internal virtual CDoubleRange TrabantOrbitRange => this.Throw<CDoubleRange>(new NotImplementedException());
        private CTrabant[] Trabants;
        private void BuildTrabants()
        {
            if (this.HasTrabants)
            {
                var aTileBuilder = this.TileBuilder;
                var aWorldGenerator = aTileBuilder.WorldGenerator;
                var aWorld = aTileBuilder.World;
                var aTrabantCountRange = this.TrabantCountRange;
                var aHasTrabants = this.TrabantPropability >= aWorldGenerator.NextDouble();
                var aTrabantRange = this.TrabantCountRange;
                var aTrabantCount =  aHasTrabants 
                                  ? aWorldGenerator.NextInteger(aTrabantCountRange.Item1, aTrabantCountRange.Item2)
                                  : 0;
                var aTrabants = new CTrabant[aTrabantCount];
                if (aTrabantCount > 0)
                {
                    var aOrbitRange = this.TrabantOrbitRange;
                    var aOrbRadius = this.Radius;
                    var aOrbitRangeMin = aOrbitRange.Item1 * aOrbRadius;
                    var aOrbitRangeMax = aOrbitRange.Item2 * aOrbRadius;
                    var aOrbitRadius = aWorldGenerator.NextDouble(aOrbitRangeMin, aOrbitRangeMax);
                    for (var aIdx = 0; aIdx < aTrabantCount; ++aIdx)
                    {
                        var aTrabantOrbit = ((double)aIdx + 1) * aOrbitRadius;
                        aTrabants[aIdx] = this.NewTrabant(aTrabantOrbit);
                    }
                }
                this.Trabants = aTrabants;
            }
            else
            {
                this.Trabants = Array.Empty<CTrabant>();
            }
        }
        internal virtual bool HasTrabants => false;
        private void DrawTrabants()
        {
            foreach(var aTrabant in this.Trabants)
            {
                aTrabant.Draw();
            }
        }
        #endregion
        internal abstract CFeature OrbVisibleFeature { get; }
        protected override void OnDraw()
        {
            if(this.OrbVisibleFeature.Enabled)
            {
                base.OnDraw();
            }


            this.DrawTrabants();
        }
        protected override void OnBuild()
        {
            base.OnBuild();

            var aTileBuilder = this.TileBuilder;
            var aWorldGenerator = aTileBuilder.WorldGenerator;
            var aWorld = aTileBuilder.World;
            this.GeneratedOrbitPlaneSlope = aWorldGenerator.NextVector3Dbl(Math.PI * 2);
            this.DayDuration  =  TimeSpan.FromSeconds(aWorldGenerator.NextDouble(aWorld.OrbDayDurationMin, aWorld.OrbDayDurationMax));
            this.BuildTrabants();

            //this.GeneratedOrbitPlaneSlope = new CVector3Dbl(Math.PI / 4d, 0, Math.PI / 4d);
        }

        internal TimeSpan DayDuration { get; private set; }

        #region SolarSystem
        internal readonly CSolarSystem SolarSystem;
        #endregion
        #region Features
        [CFeatureDeclaration]
        private static readonly CFeatureDeclaration OrbitPlaneSlopeFeatureDeclaration = new CFeatureDeclaration(new Guid("4f7f5808-f120-4e66-acb5-ac3f0bfa3429"), "SolarSystem.Orbits.PlaneSlope");
        private CFeature OrbitPlaneSlopeFeatureM;
        private CFeature OrbitPlaneSlopeFeature => CLazyLoad.Get(ref this.OrbitPlaneSlopeFeatureM, () => CFeature.Get(this, OrbitPlaneSlopeFeatureDeclaration));
        #endregion

    }

    public abstract class CTrabant : COrb
    {
        internal CTrabant(CServiceLocatorNode aParent, CTileBuilder aTileBuilder, CTileDescriptor aTileDescriptor, double aOrbit) : base(aParent, aTileBuilder, aTileDescriptor)
        {          
            this.OrbitRadius = aOrbit;
        }

        internal override double BuildRadius() => base.BuildRadius() * this.ParentOrb.Radius;

        internal readonly double OrbitRadius;
        public override bool OrbitIsDefined => true;
        public override COrbit Orbit => new COrbit(this.OrbitPlaneSlopeCur, this.ParentOrb.WorldPos, this.OrbitRadius, this.OrbitCurrentRadians);

        internal TimeSpan YearDuration { get; private set; }
        internal double OrbitCurrentRadians => this.SolarSystem.AnimateSolarSystemFeature.Enabled
                                          ? ((this.World.GameTimeTotal.TotalSeconds * Math.PI * 2d / this.YearDuration.TotalSeconds).ToRadians() + this.OrbitStartRadians).AvoidNan()
                                          : 0.0d;
        private double OrbitStartRadians { get; set; }
        internal abstract COrb ParentOrb { get; }
        protected override CVector3Dbl GenerateOriginalWorldPos()
            => this.GetWorldPos(0);
        public override CVector3Dbl WorldPos
            => this.GetWorldPos(this.OrbitCurrentRadians);

        private CVector3Dbl GetWorldPos(double aOrbitRadians)
        {
            var aOrbitRadius = this.OrbitRadius;
            var aOrbitAnglesCur = this.OrbitPlaneSlopeCur;
            var aOrbPos1 = new Vector3((float)aOrbitRadius, 0, 0);              // x    y   z
            var aRotateMatrix = Matrix.CreateRotationY((float)aOrbitRadians);   // yz   xz  xy
            //var aRotateMatrix = aOrbitAnglesCur.ToVector3().ToRotateMatrix();
            var aOrbPos2 = aRotateMatrix.Rotate(aOrbPos1);                      // ..   ..  .
            var aOrbPos3 = aOrbPos2.RotateZyx(aOrbitAnglesCur.Invert().ToVector3());
            var aParentOrbPos = this.ParentOrb.WorldPos;
            var aOrbPos4 = aOrbPos3 + aParentOrbPos.ToVector3();
            return aOrbPos4.ToVector3Dbl();
        }


        internal abstract CDoubleRange TrabantYearDurationRange { get; }

        protected override void OnBuild()
        {
            base.OnBuild();

            this.YearDuration =  TimeSpan.FromSeconds(this.WorldGenerator.NextDouble(this.TrabantYearDurationRange));
            this.OrbitStartRadians = (this.WorldGenerator.NextDouble() * Math.PI * 2d);
        }

    }

    public sealed class CPlanet : CTrabant
    {
        internal CPlanet(CServiceLocatorNode aParent, CTileBuilder aTileBuilder, CTileDescriptor aTileDescriptor, double aOrbitRadius) : base(aParent, aTileBuilder, aTileDescriptor, aOrbitRadius)
        {
            this.Sun = this.ServiceContainer.GetService<CSun>();
            this.Init();
            this.Build();
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        private readonly CSun Sun;
        internal override COrb ParentOrb => this.Sun;
        internal override CVector3Dbl GeneratedOrbitPlaneSlope => this.Sun.GeneratedOrbitPlaneSlope;

        #region HasTrabants
        internal override bool HasTrabants => true;
        internal override CIntegerRange TrabantCountRange => this.World.PlanetMoonCountRange;
        internal override CTrabant NewTrabant(double aOrbitRadius) => new CMoon(this, this.TileBuilder, this.TileDescriptor, aOrbitRadius);
        internal override double TrabantPropability =>this.World.PlanetHasMoonsProbability;
        internal override CDoubleRange TrabantOrbitRange => this.World.MoonOrbitRange;
        internal override CDoubleRange TrabantYearDurationRange => this.World.PlanetYearDurationRange;
        #endregion
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CPlanet>(() => this);
            return aServiceContainer;
        }
        #endregion
        #region Features
        [CFeatureDeclaration]
        private static readonly CFeatureDeclaration PlanetsVisibleFeatureDeclaration = new CFeatureDeclaration(new Guid("3412f4ab-14f3-447d-81fe-923f57993019"), "SolarSystem.Planets.Visible");
        private CFeature PlanetsVisibleFeatureM;
        private CFeature PlanetsVisibleFeature => CLazyLoad.Get(ref this.PlanetsVisibleFeatureM, () => CFeature.Get(this, PlanetsVisibleFeatureDeclaration));
        internal override CFeature OrbVisibleFeature => this.PlanetsVisibleFeature;
        #endregion
        public override string CategoryName => "Planet";
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
        internal override CDoubleRange BumperRadiusMax => this.World.MoonRadiusMax;
        protected override void OnDraw()
        {
            base.OnDraw();
        }
        internal override CDoubleRange TrabantYearDurationRange => this.World.MoonYearDurationRange;
        internal override bool Visible => base.Visible && this.MoonsVisibleFeature.Enabled;
        #region Features
        [CFeatureDeclaration]
        private static readonly CFeatureDeclaration MoonsVisibleFeatureDeclaration = new CFeatureDeclaration(new Guid("7962faea-31d9-482d-aab4-60667b797d54"), "SolarSystem.Moons.Visible");
        private CFeature MoonsVisibleFeatureM;
        private CFeature MoonsVisibleFeature => CLazyLoad.Get(ref this.MoonsVisibleFeatureM, () => CFeature.Get(this, MoonsVisibleFeatureDeclaration));
        internal override CFeature OrbVisibleFeature => this.MoonsVisibleFeature;
        #endregion
        public override string CategoryName => "Moon";
    }

    internal sealed class CSun : COrb
    {
        #region ctor
        internal CSun(CServiceLocatorNode aParent, CTileBuilder aTileBuilder, CQuadrantTileDescriptor aTileDescriptor) : base(aParent, aTileBuilder, aTileDescriptor)
        {
            this.QuadrantTileDescriptor = aTileDescriptor;
            this.Init();
            this.Build();
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        #endregion
        private readonly CQuadrantTileDescriptor QuadrantTileDescriptor;
        protected override CVector3Dbl GenerateOriginalWorldPos()
            => this.QuadrantTileDescriptor.QuadrantSpriteData.Center; // TODO
        internal Vector3 OrbitAxis => new Vector3(1, 1, 1);
        internal override CDoubleRange BumperRadiusMax => this.World.SunRadiusMax;


        protected override void OnBuild()
        {
            base.OnBuild();
        }
        #region Trabants
        internal override CIntegerRange TrabantCountRange => new CIntegerRange(this.World.TileBumperCountMin, this.World.TileBumperCountMax);
        internal override bool HasTrabants => true;
        internal override CTrabant NewTrabant(double aOrbitRadius)
            => new CPlanet(this, this.TileBuilder, this.TileDescriptor, aOrbitRadius);
        internal override CDoubleRange TrabantOrbitRange => this.World.PlanetOrbitRange;
        #endregion
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CSun>(() => this);
            return aServiceContainer;
        }
        #endregion
        #region Features
        [CFeatureDeclaration]
        private static readonly CFeatureDeclaration SunsVisibleFeatureDeclaration = new CFeatureDeclaration(new Guid("afeb49da-5453-49d5-87ac-94d703e6cb3b"), "SolarSystem.Suns.Visible");
        private CFeature SunsVisibleFeatureM;
        private CFeature SunsVisibleFeature => CLazyLoad.Get(ref this.SunsVisibleFeatureM, () => CFeature.Get(this, SunsVisibleFeatureDeclaration));
        internal override CFeature OrbVisibleFeature => this.SunsVisibleFeature;
        #endregion
        public override string CategoryName => "Sun";
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
        }
        protected override void OnBuild()
        {
            base.OnBuild();

            var aTileBuilder = this.TileBuilder;
            this.Sun = new CSun(this, this.TileBuilder, this);
        }
        private CSun Sun;
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CSolarSystem>(() => this);
            return aServiceContainer;
        }
        #endregion
        #region Features
        [CFeatureDeclaration]
        private static CFeatureDeclaration AnimateSolarSystemFeatureDeclaration = new CFeatureDeclaration(new Guid("cb53ccd6-dc7e-496f-a720-cbab040e5234"), "SolarSystem.Animate");
        private CFeature AnimateSolarSystemFeatureM;
        internal CFeature AnimateSolarSystemFeature => CLazyLoad.Get(ref this.AnimateSolarSystemFeatureM, () => CFeature.Get(this, AnimateSolarSystemFeatureDeclaration));
        #endregion

    }
}
