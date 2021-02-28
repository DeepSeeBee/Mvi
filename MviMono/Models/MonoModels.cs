using CharlyBeck.Mvi.Mono.GameCore;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.LazyLoad;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MviMono.Models
{
 

    internal sealed class CMonoModels : CServiceLocatorNode
    {
        #region ctor
        internal CMonoModels(CServiceLocatorNode aParent) : base(aParent)
        {
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();
        #endregion
        #region Models
        private Dictionary<object, CMonoModel> Dic = new Dictionary<object, CMonoModel>();
        internal T LoadMonoModel<T>(object aKey, Func<CServiceLocatorNode, T> aNew) where T : CMonoModel
        {
            if (this.Dic.ContainsKey(aKey))
                return (T)(object)this.Dic[aKey];
            else
            {
                var aNewModel = aNew(this);
                this.Dic.Add(aKey, aNewModel);
                return aNewModel;
            }
        }
        #endregion

    }

    internal abstract class CMonoModel : CServiceLocatorNode
    {
        internal CMonoModel(CServiceLocatorNode aParent) : base(aParent)
        { }

        #region Game
        private CGame GameM;
        internal CGame Game => CLazyLoad.Get(ref this.GameM, ()=>this.ServiceContainer.GetService<CGame>());
        #endregion
        //#region Draw
        //internal virtual void Draw() { }
        //internal virtual void DrawPrimitives() { }
        //#endregion
    }
}
