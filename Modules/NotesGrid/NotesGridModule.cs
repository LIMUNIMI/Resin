using NITHdmis.Music;
using Resin.DataTypes;
using Resin.DMIBox;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Resin.Modules.NotesGrid
{
    public class NotesGridModule
    {
        private List<KeyCheckBox> KeyCheckBoxes;
        private List<KeyLabel> KeyLabels;
        private List<KeyRadioButton> KeyRadioButtons;
        private List<KeySlider> KeySliders;
        private MainWindow mainWindow;
        private Border nanBorder;
        private Grid notesGrid;
        public MidiNotes FirstNote { get; set; }
        public int KeysNumber { get; set; }

        public NotesGridModule(MainWindow mainWindow, Grid notesGrid, Border nanBorder, int keysNumber = 36, MidiNotes firstNote = MidiNotes.C4)
        {
            this.notesGrid = notesGrid;
            KeysNumber = keysNumber;
            FirstNote = firstNote;
            this.nanBorder = nanBorder;
            this.mainWindow = mainWindow;
        }

        public void GenerateKeys()
        {
            KeyLabels = new List<KeyLabel>();
            KeyCheckBoxes = new List<KeyCheckBox>();
            KeyRadioButtons = new List<KeyRadioButton>();
            KeySliders = new List<KeySlider>();
            int genPitch = (int)FirstNote;

            // POPULATE NANBORDER
            KeyRadioButton nanRadioButton = new KeyRadioButton(new KeyLabel(MidiNotes.NaN));
            nanRadioButton.Click += KeyRadioButton_Click;
            nanRadioButton.HorizontalAlignment = HorizontalAlignment.Center;
            nanRadioButton.VerticalAlignment = VerticalAlignment.Center;
            nanRadioButton.IsChecked = true;
            nanBorder.Child = nanRadioButton;

            // POPULATE GRID
            for (int i = 0; i < KeysNumber; i++)
            {
                notesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                MidiNotes note = PitchUtils.PitchToMidiNote(genPitch + i);
                KeyLabel keyLabel = new KeyLabel(note);
                Grid.SetColumn(keyLabel, i);
                Grid.SetRow(keyLabel, 0);
                KeyLabels.Add(keyLabel);

                KeyCheckBox keyCheckBox = new KeyCheckBox(keyLabel);
                keyCheckBox.Click += KeyCheckBox_Click;
                Grid.SetColumn(keyCheckBox, i);
                Grid.SetRow(keyCheckBox, 1);
                KeyCheckBoxes.Add(keyCheckBox);

                KeySlider keySlider = new KeySlider(keyLabel, this);
                Grid.SetColumn(keySlider, i);
                Grid.SetRow(keySlider, 2);
                KeySliders.Add(keySlider);

                KeyRadioButton keyRadioButton = new KeyRadioButton(keyLabel);
                keyRadioButton.Click += KeyRadioButton_Click;
                Grid.SetColumn(keyRadioButton, i);
                Grid.SetRow(keyRadioButton, 3);
                KeyRadioButtons.Add(keyRadioButton);
            }

            foreach (KeyLabel kb in KeyLabels)
            {
                notesGrid.Children.Add(kb);
            }

            foreach (KeyCheckBox kcb in KeyCheckBoxes)
            {
                notesGrid.Children.Add(kcb);
            }

            foreach (KeySlider ksl in KeySliders)
            {
                notesGrid.Children.Add(ksl);
            }

            foreach (KeyRadioButton krb in KeyRadioButtons)
            {
                notesGrid.Children.Add(krb);
            }
        }

        public void ResetAllLabels()
        {
            foreach (KeyLabel kl in KeyLabels)
            {
                kl.Set(false);
            }
        }

        public void SetCheckBox(MidiNotes note)
        {
            foreach (KeyCheckBox kcb in KeyCheckBoxes)
            {
                if (kcb.KeyLabel.Note == note)
                {
                    kcb.IsChecked = true;
                    UpdateCheckBox(kcb);
                }
            }
        }

        public void SetLabel(MidiNotes note)
        {
            foreach (KeyLabel kl in KeyLabels)
            {
                if (kl.Note == note)
                {
                    kl.Set(true);
                }
            }
        }

        public void SetOnlyALabel(MidiNotes note)
        {
            foreach (KeyLabel kl in KeyLabels)
            {
                if (kl.Note != note)
                {
                    kl.Set(false);
                }
                else
                {
                    kl.Set(true);
                }
            }
        }

        public void SetSlidersToBeUpdated()
        {
            mainWindow.SlidersMustBeUpdated = true;
        }

        public void UpdateAllGainSliders()
        {
            foreach (ResinNoteData nd in R.DMIbox.NoteDatas)
            {
                foreach (KeySlider slider in KeySliders)
                {
                    if (slider.KeyLabel.Note == nd.MidiNote)
                    {
                        slider.Value = nd.Out_Gain;
                    }
                }
            }
        }

        public void UpdateGainSlider(object sender)
        {
            KeySlider slider = (KeySlider)sender;
            R.DMIbox.SineCarpetModule.UpdateNoteGain(slider.KeyLabel.Note, slider.Value);
        }

        private static void UpdateCheckBox(object sender)
        {
            KeyCheckBox kcb = (KeyCheckBox)sender;

            if ((bool)kcb.IsChecked)
            {
                R.DMIbox.SetNote_Playable(kcb.KeyLabel.Note);
            }
            else
            {
                R.DMIbox.SetNote_NotPlayable(kcb.KeyLabel.Note);
            }

            R.DMIbox.SineCarpetModule.UpdateSinesAndSend();
            R.DMIbox.FftPlotModule.RemapCanvas();

            R.DMIbox.FftPlotModule.DrawPlayableNotesBins();
            R.DMIbox.SetBandPassToPlayableNotes();
        }

        private static void UpdateRadioButton(object sender)
        {
            KeyRadioButton krb = (KeyRadioButton)sender;

            if ((bool)krb.IsChecked)
            {
                R.DMIbox.SineCarpetModule.SingleNoteToCalibrate = krb.KeyLabel.Note;
            }
        }

        private void KeyCheckBox_Click(object sender, RoutedEventArgs e)
        {
            UpdateCheckBox(sender);
        }

        private void KeyRadioButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateRadioButton(sender);
        }

        private void KeySlider_Drop(object sender, DragEventArgs e)
        {
            UpdateGainSlider(sender);
        }
    }
}