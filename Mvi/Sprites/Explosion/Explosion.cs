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
            this.PlattformSpriteEnum = CPlatformSpriteEnum.Explosion;

            this.Init();
        }
        protected override void OnEndUse()
        {
            base.OnEndUse();
        }
        internal double Scale => this.Radius.Value;

        internal void Place()
        {
            this.WorldMatrix = Matrix.CreateScale((float)this.Scale) * Matrix.CreateTranslation(this.WorldPos.Value.ToVector3());
            this.Reposition();
        }

        public void VideoStopped()
        {
            this.DeallocateIsQueued = true;
        }
    }


    internal sealed class CExplosionsManager :  CSinglePoolSpriteManager<CExplosionSprite>
    {


        internal CExplosionsManager(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        protected override CExplosionSprite NewSprite()
            => new CExplosionSprite(this);


        public void AddExplosion(CVector3Dbl aPos, double aRadius)
        {
            var aExplosionSprite = this.AllocateSprite();
            aExplosionSprite.WorldPos = aPos;
            aExplosionSprite.Radius = aRadius;
            this.AddSprite(aExplosionSprite);
        }
    }





}
