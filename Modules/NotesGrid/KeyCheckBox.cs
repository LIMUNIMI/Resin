using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Resin.Modules.NotesGrid
{
    public class KeyCheckBox : CheckBox
    {
        public KeyLabel KeyLabel { get; set; } 

        public KeyCheckBox(KeyLabel keyLabel)
        {
            KeyLabel = keyLabel;
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            VerticalAlignment = System.Windows.VerticalAlignment.Center;
        }
    }
}
