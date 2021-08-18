namespace Resin.Setups.Behaviors
{
    public interface IToggleableBehavior
    {
        void Switch(bool enabled);

        /* SAMPLE CODE TO ADD TO IToggleableBehaviors:

        ==========================================================

        private bool enabled;

        public Constructor(bool enabled = false)
        {
            this.enabled = enabled;
        }

        public void Enabled(bool enabled)
        {
            this.enabled = enabled;
        }

        ===========================================================

        */
    }
}