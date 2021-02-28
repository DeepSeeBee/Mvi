using CharlyBeck.Mvi.Mono.GameCore;
using CharlyBeck.Mvi.Sprites.Quadrant;
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

namespace MviMono.Sprites.Quadrant
{
    internal sealed class CQuadrantSprite
    :
       CSprite<CQuadrantSpriteData, CMonoModel>
    {
        internal CQuadrantSprite(CServiceLocatorNode aParent, CMonoFacade aMonoFacade, CQuadrantSpriteData aQuadrantSpriteData) : base(aParent, aMonoFacade, aQuadrantSpriteData)
        {
            this.QuadrantSpriteData = aQuadrantSpriteData;
            var aGraphicDevice = this.GraphicsDevice;

            var aVertexPositionColor = aQuadrantSpriteData.Lines2.ToVector3s().ToVertexPositionColor(new Color(0.1f, 0.1f, 0.1f, 0.1f));
            this.LinesVertexBuffer2 = aVertexPositionColor.ToVertexBuffer(aGraphicDevice);
            
            var aUseCornerTriangles = false;
            if (aUseCornerTriangles)
            {
                var aTriangleStripes = new VertexPositionColor[]
                {
                    new VertexPositionColor(aQuadrantSpriteData.FrontBottomLeft2.ToVector3(), Color.White.SetAlpha(0.2f)),
                    new VertexPositionColor(aQuadrantSpriteData.FrontTopLeft2.ToVector3(), Color.White.SetAlpha(0.2f)),
                    new VertexPositionColor(aQuadrantSpriteData.FrontBottomRight2.ToVector3(), Color.White.SetAlpha(0.2f)),
                };
                this.TriangleStripesVertexBuffer = aTriangleStripes.ToVertexBuffer(aGraphicDevice);
            }
        }

        internal readonly VertexBuffer TriangleStripesVertexBuffer;

        public override T Throw<T>(Exception aException)
           => aException.Throw<T>();

        internal readonly VertexBuffer LinesVertexBuffer2;
        internal readonly CQuadrantSpriteData QuadrantSpriteData;

        internal override void DrawPrimitives()
        {
            base.DrawPrimitives();
            var aGraphicsDevice = this.GraphicsDevice;

            var aDrawLines = this.QuadrantSpriteData.QuadrantFeature.Enabled;
            if (aDrawLines)
            {
                //var aOldBlendFactor = aGraphicsDevice.BlendFactor;
                //aGraphicsDevice.BlendFactor = Color.White.SetAlpha(0.001f);
                aGraphicsDevice.SetVertexBuffer(this.LinesVertexBuffer2);
                aGraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, this.LinesVertexBuffer2.VertexCount / 2);
                //aGraphicsDevice.BlendFactor = aOldBlendFactor;
            }
            if (this.TriangleStripesVertexBuffer is object)
            {
                aGraphicsDevice.SetVertexBuffer(this.TriangleStripesVertexBuffer);
                aGraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, this.TriangleStripesVertexBuffer.VertexCount / 3);
            }
        }



    }

}
