using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Resin.Modules.NotesGrid
{
    public class KeySlider : Slider
    {
        public KeyLabel KeyLabel { get; set; }

        private const float MINIMUMGAIN = 0f;
        private const float MAXIMUMGAIN = 4.5f;
        private NotesGridModule gridModule;

        public KeySlider(KeyLabel keyLabel, NotesGridModule gridModule)
        {
            KeyLabel = keyLabel;
            Orientation = Orientation.Vertical;
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            this.gridModule = gridModule;
            
            Minimum = MINIMUMGAIN;
            Maximum = MAXIMUMGAIN;
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            gridModule.UpdateGainSlider(this);
        }
    }
}
