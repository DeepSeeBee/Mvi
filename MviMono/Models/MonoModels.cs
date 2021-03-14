using CharlyBeck.Mvi.Mono.GameCore;
using CharlyBeck.Mvi.Mono.Sprites.Avatar;
using CharlyBeck.Mvi.Mono.Sprites.Crosshair;
using CharlyBeck.Mvi.Mono.Sprites.Cube;
using CharlyBeck.Mvi.Mono.Sprites.Explosion;
using CharlyBeck.Mvi.Mono.Sprites.Gem;
using CharlyBeck.Mvi.Mono.Sprites.GemSlot;
using CharlyBeck.Mvi.Mono.Sprites.Shot;
using CharlyBeck.Mvi.Sprites.GemSlot;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mvi.Models;
using MviMono.Sprites.Asteroid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharlyBeck.Mvi.Extensions;

namespace MviMono.Models
{
 
    internal sealed class CColors
    {
        internal static readonly Color QuadrantGridGray = new Color(0.1f, 0.1f, 0.1f, 1f);
        internal static readonly Color OrbitGray = new Color(0.4f, 0.4f, 0.4f, 1f);
        internal static readonly Color Shot = new Color(1f, 0f, 0f, 1f);
        internal static readonly Color Crosshair = new Color(1f, 1f, 1f, 1f);
    }
    internal sealed class CMonoModels : CServiceLocatorNode
    {
        #region ctor
        internal CMonoModels(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Game = this.ServiceContainer.GetService<CGame>();
            this.MonoAvatarModel = new CMonoAvatarModel(this);
            this.MonoCubeModel = new CMonoCubeModel(this);
            this.MonoBumperModel = new CMonoBumperModel(this);
            this.MonoShotModel = new CMonoShotModel(this);
            this.MonoCrosshairModel = new CMonoCrosshairModel(this);
            this.MonoExplosionModel = new CMonoExplosionModel(this);
            this.MonoGemModel = new CMonoGemModel(this);
            this.MonoGemSlotControlsModel = new CMonoGemSlotControlsModel(this);

            this.TextSpriteBatch = new SpriteBatch(this.Game.GraphicsDevice);
            this.TextBlendState = new BlendState();
            this.TextBlendState.ColorBlendFunction = BlendFunction.Add;
            this.TextBlendState.AlphaSourceBlend = Blend.BlendFactor;
            this.TextEffect = new BasicEffect(this.Game.GraphicsDevice);
            //this.TextEffect.View = Matrix.CreateScale(1.0f);

        }
        #endregion

        internal readonly CGame Game;
        internal SpriteFont SpriteFontEthnocentric72;
        internal Matrix SpriteFontEthnocentric72Matrix;
        private readonly SpriteBatch TextSpriteBatch;
        private readonly BlendState TextBlendState;
        private readonly BasicEffect TextEffect;

        internal void DrawString(string aText, CTextRect aRect, Color aColor)
        {
            if (aText.Length == 0)
                return;

            var aGraphicsDevice = this.Game.GraphicsDevice;
            var aViewport = aGraphicsDevice.Viewport;
            var aVpDx = aViewport.Width;
            var aVpDy = aViewport.Height;
            var aDy = (float) aRect.Dy;
            var aVpSize = new Vector2(aVpDx, aVpDy);
            var aOldBlendState = aGraphicsDevice.BlendState;
            var aOldBlendFactor = aGraphicsDevice.BlendFactor;
            var aOldDepthStencilState = aGraphicsDevice.DepthStencilState;
            var aOldRasterizerState = aGraphicsDevice.RasterizerState;
            var aTextSize1 = this.SpriteFontEthnocentric72.MeasureString(aText);
            var aTextSize2 = aTextSize1 / aVpSize;
            var aTextSize = aTextSize2;
            var aScaleVec1 = new Vector2((float)aRect.Dx, (float)aRect.Dy) / aTextSize;
            var aScale = (float)Math.Min(aScaleVec1.X, aScaleVec1.Y) /2f;
            var aScaleVec2 = new Vector2(aScale);
            var aPosition1 = new Vector2((float)aRect.X, (float)aRect.Y);
            var aX1 = aPosition1.X.F01_Map(-1f, 1f, 0f, 1f);
            var aY1 = aPosition1.Y.F01_Map(-1f, 1f, 0f, 1f);
            var aX2 = aX1;
            var aY2 = 1f - aY1;
            var aX3 = aX2.F01_Map(0f, 1f, 0f, aVpDx);
            var aY3 = aY2.F01_Map(0f, 1f, 0f, aVpDy);
            var aPosition3 = new Vector2(aX3, aY3);
            var aPosition4 = aPosition3;
            var aPosition5 = aPosition4;/// aScaleVec2;
            var aDyVp = aDy.F01_Map(0f, 2f, 0f, aVpDy);
            var aPosition = new Vector2(aPosition5.X, aPosition5.Y - aTextSize1.Y * aScale - aDyVp / 2f + aTextSize1.Y * aScale / 2f );
            //aPosition = new Vector2(0, aViewport.Height - 10);
          // aScale = aScale / 2f;

            this.TextSpriteBatch.Begin();
            this.TextSpriteBatch.DrawString(this.SpriteFontEthnocentric72, 
                                            aText, 
                                            aPosition, 
                                            aColor, 
                                            0f, 
                                            new Vector2(0f), 
                                            aScale, 
                                            default, 
                                            0f);
            this.TextSpriteBatch.End();

            aGraphicsDevice.BlendState = aOldBlendState;
            aGraphicsDevice.BlendFactor = aOldBlendFactor;
            aGraphicsDevice.DepthStencilState = aOldDepthStencilState;
            aGraphicsDevice.RasterizerState = aOldRasterizerState;
        }

        internal void LoadContent()
        {
            this.SpriteFontEthnocentric72 = this.Game.Content.Load<SpriteFont>(@"SpriteFont\Ethnocentric72");
            this.SpriteFontEthnocentric72Matrix = Matrix.CreateScale(1 / 72f);

            foreach (var aMonoModel in this.MonoModels)
                aMonoModel.LoadContent();
        }

        private IEnumerable<CMonoModel> MonoModels
        {
            get
            {
                yield return this.MonoAvatarModel;
                yield return this.MonoCubeModel;
                yield return this.MonoBumperModel;
                yield return this.MonoShotModel;
                yield return this.MonoCrosshairModel;
                yield return this.MonoExplosionModel;
                yield return this.MonoGemModel;
                yield return this.MonoGemSlotControlsModel;
            }
        }

        internal readonly CMonoAvatarModel MonoAvatarModel;
        internal readonly CMonoCubeModel MonoCubeModel;
        internal readonly CMonoBumperModel MonoBumperModel;
        internal readonly CMonoShotModel MonoShotModel;
        internal readonly CMonoCrosshairModel MonoCrosshairModel;
        internal readonly CMonoExplosionModel MonoExplosionModel;
        internal readonly CMonoGemModel MonoGemModel;
        internal readonly CMonoGemSlotControlsModel MonoGemSlotControlsModel;

    }

    internal abstract class CMonoModel : CServiceLocatorNode
    {
        internal CMonoModel(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Game = this.ServiceContainer.GetService<CGame>();
        }
        internal virtual void LoadContent()
        {
        }
        internal readonly CGame Game;
        internal CWorld World => this.Game.World;
        internal CModels Models => this.World.Models;
        internal GraphicsDevice GraphicsDevice => this.Game.GraphicsDevice;

    }
}
