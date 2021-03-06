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
        public const bool Feature_SolarSystem_Animate = true;
        public const bool Feature_SolarSystem_Sun_Visible = true;
        public const bool Feature_SolarSystem_Planet_Visible = true;
        public const bool Feature_SolarSystem_Moon_Visible = true;
        public const bool Feature_SolarSystem_Orbit_Visible = true;
        public const bool Feature_Origin_Visible = true;
        public const bool Feature_Joystick = true;
        public const bool Feature_QuadrantGridLines = false;


        public const int Cube_Size = 2;
        public const Int64 Cube_Pos_Max = 100000;

        public const int TileAsteroidCountMin = 3;
        public const int TileAsteroidCountMax = 10;

        public static readonly CDoubleRange DefaultAsteroidRadiusMax = new CDoubleRange(0.00001d, 0.01d);
        public static readonly CDoubleRange SunRadiusMax = new CDoubleRange(0.025d, 0.05d);
        public static readonly CDoubleRange PlanetRadiusMax = new CDoubleRange(0.5d, 0.8d);
        public static readonly CDoubleRange MoonRadiusMax = new CDoubleRange(0.3d, 0.6d);
        
        public const  double Shot_MinSpeed = 1.4;
        public const double Shot_DistanceToAvatarWhenDead = 2.0;
        public static readonly TimeSpan Shot_FireRate = new TimeSpan(0, 0, 0, 0, 333);


        public const bool Sound_Loading_Enabled = true;
    }
}
