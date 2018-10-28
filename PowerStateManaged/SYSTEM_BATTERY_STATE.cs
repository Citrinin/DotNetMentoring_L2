using System;
using System.Runtime.InteropServices;

namespace PowerStateManaged
{
    public struct SYSTEM_BATTERY_STATE
    {
        [MarshalAs(UnmanagedType.I1)] public bool AcOnLine;
        [MarshalAs(UnmanagedType.I1)] public bool BatteryPresent;
        [MarshalAs(UnmanagedType.I1)] public bool Charging;
        [MarshalAs(UnmanagedType.I1)] public bool Discharging;
        [MarshalAs(UnmanagedType.I1)] bool Spare1;
        byte Tag;
        public UInt32 MaxCapacity;
        public UInt32 RemainingCapacity;
        public Int32 Rate;
        public UInt32 EstimatedTime;
        public UInt32 DefaultAlert1;
        public UInt32 DefaultAlert2;

        public override string ToString()
        {
            return $"Battery information:\n" +
                   $"Is the system battery charger is currently operating on external power: {AcOnLine}\n" +
                   $"Is at least one battery is present in the system: {BatteryPresent}\n" +
                   $"Battery is currently charging: {Charging}\n" +
                   $"Battery is currently discharging: {Discharging}\n" +
                   $"The theoretical capacity of the battery when new: {MaxCapacity} \n" +
                   $"The estimated remaining capacity of the battery: {RemainingCapacity} \n" +
                   $"The current rate of discharge of the battery, in mW: {Rate}\n" +
                   $"The estimated time remaining on the battery, in seconds: {EstimatedTime}\n" +
                   $"The manufacturer's suggestion of a capacity, in mWh, at which a low battery alert should occur: {DefaultAlert1}\n" +
                   $"The manufacturer's suggestion of a capacity, in mWh, at which a warning battery alert should occur: {DefaultAlert2}";
        }
    }
}