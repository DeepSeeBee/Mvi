using CharlyBeck.Mvi.Cube;
using CharlyBeck.Mvi.Sprites.Bumper;
using CharlyBeck.Mvi.World;
using CharlyBeck.Utils3.Exceptions;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvi.Models
{
    internal static class CColors
    {
        internal static readonly CVector3Dbl White = new CVector3Dbl(1, 1, 1);
        internal static readonly CVector3Dbl Black = new CVector3Dbl(0, 0, 0);
        internal static readonly CVector3Dbl Red = new CVector3Dbl(1, 0, 0);
        internal static readonly CVector3Dbl Green = new CVector3Dbl(0, 1, 0);
        internal static readonly CVector3Dbl Blue = new CVector3Dbl(0, 0, 1);
        internal static readonly CVector3Dbl Yellow = new CVector3Dbl(1, 1, 0);
    }


    internal sealed class CEnums<TEnum>
    {
        internal CEnums()
        {
            this.Fields = typeof(TEnum).GetEnumValues().OfType<TEnum>().ToArray();
        }
        internal readonly TEnum[] Fields;

    }
    public abstract class CModel : CServiceLocatorNode
    {
        internal CModel(CServiceLocatorNode aParent) :base(aParent)
        {
            this.World = this.ServiceContainer.GetService<CWorld>();
        }

        public readonly CWorld World;
    }


    internal sealed class CModels : CServiceLocatorNode
    {
        #region ctor
        internal CModels(CServiceLocatorNode aParent) :base(aParent)
        {
            this.World = this.ServiceContainer.GetService<CWorld>();
            this.BumperModel = new CBumperModel(this); 
        }
        public override T Throw<T>(Exception aException)
            => aException.Throw<T>();        
        #endregion
        private readonly CWorld World;
        internal CBumperModel BumperModel;

    }

}
