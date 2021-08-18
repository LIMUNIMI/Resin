using NAudio.Wave;
using NeeqDMIs.Music;
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
            dispatcherTimer.Interval = new TimeSpan(100000);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
        }

        void ISineCalibrationListener.ReceiveStatus(SineCalibrationStatuses status)
        {
            this.calibrationStatus = status;
        }

        private void cbxPlaySines_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            G.TB.FFTplotModule.UpdateFrame();
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

            G.TB.HeadTrackerModule.Connect(HTport);
            if (G.TB.HeadTrackerModule.IsConnectionOk)
            {
                lblHTport.Foreground = G.GuiOkBrush;
            }
            else
            {
                lblHTport.Foreground = G.GuiFailBrush;
            }
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (isSetup)
                G.TB.FFTplotModule.RemapCanvas();
        }

        private void MidiPortChanged()
        {
            lblMidiPort.Content = G.TB.MidiModule.OutDevice.ToString();
            if (G.TB.MidiModule.IsMidiOk())
            {
                lblMidiPort.Foreground = G.GuiOkBrush;
            }
            else
            {
                lblMidiPort.Foreground = G.GuiFailBrush;
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
            G.TB.AudioModule.CalibrationSetpoint = sldCalibSetpoint.Value;
            G.TB.FFTplotModule.CalibrationSetpoint = G.TB.AudioModule.CalibrationSetpoint;
        }

        private void UpdateCalibrationStatus()
        {
            switch (calibrationStatus)
            {
                case SineCalibrationStatuses.Wait:
                    lblSineCalibrationLed.Background = G.GuiFailBrush;
                    break;

                case SineCalibrationStatuses.Finish:
                    lblSineCalibrationLed.Background = G.GuiOkBrush;
                    break;
            }
        }

        private void UpdateHTPortLabel()
        {
            lblHTport.Content = HTport.ToString();
        }

        private void UpdateKeysGrid()
        {
            G.TB.NoteKeysModule.SetOnlyALabel(G.TB.GetNoteMoreEnergeticData().MidiNote);
        }

        private void UpdateLabels()
        {
            if (monitorEnabled)
            {
                lblPeakX.Content = G.TB.FFTplotModule.PeakRealX.ToString();
                lblPeakXsmooth.Content = G.TB.FFTplotModule.PeakSmoothX.ToString();
                lblPeakY.Content = G.TB.FFTplotModule.PeakRealY.ToString();
                lblNote.Content = G.TB.PitchRecognizerModule.BinToAnyMidiNote(G.TB.FFTplotModule.PeakRealX);

                lblHTpitch.Content = G.TB.HeadTrackerModule.Data.TranspPitch.ToString();
                lblHTyaw.Content = G.TB.HeadTrackerModule.Data.TranspYaw.ToString();
                lblHTroll.Content = G.TB.HeadTrackerModule.Data.TranspRoll.ToString();
                lblHTspeed.Content = G.TB.Mon_Speed.ToString();
            }
        }

        private void UpdateNotesGridSliders()
        {
            G.TB.NoteKeysModule.UpdateAllGainSliders();
        }

        private void UpdateWaveOutSlider()
        {
            if (isSetup)
            {
                sldAudioOutVolume.Value = G.TB.AudioModule.WaveOutMasterVolume * 100f;
            }
        }

        #region Controls methods

        private bool isCalibratingEnergy = false;

        private void btnCenter_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                G.TB.HeadTrackerModule.Data.CalibrateCenter();
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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

                btnInit.Background = G.GuiDisabledBackgroundBrush;
                btnInit.Foreground = G.GuiDisabledForegroundBrush;
            }
        }

        private void btnLoadCalib_Click(object sender, RoutedEventArgs e)
        {
            G.TB.AudioModule.LoadCalibration();
        }

        private void btnMajorScale_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                var notes = Enum.GetValues(typeof(MidiNotes));
                Scale CmajorScale = ScalesFactory.Cmaj;

                G.TB.NoteKeysModule.ResetAllLabels();

                foreach (MidiNotes note in notes)
                {
                    if (note != MidiNotes.NaN)
                        if (CmajorScale.IsInScale(note.ToAbsNote()))
                        {
                            G.TB.NoteKeysModule.SetCheckBox(note);
                        }
                }
            }
        }

        private void btnMidiPortMinus_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                G.TB.MidiModule.OutDevice--;
                MidiPortChanged();
            }
        }

        private void btnMidiPortPlus_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                G.TB.MidiModule.OutDevice++;
                MidiPortChanged();
            }
        }

        private void btnSaveCalib_Click(object sender, RoutedEventArgs e)
        {
            G.TB.AudioModule.SaveCalibration();
        }

        private void btnSineCal_EnergyClick(object sender, RoutedEventArgs e)
        {
            switch (isCalibratingEnergy)
            {
                case false:
                    G.TB.FFTplotModule.NoteEnergiesDrawingMode = Modules.FFTplot.FFTplotModule.NoteEnergiesDrawingModes.All;
                    btnSineCal.Background = new SolidColorBrush(Colors.DarkRed);
                    isCalibratingEnergy = true;
                    G.TB.AudioModule.StartEnergyCalibration();
                    break;

                case true:
                    G.TB.FFTplotModule.NoteEnergiesDrawingMode = Modules.FFTplot.FFTplotModule.NoteEnergiesDrawingModes.MoreEnergetic;
                    btnSineCal.Background = new SolidColorBrush(Colors.Black);
                    isCalibratingEnergy = false;
                    G.TB.AudioModule.StopEnergyCalibration();
                    break;
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            G.TB.FftModule.Stop();
            G.TB.AudioModule.WaveInEnabled = false;
            G.TB.AudioModule.WaveOutEnabled = false;
            dispatcherTimer.Stop();
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                G.TB.NoteKeysModule.ResetAllLabels();

                G.TB.NoteKeysModule.SetCheckBox(MidiNotes.C5);
                G.TB.NoteKeysModule.SetCheckBox(MidiNotes.D5);
                G.TB.NoteKeysModule.SetCheckBox(MidiNotes.E5);
                G.TB.NoteKeysModule.SetCheckBox(MidiNotes.F5);
                G.TB.NoteKeysModule.SetCheckBox(MidiNotes.G5);
                G.TB.NoteKeysModule.SetCheckBox(MidiNotes.A5);
                G.TB.NoteKeysModule.SetCheckBox(MidiNotes.B5);

                G.TB.NoteKeysModule.SetCheckBox(MidiNotes.C6);
                G.TB.NoteKeysModule.SetCheckBox(MidiNotes.D6);
                G.TB.NoteKeysModule.SetCheckBox(MidiNotes.E6);
                G.TB.NoteKeysModule.SetCheckBox(MidiNotes.F6);
                G.TB.NoteKeysModule.SetCheckBox(MidiNotes.G6);
                G.TB.NoteKeysModule.SetCheckBox(MidiNotes.A6);
                G.TB.NoteKeysModule.SetCheckBox(MidiNotes.B6);

            }
        }

        private void cbxAudioIn_Click(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                if ((bool)cbxAudioIn.IsChecked)
                {
                    G.TB.AudioModule.WaveInEnabled = true;
                    G.TB.FftModule.Start();
                }
                else
                {
                    G.TB.FftModule.Stop();
                    G.TB.AudioModule.WaveInEnabled = false;
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
                        G.TB.ControlMode = TongControlModes.Headtracker;
                        break;

                    case false:
                        G.TB.ControlMode = TongControlModes.Keyboard;
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
                    G.TB.AudioModule.WaveOutEnabled = true;
                }
                else
                {
                    G.TB.AudioModule.WaveOutEnabled = false;
                }
            }
        }

        private void cbxPlot_Checked(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                G.TB.FFTplotModule.Enabled = true;
                G.TB.FFTplotModule.RemapCanvas();
            }
        }

        private void cbxPlot_Unchecked(object sender, RoutedEventArgs e)
        {
            if (isSetup)
            {
                G.TB.FFTplotModule.Enabled = false;
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void lstWaveInSoundCards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isSetup)
            {
                G.TB.AudioModule.WaveInDeviceIndex = lstWaveInSoundCards.SelectedIndex;
            }
        }

        private void lstWaveOutSoundCards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isSetup)
            {
                G.TB.AudioModule.WaveOutDeviceIndex = lstWaveOutSoundCards.SelectedIndex;
            }
        }

        private void sldAudioOutVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isSetup)
            {
                G.TB.AudioModule.WaveOutMasterVolume = (float)sldAudioOutVolume.Value / 100f;
            }
        }

        private void sldPanning_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isSetup)
            {
                G.TB.AudioModule.Panning = (float)sldPanning.Value;
            }
        }

        #endregion Controls methods
    }
}