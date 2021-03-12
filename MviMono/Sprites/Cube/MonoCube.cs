using CharlyBeck.Mvi.Mono.GameCore;
using CharlyBeck.Mvi.Mono.Sprites;
using CharlyBeck.Mvi.Sprites.GridLines;
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
            this.CubeModel = this.Models.GridLinesModel;
            this.LineListVertexBuffer = this.CubeModel.LineList.ToVector3s().ToVertexPositionColor(CColors.QuadrantGridGray).ToVertexBuffer(this.GraphicsDevice);
        }

        internal readonly CGridLinesModel CubeModel;
        internal readonly VertexBuffer LineListVertexBuffer;
    }

    internal sealed class CMonoGridLinesSprite
    :
       CMonoSprite<CGridLinesSprite, CMonoCubeModel>
    {
        internal CMonoGridLinesSprite(CServiceLocatorNode aParent) : base(aParent)
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
