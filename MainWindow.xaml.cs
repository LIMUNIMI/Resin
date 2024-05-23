using NAudio.Wave;
using NITHdmis.Music;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Resin.DMIBox;
using Resin.Setups;

namespace Resin
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ISineCalibrationListener
    {
        private SineCalibrationStatuses calibrationStatus = SineCalibrationStatuses.Finish;
        private DispatcherTimer dispatcherTimer;
        private int HTport = 1;
        private bool isSetup = false;
        private bool monitorEnabled = false;
        private ISetup setup;

        public bool SlidersMustBeUpdated { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ScanWaveInSoundCards();
            ScanWaveOutSoundCards();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = new TimeSpan(1000);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
        }

        void ISineCalibrationListener.ReceiveStatus(SineCalibrationStatuses status)
        {
            this.calibrationStatus = status;
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            R.DMIbox.FftPlotModule.UpdateFrame();
            UpdateLabels();
            UpdateKeysGrid();
            UpdateCalibrationStatus();
            if (SlidersMustBeUpdated)
            {
                UpdateNotesGridSliders();
                SlidersMustBeUpdated = false;
            }
        }

        private void HTportChanged()
        {
            lblHTport.Content = HTport.ToString();

            R.DMIbox.HeadTrackerModule.Connect(HTport);
            if (R.DMIbox.HeadTrackerModule.IsConnectionOk)
            {
                lblHTport.Foreground = R.GuiOkBrush;
            }
            else
            {
                lblHTport.Foreground = R.GuiFailBrush;
            }
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (isSetup)
                R.DMIbox.FftPlotModule.RemapCanvas();
        }

        private void MidiPortChanged()
        {
            lblMidiPort.Content = R.DMIbox.MidiModule.OutDevice.ToString();
            if (R.DMIbox.MidiModule.IsMidiOk())
            {
                lblMidiPort.Foreground = R.GuiOkBrush;
            }
            else
            {
                lblMidiPort.Foreground = R.GuiFailBrush;
            }
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

        private void sldCalibSetpoint_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            R.DMIbox.SineCarpetModule.CalibrationSetpoint = sldCalibSetpoint.Value;
            R.DMIbox.FftPlotModule.CalibrationSetpoint = R.DMIbox.SineCarpetModule.CalibrationSetpoint;
        }

        private void UpdateCalibrationStatus()
        {
            switch (calibrationStatus)
            {
                case SineCalibrationStatuses.Wait:
                    lblSineCalibrationLed.Background = R.GuiFailBrush;
                    break;

                case SineCalibrationStatuses.Finish:
                    lblSineCalibrationLed.Background = R.GuiOkBrush;
                    break;
            }
        }

        private void UpdateHTPortLabel()
        {
            lblHTport.Content = HTport.ToString();
        }

        private void UpdateKeysGrid()
        {
            R.DMIbox.NoteKeysModule.SetOnlyALabel(R.DMIbox.GetNoteMoreEnergeticData().MidiNote);
        }

        private void UpdateLabels()
        {
            if (monitorEnabled)
            {
                lblPeakX.Content = R.DMIbox.FftPlotModule.PeakRealX.ToString();
                lblPeakXsmooth.Content = R.DMIbox.FftPlotModule.PeakSmoothX.ToString();
                lblPeakY.Content = R.DMIbox.FftPlotModule.PeakRealY.ToString();
                lblNote.Content = R.DMIbox.PitchRecognizerModule.BinToAnyMidiNote(R.DMIbox.FftPlotModule.PeakRealX);

                lblHTpitch.Content = R.DMIbox.HeadTrackerModule.Data.CenteredPosition.Pitch.ToString();
                lblHTyaw.Content = R.DMIbox.HeadTrackerModule.Data.CenteredPosition.Yaw.ToString();
                lblHTroll.Content = R.DMIbox.HeadTrackerModule.Data.CenteredPosition.Roll.ToString();
                lblHTspeed.Content = R.DMIbox.Mon_Speed.ToString();
            }
        }

        private void UpdateNotesGridSliders()
        {
            R.DMIbox.NoteKeysModule.UpdateAllGainSliders();
        }

        private void UpdateWaveOutSlider()
        {
            if (isSetup)
            {
                sldAudioOutVolume.Value = R.DMIbox.SineCarpetModule.MasterVolume * 100f;
            }
        }

        #region Controls methods

        private bool isCalibratingEnergy = false;

        private void btnCenter_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                R.DMIbox.HeadTrackerModule.Data.SetCenterToCurrentPosition();
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            R.DMIbox.DisposeAll();
            Environment.Exit(0);
            //Application.Current.Shutdown();
        }

        private void btnHTportM_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                HTport--;
                HTportChanged();
            }
        }

        private void btnHTportP_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                HTport++;
                HTportChanged();
            }
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            if (!isSetup)
            {
                setup = new SetupDefault(this);
                setup.Setup();
                isSetup = true;

                dispatcherTimer.Start();

                MidiPortChanged();
                UpdateHTPortLabel();
                UpdateWaveOutSlider();

                btnInit.Background = R.GuiDisabledBackgroundBrush;
                btnInit.Foreground = R.GuiDisabledForegroundBrush;
            }
        }

        private void btnLoadCalib_Click(object sender, RoutedEventArgs e)
        {
            R.DMIbox.SineCarpetModule.LoadCalibration();
        }

        private void btnMajorScale_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                var notes = Enum.GetValues(typeof(MidiNotes));
                Scale CmajorScale = ScalesFactory.Cmaj;

                R.DMIbox.NoteKeysModule.ResetAllLabels();

                foreach (MidiNotes note in notes)
                {
                    if (note != MidiNotes.NaN)
                        if (CmajorScale.IsInScale(note.ToAbsNote()))
                        {
                            R.DMIbox.NoteKeysModule.SetCheckBox(note);
                        }
                }
            }
        }

        private void btnMidiPortMinus_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                R.DMIbox.MidiModule.OutDevice--;
                MidiPortChanged();
            }
        }

        private void btnMidiPortPlus_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                R.DMIbox.MidiModule.OutDevice++;
                MidiPortChanged();
            }
        }

        private void btnSaveCalib_Click(object sender, RoutedEventArgs e)
        {
            R.DMIbox.SineCarpetModule.SaveCalibration();
        }

        private void btnSineCal_EnergyClick(object sender, RoutedEventArgs e)
        {
            switch (isCalibratingEnergy)
            {
                case false:
                    R.DMIbox.FftPlotModule.NoteEnergiesDrawingMode = Modules.FFTplot.FFTplotModule.NoteEnergiesDrawingModes.All;
                    btnSineCal.Background = new SolidColorBrush(Colors.DarkRed);
                    isCalibratingEnergy = true;
                    R.DMIbox.SineCarpetModule.StartEnergyCalibration();
                    break;

                case true:
                    R.DMIbox.FftPlotModule.NoteEnergiesDrawingMode = Modules.FFTplot.FFTplotModule.NoteEnergiesDrawingModes.MoreEnergetic;
                    btnSineCal.Background = new SolidColorBrush(Colors.Black);
                    isCalibratingEnergy = false;
                    R.DMIbox.SineCarpetModule.StopEnergyCalibration();
                    break;
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            R.DMIbox.FftModule.Stop();
            R.DMIbox.WaveInModule.WaveInEnabled = false;
            R.DMIbox.SineCarpetModule.Enabled = false;
            dispatcherTimer.Stop();
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(R.DMIbox.AudioInParameters.FftPoints.ToString());
                
                MessageBox.Show(R.DMIbox.AudioInParameters.PcmDataLength.ToString());
                MessageBox.Show(R.DMIbox.AudioInParameters.ZeroPaddedArrayLength.ToString());
            if (isSetup)
            {
                R.DMIbox.NoteKeysModule.ResetAllLabels();

                R.DMIbox.NoteKeysModule.SetCheckBox(MidiNotes.C5);
                R.DMIbox.NoteKeysModule.SetCheckBox(MidiNotes.D5);
                R.DMIbox.NoteKeysModule.SetCheckBox(MidiNotes.E5);
                R.DMIbox.NoteKeysModule.SetCheckBox(MidiNotes.F5);
                R.DMIbox.NoteKeysModule.SetCheckBox(MidiNotes.G5);
                R.DMIbox.NoteKeysModule.SetCheckBox(MidiNotes.A5);
                R.DMIbox.NoteKeysModule.SetCheckBox(MidiNotes.B5);

                R.DMIbox.NoteKeysModule.SetCheckBox(MidiNotes.C6);
                R.DMIbox.NoteKeysModule.SetCheckBox(MidiNotes.D6);
                R.DMIbox.NoteKeysModule.SetCheckBox(MidiNotes.E6);
                R.DMIbox.NoteKeysModule.SetCheckBox(MidiNotes.F6);
                R.DMIbox.NoteKeysModule.SetCheckBox(MidiNotes.G6);
                R.DMIbox.NoteKeysModule.SetCheckBox(MidiNotes.A6);
                R.DMIbox.NoteKeysModule.SetCheckBox(MidiNotes.B6);

            }
        }

        private void cbxAudioIn_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                if ((bool)cbxAudioIn.IsChecked)
                {
                    R.DMIbox.WaveInModule.WaveInEnabled = true;
                    R.DMIbox.FftModule.Start();
                }
                else
                {
                    R.DMIbox.FftModule.Stop();
                    R.DMIbox.WaveInModule.WaveInEnabled = false;
                }
            }
        }

        private void cbxHeadtracker_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                switch (cbxHeadtracker.IsChecked)
                {
                    case true:
                        R.DMIbox.ControlMode = TongControlModes.Headtracker;
                        break;

                    case false:
                        R.DMIbox.ControlMode = TongControlModes.Keyboard;
                        break;
                }
            }
        }

        private void cbxMonitor_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                if ((bool)cbxMonitor.IsChecked)
                {
                    monitorEnabled = true;
                }
                else
                {
                    monitorEnabled = false;
                }
            }
        }

        private void cbxPlaySines_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                if ((bool)cbxPlaySines.IsChecked)
                {
                    R.DMIbox.SineCarpetModule.Enabled = true;
                    R.DMIbox.WaveOutMixerModule.Enabled = true;
                }
                else
                {
                    R.DMIbox.SineCarpetModule.Enabled = false;
                    R.DMIbox.WaveOutMixerModule.Enabled = false;
                }
            }
        }

        private void cbxPlot_Checked(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                R.DMIbox.FftPlotModule.Enabled = true;
                R.DMIbox.FftPlotModule.RemapCanvas();
            }
        }

        private void cbxPlot_Unchecked(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                R.DMIbox.FftPlotModule.Enabled = false;
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void lstWaveInSoundCards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isSetup)
            {
                R.DMIbox.WaveInModule.WaveInDeviceIndex = lstWaveInSoundCards.SelectedIndex;
            }
        }

        private void lstWaveOutSoundCards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isSetup)
            {
                R.DMIbox.WaveOutMixerModule.WaveOutDeviceIndex = lstWaveOutSoundCards.SelectedIndex;
            }
        }

        private void sldAudioOutVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isSetup)
            {
                R.DMIbox.SineCarpetModule.MasterVolume = (float)sldAudioOutVolume.Value / 100f;
            }
        }

        private void sldPanning_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isSetup)
            {
                R.DMIbox.SineCarpetModule.Panning = (float)sldPanning.Value;
            }
        }

        #endregion Controls methods
    }
}