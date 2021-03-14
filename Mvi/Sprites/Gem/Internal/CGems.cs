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
        internal CExtraLifeGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.ExtraLife)
        {
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
        internal CShellRepairGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.ShellRepair) 
        {
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
        internal CShieldGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.Shield) 
        {
            this.Init();
        }
        #endregion
    }

    internal sealed class CAmmoEnergyGem  : CGemSprite
    {
        #region ctor
        internal CAmmoEnergyGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.AmmoEnergy) 
        {
            this.Init();
        }
        #endregion
    }
    internal sealed class CAmmoThicknessGem : CGemSprite
    {
        #region ctor
        internal CAmmoThicknessGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.AmmoThickness)
        {
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
        internal CNuclearMissileGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.NuclearMissile) 
        {
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
        internal CThermalShieldGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.ThermalShield) 
        {
        }
        #endregion
    }

    /// <summary>
    /// Required for finding wormholes.Primarly retrieved by destroying suns.
    /// </summary>
    internal sealed class CKruskalScannerGem : CGemSprite
    {
        #region ctor
        internal CKruskalScannerGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.KruskalScanner)
        {
            this.Init();
        }
        #endregion
      
    }


    /// <summary>
    /// Zum Bohren in planeten benötigt.
    /// </summary>
    internal sealed class CDrillGem : CGemSprite
    {
        #region ctor
        internal CDrillGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.Drill)
        {
            this.Init();
        }
        #endregion
    }

    internal sealed class CAmmoSpeedGem : CGemSprite
    {
        #region ctor
        internal CAmmoSpeedGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.AmmoSpeed)
        {
            this.Init();
        }
        #endregion
    }
    internal sealed class CFireRateGem : CGemSprite
    {
        #region ctor
        internal CFireRateGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.FireRate)
        {
            this.Init(); 
        }
        #endregion

    }

    internal sealed class CSlowMotionGem :CGemSprite
    {
        #region ctor
        internal CSlowMotionGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.SlowMotion)
        {
            this.Init();
        }
        #endregion 
    }

    internal sealed class CAntigravityGem : CGemSprite
    {
        #region ctor
        internal CAntigravityGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.AntiGravity)
        {
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
        internal CGuidedMissileGem(CServiceLocatorNode aParent):base(aParent, CGemEnum.GuidedMissile)
        {
            this.Init();
        }
    }

    internal sealed class CSpaceGripGem : CGemSprite
    {
        internal CSpaceGripGem(CServiceLocatorNode aParent) : base(aParent, CGemEnum.SpaceGrip)
        {
            this.Init();
        }
    }


}
