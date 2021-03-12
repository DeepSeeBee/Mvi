using CharlyBeck.Mvi.Facade;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Value
{
    public enum CValueTypeEnum
    {
        None,
        Bool,
        Double,
        Enum,
    }

    public enum CGuiEnum
    {
        Checkbox,
        Textbox,
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
        AccumulativeViewMatrix,
        Origin,
        FullScreen,
        LandingMode,
        Gravitation,
        Mouse_Xna,
        SolarSystem_SunVisible,
        SolarSystem_PlanetVisible,
        SolarSystem_MoonVisible,
        SolarSystem_Orbits,
        SolarSystem_Chaos,
        GridLines,
        SolarSystem_OrbitPlaneSlope,
        SolarSystem_Animate,
        Joystick,
        Mouse_WinForm,
        Look_Up,
        Look_Down,
        Look_Left,
        Look_Right,
        Look_Angle,
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

    public sealed class CBoolValDecl : CValueDeclaration<bool>
    {
        public CBoolValDecl(CValueEnum aValueEnum, Guid aGuid, bool aIsPersistent, bool aDefaultValue) : base(aValueEnum, aGuid, aIsPersistent,  CGuiEnum.Checkbox, CUnitEnum.Bool, aDefaultValue)
        {
        }
        internal override CValueTypeEnum ValueTypeEnum => CValueTypeEnum.Bool;
        internal override CValue NewValue(CServiceLocatorNode aParent)
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

        protected void SetValue(Action aSetValueAction)
        {
            this.ServiceContainer.GetService<CAddInGameThreadAction>()(aSetValueAction);
        }

        public abstract object VmValue 
        {
            get;
            set; 
        }
    }

    public abstract class CValue<T> : CValue
    {
        public CValue(CServiceLocatorNode aParent, CValueDeclaration<T> aValueDeclaration) : base(aParent, aValueDeclaration)
        {
            this.Value = aValueDeclaration.DefaultValue;
        }

        private T ValueM;
        public T Value
        {
            get => this.ValueM;
            set
            {
                if (!object.Equals(this.ValueM, value))
                {
                    this.ValueM = value;
                    this.NotifyChange(nameof(this.VmValue));
                }
            }       
        }
        public override object VmValue 
        { 
            get => this.ConvertGuiValue(this.Value);
            set
            {
                var aValue = this.ConvertGuiValueBack(value);
                this.SetValue(delegate () { this.Value = aValue; });
            }
        }
        protected T ConvertGuiValueBack(object aGuiValue) => (T)aGuiValue;
        protected object ConvertGuiValue(T aValue) => aValue;
    }

    public sealed class CBoolValue : CValue<bool>
    {
        public CBoolValue(CServiceLocatorNode aParent, CBoolValDecl aValueDeclaration) : base(aParent, aValueDeclaration)
        {
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

    public sealed class CDoubleValue : CValue<double>
    {
        public CDoubleValue(CServiceLocatorNode aParent, CDoubleDeclaration aValueDeclaration) : base(aParent, aValueDeclaration)
        {
        }
    }

    public sealed class CValues 
    : 
        CServiceLocatorNode
    ,   IEnumerable<CValue>
    {
        #region ctor
        public CValues(CServiceLocatorNode aParent) : base(aParent)
        {

        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
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
    }
}
