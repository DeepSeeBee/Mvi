using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Sprites;
using CharlyBeck.Mvi.Sprites.Shot;
using CharlyBeck.Mvi.Story.Propability;
using CharlyBeck.Mvi.Story.Value;
using CharlyBeck.Mvi.World;
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
    enum CHotKeyEnum
    {
        GemDetector,
        DrillEnergy,
        KruskalScanner,
        ThermalShield,
        NuclearMissile,
        Laser,
        AutoPilot,

    }





    /// <summary>
    /// Gives an extra live to the player
    /// </summary>
    internal sealed class CExtraLifeGem : CGemSprite
    {
        #region ctor
        internal CExtraLifeGem(CServiceLocatorNode aParent) : base(aParent)
        {
            this.GetCategoryEnum = CGemCategoryEnum.Defense;
            this.Init();
        }
        #endregion
    }

    /// <summary>
    /// Repairs the shell.
    /// </summary>
    internal sealed class CShellRepairGem : CGemSprite
    {
        #region ctor
        internal CShellRepairGem(CServiceLocatorNode aParent) : base(aParent) 
        {
            this.GetCategoryEnum = CGemCategoryEnum.Defense;
            this.Init();
        }
        #endregion
    }

    /// <summary>
    /// Add shield energy
    /// </summary>
    internal sealed class CShieldGem : CGemSprite
    {
        #region ctor
        internal CShieldGem(CServiceLocatorNode aParent) : base(aParent) 
        {
            this.GetCategoryEnum = CGemCategoryEnum.Defense;
            this.Init();
        }
        #endregion
    }

    internal sealed class CLaserEnergyGem  : CGemSprite
    {
        #region ctor
        internal CLaserEnergyGem(CServiceLocatorNode aParent) : base(aParent) 
        {
            this.GetCategoryEnum = CGemCategoryEnum.Offense;
            this.Init();
        }
        #endregion
    }

    /// <summary>
    /// Required for destroing suns
    /// </summary>
    internal sealed class CNuclearMissileGem : CGemSprite
    {
        #region ctor
        internal CNuclearMissileGem(CServiceLocatorNode aParent) : base(aParent) 
        {
            this.GetCategoryEnum = CGemCategoryEnum.Offense;
            this.Init();
        }
        #endregion
    }

    /// <summary>
    /// Required for landing on a planet.
    /// </summary>
    internal sealed class CThermalShieldGem : CGemSprite
    {
        #region ctor
        internal CThermalShieldGem(CServiceLocatorNode aParent) : base(aParent) 
        {
            this.GetCategoryEnum = CGemCategoryEnum.Navigation;
        }
        #endregion
    }

    /// <summary>
    /// Required for finding wormholes.Primarly retrieved by destroying suns.
    /// </summary>
    internal sealed class CKruskalScannerGem : CGemSprite
    {
        #region ctor
        internal CKruskalScannerGem(CServiceLocatorNode aParent) : base(aParent)
        {
            this.GetCategoryEnum = CGemCategoryEnum.Navigation;
            this.Init();
        }
        #endregion
      
    }


    /// <summary>
    /// Zum Bohren in planeten benötigt.
    /// </summary>
    internal sealed class CDrillEnergyGem : CGemSprite
    {
        #region ctor
        internal CDrillEnergyGem(CServiceLocatorNode aParent) : base(aParent)
        {
            this.GetCategoryEnum = CGemCategoryEnum.Defense;
            this.Init();
        }
        #endregion
    }

    internal sealed class CAmmoSpeedGem : CGemSprite
    {
        #region ctor
        internal CAmmoSpeedGem(CServiceLocatorNode aParent) : base(aParent)
        {
            this.GetCategoryEnum = CGemCategoryEnum.Defense;
            this.Init();
        }
        #endregion
    }
    internal sealed class CFireRateGem : CGemSprite
    {
        #region ctor
        internal CFireRateGem(CServiceLocatorNode aParent) : base(aParent)
        {
            this.GetCategoryEnum = CGemCategoryEnum.Defense;
            this.Init(); 
        }
        #endregion

    }

    internal sealed class CSlowMotionGem :CGemSprite
    {
        #region ctor
        internal CSlowMotionGem(CServiceLocatorNode aParent) : base(aParent)
        {
            this.GetCategoryEnum = CGemCategoryEnum.Navigation;
            this.Init();
        }
        #endregion 
    }

    internal sealed class CAntigravityGem : CGemSprite
    {
        #region ctor
        internal CAntigravityGem(CServiceLocatorNode aParent) : base(aParent)
        {
            this.GetCategoryEnum = CGemCategoryEnum.Navigation;
            this.Init();
        }
        #endregion
    }

    //internal sealed class CGemManager :  CServiceLocatorNode
    //{
    //    #region ctor
    //    internal CGemManager(CServiceLocatorNode aParent) : base(aParent) { }
    //    #endregion
    //}

    internal sealed class CGuidedMissileGem : CGemSprite
    {
        internal CGuidedMissileGem(CServiceLocatorNode aParent):base(aParent)
        {
            this.GetCategoryEnum = CGemCategoryEnum.Offense;
            this.Init();
        }
    }

    internal sealed class CSpaceGripGem : CGemSprite
    {
        internal CSpaceGripGem(CServiceLocatorNode aParent) : base(aParent)
        {
            this.GetCategoryEnum = CGemCategoryEnum.Navigation;
            this.Init();
        }
    }


}
