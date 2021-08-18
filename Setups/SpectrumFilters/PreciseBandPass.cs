using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Resin.Modules.FFT;

namespace Resin.Setups.SpectrumFilters
{
    public class PreciseBandPass : ISpectrumFilter
    {
        private int lowCut;
        private int highCut;

        public PreciseBandPass(int lowCut, int highCut)
        {
            this.lowCut = lowCut;
            this.highCut = highCut;
        }

        public double[] Compute(double[] spectrum)
        {
            for(int i = 0; i < spectrum.Length; i++)
            {
                if(i < lowCut || i > highCut)
                {
                    spectrum[i] = 0f;
                }
            }
            return spectrum;
        }
    }
}
