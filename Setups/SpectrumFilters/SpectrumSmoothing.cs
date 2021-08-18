using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Resin.Modules.FFT;

namespace Resin.Setups.SpectrumFilters
{
    public class SpectrumSmoothing : ISpectrumFilter
    {
        private int nPoints;

        public SpectrumSmoothing(int nPoints)
        {
            this.nPoints = nPoints;
        }

        public double[] Compute(double[] spectrum)
        {
            int c = 0;
            double sum = 0;
            for(int i = 0; i < spectrum.Length; i++)
            {
                c = 0;
                sum = 0;
                for(int j = -nPoints; j <= nPoints; j++)
                {
                    if((i + j >= 0) && (i + j < spectrum.Length))
                    {
                        c++;
                        sum = sum + spectrum[i + j];
                    }
                }
                spectrum[i] = sum / c;
            }
            return spectrum;
        }
    }
}
