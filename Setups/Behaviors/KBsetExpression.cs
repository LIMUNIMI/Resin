using NITHdmis.Keyboard;
using RawInputProcessor;
using Resin.DMIBox;

namespace Resin.Setups.Behaviors
{
    public class KBsetExpression : IKeyboardBehavior, IToggleableBehavior
    {
        private readonly VKeyCodes keyPlay = VKeyCodes.E;

        private bool enabled;

        public KBsetExpression(bool enabled = false)
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
                        R.DMIbox.Keyboard_SetExpressionToZero();
                    }
                }
            }
            return 1;
        }
    }
}