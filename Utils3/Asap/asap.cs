using CharlyBeck.Utils3.Enumerables;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Utils3.Asap
{
    public delegate CReuseable CNewFunc();

    public abstract class CObjectPoolBase
    {
        #region NoOutOfMemoryException
        private bool NoOutOfMemoryExceptionM;
        public bool NoOutOfMemoryException 
        { 
            get => this.NoOutOfMemoryExceptionM; 
            set
            {
                this.NoOutOfMemoryExceptionM = value;
                this.OnNoOutOfMemoryExceptionChanged();
            }
        }
        internal virtual void OnNoOutOfMemoryExceptionChanged()
        {

        }
        #endregion
        #region Allocate
        public event Action<CReuseable> Allocated;
        protected virtual void OnAllocated(CReuseable aReusable)
        {
            if (this.Allocated is object)
            {
                this.Allocated(aReusable);
            }
        }
        public Action<CReuseable> Deallocated;
        protected virtual void OnDeallocated(CReuseable aReuseable)
        {
            if (this.Deallocated is object)
                this.Deallocated(aReuseable);
        }
        #endregion
    }


    public class CObjectPool : CObjectPoolBase
    {
        #region ctor
        public CObjectPool()
        {
        }
        #endregion
        #region Reusable
        public CNewFunc NewFunc { get; set; }

        public bool Locked;
        private readonly List<CReuseable> FreeList = new List<CReuseable>();
        public IEnumerable<CReuseable> Frees => this.FreeList;
        public CReuseable Allocate()
            => this.Allocate(this.NewFunc);
        public CReuseable Allocate(CNewFunc aNewFunc)
        {
            CReuseable aReusable;
            var aList = this.FreeList;
            lock(aList)
            {                
                if(aList.Count > 0)
                {
                    aReusable = aList[0];
                    aList.RemoveAt(0);
                }
                else  if(!this.Locked)
                {
                    aReusable = this.BuildNew(aNewFunc);
                }
                else if(this.NoOutOfMemoryException)
                {
                    aReusable = default;
                }
                else
                {
                    throw new OutOfMemoryException();
                }
                if(aReusable is object)
                {
                    this.OnAllocated(aReusable);
                }
            }
            if(aReusable is object)
            {
                aReusable.BeginUse();
            }
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
                    this.OnDeallocated(aReusable);
                }
                finally
                {
                    aReusable.DeallocateIsPending = false;
                }
                var aList = this.FreeList;
                lock (aList)
                {
                    aList.Add(aReusable);
                }

            }
        }

        public int FreeCount => this.FreeList.Count;

        internal void Reserve(int aCount, bool aLock = false)
        {
            var aList = this.FreeList;
            for (var i = 0; i < aCount; ++i)
            {
                var aItem = this.BuildNew(this.NewFunc);
                lock (aList)
                {
                    aList.Add(aItem);
                }
            }
            if (aLock)
                this.Locked = true;
        }
        #endregion
    }

    public abstract class CMultiObjectPool : CObjectPoolBase
    {
        #region ctor
        protected CMultiObjectPool()
        {

        }

        public void Reserve(int aClassId, int aObjectCount, bool aLock)
        {
            var aItem = this.MultiPoolItems[aClassId];
            aItem.Reserve(aObjectCount);
            if (aLock)
                aItem.Locked = true;
        }
        public IEnumerable<CReuseable> Frees => this.MultiPoolItems.Select(aItem => aItem.Frees).Flatten();
        protected void AllocateObjectPool(int c)
            => this.AllocateObjectPool(c, i => new CObjectPool());
        protected void AllocateObjectPool(int c, Func<int, CObjectPool> aNewFunc)
        {
            var aObjectPoolItem = new CObjectPool[c];
            for (var i = 0; i < c; ++i)
            {
                var aMultiPoolItem = aNewFunc(i);
                aMultiPoolItem.Allocated += this.OnAllocated;
                aMultiPoolItem.Deallocated += this.OnDeallocated;
                aMultiPoolItem.NoOutOfMemoryException = this.NoOutOfMemoryException;
                aObjectPoolItem[i] = aMultiPoolItem;
            }
            this.MultiPoolItems = aObjectPoolItem; 
        }
        #endregion
        #region MultiPoolItems
        private CObjectPool[] MultiPoolItems;
        #endregion
        #region Reusable
        public void SetNewFunc(int i, CNewFunc aNewFunc)
            => this.MultiPoolItems[i].NewFunc = aNewFunc;
        public CReuseable Allocate(int aPoolIdx)
            => this.MultiPoolItems[aPoolIdx].Allocate();
        #endregion
        #region NoOutOfMemoryException
        internal override void OnNoOutOfMemoryExceptionChanged()
        {
            base.OnNoOutOfMemoryExceptionChanged();
            if(this.MultiPoolItems is object)
            {
                foreach (var aItem in this.MultiPoolItems)
                    aItem.NoOutOfMemoryException = this.NoOutOfMemoryException;
            }
        }
        #endregion
    }


    public abstract class CReuseable : CServiceLocatorNode
    {
        protected CReuseable(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        internal CObjectPool SimpleObjectPool { set; get; }
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
            this.SimpleObjectPool = new CObjectPool();
            this.SimpleObjectPool.Deallocated += this.OnDeallocated;
            this.SimpleObjectPool.Allocated += this.OnAllocated;

            this.SimpleObjectPool.NewFunc = new CNewFunc(this.New);
        }
        public event Action<T> Deallocated;
        private void OnDeallocated(CReuseable aReusable)
        {
            if (this.Deallocated is object)
                this.Deallocated((T)aReusable);
        }
        public event Action<T> Allocated;
        private void OnAllocated(CReuseable aReusable)
        {
            if (this.Allocated is object)
                this.Allocated((T)aReusable);
        }
        private readonly CObjectPool SimpleObjectPool;
        public int FreeCount => this.SimpleObjectPool.FreeCount;
        public void Reserve(int aCount, bool aLock = false)
            => this.SimpleObjectPool.Reserve(aCount, aLock);
        public bool Locked { get => this.SimpleObjectPool.Locked; set => this.SimpleObjectPool.Locked = value; }
        public T Allocate()
            => (T)this.SimpleObjectPool.Allocate();
        public T Allocate(Func<T> aNewFunc)
            => (T)this.SimpleObjectPool.Allocate(()=>aNewFunc());

        public Func<T> NewFunc { get; set; }
        private CReuseable New() => this.NewFunc();

        public IEnumerable<CReuseable> Frees => this.SimpleObjectPool.Frees;

        public bool NoOutOfMemoryException { get => this.SimpleObjectPool.NoOutOfMemoryException; set => this.SimpleObjectPool.NoOutOfMemoryException = value; }
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
