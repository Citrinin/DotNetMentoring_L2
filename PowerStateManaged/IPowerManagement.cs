using System;
using System.Runtime.InteropServices;

namespace PowerStateManaged
{
    [ComVisible(true)]
    [Guid("1EFE40C7-AF23-48FF-A77B-AB940FB16DED")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IPowerManagement
    {
        string GetLastSleepTime();
        string GetLastWakeTime();
        string GetBatteryState();
        string GetPowerInformation();
        void ReserveHibernationFile();
        void RemoveHibernationFile();
        void SetSystemSleepState();
        void SetSystemHibernationState();
    }
}