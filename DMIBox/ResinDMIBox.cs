using NAudio.CoreAudioApi;
using NAudio.Wave;
using NITHdmis.Audio.In;
using NITHdmis.Audio.Out;
using NITHdmis.Filters.ValueFilters;
using NITHdmis.Headtracking.NeeqHT;
using NITHdmis.Keyboard;
using NITHdmis.MIDI;
using NITHdmis.Music;
using NITHdmis.Utils;
using Resin.DataTypes;
using Resin.Modules.Audio;
using Resin.Modules.FFT;
using Resin.Modules.FFTplot;
using Resin.Modules.NotesGrid;
using Resin.Modules.PitchRecognizer;
using Resin.Modules.SpectrumAnalyzer;
using Resin.Setups.Behaviors;
using Resin.Setups.SpectrumFilters;
using System;
using System.Collections.Generic;

namespace Resin.DMIBox
{
    public class ResinDMIBox
    {
        #region Modules
        public IMidiModule MidiModule { get; set; }
        public WaveInModule WaveInModule { get; set; }
        public WaveOutDeviceMixerModule WaveOutMixerModule { get; set; }
        public SineCarpetModule SineCarpetModule { get; set; }
        public FFTModule FftModule { get; set; }
        public FFTplotModule FftPlotModule { get; set; }
        public NeeqHTModule HeadTrackerModule { get; set; }
        public KeyboardModule KeyboardModule { get; set; }
        public MainWindow MainWindow { get; set; }
        public NotesGridModule NoteKeysModule { get; set; }
        public PitchRecognizerModule PitchRecognizerModule { get; set; }
        public SpectrumAnalyzerModule SpectrumAnalyzerModule { get; set; }
        public SineCarpetModule EnergyCalibrationModule { get; set; }

        #endregion Modules

        #region Monitors

        public MidiNotes Mon_CurrentNote { get; set; }
        public double Mon_Speed { get; set; }

        #endregion Monitors

        #region Control modes

        private TongControlModes controlMode = R.DefaultControlMode;
        private HByawStrumming hbYawStrumming;
        private KBplayNote kbPlayNote;
        private KBsetExpression kbSetExpression;
        private List<IToggleableBehavior> toggleableBehaviors = new List<IToggleableBehavior>();

        public TongControlModes ControlMode
        {
            get { return controlMode; }
            set
            {
                controlMode = value;
                OnControlModeChange();
            }
        }

        public void InitializeBehaviors()
        {
            kbPlayNote = new KBplayNote();
            kbSetExpression = new KBsetExpression();
            hbYawStrumming = new HByawStrumming();

            toggleableBehaviors.Add(kbPlayNote);
            toggleableBehaviors.Add(kbSetExpression);
            toggleableBehaviors.Add(hbYawStrumming);

            KeyboardModule.KeyboardBehaviors.Add(kbPlayNote);
            KeyboardModule.KeyboardBehaviors.Add(kbSetExpression);
            HeadTrackerModule.Behaviors.Add(hbYawStrumming);

            OnControlModeChange();
        }

        private void OnControlModeChange()
        {
            turnOffAllControlModes();

            switch (controlMode)
            {
                case TongControlModes.Keyboard:
                    kbPlayNote.Switch(true);
                    kbSetExpression.Switch(true);

                    MusicParameters.Pressure_Min = KEYBOARD_PRESSURE_MIN;
                    MusicParameters.Pressure = 127;
                    break;

                case TongControlModes.Headtracker:
                    hbYawStrumming.Switch(true);

                    MusicParameters.Pressure_Min = HEAD_TRACKER_PRESSURE_MIN;
                    break;
            }
        }

        private void turnOffAllControlModes()
        {
            foreach (IToggleableBehavior tb in toggleableBehaviors)
            {
                tb.Switch(false);
            }
        }

        #endregion Control modes

        #region Mapping

        private const int HEAD_TRACKER_PRESSURE_MIN = 0;
        private const int KEYBOARD_PRESSURE_MIN = 0;

        private const double VELOCITY_MULTIPLIER = 2.5f;
        private const double PRESSURE_MULTIPLIER = 10f;

        public MusicParams MusicParameters { get; set; }

        public void SwitchToNoteMoreEnergetic()
        {
            var n = GetNoteMoreEnergeticData();
            Mon_CurrentNote = n.MidiNote;
            MusicParameters.Notes_Switch(n.MidiNote);
        }

        #endregion Mapping

        #region Keyboard Logic

        public void Keyboard_ReceiveStrum()
        {
            MusicParameters.Velocity = 127;
            SwitchToNoteMoreEnergetic();
        }

        public void Keyboard_SetExpressionToZero()
        {
            MusicParameters.Expression = 0;
            MusicParameters.Expression_Set();
        }

        #endregion Keyboard Logic

        #region Headtracker Logic

        public ISpectrumFilter PlayableNotesBandPass = new PreciseBandPass(0, 1);
        private readonly ValueMapperDouble PressureMapper = new ValueMapperDouble(0.5f, 127);
        private readonly IDoubleFilter SpeedFilter = new DoubleFilterMAExpDecaying(0.8f);
        private readonly ValueMapperDouble YawMapper = new ValueMapperDouble(25, 127);
        private const int DEADSPEED = 40;
        private double currentYaw = 0;
        private Directions direction = Directions.Left;
        private double maxYaw;
        private double minYaw;

        private double previousYaw = 0;

        private double yawSpeed;

        private double yawSpeedFiltered;

        public double HTDeadZone
        {
            get { return maxYaw; }
            set { maxYaw = value; minYaw = -value; }
        }

        public bool InDeadZone { get; private set; }

        public void HTStrum_ElaboratePosition(NeeqHTData htData)
        {
            previousYaw = currentYaw;
            currentYaw = htData.CenteredPosition.Yaw;

            HTProcessSpeed();

            if (true)//IsOutsideDeadzone())
            {
                if (currentYaw > previousYaw && direction == Directions.Left) // && currentYaw < 0)
                {
                    direction = Directions.Right;
                    //MusicParameters.Velocity = (int)YawMapper.Map(Math.Abs(currentYaw));
                    MusicParameters.Velocity = (int)(yawSpeedFiltered * VELOCITY_MULTIPLIER - DEADSPEED);
                    SwitchToNoteMoreEnergetic();
                }
                if (currentYaw < previousYaw && direction == Directions.Right)// && currentYaw > 0)
                {
                    direction = Directions.Left;
                    //MusicParameters.Velocity = (int)YawMapper.Map(Math.Abs(currentYaw));
                    MusicParameters.Velocity = (int)(yawSpeedFiltered * VELOCITY_MULTIPLIER - DEADSPEED);
                    SwitchToNoteMoreEnergetic();
                }
            }
        }

        public void SetBandPassToPlayableNotes()
        {
            List<ResinNoteData> pnd = GetPlayableNoteDatas();

            if (pnd.Count > 0)
            {
                if (FftModule.SpectrumFilters.Contains(PlayableNotesBandPass))
                {
                    FftModule.SpectrumFilters.Remove(PlayableNotesBandPass);
                }

                PlayableNotesBandPass = new PreciseBandPass(pnd[0].In_LowBin, pnd[pnd.Count - 1].In_HighBin);
                FftModule.SpectrumFilters.Add(PlayableNotesBandPass);
            }
        }

        private void HTProcessSpeed()
        {
            yawSpeed = Math.Abs(currentYaw - previousYaw);
            SpeedFilter.Push(yawSpeed);
            yawSpeedFiltered = PressureMapper.Map(SpeedFilter.Pull());

            yawSpeedFiltered = (Math.Log(yawSpeedFiltered, 1.5f) * PRESSURE_MULTIPLIER);
            
            MusicParameters.Pressure = (int)yawSpeedFiltered - DEADSPEED;
            MusicParameters.Expression = (int)yawSpeedFiltered - DEADSPEED;
            Mon_Speed = MusicParameters.Pressure;
            MusicParameters.Pressure_Set();
            MusicParameters.Expression_Set();
        }

        private bool IsOutsideDeadzone()
        {
            if (currentYaw < -HTDeadZone || currentYaw > HTDeadZone)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public enum HTStrumModes
        {
            Continuous,
            DistanceStrum
        }

        private enum Directions
        {
            Left,
            Right
        }

        #endregion Headtracker Logic

        #region Notes

        private List<ResinNoteData> noteDatas;

        public List<ResinNoteData> NoteDatas
        {
            get
            {
                return noteDatas;
            }
            set
            {
                noteDatas = value;
            }
        }

        public int NoteMoreEnergeticDataIndex { get; set; } = 0;

        public ResinNoteData GetNoteMoreEnergeticData()
        {
            return NoteDatas[NoteMoreEnergeticDataIndex];
        }

        public List<ResinNoteData> GetPlayableNoteDatas()
        {
            List<ResinNoteData> pndlist = new List<ResinNoteData>();
            foreach (ResinNoteData nd in NoteDatas)
            {
                if (nd.IsPlayable)
                {
                    pndlist.Add(nd);
                }
            }
            return pndlist;
        }

        public void SetNote_NotPlayable(MidiNotes note)
        {
            foreach (ResinNoteData nd in NoteDatas)
            {
                if (nd.MidiNote == note)
                {
                    nd.IsPlayable = false;
                }

                UpdateBinBoundaries();
            }
        }

        public void SetNote_NotPlayable(List<MidiNotes> noteList)
        {
            foreach (MidiNotes note in noteList)
            {
                foreach (ResinNoteData nd in NoteDatas)
                {
                    if (nd.MidiNote == note)
                    {
                        nd.IsPlayable = false;
                    }
                }
            }

            UpdateBinBoundaries();
        }

        public void SetNote_Playable(MidiNotes note)
        {
            foreach (ResinNoteData nd in NoteDatas)
            {
                if (nd.MidiNote == note)
                {
                    nd.IsPlayable = true;
                }

                UpdateBinBoundaries();
            }
        }

        public void SetNote_Playable(List<MidiNotes> noteList)
        {
            foreach (MidiNotes note in noteList)
            {
                foreach (ResinNoteData nd in NoteDatas)
                {
                    if (nd.MidiNote == note)
                    {
                        nd.IsPlayable = true;
                    }
                }

                UpdateBinBoundaries();
            }
        }

        public void UpdateBinBoundaries()
        {
            for (int i = 0; i < noteDatas.Count; i++)
            {
                int dUp;
                int dDo;

                if (noteDatas.Count != 1)
                {
                    if (i == 0)  // First element special case
                    {
                        dUp = (int)((noteDatas[i + 1].In_CenterBin - noteDatas[i].In_CenterBin) / 2);
                        dDo = dUp;
                    }
                    else if (i == noteDatas.Count - 1) // Last element special case
                    {
                        dDo = (int)((noteDatas[i].In_CenterBin - noteDatas[i - 1].In_CenterBin) / 2);
                        dUp = dDo;
                    }
                    else
                    {
                        dUp = (int)((noteDatas[i + 1].In_CenterBin - noteDatas[i].In_CenterBin) / 2);
                        dDo = (int)((noteDatas[i].In_CenterBin - noteDatas[i - 1].In_CenterBin) / 2);
                    }
                }
                else
                {
                    dUp = 1;
                    dDo = 1;
                }

                noteDatas[i].In_HighBin = noteDatas[i].In_CenterBin + dUp;
                noteDatas[i].In_LowBin = noteDatas[i].In_CenterBin - dDo;
                noteDatas[i].In_Dimension = noteDatas[i].In_HighBin - noteDatas[i].In_LowBin + 1;
            }
        }

        private ResinNoteData GetNoteData(MidiNotes note)
        {
            foreach (ResinNoteData and in R.DMIbox.NoteDatas)
            {
                if (and.MidiNote == note)
                {
                    return and;
                }
            }

            // If note not found
            throw new Exception("TongBox: GetNoteData error for note " + note.ToStandardString());
        }

        #endregion Notes

        #region AudioIn Unification

        private AudioInParameters audioInParameters;
        public AudioInParameters AudioInParameters
        {
            get { return audioInParameters; } 
            set
            {
                audioInParameters = value;
                WaveInModule?.ReceiveAudioInParams(audioInParameters);
                FftModule?.ReceiveAudioInParams(audioInParameters);
                NoteDatas = NoteDataFactory.NaturalNotes(audioInParameters);
            }
        }

        #endregion

        public List<IDisposable> Disposables { get; set; } = new List<IDisposable>();

        public void DisposeAll()
        {
            foreach(IDisposable disposable in Disposables)
            {
                disposable.Dispose();
            }
        }


    }
}