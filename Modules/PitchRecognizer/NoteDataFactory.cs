using NITHdmis.Music;
using NITHdmis.Utils;
using System;
using System.Collections.Generic;
using Resin.DataTypes;
using NITHdmis.Audio.In;

namespace Resin.Modules.PitchRecognizer
{
    public static class NoteDataFactory
    {
        public const double DEFAULT_GAIN = 0.5f;
        public static List<ResinNoteData> EqualFakeBuckets(AudioInParameters audioFormatFft, MidiNotes firstNote, int firstNoteBin, int step)
        {
            var allNotes = Enum.GetValues(typeof(MidiNotes));
            List<ResinNoteData> Values = new List<ResinNoteData>();

            int it = firstNoteBin;

            bool start = false;

            foreach (MidiNotes mn in allNotes)
            {
                if (mn != MidiNotes.NaN)
                {
                    if (mn == firstNote)
                    {
                        start = true;
                    }

                    if (!start)
                    {
                        Values.Add(new ResinNoteData { MidiNote = mn, In_CenterBin = 0, Out_Frequency = 0, Out_Gain = DEFAULT_GAIN });
                    }
                    else
                    {
                        Values.Add(new ResinNoteData{ MidiNote = mn, In_CenterBin = it, Out_Frequency = audioFormatFft.BinToFrequency(it), Out_Gain = DEFAULT_GAIN });
                        it += step;
                    }
                }
            }

            return Values;
        }

        public static List<ResinNoteData> NaturalNotes(AudioInParameters audioFormatFft)
        {
            int SR = audioFormatFft.SampleRate;
            int FFTS = audioFormatFft.ZeroPaddedArrayLength;

            List<ResinNoteData> NoteDatas = new List<ResinNoteData>();

            var allNotes = Enum.GetValues(typeof(MidiNotes));

            foreach (MidiNotes mn in allNotes)
            {
                if (mn != MidiNotes.NaN)
                {
                    NoteDatas.Add(new ResinNoteData { MidiNote = mn, In_CenterBin = audioFormatFft.MidiNoteToBin(mn), Out_Frequency = mn.GetFrequency(), Out_Gain = DEFAULT_GAIN });
                }
            }

            return NoteDatas;
        }
    }
}