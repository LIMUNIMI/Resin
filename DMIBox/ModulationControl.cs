using NeeqDMIs.MIDI;

namespace Resin.DMIBox
{
    public class ModulationControl
    {
        private IMidiModule MidiModule;
        private int modulation = 0;

        public int Modulation
        {
            get { return modulation; }
            set
            {
                if (value < 50 && value > 1)
                {
                    modulation = 50;
                }
                else if (value > 127)
                {
                    modulation = 127;
                }
                else if (value == 0)
                {
                    modulation = 0;
                }
                else
                {
                    modulation = value;
                }
                SetModulation();
            }
        }

        public ModulationControl(IMidiModule midiModule)
        {
            MidiModule = midiModule;
        }

        private void SetModulation()
        {
            MidiModule.SetModulation(Modulation);
        }
    }
}