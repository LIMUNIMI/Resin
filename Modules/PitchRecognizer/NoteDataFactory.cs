using NeeqDMIs.Music;
using NeeqDMIs.Utils;
using System;
using System.Collections.Generic;
using Resin.DataTypes;

namespace Resin.Modules.PitchRecognizer
{
    public static class NoteDataFactory
    {
        public const double DEFAULT_GAIN = 0.5f;
        public static List<NoteData> EqualFakeBuckets(AudioInParameters audioFormatFft, MidiNotes firstNote, int firstNoteBin, int step)
        {
            var allNotes = Enum.GetValues(typeof(MidiNotes));
            List<NoteData> Values = new List<NoteData>();

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
                        Values.Add(new NoteData { MidiNote = mn, In_CenterBin = 0, Out_Frequency = 0, Out_Gain = DEFAULT_GAIN });
                    }
                    else
                    {
                        Values.Add(new NoteData{ MidiNote = mn, In_CenterBin = it, Out_Frequency = audioFormatFft.BinToFrequency(it), Out_Gain = DEFAULT_GAIN });
                        it += step;
                    }
                }
            }

            return Values;
        }

        public static List<NoteData> NaturalNotes(AudioInParameters audioFormatFft)
        {
            int SR = audioFormatFft.SampleRate;
            int FFTS = audioFormatFft.ZeroPaddedArrayLength;

            List<NoteData> NoteDatas = new List<NoteData>();

            var allNotes = Enum.GetValues(typeof(MidiNotes));

            foreach (MidiNotes mn in allNotes)
            {
                if (mn != MidiNotes.NaN)
                {
                    NoteDatas.Add(new NoteData { MidiNote = mn, In_CenterBin = audioFormatFft.MidiNoteToBin(mn), Out_Frequency = mn.GetFrequency(), Out_Gain = DEFAULT_GAIN });
                }
            }

            return NoteDatas;
        }
    }
}