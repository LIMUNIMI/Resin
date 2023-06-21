using NeeqDMIs.Headtracking.NeeqHT;
using NeeqDMIs.MIDI;
using NeeqDMIs.Utils;
using NeeqDMIs.Utils.ValueFilters;
using System;
using Resin.DMIBox;
using NeeqDMIs.Filters.ValueFilters;

namespace Resin.Setups.Behaviors
{
    public class HByawControl_old : INeeqHTbehavior
    {
        private const int MinimumPressure = 90;
        private readonly IDoubleFilter SpeedFilter = new DoubleFilterMAExpDecaying(0.4f);
        private readonly ValueMapperDouble SpeedMapper = new ValueMapperDouble(0.5f, 127);
        //private readonly ValueMapperDouble YawMapper = new ValueMapperDouble(50, 127);
        private double currentYaw = 0;
        private Directions direction = Directions.Right;
        private double previousYaw = 0;
        private double speed = 0;
        public int DeadZone { get; set; } = 1;
        public PressureControl PressureControl { get; set; }

        public double Speed
        {
            get => speed;
            set
            {
                speed = Math.Abs(value);
                SpeedFilter.Push(speed);
                int ausssh = (int)SpeedMapper.Map(SpeedFilter.Pull());
                PressureControl.Pressure = 120;
            }
        }

        public HByawControl_old(IMidiModule midiModule)
        {
            PressureControl = new PressureControl(midiModule);
            PressureControl.MinimumPressure = MinimumPressure;
        }

        public void ReceiveHeadTrackerData(NeeqHTData data)
        {
            currentYaw = data.CenteredPosition.Yaw;
            speed = currentYaw - previousYaw;

            Speed = speed;
            R.DMIbox.Mon_Speed = speed;

            ProcessStrumming();

            previousYaw = currentYaw;
        }

        private bool IsOutsideDeadzone()
        {
            if (currentYaw < -DeadZone || currentYaw > DeadZone)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ProcessStrumming()
        {
            if (IsOutsideDeadzone())
            {
                if (previousYaw < currentYaw && direction == Directions.Left)
                {
                    direction = Directions.Right;
                    //G.TB.ReceiveStrum((int)YawMapper.Map(Math.Abs(currentYaw - DeadZone)));
                }
                if (previousYaw > currentYaw && direction == Directions.Right)
                {
                    direction = Directions.Left;
                    //G.TB.ReceiveStrum((int)YawMapper.Map(Math.Abs(currentYaw - DeadZone)));
                }
            }
        }

        private enum Directions
        {
            Left,
            Right
        }
    }
}