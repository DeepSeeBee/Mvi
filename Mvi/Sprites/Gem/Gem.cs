using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Models;
using CharlyBeck.Mvi.Sprites.Gem.Internal;
using CharlyBeck.Mvi.Sprites.Shot;
using CharlyBeck.Mvi.Story.Propability;
using CharlyBeck.Mvi.Value;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.Reflection;
using CharlyBeck.Utils3.ServiceLocator;
using Mvi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharlyBeck.Utils3.Asap;
using CharlyBeck.Mvi.Sprites.GemSlot;
using System.Diagnostics;

namespace CharlyBeck.Mvi.Sprites.Gem
{
    using CGemPropability = CPropability<CGemEnum>;
    public enum CGemCategoryEnum
    {
        Offense,
        Defense,
        Navigation,
    }

    public sealed class CGemShortNameAttribute : Attribute
    {
        public CGemShortNameAttribute(string aShortName)
        {
            this.ShortName = aShortName;
        }
        public readonly string ShortName;
    }
    public sealed class CGemNameAttribute : Attribute
    {
        public CGemNameAttribute(string aName)
        {
            this.Name = aName;
        }
        public readonly string Name;
    }

    public sealed class CGemDescriptionAttribute : Attribute
    {
        public CGemDescriptionAttribute(string aDescription)
        {
            this.Description = aDescription;
        }
        public readonly string Description;
    }

    public sealed class CGemAffectSpaceAttribute : Attribute
    {
        public CGemAffectSpaceAttribute(bool aAffectSpace)
        {
            this.AffectSpace = aAffectSpace;
        }
        public readonly bool AffectSpace;
    }


    public sealed class CGemAffectSurfaceAttribute : Attribute
    {
        public CGemAffectSurfaceAttribute(bool aAffectSurface)
        {
            this.AffectSurface = aAffectSurface;
        }
        public readonly bool AffectSurface;
    }

    public sealed class CGemCategoryEnumAttribute : Attribute
    {
        public CGemCategoryEnumAttribute(CGemCategoryEnum aGemCategoryEnum)
        {
            this.GemCategoryEnum = aGemCategoryEnum;
        }
        public readonly CGemCategoryEnum GemCategoryEnum;
    }

    internal enum CGemEnum
    {
        /////////////////////// Class .........Text...............................................................Hilfetext


        [CPropability(0.1)]
        [CGemShortName("EXLF")]
        [CGemName("Extra Life")]
        [CGemCategoryEnum(CGemCategoryEnum.Defense)]
        [CGemDescription("Increments ship count.")]
        [CGemAffectSpace(true)]
        [CGemAffectSurface(true)]
        ExtraLife,        // Defense       <EXLF> // Extraleben.

        [CPropability(1d)]
        [CGemShortName("SHRP")]
        [CGemName("Shell repair")]
        [CGemCategoryEnum(CGemCategoryEnum.Defense)]
        [CGemDescription("Repairs the shell which is damaged on collisions.")]
        [CGemAffectSpace(true)]
        [CGemAffectSurface(true)]
        ShellRepair,         // Defense       <SHRP> // Hülle reparieren, hülle geht bei kollisionen kaputt

        [CPropability(1d)]
        [CGemShortName("SHLD")]
        [CGemName("Shield")]
        [CGemCategoryEnum(CGemCategoryEnum.Defense)]
        [CGemDescription("Go kamikaze and fly through matter.")]
        [CGemAffectSpace(true)]
        [CGemAffectSurface(true)]
        Shield,           // Defense       <SHLD> // Schutzschild, kamikatze flug auf planeten

        [CPropability(1d)]
        [CGemShortName("AMTH")]
        [CGemName("Ammo thickness")]
        [CGemCategoryEnum(CGemCategoryEnum.Offense)]
        [CGemDescription("Increases thickness of shots.")]
        [CGemAffectSpace(true)]
        [CGemAffectSurface(true)]
        AmmoThickness,

        [CPropability(1d)]
        [CGemShortName("AMSP")]
        [CGemName("Ammo speed")]
        [CGemCategoryEnum(CGemCategoryEnum.Offense)]
        [CGemDescription("Increases speed of shots")]
        [CGemAffectSpace(true)]
        [CGemAffectSurface(true)]
        AmmoSpeed,           // Offense       <AMSP> // Schüsse flieen schneller

        [CPropability(1d)]
        [CGemShortName("FIRR")]
        [CGemName("Fire rate")]
        [CGemCategoryEnum(CGemCategoryEnum.Offense)]
        [CGemDescription("Increases fire rate.")]
        [CGemAffectSpace(true)]
        [CGemAffectSurface(true)]
        FireRate,            // Offense       <FIRR> // Erhöht feuerrate

        [CPropability(1d)]
        [CGemShortName("AMEN")]
        [CGemName("Ammo energy")]
        [CGemCategoryEnum(CGemCategoryEnum.Offense)]
        [CGemDescription("Increases strength of ammo.")]
        [CGemAffectSpace(true)]
        [CGemAffectSurface(true)]
        AmmoEnergy,      // Offense       <LSEN> // zum zerstören von asteroiden                              

        [CPropability(0.25)]
        [CGemShortName("NUKE")]
        [CGemName("Nuclear missile")]
        [CGemCategoryEnum(CGemCategoryEnum.Offense)]
        [CGemDescription("Produces a huge explosion.")]
        [CGemAffectSpace(true)]
        [CGemAffectSurface(true)]
        NuclearMissile,   // Offense       <NUKE> // zum zerstören von sonnen4Destroy a sun. (Dont do when planets remain)

        [CPropability(0.4d)]
        [CGemShortName("GDMS")]
        [CGemName("Guided Missile")]
        [CGemCategoryEnum(CGemCategoryEnum.Offense)]
        [CGemDescription("Target seeking missile.")]
        [CGemAffectSpace(true)]
        [CGemAffectSurface(true)] // Verhält sich, wie ein starker schuss.
        GuidedMissile,       // Offense       <GDEM> // Hilfeich zum zsertören kleiner monde.                     Shoot to fire, press again to follow target.

        [CPropability(0.3d)]
        [CGemShortName("THSH")]
        [CGemName("Thermal shield")]
        [CGemCategoryEnum(CGemCategoryEnum.Navigation)]
        [CGemDescription("Land on a planet.")]
        [CGemAffectSpace(true)]
        [CGemAffectSurface(true)] // Verhält sich wie normales schild.
        ThermalShield,    // Navigation    <THSH> // Zum landen auf planeten                                   Land on a planet.

        [CPropability(0.15d)]
        [CGemShortName("KRSK")]
        [CGemName("Kruskal scanner")]
        [CGemCategoryEnum(CGemCategoryEnum.Navigation)]
        [CGemDescription("Produce wormholes upon planet hit.")]
        [CGemAffectSpace(true)]
        [CGemAffectSurface(true)]
        KruskalScanner,   // Navigation    <WHLE> // Zum öffnen von wurmlöchern                                Turn a sun into a wormhole to teleport and teleport..

        [CPropability(0.4d)]
        [CGemShortName("SLMO")]
        [CGemName("Slow motion")]
        [CGemCategoryEnum(CGemCategoryEnum.Navigation)]
        [CGemDescription("Decelerate time.")]
        [CGemAffectSpace(true)]
        [CGemAffectSurface(true)]
        SlowMotion,          // Navigation    <SLMO> // Verlangsamt alles                                         Turn down the speed of time.

        [CPropability(0.4d)]
        [CGemShortName("AGRA")]
        [CGemName("AntiGravity")]
        [CGemCategoryEnum(CGemCategoryEnum.Navigation)]
        [CGemDescription("Reduce gravity.")]
        [CGemAffectSpace(true)]
        [CGemAffectSurface(true)]
        AntiGravity,         // Navigation    <AGRA> // Gravitation von planeten hat keinen/weniger einfluss.     Turn off or lower gravity.

       // [CPropability(0.2d)]
       // AutoPilot,           // Navigation    <AUTP> // Zum automatischen folgen von umlaufbahnen.                Follow path of orb. Press when orb is focused.

        [CPropability(0.3d)]
        [CGemShortName("SPCG")]
        [CGemName("Space grip")]
        [CGemCategoryEnum(CGemCategoryEnum.Navigation)]
        [CGemDescription("Reduce inertia.")]
        [CGemAffectSpace(true)]
        [CGemAffectSurface(true)]
        SpaceGrip, // Trägheit auf minimum.

        [CPropability(0.2d)]
        [CGemShortName("DRLL")]
        [CGemName("Earth Drill")]
        [CGemCategoryEnum(CGemCategoryEnum.Offense)]
        [CGemDescription("Drill holes into planets.")]
        [CGemAffectSpace(true)] // Wirkt wie ein starker schuss.
        [CGemAffectSurface(true)]
        Drill, // Trägheit auf minimum.

        //
        //FuelGem,
        //QuestGem,
    }


    public sealed class CGemModel: CModel
    {
        internal CGemModel(CServiceLocatorNode aParent):base(aParent)
        {
            this.Octaeder = new COctaeder();
        }

        public readonly COctaeder Octaeder;
        
    }

    internal abstract class CGemCategory : CServiceLocatorNode
    {
        internal CGemCategory(CServiceLocatorNode aParent)
        {

        }
        internal CGemCategoryEnum GemClassEnum;
        internal CVector3Dbl Color;
    }

    internal sealed class CGemCategories : CServiceLocatorNode
    {
        #region ctor
        internal CGemCategories(CServiceLocatorNode aParent): base(aParent)
        {
            this.Array = this.NewGemClasses();
        }
        #endregion
        #region GemClasses
        private CGemCategory[] NewGemClasses()
        {
            var aCount = typeof(CGemCategoryEnum).GetEnumMaxValue() + 1;
            var aGemCategories = new CGemCategory[aCount];
            aGemCategories[(int)CGemCategoryEnum.Offense] = new COffenseGemClass(this);
            aGemCategories[(int)CGemCategoryEnum.Defense] = new CDefenseGemClass(this);
            aGemCategories[(int)CGemCategoryEnum.Navigation] = new CNavigationGemClass(this);
            return aGemCategories;
        }
        internal readonly CGemCategory[] Array;
        internal CGemCategory Get(CGemCategoryEnum aGemCategoryEnum)
            => this.Array[(int)aGemCategoryEnum];
        #endregion
    }

    internal sealed class CDefenseGemClass : CGemCategory
    {
        internal CDefenseGemClass(CServiceLocatorNode aParent):base(aParent)
        {
            this.GemClassEnum = CGemCategoryEnum.Defense;
            this.Color = CColors.Green;
        }
    }

    internal sealed class CNavigationGemClass : CGemCategory
    {
        internal CNavigationGemClass(CServiceLocatorNode aParent):base(aParent)
        {
            this.GemClassEnum = CGemCategoryEnum.Navigation;
            this.Color = CColors.Blue;
        }
    }

    internal sealed class COffenseGemClass : CGemCategory
    {
        internal COffenseGemClass(CServiceLocatorNode aParent):base(aParent)
        {
            this.GemClassEnum = CGemCategoryEnum.Offense;
            this.Color = CColors.Red;
        }
    }

    /// <summary>
    /// Item to collect, grants special abilites.
    /// </summary>
    public abstract class CGemSprite : CSprite
    {
        #region ctor
        internal CGemSprite(CServiceLocatorNode aParent, CGemEnum aGemEnum) : base(aParent)
        {
            this.GemEnum = aGemEnum;
            this.GemCategories = this.ServiceContainer.GetService<CGemCategories>();

            this.GemCategoryEnum = aGemEnum.GetCustomAttribute<CGemCategoryEnumAttribute>().GemCategoryEnum;
            this.GemCategory = this.GemCategories.Get(this.GemCategoryEnum);
            this.Name = aGemEnum.GetCustomAttribute<CGemNameAttribute>().Name;
            this.ShortName = aGemEnum.GetCustomAttribute<CGemShortNameAttribute>().ShortName;

            this.PlattformSpriteEnum = CPlatformSpriteEnum.Gem;
            this.CollisionSourceEnum = CCollisionSourceEnum.Gem;

            this.Init();
        }
        protected override void Init()
        {
            base.Init();
        }
        protected override void OnEndUse()
        {
            base.OnEndUse();

            Debug.Assert(!(this.GemSlotNullable is object));
        }
        #endregion
        internal readonly CGemEnum GemEnum;
        #region GemCategories
        private readonly CGemCategories GemCategories;

        #endregion
        #region strings
        internal string Name;
        internal string ShortName;
        #endregion
        internal virtual CValue TargetValue => throw new NotImplementedException();
        internal virtual bool ModifyTargetValueIsEnabled => false;
        internal virtual CValue SourceValue => throw new NotImplementedException();
        internal readonly CGemCategoryEnum GemCategoryEnum;
        internal void Collect()
        {
            if (this.ModifyTargetValueIsEnabled)
            {
                //throw new NotImplementedException();
                //this.TargetValue.Add(this.SourceValue);
            }

            this.World.OnGemCollected(this);
            if(this.IsReferenced)
            {
                this.IsHiddenInWorld = true;
                this.TimeToLive = default;
            }
            else
            {
                this.DeallocateIsQueued = true;
            }
        }
        protected override void OnCollide(CSprite aCollideWith, double aDistance)
        {
            base.OnCollide(aCollideWith, aDistance);

            this.Collect();
        }
        internal override void Collide()
        {
            base.Collide();
        }
        internal virtual void BuildGem(CSprite aDestroyed, CShotSprite aDestroying, CRandomGenerator aRandomGenerator)
        {
            this.WorldPos = aDestroyed.WorldPos.Value;
            this.TimeToLive = new TimeSpan(0, 0, 10);
            this.Scale = 0.05d;
            this.Radius = 0.05d;
        }

        internal override void Update(CFrameInfo aFrameInfo)
        {
            base.Update(aFrameInfo);

            this.WorldMatrix = this.NewWorldMatrix();

            this.Reposition();
        }
        internal CGemSlot GemSlotNullable;
        internal bool IsReferenced => this.GemSlotNullable is object;
        internal CGemCategory GemCategory { get; private set; }
    }

    internal sealed class CGemSpriteManager : CMultiPoolSpriteManager<CGemSprite, CGemEnum>
    {

        internal CGemSpriteManager(CServiceLocatorNode aParent) : base(aParent)
        {
            this.AddOnAllocate = true;

            this.RandomGenerator = new CRandomGenerator(this);
            this.RandomGenerator.Begin();
            this.GemPropability = CGemPropability.NewFromEnum(this);
            this.World.SpriteDestroyedByShot += this.OnSpriteDestroyedByShot;

            this.Init();
        }

        protected override void Init()
        {
            base.Init();

            { // Reserve
                var aLock = true;
                foreach (var aGemClass in typeof(CGemEnum).GetEnumValues().Cast<CGemEnum>())
                {
                    this.Reserve(aGemClass, CStaticParameters.Gem_Class_InstanceCount, aLock);
                }
            }
        }

        private readonly CGemPropability GemPropability;
        internal override int SpriteClassCount => typeof(CGemEnum).GetEnumMaxValue() + 1;
        internal override CNewFunc GetNewFunc(CGemEnum aClassEnum)
        {
            switch (aClassEnum)
            {
                case CGemEnum.ExtraLife: return new CNewFunc(() => new CExtraLifeGem(this));
                case CGemEnum.ShellRepair: return new CNewFunc(() => new CShellRepairGem(this));
                case CGemEnum.Shield: return new CNewFunc(() => new CShieldGem(this));
                case CGemEnum.AmmoSpeed: return new CNewFunc(() => new CAmmoSpeedGem(this));
                case CGemEnum.FireRate: return new CNewFunc(() => new CFireRateGem(this));
                case CGemEnum.AmmoEnergy: return new CNewFunc(() => new CAmmoEnergyGem(this));
                case CGemEnum.NuclearMissile: return new CNewFunc(() => new CNuclearMissileGem(this));
                case CGemEnum.GuidedMissile: return new CNewFunc(() => new CGuidedMissileGem(this));
                case CGemEnum.ThermalShield: return new CNewFunc(() => new CThermalShieldGem(this));
                case CGemEnum.KruskalScanner: return new CNewFunc(() => new CKruskalScannerGem(this));
                case CGemEnum.SlowMotion: return new CNewFunc(() => new CSlowMotionGem(this));
                case CGemEnum.AntiGravity: return new CNewFunc(() => new CAntigravityGem(this));
                case CGemEnum.SpaceGrip: return new CNewFunc(() => new CSpaceGripGem(this));
                case CGemEnum.Drill: return new CNewFunc(() => new CDrillGem(this));
                case CGemEnum.AmmoThickness: return new CNewFunc(() => new CAmmoThicknessGem(this));
                default:
                    throw new NotImplementedException();
            }
        }
        private readonly CRandomGenerator RandomGenerator;

        private void OnSpriteDestroyedByShot(CSprite aDestroyed, CShotSprite aDestroying)
        {
            var aGemClass = this.GemPropability.Next();
            var aGem = this.AllocateSpriteNullable(aGemClass);
            if(aGem is object)
            {
                aGem.BuildGem(aDestroyed, aDestroying, this.RandomGenerator);
                aGem.Update(this.World.FrameInfo);
            }
        }

        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CRandomGenerator>(() => this.RandomGenerator);
            return aServiceContainer;
        }
        #endregion

    }

}
