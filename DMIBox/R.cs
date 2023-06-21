using System.Collections.Generic;
using System.Windows.Media;
using Resin.DataTypes;

namespace Resin.DMIBox
{
    public static class R
    {
        #region Look and feel

        public static readonly SolidColorBrush GuiDisabledBackgroundBrush = new SolidColorBrush(Colors.White);
        public static readonly SolidColorBrush GuiDisabledForegroundBrush = new SolidColorBrush(Colors.Black);
        public static readonly SolidColorBrush GuiFailBrush = new SolidColorBrush(Colors.Red);
        public static readonly SolidColorBrush GuiOkBrush = new SolidColorBrush(Colors.LightGreen);
        public static readonly SolidColorBrush KeyLabelFontColor = new SolidColorBrush(Colors.White);
        public static readonly SolidColorBrush KeyLabelOffBrush = new SolidColorBrush(Colors.Black);
        public static readonly SolidColorBrush KeyLabelOnBrush = new SolidColorBrush(Colors.Red);

        #endregion Look and feel

        public static readonly TongControlModes DefaultControlMode = TongControlModes.Keyboard;

        #region DMIBox

        private static ResinDMIBox dmiBox;
        public static ResinDMIBox DMIbox { get => dmiBox; set => dmiBox = value; }

        #endregion TongBox

        public const string CalibrationRadioButtonsGroup = "noteCalibRadio";
    }
}