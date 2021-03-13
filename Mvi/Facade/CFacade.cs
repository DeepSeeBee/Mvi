using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Value;
using CharlyBeck.Mvi.Internal;
using CharlyBeck.Mvi.Sfx;
using CharlyBeck.Mvi.Sprites;
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
using CharlyBeck.Utils3.Asap;
using Microsoft.Xna.Framework;
using System.Threading;
using CharlyBeck.Mvi.ContentManager;

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


    public delegate void CAddInGameThreadAction(Action aAction);
    public enum CPlatformSpriteEnum
    {
        Avatar,
        Bumper,
        GridLines,
        Shot,
        Crosshair,
        Explosion,
        Gem,
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
            this.LookUp.Load();
            this.LookDown.Load();
            this.LookLeft.Load();
            this.LookRight.Load();
        }

        public CFacade() : this(new CDefaultServiceLocatorNode())
        {
        }
        protected override void Init()
        {
            base.Init();
            this.SoundManager.Load();
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
        #region SoundManager
        private CSoundManager SoundManagerM;
        private CSoundManager SoundManager
            => CLazyLoad.Get(ref this.SoundManagerM, () => new CSoundManager(this));
        #endregion
        public void Draw()
           => this.World.Draw();
        public void Update()
        {
            this.SoundManager.Update();
        }

        #region Values
        private CValues ValuesM;
        public CValues Values => CLazyLoad.Get(ref this.ValuesM, this.NewValueRegistry);
        public object VmValues => this.Values;
        private CValues NewValueRegistry()
        {
            var aValueRegistry = new CValues(this);
            this.BuildValueRegistry(aValueRegistry);
            return aValueRegistry;
        }
        protected virtual void BuildValueRegistry(CValues aValueRegistry)
        {
            var aAssemblies = new Assembly[] { typeof(CFacade).Assembly, this.GetType().Assembly };
            aValueRegistry.AddRange(aAssemblies);
        }
        #endregion
        #region IngameSequence
        public Task RunIngameTimedSequence(Action<int> aRunItem, TimeSpan aDelay, int aCount)
        {
            var aIngameAction = this.ServiceContainer.GetService<CAddInGameThreadAction>();
            var aTask = Task.Factory.StartNew(delegate ()
            {
                var aDoneEvent = new AutoResetEvent(false);
                for (var aIdx = 0; aIdx < aCount; ++aIdx)
                {
                    aIngameAction(delegate ()
                    {
                        aRunItem(aIdx);
                        aDoneEvent.Set();
                    });
                    System.Threading.Thread.Sleep((int)aDelay.TotalMilliseconds);
                    aDoneEvent.WaitOne();
                }
            });
            return aTask;
        }
        #endregion
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CFacade>(() => this);
            aServiceContainer.AddService<CValues>(() => this.Values);
            aServiceContainer.AddService<CAddInGameThreadAction>(() => new CAddInGameThreadAction(this.AddInGameThreadAction));
            aServiceContainer.AddService<CWorld>(() => this.World);
            aServiceContainer.AddService<CContentManager>(() => this.ContentManager);
            return aServiceContainer;
        }
        #endregion   
        public abstract void AddInGameThreadAction(Action aAction);
        #region Look

        [CMemberDeclaration]
        internal static readonly CCommandDeclaration LookUpDecl = new CCommandDeclaration(CValueEnum.Look_Up, new Guid("790166c7-a357-4355-953f-a4c3fc78ae97"));
            [CMemberDeclaration]
        internal static readonly CCommandDeclaration LookDownDecl = new CCommandDeclaration(CValueEnum.Look_Down, new Guid("eef4fc43-1ff7-479e-b38a-f0190dffcc06"));
        [CMemberDeclaration]
        internal static readonly CCommandDeclaration LookRightDecl = new CCommandDeclaration(CValueEnum.Look_Right, new Guid("afbc52f7-b718-4710-94b2-9cb3d4a90e28"));
        [CMemberDeclaration]
        internal static readonly CCommandDeclaration LookLeftDecl = new CCommandDeclaration(CValueEnum.Look_Left, new Guid("5f01a09a-7e94-45e5-b292-2d53c16b029c"));
        [CMemberDeclaration]
        internal static readonly CDoubleDeclaration LookAngleDecl = new CDoubleDeclaration(CValueEnum.Look_Angle, new Guid("492d14aa-9870-48c0-9031-f7517550827e"), true, CGuiEnum.Slider, CUnitEnum.Radians, (45d).ToRadians(), (0d).ToRadians(), (360d).ToRadians(), (45d).ToRadians(), (180d).ToRadians(), 0);

        private CDoubleValue LookAngleM;
        private CDoubleValue LookAngle => CLazyLoad.Get(ref this.LookAngleM, () => CDoubleValue.GetStaticValue<CDoubleValue>(this, LookAngleDecl));

        private void Look(Action aAction)
        {
            var aAngle = (int) this.LookAngle.Value.ToDegrees();
            this.RunIngameTimedSequence(i=>aAction(), new TimeSpan(0, 0, 0, 0, 10), aAngle);
        }
        private CCommand LookUpM;
        private CCommand LookUp => CLazyLoad.Get(ref this.LookUpM, this.NewLookUp);
        private CCommand NewLookUp()
        {
            var aCmd = CCommand.GetStaticValue<CCommand>(this, LookUpDecl);
            aCmd.Invoked += delegate ()
            {
               this.Look(delegate () { this.World.LookUpDown = this.World.LookUpDown + 1d.ToRadians(); });
            };
            return aCmd;
        }
        private CCommand LookDownM;
        private CCommand LookDown => CLazyLoad.Get(ref this.LookDownM, this.NewLookDown);
        private CCommand NewLookDown()
        {
            var aCmd = CCommand.GetStaticValue<CCommand>(this, LookDownDecl);
            aCmd.Invoked += delegate ()
            {
               this.Look(delegate () { this.World.LookUpDown = this.World.LookUpDown - 1d.ToRadians(); });
            };
            return aCmd;
        }
        private CCommand LookLeftM;
        private CCommand LookLeft => CLazyLoad.Get(ref this.LookLeftM, this.NewLookLeft);
        private CCommand NewLookLeft()
        {
            var aCmd = CCommand.GetStaticValue<CCommand>(this, LookLeftDecl);
            aCmd.Invoked += delegate ()
            {
               this.Look(delegate () { this.World.LookLeftRight = this.World.LookLeftRight + 1d.ToRadians(); });
            };
            return aCmd;
        }
        private CCommand LookRightM;
        private CCommand LookRight => CLazyLoad.Get(ref this.LookRightM, this.NewLookRight);
        private CCommand NewLookRight()
        {
            var aCmd = CCommand.GetStaticValue<CCommand>(this, LookRightDecl);
            aCmd.Invoked += delegate ()
            {
               this.Look(delegate () { this.World.LookLeftRight = this.World.LookLeftRight - 1d.ToRadians(); });
            };
            return aCmd;
        }
        #endregion
        #region ContentManager
        private CContentManager ContentManagerM;
        private CContentManager ContentManager => CLazyLoad.Get(ref this.ContentManagerM, () => new CContentManager(this));
        #endregion
    }

}
