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

namespace CharlyBeck.Mvi.Sprites.Gem
{
    using CGemPropability = CPropability<CGemEnum>;
    enum CGemClassEnum
    {
        Offense,
        Defense,
        Navigation,
    }

    internal enum CGemEnum
    {
        /////////////////////// Class .........Text...............................................................Hilfetext

        [CPropability(0.05)]
        ExtraLifeGem,        // Defense       <EXLF> // Extraleben.

        [CPropability(1d)]
        ShellRepair,         // Defense       <SHRP> // Hülle reparieren, hülle geht bei kollisionen kaputt

        [CPropability(1d)]
        ShieldGem,           // Defense       <SHLD> // Schutzschild, kamikatze flug auf planeten

        [CPropability(1d)]
        AmmoSpeed,           // Offense       <AMSP> // Schüsse flieen schneller

        [CPropability(1d)]
        FireRate,            // Offense       <FIRR> // Erhöht feuerrate

        [CPropability(1d)]
        LaserEnergyGem,      // Offense       <LSEN> // zum zerstören von asteroiden                              

        [CPropability(0.1)]
        NuclearMissileGem,   // Offense       <NUKE> // zum zerstören von sonnen4Destroy a sun. (Dont do when planets remain)

        [CPropability(0.2d)]
        GuidedMissile,       // Offense       <GDEM> // Hilfeich zum zsertören kleiner monde.                     Shoot to fire, press again to follow target.

        [CPropability(0.1d)]
        ThermalShieldGem,    // Navigation    <THSH> // Zum landen auf planeten                                   Land on a planet.

        [CPropability(0.05d)]
        KruskalScannerGem,   // Navigation    <WHLE> // Zum öffnen von wurmlöchern                                Turn a sun into a wormhole to teleport and teleport..

        [CPropability(0.2d)]
        SlowMotion,          // Navigation    <SLMO> // Verlangsamt alles                                         Turn down the speed of time.

        [CPropability(0.2d)]
        Antigravity,         // Navigation    <AGRA> // Gravitation von planeten hat keinen/weniger einfluss.     Turn off or lower gravity.

       // [CPropability(0.2d)]
       // AutoPilot,           // Navigation    <AUTP> // Zum automatischen folgen von umlaufbahnen.                Follow path of orb. Press when orb is focused.

        [CPropability(0.5d)]
        SpaceGrip, // Trägheit auf minimum.


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

    /// <summary>
    /// Item to collect, grants special abilites.
    /// </summary>
    public abstract class CGemSprite : CSprite
    {
        #region ctor
        internal CGemSprite(CServiceLocatorNode aParent) : base(aParent)
        {
            this.PlattformSpriteEnum = CPlatformSpriteEnum.Gem;
            this.CollisionSourceEnum = CCollisionSourceEnum.Gem;


            this.Init();
        }
        #endregion
        internal virtual CValue TargetValue => throw new NotImplementedException();
        internal virtual bool ModifyTargetValueIsEnabled => false;
        internal virtual CValue SourceValue => throw new NotImplementedException();
        internal abstract CGemClassEnum GemClassEnum { get; }
        internal void Collect()
        {
            if (this.ModifyTargetValueIsEnabled)
            {
                //throw new NotImplementedException();
                //this.TargetValue.Add(this.SourceValue);
            }

            this.DeallocateIsQueued = true;
            this.World.OnGemCollected(this);
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
                case CGemEnum.ExtraLifeGem: return new CNewFunc(() => new CExtraLifeGem(this));
                case CGemEnum.ShellRepair: return new CNewFunc(() => new CShellRepairGem(this));
                case CGemEnum.ShieldGem: return new CNewFunc(() => new CShieldGem(this));
                case CGemEnum.AmmoSpeed: return new CNewFunc(() => new CAmmoSpeedGem(this));
                case CGemEnum.FireRate: return new CNewFunc(() => new CFireRateGem(this));
                case CGemEnum.LaserEnergyGem: return new CNewFunc(() => new CLaserEnergyGem(this));
                case CGemEnum.NuclearMissileGem: return new CNewFunc(() => new CNuclearMissileGem(this));
                case CGemEnum.GuidedMissile: return new CNewFunc(() => new CGuidedMissileGem(this));
                case CGemEnum.ThermalShieldGem: return new CNewFunc(() => new CThermalShieldGem(this));
                case CGemEnum.KruskalScannerGem: return new CNewFunc(() => new CKruskalScannerGem(this));
                case CGemEnum.SlowMotion: return new CNewFunc(() => new CSlowMotionGem(this));
                case CGemEnum.Antigravity: return new CNewFunc(() => new CAntigravityGem(this));
                case CGemEnum.SpaceGrip: return new CNewFunc(() => new CSpaceGripGem(this));
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
