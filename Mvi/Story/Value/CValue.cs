using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Story.Value
{
    enum CAvatarValueEnum
    {
        Lifes,
        Shell,
        Shield,
        AutoPilot,
        LaserEnergy,
        NuclearMisileCount,
        ThermalShield,
        KruskalScanner,
        Fuel,
        DrillEnergy,
        GemDetetorPower,
    }

    internal abstract class CValue:CServiceLocatorNode
    {
        #region ctor
        internal CValue(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion

        internal abstract void Add(CValue aValue);

        internal abstract void AcceptAdd(CValue aValue);
        internal abstract void VisitAdd(CDoubleValue aDoubleValue);

    }


    internal abstract class CValue<T> : CValue
    {
        #region ctor
        internal CValue(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        public T Value { get; set; }
    }

    internal abstract class CDoubleValue :CValue<double>
    {
        #region ctor
        internal CDoubleValue(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override void AcceptAdd(CValue aValue)
            => aValue.VisitAdd(this);
        internal override void VisitAdd(CDoubleValue aDoubleValue)
            => this.Value = this.Value + aDoubleValue.Value;

    }

    internal sealed class CDefaultDoubleValue  : CDoubleValue
    {
        #region ctor
        internal CDefaultDoubleValue(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
        internal override void Add(CValue aValue)
            => aValue.AcceptAdd(this);

    }


    internal abstract class CValueHolder :CServiceLocatorNode
    {
        #region ctor
        internal CValueHolder(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
    }

    internal sealed class CPlayerValueHolder : CServiceLocatorNode
    {
        #region ctor
        internal CPlayerValueHolder(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion


    }

}
