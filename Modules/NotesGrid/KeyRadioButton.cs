using System.Windows.Controls;
using System.Windows.Media;
using Resin.DMIBox;

namespace Resin.Modules.NotesGrid
{
    public class KeyRadioButton : RadioButton
    {
        public KeyLabel KeyLabel { get; set; }

        public KeyRadioButton(KeyLabel keyLabel)
        {
            KeyLabel = keyLabel;
            GroupName = R.CalibrationRadioButtonsGroup;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.Background = new SolidColorBrush(Colors.SandyBrown);
        }
    }
}