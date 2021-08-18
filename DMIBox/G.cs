using System.Collections.Generic;
using System.Windows.Media;
using Resin.DataTypes;

namespace Resin.DMIBox
{
    public static class G
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

        #region TongBox

        private static TongBox tongBox;
        public static TongBox TB { get => tongBox; set => tongBox = value; }

        #endregion TongBox

        public const string CalibrationRadioButtonsGroup = "noteCalibRadio";
    }
}