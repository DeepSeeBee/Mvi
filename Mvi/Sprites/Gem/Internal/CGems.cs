using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Mvi.Sprites.GemSlot;
using CharlyBeck.Mvi.Sprites.Shot;
using CharlyBeck.Mvi.Value;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.Enumerables;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*Ablauf:
 *  - Mit Blaster Asteroiden zerstören:
 *      - Belohnung: Laser, ShellRepair, Shield 
 *      
 *  - Mit Laser Planeten Zerstören:
 *      - Belohnung: ThermalShield, DrillEnergy
 * 
 *  - Auf planeten landen und nuklear rakete sammeln
 *  
 *  - Sonne zerstören und Kruskal-Scanner Gem erhalten
 * 
 * 
 * CExtraLifeGem
 * CShellRepair
 * CShieldGem
 * CLaserEnergyGem           *
 * CNuclearMissileGem        *
 * CThermalShieldGem         *
 * CDrillEnergyGem           *
 * CKruskalScannerGem        *
 *
 * CFuelGem
 * (CGemDetector)
 * 
 * CQuestGem 
 * 
 * 
 * 
 * 
 * 
 * 
 */
namespace CharlyBeck.Mvi.Sprites.Gem.Internal
{

    internal sealed class CGemActivateOnCollectIfNoSlotAttribute : Attribute
    {
        internal CGemActivateOnCollectIfNoSlotAttribute(bool aGemActivateOnCollectIfNoSlotAttribute)
        {
            this.ActivateOnCollectIfNoSlot = aGemActivateOnCollectIfNoSlotAttribute;
        }
        internal readonly bool ActivateOnCollectIfNoSlot;
    }

    internal sealed class CGemShortNameAttribute : Attribute
    {
        public CGemShortNameAttribute(string aShortName)
        {
            this.ShortName = aShortName;
        }
        public readonly string ShortName;
    }
    internal sealed class CGemNameAttribute : Attribute
    {
        public CGemNameAttribute(string aName)
        {
            this.Name = aName;
        }
        public readonly string Name;
    }

    internal sealed class CGemDescriptionAttribute : Attribute
    {
        public CGemDescriptionAttribute(string aDescription)
        {
            this.Description = aDescription;
        }
        public readonly string Description;
    }

    internal sealed class CGemAffectSpaceAttribute : Attribute
    {
        public CGemAffectSpaceAttribute(bool aAffectSpace)
        {
            this.AffectSpace = aAffectSpace;
        }
        public readonly bool AffectSpace;
    }
    internal sealed class CGemShotEnumAttribute : Attribute
    {
        public CGemShotEnumAttribute(CShotEnum aShotEnum)
        {
            this.ShotEnum = aShotEnum;
        }
        public readonly CShotEnum ShotEnum;
    }

    internal sealed class CGemCollectIfValueNotFullAttribute : Attribute
    {
        public CGemCollectIfValueNotFullAttribute(CValueEnum aValueEnum, bool aLowerCategoryNeedsToBeFull)
        {
            this.ValueEnum = aValueEnum;
            this.LowerCategoryNeedsToBeFull = aLowerCategoryNeedsToBeFull;
        }
        internal readonly CValueEnum ValueEnum;
        internal readonly bool LowerCategoryNeedsToBeFull;
    }


    internal sealed class CGemAffectSurfaceAttribute : Attribute
    {
        public CGemAffectSurfaceAttribute(bool aAffectSurface)
        {
            this.AffectSurface = aAffectSurface;
        }
        public readonly bool AffectSurface;
    }

    internal sealed class CGemCategoryEnumAttribute : Attribute
    {
        public CGemCategoryEnumAttribute(CGemCategoryEnum aGemCategoryEnum)
        {
            this.GemCategoryEnum = aGemCategoryEnum;
        }
        public readonly CGemCategoryEnum GemCategoryEnum;
    }

    /// <summary>
    /// Gives an extra live to the player
    /// </summary>
    internal sealed class CExtraLifeGem : CCollectedGemSprite
    {
        #region ctor
        internal CExtraLifeGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.ExtraLife)
        {
            this.ActiveDurationIsEnabled = false;
            this.ActivateOnCollect = true;
            this.Init();
        }
        internal override CValueModifier NewDefaultValueModifier()
            => new CAddInt64Modifier(this)
            {
                ApplyValue = 1,
                Value = this.AvatarValues.LifeCountValue,
            };
        #endregion
    }

    /// <summary>
    /// Repairs the shell.
    /// </summary>
    internal sealed class CShellRepairGem : CCollectedGemSprite
    {
        #region ctor
        internal CShellRepairGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.ShellRepair)
        {
            this.ActiveDurationIsEnabled = false;
            this.Init();
        }
        internal override CValueModifier NewDefaultValueModifier()
        => new CAddDoubleModifier(this)
        {
            ApplyValue = CStaticParameters.Gem_ShellRepair_ModifierValue,
            Value = this.AvatarValues.ShellValue,
        };
        #endregion
    }

    /// <summary>
    /// Add shield energy
    /// </summary>
    internal sealed class CShieldGem : CCollectedGemSprite
    {
        #region ctor
        internal CShieldGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.Shield) 
        {
            this.ActiveDurationIsEnabled = false;
            this.Init();
        }
        internal override CValueModifier NewDefaultValueModifier()
            => new CAddDoubleModifier(this)
            {
                ApplyValue = CStaticParameters.Gem_Shield_ModifierValue,
                Value = this.AvatarValues.ShieldValue,
            };
        #endregion
    }

    internal sealed class CAmmoEnergyGem  : CCollectedGemSprite
    {
        #region ctor
        internal CAmmoEnergyGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.AmmoEnergy) 
        {
            this.ActiveDurationIsEnabled = true;
            this.Init();
        }
        internal override CValueModifier NewDefaultValueModifier()
            => new CAddDoubleModifier(this)
            {
                ApplyValue = CStaticParameters.Gem_AmmoEnergy_ModifierValue,
                UnapplyValue = -CStaticParameters.Gem_AmmoEnergy_ModifierValue,
                Value = this.AvatarValues.AmmoEnergyValue,
            };
        #endregion
    }
    internal sealed class CAmmoThicknessGem : CCollectedGemSprite
    {
        #region ctor
        internal CAmmoThicknessGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.AmmoThickness)
        {
            this.ActiveDurationIsEnabled = true;
            this.Init();
        }
        internal override CValueModifier NewDefaultValueModifier()
            => new CAddDoubleModifier(this)
            {
                ApplyValue = CStaticParameters.Gem_AmmoThickness_ModifierValue,
                UnapplyValue = -CStaticParameters.Gem_AmmoThickness_ModifierValue,
                Value = this.AvatarValues.AmmoThicknessValue,
            };
        #endregion
    }
    /// <summary>
    /// Required for destroing suns
    /// </summary>
    internal sealed class CNuclearMissileGem : CCollectedGemSprite
    {
        #region ctor
        internal CNuclearMissileGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.NuclearMissile) 
        {
            this.ActivateOnCollect = true;
            this.ActiveDurationIsEnabled = false;
            this.Init();
        }
        internal override CValueModifier NewDefaultValueModifier()
            => new CAddInt64Modifier(this)
            {
                ApplyValue = CStaticParameters.Gem_NuclearMissile_ModifierValue,
                Value = this.AvatarValues.NuclearMissileCountValue,
            };
        #endregion
    }

    /// <summary>
    /// Required for landing on a planet.
    /// </summary>
    internal sealed class CThermalShieldGem : CCollectedGemSprite
    {
        #region ctor
        internal CThermalShieldGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.ThermalShield) 
        {
            this.ActiveDurationIsEnabled = false;

            this.Init();
        }
        internal override CValueModifier NewDefaultValueModifier()
            => new CAddTimeSpanModifier(this)
            {
                ApplyValue = CStaticParameters.Gem_ThermalShield_ModifierValue,
                Value = this.AvatarValues.ThermalShieldValue,
            };
        #endregion
    }

    /// <summary>
    /// Required for finding wormholes.Primarly retrieved by destroying suns.
    /// </summary>
    internal sealed class CKruskalScannerGem : CCollectedGemSprite
    {
        #region ctor
        internal CKruskalScannerGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.KruskalScanner)
        {
            this.ActiveDurationIsEnabled = false;
            this.Init();
        }
        internal override CValueModifier NewDefaultValueModifier()
            => new CAddInt64Modifier(this)
            {
                ApplyValue = CStaticParameters.Gem_KruskalScanner_ModifierValue,
                Value = this.AvatarValues.KruskalScannerCountValue,
            };
        #endregion

    }


    /// <summary>
    /// Zum Bohren in planeten benötigt.
    /// </summary>
    internal sealed class CDrillGem : CCollectedGemSprite
    {
        #region ctor
        internal CDrillGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.Drill)
        {
            this.ActiveDurationIsEnabled = false;
            this.ActivateOnCollect = true;
            this.Init();
        }
        internal override CValueModifier NewDefaultValueModifier()
            => new CAddInt64Modifier(this)
            {
                ApplyValue = CStaticParameters.Gem_Drill_ModifierValue,
                Value = this.AvatarValues.DrillCountValue,
            };
        #endregion
    }

    internal sealed class CAmmoSpeedGem : CCollectedGemSprite
    {
        #region ctor
        internal CAmmoSpeedGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.AmmoSpeed)
        {
            this.ActiveDurationIsEnabled = true;
            this.Init();
        }
        internal override CValueModifier NewDefaultValueModifier()
        => new CAddDoubleModifier(this)
        {
            ApplyValue = CStaticParameters.Gem_AmmoSpeed_ModifierValue,
            UnapplyValue = -CStaticParameters.Gem_AmmoSpeed_ModifierValue,
            Value = this.AvatarValues.AmmoSpeedValue,
        };
        #endregion
    }
    internal sealed class CFireRateGem : CCollectedGemSprite
    {
        #region ctor
        internal CFireRateGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.FireRate)
        {
            this.ActiveDurationIsEnabled = true;
            this.Init(); 
        }
        internal override CValueModifier NewDefaultValueModifier()
        => new CAddDoubleModifier(this)
        {
            ApplyValue = CStaticParameters.Gem_AmmoFireRate_ModifierValue,
            UnapplyValue = -CStaticParameters.Gem_AmmoFireRate_ModifierValue,
            Value = this.AvatarValues.AmmoFireRateValue,
        };
        #endregion

    }

    internal sealed class CSlowMotionGem :CCollectedGemSprite
    {
        #region ctor
        internal CSlowMotionGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.SlowMotion)
        {
            this.ActiveDurationIsEnabled = true;
            this.Init();
        }
        internal override CValueModifier NewDefaultValueModifier()
        => new CAddDoubleModifier(this)
        {
            ApplyValue = CStaticParameters.Gem_SlowMotion_ModifierValue,
            UnapplyValue = -CStaticParameters.Gem_SlowMotion_ModifierValue,
            Value = this.AvatarValues.SlowMotionValue,
        };
        #endregion 
    }

    internal sealed class CAntiGravityGem : CCollectedGemSprite
    {
        #region ctor
        internal CAntiGravityGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.AntiGravity)
        {
            this.ActiveDurationIsEnabled = true;
            this.Init();
        }
        internal override CValueModifier NewDefaultValueModifier()
        => new CAddDoubleModifier(this)
        {
            ApplyValue = CStaticParameters.Gem_AntiGravity_ModifierValue,
            UnapplyValue = -CStaticParameters.Gem_AntiGravity_ModifierValue,
            Value = this.AvatarValues.AntiGravityValue,
        };
        #endregion
    }

    //internal sealed class CGemManager :  CServiceLocatorNode
    //{
    //    #region ctor
    //    internal CGemManager(CServiceLocatorNode aParent) : base(aParent) { }
    //    #endregion
    //}

    internal sealed class CGuidedMissileGem : CCollectedGemSprite
    {
        internal CGuidedMissileGem(CServiceLocatorNode aParent):base(aParent, CGemEnum.GuidedMissile)
        {
            this.ActiveDurationIsEnabled = false;
            this.ActivateOnCollect = true;
            this.Init();
        }
        internal override CValueModifier NewDefaultValueModifier()
               => new CAddInt64Modifier(this)
               {
                   ApplyValue = CStaticParameters.Gem_GuidedMissile_ModifierValue,
                   Value = this.AvatarValues.GuidedMissileCountValue,
               };
    }

    internal sealed class CSpaceGripGem : CCollectedGemSprite
    {
        internal CSpaceGripGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.SpaceGrip)
        {
            this.ActiveDurationIsEnabled = true;
            this.Init();
        }
        internal override CValueModifier NewDefaultValueModifier()
            => new CAddDoubleModifier(this)
            {
                ApplyValue = CStaticParameters.Gem_SpaceGrip_ModifierValue,
                UnapplyValue = -CStaticParameters.Gem_SpaceGrip_ModifierValue,
                Value = this.AvatarValues.SpaceGripValue,
            };
    }

    /// <summary>
    /// Bonus item that is first emmited to space,
    /// when collected creates another gem, whichs type is dependend
    /// on the current gemslotstate.
    /// </summary>
    internal sealed class CCollectableGemSprite : CGemSprite
    {
        #region ctor
        internal CCollectableGemSprite(CServiceLocatorNode aParent) : base(aParent, CGemEnum.Collectable)
        {
            this.GemSpriteManager = this.ServiceContainer.GetService<CGemSpriteManager>();

            this.Init();
        }
        #endregion
        private readonly CGemSpriteManager GemSpriteManager;
        #region GemSlots
        private CGemSlotControlsSpriteManager GemSlotControlsSpriteManagerM;
        private CGemSlotControlsSpriteManager GemSlotControlsSpriteManager => CLazyLoad.Get(ref this.GemSlotControlsSpriteManagerM, () => this.ServiceContainer.GetService<CGemSlotControlsSpriteManager>());
        private CGemSlotControlsSprite GemSlotControlsSprite => this.GemSlotControlsSpriteManager.GemSlotControlsSprite;
        #endregion
        #region Collect
        internal override void Collect()
        {
            base.Collect();

            var aSlotSprite = this.CreateCollectedSpriteNullable();
            if(aSlotSprite is object)
            {
                aSlotSprite.Collect();
            }
            else
            {
                // TODO  - Play zonk sound
            }
            this.DeallocateIsQueued = true;
        }
        private CGemSprite CreateCollectedSpriteNullable()
        {
            var aGemEnums = this.GemSlotControlsSprite.GetCollectableGemEnums().ToArray();
            if(aGemEnums.IsEmpty())
            {
                // TODO_SND: All is full, what to do now?
            }
            else
            {
                var aGem = this.GemSpriteManager.CreateGemNullable(aGemEnums, this.WorldPos.Value);
                return aGem;
            }
            return default;
        }
        #endregion

    }
}
