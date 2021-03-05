using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Feature;
using CharlyBeck.Mvi.Internal;
using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Mvi.Sprites.Quadrant;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Facade
{
    public interface ISprite
    {
       // void Update(BitArray aWhat);
        void Draw();
        void Unload();

        CSpriteData SpriteData { get; }
    }

    public interface ISprite<T> : ISprite
    {

    }

    public delegate void CAddInGameTheradAction(Action aAction);

    public abstract class CFacade : CServiceLocatorNode
    {
        public CFacade(CServiceLocatorNode aParent) : base(aParent)
        {
            this.World = new CWorld(this);
        }
        public CFacade() : this(new CDefaultServiceLocatorNode())
        {
        }
        protected override void Init()
        {
            base.Init();
            this.World.Load();
        }
        public override void Load()
        {
            base.Load();
            this.World.Load();
        }

        public readonly CWorld World;
        internal ICube Cube => this.World.Cube;

        public abstract ISprite<T> NewSprite<T>(T aSpriteData);


        public void Draw()
           => this.World.Draw();

        //public void SetCubeCoordinates(CCubePos aCoordinates)
        //{
        //    this.Cube.MoveToCubeCoordinatesOnDemand(aCoordinates);
        //}

        public CVector3Dbl WorldPos { set => this.Cube.MoveTo(this.World.GetCubePos(value), true); }

        #region Features
        private CFeatures FeaturesM;
        public CFeatures Features => CLazyLoad.Get(ref this.FeaturesM, this.NewFeatureRegistry);
        public object VmFeatures => this.Features;
        private CFeatures NewFeatureRegistry()
        {
            var aFeatureRegistry = new CFeatures(this);
            this.BuildFeatureRegistry(aFeatureRegistry);
            return aFeatureRegistry;
        }
        protected virtual void BuildFeatureRegistry(CFeatures aFeatureRegistry)
        {
            //aFeatureRegistry.Add(typeof(CQuadrantSpriteData));
            var aAssemblies = new Assembly[] { typeof(CFacade).Assembly, this.GetType().Assembly };
            aFeatureRegistry.AddRange(aAssemblies);

        }
        #endregion


        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CFacade>(() => this);
            aServiceContainer.AddService<CFeatures>(() => this.Features);
            aServiceContainer.AddService<CAddInGameTheradAction>(() => new CAddInGameTheradAction(this.AddInGameThreadAction));
            return aServiceContainer;
        }
        #endregion   
        public abstract void AddInGameThreadAction(Action aAction);

    }

}
