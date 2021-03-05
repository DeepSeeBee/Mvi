using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Mono.GameCore;
using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Mvi.Sprites.Bumper;
using CharlyBeck.Mvi.Sprites.Quadrant;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MviMono.Models;
using MviMono.Sprites.Bumper;
using MviMono.Sprites.Quadrant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils3.Asap;

namespace CharlyBeck.Mvi.Mono.Sprites
{
    internal abstract class CSprite
    :
        CReusable
    ,   ISprite
    {
        #region ctor
        internal CSprite(CServiceLocatorNode aParent) : base()
        {//, CMonoFacade aMonoFacade, CSpriteData aSpriteData
            //this.MonoFacade = aMonoFacade;
            this.ServiceLocatorNode = aParent;
            this.Game = this.ServiceLocatorNode.ServiceContainer.GetService<CGame>();
            this.BasicEffect = new BasicEffect(this.Game.GraphicsDevice);
        }

        internal readonly CServiceLocatorNode ServiceLocatorNode;

        public CSpriteData SpriteData { get; protected set; }

        void ISprite.Unload()
            => this.Deallocate();
        protected override void OnEndUse()
        {
            base.OnEndUse();
            // this.Game.AvatarChanged -= this.UpdateBasicEffect;
            this.SpriteData = default;
        }
        #endregion
        internal CGame Game { get; private set; }
        internal GraphicsDevice GraphicsDevice => this.Game.GraphicsDevice;

        #region BasicEffect
        internal readonly BasicEffect BasicEffect;
        internal void UpdateBasicEffect()
        {
            var aGame = this.Game;
            //this.BasicEffect = aGame.BasicEffect;
            this.BasicEffect.Alpha = 1f;
            this.BasicEffect.VertexColorEnabled = true;
            this.BasicEffect.World = this.WorldMatrix;
            this.BasicEffect.View = aGame.ViewMatrix;
            this.BasicEffect.Projection = aGame.ProjectionMatrix;
        }
        #endregion
        #region WorldTranslateIsDefined
        //internal virtual Vector3 WorldTranslate => new Vector3();
        //internal virtual bool WorldTranslateIsDefined => false;
        internal virtual Matrix WorldMatrix
            => //this.WorldTranslateIsDefined
               // ? this.Game.WorldMatrix * Matrix.CreateTranslation(this.WorldTranslate)
               // : 
            this.Game.WorldMatrix
             ;
        #endregion
        #region Draw
        internal virtual void DrawPrimitives()
        {
        }
        internal virtual void OnDraw()
        {
            this.UpdateBasicEffect();
            foreach (var aEffectPass in this.BasicEffect.CurrentTechnique.Passes)
            {
                aEffectPass.Apply();
                this.DrawPrimitives();
            }
        }
        internal virtual void Draw()
        {

            this.OnDraw();
        }
        void ISprite.Draw()
           => this.Draw();
        #endregion

    }

    internal abstract class CSprite<TSpriteData, TMonoModel>
    :
        CSprite
    ,   ISprite<TSpriteData>
        where TMonoModel : CMonoModel
    {
        internal CSprite(CServiceLocatorNode aParent) : base(aParent)
        {
        }

        #region GenericMonoModel
        private TMonoModel GenericMonoModelM;
        internal TMonoModel GenericMonoModel
            => CLazyLoad.Get(ref this.GenericMonoModelM, this.LoadGenericMonoModel);
        internal virtual TMonoModel NewGenericMonoModel(CServiceLocatorNode aParent)
            => throw new NotImplementedException();
        internal virtual bool GenericMonoModelIsDefined => false;
        private TMonoModel LoadGenericMonoModel()
        {
            var aKey = this.GenericMonoModelKey;
            var aMonoModel = this.Game.Models.LoadMonoModel<TMonoModel>(aKey, aParent => this.NewGenericMonoModel(aParent));
            return aMonoModel;
        }
        internal object GenericMonoModelKey => this.SpriteData.Model;
        #endregion
    }
    internal enum CSpriteEnum
    {
        Bumper,
        Quadrant,

        _Count
    }

    internal sealed class CSpritePool : CMultiObjectPool
    {
        internal CSpritePool(CServiceLocatorNode aParent)
        {
            this.ServiceLocatorNode = aParent;

            var c = (int)CSpriteEnum._Count;
            this.AllocateObjectPool(c);
            this.SetNewFunc((int)CSpriteEnum.Bumper, () => new CBumperSprite(this.ServiceLocatorNode));
            this.SetNewFunc((int)CSpriteEnum.Quadrant, () => new CQuadrantSprite(this.ServiceLocatorNode));

        }

        internal readonly CServiceLocatorNode ServiceLocatorNode;

        internal CBumperSprite AllocateBumperSprite(CBumperSpriteData aBumperSpriteData)
        {
            var aBumperSprite = (CBumperSprite)this.Allocate((int)CSpriteEnum.Bumper);
            aBumperSprite.BumperSpriteData = aBumperSpriteData;
            return aBumperSprite;
        }
        internal CQuadrantSprite AllocateQuadrantSprite(CQuadrantSpriteData aQuadrantSpriteData)
        {
            var aQuadrantSprite = (CQuadrantSprite)this.Allocate((int)CSpriteEnum.Quadrant);
            aQuadrantSprite.QuadrantSpriteData = aQuadrantSpriteData;
            return aQuadrantSprite;

        }
    }
}
