using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Utils3.ServiceLocator
{
    public abstract class CServiceLocatorNode 
    {
        public CServiceLocatorNode(CServiceLocatorNode aParent)
        {
            this.ParentServiceLocatorNodeNullable = aParent;
        }
        public CServiceLocatorNode(CNoParentEnum aNoParent)
        {
        }
        protected virtual void Init()
        {
            this.LoadServices();
        }
        protected virtual void LoadServices()
        {
        }
        public virtual void Load() { }
        public enum CNoParentEnum
        {
            NoParent
        }
        public const CNoParentEnum NoParent = CNoParentEnum.NoParent;

        public virtual T Throw<T>(Exception aException)
            => aException.Throw<T>();

        public void Throw(Exception aExc)
           => this.Throw<object>(aExc);

        public readonly CServiceLocatorNode ParentServiceLocatorNodeNullable;
        public CServiceLocatorNode ParentServiceLocatorNode
        {
            get
            {
                if (this.ParentServiceLocatorNodeNullable is object)
                    return this.ParentServiceLocatorNodeNullable;
                throw new InvalidOperationException();
            }
        }

        private CServiceContainer ServiceContainerM;
        public virtual CServiceContainer ServiceContainer
           => CLazyLoad.Get(ref this.ServiceContainerM, () =>
              (this.ParentServiceLocatorNodeNullable is object)
              ? this.ParentServiceLocatorNode.ServiceContainer.Inherit(this)
              : CServiceContainer.New(this, default));

        public IEnumerable<CServiceLocatorNode> ServiceLocatorPath
        {
            get
            {
                if (this.ParentServiceLocatorNodeNullable is object)
                    foreach (var aServiceLocator in this.ParentServiceLocatorNodeNullable.ServiceLocatorPath)
                        yield return aServiceLocator;
                yield return this;
            }
        }
        public string ServiceLocatorPathText => (from aServiceLocator in this.ServiceLocatorPath select ".(" + aServiceLocator.ToString() + ")").JoinString();

    }

    public sealed class CDefaultServiceLocatorNode : CServiceLocatorNode
    {
        public CDefaultServiceLocatorNode(CServiceLocatorNode aParent = null) : base(aParent)
        {
        }
        public override T Throw<T>(Exception aExc) => aExc.Throw<T>();
    }

    public sealed class CServiceContainer
    {
        private CServiceContainer(CServiceContainer aParentServiceContainerNullable, CServiceLocatorNode aOwner, Type aOwnerType)
        {
            this.ParentServiceContainerNullable = aParentServiceContainerNullable;
            this.Owner = aOwner;
            this.OwnerTypeNullable = aOwnerType;
        }
        public static CServiceContainer New<T>(T aOwner, CServiceContainer aParentServiceContainerNullable) where T : CServiceLocatorNode
           => new CServiceContainer(aParentServiceContainerNullable, aOwner, typeof(T));
        public CServiceContainer()
        {
        }

        public CServiceContainer Inherit<T>(T aOwner) where T : CServiceLocatorNode
           => New(aOwner, this);

        private readonly CServiceContainer ParentServiceContainerNullable;
        private readonly CServiceLocatorNode Owner;
        private readonly Type OwnerTypeNullable;
        private readonly Dictionary<object, Func<object>> LocalServices = new Dictionary<object, Func<object>>();
        public void AddService(object aKey, Func<object> aGetObjectFunc)
           => this.LocalServices.Add(aKey, aGetObjectFunc);
        public void AddService<T>(Func<T> aService)
           => this.AddService(typeof(T), () => aService());
        private Exception NewServiceNotAvailableExc(string aMsg, object aKey)
           => new Exception(aMsg + " Key='" + aKey.ToString() + "' " + "Context='" + this.Owner.ServiceLocatorPathText + "'");
        public object GetLocalService(object aKey)
        {
            if (this.LocalServices.ContainsKey(aKey))
                return this.LocalServices[aKey]();
            var aExc = this.NewServiceNotAvailableExc("Service not available locally.", aKey);
            this.Owner.Throw(aExc);
            throw aExc;
        }
        public bool LocalServiceIsDefined(object aKey)
           => this.LocalServices.ContainsKey(aKey);
        public bool LocalServiceIsDefined<T>()
           => this.LocalServiceIsDefined(typeof(T));
        public object GetContextService(object aKey)
           => this.GetContextService(this.Owner);
        private object GetContextServiceNullable(CServiceLocatorNode aRequestor, object aKey)
        {
            if (this.LocalServiceIsDefined(aKey))
                return this.GetLocalService(aKey);
            else if (this.ParentServiceContainerNullable is object)
                return this.ParentServiceContainerNullable.GetContextServiceNullable(aRequestor, aKey);
            else
                return default;
        }
        private object GetContextService(CServiceLocatorNode aRequestor, object aKey)
        {
            var aServiceNullable = this.GetContextServiceNullable(aRequestor, aKey);
            if (aServiceNullable is object)
                return aServiceNullable;
            var aExc = this.NewServiceNotAvailableExc("Service not available in context.", aKey);
            aRequestor.Throw(aExc);
            throw aExc;
        }

        public T GetContextService<T>()
           => (T)this.GetContextService(this.Owner, typeof(T));
        public T GetContextServiceNullable<T>()
           => (T)this.GetContextServiceNullable(this.Owner, typeof(T));
        public object GetContextServiceNullable(object aKey)
           => this.GetContextServiceNullable(this.Owner, aKey);
        public T GetService<T>()
           => this.GetContextService<T>();
        public object GetService(object aKey)
           => this.GetContextService(aKey);
        public object GetServiceNullable(object aKey)
           => this.GetContextServiceNullable(aKey);
        public T GetServiceNullable<T>()
           => (T)this.GetServiceNullable(typeof(T));


    }
}
