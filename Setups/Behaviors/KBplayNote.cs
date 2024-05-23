using NITHdmis.Keyboard;
using RawInputProcessor;
using Resin.DMIBox;

namespace Resin.Setups.Behaviors
{
    public class KBplayNote : IKeyboardBehavior, IToggleableBehavior
    {
        private readonly VKeyCodes keyPlay = VKeyCodes.Space;

        private bool enabled;

        public KBplayNote(bool enabled = false)
        {
            this.enabled = enabled;
        }

        public void Switch(bool enabled)
        {
            this.enabled = enabled;
        }

        public int ReceiveEvent(RawInputEventArgs e)
        {
            if (enabled)
            {
                if (e.VirtualKey == (int)keyPlay)
                {
                    if (e.KeyPressState == KeyPressState.Down)
                    {
                        R.DMIbox.Keyboard_ReceiveStrum();
                        return 0;
                    }
                }
            }
            return 1;
        }
    }
}