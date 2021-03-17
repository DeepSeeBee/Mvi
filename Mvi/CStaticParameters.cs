using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi
{
    using CDoubleRange = System.Tuple<double, double>;
    using CTimeSpanRange = System.Tuple<TimeSpan, TimeSpan>;

    public static class CStaticParameters
    {
        public const bool Quadrant_Asteroids = true;
        public const bool Quadrant_SolarSystem = true;

        public const bool Value_SolarSystem_Animate = true;
        public const bool Value_SolarSystem_Sun_Visible = true;
        public const bool Value_SolarSystem_Planet_Visible = true;
        public const bool Value_SolarSystem_Moon_Visible = true;
        public const bool Value_SolarSystem_Orbit_Visible = true;
        public const bool Value_Origin_Visible = true;
        public const bool Value_Joystick = true;
        public const bool Value_QuadrantGridLines = false;
        public const bool Value_AccumulativeViewMatrix = true;

        public const int Cube_Size = 2;
        public const int Cube_Count = 2;
        public const Int64 Cube_Pos_Max = 100000;
        internal const int Cube_EdgeLen = Cube_Size * 2 + 1;
        internal static int Cube_QuadrantCount = Cube_EdgeLen * Cube_EdgeLen * Cube_EdgeLen;

        public const int TileAsteroidCountMin = 0;
        public const int TileAsteroidCountMax = 3;
        public const int SunTrabantCountMin = 3;// 3;
        public const int SunTrabantCountMax = 7;//10;

        public static readonly CDoubleRange DefaultAsteroidRadiusMax = new CDoubleRange(0.01d, 0.04d);
        public static readonly CDoubleRange SunRadiusMax = new CDoubleRange(0.025d, 0.04d);
        public static readonly CDoubleRange PlanetRadiusMax = new CDoubleRange(0.5d, 0.7d);
        public static readonly CDoubleRange MoonRadiusMax = new CDoubleRange(0.5d, 0.6d);
        public static readonly double SolarSystem_InheritOrbPlaneSlopePropabiltiy = 0.7d;

        public static readonly CDoubleRange Shot_SpeedRange = new CDoubleRange(0.1d, 4d);
        public static readonly double Shot_DieOnDistanceToAvatar = 2d;
        public static readonly CDoubleRange Shot_RadiusRange = new CDoubleRange(0.0005, 0.05);
        public static readonly CTimeSpanRange Shot_FireRateRange = new CTimeSpanRange(TimeSpan.FromMilliseconds(75), TimeSpan.FromMilliseconds(750));
        public static readonly CTimeSpanRange Shot_TimeToLive = new CTimeSpanRange(TimeSpan.FromMilliseconds(3000), TimeSpan.FromMilliseconds(6000));

        public const bool Gravity_Enabled = false;
        public const double Gravity_MassMultiply = 3000d;
        public const double Gravity_G = 0.000000000066743d;
        public const double Gravity_NoImpactDistance = 1d;
        public const bool Sound_Loading_Enabled = true;

        public const int ExplosionCountMax = 10; // TODO-Not deterministic.
        public const int Shot_Count_Max = 20;
        public const int Gem_Class_InstanceCount = 10;

        public const double Gem_ShellRepair_ModifierValue = 0.1d;
        public const double Gem_Shield_ModifierValue = 0.1d;
        public const double Gem_AmmoEnergy_ModifierValue = 0.1d;
        public const double Gem_AmmoThickness_ModifierValue = 0.1d;
        public const double Gem_AmmoSpeed_ModifierValue = 0.1d;
        public const double Gem_AmmoFireRate_ModifierValue = 0.1d;
        public const double Gem_SlowMotion_ModifierValue = -0.1d;
        public const double Gem_AntiGravity_ModifierValue = 0.1d;
        public const double Gem_SpaceGrip_ModifierValue = 0.1d;

        public static readonly TimeSpan Gem_ThermalShield_ModifierValue = TimeSpan.FromSeconds(20);

        public const Int64 Gem_GuidedMissile_ModifierValue = 1;
        public const Int64 Gem_NuclearMissile_ModifierValue = 1;
        public const Int64 Gem_Drill_ModifierValue = 1;
        public const Int64 Gem_KruskalScanner_ModifierValue = 1;

        public static readonly TimeSpan  Gem_ActiveTime = new TimeSpan(0, 0, 90); 
    }
}
