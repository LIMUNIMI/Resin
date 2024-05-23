using NITHdmis.Utils.ValueFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Resin.Modules.FFT;

namespace Resin.Setups.SpectrumFilters
{
    public class SpectrumMA : ISpectrumFilter
    {
        private IDoubleArrayFilter filter;

        public SpectrumMA(float alpha)
        {
            filter = new DoubleArrayFilterMAExpDecaying(alpha);
        }

        public double[] Compute(double[] spectrum)
        {
            filter.Push(spectrum);
            return filter.Pull();
        }
    }
}
