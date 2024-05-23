using NAudio.Wave;
using NITHdmis.Audio.In;
using NITHdmis.Audio.Out;
using NITHdmis.Filters.ValueFilters;
using NITHdmis.Headtracking.NeeqHT;
using NITHdmis.Keyboard;
using NITHdmis.MIDI;
using Resin.DMIBox;
using Resin.Modules.Audio;
using Resin.Modules.FFT;
using Resin.Modules.FFTplot;
using Resin.Modules.NotesGrid;
using Resin.Modules.PitchRecognizer;
using Resin.Modules.SpectrumAnalyzer;
using Resin.Setups.SpectrumFilters;
using System;
using System.Windows.Interop;

namespace Resin.Setups
{
    public class SetupDefault : ISetup
    {
        private ResinDMIBox T;

        public SetupDefault(MainWindow window)
        {
            R.DMIbox.MainWindow = window;
        }

        public void Setup()
        {
            // INSTANCES ====================

            T = R.DMIbox;

            AudioInParameters DefaultAudioInParameters = new AudioInParameters(48_000, 24, 1, 43, ZeroPaddingModes.FillToPowerOfTwo); // old bit 43, fillanddouble
            T.AudioInParameters = DefaultAudioInParameters;

            // Optional: this is if you want to use the (pretty unusable) fake buckets system where there's no direct mapping between in frequencies and real pitch
            // G.AllNotesValues = NoteValuesFactory.EqualFakeBuckets(T.AudioFormatFft, MidiNotes.A4, 30, 10); 
            
            R.DMIbox.NoteDatas = NoteDataFactory.NaturalNotes(T.AudioInParameters);

            T.WaveInModule = new WaveInModule(T.AudioInParameters);
            T.WaveOutMixerModule = new WaveOutDeviceMixerModule(new WaveFormat(R.DEFAULT_WAVEOUT_RATE, R.DEFAULT_WAVEOUT_CHANNELS));
            T.SineCarpetModule = new SineCarpetModule(T.WaveOutMixerModule);
            T.FftModule = new FFTModule(T.AudioInParameters);
            T.SpectrumAnalyzerModule = new SpectrumAnalyzerModule(new DoubleFilterMAExpDecaying(0.1f));
            T.MidiModule = new MidiModuleNAudio(1, 1);
            IntPtr windowHandle = new WindowInteropHelper(T.MainWindow).Handle;
            T.KeyboardModule = new KeyboardModule(windowHandle, RawInputProcessor.RawInputCaptureMode.Foreground);
            T.PitchRecognizerModule = new PitchRecognizerModule();
            T.NoteKeysModule = new NotesGridModule(T.MainWindow, T.MainWindow.gridKeys, T.MainWindow.brdNanRadio);
            T.HeadTrackerModule = new NeeqHTModule(115200, "COM");
            T.FftPlotModule = new FFTplotModule(T.MainWindow.cnvsPlot);

            T.MusicParameters = new MusicParams(T.MidiModule);
            T.MusicParameters.Pressure_Min = 0;

            // SETUPS =====================

            // Connect mixer to Sines Carpet module
            T.WaveOutMixerModule.DeviceMixerListeners.Add(T.SineCarpetModule);

            // Midi finder
            MidiDeviceFinder midiDeviceFinder = new MidiDeviceFinder(T.MidiModule);
            midiDeviceFinder.SetToLastDevice();

            // Connect [WaveIn -> FftModule]
            T.WaveInModule.PcmDataReceivers.Add(T.FftModule);

            // Define FftModule filters chain
            T.FftModule.SpectrumFilters.Add(new SpectrumSmoothing(2)); // 2 is optimal
            T.FftModule.SpectrumFilters.Add(new BrutalNoiseGate(33)); // 30 works fine
            T.FftModule.SpectrumFilters.Add(new SpectrumMA(0.9f)); //0.8f also works

            // Connect [FftModule -> FftPlot, SpectrumAnalyzer]
            T.FftModule.FftDataReceivers.Add(T.FftPlotModule);
            T.FftModule.FftDataReceivers.Add(T.SpectrumAnalyzerModule);

            // Connect some output values of the [SpectrumAnalyzerModule -> FftPlotModule] to be plotted
            T.SpectrumAnalyzerModule.PeakBucketRealReceivers.Add(T.FftPlotModule);
            T.SpectrumAnalyzerModule.PeakBucketSmoothReceivers.Add(T.FftPlotModule);
            T.SpectrumAnalyzerModule.PeakIntensityReceivers.Add(T.FftPlotModule);
            
            // BandPass all the FFT to only the Playable Notes range
            T.SetBandPassToPlayableNotes();

            // Generate the keys in the keys bar
            T.NoteKeysModule.GenerateKeys();

            // Behaviors
            T.InitializeBehaviors(); // TODO: this sucks. I'm defining behavior in the DMIbox class...

            // Set Deadzone for the headtracker
            T.HTDeadZone = 5;

            // Set Disposables
            T.Disposables.Add(T.WaveOutMixerModule);
            T.Disposables.Add(T.WaveInModule);
            
        }
    }
}