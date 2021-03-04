using CharlyBeck.Mvi.Feature;
using CharlyBeck.Mvi.Mono.Input.Hid;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Mono.GameViewModel
{
    internal sealed class CGameVm : CChangeNotifier
    {
        #region ctor
        internal CGameVm(CServiceLocatorNode aParent):base(aParent)
        {
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        #endregion
        #region HidDevicesVm
        private CHidDevicesVm HidDevicesVmM;
        private CHidDevicesVm HidDevicesVm => CLazyLoad.Get(ref this.HidDevicesVmM, () => new CHidDevicesVm(this));
        #endregion
        #region ServiceLocator
        internal void RegisterComponentServices(CServiceContainer aServiceContainer)
        {
            aServiceContainer.AddService<CHidDevicesVm>(() => this.HidDevicesVm);
        }
        #endregion

    }
}
