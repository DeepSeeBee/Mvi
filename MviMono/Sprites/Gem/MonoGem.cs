using CharlyBeck.Mvi.Sprites.Gem;
using CharlyBeck.Mvi.XnaExtensions;
using CharlyBeck.Utils3.ServiceLocator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MviMono.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Mono.Sprites.Gem
{
    internal sealed class CMonoGemModel : CMonoModel
    {
        internal CMonoGemModel(CServiceLocatorNode aParent) : base(aParent)
        {
            var aModel = this.Models.GemModel;

            this.OctaederTriangleListVertexBuffer = aModel.Octaeder.ColoredTriangleList.ToVertexPositionColor().ToVertexBuffer(this.GraphicsDevice);
            this.OctaederLineListVertexBuffer = aModel.Octaeder.LineList.ToVector3s().ToVertexPositionColor(Color.White).ToVertexBuffer(this.GraphicsDevice);
        }

        private readonly VertexBuffer OctaederTriangleListVertexBuffer;
        private readonly VertexBuffer OctaederLineListVertexBuffer;

        internal void Draw()
        {
            this.OctaederTriangleListVertexBuffer.DrawTriangleList(this.GraphicsDevice);
            this.OctaederLineListVertexBuffer.DrawLineList(this.GraphicsDevice);
        }
    }

    internal sealed class CMonoGemSprite : CMonoSprite<CGemSprite, CMonoGemModel>
    {
        internal CMonoGemSprite(CServiceLocatorNode aParent):base(aParent)
        {
            this.TransformToSpriteMatrix = true;
                
            this.MonoModel = this.MonoModels.MonoGemModel;
        }

        public override void Draw()
        {
            base.Draw();

            foreach(var aEffectPass in this.BasicEffect.CurrentTechnique.Passes)
            {
                aEffectPass.Apply();
                this.MonoModel.Draw();
            }
        }
    }
}
