using CharlyBeck.Mvi.Cube;
using CharlyBeck.Utils3.Reflection;
using CharlyBeck.Utils3.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Propability
{
    using CDoubleRange = Tuple<double, double>;


  //  [AttributeUsage(AttributeTargets.Enum)]
    internal sealed class CPropabilityAttribute : Attribute
    {
        internal CPropabilityAttribute(double aWeight)
        {
            this.Weight = aWeight;
        }
        public readonly double Weight;
    }


    internal class CPropability<T> : CServiceLocatorNode
    {
        internal CPropability(CServiceLocatorNode aParent, Tuple<double, T> [] aWeights) :base(aParent)
        {
            this.Weights = aWeights;
            this.RandomGenerator = this.ServiceContainer.GetService<CRandomGenerator>();
        }

        private CRandomGenerator RandomGenerator;

        internal static CPropability<T> NewFromEnum(CServiceLocatorNode aParent)
        {
            var aAttributes = typeof(T).GetEnumAttributes<T, CPropabilityAttribute>();
            var aWeights = (from aAttribute in aAttributes select new Tuple<double, T>(aAttribute.Item2.Weight, aAttribute.Item1)).ToArray();
            var aPropability = new CPropability<T>(aParent, aWeights);
            return aPropability;
        }
        private Tuple<double, T>[] Map;
        private void UpdateMap()
        {
            var ws = this.Weights;
            var c = ws.Length;
            var aSum = (from w in ws select w.Item1).Sum();
            var fs = (from w in this.Weights select new Tuple<double, T>(w.Item1 / aSum, w.Item2)).ToArray();
            var aMap = new Tuple<double, T>[c];
            var w1 = 0d;
            for(var i = 0; i < c; ++i)
            {
                var f = fs[i];
                w1 += f.Item1;
                aMap[i] = new Tuple<double, T>(w1, f.Item2);
            }
            this.Map = aMap;
        }

        private Tuple<double, T>[] WeihtsM;
        public Tuple<double, T>[] Weights
        {
            get => this.WeihtsM;
            set
            {
                this.WeihtsM = value;
                this.UpdateMap();
            }
        }

        public T Next()
            => this[this.RandomGenerator.NextDouble()];

        public T this[double aRandomBool]
        {
            get
            {
                var m = this.Map;
                var c = m.Length;
                for (var i = 0; i < c; ++i)
                {
                    if (m[i].Item1 >= aRandomBool)
                        return m[i].Item2;
                }
                System.Diagnostics.Debug.Assert(false);
                return m[0].Item2;
            }
        }

    }
}
