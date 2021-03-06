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
using Utils3.Asap;

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
        }
        protected override void OnEndUse()
        {
            base.OnEndUse();

            this.ShotWorldPos = default;
            this.MoveVector = default;
            this.Speed = default;
            this.DellocateIsQueued = false;
        }

        internal override CPlatformSpriteEnum PlattformSpriteEnum => CPlatformSpriteEnum.Shot;

        internal CVector3Dbl? ShotWorldPos;
        internal CVector3Dbl? MoveVector;
        internal double? Speed;
        internal bool DellocateIsQueued;
        internal double Scale = 0.01d;

        public override CVector3Dbl WorldPos => this.ShotWorldPos.Value;

        internal void Animate(CFrameInfo aFrameInfo)
        {
            this.ShotWorldPos = this.ShotWorldPos + this.MoveVector.Value.MakeLongerDelta(this.Speed.Value * aFrameInfo.GameTimeElapsed.TotalSeconds);
            this.WorldMatrix =Matrix.CreateScale((float)this.Scale) * Matrix.CreateTranslation(this.ShotWorldPos.Value.ToVector3());
            this.Reposition();
            if (this.DistanceToAvatar > CStaticParameters.Shot_DistanceToAvatarWhenDead)
                this.DellocateIsQueued = true;
        }



        internal void Collide()
        {
            var aShot = this;
            var aShotables = from aTest in this.FrameInfo.Sprites
                             where aTest.IsHitable
                             where !aTest.IsHit
                             select aTest;
            var aOwnPos = this.WorldPos;
            foreach(var aShotable in aShotables)
            {
                var aOtherPos = aShotable.WorldPos;
                var aOtherRadius = aShotable.Radius;
                var aDistance = aOwnPos.GetDistance(aOtherPos);
                var aIsHit = aDistance < aOtherRadius;
                if(aIsHit)
                {
                    aShotable.HitGameTimeTotal = this.FrameInfo.GameTimeTotal;
                    this.DellocateIsQueued = true;
                }
            }
        }
        internal override void Draw()
        {
            base.Draw();
        }
    }


    internal sealed class CShotSprites : CServiceLocatorNode
    {
        internal CShotSprites(CServiceLocatorNode aParent) : base(aParent)
        {
            this.World = this.ServiceContainer.GetService<CWorld>();
            this.ShotSpritePool = new CObjectPool<CShotSprite>();
            this.ShotSpritePool.NewFunc = new Func<CShotSprite>(() => new CShotSprite(this));
        }

        internal readonly CWorld World;
        private CObjectPool<CShotSprite> ShotSpritePool;
        private List<CShotSprite> ActiveShots = new List<CShotSprite>();
        internal IEnumerable<CSprite> Sprites => this.ActiveShots;
        private void AddShot(CVector3Dbl aShotWorldPos, CVector3Dbl aMoveVector, double aSpeed)
        {
            var aShot = this.ShotSpritePool.Allocate();
            aShot.ShotWorldPos = aShotWorldPos;
            aShot.MoveVector = aMoveVector;
            aShot.Speed = aSpeed;
            this.ActiveShots.Add(aShot);
        }

        private void RemoveDeadShots()
        {
            var aDeadShots = (from aTest in this.ActiveShots where aTest.DellocateIsQueued select aTest).ToArray();
            foreach (var aDeadShot in aDeadShots)
            {
                aDeadShot.Deallocate();
                this.ActiveShots.Remove(aDeadShot);
            }
        }

        private void AnimateShots(CFrameInfo aFrameInfo)
        {
            foreach (var aShot in this.ActiveShots)
            {
                aShot.Animate(aFrameInfo);
            }
        }

        private void CollideShots()
        {
            foreach (var aShot in this.ActiveShots)
            {
                aShot.Collide();
            }
        }

        internal void Update(CFrameInfo aFrameInfo)
        {
            this.AnimateShots(aFrameInfo);
            this.CollideShots();
            this.RemoveDeadShots();            
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
