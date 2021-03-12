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
        public CNewFunc NewFunc { get; set; }

        public bool Locked;
        private readonly List<CReuseable> Free = new List<CReuseable>();
        public CReuseable Allocate()
            => this.Allocate(this.NewFunc);
        public CReuseable Allocate(CNewFunc aNewFunc)
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
                    aReusable = this.BuildNew(aNewFunc);
                }
            }
            aReusable.BeginUse();
            return aReusable;
        }
        internal CReuseable BuildNew(CNewFunc aNewFunc)
        {
            var aNew = aNewFunc();
            aNew.SimpleObjectPool = this;
            return aNew;
        }
        internal void Deallocate(CReuseable aReusable)
        { 
            if(!aReusable.IsInUse)
            {
                throw new InvalidOperationException();
            }
            else if(aReusable.DeallocateIsPending)
            {
                throw new InvalidOperationException();
            }
            else
            {
                aReusable.IsInUse = false;
                aReusable.DeallocateIsPending = true;
                try
                {
                    this.OnDeallocating(aReusable);
                }
                finally
                {
                    aReusable.DeallocateIsPending = false;
                }
                var aList = this.Free;
                lock (aList)
                {
                    aList.Add(aReusable);
                }

            }
        }
        internal Action<CReuseable> Deallocating;
        internal virtual void OnDeallocating(CReuseable aReuseable)
        {
            if (this.Deallocating is object)
                this.Deallocating(aReuseable);
        }

        internal void Reserve(int aCount)
        {
            var aList = this.Free;
            for (var i = 0; i < aCount; ++i)
            {
                var aItem = this.BuildNew(this.NewFunc);
                lock (aList)
                {
                    aList.Add(aItem);
                }
            }
        }
        #endregion
    }





    public abstract class CMultiObjectPool : CObjectPool
    {
        #region ctor
        protected CMultiObjectPool()
        {

        }

        public sealed class CMultiPoolItem : CSimpleObjectPool
        {
            internal CMultiPoolItem()
            {
            }

            public CMultiObjectPool MultiObjectPool { get; internal set; }

            internal override void OnDeallocating(CReuseable aReuseable)
            {
                base.OnDeallocating(aReuseable);
                this.MultiObjectPool.OnDeallocating(aReuseable);
            }

        }
        protected void AllocateObjectPool(int c)
            => this.AllocateObjectPool(c, i => new CMultiPoolItem());
        protected void AllocateObjectPool(int c, Func<int, CMultiPoolItem> aNewFunc)
        {
            var aMultiPoolItems = new CMultiPoolItem[c];
            for (var i = 0; i < c; ++i)
            {
                var aMultiPoolItem = aNewFunc(i);
                aMultiPoolItem.Deallocating += this.OnDeallocating;
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
        public CReuseable Allocate(int aPoolIdx)
            => this.MultiPoolItems[aPoolIdx].Allocate();

        public event Action<CReuseable> Deallocating;
        protected virtual void OnDeallocating(CReuseable aReusable)
        {
            if(this.Deallocating is object)
            {
                this.Deallocating(aReusable);
            }
        }
        #endregion
    }


    public abstract class CReuseable : CServiceLocatorNode
    {
        protected CReuseable(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        internal CSimpleObjectPool SimpleObjectPool { set; get; }
        public bool IsInUse { get; internal set; }
        internal bool DeallocateIsPending;

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
            }
            else
            {
                // TODO: 
                throw new InvalidOperationException();
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
            this.SimpleObjectPool.Deallocating += this.OnDeallocating;
            this.SimpleObjectPool.NewFunc = new CNewFunc(this.New);
        }
        public event Action<T> Deallocating;
        private void OnDeallocating(CReuseable aReusable)
        {
            if (this.Deallocating is object)
                this.Deallocating((T)aReusable);
        }
        private readonly CSimpleObjectPool SimpleObjectPool;
        public void Reserve(int aCount)
            => this.SimpleObjectPool.Reserve(aCount);
        public bool Locked { get => this.SimpleObjectPool.Locked; set => this.SimpleObjectPool.Locked = value; }
        public T Allocate()
            => (T)this.SimpleObjectPool.Allocate();
        public T Allocate(Func<T> aNewFunc)
            => (T)this.SimpleObjectPool.Allocate(()=>aNewFunc());

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
