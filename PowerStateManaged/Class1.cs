using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PowerStateManaged
{
    public static class PowerStateManaged
    {
        private const int LastSleepTime = 15;
        private const int LastWakeTime = 14;
        private const int SystemBatteryState = 5;
        private const int SystemPowerInformation = 12;
        private const int SystemReserveHiberFile = 10;

        private const uint STATUS_ACCESS_DENIED = 0xC0000022;
        private const uint STATUS_BUFFER_TOO_SMALL = 0xC0000023;
        private const uint STATUS_PRIVILEGE_NOT_HELD = 0xC0000061;

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
        }

        public struct SYSTEM_POWER_INFORMATION
        {
            public UInt32 MaxIdlenessAllowed;
            public UInt32 Idleness;
            public UInt32 TimeRemaining;
            public ushort CoolingMode;
        }

        static void Main(string[] args)
        {
            Console.WriteLine(GetLastSleepTime());
            Console.WriteLine(GetLastWakeTime());
            Console.WriteLine(GetBatteryState());
            var sysinfo = GetPowerInformation();
            //RemoveHibernationFile();
            ReserveHibernationFile();
            GoToSleep();
        }

        [DllImport("powrprof.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern UInt32 CallNtPowerInformation(
            Int32 InformationLevel,
            IntPtr lpInputBuffer,
            UInt32 nInputBufferSize,
            IntPtr lpOutputBuffer,
            UInt32 nOutputBufferSize
        );

        [DllImport("powrprof.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool SetSuspendState(
            [MarshalAs(UnmanagedType.I1)] bool bHibernate,
            [MarshalAs(UnmanagedType.I1)] bool bForce,
            [MarshalAs(UnmanagedType.I1)] bool bWakeupEventsDisabled
        );

        public static DateTime GetLastSleepTime()
        {
            var size = Marshal.SizeOf(typeof(UInt64));
            uint usize = (UInt32)size;
            IntPtr sleepTimeInfo = Marshal.AllocCoTaskMem(size);
            uint retval = CallNtPowerInformation(
                LastSleepTime,
                IntPtr.Zero,
                0,
                sleepTimeInfo,
                usize
            );
            //specifies the interrupt-time count, in 100-nanosecond units, at the last system sleep time
            var time = Marshal.ReadInt64(sleepTimeInfo);
            Marshal.FreeHGlobal(sleepTimeInfo);

            return ConvertTicksToDateTime(time);
        }

        public static DateTime GetLastWakeTime()
        {
            var size = Marshal.SizeOf(typeof(UInt64));
            uint usize = (UInt32)size;
            IntPtr wakeTimeInfo = Marshal.AllocCoTaskMem(size);
            uint retval = CallNtPowerInformation(
                LastWakeTime,
                IntPtr.Zero,
                0,
                wakeTimeInfo,
                usize
            );

            //specifies the interrupt - time count, in 100 - nanosecond units, at the last system wake time
            var time = Marshal.ReadInt64(wakeTimeInfo);
            Marshal.FreeHGlobal(wakeTimeInfo);

            return ConvertTicksToDateTime(time);
        }

        public static string GetBatteryState()
        {
            var size = Marshal.SizeOf(typeof(SYSTEM_BATTERY_STATE));
            uint usize = (UInt32)size;
            IntPtr systemBatteryInfo = Marshal.AllocCoTaskMem(size);

            uint retval = CallNtPowerInformation(
                SystemBatteryState,
                IntPtr.Zero,
                0,
                systemBatteryInfo,
                usize
            );

            var result = Marshal.PtrToStructure<SYSTEM_BATTERY_STATE>(systemBatteryInfo);
            
            Marshal.FreeHGlobal(systemBatteryInfo);
            return GetBatteryInfoDescription(result);
        }

        public static SYSTEM_POWER_INFORMATION GetPowerInformation()
        {
            var size = Marshal.SizeOf(typeof(SYSTEM_POWER_INFORMATION));
            uint usize = (UInt32)size;
            IntPtr systemBatteryInfo = Marshal.AllocCoTaskMem(size);

            uint retval = CallNtPowerInformation(
                SystemBatteryState,
                IntPtr.Zero,
                0,
                systemBatteryInfo,
                usize
            );

            var result = Marshal.PtrToStructure<SYSTEM_POWER_INFORMATION>(systemBatteryInfo);
            Marshal.FreeHGlobal(systemBatteryInfo);
            return result;
        }

        private static void ReserveHibernationFile()
        {
            var size =  Marshal.SizeOf<Int32>();
            IntPtr pBool = Marshal.AllocHGlobal(size);
            // If the value is TRUE, the hibernation file is reserved
            Marshal.WriteInt32(pBool, 0, 1); // last parameter 0 (FALSE), 1 (TRUE)

            uint retval = CallNtPowerInformation(
                SystemReserveHiberFile,
                pBool,
                (uint) size,
                IntPtr.Zero, 
                0
            );

            Console.WriteLine(retval != 0 ? "Access denied" : "Success");
        }

        private static void RemoveHibernationFile()
        {
            var size = Marshal.SizeOf<Int32>();
            IntPtr pBool = Marshal.AllocHGlobal(size);
            // If the value is FALSE, the hibernation file is removed
            Marshal.WriteInt32(pBool, 0, 0); // last parameter 0 (FALSE), 1 (TRUE)

            uint retval = CallNtPowerInformation(
                SystemReserveHiberFile,
                pBool,
                (uint)size,
                IntPtr.Zero,
                0
            );

            Console.WriteLine(retval != 0 ? "Access denied" : "Success");
        }

        public static void GoToSleep()
        {
            SetSuspendState(false, false, false);
        }

        private static string GetBatteryInfoDescription(SYSTEM_BATTERY_STATE sbs)
        {

            return $"Battery info:\n" +
                   $"Is the system battery charger is currently operating on external power: {sbs.AcOnLine}\n" +
                   $"Is at least one battery is present in the system: {sbs.BatteryPresent}\n" +
                   $"Battery is currently charging: {sbs.Charging}\n" +
                   $"Battery is currently discharging: {sbs.Discharging}\n" +
                   $"The theoretical capacity of the battery when new: {sbs.MaxCapacity} mW\\h\n" +
                   $"The estimated remaining capacity of the battery: {sbs.RemainingCapacity} mW\\h\n" +
                   $"The current rate of discharge of the battery, in mW: {sbs.Rate}\n" +
                   $"The estimated time remaining on the battery, in seconds: {sbs.EstimatedTime}\n" +
                   $"The manufacturer's suggestion of a capacity, in mWh, at which a low battery alert should occur: {sbs.DefaultAlert1}\n" +
                   $"The manufacturer's suggestion of a capacity, in mWh, at which a warning battery alert should occur: {sbs.DefaultAlert2}";
        }

        private static DateTime ConvertTicksToDateTime(long ticks)
        {
            var time = (long)((ulong)Environment.TickCount * 10000);
            return new DateTime(DateTime.Now.Ticks - time + ticks);
        }
    }
}
