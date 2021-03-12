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

            this.Init();
        }

    }


    internal sealed class CAvatarManager : CSinglePoolSpriteManager<CAvatarSprite>
    {
        internal CAvatarManager(CServiceLocatorNode aParent) :base(aParent)
        {
            this.AvatarSprite = this.AllocateSprite();
            this.AddSprite(this.AvatarSprite);
        }
        protected override CAvatarSprite NewSprite()
            => new CAvatarSprite(this);

        private readonly CAvatarSprite AvatarSprite;

        internal CVector3Dbl AvatarPos { get => this.AvatarSprite.AvatarPos; set => this.AvatarSprite.AvatarPos = value; }
    }
}
