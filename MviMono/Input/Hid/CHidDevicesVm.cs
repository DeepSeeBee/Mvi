using CharlyBeck.Mvi.Feature; // CChangeNotifier
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Mono.Input.Hid
{


    public abstract class CJoystick : CServiceLocatorNode
    {
        #region ctor
        internal CJoystick(CServiceLocatorNode aParent) :base(aParent)
        {
            this.HidDevicesVm = this.ServiceContainer.GetService<CHidDevicesVm>();
            this.NullDeviceVm = new CHidNullDeviceVm(this);
        }
        #endregion
        #region HidDevicesVm
        private readonly  CHidDevicesVm HidDevicesVm;
        #endregion
        #region HidDevice
        private CHidDeviceVm NullDeviceVm;
        private CHidDeviceVm HidDeviceVm
        {
            get
            {
                var aItem = this.HidDevicesVm.ItemVm;
                if (aItem is object)
                    return aItem;
                return this.NullDeviceVm;
            }
        }
        protected byte GetByte(int aByteIndex)
        {
            var aHidDeviceVm = this.HidDeviceVm;
            var aByte = aHidDeviceVm.GetDeviceState(aByteIndex);
            return aByte;
        }
        protected sbyte GetSByte(int aByteIndex)
            => (sbyte)this.GetByte(aByteIndex);


        private const double D_80 = 0x80;
        private const double D_FF = 0xff;

        protected double GetMByte(int aByteIndex)
        {
            var b = (double)this.GetByte(aByteIndex);
            if (b == D_80)
                return 0;
            else if (b < D_80)
                return (b - D_80) / D_80;
            else
                return (b - D_80+1) / D_80;
        }
            
        protected bool GetMaskedByte(int aByteIndex, byte aMask)
            => (this.GetByte(aByteIndex) & aMask) == aMask;
        protected double GetAxisByByteIndex(int aByteIndex)
            => GetAxisTolerance(this.GetMByte(aByteIndex));
           protected double GetAxisTolerance(double aAxis)
            => Math.Abs(aAxis) < this.AxisTolerance ? 0d : aAxis - (this.AxisTolerance * Math.Sign(aAxis));
        public double AxisTolerance = 0.2d;
        #endregion
        internal void BeginUpdate()
            => this.HidDeviceVm.BeginUpdate();
    }
    public sealed class CJoystick1  : CJoystick
    {
        #region ctor
        internal CJoystick1(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        #endregion
        #region Buttons
        public enum CButtonEnum
        {
            Side1,
            Side2,
            Side3,
            Side4,
            Side5,
            Side6,
            StickFront,
            StickSide,
            StickTop1,
            StickTop2,
            StickTop3,
            StickTop4
        }
        internal  bool IsButtonPressed(CButtonEnum aButton)
        {
            switch(aButton)
            {
                case CButtonEnum.Side1:
                    return this.GetMaskedByte(4, 0x40);
                case CButtonEnum.Side2:
                    return this.GetMaskedByte(4, 0x80);
                case CButtonEnum.Side3:
                    return this.GetMaskedByte(6, 0x1);
                case CButtonEnum.Side4:
                    return this.GetMaskedByte(6, 0x2);
                case CButtonEnum.Side5:
                    return this.GetMaskedByte(6, 0x4);
                case CButtonEnum.Side6:
                    return this.GetMaskedByte(6, 0x8);
                case CButtonEnum.StickFront:
                    return this.GetMaskedByte(4, 1);
                case CButtonEnum.StickSide:
                    return this.GetMaskedByte(4, 2);
                case CButtonEnum.StickTop1:
                    return this.GetMaskedByte(4, 0x10);
                case CButtonEnum.StickTop2:
                    return this.GetMaskedByte(4, 0x20);
                case CButtonEnum.StickTop3:
                    return this.GetMaskedByte(4, 0x4);
                case CButtonEnum.StickTop4:
                    return this.GetMaskedByte(4, 0x8);

                default:
                    return false;
            }
        }
        #endregion
        #region Axis
        internal enum CAxisEnum
        {
            X,
            Y,
            Z
        }
        internal double GetAxis(CAxisEnum aAxis)
        {
            switch (aAxis)
            {
                case CAxisEnum.X:
                    return this.GetAxisByByteIndex(0);
                case CAxisEnum.Y:
                    return this.GetAxisByByteIndex(1);
                case CAxisEnum.Z:
                    return this.GetAxisByByteIndex(3);
                default:
                    return 0.0d;
            }
        }

        internal double GetThroodle()
            => this.GetAxisByByteIndex(5);
        #endregion

    }

    internal abstract class CHidDeviceVm  : CChangeNotifier
    {
        #region ctor
        internal CHidDeviceVm(CServiceLocatorNode aParent) : base(aParent)
        {
        }

        #endregion
        #region DeviceState
        private volatile byte[] DeviceStateM;
        protected byte[] DeviceState { get => this.DeviceStateM; set => this.DeviceStateM = value; }
        internal byte GetDeviceState(int aIdx)
        {
            var aDeviceState = this.DeviceState;
            if (aDeviceState is object
            && aIdx < aDeviceState.Length)
                return aDeviceState[aIdx];
            return 0;
        }
        #endregion
        #region Active
        private bool ActiveM;
        public bool Active
        {
            get => this.ActiveM;
            set
            {
                lock (this)
                {
                    if (this.ActiveM != value)
                    {

                        if (this.ActiveM)
                        {
                            this.Deactivate();
                            this.ActiveM = false;
                        }
                        if (value)
                        {
                            this.Activate();
                            this.ActiveM = true;
                        }
                    }
                }
            }
        }
        protected abstract void Activate();
        protected abstract void Deactivate();
        internal abstract void BeginUpdate();
        #endregion
    }

    internal sealed class CHidNullDeviceVm : CHidDeviceVm
    {
        #region ctor
        internal CHidNullDeviceVm(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        #endregion
        #region Active
        protected override void Activate()
        {
        }
        protected override void Deactivate()
        {
        }
        internal override void BeginUpdate()
        {
        }
        #endregion

    }

    internal sealed class CHidValidDeviceVm  : CHidDeviceVm
    {
        #region ctor
        internal CHidValidDeviceVm(CServiceLocatorNode aParent, HidDevice aHidDevice) : base(aParent)
        {
            this.HidDevice = aHidDevice;
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        #endregion
        internal readonly HidDevice HidDevice;

        public override string ToString()
            => this.HidDevice.Description;

        #region Active
        protected override void Activate()
        {
            this.HidDevice.OpenDevice();
            this.BeginUpdate(true);
        }
        protected override void Deactivate()
        {
            this.HidDevice.CloseDevice();
        }
        #endregion
        #region Read
        private void ReadAsync()
        {
            this.HidDevice.ReadReport(this.OnReadResult);
        }
        private void OnReadResult(HidReport aReport)
        {
            this.DeviceState = aReport.Data;
            this.BeginUpdate(false);
        }
        internal void BeginUpdate(bool aIgnoreInactive)
        {
            lock (this)
            {
                if(aIgnoreInactive 
                || this.Active)
                {
                    this.ReadAsync();
                }
            }

        }
        internal override void BeginUpdate()
        {
            this.BeginUpdate(false);
        }
        #endregion
    }

    internal sealed class CHidDevicesVm : CChangeNotifier
    {
        #region ctor
        internal CHidDevicesVm(CServiceLocatorNode aParent) : base(aParent)
        {
            this.Init();
        }
        public CHidDevicesVm() :this(new CDefaultServiceLocatorNode())
        {
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        #endregion
        #region HidDeviceVms
        private IEnumerable<CHidDeviceVm> HidDeviceVmsM;
        private IEnumerable<CHidDeviceVm> HidDeviceVms
            => CLazyLoad.Get(ref this.HidDeviceVmsM, () => (from aHidDevice in this.HidDevices select new CHidValidDeviceVm(this, aHidDevice)).ToArray());
        public object VmItemVms
            => this.HidDeviceVms;
        #endregion
        #region HidDevices
        private IEnumerable<HidDevice> HidDevicesM;
        private IEnumerable<HidDevice> HidDevices=>CLazyLoad.Get(ref this.HidDevicesM, ()=> HidLibrary.HidDevices.Enumerate().ToArray());
        #endregion
        #region SelectedItem
        private CHidDeviceVm ItemVmM;
        internal CHidDeviceVm ItemVm
        {
            get => CLazyLoad.Get(ref this.ItemVmM, ()=>this.NewItemVm(true));
            set 
            {
                if (!this.ItemVmM.RefEquals<CHidDeviceVm>(value))
                {
                    if (this.ItemVmM is object)
                    { 
                        this.ItemVmM.Active = false;
                        this.ItemVmM = default;
                    }
                    if (value is object)
                    {
                        value.Active = true;
                        this.ItemVmM = value;
                    }
                    this.NotifyChange(nameof(this.VmItemVm));
                }
            }
        }
        private CHidDeviceVm NewItemVm(bool aActivate)
        {
            var aItem = (from aHidDeviceVms in this.HidDeviceVms
                where aHidDeviceVms.ToString() == "HID-konformer Gamecontroller"
                select aHidDeviceVms).FirstOrDefault();
            if(aItem is object
            && aActivate)
            {
                aItem.Active = true;
            }
            return aItem;
        }
        public object VmItemVm
        {
            get => this.ItemVm;
            set => this.ItemVm = (CHidDeviceVm)value;
        }
        #endregion
    }
}
