﻿using CharlyBeck.Mvi.Cube;
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

namespace CharlyBeck.Mvi.Sprites.Bumper
{

    using COrbit = Tuple<CVector3Dbl, CVector3Dbl, double, double>; // OrbitAngles, OrbitCenter, OrbitRadius, OrbitCurrentRadians



    public abstract class CBumperSprite : CSprite
    {
        private static readonly CEnums<CAccelerateEnum> AllAccelerateEnums = new CEnums<CAccelerateEnum>();

        internal CBumperSprite(CServiceLocatorNode aParent) : base(aParent)
        {

        }
        internal abstract CDoubleRange AsteroidRadiusMax { get; }

        internal override void Build(CSpriteBuildArgs a)
        {
            base.Build(a);
            var aRandomGenerator = a.QuadrantBuildArgs.RandomGenerator;
            var aWorld = this.World;
            this.OriginalWorldPos = this.GenerateOriginalWorldPos(aRandomGenerator);
            this.Radius = this.BuildRadius(aRandomGenerator); 
            this.Color = aRandomGenerator.NextWorldPos();
            this.AccelerateEnums = aRandomGenerator.NextItems<CAccelerateEnum>(AllAccelerateEnums.Fields);
            this.GravityIsEnabled = this.AccelerateEnums.Contains(CAccelerateEnum.Gravity);
            this.GravityRadius = aRandomGenerator.NextDouble(aWorld.AsteroidGravityRadiusMax);
            this.GravityStrength = aRandomGenerator.NextDouble(aWorld.AsteroidGravityStrengthMax);
            this.GravityRepulsive = aRandomGenerator.NextBoolean();
            this.AccelerateIsEnabled = this.AccelerateEnums.Contains(CAccelerateEnum.Accelerate);
            this.AccelerateHasVector = aRandomGenerator.NextBoolean();
            this.AccelerateVector = aRandomGenerator.NextWorldPos();
            this.AccelerateStrength = aRandomGenerator.NextDouble(1.0d);
            this.AccelerateIsRepulsive = aRandomGenerator.NextBoolean();
            this.TargetCubePos = aRandomGenerator.NextCubePos();
        }
        internal virtual double BuildRadius(CRandomGenerator aRandomGenerator)
            => aRandomGenerator.NextDouble(this.AsteroidRadiusMax);

        internal abstract CVector3Dbl GenerateOriginalWorldPos(CRandomGenerator aRandomGenerator);
        internal CVector3Dbl GenerateDefaultWorldPos(CRandomGenerator aRandomGenerator)
            => this.GetWorldPos(this.TileCubePos.Value).Add(aRandomGenerator.NextDouble(this.World.EdgeLenAsPos));
        internal override ISprite NewSprite()
           => this.Facade.NewSprite(this);
        internal override int ChangesCount => (int)CChangeEnum._Count;

        public override CVector3Dbl WorldPos => this.OriginalWorldPos;
        internal CVector3Dbl OriginalWorldPos { get; private set; }
        internal bool WarpIsActive { get; set; }
        public double Radius { get; private set; }
        public CVector3Dbl Color { get; private set; }
        internal CAccelerateEnum[] AccelerateEnums { get; private set; }
        public bool GravityIsEnabled { get; private set; }
        public double GravityRadius { get; private set; }
        public double GravityStrength { get; private set; }
        public bool GravityRepulsive { get; private set; }
        public bool AccelerateIsEnabled { get; private set; }
        public bool AccelerateHasVector { get; private set; }
        public CVector3Dbl AccelerateVector { get; private set; }
        public double AccelerateStrength { get; private set; }
        public bool AccelerateIsRepulsive { get; private set; }
        internal CCubePos TargetCubePos { get; private set; }
        public CAsteroidModel AsteroidModel => this.World.Models.AsteroidModel;
        public virtual bool OrbitIsDefined => false;
        public virtual COrbit Orbit => this.Throw<COrbit>(new InvalidOperationException());
        internal enum CChangeEnum
        {
            _Count
        }

        public abstract string CategoryName {get;}
        public string VmCategoryName => this.CategoryName;

        internal enum CAccelerateEnum
        {
            Gravity,
            Accelerate
        }

        internal override bool ModelIsDefined => true;

        public double AvatarDistanceToSurface { get; private set; }
        public bool IsBelowSurface { get; private set; }
        internal bool IsBelowSurfaceInWarpArea { get; private set; }
        public bool IsNearestAsteroidToAvatar { get; set; }

        internal override CModel NewModel() => this.World.Models.AsteroidModel;

        internal override void Update(CVector3Dbl aAvatarPos)
        {
            base.Update(aAvatarPos);

            this.AvatarDistanceToSurface = this.DistanceToAvatar - this.Radius;
            this.IsBelowSurface =  this.DistanceToAvatar < this.Radius;
            this.IsBelowSurfaceInWarpArea = this.DistanceToAvatar < this.Radius / 2;
        }
        internal override void Update(CFrameInfo aFrameInfo)
        {
            base.Update(aFrameInfo);

            this.IsNearestAsteroidToAvatar = aFrameInfo.NearestAsteroidIsDefined
                                        && aFrameInfo.NearestAsteroid.RefEquals<CBumperSprite>(this);
            this.WormholeCubes.Swap(this);
        }

        #region Cube
        private CWormholeCubes WormholeCubesM;
        private CWormholeCubes WormholeCubes => CLazyLoad.Get(ref this.WormholeCubesM, () => this.ServiceContainer.GetService<CWormholeCubes>());
        #endregion
    }


}
