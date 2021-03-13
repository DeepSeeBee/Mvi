using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Sprites.Avatar
{

    enum CAvatarValueEnum
    {
        Lifes,
        Shell,
        Shield,
        AutoPilot,
        LaserEnergy,
        NuclearMisileCount,
        ThermalShield,
        KruskalScanner,
        Fuel,
        DrillEnergy,
        GemDetetorPower,
    }


    public sealed class CAvatarSprite : CSprite
    {

        internal CAvatarSprite(CServiceLocatorNode aParent):base(aParent)
        {
            this.PlattformSpriteEnum = CPlatformSpriteEnum.Avatar;
            this.SetCollisionIsEnabled(CCollisionSourceEnum.Gem, true);
            this.BuildIsDone = true;
            this.Radius = 0.01;
            this.Init();
        }

    }


    internal sealed class CAvatarManager : CSinglePoolSpriteManager<CAvatarSprite>
    {
        internal CAvatarManager(CServiceLocatorNode aParent) :base(aParent)
        {
            this.AddOnAllocate = true;
            this.Init();
        }

        protected override void Init()
        {
            base.Init();

            var aLock = true;
            this.Reserve(1, aLock);
        }

        internal override void InitialAllocate()
        {
            base.InitialAllocate();
            this.AvatarSprite = this.AllocateSpriteNullable();
        }
        protected override CAvatarSprite NewSprite()
            => new CAvatarSprite(this);

        private CAvatarSprite AvatarSprite;

        internal CVector3Dbl AvatarPos { get => this.AvatarSprite.WorldPos.Value; set => this.AvatarSprite.WorldPos = value; }
    }
}
