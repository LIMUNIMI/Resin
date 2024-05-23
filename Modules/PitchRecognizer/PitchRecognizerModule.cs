using NITHdmis.Music;
using System;
using System.Collections.Generic;
using Resin.DataTypes;
using Resin.DMIBox;

namespace Resin.Modules.PitchRecognizer
{
    public class PitchRecognizerModule
    {
        public MidiNotes BinToAnyMidiNote(int bin)
        {
            MidiNotes result = MidiNotes.C4;
            int diff = 100000;
            int c;
            foreach (ResinNoteData nd in R.DMIbox.NoteDatas)
            {
                if(nd.In_LowBin <= bin && nd.In_HighBin >= bin)
                {
                    return nd.MidiNote;
                }
            }
            return result;
        }

        public MidiNotes BinToMidiNoteOnPlayableList(int bin)
        {
            MidiNotes result = MidiNotes.C4;
            int diff = 1000000;
            int c;
            foreach (ResinNoteData nd in R.DMIbox.GetPlayableNoteDatas())
            {
                if (nd.In_LowBin <= bin && nd.In_HighBin >= bin)
                {
                    return nd.MidiNote;
                }
            }
            return result;
        }
    }
}