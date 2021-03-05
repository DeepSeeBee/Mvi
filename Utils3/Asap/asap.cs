using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils3.Asap
{
    public delegate CReusable CNewFunc();

    public abstract class CObjectPool 
    {
        protected CObjectPool()
        {
        }
        #region Factory
        public CNewFunc NewFunc { get; set; }
        protected virtual CReusable New()
            => this.NewFunc();
        #endregion
    }

    public abstract class CSimpleObjectPool : CObjectPool
    {
        #region ctor
        public CSimpleObjectPool()
        {
        }
        #endregion
        #region Reusable
        private readonly List<CReusable> Free = new List<CReusable>();
        public CReusable Allocate()
        {
            CReusable aReusable;
            var aList = this.Free;
            lock(aList)
            {                
                if(aList.Count == 0)
                {
                    aReusable = this.New();
                    aReusable.SimpleObjectPool = this;
                }
                else
                {
                    aReusable = aList[0];
                    aList.RemoveAt(0);
                }
            }
            aReusable.BeginUse();
            return aReusable;
        }

        internal void Deallocate(CReusable aReusable)
        {
            var aList = this.Free;
            lock(aList)
            {
                aList.Add(aReusable);
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
        protected CReusable Allocate(int aPoolIdx)
            => this.MultiPoolItems[aPoolIdx].Allocate();
        #endregion
    }


    public abstract class CReusable 
    {
        protected CReusable() : base()
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
            this.EndUse();
            this.SimpleObjectPool.Deallocate(this);
        }
    }


}
