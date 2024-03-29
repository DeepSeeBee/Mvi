﻿using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Cube.Mvi;
using CharlyBeck.Mvi.Extensions;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Value;
using CharlyBeck.Mvi.Sfx;
using CharlyBeck.Mvi.Sprites.Asteroid;
using CharlyBeck.Mvi.Sprites.Bumper;
using CharlyBeck.Mvi.World;
using CharlyBeck.Mvi.XnaExtensions;
using CharlyBeck.Utils3.Reflection;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharlyBeck.Utils3.Asap;
using CharlyBeck.Mvi.Sprites.GridLines;

namespace CharlyBeck.Mvi.Sprites.SolarSystem
{
    using CDoubleRange = Tuple<double, double>;
    using CIntegerRange = Tuple<int, int>;
    using COrbit = Tuple<CVector3Dbl, CVector3Dbl, double, double>; // OrbitAngles, OrbitCenter, OrbitRadius, OrbitCurrentRadians // TODO_OPT

    public abstract class COrb : CBumperSprite
    {
        #region ctor
        internal COrb(CServiceLocatorNode aParent) : base(aParent)
        {
            this.PlaysFlybySound = true;
            this.MassIsDefined = true;
            this.AsteroidRadiusMax = this.World.PlanetRadiusMax;
        }

        private CQuadrantSpriteManager SolarSystemSpriteManagerM;
        internal CQuadrantSpriteManager SolarSystemSpriteManager => CLazyLoad.Get(ref this.SolarSystemSpriteManagerM, () => this.ServiceContainer.GetService<CQuadrantSpriteManager>());

        protected override void OnBeginUse()
        {
            base.OnBeginUse();
            this.ParentOrb = default;
        }

        internal override void Build(CSpriteBuildArgs a)
        {
            base.Build(a);

            var aWorld = this.World;
            var aRandomGenerator = a.QuadrantBuildArgs.RandomGenerator;
            this.OrbitPlaneSlopeMaxM = a.QuadrantBuildArgs.RandomGenerator.NextDouble();
            this.OrbitPlaneSlopeMax = this.ParentOrbIsDefined ? this.ParentOrb.OrbitPlaneSlopeMax : this.OrbitPlaneSlopeMaxM.Value;
            this.GeneratedOrbitPlaneSlope = aRandomGenerator.NextVector3Dbl(Math.PI * 2) * new CVector3Dbl(this.OrbitPlaneSlopeMax);
            this.OrbitPlaneSlopeCur = this.OrbitPlaneSlopeValue.Value ? this.GeneratedOrbitPlaneSlope : new CVector3Dbl(0d);            
            this.DayDuration = TimeSpan.FromSeconds(aRandomGenerator.NextDouble(aWorld.OrbDayDurationMin, aWorld.OrbDayDurationMax));
            if(!this.ParentOrbIsDefined)
            {
                this.OwnInheritOrbPlaneSlope = aRandomGenerator.NextDouble() < CStaticParameters.SolarSystem_InheritOrbPlaneSlopePropabiltiy;
            }
            this.BuildTrabants(a);
        }
        protected override void OnEndUse()
        {
            base.OnEndUse();

            this.Trabants.DeallocateItems();
            this.GeneratedOrbitPlaneSlope = default;
            this.DayDuration = default;
            this.Trabants = default;
            this.OwnInheritOrbPlaneSlope = default;
            this.OrbitPlaneSlopeMaxM = default;
            this.OrbitPlaneSlopeCur = default;
            this.OrbitPlaneSlopeMax = default;
        }
        internal double? OrbitPlaneSlopeMaxM;
        #endregion
        internal bool? OwnInheritOrbPlaneSlope;
        internal COrb ParentOrb;
        internal bool ParentOrbIsDefined => this.ParentOrb is object;
        internal double OrbitPlaneSlopeMax;
        internal IEnumerable<CSprite> Sprites
        {
            get
            {
                yield return this;
                foreach (var aSprite in from aTrabant in this.Trabants from aSprite in aTrabant.Sprites select aSprite)
                    yield return aSprite;
            }
        }

        internal CVector3Dbl? GeneratedOrbitPlaneSlope;
        internal CVector3Dbl? OrbitPlaneSlopeCur;

        internal override void UpdateAvatarPos()
        {
            base.UpdateAvatarPos();


        }

        #region Trabants
        internal virtual double TrabantPropability => 1d;
        internal virtual CIntegerRange TrabantCountRange => new CIntegerRange(0,0);
        internal virtual CTrabant AllocateTrabantNullable() => throw new NotImplementedException();
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
                        var aTrabant = this.AllocateTrabantNullable();
                        if(aTrabant is object)
                        {
                            aTrabant.OrbitRadius = aTrabantOrbit;
                            aTrabant.OrbitRadius = aTrabantOrbit;
                            aTrabant.Build(aSpriteBuildArgs);
                        }
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
        internal abstract CBoolValue OrbVisibleValue { get; }
        internal override void Draw()
        {

            if (this.OrbVisibleValue.Value)
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
        #region Values
        [CMemberDeclaration]
        private static readonly CBoolDeclaration OrbitPlaneSlopeValueDeclaration = new CBoolDeclaration
            ( CValueEnum.Global_SolarSystem_OrbitPlaneSlope, new Guid("4f7f5808-f120-4e66-acb5-ac3f0bfa3429"), true, CStaticParameters.Value_SolarSystem_Orbit_Visible);
        private CBoolValue OrbitPlaneSlopeValueM;
        private CBoolValue OrbitPlaneSlopeValue => CLazyLoad.Get(ref this.OrbitPlaneSlopeValueM, () => CValue.GetStaticValue<CBoolValue>(this, OrbitPlaneSlopeValueDeclaration));
        #endregion
        #region Values
        [CMemberDeclaration]
        private static CValueDeclaration AnimateSolarSystemValueDeclaration = new CBoolDeclaration
            ( CValueEnum.Global_SolarSystem_Animate, new Guid("cb53ccd6-dc7e-496f-a720-cbab040e5234"), true, CStaticParameters.Value_SolarSystem_Animate);
        private CBoolValue AnimateSolarSystemValueM;
        internal CBoolValue AnimateSolarSystemValue => CLazyLoad.Get(ref this.AnimateSolarSystemValueM, () => CValue.GetStaticValue<CBoolValue>(this, AnimateSolarSystemValueDeclaration));
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
            this.YearDuration = TimeSpan.FromSeconds(aRandomGenerator.NextFromDoubleRange(this.TrabantYearDurationRange));
            this.OrbitStartRadians = (aRandomGenerator.NextDouble() * Math.PI * 2d);
            this.UpdateCurrentRadians();
            this.WorldPos = this.GetWorldPosByOrbitRadians(0d);
            this.UpdateOrbit();
        }

        protected override void OnEndUse()
        {
            base.OnEndUse();
            this.YearDuration = default;
            this.OrbitStartRadians = default;
            this.OrbitCurrentRadians = default;
            this.Orbit = default;
        }

        internal override double BuildRadius(CRandomGenerator aRandomGenerator) => base.BuildRadius(aRandomGenerator) * this.ParentOrb.Radius.Value;

        internal double? OrbitRadius;
        public override bool OrbitIsDefined => true;

            internal TimeSpan YearDuration { get; private set; }
        internal double? OrbitCurrentRadians;
        private double OrbitStartRadians { get; set; }

        internal override void Update(CFrameInfo aFrameInfo)
        {
            base.Update(aFrameInfo);
            this.Orbit = default;

            this.UpdateCurrentRadians();
            this.WorldPos = this.GetWorldPosByOrbitRadians(this.OrbitCurrentRadians.Value);
            this.UpdateOrbit();
        }

        private void UpdateCurrentRadians()
        {
            this.OrbitCurrentRadians = this.AnimateSolarSystemValue.Value
                                          ? ((this.World.GameTimeTotal.TotalSeconds * Math.PI * 2d / this.YearDuration.TotalSeconds).ToRadians() + this.OrbitStartRadians).AvoidNan()
                                          : 0.0d;
        }
        private void UpdateOrbit()
        {
            this.Orbit = new COrbit(this.OrbitPlaneSlopeCur.Value, this.ParentOrb.WorldPos.Value, this.OrbitRadius.Value, this.OrbitCurrentRadians.Value);
        }

        private CVector3Dbl GetWorldPosByOrbitRadians(double aOrbitRadians)
        {
            var aOrbitRadius = this.OrbitRadius;
            var aOrbitAnglesCur = this.OrbitPlaneSlopeCur;
            var aOrbPos1 = new Vector3((float)aOrbitRadius, 0, 0);              // x    y   z
            var aRotateMatrix = Matrix.CreateRotationY((float)aOrbitRadians);   // yz   xz  xy
            //var aRotateMatrix = aOrbitAnglesCur.ToVector3().ToRotateMatrix();
            var aOrbPos2 = aRotateMatrix.Rotate(aOrbPos1);                      // ..   ..  .
            var aOrbPos3 = aOrbPos2.RotateZyx(aOrbitAnglesCur.Value.Invert().ToVector3());
            var aParentOrbPos = this.ParentOrb.WorldPos.Value;
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
            this.DestroyedSound = CSoundDirectoryEnum.Audio_Destroyed_Planet;
            this.CategoryName = "Planet";
            
            this.Init();
        }
        protected override void OnEndUse()
        {
            base.OnEndUse();
            this.Sun = default;
        }
        #endregion
        #region Sun
        internal CSun Sun;
        #endregion
        #region Trabants
        internal override bool HasTrabants => true;
        internal override CIntegerRange TrabantCountRange => this.World.PlanetMoonCountRange;
        internal override CTrabant AllocateTrabantNullable()
        {
            var aMoon = this.SolarSystemSpriteManager.AllocateMoonNullable();
            if(aMoon is object)
            {
                aMoon.Planet = this;
                aMoon.ParentOrb = this;
            }
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
        #region Values
        [CMemberDeclaration]
        private static readonly CBoolDeclaration PlanetsVisibleValueDeclaration = new CBoolDeclaration
            ( CValueEnum.Global_SolarSystem_PlanetVisible, new Guid("3412f4ab-14f3-447d-81fe-923f57993019"), true, CStaticParameters.Value_SolarSystem_Planet_Visible);
        private CBoolValue PlanetsVisibleValueM;
        private CBoolValue PlanetsVisibleValue => CLazyLoad.Get(ref this.PlanetsVisibleValueM, () => CValue.GetStaticValue<CBoolValue>(this, PlanetsVisibleValueDeclaration));
        internal override CBoolValue OrbVisibleValue => this.PlanetsVisibleValue;
        #endregion
    }

    public sealed class CMoon : CTrabant
    {
        internal CMoon(CServiceLocatorNode aParent) : base(aParent)
        {
            this.DestroyedSound = CSoundDirectoryEnum.Audio_Destroyed_Moon;
            this.AsteroidRadiusMax = this.World.MoonRadiusMax;
            this.CategoryName = "Moon";

            this.Init();
        }
        protected override void OnEndUse()
        {
            base.OnEndUse();
            this.Planet = default;
            this.OrbitRadius = default;
        }
        internal CPlanet Planet;
        internal override CDoubleRange TrabantYearDurationRange => this.World.MoonYearDurationRange;
        internal override bool Visible => base.Visible && this.MoonsVisibleValue.Value;

        #region Values
        [CMemberDeclaration]
        private static readonly CBoolDeclaration MoonsVisibleValueDeclaration = new CBoolDeclaration
            ( CValueEnum.Global_SolarSystem_MoonVisible, new Guid("7962faea-31d9-482d-aab4-60667b797d54"), true, CStaticParameters.Value_SolarSystem_Moon_Visible);
        private CBoolValue MoonsVisibleValueM;
        private CBoolValue MoonsVisibleValue => CLazyLoad.Get(ref this.MoonsVisibleValueM, () => CValue.GetStaticValue<CBoolValue>(this, MoonsVisibleValueDeclaration));
        internal override CBoolValue OrbVisibleValue => this.MoonsVisibleValue;
        #endregion
    }

    internal sealed class CSun : COrb
    {
        #region ctor
        internal CSun(CServiceLocatorNode aParent) : base(aParent)
        {
            this.DestroyedSound = CSoundDirectoryEnum.Audio_Destroyed_Sun;
            this.AsteroidRadiusMax = this.World.SunRadiusMax;
            this.CategoryName = "Sun";

            this.Init();
        }
        internal override void Build(CSpriteBuildArgs a)
        {
            

            base.Build(a);
        }
        protected override void OnEndUse()
        {
            base.OnEndUse();

        }
        #endregion
        internal Vector3 OrbitAxis => new Vector3(1, 1, 1);

        #region Trabants
        internal override CIntegerRange TrabantCountRange => new CIntegerRange(CStaticParameters.SunTrabantCountMin, CStaticParameters.SunTrabantCountMax);
        internal override bool HasTrabants => true;
        internal override CTrabant AllocateTrabantNullable()
        {
            var aPlanet = this.SolarSystemSpriteManager.AllocatePlanetNullable();
            if(aPlanet is object)
            {
                aPlanet.Sun = this;
                aPlanet.ParentOrb = this;
            }
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
        #region Values
        [CMemberDeclaration]
        private static readonly CBoolDeclaration SunsVisibleValueDeclaration = new CBoolDeclaration
            ( CValueEnum.Global_SolarSystem_SunVisible, new Guid("afeb49da-5453-49d5-87ac-94d703e6cb3b"), true, CStaticParameters.Value_SolarSystem_Sun_Visible);
        private CBoolValue SunsVisibleValueM;
        private CBoolValue SunsVisibleValue => CLazyLoad.Get(ref this.SunsVisibleValueM, () => CBoolValue.GetStaticValue<CBoolValue>(this, SunsVisibleValueDeclaration));
        internal override CBoolValue OrbVisibleValue => this.SunsVisibleValue;
        #endregion
        #region Chaos
        [CMemberDeclaration]
        internal static CDoubleDeclaration ChaosDecl = new CDoubleDeclaration
            (CValueEnum.Global_SolarSystem_Chaos, new Guid("ef38d532-b91a-4865-b054-b46a4f1e8773"), true, CGuiEnum.Slider, CUnitEnum.Percent, 0d, 0d, 1d, 0.01d, 0.1d, 0);
        private CDoubleValue ChaosM;
        internal CDoubleValue Chaos => CLazyLoad.Get(ref this.ChaosM, ()=>CValue.GetStaticValue<CDoubleValue>(this, ChaosDecl));
        
        #endregion
    }

    internal sealed class CSolarSystemQuadrantContent : CQuadrantContent
    {
        #region ctor
        public CSolarSystemQuadrantContent(CServiceLocatorNode aParent) : base(aParent)
        { 
        }

        internal override void DeallocateContent()
        {
            base.DeallocateContent();

            if (this.Sun is object)
            {
                this.Sun.Deallocate();
                this.Sun = default;
            }
        }
        protected override void OnEndUse()
        {
            base.OnEndUse();

            this.Sun.Deallocate();
            this.Sun = default;
        }
        internal override void Build(CQuadrantBuildArgs a)
        {
            this.Sun = this.SolarSystemSpriteManager.AllocateSun();
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
            aServiceContainer.AddService<CSolarSystemQuadrantContent>(() => this);
            return aServiceContainer;
        }
        #endregion
        
    }

    internal enum CSolarSystemSpriteEnum
    {
        GridLines,
        Asteroid,
        Sun,
        Planet,
        Moon,
    }


 
}
