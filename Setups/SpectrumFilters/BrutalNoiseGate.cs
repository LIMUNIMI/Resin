using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Resin.Modules.FFT;

namespace Resin.Setups.SpectrumFilters
{
    public class BrutalNoiseGate : ISpectrumFilter
    {
        private int lowCut;

        public BrutalNoiseGate(int lowCut)
        {
            this.lowCut = lowCut;
        }

        public double[] Compute(double[] spectrum)
        {
            for(int i = 0; i < spectrum.Length; i++)
            {
                if(spectrum[i] < lowCut)
                {
                    spectrum[i] = 0f;
                }
            }
            return spectrum;
        }
    }
}
