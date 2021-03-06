using NeeqDMIs.Headtracking.NeeqHT;
using Resin.DMIBox;

namespace Resin.Setups.Behaviors
{
    public class HByawStrumming : INeeqHTbehavior, IToggleableBehavior
    {
        private bool enabled;

        public HByawStrumming(bool enabled = false)
        {
            this.enabled = enabled;
        }

        public void Switch(bool enabled)
        {
            this.enabled = enabled;
        }

        public void ReceiveHeadTrackerData(HeadTrackerData data)
        {
            if (enabled)
            {
                G.TB.HTStrum_ElaboratePosition(data);
            }
        }
    }
}