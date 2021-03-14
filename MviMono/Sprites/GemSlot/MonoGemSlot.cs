using CharlyBeck.Mvi.Sprites.GemSlot;
using CharlyBeck.Mvi.XnaExtensions;
using CharlyBeck.Utils3.Enumerables;
using CharlyBeck.Utils3.ServiceLocator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MviMono.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Mono.Sprites.GemSlot
{
    internal sealed class CMonoGemSlotControlsModel : CMonoModel
    {
        internal CMonoGemSlotControlsModel(CServiceLocatorNode aParent) : base(aParent)
        {
            this.GemSlotTriangleList = this.Models.GemControlsModel.GemSlotModel.TriangleList.ToVector3s().ToVertexPositionColor(Color.White).ToVertexBuffer(this.GraphicsDevice);
            this.GemSlotLineList = this.Models.GemControlsModel.GemSlotModel.LineList.ToVector3s().ToVertexPositionColor(Color.White).ToVertexBuffer(this.GraphicsDevice);
        }
        internal readonly VertexBuffer GemSlotTriangleList;
        internal readonly VertexBuffer GemSlotLineList;
    }

    internal sealed class CMonoGemSlotControlsSprite : CMonoSprite<CGemSlotControlsSprite, CMonoGemSlotControlsModel>
    {
        internal CMonoGemSlotControlsSprite(CServiceLocatorNode aParent) :base(aParent)
        {
            this.MonoModel = this.MonoModels.MonoGemSlotControlsModel;
            this.RasterizerStateNoCull = new RasterizerState();
            this.RasterizerStateNoCull.CullMode = CullMode.None;
            this.BlendState = new BlendState();
            this.BlendState.ColorSourceBlend = Blend.BlendFactor;
        }
        private readonly RasterizerState RasterizerStateNoCull;
        private readonly BlendState BlendState;

        public override void Draw()
        {
            base.Draw();

            var aDrawInfos = this.MonoModel.Models.GemControlsModel.GetDrawInfo(this.Sprite);
            var aViewport = this.GraphicsDevice.Viewport;
            var dx = aViewport.Width;
            var dy = aViewport.Height;
            var s = new Vector3(dx, dy, 1f);
            var sm = Matrix.Identity; // Matrix.CreateScale(s);
            var e = this.BasicEffect;
            var aGraphicsDevice = this.GraphicsDevice;
            var aOldRasterizerState = aGraphicsDevice.RasterizerState;
            var aOldView = e.View;
            var aOldWorld = e.World;
            var aOldProjection = e.Projection;
            var aOldBlendState = aGraphicsDevice.BlendState;
            var aOldBlendFactor = aGraphicsDevice.BlendFactor;
            e.View = Matrix.Identity;
            e.Projection = Matrix.Identity;
            e.World = Matrix.Identity;
            aGraphicsDevice.RasterizerState = this.RasterizerStateNoCull;
            foreach (var aDrawInfo in aDrawInfos)
            {
                e.View = aDrawInfo.Matrix;
                aGraphicsDevice.BlendState = this.BlendState; 
                aGraphicsDevice.BlendFactor = aDrawInfo.Color.ToColor();
        
                foreach (var p in e.CurrentTechnique.Passes)
                {
                    p.Apply();
                    if (aDrawInfo.TriangleListIsDefined)
                    {
                        this.MonoModel.GemSlotTriangleList.DrawTriangleList(this.GraphicsDevice);                    
                    }
                    this.MonoModel.GemSlotLineList.DrawLineList(this.GraphicsDevice);

                }
            }
            e.View = aOldView;
            e.World = aOldWorld;
            e.Projection = aOldProjection;
            aGraphicsDevice.RasterizerState = aOldRasterizerState;
            aGraphicsDevice.BlendState = aOldBlendState;
            aGraphicsDevice.BlendFactor = aOldBlendFactor;
        }
    }
}
