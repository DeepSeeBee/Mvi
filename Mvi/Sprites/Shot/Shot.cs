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
    using CharlyBeck.Mvi.Sprites.Avatar;
    using CharlyBeck.Mvi.XnaExtensions;

    public sealed class CShotModel : CModel
    {
        public readonly CSphere Sphere;

        internal CShotModel(CServiceLocatorNode aParent):base(aParent)
        {
            this.Sphere = new CSphere(5, 1, true);
        }
    }

    public enum CShotEnum
    {
        Canon,
        GuidedMissile,
        NuclearMissile,
        Drill,
        KruscalScanner
    }


    public abstract class CShotSprite
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
        public CVector3Dbl? Color { get; internal set; }

        internal override void Update(CFrameInfo aFrameInfo)
        {
            base.Update(aFrameInfo);

            this.WorldPos = this.WorldPos + this.MoveVector.Value.MakeLongerDelta(this.Speed.Value * aFrameInfo.GameTimeElapsed.TotalSeconds);
            //this.WorldMatrix = Matrix.CreateScale((float)this.Scale) * Matrix.CreateTranslation(this.WorldPos.Value.ToVector3());
            this.WorldMatrix = this.NewWorldMatrix(CMatrixModifierBitEnum.Scale | CMatrixModifierBitEnum.Position);
            this.Reposition();
            if (this.DistanceToAvatar > CStaticParameters.Shot_DieOnDistanceToAvatar)
                this.DellocateIsQueued = true;
        }

        protected override void OnCollide(CSprite aCollideWith, double aDistance)
        {
            base.OnCollide(aCollideWith, aDistance);
            aCollideWith.OnShotHit(this);
            //this.DellocateIsQueued = true;
        }

        internal override bool CanCollideWithTarget(CSprite aSprite)
            => base.CanCollideWithTarget(aSprite) 
            //&& !aSprite.IsHitByShot
            ;
    }

    internal sealed class CCanonShotSprite : CShotSprite
    {
        internal CCanonShotSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Color = CColors.Shot_Canon;
        }
    }
    internal sealed class CGuidedMissileShotSprite : CShotSprite
    {
        internal CGuidedMissileShotSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Color = CColors.Shot_GuidedMissile;
        }
    }
    internal sealed class CNuclearMissileShotSprite : CShotSprite
    {
        internal CNuclearMissileShotSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Color = CColors.Shot_NuclearMissile;
        }
    }
    internal sealed class CDrillShotSprite : CShotSprite
    {
        internal CDrillShotSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Color = CColors.Shot_Drill;
        }
    }   
    internal sealed class CKruscalScannerShotSprite : CShotSprite
    {
        internal CKruscalScannerShotSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Color = CColors.Shot_KruskalScanner;
        }
    }

    internal sealed class CShotManager : CMultiPoolSpriteManager<CShotSprite, CShotEnum>
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
            this.Reserve(CShotEnum.Canon, CStaticParameters.Shot_Canon_Count_Max, aLock);
            this.Reserve(CShotEnum.GuidedMissile, CStaticParameters.Shot_Canon_Count_Max, aLock);
            this.Reserve(CShotEnum.NuclearMissile, CStaticParameters.Shot_Canon_Count_Max, aLock);
            this.Reserve(CShotEnum.Drill, CStaticParameters.Shot_Canon_Count_Max, aLock);
            this.Reserve(CShotEnum.KruscalScanner, CStaticParameters.Shot_Canon_Count_Max, aLock);
        }
        internal override CNewFunc GetNewFunc(CShotEnum aClassEnum)
        {
            switch(aClassEnum)
            {
                case CShotEnum.Canon: return new CNewFunc(() => new CCanonShotSprite(this));
                case CShotEnum.Drill: return new CNewFunc(() => new CDrillShotSprite(this));
                case CShotEnum.GuidedMissile: return new CNewFunc(() => new CGuidedMissileShotSprite(this));
                case CShotEnum.KruscalScanner: return new CNewFunc(() => new CKruscalScannerShotSprite(this));
                case CShotEnum.NuclearMissile: return new CNewFunc(() => new CNuclearMissileShotSprite(this));
                default:
                    throw new ArgumentException();
            }
        }
        internal event Action ShotFired;
        private void OnShotFired()
        {
            if (this.ShotFired is object)
                this.ShotFired();
        }

        private TimeSpan? LastShot;

        private CAvatarSprite AvatarSprite => this.World.AvatarSprite;

        private TimeSpan FireShotRate 
            => CStaticParameters.Shot_FireRateRange.GetInRangeTimespan(1.0d - this.AvatarSprite.AvatarValues.AmmoFireRateValue.Value);
        internal bool FireRateChoke()
        {
            bool aShot;
            if(!this.LastShot.HasValue)
            {
                aShot = true;
            }
            else if(this.World.GameTimeTotal.Subtract(this.LastShot.Value).CompareTo(this.FireShotRate)> 0)
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

        internal void Shoot(CShotEnum aShotEnum)
        {
            if(this.FireRateChoke())
            {
                var aValues = this.AvatarSprite.AvatarValues;
                var aRemainingShotsValueNullable = aValues.GetValueNullable(aShotEnum);
                var aCanShoot = aRemainingShotsValueNullable is object
                              ? aRemainingShotsValueNullable.Value > 0
                              : true
                              ;
                if (aCanShoot)
                {
                    var aShot = this.AllocateSpriteNullable(aShotEnum);
                    if (aShot is object)
                    {
                        var aAvatarPos = this.World.AvatarWorldPos;
                        var aAvatarSpeed = this.World.AvatarSpeed;
                        var aAvatarShootDirection = this.World.AvatarShootDirection;
                        var aShotPosition = aAvatarPos;
                        var aShotSpeed1 = CStaticParameters.Shot_SpeedRange.GetInRangeDouble(this.AvatarSprite.AvatarValues.AmmoSpeedValue.Value);
                        var aShotSpeed = aShotSpeed1 + aAvatarSpeed;
                        var aShotMoveVector = aAvatarShootDirection;
                        var aRadius = CStaticParameters.Shot_RadiusRange.GetInRangeDouble(this.AvatarSprite.AvatarValues.AmmoThicknessValue.Value);
                        var aTimeToLive = CStaticParameters.Shot_TimeToLive.GetInRangeTimespan(this.AvatarSprite.AvatarValues.AmmoEnergyValue.Value);
                        aShot.WorldPos = aShotPosition;
                        aShot.MoveVector = aShotMoveVector;
                        aShot.Speed = aShotSpeed;
                        aShot.Radius = aRadius;
                        aShot.Scale = aRadius;
                        aShot.TimeToLive = aTimeToLive;
                        if(aRemainingShotsValueNullable is object)
                        {
                            aRemainingShotsValueNullable.Value -= 1;
                        }
                        this.OnShotFired();
                    }
                    else
                    {
                        // TODO-Fehlzündungssound
                    }
                }
                else
                {
                    // TODO-OutOfAmmoSound
                }
            }
        }
    }
}
