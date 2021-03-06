using CharlyBeck.Mvi.Mono.GameCore;
using CharlyBeck.Mvi.Mono.Sprites.Crosshair;
using CharlyBeck.Mvi.Mono.Sprites.Cube;
using CharlyBeck.Mvi.Mono.Sprites.Shot;
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
            this.MonoCubeModel = new CMonoCubeModel(this);
            this.MonoBumperModel = new CMonoBumperModel(this);
            this.MonoShotModel = new CMonoShotModel(this);
            this.MonoCrosshairModel = new CMonoCrosshairModel(this);
        }
        #endregion

        internal readonly CMonoCubeModel MonoCubeModel;
        internal readonly CMonoBumperModel MonoBumperModel;
        internal readonly CMonoShotModel MonoShotModel;
        internal readonly CMonoCrosshairModel MonoCrosshairModel;
    }

    internal abstract class CMonoModel : CServiceLocatorNode
    {
        internal CMonoModel(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Game = this.ServiceContainer.GetService<CGame>();
        }
        internal readonly CGame Game;
        internal CWorld World => this.Game.World;
        internal CModels Models => this.World.Models;
        internal GraphicsDevice GraphicsDevice => this.Game.GraphicsDevice;

    }
}
