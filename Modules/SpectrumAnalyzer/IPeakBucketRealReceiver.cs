using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resin.Modules.SpectrumAnalyzer
{
    public interface IPeakBucketRealReceiver
    {
        void ReceivePeakBucketReal(int peakBucketReal);
    }
}
