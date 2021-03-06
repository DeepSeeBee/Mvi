using CharlyBeck.Mvi.Sprites.Shot;
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

namespace CharlyBeck.Mvi.Mono.Sprites.Shot
{
    internal sealed class CMonoShotModel : CMonoModel
    {
        internal CMonoShotModel(CServiceLocatorNode aParent) : base(aParent)
        {
            this.TriangleStripsVertexBuffer = this.Models.ShotModel.Sphere.TriangleStrips.ToVector3s().ToVertexPositionColor(CColors.Shot).ToVertexBuffer(this.GraphicsDevice);
        }
        private readonly VertexBuffer TriangleStripsVertexBuffer;
        internal void Draw()
        {
            this.TriangleStripsVertexBuffer.DrawTriangleStrip(this.GraphicsDevice); 
        }
    }

    internal sealed class CMonoShotSprite : CMonoSprite<CShotSprite, CMonoShotModel>
    {
        #region ctor
        internal CMonoShotSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.TransformToSpriteMatrix = true;
            this.MonoModel = this.MonoModels.MonoShotModel;
        }
        #endregion

        public override void Draw()
        {
            base.Draw();
            foreach(var aPass in this.BasicEffect.CurrentTechnique.Passes)
            {
                aPass.Apply();
                this.MonoModel.Draw();
            }
        }
    }
}
