using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resin.Modules.FFT
{
    public interface IFftDataReceiver
    {
        void ReceiveFFTData(double[] fftData);
    }
}
