using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Internal
{
    public abstract class CBase : CServiceLocatorNode
    {
        internal CBase(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        internal CBase(CNoParentEnum aNoParent) : base(aNoParent)
        {
        }
    }

    internal sealed class CRootBase : CBase
    {
        internal CRootBase(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Init();
        }

        internal CRootBase(CNoParentEnum aNoParent) : base(aNoParent)
        {
            this.Init();
        }

        public override T Throw<T>(Exception aException)
           => throw aException;
    }

}
