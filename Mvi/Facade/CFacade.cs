using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Feature;
using CharlyBeck.Mvi.Internal;
using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Mvi.Sprites.Cube;
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
using Utils3.Asap;

namespace CharlyBeck.Mvi.Facade
{
    public abstract class CPlatformSprite : CReuseable
    {
        protected CPlatformSprite(CServiceLocatorNode aParent):base(aParent)
        {
        }
        
        public abstract void Draw();

        public virtual void Reposition()
        {
        }

        //  public abstract CSprite Sprite { get; }
    }


    public delegate void CAddInGameTheradAction(Action aAction);
    public enum CPlatformSpriteEnum
    {
        Bumper,
        Cube,
        Shot,
        Crosshair,
        _Count
    }
    public delegate CPlatformSprite CNewPlatformSpriteFunc(Tuple<CServiceLocatorNode, CSprite> aParentAndSprite);

    public sealed class CPlatformSpriteFactory : CServiceLocatorNode
    {
        
        internal CPlatformSpriteFactory(CServiceLocatorNode aParent):base(aParent)
        {
            var c = (int)CPlatformSpriteEnum._Count;
            var aEntries = new CEntry[c];
            for(var i = 0; i < c; ++i)
            {
                var aObjectPool = new CObjectPool<CPlatformSprite>();
                var aEntry = new CEntry(aObjectPool);
                aEntries[i] = aEntry;
            }
            this.Entries = aEntries;
        }


        private class CEntry
        {
            internal CEntry(CObjectPool<CPlatformSprite> aObjectPool)
            {
                this.ObjectPool = aObjectPool;
            }
            internal CNewPlatformSpriteFunc NewFunc;
            internal CObjectPool<CPlatformSprite> ObjectPool;
        }
        private readonly CEntry[] Entries;

        public CNewPlatformSpriteFunc this[CPlatformSpriteEnum aSpriteEnum]
        {
            get => this.Entries[(int)aSpriteEnum].NewFunc;
            set => this.Entries[(int)aSpriteEnum].NewFunc = value;
        }

        internal CPlatformSprite NewPlatformSprite(CSprite aSprite)
        {
            var aEntry = this.Entries[(int)aSprite.PlattformSpriteEnum];
            var aObjectPool = aEntry.ObjectPool;
            var aPlatformSprite = aObjectPool.Allocate(() => aEntry.NewFunc(new Tuple<CServiceLocatorNode, CSprite>(this, aSprite)));
            return aPlatformSprite;
        }
    }

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


        #region PlatformSpriteFactory
        private CPlatformSpriteFactory PlatformSpriteFactoryM;
        public CPlatformSpriteFactory PlatformSpriteFactory { get => CLazyLoad.Get(ref this.PlatformSpriteFactoryM, this.NewPlatformSpriteFactory); }
        private CPlatformSpriteFactory NewPlatformSpriteFactory()
        {
            var aFactory = new CPlatformSpriteFactory(this);
            this.BuildPlatformSpriteFactory(aFactory);
            return aFactory;
        }
        protected abstract void BuildPlatformSpriteFactory(CPlatformSpriteFactory aPlatformSpriteFactory);
        #endregion

        public void Draw()
           => this.World.Draw();


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
