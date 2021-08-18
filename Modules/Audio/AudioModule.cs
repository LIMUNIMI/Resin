using CsvHelper;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NeeqDMIs.MicroLibrary;
using NeeqDMIs.Music;
using NeeqDMIs.Utils;
using Resin.DataTypes;
using Resin.DMIBox;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Resin.Modules.Audio
{
    public class AudioModule
    {
        #region WaveIn

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

        #endregion WaveIn

        #region WaveOut

        private const float calibGain = 0.8f;

        private MixingSampleProvider MixProvider;

        private float panning = 0;

        private PanningSampleProvider PanProvider;

        private List<ISampleProvider> sampleProviders;

        private int waveOutDeviceIndex = 0;

        private bool waveOutEnabled = false;

        private WaveOutEvent waveOutEvent;

        private float waveOutMasterVolume = 0.8f;

        public SignalGeneratorType CarpetSignalType { get; set; } = SignalGeneratorType.Sin;

        public float Panning
        {
            get { return panning; }
            set
            {
                panning = value;
                if (WaveOutEnabled)
                {
                    StopWaveOut();
                    StartWaveOutSinesCarpet();
                }
            }
        }

        public int WaveOutDeviceIndex
        {
            get { return waveOutDeviceIndex; }
            set { waveOutDeviceIndex = value; InitializeWaveOut(); }
        }

        public bool WaveOutEnabled
        {
            get { return waveOutEnabled; }
            set
            {
                waveOutEnabled = value;
                switch (value)
                {
                    case true: StartWaveOutSinesCarpet(); break;
                    case false: StopWaveOut(); break;
                    default: break;
                }
            }
        }

        public float WaveOutMasterVolume
        {
            get { return waveOutMasterVolume; }
            set
            {
                waveOutMasterVolume = value;
                if (WaveOutEnabled)
                {
                    StopWaveOut();
                    StartWaveOutSinesCarpet();
                }
            }
        }

        public int WaveOutSampleRate { get; } = 48_000;

        public void UpdateGain(MidiNotes note, double gain)
        {
            foreach (NoteData and in G.TB.NoteDatas)
            {
                if (and.MidiNote == note)
                {
                    and.Out_Gain = gain;
                }
            }
            if (waveOutEnabled)
            {
                StopWaveOut();
                StartWaveOutSinesCarpet();
            }
        }

        public void UpdatePlayableNotes()
        {
            if (waveOutEnabled)
            {
                StartWaveOutSinesCarpet();
            }
        }

        private float GetStandardGain()
        {
            return WaveOutMasterVolume / G.TB.GetPlayableNoteDatas().Count;
        }

        private void InitializeWaveOut()
        {
            if (waveOutEvent != null)
                waveOutEvent.Stop();

            waveOutEvent = new WaveOutEvent();
            waveOutEvent.NumberOfBuffers = 2;
            waveOutEvent.DeviceNumber = WaveOutDeviceIndex;
            waveOutEvent.DesiredLatency = 100;
        }

        private void StartWaveOutSinesCarpet()
        {
            var pn = G.TB.GetPlayableNoteDatas();

            if (pn.Count > 0)
            {
                waveOutEvent?.Stop();

                InitializeWaveOut();
                UpdateCarpetSampleProviders();

                waveOutEvent.Init(new SampleToWaveProvider(PanProvider));
                waveOutEvent.Play();
            }
            else
            {
                waveOutEvent?.Stop();
            }
        }

        private void StopWaveOut()
        {
            if (waveOutEvent != null)
                waveOutEvent.Stop();
        }

        private void UpdateCarpetSampleProviders()
        {
            sampleProviders = new List<ISampleProvider>();

            var pn = G.TB.GetPlayableNoteDatas();

            foreach (NoteData noteData in pn)
            {
                double freq = 0;
                double gain = 0;      // Gain is reduced with number of notes increase to avoid distortion

                try
                {
                    freq = noteData.Out_Frequency;
                }
                catch
                {
                    throw new Exception("AudioModule exception: unable to get the frequency for the note " + noteData.MidiNote.ToStandardString() + ".");
                }

                gain = noteData.Out_Gain;

                sampleProviders.Add(new SignalGenerator(WaveOutSampleRate, 1) { Frequency = freq, Gain = gain / 70f, Type = CarpetSignalType });
            }

            MixProvider = new MixingSampleProvider(sampleProviders);
            PanProvider = new PanningSampleProvider(MixProvider);
            PanProvider.Pan = Panning;
        }

        #endregion WaveOut

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

        public AudioModule(AudioFormatFft audioFormatFft)
        {
            WaveInSampleRate = audioFormatFft.SampleRate;
            WaveInBitRate = audioFormatFft.BitRate;
            WaveInChannels = audioFormatFft.Channels;
            WaveInBufferMilliseconds = audioFormatFft.BufferMilliseconds;
        }

        #region Energy Calibration

        private const float CALIBRATION_INCREMENT = 0.05f;
        private const int ENERGYCALIBRATION_INTERVAL = 700;
        private const string saveCalibPath = "Calibration.csv";
        private MicroTimer energyCalibTimer;

        public double CalibrationSetpoint { get; set; } = 0;
        public MidiNotes SingleNoteToCalibrate { get; set; } = MidiNotes.NaN;

        public void LoadCalibration()
        {
            using (var reader = new StreamReader(saveCalibPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                G.TB.NoteDatas.Clear();
                var ands = csv.GetRecords<NoteData>();
                G.TB.NoteDatas.AddRange(from NoteData and in ands
                                        select and);
            }

            G.TB.NoteKeysModule.SetSlidersToBeUpdated();
        }

        public void SaveCalibration()
        {
            using (var writer = new StreamWriter(saveCalibPath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(G.TB.NoteDatas);
            }
        }

        public void StartEnergyCalibration(int intervalMilliseconds = ENERGYCALIBRATION_INTERVAL)
        {
            energyCalibTimer = new MicroTimer(intervalMilliseconds * 1000);
            energyCalibTimer.MicroTimerElapsed += EnergyCalibTimer_MicroTimerElapsed;
            waveOutEnabled = true;
            energyCalibTimer.Start();
        }

        public void StopEnergyCalibration()
        {
            energyCalibTimer.Stop();
            WaveOutEnabled = false;
        }

        private void EnergyCalibTimer_MicroTimerElapsed(object sender, MicroTimerEventArgs e)
        {
            foreach (NoteData and in G.TB.NoteDatas)
            {
                if (and.IsPlayable)
                {
                    // ADDED SINGLE NOTE CONTROL CONDITION
                    if (SingleNoteToCalibrate == and.MidiNote || SingleNoteToCalibrate == MidiNotes.NaN)
                    {
                        if (and.In_Energy > CalibrationSetpoint && and.Out_Gain > 0f)
                        {
                            and.Out_Gain -= CALIBRATION_INCREMENT;
                        }
                        else if (and.In_Energy < CalibrationSetpoint) //&& and.Gain < 1f - CALIBRATION_INCREMENT)
                        {
                            and.Out_Gain += CALIBRATION_INCREMENT;
                        }

                        if (and.Out_Gain < 0f)
                            and.Out_Gain = 0f;
                    }
                }
            }

            UpdateCarpetSampleProviders();
            waveOutEvent?.Stop();
            waveOutEvent.Init(new SampleToWaveProvider(PanProvider));
            waveOutEvent.Play();

            G.TB.NoteKeysModule.SetSlidersToBeUpdated();
        }

        #endregion Energy Calibration
    }
}