using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerStateManaged;

namespace PowerStateManagedTests
{
    [TestClass]
    public class PowerManagementTest
    {
        [TestMethod]
        public void GetLastSleepTime_Test()
        {
            var powerManager = new PowerManagement();
            Console.WriteLine(powerManager.GetLastSleepTime());
        }

        [TestMethod]
        public void GetLastWakeTime_Test()
        {
            var powerManager = new PowerManagement();
            Console.WriteLine(powerManager.GetLastWakeTime());
        }

        [TestMethod]
        public void GetBatteryState_Test()
        {
            var powerManager = new PowerManagement();
            Console.WriteLine(powerManager.GetBatteryState());
        }

        [TestMethod]
        public void GetPowerInformation_Test()
        {
            var powerManager = new PowerManagement();
            Console.WriteLine(powerManager.GetPowerInformation());
        }

        [TestMethod]
        public void ReserveHibernationFile_Test()
        {
            var powerManager = new PowerManagement();
            powerManager.ReserveHibernationFile();
        }

        [TestMethod]
        public void RemoveHibernationFile_Test()
        {
            var powerManager = new PowerManagement();
            powerManager.RemoveHibernationFile();
        }

        [TestMethod]
        public void SetSystemSleepState_Test()
        {
            var powerManager = new PowerManagement();
            powerManager.SetSystemSleepState();
        }

        [TestMethod]
        public void SetSystemHibernationState_Test()
        {
            var powerManager = new PowerManagement();
            powerManager.SetSystemHibernationState();
        }
    }
}
