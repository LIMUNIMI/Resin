using CsvHelper;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NITHdmis.Audio.Out;
using NITHdmis.MicroLibrary;
using NITHdmis.Music;
using Resin.DataTypes;
using Resin.DMIBox;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Resin.Modules.Audio
{
    /// <summary>
    /// A WaveSampleMaker which makes a sine wave carpet, with defined sine wave frequencies.
    /// Includes a way to calibrate the sines carpet
    /// </summary>
    public class SineCarpetModule : AWaveSampleMaker
    {
        public List<ISampleProvider> SinesProviders;
        private const float CALIBRATION_INCREMENT = 0.05f;
        private const int ENERGYCALIBRATION_INTERVAL = 500;
        private const string saveCalibPath = "Calibration.csv";
        private const float DEFAULT_GAIN_DIVIDER = 70f;
        private bool enabled = false;
        private MicroTimer energyCalibTimer;

        private float masterVolume;

        private float panning = 0;
        private SignalGenerator muteSignalGenerator;
        private WaveFormat waveFormat;

        private VolumeSampleProvider VolumeSampleProvider { get; set; }
        public SineCarpetModule(ISampleMixer sampleMixer) : base(sampleMixer)
        {
            waveFormat = sampleMixer.GetWaveFormat();
            muteSignalGenerator = new SignalGenerator(waveFormat.SampleRate, 1) { Gain = 0 };
            //BuildChain();
        }

        /// <summary>
        /// Setpoint for gain calibraton
        /// </summary>
        public double CalibrationSetpoint { get; set; } = 0;

        public SignalGeneratorType CarpetSignalType { get; set; } = SignalGeneratorType.Sin;

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                UpdateSinesAndSend();
            }
        }

        public float MasterVolume
        {
            get { return masterVolume; }
            set
            {
                masterVolume = value;
                if (Enabled)
                {
                    UpdateSinesAndSend();
                }
            }
        }

        private void BuildChain()
        {
            // MixProvider must contain at least one sample.
            if (SinesProviders.Count == 0)
            {
                SinesProviders.Add(muteSignalGenerator);
            }

            MixProvider = new MixingSampleProvider(SinesProviders);

            SinesProviders.Remove(muteSignalGenerator);

            PanProvider = new PanningSampleProvider(MixProvider);
            PanProvider.Pan = Panning;
            VolumeSampleProvider = new VolumeSampleProvider(PanProvider);
            VolumeSampleProvider.Volume = masterVolume;
            
        }

        private MixingSampleProvider MixProvider { get; set; }

        public float Panning
        {
            get { return panning; }
            set
            {
                panning = value;
                if (Enabled)
                {
                    UpdateSinesAndSend();
                }
            }
        }

        private PanningSampleProvider PanProvider { get; set; }
        public MidiNotes SingleNoteToCalibrate { get; set; } = MidiNotes.NaN;

        /// <summary>
        /// Load sine wave gains calibration data from a local file
        /// </summary>
        public void LoadCalibration()
        {
            using (var reader = new StreamReader(saveCalibPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                R.DMIbox.NoteDatas.Clear();
                var ands = csv.GetRecords<ResinNoteData>();
                R.DMIbox.NoteDatas.AddRange(from ResinNoteData and in ands
                                            select and);
            }

            R.DMIbox.NoteKeysModule.SetSlidersToBeUpdated();
        }

        /// <summary>
        /// What happens when audio device is changed: do nothing. This module won't update anything, and doesn't care about device changes.
        /// </summary>
        public override void ReceiveDeviceChanged()
        {
            waveFormat = sampleMixer.GetWaveFormat();
            UpdateSinesAndSend();
        }

        /// <summary>
        /// Saves the data of the calibrated sines.
        /// </summary>
        public void SaveCalibration()
        {
            using (var writer = new StreamWriter(saveCalibPath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(R.DMIbox.NoteDatas);
            }
        }

        /// <summary>
        /// Command to initialize the sines'energy calibration process, to flatten the energy response
        /// </summary>
        /// <param name="intervalMilliseconds"></param>
        public void StartEnergyCalibration(int intervalMilliseconds = ENERGYCALIBRATION_INTERVAL)
        {
            energyCalibTimer = new MicroTimer(intervalMilliseconds * 1000);
            energyCalibTimer.MicroTimerElapsed += EnergyCalibTimer_MicroTimerElapsed;
            Enabled = true;
            energyCalibTimer.Start();
        }

        /// <summary>
        /// Stops the gain calibration process
        /// </summary>
        public void StopEnergyCalibration()
        {
            energyCalibTimer.Stop();
            Enabled = false;
        }

        /// <summary>
        /// Updates the Sample Providers and notify the SampleMixer if enabled
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void UpdateSinesAndSend()
        {
            SinesProviders = new List<ISampleProvider>();

            var pn = R.DMIbox.GetPlayableNoteDatas();

            foreach (ResinNoteData noteData in pn)
            {
                double freq = 0;
                double gain = 0;      // Gain is reduced with number of notes increase to avoid distortion

                try
                {
                    freq = noteData.Out_Frequency;
                }
                catch
                {
#pragma warning disable S112 // General exceptions should never be thrown
                    throw new Exception(this.GetType().Name + " exception: unable to get the frequency for the note " + noteData.MidiNote.ToStandardString() + ".");
#pragma warning restore S112 // General exceptions should never be thrown
                }

                gain = noteData.Out_Gain;

                SinesProviders.Add(new SignalGenerator(waveFormat.SampleRate, 1) { Frequency = freq, Gain = gain / DEFAULT_GAIN_DIVIDER, Type = CarpetSignalType });
            }

            BuildChain();
            candidateSampleProvider = VolumeSampleProvider;

            if (enabled)
            {
                NotifySampleChanged();
            }
        }

        /// <summary>
        /// Updates the gain of the sine corresponding to a given MidiNote
        /// </summary>
        /// <param name="note"></param>
        /// <param name="gain"></param>
        public void UpdateNoteGain(MidiNotes note, double gain)
        {
            foreach (ResinNoteData and in R.DMIbox.NoteDatas)
            {
                if (and.MidiNote == note)
                {
                    and.Out_Gain = gain;
                }
            }
            UpdateSinesAndSend();
        }

        /// <summary>
        /// Energy calibration timer. At each cycle, updates the gain data, and starts the sample updating process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnergyCalibTimer_MicroTimerElapsed(object sender, MicroTimerEventArgs e)
        {
            foreach (ResinNoteData and in R.DMIbox.NoteDatas)
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

            UpdateSinesAndSend();

            R.DMIbox.NoteKeysModule.SetSlidersToBeUpdated();
        }
    }
}