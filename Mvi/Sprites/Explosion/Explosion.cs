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

namespace CharlyBeck.Mvi.Sprites.Explosion
{
  
    public sealed class CExplosionModel :CModel
    {
        internal CExplosionModel(CServiceLocatorNode aParent):base(aParent)
        {

        }

        public string ResourceName = @"Video\Explosion1\Explosion Large Topview";

    }

    public sealed class CExplosionSprite : CSprite
    {
        internal CExplosionSprite(CServiceLocatorNode aParent):base(aParent)
        {

        }
        protected override void OnEndUse()
        {
            base.OnEndUse();
            this.ExplosionWorldPos = default;
        }
        internal CVector3Dbl? ExplosionWorldPos;
        public override CVector3Dbl WorldPos => this.ExplosionWorldPos.Value;
        internal double Scale => this.Radius.Value;

        internal void Place()
        {
            this.WorldMatrix = Matrix.CreateScale((float)this.Scale) * Matrix.CreateTranslation(this.ExplosionWorldPos.Value.ToVector3());
            this.Reposition();
        }

        internal override CPlatformSpriteEnum PlattformSpriteEnum => CPlatformSpriteEnum.Explosion;

        public void VideoStopped()
        {
            this.DeallocateIsQueued = true;
        }
    }


    internal sealed class CExplosionSprites :  CSpriteManager<CExplosionSprite>
    {


        internal CExplosionSprites(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        protected override CExplosionSprite NewSprite()
            => new CExplosionSprite(this);


        public void AddExplosion(CVector3Dbl aPos, double aRadius)
        {
            var aExplosionSprite = this.AllocateSprite();
            aExplosionSprite.ExplosionWorldPos = aPos;
            aExplosionSprite.Radius = aRadius;
            this.AddSprite(aExplosionSprite);
        }
    }





}
