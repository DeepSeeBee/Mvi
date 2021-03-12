using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi
{
    using CDoubleRange = System.Tuple<double, double>;

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

        public const int TileAsteroidCountMin = 3;
        public const int TileAsteroidCountMax = 7;
        public const int SunTrabantCountMin = 3;// 3;
        public const int SunTrabantCountMax = 7;//10;

        public static readonly CDoubleRange DefaultAsteroidRadiusMax = new CDoubleRange(0.0001d, 0.01d);
        public static readonly CDoubleRange SunRadiusMax = new CDoubleRange(0.025d, 0.04d);
        public static readonly CDoubleRange PlanetRadiusMax = new CDoubleRange(0.5d, 0.7d);
        public static readonly CDoubleRange MoonRadiusMax = new CDoubleRange(0.5d, 0.6d);
        public static readonly double SolarSystem_InheritOrbPlaneSlopePropabiltiy = 0.7d;

        public const  double Shot_MinSpeed = 1.4;
        public const double Shot_DistanceToAvatarWhenDead = 2.0;
        public static readonly TimeSpan Shot_FireRate = new TimeSpan(0, 0, 0, 0, 333);
        public const bool Gravity_Enabled = false;
        public const double Gravity_MassMultiply = 3000d;
        public const double Gravity_G = 0.000000000066743d;
        public const double Gravity_NoImpactDistance = 1d;
        public const bool Sound_Loading_Enabled = true;

        public const int ExplosionCountMax = 10; // TODO-Not deterministic.
        public const int Shot_Count_Max = 20;

        public static readonly TimeSpan Shot_TimeToLive = new TimeSpan(0, 0, 0, 0, 1333);
    }
}
