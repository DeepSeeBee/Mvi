using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlyBeck.Mvi.Story.Propability
{
    using CDoubleRange = Tuple<double, double>;
    internal class CPropability<T>
    {
        internal CPropability(Tuple<double, T> [] aFaktors)
        {
            this.Weights = aFaktors;
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

        public T this[double p]
        {
            get
            {
                var m = this.Map;
                var c = m.Length;
                for (var i = 0; i < c; ++i)
                {
                    if (m[i].Item1 >= p)
                        return m[i].Item2;
                }
                System.Diagnostics.Debug.Assert(false);
                return m[0].Item2;
            }
        }

    }
}
