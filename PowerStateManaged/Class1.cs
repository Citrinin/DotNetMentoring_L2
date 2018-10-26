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
        [DllImport("powrprof.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "1")]
        public static extern int GetPowerInfo(byte informationLevel, UInt64 lpInBuffer, UInt32 inBufferSize,
            UInt64 lpOutBuffer, UInt32 outBufferSize);
    }
}
