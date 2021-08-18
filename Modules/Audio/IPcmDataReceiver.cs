using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resin.Modules.Audio
{
    public interface IPcmDataReceiver
    {
        void ReceivePCMData(Int16[] pcmData);
    }
}
