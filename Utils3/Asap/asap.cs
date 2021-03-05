using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils3.Asap
{
    public delegate CReuseable CNewFunc();

    public abstract class CObjectPool 
    {
        protected CObjectPool()
        {
        }
        #region Factory
        public CNewFunc NewFunc { get; set; }
        protected virtual CReuseable New()
            => this.NewFunc();
        #endregion
    }

    public class CSimpleObjectPool : CObjectPool
    {
        #region ctor
        public CSimpleObjectPool()
        {
        }
        #endregion
        #region Reusable
        public bool Locked;
        private readonly List<CReuseable> Free = new List<CReuseable>();
        public CReuseable Allocate()
        {
            CReuseable aReusable;
            var aList = this.Free;
            lock(aList)
            {                
                if(aList.Count > 0)
                {
                    aReusable = aList[0];
                    aList.RemoveAt(0);
                }
                else  if(this.Locked)
                {
                    throw new OutOfMemoryException();
                }
                else
                {
                    aReusable = this.BuildNew();
                }
            }
            aReusable.BeginUse();
            return aReusable;
        }
        internal CReuseable BuildNew()
        {
            var aNew = this.New();
            aNew.SimpleObjectPool = this;
            return aNew;
        }
        internal void Deallocate(CReuseable aReusable)
        {
            var aList = this.Free;
            lock (aList)
            {
                aList.Add(aReusable);
            }
        }

        internal void Reserve(int aCount)
        {
            var aList = this.Free;
            for (var i = 0; i < aCount; ++i)
            {
                var aItem = this.BuildNew();
                lock (aList)
                {
                    aList.Add(aItem);
                }
            }
        }
        #endregion
    }

    public abstract class CMultiPoolItem : CSimpleObjectPool
    {
        protected CMultiPoolItem()
        {
        }

        public CMultiObjectPool MultiObjectPool { get; internal set; }

    }

    internal sealed class CDefaultMultiPoolItem : CMultiPoolItem
    {
        public CDefaultMultiPoolItem()
        {
        }
    }

    public abstract class CMultiObjectPool : CObjectPool
    {
        #region ctor
        protected CMultiObjectPool()
        {

        }
        protected void AllocateObjectPool(int c)
            => this.AllocateObjectPool(c, i => new CDefaultMultiPoolItem());
        protected void AllocateObjectPool(int c, Func<int, CMultiPoolItem> aNewFunc)
        {
            var aMultiPoolItems = new CMultiPoolItem[c];
            for (var i = 0; i < c; ++i)
            {
                var aMultiPoolItem = aNewFunc(i);
                aMultiPoolItem.MultiObjectPool = this;
                aMultiPoolItems[i] = aMultiPoolItem;
            }
            this.MultiPoolItems = aMultiPoolItems; 
        }
        #endregion
        #region MultiPoolItems
        private CMultiPoolItem[] MultiPoolItems;
        #endregion
        #region Reusable
        public void SetNewFunc(int i, CNewFunc aNewFunc)
            => this.MultiPoolItems[i].NewFunc = aNewFunc;
        protected CReuseable Allocate(int aPoolIdx)
            => this.MultiPoolItems[aPoolIdx].Allocate();
        #endregion
    }


    public abstract class CReuseable : CServiceLocatorNode
    {
        protected CReuseable(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        internal CSimpleObjectPool SimpleObjectPool { set; get; }
        public bool IsInUse { get; private set; }
        protected virtual void OnBeginUse()
        {
        }
        internal void BeginUse()
        {
            if(this.IsInUse)
            {
                throw new InvalidOperationException();
            }
            else
            {
                this.OnBeginUse();
                this.IsInUse = true;
            }
        }
        protected virtual void OnEndUse()
        {
        }
        internal void EndUse()
        {
            if (this.IsInUse)
            {
                this.OnEndUse();
                this.IsInUse = false;
            }
            else
            {
                // TODO: throw new InvalidOperationException();
            }
        }
        public void Deallocate()
        {
            if (this.IsInUse)
            {
                this.EndUse();
                this.SimpleObjectPool.Deallocate(this);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }

    public sealed class CObjectPool<T>  where T: CReuseable
    {
        public CObjectPool()
        {
            this.SimpleObjectPool = new CSimpleObjectPool();
            this.SimpleObjectPool.NewFunc = new CNewFunc(this.New);
        }
        private readonly CSimpleObjectPool SimpleObjectPool;
        public void Reserve(int aCount)
            => this.SimpleObjectPool.Reserve(aCount);
        public bool Locked { get => this.SimpleObjectPool.Locked; set => this.SimpleObjectPool.Locked = value; }
        public T Allocate()
            => (T)this.SimpleObjectPool.Allocate();

        public Func<T> NewFunc { get; set; }
        private CReuseable New() => this.NewFunc();
    }
    public static class CEntensions
    {
        public static void DeallocateItems<T>(this T[] a) where T:CReuseable
        {
            if (a is object)
            {
                for (var i = 0; i < a.Length; ++i)
                {
                    if (a[i] is object)
                    {
                        a[i].Deallocate();
                        a[i] = default;
                    }
                }
            }
        }
    }
}
