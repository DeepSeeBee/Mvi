using CharlyBeck.Mvi.Facade;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CharlyBeck.Utils3.Strings;

namespace CharlyBeck.Mvi.Value
{
    public enum CValueTypeEnum
    {
        None,
        Bool,
        Double,
        Enum,
        Int64,
    }

    public enum CGuiEnum
    {
        Checkbox,
        Textbox,
        NumericTextBox,
        Slider,
        ComboBox,
        Button,
    }

    public enum CUnitEnum
    {
        None,
        Radians,
        Percent,
        Count,
        Bool,
    }

    public enum CValueEnum
    {
        Global_AccumulativeViewMatrix,
        Global_Origin,
        Global_FullScreen,
        Global_LandingMode,
        Global_Gravitation,
        Global_Mouse_Xna,
        Global_SolarSystem_SunVisible,
        Global_SolarSystem_PlanetVisible,
        Global_SolarSystem_MoonVisible,
        Global_SolarSystem_Orbits,
        Global_SolarSystem_Chaos,
        Global_GridLines,
        Global_SolarSystem_OrbitPlaneSlope,
        Global_SolarSystem_Animate,
        Global_Joystick,
        Global_Mouse_WinForm,
        Global_Look_Up,
        Global_Look_Down,
        Global_Look_Left,
        Global_Look_Right,
        Global_Look_Angle,

        Object_Avatar_AmmoEnergy,
        Object_Avatar_AmmoFireRate,
        Object_Avatar_AmmoSpeed,
        Object_Avatar_AmmoThickness,
        Object_Avatar_AntiGravity,
        Object_Avatar_Drill,
        Object_Avatar_LifeCount,
        Object_Avatar_GuidedMissileCount,
        Object_Avatar_KruskalScannerCount,
        Object_Avatar_NuclearMissileCount,
        Object_Avatar_Shell,
        Object_Avatar_SlowMotion,
        Object_Avatar_SpaceGrip,
        Object_Avatar_ThermalShield,
    }

    public sealed class CMemberDeclarationAttribute : Attribute
    {
        public CMemberDeclarationAttribute ()
        {
        }
    }

    public abstract class CChangeNotifier
    : 
        CServiceLocatorNode
    ,   INotifyPropertyChanged
    {
        public CChangeNotifier(CServiceLocatorNode aParent)  : base(aParent)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyChange(string aPropertyName)
        {
            if(this.PropertyChanged is object)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(aPropertyName));
            }
        }
    }

    public abstract class CValueDeclaration 
    {
        public CValueDeclaration(CValueEnum aValueEnum, Guid aGuid, bool aIsPersistent, CGuiEnum aGuiEnum, CUnitEnum aUnitEnum)
        {
            this.ValueEnum = aValueEnum;
            this.Guid = aGuid;
            this.GuiEnum = aGuiEnum;
            this.UnitEnum = aUnitEnum;
        }

        internal CValueEnum ValueEnum;
        internal readonly CUnitEnum UnitEnum;
        public string Name => this.ValueEnum.ToString();
        public object VmName => this.Name;
        internal readonly Guid Guid;

        internal abstract CValueTypeEnum ValueTypeEnum { get; }
        public CGuiEnum GuiEnum { get; private set; }
        internal abstract CValue NewValue(CServiceLocatorNode aParent);
    }

    public sealed class CCommandDeclaration : CValueDeclaration
    {
        public CCommandDeclaration(CValueEnum aValueEnum, Guid aGuid) : base(aValueEnum, aGuid, false,  CGuiEnum.Button, CUnitEnum.None)
        {
        }
        internal override CValueTypeEnum ValueTypeEnum => CValueTypeEnum.None;
        internal override CValue NewValue(CServiceLocatorNode aParent)
            => new CCommand(aParent, this);
    }


    public sealed class CCommand : CValue
    {
        internal CCommand (CServiceLocatorNode aParent, CCommandDeclaration aCmdDeclaration) : base(aParent, aCmdDeclaration)
        {
            this.Init();
        }

        public event Action Invoked;
        public void Invoke()
        {
            var aGuiDispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            var aGameAction = this.ServiceContainer.GetService<CAddInGameThreadAction>();
            aGameAction(delegate ()
            {
                try
                {
                    if (this.Invoked is object)
                    {
                        this.Invoked();
                    }
                }
                catch (Exception aExc)
                {
                    aGuiDispatcher.BeginInvoke(new Action(delegate ()
                    {
                        System.Windows.MessageBox.Show(aExc.Message, this.ValueDeclaration.Name, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }));
                }
            });
        }

        public override object VmValue { get => throw new InvalidOperationException(); set => throw new InvalidOperationException(); } // TODO_OO
    }

    public abstract class CValueDeclaration<T>  :CValueDeclaration
    {
        internal CValueDeclaration(CValueEnum aValueEnum, Guid aGuid, bool aIsPersistent, CGuiEnum aGuiEnum, CUnitEnum aUnitEnum, T aDefaultValue) :base(aValueEnum, aGuid, aIsPersistent, aGuiEnum, aUnitEnum)
        {
            this.DefaultValue = aDefaultValue;
        }

        internal readonly T DefaultValue;
    }

    public sealed class CBoolDeclaration : CValueDeclaration<bool>
    {
        public CBoolDeclaration(CValueEnum aValueEnum, Guid aGuid, bool aIsPersistent, bool aDefaultValue) : base(aValueEnum, aGuid, aIsPersistent,  CGuiEnum.Checkbox, CUnitEnum.Bool, aDefaultValue)
        {
        }
        internal override CValueTypeEnum ValueTypeEnum => CValueTypeEnum.Bool;
        internal override CValue NewValue(CServiceLocatorNode aParent)
                => this.NewBoolValue(aParent);

        internal CBoolValue NewBoolValue(CServiceLocatorNode aParent)
            => new CBoolValue(aParent, this);
    }

    public abstract class CNumericValueDeclaration<T> : CValueDeclaration<T>
    {
        internal CNumericValueDeclaration(CValueEnum aValueEnum, Guid aGuid, bool aIsPersistent, CGuiEnum aGuiEnum, CUnitEnum aUnitEnum, T aDefaultValue, T aMin, T aMax, T aSmallChange, T aLargeChange)
            : base(aValueEnum, aGuid, aIsPersistent, aGuiEnum, aUnitEnum, aDefaultValue)
        {
            this.Minimum = aMin;
            this.Maximum = aMax;
            this.LargeChange = aLargeChange;
            this.SmallChange = aSmallChange;
        }
        internal readonly T Minimum;
        internal readonly T Maximum;
        internal readonly T LargeChange;
        internal readonly T SmallChange;
    }
    public sealed class CInt64Declaration : CNumericValueDeclaration<Int64>
    {
        public CInt64Declaration(CValueEnum aValueEnum,
                                    Guid aGuid,
                                    bool aIsPersistent,
                                    CUnitEnum aUnitEnum,
                                    Int64 aDefaultValue,
                                    Int64 aMin,
                                    Int64 aMax,
                                    Int64 aSmallChange,
                                    Int64 aLargeChange)
            :
            base(aValueEnum, aGuid, aIsPersistent, CGuiEnum.NumericTextBox, aUnitEnum, aDefaultValue, aMin, aMax, aSmallChange, aLargeChange)
        {
        }
        internal override CValueTypeEnum ValueTypeEnum => CValueTypeEnum.Int64;
        internal override CValue NewValue(CServiceLocatorNode aParent)
            => this.NewInt64Value(aParent);
        internal CInt64Value NewInt64Value(CServiceLocatorNode aParent)
            => new CInt64Value(aParent, this);


    }
    public sealed class CDoubleDeclaration : CNumericValueDeclaration<Double>
    {
        public CDoubleDeclaration(CValueEnum aValueEnum, 
                                           Guid aGuid, 
                                           bool aIsPersistent, 
                                           CGuiEnum aGuiEnum,
                                           CUnitEnum aUnitEnum,
                                           double aDefaultValue,
                                           double aMin,
                                           double aMax,
                                           double aSmallChange,
                                           double aLargeChange,
                                           int aDigits
                                            ) : base(aValueEnum, aGuid, aIsPersistent,  aGuiEnum, aUnitEnum, aDefaultValue, aMin, aMax, aSmallChange, aLargeChange)
        {
            this.Digits = aDigits;
        }
        internal readonly int Digits;
        internal override CValueTypeEnum ValueTypeEnum => CValueTypeEnum.Double;
        internal override CValue NewValue(CServiceLocatorNode aParent)
            => this.NewDoubleValue(aParent);
        internal CDoubleValue NewDoubleValue(CServiceLocatorNode aParent)
            => new CDoubleValue(aParent, this);
    }
    public abstract class CValue :  CChangeNotifier
    {
        public CValue(CServiceLocatorNode aServiceLocatorNode, CValueDeclaration aValueDeclaration) :base(aServiceLocatorNode)
        {
            this.ValueDeclaration = aValueDeclaration;
        }
        public static TValue GetStaticValue<TValue>(CServiceLocatorNode aParent, CValueDeclaration aValueDeclaration) where TValue : CValue
            => (TValue)(object)aParent.ServiceContainer.GetService<CValues>().GetValue(aValueDeclaration);

        internal readonly CValueDeclaration ValueDeclaration;
        public object VmValueDeclaration => this.ValueDeclaration;

        protected void SetValueInGuiThread(Action aSetValueAction)
        {
            this.ServiceContainer.GetService<CAddInGameThreadAction>()(aSetValueAction);
        }
        #region ViewModel
        public abstract object VmValue 
        {
            get;
            set; 
        }
        public object VmName => this.ValueDeclaration.Name.TrimStart(this.Values.NamePrefix);
        #endregion
        #region Values
        private CValues ValuesM;
        private CValues Values => CLazyLoad.Get(ref this.ValuesM, () => this.ServiceContainer.GetService<CValues>());
        #endregion
        #region Increment
        internal virtual void Increment(bool aDecrement, bool aLargeChange) { }
        #endregion
        #region ViewModel
        protected void InvokeFromGui(Action aInGameThreadAction, Action aInGuiAction = null)
        {
            var aGuiDispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            var aGameAction = this.ServiceContainer.GetService<CAddInGameThreadAction>();
            aGameAction(delegate ()
            {
                try
                {
                    aInGameThreadAction();
                }
                catch (Exception aExc)
                {
                    aGuiDispatcher.BeginInvoke(new Action(delegate ()
                    {
                        System.Windows.MessageBox.Show(aExc.Message, this.ValueDeclaration.Name, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }));
                }
                if(aInGuiAction is object)
                    aInGuiAction();
            });
        }

        public void VmIncrement()
        {
            this.InvokeFromGui(delegate () { this.Increment(false, false); });
        }
        public void VmDecrement()
        {
            this.InvokeFromGui(delegate () { this.Increment(true, false); });
        }
        #endregion



    }

    public abstract class CValue<T> : CValue
    {
        public CValue(CServiceLocatorNode aParent, CValueDeclaration<T> aValueDeclaration) : base(aParent, aValueDeclaration)
        {
            this.ValueDeclaration = aValueDeclaration;
        }

        internal readonly CValueDeclaration<T> ValueDeclaration;
        protected override void Init()
        {
            base.Init();
            this.Value = this.ValueDeclaration.DefaultValue;
        }

        private T ValueM;
        public T Value
        {
            get => this.ValueM;
            set
            {
                var aNewVal = this.Coerce(value);
                if (!object.Equals(this.ValueM, aNewVal))
                {
                    this.ValueM =  aNewVal;
                    this.NotifyChange(nameof(this.VmValue));
                }
            }       
        }
        protected virtual T Coerce(T aValue)
            => aValue;
        public override object VmValue 
        { 
            get => this.ConvertGuiValue(this.Value);
            set
            {
                var aValue = this.ConvertGuiValueBack(value);
                this.SetValueInGuiThread(delegate () { this.Value = aValue; });
            }
        }
        protected T ConvertGuiValueBack(object aGuiValue) => (T)aGuiValue;
        protected object ConvertGuiValue(T aValue) => aValue;
    }

    public sealed class CBoolValue : CValue<bool>
    {
        public CBoolValue(CServiceLocatorNode aParent, CBoolDeclaration aValueDeclaration) : base(aParent, aValueDeclaration)
        {
            this.Init();
        }
    }

    public abstract class CNumericVal<T> : CValue<T>
    {
        internal CNumericVal(CServiceLocatorNode aParent, CNumericValueDeclaration<T> aNumericValueDeclaration) : base(aParent, aNumericValueDeclaration)
        {
            this.NumericValueDeclaration = aNumericValueDeclaration;
        }
        internal readonly CNumericValueDeclaration<T> NumericValueDeclaration;
        public object VmMinmum => this.NumericValueDeclaration.Minimum;
        public object VmMaximum => this.NumericValueDeclaration.Maximum;
        public object VmSmallChange => this.NumericValueDeclaration.SmallChange;
        public object VmLargeChange => this.NumericValueDeclaration.LargeChange;

    }

    public sealed class CDoubleValue : CNumericVal<double>
    {
        public CDoubleValue(CServiceLocatorNode aParent, CDoubleDeclaration aValueDeclaration) : base(aParent, aValueDeclaration)
        {
            this.DoubleDeclaration = aValueDeclaration;
            this.Init();
        }
        internal readonly CDoubleDeclaration DoubleDeclaration;
        internal override void Increment(bool aDecrement, bool aLargeChange)
        {
            var aStep = aLargeChange ? this.DoubleDeclaration.LargeChange : this.DoubleDeclaration.SmallChange;
            var aDelta = aDecrement ? -aStep : aStep;
            this.Value += aDelta;
        }
        protected override double Coerce(double aValue)
            => Math.Max(this.DoubleDeclaration.Minimum, Math.Min(this.DoubleDeclaration.Maximum, aValue));
    }
    public sealed class CInt64Value : CValue<Int64>
    {
        public CInt64Value(CServiceLocatorNode aParent, CInt64Declaration aValueDeclaration) : base(aParent, aValueDeclaration)
        {
            this.Int64Declaration = aValueDeclaration;
            this.Init();
        }
        internal readonly CInt64Declaration Int64Declaration;
        internal override void Increment(bool aDecrement, bool aLargeChange)
        {
            var aStep = aLargeChange ? this.Int64Declaration.LargeChange : this.Int64Declaration.SmallChange;
            var aDelta = aDecrement ? -aStep : aStep;
            this.Value += aDelta;
        }
        protected override Int64 Coerce(Int64 aValue)
                => Math.Max(this.Int64Declaration.Minimum, Math.Min(this.Int64Declaration.Maximum, aValue));

    }
    public sealed class CDefaultValues
    :
        CValues
    {
        public CDefaultValues(CServiceLocatorNode aParent) : base(aParent)
        {
        }
    }

    public abstract class CValues 
    :
        CServiceLocatorNode
    ,   IEnumerable<CValue>
    {
        #region ctor
        public CValues(CServiceLocatorNode aParent) : base(aParent)
        {

        }
        #endregion
        #region Values
        public void AddRange(IEnumerable<Assembly> aAssemblies)
        {
            foreach (var aAssembly in aAssemblies)
                this.Add(aAssembly);
        }
        public void Add(Assembly aAssembly)
        {
            foreach(var aType in aAssembly.GetTypes())
            {
                this.Add(aType);
            }
        }
        public void Add(Type aType)
        {
            foreach(var aField in aType.GetFields(BindingFlags.Static | BindingFlags.NonPublic))
            {
                this.Add(aField);
            }
        }

        public void Add(FieldInfo aFieldInfo)
        {
            var aIsValue = aFieldInfo.IsDefined(typeof(CMemberDeclarationAttribute), true);
            if(aIsValue)
            {
                var aAttribute = aFieldInfo.GetCustomAttribute<CMemberDeclarationAttribute>();
                var aValueDeclaration = (CValueDeclaration)aFieldInfo.GetValue(default);
                this.Add(aValueDeclaration);
            }
        }

        public void Add(CValueDeclaration aValueDeclaation)
            => this.Add(aValueDeclaation.NewValue(this));
        public void Add(CValue aValue)
            => this.ValueDic.Add(aValue.ValueDeclaration, aValue);

        private Dictionary<CValueDeclaration, CValue> ValueDic = new Dictionary<CValueDeclaration, CValue>();
        public IEnumerator<CValue> GetEnumerator() => this.ValueDic.Values.OrderBy(aValue=>aValue.ValueDeclaration.Name).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        public CValue GetValue(CValueDeclaration aValueDeclaration)
            => this.ValueDic[aValueDeclaration];
        #endregion
        #region NamePrefix
        private string NamePrefixM;
        internal string NamePrefix => CLazyLoad.Get(ref this.NamePrefixM, this.NewNamePrefix);
        private string NewNamePrefix()
        {
            var aValues = this.ValueDic.Keys;
            var aEnums = aValues.Select(v => v.ValueEnum);
            var aTexts = aEnums.Select(e => e.ToString()).ToArray();
            if (aTexts.Length > 1)
            {
                var aLens = aTexts.Select(t => t.Length);
                var aLenMin = aLens.Min();
                for (var i = 1; i < aLenMin; ++i)
                {
                    var aRefText = aTexts[0];
                    var aPrefix = aRefText.Substring(0, i);
                    var âMatch = aTexts.Select(t => t.StartsWith(aPrefix));
                    var aDontMatch = âMatch.Contains(false);
                    if (aDontMatch)
                    {
                        return aRefText.Substring(0, i - 1);
                    }
                }
            }
            return string.Empty;


        }
        #endregion

    }

    internal abstract class CValueObject : CValues
    {
        #region ctor
        internal CValueObject(CServiceLocatorNode aParent) : base(aParent)
        {
            this.ValueObjectRegistry = this.ServiceContainer.GetService<CValueObjectRegistry>();
        }
        #endregion
        #region ValueObjectRegistry
        internal void Register()
        {
            this.ValueObjectRegistry.Add(this);
        }
        internal void Unregister()
        {
            this.ValueObjectRegistry.Remove(this);
        }
        private readonly CValueObjectRegistry ValueObjectRegistry;
        #endregion
        #region ServiceContainer
        private CServiceContainer ServiceContainerM;
        public override CServiceContainer ServiceContainer => CLazyLoad.Get(ref this.ServiceContainerM, this.NewServiceContainer);
        private CServiceContainer NewServiceContainer()
        {
            var aServiceContainer = base.ServiceContainer.Inherit(this);
            aServiceContainer.AddService<CValues>(() => this);
            return aServiceContainer;
        }
        #endregion
    }

    internal sealed class CValueObjectRegistry : CServiceLocatorNode 
    {
        internal CValueObjectRegistry(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        private readonly List<CValueObject> ValueObjects = new List<CValueObject>();
        internal void Add(CValueObject aValueObject)
        {
            this.ValueObjects.Add(aValueObject);
        }
        internal void Remove(CValueObject aValueObject)
        {
            this.ValueObjects.Remove(aValueObject);
        }
        public object VmItems => this.ValueObjects;
    }
}
