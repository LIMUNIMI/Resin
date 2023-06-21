using NeeqDMIs.Headtracking.NeeqHT;
using NeeqDMIs.Keyboard;
using NeeqDMIs.MIDI;
using NeeqDMIs.Utils;
using NeeqDMIs.Utils.ValueFilters;
using System;
using System.Collections.Generic;
using System.Windows.Interop;
using Resin.DataTypes;
using Resin.DMIBox;
using Resin.Modules.Audio;
using Resin.Modules.FFT;
using Resin.Modules.FFTplot;
using Resin.Modules.NotesGrid;
using Resin.Modules.PitchRecognizer;
using Resin.Modules.SpectrumAnalyzer;
using Resin.Setups.Behaviors;
using Resin.Setups.SpectrumFilters;
using NeeqDMIs.Filters.ValueFilters;

namespace Resin.Setups
{
    public class SetupDefault : ISetup
    {
        private ResinDMIBox T;

        public SetupDefault(MainWindow window)
        {
            R.DMIbox = new ResinDMIBox();
            R.DMIbox.MainWindow = window;
        }

        public void Setup()
        {
            #region Instances

            T = R.DMIbox;

            T.AudioFormatFft = new AudioFormatFft(48_000, 24, 1, 43, AudioFormatFft.ZeroPaddingModes.FillToPowerOfTwo); // old bit 43, fillanddouble

            //G.AllNotesValues = NoteValuesFactory.EqualFakeBuckets(T.AudioFormatFft, MidiNotes.A4, 30, 10);
            R.DMIbox.NoteDatas = NoteDataFactory.NaturalNotes(T.AudioFormatFft);

            T.AudioModule = new AudioModule(T.AudioFormatFft);
            T.FftModule = new FFTModule(T.AudioFormatFft);
            T.SpectrumAnalyzerModule = new SpectrumAnalyzerModule(new DoubleFilterMAExpDecaying(0.3f));
            T.MidiModule = new MidiModuleNAudio(1, 1);
            IntPtr windowHandle = new WindowInteropHelper(T.MainWindow).Handle;
            T.KeyboardModule = new KeyboardModule(windowHandle, RawInputProcessor.RawInputCaptureMode.Foreground);
            T.PitchRecognizerModule = new PitchRecognizerModule();
            T.NoteKeysModule = new NotesGridModule(T.MainWindow, T.MainWindow.gridKeys, T.MainWindow.brdNanRadio);
            T.HeadTrackerModule = new NeeqHTModule(115200, "COM");
            T.FFTplotModule = new FFTplotModule(T.MainWindow.cnvsPlot);

            T.MusicParameters = new MusicParams(T.MidiModule);
            T.MusicParameters.Pressure_Min = 0;

            #endregion Instances

            #region Setups

            MidiDeviceFinder midiDeviceFinder = new MidiDeviceFinder(T.MidiModule);
            midiDeviceFinder.SetToLastDevice();

            T.AudioModule.PcmDataReceivers.Add(T.FftModule);

            //T.FftModule.SpectrumFilters.Add(new PreciseBandPass(50, 10000));
            T.FftModule.SpectrumFilters.Add(new SpectrumSmoothing(2)); // 2 is optimal
            T.FftModule.SpectrumFilters.Add(new BrutalNoiseGate(33)); // 30 works fine
            T.FftModule.SpectrumFilters.Add(new SpectrumMA(0.9f)); //0.8f also works

            T.FftModule.FftDataReceivers.Add(T.FFTplotModule);
            T.FftModule.FftDataReceivers.Add(T.SpectrumAnalyzerModule);

            T.SpectrumAnalyzerModule.PeakBucketRealReceivers.Add(T.FFTplotModule);
            T.SpectrumAnalyzerModule.PeakBucketSmoothReceivers.Add(T.FFTplotModule);
            T.SpectrumAnalyzerModule.PeakIntensityReceivers.Add(T.FFTplotModule);
            T.SetBandPassToPlayableNotes();
            // T.SpectrumAnalyzerModule.NoteEnergiesReceivers.Add(T.FFTplotModule);
            // T.SpectrumAnalyzerModule.NoteEnergiesReceivers.Add(T.AudioModule);

            T.NoteKeysModule.GenerateKeys();

            T.InitializeBehaviors();
            T.HTDeadZone = 5;
            

            #endregion Setups
        }
    }
}