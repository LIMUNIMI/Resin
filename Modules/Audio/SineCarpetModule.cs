using CsvHelper;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NeeqDMIs.MicroLibrary;
using NeeqDMIs.Music;
using Resin.DataTypes;
using Resin.DMIBox;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Resin.Modules.Audio;

namespace Resin.Modules.Audio
{
    public class SineCarpetModule
    {
        public List<ISampleProvider> SampleProviders;
        private const float CALIBRATION_INCREMENT = 0.05f;
        private const int ENERGYCALIBRATION_INTERVAL = 700;
        private const string saveCalibPath = "Calibration.csv";
        private bool enabled = false;
        private MicroTimer energyCalibTimer;

        private float masterVolume = 0.8f;
        private float panning = 0;

        private WaveOutModule waveOutModule;

        public SineCarpetModule(WaveOutModule waveOutModule)
        {
            this.waveOutModule = waveOutModule;
        }

        public float CalibGain { get; set; } = 0.8f;
        public double CalibrationSetpoint { get; set; } = 0;

        public SignalGeneratorType CarpetSignalType { get; set; } = SignalGeneratorType.Sin;

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                switch (value)
                {
                    case true: StartSinesCarpet(); break;
                    case false: waveOutModule.StopWaveOut(); break;
                    default: break;
                }
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
                    waveOutModule.StopWaveOut();
                    StartSinesCarpet();
                }
            }
        }

        public MixingSampleProvider MixProvider { get; set; }

        public float Panning
        {
            get { return panning; }
            set
            {
                panning = value;
                if (Enabled)
                {
                    waveOutModule.StopWaveOut();
                }
            }
        }

        public PanningSampleProvider PanProvider { get; set; }
        public MidiNotes SingleNoteToCalibrate { get; set; } = MidiNotes.NaN;

        public void LoadCalibration()
        {
            using (var reader = new StreamReader(saveCalibPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                R.DMIbox.NoteDatas.Clear();
                var ands = csv.GetRecords<NoteData>();
                R.DMIbox.NoteDatas.AddRange(from NoteData and in ands
                                            select and);
            }

            R.DMIbox.NoteKeysModule.SetSlidersToBeUpdated();
        }

        public void SaveCalibration()
        {
            using (var writer = new StreamWriter(saveCalibPath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(R.DMIbox.NoteDatas);
            }
        }

        public void StartEnergyCalibration(int intervalMilliseconds = ENERGYCALIBRATION_INTERVAL)
        {
            energyCalibTimer = new MicroTimer(intervalMilliseconds * 1000);
            energyCalibTimer.MicroTimerElapsed += EnergyCalibTimer_MicroTimerElapsed;
            Enabled = true;
            energyCalibTimer.Start();
        }

        public void StopEnergyCalibration()
        {
            energyCalibTimer.Stop();
            Enabled = false;
        }

        public void UpdateCarpetSampleProviders()
        {
            SampleProviders = new List<ISampleProvider>();

            var pn = R.DMIbox.GetPlayableNoteDatas();

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

                SampleProviders.Add(new SignalGenerator(waveOutModule.WaveOutSampleRate, 1) { Frequency = freq, Gain = gain / 70f, Type = CarpetSignalType });
            }

            MixProvider = new MixingSampleProvider(SampleProviders);
            PanProvider = new PanningSampleProvider(MixProvider);
            PanProvider.Pan = Panning;
        }

        public void UpdateGain(MidiNotes note, double gain)
        {
            foreach (NoteData and in R.DMIbox.NoteDatas)
            {
                if (and.MidiNote == note)
                {
                    and.Out_Gain = gain;
                }
            }
            if (Enabled)
            {
                waveOutModule.StopWaveOut();
                StartSinesCarpet();
            }
        }

        public void UpdatePlayableNotes()
        {
            if (Enabled)
            {
                StartSinesCarpet();
            }
        }

        private void EnergyCalibTimer_MicroTimerElapsed(object sender, MicroTimerEventArgs e)
        {
            foreach (NoteData and in R.DMIbox.NoteDatas)
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
            waveOutModule.StopWaveOut();
            waveOutModule.WaveOutEvent.Init(new SampleToWaveProvider(PanProvider));
            waveOutModule.WaveOutEvent.Play();

            R.DMIbox.NoteKeysModule.SetSlidersToBeUpdated();
        }

        private float GetStandardGain()
        {
            return MasterVolume / R.DMIbox.GetPlayableNoteDatas().Count;
        }

        private void StartSinesCarpet()
        {
            var pn = R.DMIbox.GetPlayableNoteDatas();

            if (pn.Count > 0)
            {
                waveOutModule.WaveOutEvent?.Stop();

                waveOutModule.InitializeWaveOut();
                UpdateCarpetSampleProviders();

                waveOutModule.WaveOutEvent.Init(new SampleToWaveProvider(PanProvider));
                waveOutModule.WaveOutEvent.Play();
            }
            else
            {
                waveOutModule.WaveOutEvent?.Stop();
            }
        }
    }
}