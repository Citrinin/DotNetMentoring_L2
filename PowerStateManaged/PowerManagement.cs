using System;
using System.Runtime.InteropServices;

namespace PowerStateManaged
{
    //Command for registration
    //"C:\Windows\Microsoft.NET\Framework\v4.0.30319\regasm.exe" PowerStateManaged.dll /tlb /codebase
    [ComVisible(true)]
    [Guid("4F5C361D-CE3F-4968-8391-5BE52D6820A3")]
    [ClassInterface(ClassInterfaceType.None)]
    public class PowerManagement : IPowerManagement
    {
        private const int LastSleepTime = 15;
        private const int LastWakeTime = 14;
        private const int SystemBatteryState = 5;
        private const int SystemPowerInformation = 12;
        private const int SystemReserveHiberFile = 10;

        private const uint STATUS_SUCCESS = 0x00000000;
        private const uint STATUS_ACCESS_DENIED = 0xC0000022;
        private const uint STATUS_BUFFER_TOO_SMALL = 0xC0000023;
        private const uint STATUS_PRIVILEGE_NOT_HELD = 0xC0000061;

        public string GetLastSleepTime()
        {
            var size = Marshal.SizeOf(typeof(UInt64));
            IntPtr sleepTimeInfo = Marshal.AllocCoTaskMem(size);

            uint retval = PowerManagementInterop.CallNtPowerInformation(
                LastSleepTime,
                IntPtr.Zero,
                0,
                sleepTimeInfo,
                (UInt32)size
            );

            //specifies the interrupt-time count, in 100-nanosecond units, at the last system sleep time
            var time = Marshal.ReadInt64(sleepTimeInfo);
            Marshal.FreeHGlobal(sleepTimeInfo);
            CheckError(retval);

            return ConvertTicksToDateTime(time).ToString();
        }

        public string GetLastWakeTime()
        {
            var size = Marshal.SizeOf(typeof(UInt64));
            IntPtr wakeTimeInfo = Marshal.AllocCoTaskMem(size);

            uint retval = PowerManagementInterop.CallNtPowerInformation(
                LastWakeTime,
                IntPtr.Zero,
                0,
                wakeTimeInfo,
                (UInt32)size
            );

            //specifies the interrupt - time count, in 100 - nanosecond units, at the last system wake time
            var time = Marshal.ReadInt64(wakeTimeInfo);
            Marshal.FreeHGlobal(wakeTimeInfo);
            CheckError(retval);

            return ConvertTicksToDateTime(time).ToString();
        }

        public string GetBatteryState()
        {
            var size = Marshal.SizeOf(typeof(SYSTEM_BATTERY_STATE));
            IntPtr systemBatteryInfo = Marshal.AllocCoTaskMem(size);

            uint retval = PowerManagementInterop.CallNtPowerInformation(
                SystemBatteryState,
                IntPtr.Zero,
                0,
                systemBatteryInfo,
                (UInt32)size
            );

            var result = Marshal.PtrToStructure<SYSTEM_BATTERY_STATE>(systemBatteryInfo);
            Marshal.FreeHGlobal(systemBatteryInfo);
            CheckError(retval);

            return result.ToString();
        }

        public string GetPowerInformation()
        {
            var size = Marshal.SizeOf(typeof(SYSTEM_POWER_INFORMATION));
            IntPtr systemBatteryInfo = Marshal.AllocCoTaskMem(size);

            uint retval = PowerManagementInterop.CallNtPowerInformation(
                SystemPowerInformation,
                IntPtr.Zero,
                0,
                systemBatteryInfo,
                (UInt32)size
            );

            var result = Marshal.PtrToStructure<SYSTEM_POWER_INFORMATION>(systemBatteryInfo);
            Marshal.FreeHGlobal(systemBatteryInfo);
            CheckError(retval);

            return result.ToString();
        }

        public void ReserveHibernationFile()
        {
            var size = Marshal.SizeOf<Int32>();
            IntPtr pBool = Marshal.AllocHGlobal(size);
            // If the value is TRUE, the hibernation file is reserved
            Marshal.WriteInt32(pBool, 0, 1); // last parameter 0 (FALSE), 1 (TRUE)

            uint retval = PowerManagementInterop.CallNtPowerInformation(
                SystemReserveHiberFile,
                pBool,
                (UInt32)size,
                IntPtr.Zero,
                0
            );

            CheckError(retval);
        }

        public void RemoveHibernationFile()
        {
            var size = Marshal.SizeOf<Int32>();
            IntPtr pBool = Marshal.AllocHGlobal(size);
            // If the value is FALSE, the hibernation file is removed
            Marshal.WriteInt32(pBool, 0, 0); // last parameter 0 (FALSE), 1 (TRUE)

            uint retval = PowerManagementInterop.CallNtPowerInformation(
                SystemReserveHiberFile,
                pBool,
                (uint)size,
                IntPtr.Zero,
                0
            );

            CheckError(retval);
        }

        public void SetSystemSleepState()
        {
            PowerManagementInterop.SetSuspendState(false, false, false);
        }

        public void SetSystemHibernationState()
        {
            PowerManagementInterop.SetSuspendState(true, false, false);
        }

        private static DateTime ConvertTicksToDateTime(long ticks)
        {
            var time = (long)((ulong)Environment.TickCount * 10000);
            return new DateTime(DateTime.Now.Ticks - time + ticks);
        }

        private static void CheckError(uint retval)
        {
            switch (retval)
            {
                case STATUS_SUCCESS:
                {
                    return;
                }
                case STATUS_ACCESS_DENIED:
                {
                    throw new UnauthorizedAccessException("Access denied. Try to run Application as administrator");
                }
                case STATUS_BUFFER_TOO_SMALL:
                {
                    throw new ArgumentException("Buffer too small");
                }
                case STATUS_PRIVILEGE_NOT_HELD:
                {
                    throw new UnauthorizedAccessException("Privilege not held. Try to run Application as administrator");
                }
                default:
                {
                    throw new Exception($"Error {retval:X)} occurs. For more details see help link")
                    {
                        HelpLink = "https://msdn.microsoft.com/en-us/library/cc704588.aspx"
                    };
                }
            }
        }
    }
}
