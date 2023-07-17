using NAudio.Wave;
using NeeqDMIs.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resin.Modules.Audio
{
    public class WaveInModule
    {
        public WaveInModule(AudioInParameters audioInParams)
        {
            WaveInSampleRate = audioInParams.SampleRate;
            WaveInBitRate = audioInParams.BitRate;
            WaveInChannels = audioInParams.Channels;
            WaveInBufferMilliseconds = audioInParams.BufferMilliseconds;
        }

        private short[] PcmData;

        private int WaveInBitRate = 16;

        private int WaveInBufferMilliseconds = 43;

        private int WaveInChannels = 1;

        private int waveInDeviceIndex = 0;

        private bool waveInEnabled = false;

        private WaveInEvent waveInEvent;

        private int WaveInSampleRate = 48_000;

        public int WaveInDeviceIndex
        {
            get { return waveInDeviceIndex; }
            set { waveInDeviceIndex = value; InitializeWaveIn(); }
        }

        public bool WaveInEnabled
        {
            get { return waveInEnabled; }
            set
            {
                waveInEnabled = value;
                switch (value)
                {
                    case true: StartWaveIn(); break;
                    case false: StopWaveIn(); break;
                    default: break;
                }
            }
        }

        private void InitializeWaveIn() // 2064 samples
        {
            if (waveInEvent != null)
                StopWaveIn();

            waveInEvent = new WaveInEvent();
            waveInEvent.DeviceNumber = waveInDeviceIndex;
            waveInEvent.WaveFormat = new WaveFormat(WaveInSampleRate, WaveInBitRate, WaveInChannels);
            waveInEvent.DataAvailable += OnWaveInDataAvailable;
            waveInEvent.BufferMilliseconds = WaveInBufferMilliseconds;
        }

        private void OnWaveInDataAvailable(object sender, WaveInEventArgs e)
        {
            int bytesPerSample = waveInEvent.WaveFormat.BitsPerSample / 8;
            int samplesRecorded = e.BytesRecorded / bytesPerSample;
            if (PcmData == null)
                PcmData = new Int16[samplesRecorded];
            //MessageBox.Show(samplesRecorded.ToString());
            for (int i = 0; i < samplesRecorded; i++)
                PcmData[i] = BitConverter.ToInt16(e.Buffer, i * bytesPerSample);

            NotifyPcmListeners();
        }

        private void StartWaveIn()
        {
            if (waveInEvent != null)
            {
                waveInEvent.StopRecording();
            }

            InitializeWaveIn();
            waveInEvent.StartRecording();
        }

        private void StopWaveIn()
        {
            if (waveInEvent != null)
                waveInEvent.StopRecording();
        }

        #region Interface

        // OUTPUTS
        public List<IPcmDataReceiver> PcmDataReceivers { get; set; } = new List<IPcmDataReceiver>();

        private void NotifyPcmListeners()
        {
            foreach (IPcmDataReceiver l in PcmDataReceivers)
            {
                l.ReceivePCMData(PcmData);
            }
        }

        #endregion Interface
    }
}
