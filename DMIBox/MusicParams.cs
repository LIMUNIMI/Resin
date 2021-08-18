using NeeqDMIs.MIDI;
using NeeqDMIs.Music;
using System.Collections.Generic;

namespace Resin.DMIBox
{
    public class MusicParams
    {
        #region Global

        private IMidiModule midiModule;

        public MusicParams(IMidiModule midiModule)
        {
            this.midiModule = midiModule;
        }

        #endregion Global

        #region Pressure

        private int pressure = 0;

        public int Pressure
        {
            get { return pressure; }
            set
            {
                pressure = value;

                if (pressure < 0)
                    pressure = 0;
                if (pressure > 127)
                    pressure = 127;
                if (pressure < Pressure_Min)
                    pressure = Pressure_Min;
                if (pressure > Pressure_Max)
                    pressure = Pressure_Max;
            }
        }

        public int Pressure_Max { get; set; } = 127;
        public int Pressure_Min { get; set; } = 0;

        public void Pressure_Set()
        {
            midiModule.SetPressure(pressure);
        }

        #endregion Pressure

        #region Velocity

        private int velocity;

        public int Velocity
        {
            get { return velocity; }
            set
            {
                velocity = value;

                if (velocity < 0)
                    velocity = 0;
                if (velocity > 127)
                    velocity = 127;
                if (velocity < Velocity_Min)
                    velocity = Velocity_Min;
                if (velocity > Velocity_Max)
                    velocity = Velocity_Max;
            }
        }

        public int Velocity_Max { get; set; } = 127;
        public int Velocity_Min { get; set; } = 0;

        #endregion Velocity

        #region Notes

        public List<MidiNotes> NotesOn = new List<MidiNotes>();
        public List<MidiNotes> NotesOn_Previous = new List<MidiNotes>();

        public void Notes_Add(List<MidiNotes> notes)
        {
            notes_savePrevious();

            foreach (MidiNotes note in notes)
            {
                midiModule.PlayNote((int)note, velocity);
                NotesOn.Add(note);
            }
        }

        public void Notes_Add(MidiNotes note)
        {
            notes_savePrevious();

            midiModule.PlayNote((int)note, velocity);
            NotesOn.Add(note);
        }

        public void Notes_Remove(MidiNotes note)
        {
            notes_savePrevious();

            midiModule.StopNote((int)note);
            NotesOn.Remove(note);
        }

        public void Notes_Remove(List<MidiNotes> notes)
        {
            notes_savePrevious();

            foreach (MidiNotes note in notes)
            {
                midiModule.StopNote((int)note);
                NotesOn.Remove(note);
            }
        }

        public void Notes_StopAll()
        {
            foreach (MidiNotes note in NotesOn)
            {
                midiModule.StopNote((int)note);
            }

            notes_savePrevious();
            NotesOn.Clear();
        }

        public void Notes_Switch(List<MidiNotes> notes)
        {
            foreach (MidiNotes note in NotesOn)
            {
                midiModule.StopNote((int)note);
            }

            notes_savePrevious();
            NotesOn.Clear();

            foreach (MidiNotes note in notes)
            {
                midiModule.PlayNote((int)note, velocity);
                NotesOn.Add(note);
            }
        }

        public void Notes_Switch(MidiNotes note)
        {
            foreach (MidiNotes mn in NotesOn)
            {
                midiModule.StopNote((int)mn);
            }

            notes_savePrevious();
            NotesOn.Clear();

            midiModule.PlayNote((int)note, velocity);
            NotesOn.Add(note);
        }

        private void notes_savePrevious()
        {
            NotesOn_Previous.Clear();
            foreach (MidiNotes note in NotesOn)
            {
                NotesOn_Previous.Add(note);
            }
        }

        #endregion Notes

        #region Expression

        private int expression;

        public int Expression
        {
            get { return expression; }
            set
            {
                expression = value;

                if (expression < 0)
                    expression = 0;
                if (expression > 127)
                    expression = 127;
                if (expression < Expression_Min)
                    expression = Expression_Min;
                if (expression > Expression_Max)
                    expression = Expression_Max;
            }
        }

        public int Expression_Max { get; set; } = 127;
        public int Expression_Min { get; set; } = 0;

        public void Expression_Set()
        {
            midiModule.SetExpression(expression);
        }

        #endregion Expression
    }
}