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
using CharlyBeck.Mvi.Sprites.Asteroid;
using Microsoft.Xna.Framework;
using System.Collections;
using CharlyBeck.Mvi.XnaExtensions;
using CharlyBeck.Mvi.Models;
using CharlyBeck.Mvi.Value;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Mvi.Mono.Sprites;
using CharlyBeck.Mvi.Sprites.Bumper;

namespace MviMono.Sprites.Asteroid
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
        internal CMonoBumperModel(CServiceLocatorNode aParent) : base(aParent)
        {
            var aAsteroidModel = this.Models.AsteroidModel;
            this.OctaederLineListVertexBuffer = aAsteroidModel.Octaeder.ColoredLineList.ToVertexPositionColor().ToVertexBuffer(this.Game.GraphicsDevice);
            this.SphereDots = aAsteroidModel.Sphere.Dots;
            var aGraphicsDevice = this.Game.GraphicsDevice;
            this.SphereLineListVertexBuffer = aAsteroidModel.Sphere.HorizontalOutterPolygonLineList.ToVector3s().ToVertexPositionColor(Color.White).ToVertexBuffer(aGraphicsDevice);
            this.SphereLineListVertexBuffers = (from aSphere in aAsteroidModel.Spheres 
                                                select aSphere.HorizontalOutterPolygonLineList.ToVector3s().ToVertexPositionColor(Color.White)
                                                .ToVertexBuffer(aGraphicsDevice)).ToArray();
            this.SphereTriangleListVertexBuffers = (from aSphere in aAsteroidModel.Spheres 
                                                    select aSphere.TriangleStrips.ToVector3s().ToVertexPositionColor(Color.White)
                                                    .ToVertexBuffer(aGraphicsDevice)).ToArray();
            this.CircleVertexBuffers = new CCircleVertexBuffers(aGraphicsDevice, aAsteroidModel.Circles, CColors.OrbitGray);
            
            
            this.MviAsteroidModel = aAsteroidModel;

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

        private readonly CBumperModel MviAsteroidModel;
        private readonly VertexBuffer OctaederLineListVertexBuffer;
        private readonly VertexBuffer SphereLineListVertexBuffer;
        private readonly VertexBuffer[] SphereLineListVertexBuffers;
        private readonly VertexBuffer[] SphereTriangleListVertexBuffers;
        private readonly CCircleVertexBuffers CircleVertexBuffers;

        private VertexBuffer GetSphereLineListVertexBuffer(double aDistanceToSurface)
            => this.SphereLineListVertexBuffers[this.MviAsteroidModel.GetSphereIdx(aDistanceToSurface)];
        private VertexBuffer GetSphereTriangleStripVertexBuffer(double aDistanceToSurface)
            => this.SphereTriangleListVertexBuffers[this.MviAsteroidModel.GetSphereIdx(aDistanceToSurface)];

        private CVector3Dbl[] SphereDots;
    
        private void DrawOctaedres(CMonoBumperSprite aAsteroidSprite)
        {
            var aEffect = aAsteroidSprite.BasicEffect;
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


        private void DrawSphere(CMonoBumperSprite aAsteroidSprite)
        {
            var aBumperSprite = aAsteroidSprite.Sprite;
            var aAlpha = aBumperSprite.GetAlpha(this.Game.Avatar.WorldPos); 
            var aInvertedAlpha = (float)(1d - aAlpha);
            var aBasicEffect = aAsteroidSprite.BasicEffect;
            var aOldAlpha = aBasicEffect.Alpha;
            var aOldWorldMatrix = this.Game.WorldMatrix;
            var aScaleMatrix = Matrix.CreateScale((float)aAsteroidSprite.Sprite.Radius);
            var aTranslateMatrix = Matrix.CreateTranslation(aBumperSprite.WorldPos.Value.ToVector3());
            var aWorldMatrix = aOldWorldMatrix * aScaleMatrix * aTranslateMatrix;
            aBasicEffect.Alpha = (float)aAlpha;
            aBasicEffect.World = aWorldMatrix;
            var aAvatarIsInTile = aBumperSprite.AvatarIsInQuadrant;
            var aAvatarDistanceToSurface = aBumperSprite.AvatarDistanceToSurface;
            var aVertexBufferIndex = this.MviAsteroidModel.GetSphereIdx(aAvatarDistanceToSurface);
            var aLineListVertexBuffer = this.SphereLineListVertexBuffers[aVertexBufferIndex]; // this.GetSphereLineListVertexBuffer(aAvatarDistanceToSurface);
            var aTriangleStripVertexBuffer = this.SphereTriangleListVertexBuffers[aVertexBufferIndex];// this.GetSphereTriangleStripVertexBuffer(aAvatarDistanceToSurface);

            var aIsNearest = aBumperSprite.IsNearest;
            var aIsBelowSurface = aBumperSprite.IsBelowSurface;

            //if (((int)aBumperSprite.Color.x * 1000) == ((int)0.0577762010776327 * 1000))
            //{
            //    System.Diagnostics.Debug.Assert(true);
            //}
            var aAsteroidColor = aBumperSprite.Color.ToColor().SetAlpha(aInvertedAlpha);
            var aAsteroidColorWhite = Color.White.SetAlpha((float)aAlpha);

            var aGraphicsDevice = this.Game.GraphicsDevice;
            var aBlendState = this.BlendState;

            var aOldBlendState = aGraphicsDevice.BlendState;

            aGraphicsDevice.BlendState = aBlendState;
            aGraphicsDevice.BlendFactor = aAsteroidColor;

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
                aGraphicsDevice.BlendFactor = aAsteroidColorWhite.SetAlpha(aInvertedAlpha);
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

        private void DrawOrbit(CMonoBumperSprite aAsteroidSprite)
        {
            var aBumperSprite = aAsteroidSprite.Sprite;
            if (aBumperSprite.OrbitIsDefined
            && this.DrawOrbitsValue.Value)
            {
                var aGraphicsDevice = this.Game.GraphicsDevice;
                var aOrbit = aBumperSprite.Orbit;

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
                var aAlpha = aBumperSprite.GetAlpha(this.Game.Avatar.WorldPos); // TODO_OPT
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

        internal void Draw(CMonoBumperSprite aAsteroidSprite)
        {
            //this.DrawOctaedres(aAsteroidSprite);
            this.DrawSphere(aAsteroidSprite);
            this.DrawOrbit(aAsteroidSprite);    


            

        }

        #region Values
        [CMemberDeclaration]
        private static readonly CBoolValDecl DrawOrbitsValueDeclaration = new CBoolValDecl
            ( CValueEnum.SolarSystem_Orbits, new Guid("54570387-102d-48a8-aac9-a68044fefc54"), true, true);
        private CBoolValue DrawOrbitsValueM;
        private CBoolValue DrawOrbitsValue => CLazyLoad.Get(ref this.DrawOrbitsValueM, () => CValue.GetStaticValue<CBoolValue>(this, DrawOrbitsValueDeclaration));
        #endregion
    }


    internal sealed class CMonoBumperSprite
    :
       CMonoSprite<CBumperSprite, CMonoBumperModel>
    {
        internal CMonoBumperSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.MonoModel = this.MonoModels.MonoBumperModel;
            this.TranslateToTilePosition = true;
        }


        public override void Draw()
        {
            base.Draw();
            foreach (var aEffectPass in this.BasicEffect.CurrentTechnique.Passes)
            {
                aEffectPass.Apply();
                this.MonoModel.Draw(this);
            }
        }
    }
}
