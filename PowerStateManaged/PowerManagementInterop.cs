using System;
using System.Runtime.InteropServices;

namespace PowerStateManaged
{
    internal static class PowerManagementInterop
    {
        [DllImport("powrprof.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern UInt32 CallNtPowerInformation(
            Int32 InformationLevel,
            IntPtr lpInputBuffer,
            UInt32 nInputBufferSize,
            IntPtr lpOutputBuffer,
            UInt32 nOutputBufferSize
        );

        [DllImport("powrprof.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static extern bool SetSuspendState(
            [MarshalAs(UnmanagedType.I1)] bool bHibernate,
            [MarshalAs(UnmanagedType.I1)] bool bForce,
            [MarshalAs(UnmanagedType.I1)] bool bWakeupEventsDisabled
        );
    }
}
