using CharlyBeck.Mvi.Mono.GameCore;
using CharlyBeck.Mvi.World;
using CharlyBeck.Mvi.Cube;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.ServiceLocator;
using Microsoft.Xna.Framework.Graphics;
using Mvi.Models;
using MviMono.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharlyBeck.Mvi.Sprites.Bumper;
using Microsoft.Xna.Framework;
using System.Collections;
using CharlyBeck.Mvi.XnaExtensions;
using CharlyBeck.Mvi.Models;
using CharlyBeck.Mvi.Feature;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Mvi.Mono.Sprites;

namespace MviMono.Sprites.Bumper
{
    internal sealed class CCircleVertexBuffers : CShapeScales<VertexBuffer>
    {
        internal CCircleVertexBuffers(GraphicsDevice aGraphicsDevice, CCircles aCircles, Color aColor) : base(aCircles.MinScale, aCircles.MaxScale)
        {
            this.GraphicsDevice = aGraphicsDevice;
            this.Circles = aCircles;
            this.Color = aColor;

            this.Init();
        }
        private readonly CCircles Circles;
        private readonly GraphicsDevice GraphicsDevice;
        private readonly Color Color;

        protected override VertexBuffer NewShape(int aScale)
            => this.Circles.GetShapeByScale(aScale).LineList.ToVector3s().ToVertexPositionColor(this.Color).ToVertexBuffer(this.GraphicsDevice);
        internal VertexBuffer GetShapeByRadius(double r)=>this.GetShapeByScale(this.Circles.GetScaleByRadius(r));
    }


    internal sealed class CMonoBumperModel : CMonoModel
    {
        internal CMonoBumperModel(CServiceLocatorNode aParent, CBumperModel aMviBumperModel) : base(aParent)
        {
            this.OctaederLineListVertexBuffer = aMviBumperModel.Octaeder.ColoredLineList.ToVertexPositionColor().ToVertexBuffer(this.Game.GraphicsDevice);
            this.SphereDots = aMviBumperModel.Sphere.Dots;
            var aGraphicsDevice = this.Game.GraphicsDevice;
            this.SphereLineListVertexBuffer = aMviBumperModel.Sphere.HorizontalOutterPolygonLineList.ToVector3s().ToVertexPositionColor(Color.White).ToVertexBuffer(aGraphicsDevice);
            this.SphereLineListVertexBuffers = (from aSphere in aMviBumperModel.Spheres 
                                                select aSphere.HorizontalOutterPolygonLineList.ToVector3s().ToVertexPositionColor(Color.White)
                                                .ToVertexBuffer(aGraphicsDevice)).ToArray();
            this.SphereTriangleListVertexBuffers = (from aSphere in aMviBumperModel.Spheres 
                                                    select aSphere.TriangleStrips.ToVector3s().ToVertexPositionColor(Color.White)
                                                    .ToVertexBuffer(aGraphicsDevice)).ToArray();
            this.CircleVertexBuffers = new CCircleVertexBuffers(aGraphicsDevice, aMviBumperModel.Circles, CColors.OrbitGray);
            
            
            this.MviBumperModel = aMviBumperModel;

            this.BlendState = new BlendState();
            this.BlendState.ColorSourceBlend = Blend.BlendFactor;
            this.BlendState.AlphaBlendFunction = BlendFunction.Subtract; // test
            this.BlendState.AlphaSourceBlend = Blend.BlendFactor;

            var aRasterizerState = new RasterizerState();
            aRasterizerState.CullMode = CullMode.CullClockwiseFace;
            this.RasterizerState = aRasterizerState;

        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();

        private readonly CBumperModel MviBumperModel;
        private readonly VertexBuffer OctaederLineListVertexBuffer;
        private readonly VertexBuffer SphereLineListVertexBuffer;
        private readonly VertexBuffer[] SphereLineListVertexBuffers;
        private readonly VertexBuffer[] SphereTriangleListVertexBuffers;
        private readonly CCircleVertexBuffers CircleVertexBuffers;

        private VertexBuffer GetSphereLineListVertexBuffer(double aDistanceToSurface)
            => this.SphereLineListVertexBuffers[this.MviBumperModel.GetSphereIdx(aDistanceToSurface)];
        private VertexBuffer GetSphereTriangleStripVertexBuffer(double aDistanceToSurface)
            => this.SphereTriangleListVertexBuffers[this.MviBumperModel.GetSphereIdx(aDistanceToSurface)];

        private CVector3Dbl[] SphereDots;
    
        private void DrawOctaedres(CBumperSprite aBumperSprite)
        {
            var aEffect = aBumperSprite.BasicEffect;
            var aBasicMatrix = aEffect.World;
            foreach (var aSphereDot in this.SphereDots)
            {
                var aDotMatrix = aBasicMatrix * Matrix.CreateTranslation(aSphereDot.ToVector3());
                aEffect.World = aDotMatrix;
                foreach (var aEffectPass in aEffect.CurrentTechnique.Passes)
                {
                    aEffectPass.Apply();
                    this.OctaederLineListVertexBuffer.DrawLineList(this.Game.GraphicsDevice);
                }
            }
        }

        private readonly BlendState BlendState;
        private readonly RasterizerState RasterizerState;


        private void DrawSphere(CBumperSprite aBumperSprite)
        {
            var aBumperSpriteData = aBumperSprite.BumperSpriteData;
            var aAlpha = aBumperSpriteData.GetAlpha(this.Game.Avatar.WorldPos); 
            var aInvertedAlpha = (float)(1d - aAlpha);
            var aBasicEffect = aBumperSprite.BasicEffect;
            var aOldAlpha = aBasicEffect.Alpha;
            var aOldWorldMatrix = this.Game.WorldMatrix;
            var aScaleMatrix = Matrix.CreateScale((float)aBumperSprite.BumperSpriteData.Radius);
            var aTranslateMatrix = Matrix.CreateTranslation(aBumperSpriteData.WorldPos.ToVector3());
            var aWorldMatrix = aOldWorldMatrix * aScaleMatrix * aTranslateMatrix;
            aBasicEffect.Alpha = (float)aAlpha;
            aBasicEffect.World = aWorldMatrix;
            var aAvatarIsInTile = aBumperSpriteData.AvatarIsInTile;
            var aAvatarDistanceToSurface = aBumperSpriteData.AvatarDistanceToSurface;
            var aVertexBufferIndex = this.MviBumperModel.GetSphereIdx(aAvatarDistanceToSurface);
            var aLineListVertexBuffer = this.SphereLineListVertexBuffers[aVertexBufferIndex]; // this.GetSphereLineListVertexBuffer(aAvatarDistanceToSurface);
            var aTriangleStripVertexBuffer = this.SphereTriangleListVertexBuffers[aVertexBufferIndex];// this.GetSphereTriangleStripVertexBuffer(aAvatarDistanceToSurface);

            var aIsNearest = aBumperSpriteData.IsNearest;
            var aIsBelowSurface = aBumperSpriteData.IsBelowSurface;

            var aBumperColor = aBumperSpriteData.Color.ToColor().SetAlpha(aInvertedAlpha);
            var aBumperColorWhite = Color.White.SetAlpha((float)aAlpha);

            var aGraphicsDevice = this.Game.GraphicsDevice;
            var aBlendState = this.BlendState;

            var aOldBlendState = aGraphicsDevice.BlendState;

            aGraphicsDevice.BlendState = aBlendState;
            aGraphicsDevice.BlendFactor = aBumperColor;

            //aGraphicsDevice.BlendState = Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend;

            //if(aIsNearest)
            { // TriangleStrip
                var aOldRasterizerState = aGraphicsDevice.RasterizerState;
                aGraphicsDevice.RasterizerState = this.RasterizerState;
                foreach (var aPass in aBasicEffect.CurrentTechnique.Passes)
                {
                    aPass.Apply();
                    aTriangleStripVertexBuffer.DrawTriangleList(aGraphicsDevice);
                }
                aGraphicsDevice.RasterizerState = aOldRasterizerState;
            }

            if(aIsNearest
            || aIsBelowSurface)
            { // LineList
               // aBasicEffect.Alpha = 0.0f;
                aGraphicsDevice.BlendFactor = aBumperColorWhite.SetAlpha(aInvertedAlpha);
                foreach (var aPass in aBasicEffect.CurrentTechnique.Passes)
                {
                    aPass.Apply();
                    aLineListVertexBuffer.DrawLineList(aGraphicsDevice);
                    //this.SphereLineListVertexBuffer.DrawLineList(this.Game.GraphicsDevice);
                }
            }
            aBasicEffect.Alpha = aOldAlpha;
            aBasicEffect.World = aOldWorldMatrix;
            aGraphicsDevice.BlendState = aOldBlendState;
        }

        private void DrawOrbit(CBumperSprite aBumperSprite)
        {
            var aBumperSpriteData = aBumperSprite.BumperSpriteData;
            if (aBumperSpriteData.OrbitIsDefined
            && this.DrawOrbitsFeature.Enabled)
            {
                var aGraphicsDevice = this.Game.GraphicsDevice;
                var aOrbit = aBumperSpriteData.Orbit;

                var aOrbitRadians = aOrbit.Item1;
                var aOrbitCenter = aOrbit.Item2;
                var aOrbitRadius = aOrbit.Item3;

                var aScaleMatrix = Matrix.CreateScale((float)aOrbitRadius);

                var aRotateMatrix = aOrbitRadians.ToVector3().ToRotateMatrixXyz();
                var aTranslateMatrix = aOrbitCenter.ToVector3().ToTranslateMatrix(); 
                
                var aBasicEffect = this.Game.BasicEffect;
                var aOldWorldMatrix = aBasicEffect.World;
                
                var aWorldMatrix = aOldWorldMatrix * aScaleMatrix *aRotateMatrix * aTranslateMatrix ;
                
                aBasicEffect.World = aWorldMatrix;
                
                var aVertexBuffer = this.CircleVertexBuffers.GetShapeByRadius(aOrbitRadius * 1000);




                var aOldAlpha = aBasicEffect.Alpha;
                var aAlpha = aBumperSpriteData.GetAlpha(this.Game.Avatar.WorldPos); // TODO_OPT
                aBasicEffect.Alpha = (float)aAlpha;



                foreach (var aPass in aBasicEffect.CurrentTechnique.Passes)
                {
                    aPass.Apply();
                    aVertexBuffer.DrawLineList(aGraphicsDevice);
                }

                aBasicEffect.World = aOldWorldMatrix;
                aBasicEffect.Alpha = aOldAlpha;
            }
        }

        internal void Draw(CBumperSprite aBumperSprite)
        {
            //this.DrawOctaedres(aBumperSprite);
            this.DrawSphere(aBumperSprite);
            this.DrawOrbit(aBumperSprite);    


            

        }

        #region Features
        [CFeatureDeclaration]
        private static readonly CFeatureDeclaration DrawOrbitsFeatureDeclaration = new CFeatureDeclaration(new Guid("54570387-102d-48a8-aac9-a68044fefc54"), "SolarSystem.Orbits.Draw", true);
        private CFeature DrawOrbitsFeatureM;
        private CFeature DrawOrbitsFeature => CLazyLoad.Get(ref this.DrawOrbitsFeatureM, () => CFeature.Get(this, DrawOrbitsFeatureDeclaration));
        #endregion
    }


    internal sealed class CBumperSprite
    :
       CSprite<CBumperSpriteData, CMonoBumperModel>
    {
        internal CBumperSprite(CServiceLocatorNode aParent) : base(aParent)
        {
        }




        internal CBumperSpriteData BumperSpriteData
        {
            get => (CBumperSpriteData)this.SpriteData;
            set => this.SpriteData = value;
        }


        internal override void OnDraw()
        {
            base.OnDraw();

            this.GenericMonoModel.Draw(this);
        }

        internal override bool GenericMonoModelIsDefined => true;
        internal override CMonoBumperModel NewGenericMonoModel(CServiceLocatorNode aParent)
            => new CMonoBumperModel(aParent, this.BumperSpriteData.BumperModel);
    }
}
