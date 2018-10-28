using System;

namespace PowerStateManaged
{
    public struct SYSTEM_POWER_INFORMATION
    {
        public UInt32 MaxIdlenessAllowed;
        public UInt32 Idleness;
        public UInt32 TimeRemaining;
        public ushort CoolingMode;
        public override string ToString()
        {
            return
                $"Power information:\n" +
                $"The idleness at which the system is considered idle and the idle time-out begins counting, expressed as a percentage: {MaxIdlenessAllowed}\n" +
                $"The current idle level, expressed as a percentage: {Idleness}\n" +
                $"The time remaining in the idle timer, in seconds: {TimeRemaining}\n" +
                $"The current system cooling mode: {CoolingMode}\n" +
                $"\t*0 - The system is currently in Active cooling mode\n" +
                $"\t*1 - The system is currently in Passive cooling mode\n" +
                $"\t*2 - The system does not support CPU throttling, or there is no thermal zone defined in the system";
        }
    }
}