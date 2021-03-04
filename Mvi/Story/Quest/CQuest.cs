using CharlyBeck.Mvi.Story.Gems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Quest
{

    // - Land on a planet
    // - Destroy X planets
    // - Destroy X Moons
    // - Escape trhough wormhole


    // Items: ( Tauchen (vermehrt) auf, wenn man Umlaufbahnen folgt)
    // - Orbit Auto Pilot
    // - ExtraLife
    // - Ammo
    // - 


    internal abstract class CQuest: CServiceLocatorNode
    {
        #region ctor
        internal CQuest(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal abstract double Propability{get;}

        internal virtual double GetRewardPropability(CKruskalScannerGem aKruskalScannerGem) => 0d; // TOOD: Parameter
        internal virtual double GetRewardPropability(CThermalShieldGem aThermalShieldGem) => 0d; // TOOD: Parameter

    }

    internal sealed class CLandOnPlanetQuest : CQuest
    {
        #region ctor
        internal CLandOnPlanetQuest(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override double Propability => throw new NotImplementedException();
    }

    internal abstract class CDestroyOrbsQuest : CQuest
    {
        #region ctor
        internal CDestroyOrbsQuest(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
    }

    internal sealed class CDestroyPlanetsQuest : CQuest
    {
        #region ctor
        internal CDestroyPlanetsQuest(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override double Propability => throw new NotImplementedException();
    }

    internal sealed class CDestroyMoonQuest : CQuest
    {
        #region ctor
        internal CDestroyMoonQuest(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override double Propability => throw new NotImplementedException();
    }

    internal sealed class CWormholeEscapeQuest : CQuest
    {
        #region ctor
        internal CWormholeEscapeQuest(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override double Propability => throw new NotImplementedException();
    }

    internal sealed class CQuestManager
    {
        #region ctor
        internal CQuestManager(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
    }
}
