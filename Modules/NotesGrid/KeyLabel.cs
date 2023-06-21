using NeeqDMIs.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Resin.DMIBox;

namespace Resin.Modules.NotesGrid
{
    public class KeyLabel : Label
    {
        public MidiNotes Note { get; set; }
        public KeyLabel(MidiNotes note)
        {
            Note = note;
            Content = note.ToStandardString();
            Foreground = R.KeyLabelFontColor;
            FontWeight = FontWeights.Bold;
            Background = R.KeyLabelOffBrush;
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;
            this.HorizontalContentAlignment = HorizontalAlignment.Center;
            this.VerticalContentAlignment = VerticalAlignment.Center;
        }

        public void Set(bool value)
        {
            switch (value)
            {
                case true:
                    Background = R.KeyLabelOnBrush;
                    break;
                case false:
                    Background = R.KeyLabelOffBrush;
                    break;
                default:
                    break;
            }
        }
    }
}
