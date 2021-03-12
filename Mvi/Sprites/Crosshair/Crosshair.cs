using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.World;
using CharlyBeck.Mvi.XnaExtensions;
using CharlyBeck.Utils3.ServiceLocator;
using Microsoft.Xna.Framework;
using Mvi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Sprites.Crosshair
{
    public sealed class CCrosshairModel : CModel
    {
        internal CCrosshairModel(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Top = new CVector3Dbl(0, -0.5, 0);
            this.Bottom = new CVector3Dbl(0, 0.5, 0);
            this.Left = new CVector3Dbl(-0.5d, 0, 0);
            this.Right = new CVector3Dbl(+0.5, 0, 0);
            this.LineList = new CVector3Dbl[] { this.Top, this.Bottom, this.Left, this.Right };
        }

        public readonly CVector3Dbl Top;
        public readonly CVector3Dbl Bottom;
        public readonly CVector3Dbl Left;
        public readonly CVector3Dbl Right;

        public readonly CVector3Dbl[] LineList;
    }

    public sealed class CCrosshairSprite : CSprite
    {
        internal CCrosshairSprite(CServiceLocatorNode aParent): base(aParent)
        {
            this.PlattformSpriteEnum = CPlatformSpriteEnum.Crosshair;

            this.Init();
        }

        internal override void UpdateAvatarPos()
        {
            base.UpdateAvatarPos();
            this.WorldPos = new CVector3Dbl(this.AvatarPos.x, this.AvatarPos.y, this.AvatarPos.z + 0.1);
            this.WorldMatrix = Matrix.CreateTranslation(this.WorldPos.Value.ToVector3());
            this.Reposition();
        }
    }
    internal sealed class CCrosshairManager : CSinglePoolSpriteManager<CCrosshairSprite>
    {
        internal CCrosshairManager(CServiceLocatorNode aParent) : base(aParent)
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
            this.CrosshairSprite = this.AllocateSpriteNullable();
        }

        protected override CCrosshairSprite NewSprite()
            => new CCrosshairSprite(this);

        private CCrosshairSprite CrosshairSprite;
    }
}
