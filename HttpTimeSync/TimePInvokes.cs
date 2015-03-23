using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpTimeSync
{
    public static class TimePInvokes
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private extern static void GetSystemTime(ref SYSTEMTIME lpSystemTime);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetSystemTime(ref SYSTEMTIME lpSystemTime);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetSystemTimeAdjustment(uint dwTimeAdjustment, bool bTimeAdjustmentDisabled);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetSystemTimeAdjustment(out uint lpTimeAdjustment, out uint lpTimeIncrement, out bool lpTimeAdjustmentDisabled);

        private struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }

        public static DateTime GetTime()
        {
            // Call the native GetSystemTime method 
            // with the defined structure.
            SYSTEMTIME stime = new SYSTEMTIME();
            GetSystemTime(ref stime);

            //// Show the current time.           
            //MessageBox.Show("Current Time: " +
            //                stime.wHour.ToString() + ":"
            //                + stime.wMinute.ToString());
            var now = new DateTime(stime.wYear, stime.wMonth, stime.wDay, stime.wHour, stime.wMinute, stime.wSecond, stime.wMilliseconds, DateTimeKind.Utc);
            return now;
        }

        public static void SetTime(DateTime set)
        {
            // Call the native GetSystemTime method 
            // with the defined structure.
            SYSTEMTIME systime = new SYSTEMTIME();
            GetSystemTime(ref systime);

            // Set the system clock ahead one hour.
            systime.wYear = (ushort) set.Year;
            systime.wMonth = (ushort)set.Month;
            systime.wDay = (ushort) set.Day;
            systime.wHour = (ushort)set.Hour;
            systime.wMinute = (ushort)set.Minute;
            systime.wSecond = (ushort) set.Second;
            systime.wMilliseconds = (ushort)set.Millisecond;
            var result = SetSystemTime(ref systime);
            Debug.Assert(result, String.Format("SetSystemTime failed. GetLastError: {0}", Marshal.GetLastWin32Error()));
        }

        public struct ClockAdjust
        {
            public uint adjustment;
            public uint newadjustment;
            public uint increment;
            public bool disabled;
        }

        const int NanosecondsPerMilisecond = 1000000;
        
        public static ClockAdjust AdjustClockspeed(TimeSpan offset, int updateInterval, int adjustFactor)
        {
            var val = new ClockAdjust();

            GetSystemTimeAdjustment(out val.adjustment, out val.increment, out val.disabled);

            
            long offsetNano = Convert.ToInt64(offset.TotalMilliseconds) * NanosecondsPerMilisecond;
            long nextNano = (long)updateInterval * NanosecondsPerMilisecond;

            var intervals = nextNano/val.increment;

            uint adjustedIncrement = (uint)((nextNano + (offsetNano / adjustFactor)) / intervals);

            // fix only the adjustFactor of the offset in this interval
            val.newadjustment = adjustedIncrement;
            var result = SetSystemTimeAdjustment(val.newadjustment, false);
            Debug.Assert(result, String.Format("SetSystemTimeAdjustment failed. GetLastError: {0}", Marshal.GetLastWin32Error()));
            
            return val;
        }

        public static void StopAdjustClockspeed()
        {
            SetSystemTimeAdjustment(0, true);
        }
    }
}
