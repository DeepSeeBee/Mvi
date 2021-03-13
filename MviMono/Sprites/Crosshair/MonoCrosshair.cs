using CharlyBeck.Mvi.Sprites.Crosshair;
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

namespace CharlyBeck.Mvi.Mono.Sprites.Crosshair
{
    internal sealed class CMonoCrosshairModel : CMonoModel
    {
        internal CMonoCrosshairModel(CServiceLocatorNode aParent) : base(aParent)
        {
            this.CrosshairModel = this.Models.CrosshairModel;
            this.LineListVertexBuffer = this.CrosshairModel.LineList.ToVector3s().ToVertexPositionColor(CColors.Crosshair).ToVertexBuffer(this.GraphicsDevice);
        }

        internal readonly CCrosshairModel CrosshairModel;
        internal readonly VertexBuffer LineListVertexBuffer;

        internal void Draw()
            => this.LineListVertexBuffer.DrawLineList(this.GraphicsDevice);
    }

    internal sealed class CMonoCrosshairSprite
    :
       CMonoSprite<CCrosshairSprite, CMonoCrosshairModel>
    {
        internal CMonoCrosshairSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.MonoModel = this.MonoModels.MonoCrosshairModel;
            this.TransformToSpriteMatrix = true;
        }

        public override void Draw()
        {
            //base.Draw();
            this.BasicEffect.World = Matrix.Identity * Matrix.CreateScale(0.1f); // this.Sprite.WorldMatrix;
            this.BasicEffect.View = Matrix.Identity;
            this.BasicEffect.Projection = Matrix.Identity;

            foreach (var aEffectPass in this.BasicEffect.CurrentTechnique.Passes)
            {
                aEffectPass.Apply();
                this.MonoModel.Draw();
            }
        }
    }
}
