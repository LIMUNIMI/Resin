using NAudio.Wave;
using System.Collections.Generic;

namespace Resin.Modules.Audio
{
    public class WaveOutModule
    {
        private int waveOutDeviceIndex = 0;

        public int WaveOutDeviceIndex
        {
            get { return waveOutDeviceIndex; }
            set { waveOutDeviceIndex = value; InitializeWaveOut(); }
        }

        public WaveOutEvent WaveOutEvent { get; set; }

        public int WaveOutSampleRate { get; } = 48_000;

        public void InitializeWaveOut()
        {
            if (WaveOutEvent != null)
                WaveOutEvent.Stop();

            WaveOutEvent = new WaveOutEvent();
            WaveOutEvent.NumberOfBuffers = 2;
            WaveOutEvent.DeviceNumber = WaveOutDeviceIndex;
            WaveOutEvent.DesiredLatency = 100;
        }

        public void StopWaveOut()
        {
            if (WaveOutEvent != null)
                WaveOutEvent.Stop();
        }
    }
}