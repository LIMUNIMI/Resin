using Resin.Modules.Audio;
using System.Windows.Media;

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
        public static readonly int DEFAULT_WAVEOUT_RATE = 48_000;
        internal static readonly int DEFAULT_WAVEOUT_CHANNELS = 2;

        #region DMIBox

        public static ResinDMIBox DMIbox { get; set; } = new ResinDMIBox();

        #endregion TongBox

        public const string CalibrationRadioButtonsGroup = "noteCalibRadio";
    }
}