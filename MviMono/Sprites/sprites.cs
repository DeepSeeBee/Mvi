using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Mono.GameCore;
using CharlyBeck.Mvi.Mono.Sprites.Cube;
using CharlyBeck.Mvi.Mono.Sprites.Shot;
using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Mvi.Sprites.Asteroid;
using CharlyBeck.Mvi.Sprites.Bumper;
using CharlyBeck.Mvi.World;
using CharlyBeck.Mvi.XnaExtensions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MviMono.Models;
using MviMono.Sprites.Asteroid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils3.Asap;

namespace CharlyBeck.Mvi.Mono.Sprites
{
    internal abstract class CMonoSprite
    :
        CPlatformSprite
    {
        #region ctor
        internal CMonoSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Game = this.ServiceContainer.GetService<CGame>();
            this.MonoModels = this.ServiceContainer.GetService<CMonoModels>();
            this.BasicEffect = new BasicEffect(this.Game.GraphicsDevice);
        }

        internal readonly CGame Game;
        internal CWorld World => this.Game.World;
        internal readonly CMonoModels MonoModels;

        public abstract CSprite SpriteBase { get; }
        #endregion
        internal GraphicsDevice GraphicsDevice => this.Game.GraphicsDevice;
        #region BasicEffect
        internal readonly BasicEffect BasicEffect;
        private void UpdateBasicEffect()
        {
            var aGame = this.Game;
            this.BasicEffect.Alpha = 1f;
            this.BasicEffect.VertexColorEnabled = true;
            this.BasicEffect.World = this.WorldMatrix;
            this.BasicEffect.View = aGame.ViewMatrix;
            this.BasicEffect.Projection = aGame.ProjectionMatrix;
        }
        #endregion
        #region WorldMatrix

        internal bool TranslateToTilePosition;
        internal bool TransformToSpriteMatrix;

        private Matrix WorldMatrix = Matrix.Identity;

        public override void Reposition()
        {
            base.Reposition();
            var aWorldMatrix = this.Game.WorldMatrix;
            if(this.TranslateToTilePosition)
            {
                aWorldMatrix = aWorldMatrix * Matrix.CreateTranslation(this.SpriteBase.TileWorldPos.Value.ToVector3());
            }
            if(this.TransformToSpriteMatrix)
            {
                aWorldMatrix = this.SpriteBase.WorldMatrix;
            }
            this.WorldMatrix = aWorldMatrix;
        }
        #endregion


        #region Draw
        public override void Draw()
        {
            this.UpdateBasicEffect();
        }
        #endregion

    }

    internal abstract class CMonoSprite<TSprite, TMonoModel>
    :
        CMonoSprite
        where TMonoModel : CMonoModel
        where TSprite : CSprite
    {
        internal CMonoSprite(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        #region Sprite
        internal TSprite Sprite;
        public override CSprite SpriteBase => this.Sprite;
        #endregion
        #region GenericMonoModel
        public TMonoModel MonoModel { get; protected set; }
        #endregion
    }


}
