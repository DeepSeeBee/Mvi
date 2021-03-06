using CharlyBeck.Mvi.Story.Bonus;
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
namespace CharlyBeck.Mvi.Story.Bonus.Gems
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
    internal sealed class CExtraLifeGem : CGem
    {
        #region ctor
        internal CExtraLifeGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override CGemClassEnum GemClassEnum => CGemClassEnum.Defense;
    }

    /// <summary>
    /// Repairs the shell.
    /// </summary>
    internal sealed class CShellRepair : CGem
    {
        #region ctor
        internal CShellRepair(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override CGemClassEnum GemClassEnum => CGemClassEnum.Defense;
    }

    /// <summary>
    /// Add shield energy
    /// </summary>
    internal sealed class CShieldGem : CGem
    {
        #region ctor
        internal CShieldGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override CGemClassEnum GemClassEnum => CGemClassEnum.Defense;
    }

    internal sealed class CLaserEnergyGem  : CGem
    {
        #region ctor
        internal CLaserEnergyGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override CGemClassEnum GemClassEnum => CGemClassEnum.Offense;
    }

    /// <summary>
    /// Required for destroing suns
    /// </summary>
    internal sealed class CNuclearMissileGem : CGem
    {
        #region ctor
        internal CNuclearMissileGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override CGemClassEnum GemClassEnum => CGemClassEnum.Offense;
    }

    /// <summary>
    /// Required for landing on a planet.
    /// </summary>
    internal sealed class CThermalShieldGem : CGem
    {
        #region ctor
        internal CThermalShieldGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override CGemClassEnum GemClassEnum => CGemClassEnum.Navigation;
    }

    /// <summary>
    /// Required for finding wormholes.Primarly retrieved by destroying suns.
    /// </summary>
    internal sealed class CKruskalScannerGem : CGem
    {
        #region ctor
        internal CKruskalScannerGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override CGemClassEnum GemClassEnum => CGemClassEnum.Navigation;
    }


    /// <summary>
    /// Zum Bohren in planeten benötigt.
    /// </summary>
    internal sealed class CDrillEnergyGem : CGem
    {
        #region ctor
        internal CDrillEnergyGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override CGemClassEnum GemClassEnum => CGemClassEnum.Defense;
    }

    internal sealed class CAmmoSpeedGem : CGem
    {
        #region ctor
        internal CAmmoSpeedGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override CGemClassEnum GemClassEnum => CGemClassEnum.Defense;
    }
    internal sealed class CFireRateGem : CGem
    {
        #region ctor
        internal CFireRateGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override CGemClassEnum GemClassEnum => CGemClassEnum.Defense;
    }

    internal sealed class CSlowMotionGem :CGem
    {
        #region ctor
        internal CSlowMotionGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override CGemClassEnum GemClassEnum => CGemClassEnum.Navigation;
    }

    internal sealed class CAntigravityGem : CGem
    {
        #region ctor
        internal CAntigravityGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override CGemClassEnum GemClassEnum => CGemClassEnum.Navigation;
    }

    internal sealed class CGemManager :  CServiceLocatorNode
    {
        #region ctor
        internal CGemManager(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion


    }
    /// <summary>
    /// Item to collect, grants special abilites.
    /// Prototype Pattern
    /// </summary>
    internal abstract class CGem : CServiceLocatorNode
    {
        #region ctor
        internal CGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal virtual CValue TargetValue => throw new NotImplementedException();
        internal virtual bool ModifyTargetValueIsEnabled => false;
        internal virtual CValue SourceValue => throw new NotImplementedException();
        internal abstract CGemClassEnum GemClassEnum { get; }
        internal void Collect()
        {
            if (this.ModifyTargetValueIsEnabled)
            {
                this.TargetValue.Add(this.SourceValue);
            }
        }

    }
}
