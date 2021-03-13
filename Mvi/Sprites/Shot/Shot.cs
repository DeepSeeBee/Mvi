using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Models;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.ServiceLocator;
using Mvi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharlyBeck.Utils3.Asap;

namespace CharlyBeck.Mvi.Sprites.Shot
{
    using CharlyBeck.Mvi.Extensions;
    using CharlyBeck.Mvi.XnaExtensions;
    using Microsoft.Xna.Framework;

    public sealed class CShotModel : CModel
    {
        public readonly CSphere Sphere;

        internal CShotModel(CServiceLocatorNode aParent):base(aParent)
        {
            this.Sphere = new CSphere(5, 1, true);
        }
    }


    public sealed class CShotSprite
    :
        CSprite
    {
        internal CShotSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.CollisionSourceEnum = CCollisionSourceEnum.Shot;
            this.PlattformSpriteEnum = CPlatformSpriteEnum.Shot;


            this.Init();
        }
        protected override void OnBeginUse()
        {
            base.OnBeginUse();

            this.TimeToLive = CStaticParameters.Shot_TimeToLive;
        }
        protected override void OnEndUse()
        {
            base.OnEndUse();

            this.MoveVector = default;
            this.Speed = default;
            this.DellocateIsQueued = false;
            this.TimeToLive = default;
        }
        internal CVector3Dbl? MoveVector;
        internal double? Speed;
        internal bool DellocateIsQueued;


        internal override void Update(CFrameInfo aFrameInfo)
        {
            base.Update(aFrameInfo);

            this.WorldPos = this.WorldPos + this.MoveVector.Value.MakeLongerDelta(this.Speed.Value * aFrameInfo.GameTimeElapsed.TotalSeconds);
            //this.WorldMatrix = Matrix.CreateScale((float)this.Scale) * Matrix.CreateTranslation(this.WorldPos.Value.ToVector3());
            this.WorldMatrix = this.NewWorldMatrix(CMatrixModifierBitEnum.Scale | CMatrixModifierBitEnum.Position);
            this.Reposition();
            if (this.DistanceToAvatar > CStaticParameters.Shot_DistanceToAvatarWhenDead)
                this.DellocateIsQueued = true;
        }

        protected override void OnCollide(CSprite aCollideWith, double aDistance)
        {
            base.OnCollide(aCollideWith, aDistance);
            aCollideWith.OnShotHit(this);
            //this.DellocateIsQueued = true;
        }

        internal void Build()
        {
            this.Scale = 0.01d;
            this.Radius = 0.01d;
        }

        internal override bool CanCollideWithTarget(CSprite aSprite)
            => base.CanCollideWithTarget(aSprite) 
            //&& !aSprite.IsHitByShot
            ;

        
        // internal void Collide()
        // {
        //    var aShot = this;
        //    var aShotables = from aTest in this.FrameInfo.Sprites
        //                     where aTest.IsHitable
        //                     where !aTest.IsHit
        //                     select aTest;
        //    var aOwnPos = this.WorldPos;
        //    foreach(var aShotable in aShotables)
        //    {
        //        var aOtherPos = aShotable.WorldPos;
        //        var aOtherRadius = aShotable.Radius;
        //        var aDistance = aOwnPos.GetDistance(aOtherPos);
        //        var aIsHit = aDistance < aOtherRadius;
        //        if(aIsHit)
        //        {

        //        }
        //    }
        //}
        //internal override void Draw()
        //{
        //    base.Draw();
        //}
    }


    internal sealed class CShotManager : CSinglePoolSpriteManager<CShotSprite>
    {
        internal CShotManager(CServiceLocatorNode aParent) : base(aParent)
        {
            this.AddOnAllocate = true;
            this.Init();
        }
        protected override void Init()
        {
            base.Init();

            var aLock = true;
            this.Reserve(CStaticParameters.Shot_Count_Max, aLock);
        }
        protected override CShotSprite NewSprite()
            => new CShotSprite(this);

        internal event Action ShotFired;
        private void OnShotFired()
        {
            if (this.ShotFired is object)
                this.ShotFired();
        }

        private void AddShot(CVector3Dbl aShotWorldPos, CVector3Dbl aMoveVector, double aSpeed)
        {
            var aShot = this.AllocateSpriteNullable();
            if(aShot is object)
            {
                aShot.WorldPos = aShotWorldPos;
                aShot.MoveVector = aMoveVector;
                aShot.Speed = aSpeed;
                aShot.Build();
                this.OnShotFired();
            }
            else
            {
                // TODO-Fehlzündungssound
            }
        }

        private TimeSpan? LastShot;

        internal bool FireRateChoke()
        {
            bool aShot;
            if(!this.LastShot.HasValue)
            {
                aShot = true;
            }
            else if(this.World.GameTimeTotal.Subtract(this.LastShot.Value).CompareTo(CStaticParameters.Shot_FireRate)> 0)
            {
                aShot = true;
            }
            else
            {
                aShot = false;
            }
            if(aShot)
            {
                this.LastShot = this.World.GameTimeTotal;
            }
            return aShot;
        }

        internal void Shoot()
        {
            if(this.FireRateChoke())
            {
                var aAvatarPos = this.World.AvatarWorldPos;
                var aAvatarSpeed = this.World.AvatarSpeed;
                var aAvatarShootDirection = this.World.AvatarShootDirection;
                var aShotPosition = aAvatarPos;
                var aShotSpeed = Math.Max(aAvatarSpeed, CStaticParameters.Shot_MinSpeed);
                var aShotMoveVector = aAvatarShootDirection;
                this.AddShot(aShotPosition, aShotMoveVector, aShotSpeed);
            }
        }
    }
}
