using NeeqDMIs.Music;

namespace Resin.DataTypes
{
    public class NoteData
    {
        public const double DEFAULT_OUTGAIN = 0.8f;
        private const bool DEFAULT_ISPLAYABLE = false;

        public bool IsPlayable { get; set; } = DEFAULT_ISPLAYABLE;
        public MidiNotes MidiNote { get; set; }

        #region Input

        public int In_CenterBin { get; set; }
        public int In_Dimension { get; set; }
        public double In_Energy { get; set; }
        public double In_Frequency { get; set; }
        public int In_HighBin { get; set; }
        public int In_LowBin { get; set; }

        #endregion Input

        #region Output

        public double Out_Frequency { get; set; }
        public double Out_Gain { get; set; } = DEFAULT_OUTGAIN;

        #endregion Output
    }
}