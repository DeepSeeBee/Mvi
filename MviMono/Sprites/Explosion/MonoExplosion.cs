using CharlyBeck.Mvi.Sprites.Explosion;
using CharlyBeck.Utils3.ServiceLocator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MviMono.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Mono.Sprites.Explosion
{
    internal sealed class CMonoExplosionModel : CMonoModel
    {
        internal CMonoExplosionModel(CServiceLocatorNode aParent) : base(aParent)
        {
        }

        internal Video Video;

        internal override void LoadContent()
        {
            base.LoadContent();

            this.Video = this.Game.Content.Load<Video>(this.Models.ExplosionModel.ResourceName);


            //Texture2D videoTexture = player.GetTexture();
        }
    }

    internal sealed class CMonoExplosionSprite : CMonoSprite<CExplosionSprite, CMonoExplosionModel>
    {
        internal CMonoExplosionSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.MonoModel = this.MonoModels.MonoExplosionModel;
        }

        protected override void OnBeginUse()
        {
            base.OnBeginUse();

            //https://community.monogame.net/t/cannot-start-video-intermittent-crash/7991/11

            //if(!(this.VideoPlayer is object))
            //{
            //    this.VideoPlayer = new VideoPlayer();
            //}
            //if(!(this.VideoSpriteBatch is object))
            //{
            //    this.VideoSpriteBatch = new SpriteBatch(this.GraphicsDevice);
            //}
            //this.VideoPlayer.Play(this.MonoModels.MonoExplosionModel.Video);
        }

        private VideoPlayer VideoPlayer;
        private SpriteBatch VideoSpriteBatch;

        public override void Draw()
        {
            base.Draw();
            var aPlzFixMe = true;
            if(aPlzFixMe)
            {
                this.Sprite.VideoStopped();
            }
            else
            {
            //https://community.monogame.net/t/cannot-start-video-intermittent-crash/7991/11
            //foreach(var aPass in this.BasicEffect.CurrentTechnique.Passes)
            //{
            //    aPass.Apply();

            //    var aVideoTexture = this.VideoPlayer.GetTexture();
            //    this.VideoSpriteBatch.Begin();
            //    this.VideoSpriteBatch.Draw(aVideoTexture, new Vector2(0, 0), Color.White);
            //    this.VideoSpriteBatch.End();
            //  //  aVideoTexture.Dispose();

            //    if(this.VideoPlayer.State == MediaState.Stopped)
            //    {
            //        this.VideoPlayer.Stop();
            //        this.Sprite.VideoStopped();
            //    }
            //}
            }

        }
    }
}
