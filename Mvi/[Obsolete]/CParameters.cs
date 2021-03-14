using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Story.Parameters
{
    internal enum CParameterEnum
    {
        // TOOD: Alle parameter aus world sammeln.
    }
    internal sealed class CParameters
    {
        #region ctor
        internal CParameters(CServiceLocatorNode aParent) : base(aParent) { }
        #endregion
    }
}
