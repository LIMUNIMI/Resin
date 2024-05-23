using NITHdmis.MIDI;

namespace Resin.DMIBox
{
    public class PressureControl
    {
        private IMidiModule MidiModule;

        private int pressure = 0;

        public int MinimumPressure { get; set; } = 0;

        public int Pressure
        {
            get { return pressure; }
            set
            {
                pressure = value;
                if (pressure < MinimumPressure)
                {
                    pressure = MinimumPressure;
                }
                if (pressure > 127)
                {
                    pressure = 127;
                }
                if (pressure == 0)
                {
                    pressure = 0;
                }
                SetPressure();
            }
        }

        public PressureControl(IMidiModule midiModule, int minimumPressure = 0)
        {
            this.MinimumPressure = minimumPressure;
            this.MidiModule = midiModule;
        }

        private void SetPressure()
        {
            MidiModule.SetPressure(pressure);
        }
    }
}