using CharlyBeck.Mvi.Facade;
using CharlyBeck.Mvi.Sprites.Quadrant;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Feature
{
    public sealed class CFeatureDeclarationAttribute : Attribute
    {
        public CFeatureDeclarationAttribute ()
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

    public sealed class CFeatureDeclaration 
    {
        public CFeatureDeclaration(Guid aGuid, string aName)
        {
            this.Guid = aGuid;
            this.Name = aName;
        }

        public string Name { get; private set; }
        public object VmName => this.Name;
        internal readonly Guid Guid;
    }

    public sealed class CFeature :  CChangeNotifier
    {
        public CFeature(CServiceLocatorNode aServiceLocatorNode, CFeatureDeclaration aFeatureDeclaration) :base(aServiceLocatorNode)
        {
            this.FeatureDeclaration = aFeatureDeclaration;
            this.Enabled = true;
        }
        public static CFeature Get(CServiceLocatorNode aParent, CFeatureDeclaration aFeatureDeclaration)
            => aParent.ServiceContainer.GetService<CFeatures>().GetFeature(aFeatureDeclaration);
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();

        internal readonly CFeatureDeclaration FeatureDeclaration;
        public object VmFeatureDeclaration => this.FeatureDeclaration;

        private bool EnabledM;
        public bool Enabled
        {
            get => this.EnabledM;
            set
            {
                if (this.EnabledM != value)
                {
                    this.EnabledM = value;
                    //this.NotifyChange(nameof(this.Enabled));
                }
            }
        }
        private bool? VmEnabledM;
        public bool VmEnabled 
        {
            get => CLazyLoad.Get(ref this.VmEnabledM, () => this.Enabled); 
            set 
            {
                this.VmEnabledM = value;
                this.ServiceContainer.GetService<CAddInGameTheradAction>()(delegate () { this.Enabled = (bool)value; });
            }
        }
    }


    public sealed class CFeatures 
    : 
        CServiceLocatorNode
    ,   IEnumerable<CFeature>
    {
        #region ctor
        public CFeatures(CServiceLocatorNode aParent) : base(aParent)
        {

        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        #endregion
        #region Features


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
            var aIsFeature = aFieldInfo.IsDefined(typeof(CFeatureDeclarationAttribute), true);
            if(aIsFeature)
            {
                var aAttribute = aFieldInfo.GetCustomAttribute<CFeatureDeclarationAttribute>();
                var aFeatureDeclaration = (CFeatureDeclaration)aFieldInfo.GetValue(default);
                this.Add(aFeatureDeclaration);
            }
        }

        public void Add(CFeatureDeclaration aFeatureDeclaation)
            => this.Add(new CFeature(this, aFeatureDeclaation));
        public void Add(CFeature aFeature)
            => this.FeatureDic.Add(aFeature.FeatureDeclaration, aFeature);

        private Dictionary<CFeatureDeclaration, CFeature> FeatureDic = new Dictionary<CFeatureDeclaration, CFeature>();
        public IEnumerator<CFeature> GetEnumerator() => this.FeatureDic.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        public CFeature GetFeature(CFeatureDeclaration aFeatureDeclaration)
            => this.FeatureDic[aFeatureDeclaration];
        #endregion
    }
}
