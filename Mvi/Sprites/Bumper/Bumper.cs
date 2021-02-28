using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.World;
using Mvi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharlyBeck.Mvi.Models;
using CharlyBeck.Mvi.Extensions;
using CharlyBeck.Utils3.ServiceLocator;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;

namespace CharlyBeck.Mvi.Sprites.Bumper
{
    using CColoredVertex = Tuple<CVector3Dbl, CVector3Dbl>;
  

    public sealed class CBumperModel : CModel
    {
        internal CBumperModel(CServiceLocatorNode aParent):base(aParent)
        {
            this.Sphere = new CSphere(4, 1.0f, true);
            var aLen = this.World.SphereScaleCount; // 33;
            this.Spheres = (from aIdx in Enumerable.Range(0, aLen) select new CSphere(aIdx + 1, 1.0d, true)).ToArray();
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        public readonly COctaeder Octaeder = new COctaeder(0.01);
        public readonly CSphere Sphere;
        public readonly CSphere[] Spheres;
        public int GetSphereIdx(double aDistanceToSurface)
        {
            var aBase = 4;
            var aStartScaleAt= 0.1; // TODO -  in cworld ablegen 
            var aMaxScaleAt = 0.001;
            if (aDistanceToSurface > aStartScaleAt)
                return aBase;
            else if(aDistanceToSurface < 0d)
            {
                return this.Spheres.Length - 1;
            }
            else
            {
                var d = Math.Max(aMaxScaleAt, aDistanceToSurface);
                var r = aStartScaleAt - aMaxScaleAt;
                var f = 1 - (d - aMaxScaleAt) / r;
                var i = (int)(((double)this.Spheres.Length-1) * f);
                return Math.Max(aBase, i);
            }
        }
    }

    public sealed class CBumperSpriteData : CSpriteData
    {
        private static readonly CEnums<CAccelerateEnum> AllAccelerateEnums = new CEnums<CAccelerateEnum>();

        internal CBumperSpriteData(CTileBuilder aTileBuilder, CTileDescriptor aTileDescriptor) : base(aTileBuilder, aTileDescriptor)
        {
            var aWorldGenerator = aTileBuilder.WorldGenerator;
            var aWorld = aTileBuilder.World;
            var aTile = aTileBuilder.Tile;
            var aTileAbsoluteCubeCoordinates = aTile.AbsoluteCubeCoordinates;
            this.WorldPosM = aWorld.GetWorldPos(aTileAbsoluteCubeCoordinates).Add(aWorldGenerator.NextDouble(aWorld.EdgeLenAsPos));
            //this.Coordinates2 = aWorld.GetWorldPos(aTileAbsoluteCubeCoordinates)
            var aRadiusMax = aWorldGenerator.NextItem(this.World.BumperRadiusMax);
            this.Radius = aWorldGenerator.NextDouble(aRadiusMax);
            this.Color = aWorldGenerator.NextWorldPos();
            this.AccelerateEnums = aWorldGenerator.NextItems<CAccelerateEnum>(AllAccelerateEnums.Fields);
            this.GravityIsEnabled = this.AccelerateEnums.Contains(CAccelerateEnum.Gravity);
            this.GravityRadius = aWorldGenerator.NextDouble(aWorld.BumperGravityRadiusMax);
            this.GravityStrength = aWorldGenerator.NextDouble(aWorld.BumperGravityStrengthMax);
            this.GravityRepulsive = aWorldGenerator.NextBoolean();
            this.AccelerateIsEnabled = this.AccelerateEnums.Contains(CAccelerateEnum.Accelerate);
            this.AccelerateHasVector = aWorldGenerator.NextBoolean();
            this.AccelerateVector = aWorldGenerator.NextWorldPos();
            this.AccelerateStrength = aWorldGenerator.NextDouble(1.0d);
            this.AccelerateIsRepulsive = aWorldGenerator.NextBoolean();

            this.Init();
        }


        internal override ISprite NewSprite()
           => this.Facade.NewSprite(this);
        internal override int ChangesCount => (int)CChangeEnum._Count;

        public override CVector3Dbl WorldPos => this.WorldPosM;
        public readonly CVector3Dbl WorldPosM;
        public readonly double Radius;
        public readonly CVector3Dbl Color;
        internal readonly CAccelerateEnum[] AccelerateEnums;
        public readonly bool GravityIsEnabled;
        public readonly double GravityRadius;
        public readonly double GravityStrength;
        public readonly bool GravityRepulsive;
        public readonly bool AccelerateIsEnabled;
        public readonly bool AccelerateHasVector;
        public readonly CVector3Dbl AccelerateVector;
        public readonly double AccelerateStrength;
        public readonly bool AccelerateIsRepulsive;

        public CBumperModel BumperModel => this.World.Models.BumperModel;

        internal enum CChangeEnum
        {
            _Count
        }


        internal enum CAccelerateEnum
        {
            Gravity,
            Accelerate
        }

        internal override bool ModelIsDefined => true;

        public double AvatarDistanceToSurface { get; private set; }
        public bool IsBelowSurface { get; set; }
        public bool IsNearestBumperToAvatar { get; set; }

        internal override CModel NewModel() => this.World.Models.BumperModel;

        internal override void UpdateFromWorldPos(CVector3Dbl aAvatarPos)
        {
            base.UpdateFromWorldPos(aAvatarPos);

            this.AvatarDistanceToSurface = this.DistanceToAvatar - this.Radius;
            this.IsBelowSurface =  this.DistanceToAvatar < this.Radius;
        }
        internal override void UpdateFromFrameInfo(CFrameInfo aFrameInfo)
        {
            base.UpdateFromFrameInfo(aFrameInfo);

            this.IsNearestBumperToAvatar = aFrameInfo.NearestBumperIsDefined
                                        && aFrameInfo.NearestBumper.RefEquals<CBumperSpriteData>(this);
        }
    }

}
