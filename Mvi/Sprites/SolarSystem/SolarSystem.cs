﻿using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Cube.Mvi;
using CharlyBeck.Mvi.Extensions;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Feature;
using CharlyBeck.Mvi.Sprites.Asteroid;
using CharlyBeck.Mvi.Sprites.Bumper;
using CharlyBeck.Mvi.Sprites.Cube;
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
using Utils3.Asap;

namespace CharlyBeck.Mvi.Sprites.SolarSystem
{
    using CDoubleRange = Tuple<double, double>;
    using CIntegerRange = Tuple<int, int>;
    using COrbit = Tuple<CVector3Dbl, CVector3Dbl, double, double>; // OrbitAngles, OrbitCenter, OrbitRadius, OrbitCurrentRadians


    public abstract class COrb : CBumperSprite
    {
        #region ctor
        internal COrb(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        internal override void Build(CSpriteBuildArgs a)
        {
            base.Build(a);
            var aWorld = this.World;
            var aRandomGenerator = a.QuadrantBuildArgs.RandomGenerator;
            this.GeneratedOrbitPlaneSlope = aRandomGenerator.NextVector3Dbl(Math.PI * 2);
            this.DayDuration = TimeSpan.FromSeconds(aRandomGenerator.NextDouble(aWorld.OrbDayDurationMin, aWorld.OrbDayDurationMax));
            this.BuildTrabants(a);
        }
        protected override void OnEndUse()
        {
            base.OnEndUse();

            this.Trabants.DeallocateItems();
            this.GeneratedOrbitPlaneSlope = default;
            this.DayDuration = default;
            this.Trabants = default;
        }
        #endregion

        internal IEnumerable<CSprite> Sprites
        {
            get
            {
                yield return this;
                foreach (var aSprite in from aTrabant in this.Trabants from aSprite in aTrabant.Sprites select aSprite)
                    yield return aSprite;
            }
        }

        internal override CDoubleRange AsteroidRadiusMax => this.World.PlanetRadiusMax;

        internal virtual CVector3Dbl GeneratedOrbitPlaneSlope { get; set; }
        internal CVector3Dbl OrbitPlaneSlopeCur => this.OrbitPlaneSlopeFeature.Enabled ? this.GeneratedOrbitPlaneSlope : new CVector3Dbl(0d);

        #region Trabants
        internal virtual double TrabantPropability => 1d;
        internal virtual CIntegerRange TrabantCountRange => new CIntegerRange(0,0);
        internal virtual CTrabant NewTrabant() => throw new NotImplementedException();
        internal virtual CDoubleRange TrabantOrbitRange => this.Throw<CDoubleRange>(new NotImplementedException());
        private CTrabant[] Trabants;
        private void BuildTrabants(CSpriteBuildArgs aSpriteBuildArgs)
        {
            if (this.HasTrabants)
            {
                var aWorld = this.World;
                var aRandomGenerator = aSpriteBuildArgs.QuadrantBuildArgs.RandomGenerator;
                var aTrabantCountRange = this.TrabantCountRange;
                var aHasTrabants = this.TrabantPropability >= aRandomGenerator.NextDouble();
                var aTrabantCount =  aHasTrabants 
                                  ? aRandomGenerator.NextInteger(aTrabantCountRange.Item1, aTrabantCountRange.Item2)
                                  : 0;
                this.Trabants.DeallocateItems();
                var aTrabants = new CTrabant[aTrabantCount];
                if (aTrabantCount > 0)
                {
                    var aOrbitRange = this.TrabantOrbitRange;
                    var aOrbRadius = this.Radius;
                    var aOrbitRangeMin = aOrbitRange.Item1 * aOrbRadius;
                    var aOrbitRangeMax = aOrbitRange.Item2 * aOrbRadius;
                    var aOrbitRadius = aRandomGenerator.NextDouble(aOrbitRangeMin.Value, aOrbitRangeMax.Value);
                    for (var aIdx = 0; aIdx < aTrabantCount; ++aIdx)
                    {
                        var aTrabantOrbit = ((double)aIdx + 1) * aOrbitRadius;
                        var aTrabant = this.NewTrabant();
                        aTrabant.OrbitRadius = aTrabantOrbit;
                        aTrabant.OrbitRadius = aTrabantOrbit;
                        aTrabant.Build(aSpriteBuildArgs);
                        aTrabants[aIdx] = aTrabant;
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
        internal override void Draw()
        {
            if (this.OrbVisibleFeature.Enabled)
            {
                base.Draw();
            }
            this.DrawTrabants();
        }
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override  CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<COrb>(() => this);
            return aServiceContainer;
        }
        #endregion

        internal TimeSpan DayDuration { get; private set; }

        #region Features
        [CFeatureDeclaration]
        private static readonly CFeatureDeclaration OrbitPlaneSlopeFeatureDeclaration = new CFeatureDeclaration(new Guid("4f7f5808-f120-4e66-acb5-ac3f0bfa3429"), "SolarSystem.Orbits.PlaneSlope", CStaticParameters.Feature_SolarSystem_Orbit_Visible);
        private CFeature OrbitPlaneSlopeFeatureM;
        private CFeature OrbitPlaneSlopeFeature => CLazyLoad.Get(ref this.OrbitPlaneSlopeFeatureM, () => CFeature.Get(this, OrbitPlaneSlopeFeatureDeclaration));
        #endregion
        #region Features
        [CFeatureDeclaration]
        private static CFeatureDeclaration AnimateSolarSystemFeatureDeclaration = new CFeatureDeclaration(new Guid("cb53ccd6-dc7e-496f-a720-cbab040e5234"), "SolarSystem.Animate", CStaticParameters.Feature_SolarSystem_Animate);
        private CFeature AnimateSolarSystemFeatureM;
        internal CFeature AnimateSolarSystemFeature => CLazyLoad.Get(ref this.AnimateSolarSystemFeatureM, () => CFeature.Get(this, AnimateSolarSystemFeatureDeclaration));
        #endregion

    }

    public abstract class CTrabant : COrb
    {
        internal CTrabant(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        internal override void Build(CSpriteBuildArgs a)
        {
            base.Build(a);
            var aRandomGenerator = a.QuadrantBuildArgs.RandomGenerator;
            this.YearDuration = TimeSpan.FromSeconds(aRandomGenerator.NextDouble(this.TrabantYearDurationRange));
            this.OrbitStartRadians = (aRandomGenerator.NextDouble() * Math.PI * 2d);
        }

        protected override void OnEndUse()
        {
            base.OnEndUse();
            this.YearDuration = default;
            this.OrbitStartRadians = default;
        }

        internal override double BuildRadius(CRandomGenerator aRandomGenerator) => base.BuildRadius(aRandomGenerator) * this.ParentOrb.Radius.Value;

        internal double? OrbitRadius;
        public override bool OrbitIsDefined => true;
        public override COrbit Orbit => new COrbit(this.OrbitPlaneSlopeCur, this.ParentOrb.WorldPos, this.OrbitRadius.Value, this.OrbitCurrentRadians);

        internal TimeSpan YearDuration { get; private set; }
        internal double OrbitCurrentRadians => this.AnimateSolarSystemFeature.Enabled
                                          ? ((this.World.GameTimeTotal.TotalSeconds * Math.PI * 2d / this.YearDuration.TotalSeconds).ToRadians() + this.OrbitStartRadians).AvoidNan()
                                          : 0.0d;
        private double OrbitStartRadians { get; set; }
        internal abstract COrb ParentOrb { get; }
        internal override CVector3Dbl GenerateOriginalWorldPos(CRandomGenerator aRandomGenerator)
            => this.GetWorldPosByOrbitRadians(0d);
        public override CVector3Dbl WorldPos
            => this.GetWorldPosByOrbitRadians(this.OrbitCurrentRadians);

        private CVector3Dbl GetWorldPosByOrbitRadians(double aOrbitRadians)
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

    }

    public sealed class CPlanet : CTrabant
    {
        #region ctor
        internal CPlanet(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        #endregion
        #region Sun
        internal CSun Sun;
        internal override COrb ParentOrb => this.Sun;
        #endregion
        #region Trabants
        internal override bool HasTrabants => true;
        internal override CIntegerRange TrabantCountRange => this.World.PlanetMoonCountRange;
        internal override CTrabant NewTrabant()
        {
            var aMoon = this.SpritePool.NewMoon();
            aMoon.Planet = this;
            return aMoon;
        }
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
        private static readonly CFeatureDeclaration PlanetsVisibleFeatureDeclaration = new CFeatureDeclaration(new Guid("3412f4ab-14f3-447d-81fe-923f57993019"), "SolarSystem.Planets.Visible", CStaticParameters.Feature_SolarSystem_Planet_Visible);
        private CFeature PlanetsVisibleFeatureM;
        private CFeature PlanetsVisibleFeature => CLazyLoad.Get(ref this.PlanetsVisibleFeatureM, () => CFeature.Get(this, PlanetsVisibleFeatureDeclaration));
        internal override CFeature OrbVisibleFeature => this.PlanetsVisibleFeature;
        #endregion
        #region CategorName
        public override string CategoryName => "Planet";
        #endregion
        internal override CPlatformSpriteEnum PlattformSpriteEnum => CPlatformSpriteEnum.Bumper;
    }

    public sealed class CMoon : CTrabant
    {
        internal CMoon(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        protected override void OnEndUse()
        {
            base.OnEndUse();
            this.Planet = default;
            this.OrbitRadius = default;
        }
        internal CPlanet Planet;
        internal override COrb ParentOrb => this.Planet;
        internal override CDoubleRange AsteroidRadiusMax => this.World.MoonRadiusMax;
        internal override CDoubleRange TrabantYearDurationRange => this.World.MoonYearDurationRange;
        internal override bool Visible => base.Visible && this.MoonsVisibleFeature.Enabled;

        #region Features
        [CFeatureDeclaration]
        private static readonly CFeatureDeclaration MoonsVisibleFeatureDeclaration = new CFeatureDeclaration(new Guid("7962faea-31d9-482d-aab4-60667b797d54"), "SolarSystem.Moons.Visible", CStaticParameters.Feature_SolarSystem_Moon_Visible);
        private CFeature MoonsVisibleFeatureM;
        private CFeature MoonsVisibleFeature => CLazyLoad.Get(ref this.MoonsVisibleFeatureM, () => CFeature.Get(this, MoonsVisibleFeatureDeclaration));
        internal override CFeature OrbVisibleFeature => this.MoonsVisibleFeature;
        #endregion
        public override string CategoryName => "Moon";
    }

    internal sealed class CSun : COrb
    {
        #region ctor
        internal CSun(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        internal override void Build(CSpriteBuildArgs a)
        {
            base.Build(a);
        }
        #endregion
        internal override CVector3Dbl GenerateOriginalWorldPos(CRandomGenerator aRandomGenerator)
            => this.GenerateDefaultWorldPos(aRandomGenerator); // this.QuadrantTileDescriptor.QuadrantSpriteData.Center; // TODO
        internal Vector3 OrbitAxis => new Vector3(1, 1, 1);
        internal override CDoubleRange AsteroidRadiusMax => this.World.SunRadiusMax;


        //protected override void OnBuild()
        //{
        //    base.OnBuild();
        //}
        #region Trabants
        internal override CIntegerRange TrabantCountRange => new CIntegerRange(this.World.TileAsteroidCountMin, this.World.TileAsteroidCountMax);
        internal override bool HasTrabants => true;
        internal override CTrabant NewTrabant()
        {
            var aPlanet = this.SpritePool.NewPlanet();
            aPlanet.Sun = this;
            return aPlanet;
        }
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
        private static readonly CFeatureDeclaration SunsVisibleFeatureDeclaration = new CFeatureDeclaration(new Guid("afeb49da-5453-49d5-87ac-94d703e6cb3b"), "SolarSystem.Suns.Visible", CStaticParameters.Feature_SolarSystem_Sun_Visible);
        private CFeature SunsVisibleFeatureM;
        private CFeature SunsVisibleFeature => CLazyLoad.Get(ref this.SunsVisibleFeatureM, () => CFeature.Get(this, SunsVisibleFeatureDeclaration));
        internal override CFeature OrbVisibleFeature => this.SunsVisibleFeature;
        #endregion
        public override string CategoryName => "Sun";
    }

    internal sealed class CSolarSystemQuadrant : CSpaceQuadrant
    {
        #region ctor
        public CSolarSystemQuadrant(CServiceLocatorNode aParent) : base(aParent)
        { 
            this.Sun = new CSun(this);
        }
        protected override void OnEndUse()
        {
            base.OnEndUse();
        }
        internal override void Build(CQuadrantBuildArgs a)
        {
            base.Build(a);
            this.Sun.Build(a);
        }
        #endregion
        public override IEnumerable<CSprite> Sprites => this.Sun.Sprites;
        //protected override void OnDraw()
        //{
        //    base.OnDraw();

        //    this.Sun.Draw();
        //}
        //protected override void OnBuild()
        //{
        //    base.OnBuild();

        //    var aTileBuilder = this.TileBuilder;
        //    this.Sun = new CSun(this, this.TileBuilder, this);
        //}
        private CSun Sun;
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CSolarSystemQuadrant>(() => this);
            return aServiceContainer;
        }
        #endregion
        
    }
}