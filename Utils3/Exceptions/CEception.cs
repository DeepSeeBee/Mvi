using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Utils3.Exceptions
{
   public static class CEceptionExtensions
   {
      public static T Throw<T>(this Exception aExc)
         => throw aExc;
   }

    public sealed class CMethodNotOverridenExc : Exception { }
}
