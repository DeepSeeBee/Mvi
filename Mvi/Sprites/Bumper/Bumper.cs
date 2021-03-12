using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.World;
using Mvi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CharlyBeck.Mvi.Models;
using CharlyBeck.Utils3.ServiceLocator;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;

using CDoubleRange = System.Tuple<double, double>;
using CharlyBeck.Mvi.Cube.Mvi;
using Utils3.Asap;
using CharlyBeck.Mvi.Sprites.Asteroid;
using CharlyBeck.Mvi.Sfx;

namespace CharlyBeck.Mvi.Sprites.Bumper
{

    using COrbit = Tuple<CVector3Dbl, CVector3Dbl, double, double>; // OrbitAngles, OrbitCenter, OrbitRadius, OrbitCurrentRadians

    public sealed class CBumperModel : CModel
    {
        internal CBumperModel(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Sphere = new CSphere(4, 1.0f, true);
            var aLen = this.World.SphereScaleCount; // 33;
            this.Spheres = (from aIdx in Enumerable.Range(0, aLen) select new CSphere(aIdx + 1, 1.0d, true)).ToArray();
            this.Circles = new CCircles(3, 100);
        }
        public readonly COctaeder Octaeder = new COctaeder(0.01);
        public readonly CSphere Sphere;
        public readonly CSphere[] Spheres;
        public readonly CCircles Circles;

        public int GetSphereIdx(double aDistanceToSurface)
        {
            var aBase = 4;
            var aStartScaleAt = 0.1; // TODO -  in cworld ablegen 
            var aMaxScaleAt = 0.001;
            if (aDistanceToSurface > aStartScaleAt)
                return aBase;
            else if (aDistanceToSurface < 0d)
            {
                return this.Spheres.Length - 1;
            }
            else
            {
                var d = Math.Max(aMaxScaleAt, aDistanceToSurface);
                var r = aStartScaleAt - aMaxScaleAt;
                var f = 1 - (d - aMaxScaleAt) / r;
                var i = (int)(((double)this.Spheres.Length - 1) * f);
                return Math.Max(aBase, i);
            }
        }
    }

    public abstract class CBumperSprite : CSprite
    {
        private static readonly CEnums<CAccelerateEnum> AllAccelerateEnums = new CEnums<CAccelerateEnum>();

        internal CBumperSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.SetCollisionIsEnabled(CCollisionSourceEnum.Shot, true);
            this.PersistencyEnabled = true;
            this.PlattformSpriteEnum = CPlatformSpriteEnum.Bumper;
        }
        internal CDoubleRange AsteroidRadiusMax;

        protected override void OnEndUse()
        {
            base.OnEndUse();

            this.Orbit = default;
            this.WorldPos = default;
            this.Radius = default;
            this.Color = default;
        }

        internal override void Build(CSpriteBuildArgs a)
        {
            base.Build(a);
            var aRandomGenerator = a.QuadrantBuildArgs.RandomGenerator;
            var aWorld = this.World;
            this.WorldPos = this.GetRandomWorldPos(aRandomGenerator);
            //this.OriginalWorldPos = this.GenerateOriginalWorldPos(aRandomGenerator);
            this.Radius = this.BuildRadius(aRandomGenerator); 
            this.Color = aRandomGenerator.NextWorldPos();
            this.TargetCubePos = aRandomGenerator.NextCubePos();
        }
        internal virtual double BuildRadius(CRandomGenerator aRandomGenerator)
            => aRandomGenerator.NextFromDoubleRange(this.AsteroidRadiusMax);

        //internal abstract CVector3Dbl GenerateOriginalWorldPos(CRandomGenerator aRandomGenerator);

        internal override int ChangesCount => (int)CChangeEnum._Count;

        internal bool WarpIsActive { get; set; }
        public CVector3Dbl Color { get; private set; }
        internal CCubePos TargetCubePos { get; private set; }
        public CBumperModel AsteroidModel => this.World.Models.AsteroidModel;
        public virtual bool OrbitIsDefined => false;
        public  COrbit Orbit;
        internal enum CChangeEnum
        {
            _Count
        }

        public string CategoryName;
        public string VmCategoryName => this.CategoryName;

        internal enum CAccelerateEnum
        {
            Gravity,
            Accelerate
        }

        public double AvatarDistanceToSurface { get; private set; }
        public bool IsBelowSurface { get; private set; }
        internal bool IsBelowSurfaceInWarpArea { get; private set; }
        public bool IsNearestAsteroidToAvatar { get; set; }

        internal override void UpdateAvatarPos()
        {
            base.UpdateAvatarPos();

            this.AvatarDistanceToSurface = this.DistanceToAvatar - this.Radius.Value;
            this.IsBelowSurface =  this.DistanceToAvatar < this.Radius;
            this.IsBelowSurfaceInWarpArea = this.DistanceToAvatar < this.Radius / 2;
        }
        internal override void Update(CFrameInfo aFrameInfo)
        {
            base.Update(aFrameInfo);
            this.IsNearestAsteroidToAvatar = aFrameInfo.NearestBumperIsDefined
                                        && aFrameInfo.NearestAsteroid.RefEquals<CBumperSprite>(this);
            this.WormholeCubes.Swap(this);
        }

        #region Cube
        private CWormholeCubes WormholeCubesM;

        private CWormholeCubes WormholeCubes => CLazyLoad.Get(ref this.WormholeCubesM, () => this.ServiceContainer.GetService<CWormholeCubes>());
        #endregion
    }


}
