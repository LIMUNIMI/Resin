using NAudio.Wave;
using NeeqDMIs.Music;
using NeeqDMIs.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Tong_Sharp.DMIBox;
using Tong_Sharp.Modules.FFT;
using Tong_Sharp.Modules.SpectrumAnalyzer;
using Tong_Sharp.Setups;

namespace Tong_Sharp
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IPeakIntensityReceiver, IPeakBucketRealReceiver, IPeakBucketSmoothReceiver, IFftDataReceiver
    {
        private const string strPeakX = "Peak X: ";
        private const string strPeakY = "Peak Y: ";
        private const string strPeakSmoothX = "PeakSmooth X: ";
        private const string strNote = "Note: ";
        private const string strYaw = "Yaw: ";
        private const string strPitch = "Pitch: ";
        private const string strRoll = "Roll: ";
        private const string strSpeed = "Speed: ";

        private ValueMapperDouble cnvMapX;
        private ValueMapperDouble cnvMapY;
        private double maxDb = 6000f;

        private Polyline spectrumLine;
        private readonly SolidColorBrush spectrumLineBrush = new SolidColorBrush(Colors.White);

        private Line peakRealLine;
        private readonly SolidColorBrush peakRealBrush = new SolidColorBrush(Colors.Blue);

        private Line peakSmoothLine;
        private readonly SolidColorBrush peakSmoothBrush = new SolidColorBrush(Colors.Red);

        private readonly SolidColorBrush failBrush = new SolidColorBrush(Colors.Red);
        private readonly SolidColorBrush okBrush = new SolidColorBrush(Colors.DarkGreen);

        private DispatcherTimer dispatcherTimer;
        private ISetup setup;

        private bool isSetup = false;

        #region Interface
        private double[] FftData;
        private int PeakRealX = 0;
        private int PeakSmoothX = 0;
        private double PeakRealY = 0;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            ScanWaveInSoundCards();
            ScanWaveOutSoundCards();
            AddComListElements();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = new TimeSpan(100000);
            dispatcherTimer.Tick += DispatcherTimer_Tick;

            RemapCanvas();
        }

        private void RemapCanvas()
        {
            //cnvMapX = new ValueMapperDouble(1028, cnvsPlot.ActualWidth);
            cnvMapX = new ValueMapperDouble(300, cnvsPlot.ActualWidth);
            cnvMapY = new ValueMapperDouble(maxDb, cnvsPlot.ActualHeight);
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (FftData != null)
                DrawFrame();
        }

        private void DrawFrame()
        {
            DrawSpectrumLine();
            DrawPeakReal();
            DrawPeakSmooth();
            UpdateLabels();
            UpdateKeysGrid();
        }

        private void UpdateKeysGrid()
        {
            R.TB.NoteKeysModule.SetOnlyALabel(R.TB.PitchRecognizerModule.BucketToMidiNoteOnPlayableList(R.TB.SpectrumAnalyzerModule.PeakBucketSmooth));
        }

        private void UpdateLabels()
        {
            lblPeaks.Content = strPeakX + PeakRealX.ToString() + "\n" +
                                strPeakY + PeakRealY.ToString() + "\n" +
                                strPeakSmoothX + PeakSmoothX.ToString() + "\n" +
                                strNote + R.TB.PitchRecognizerModule.BucketToAnyMidiNote(PeakRealX);

            lblHeadTracker.Content = strYaw + R.TB.HeadTrackerModule.Data.TranspYaw.ToString() + "\n" +
                                strPitch + R.TB.HeadTrackerModule.Data.TranspPitch.ToString() + "\n" +
                                strRoll + R.TB.HeadTrackerModule.Data.TranspRoll.ToString() + "\n" +
                                strSpeed + R.TB.Speed.ToString() + "\n";
        }

        private void DrawPeakSmooth()
        {
            if (peakSmoothLine != null)
            {
                cnvsPlot.Children.Remove(peakSmoothLine);
            }

            peakSmoothLine = new Line();
            peakSmoothLine.Stroke = peakSmoothBrush;
            peakSmoothLine.StrokeThickness = 1;

            double X = cnvMapX.Map(PeakSmoothX);
            peakSmoothLine.X1 = X;
            peakSmoothLine.Y1 = cnvsPlot.ActualHeight;
            peakSmoothLine.X2 = X;
            peakSmoothLine.Y2 = 0;

            cnvsPlot.Children.Add(peakSmoothLine);
        }

        private void DrawPeakReal()
        {
            if (peakRealLine != null)
            {
                cnvsPlot.Children.Remove(peakRealLine);
            }

            peakRealLine = new Line();
            peakRealLine.Stroke = peakRealBrush;
            peakRealLine.StrokeThickness = 1;

            double X = cnvMapX.Map(PeakRealX);
            peakRealLine.X1 = X;
            peakRealLine.Y1 = cnvsPlot.ActualHeight;
            peakRealLine.X2 = X;
            peakRealLine.Y2 = 0;

            cnvsPlot.Children.Add(peakRealLine);
        }

        private void DrawSpectrumLine()
        {
            if (spectrumLine != null)
            {
                cnvsPlot.Children.Remove(spectrumLine);
            }

            spectrumLine = new Polyline();
            spectrumLine.Stroke = spectrumLineBrush;
            spectrumLine.StrokeThickness = 1;

            cnvMapX.BaseMax = 300;


            for (int i = 0; i < 300; i++)
            {
                spectrumLine.Points.Add(new Point(cnvMapX.Map(i), cnvsPlot.ActualHeight - cnvMapY.Map(FftData[i])));
                if (FftData[i] > cnvMapY.BaseMax)
                {
                    //cnvMapY.BaseMax = dataFft[i];
                }
            }

            cnvsPlot.Children.Add(spectrumLine);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// Scans for soundcards to be put into the listbox for selection
        /// </summary>
        private void ScanWaveInSoundCards()
        {
            lstWaveInSoundCards.Items.Clear();
            for (int i = 0; i < WaveIn.DeviceCount; i++)
                lstWaveInSoundCards.Items.Add(WaveIn.GetCapabilities(i).ProductName);
            if (lstWaveInSoundCards.Items.Count > 0)
                lstWaveInSoundCards.SelectedIndex = 0;
            else
                MessageBox.Show("ERROR: no recording devices available");
        }

        private void ScanWaveOutSoundCards()
        {
            lstWaveOutSoundCards.Items.Clear();
            for (int i = 0; i < WaveOut.DeviceCount; i++)
                lstWaveOutSoundCards.Items.Add(WaveOut.GetCapabilities(i).ProductName);
            if (lstWaveOutSoundCards.Items.Count > 0)
                lstWaveOutSoundCards.SelectedIndex = 0;
            else
                MessageBox.Show("ERROR: no output devices available");
        }

        private void AddComListElements()
        {
            for(int i = 0; i < 20; i++)
            {
                lstComSensorPort.Items.Add(i);
            }
            
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!isSetup)
            {
                setup = new SetupDefault(this);
                setup.Setup();
                isSetup = true;
            }
            R.TB.AudioModule.WaveInActive = true;
            R.TB.AudioModule.WaveOutActive = true;
            R.TB.FftModule.Start();
            
            dispatcherTimer.Start();
            UpdateMidiPortLabel();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            R.TB.FftModule.Stop();
            R.TB.AudioModule.WaveInActive = false;
            R.TB.AudioModule.WaveOutActive = false;
            dispatcherTimer.Stop();
        }

        private void TestFunction()
        {
            spectrumLine = new Polyline();
            spectrumLine.Stroke = spectrumLineBrush;
            spectrumLine.StrokeThickness = 4;

            spectrumLine.Points.Add(new Point(20, 20));
            spectrumLine.Points.Add(new Point(30, 30));

            cnvsPlot.Children.Add(spectrumLine);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RemapCanvas();
        }

        void IPeakIntensityReceiver.ReceivePeakIntensity(double peakIntensity)
        {
            PeakRealY = peakIntensity;
        }

        void IPeakBucketRealReceiver.ReceivePeakBucketReal(int peakBucketReal)
        {
            PeakRealX = peakBucketReal;
        }

        void IPeakBucketSmoothReceiver.ReceivePeakBucketSmooth(int peakBucketSmooth)
        {
            PeakSmoothX = peakBucketSmooth;
        }

        void IFftDataReceiver.ReceiveFFTData(double[] fftData)
        {
            FftData = fftData;
        }

        private void btnMidiPortPlus_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                R.TB.MidiModule.OutDevice++;
                UpdateMidiPortLabel();
            }
        }

        private void btnMidiPortMinus_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                R.TB.MidiModule.OutDevice--;
                UpdateMidiPortLabel();
            }
        }

        private void UpdateMidiPortLabel()
        {
            lblMidiPort.Content = R.TB.MidiModule.OutDevice.ToString();
            switch (R.TB.MidiModule.IsMidiOk())
            {
                case true:
                    lblMidiPort.Foreground = okBrush;
                    break;
                case false:
                    lblMidiPort.Foreground = failBrush;
                    break;
                default:
                    break;
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(R.TB.PlayableNotes.Count.ToString());
        }

        private void cbxPlaySines_Click(object sender, RoutedEventArgs e)
        {

        }

        private void lstWaveInSoundCards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isSetup)
            {
                R.TB.AudioModule.WaveInDeviceIndex = lstWaveInSoundCards.SelectedIndex;
            }
        }

        private void lstWaveOutSoundCards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isSetup)
            {
                R.TB.AudioModule.WaveOutDeviceIndex = lstWaveOutSoundCards.SelectedIndex;
            }
        }

        private void comSensorPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            R.TB.HeadTrackerModule.Connect(lstComSensorPort.SelectedIndex);
            if (R.TB.HeadTrackerModule.IsConnectionOk)
            {
                lstComSensorPort.Foreground = okBrush;
            }
            else
            {
                lstComSensorPort.Foreground = failBrush;
            }
        }

        private void btnCenter_Click(object sender, RoutedEventArgs e)
        {
            R.TB.HeadTrackerModule.Data.SetDeltaForAll();
        }
    }
}
