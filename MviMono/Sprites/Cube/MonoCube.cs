using CharlyBeck.Mvi.Mono.GameCore;
using CharlyBeck.Mvi.Mono.Sprites;
using CharlyBeck.Mvi.Sprites.Cube;
using CharlyBeck.Mvi.XnaExtensions;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.ServiceLocator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MviMono.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Mono.Sprites.Cube
{
    internal sealed class CMonoCubeModel : CMonoModel
    {
        internal CMonoCubeModel(CServiceLocatorNode aParent) :base(aParent)
        {
            this.CubeModel = this.Models.CubeModel;
            this.LineListVertexBuffer = this.CubeModel.LineList.ToVector3s().ToVertexPositionColor(CColors.QuadrantGridGray).ToVertexBuffer(this.GraphicsDevice);
        }

        internal readonly CCubeModel CubeModel;
        internal readonly VertexBuffer LineListVertexBuffer;
    }

    internal sealed class CMonoCubeSprite
    :
       CMonoSprite<CCubeSprite, CMonoCubeModel>
    {
        internal CMonoCubeSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.MonoModel = this.MonoModels.MonoCubeModel;
            this.TranslateToTilePosition = true;
        }

        public override void Draw()
        {
            base.Draw();
            foreach (var aEffectPass in this.BasicEffect.CurrentTechnique.Passes)
            {
                aEffectPass.Apply();
                this.MonoModel.LineListVertexBuffer.DrawLineList(this.GraphicsDevice);
            }
        }
    }
}
