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
namespace CharlyBeck.Mvi.Story.Gems
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

    internal enum CGemEnum
    {
        ExtraLifeGem,
        ShellRepair,
        ShieldGem,
        AutoPilot,
        LaserEnergyGem,
        NuclearMissileGem,
        ThermalShieldGem,
        KruskalScannerGem,
        FuelGem,
        QuestGem,
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
        internal virtual CValue TargetValue => this.Throw<CValue>(new NotImplementedException());
        internal virtual bool ModifyTargetValueIsEnabled => false;
        internal virtual CValue SourceValue => this.Throw<CValue>(new NotImplementedException());
        internal void Collect()
        {
            if(this.ModifyTargetValueIsEnabled)
            {
                this.TargetValue.Add(this.SourceValue);
            }
        }

    }

    /// <summary>
    /// Gives an extra live to the player
    /// </summary>
    internal sealed class CExtraLifeGem : CGem
    {
        #region ctor
        internal CExtraLifeGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
    }

    /// <summary>
    /// Repairs the shell.
    /// </summary>
    internal sealed class CShellRepair : CGem
    {
        #region ctor
        internal CShellRepair(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
    }

    /// <summary>
    /// Add shield energy
    /// </summary>
    internal sealed class CShieldGem : CGem
    {
        #region ctor
        internal CShieldGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
    }

    /// <summary>
    /// Eases staying on an orbit/landing on planets. 
    /// Is activated while holding a button,
    /// decreases energy of autopilot.
    /// </summary>
    internal abstract class CAutoPilotGem : CGem
    {
        #region ctor
        internal CAutoPilotGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
    }

    /// <summary>
    /// Munition. Standard laser allows to decrease integrity of moons and planets.
    /// </summary>
    internal abstract class CAmmoGem : CGem
    {
        #region ctor
        internal CAmmoGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
    }

    internal sealed class CLaserEnergyGem  : CGem
    {
        #region ctor
        internal CLaserEnergyGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
    }

    /// <summary>
    /// Required for destroing suns
    /// </summary>
    internal sealed class CNuclearMissileGem : CAmmoGem
    {
        #region ctor
        internal CNuclearMissileGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
    }

    /// <summary>
    /// Required for landing on a planet.
    /// </summary>
    internal sealed class CThermalShieldGem : CGem
    {
        #region ctor
        internal CThermalShieldGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
    }

    /// <summary>
    /// Required for finding wormholes.Primarly retrieved by destroying suns.
    /// </summary>
    internal sealed class CKruskalScannerGem : CGem
    {
        #region ctor
        internal CKruskalScannerGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
    }

    internal sealed class CFuelGem : CGem
    {
        #region ctor
        internal CFuelGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
    }

    /// <summary>
    /// Starts a Quest. 
    /// </summary>
    internal sealed class CQuestGem : CGem
    {
        #region ctor
        internal CQuestGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
    }

    /// <summary>
    /// Zum Bohren in planeten benötigt.
    /// </summary>
    internal sealed class CDrillEnergyGem : CGem
    {
        #region ctor
        internal CDrillEnergyGem(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
    }

    /// <summary>
    /// Shows direction to fly to collect a focused Gem
    /// 
    /// </summary>
    internal sealed class CGemDetector : CGem
    {
        #region ctor
        internal CGemDetector(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
    }


    internal sealed class CGemManager :  CServiceLocatorNode
    {
        #region ctor
        internal CGemManager(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion

        internal void AddGem(CVector3Dbl aPos, 
                             CPropability<int> aCounts, 
                             CPropability<CGemEnum> aGemEnums)
        {
            throw new NotImplementedException();
        }


    }

}
