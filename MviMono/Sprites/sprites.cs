using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Mono.GameCore;
using CharlyBeck.Mvi.Mono.Sprites.Cube;
using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Mvi.Sprites.Asteroid;
using CharlyBeck.Mvi.Sprites.Bumper;
using CharlyBeck.Mvi.Sprites.Cube;
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
        CReuseable
    ,   ISprite
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

        void ISprite.Deallocate()
            => this.Deallocate();
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
        private Matrix WorldMatrix = Matrix.Identity;

        private void Reposition()
        {
            var aWorldMatrix = this.Game.WorldMatrix;
            if(this.TranslateToTilePosition)
            {
                aWorldMatrix = aWorldMatrix * Matrix.CreateTranslation(this.SpriteBase.TileWorldPos.Value.ToVector3());
            }
            this.WorldMatrix = aWorldMatrix;
        }
        #endregion


        #region Draw
        internal virtual void Draw()
        {
            this.UpdateBasicEffect();
        }
        #endregion
        #region ISprite
        CSprite ISprite.Sprite  => this.SpriteBase; 
        void ISprite.Draw()
           => this.Draw();

        void ISprite.Reposition()
            => this.Reposition();
        #endregion

    }

    internal abstract class CMonoSprite<TSprite, TMonoModel>
    :
        CMonoSprite
    ,   ISprite<TSprite>
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
    internal enum CSpriteEnum
    {
        Asteroid,
        Cube,

        _Count
    }

    internal sealed class CSpritePool : CMultiObjectPool
    {
        internal CSpritePool(CServiceLocatorNode aParent)
        {
            this.ServiceLocatorNode = aParent;

            var c = (int)CSpriteEnum._Count;
            this.AllocateObjectPool(c);
            this.SetNewFunc((int)CSpriteEnum.Asteroid, () => new CMonoBumperSprite(this.ServiceLocatorNode));
            this.SetNewFunc((int)CSpriteEnum.Cube, () => new CMonoCubeSprite(this.ServiceLocatorNode));
        }

        internal readonly CServiceLocatorNode ServiceLocatorNode;

        internal CMonoBumperSprite AllocateAsteroidSprite(CBumperSprite aBumperSprite)
        {
            var aAsteroidSprite = (CMonoBumperSprite)this.Allocate((int)CSpriteEnum.Asteroid);
            aAsteroidSprite.Sprite = aBumperSprite;
            return aAsteroidSprite;
        }
        internal CMonoCubeSprite AllocateQuadrantSprite(Mvi.Sprites.Cube.CCubeSprite aCubeSprite)
        {
            var aMonoCubeSprite = (CMonoCubeSprite)this.Allocate((int)CSpriteEnum.Cube);
            aMonoCubeSprite.Sprite = aCubeSprite;
            return aMonoCubeSprite;

        }
    }
}
