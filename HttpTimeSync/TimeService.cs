using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HttpTimeSync.Properties;
using log4net;

namespace HttpTimeSync
{
    public class TimeService
    {
        public TimeService(ServerList list, ILog log)
        {
            ServerList = list;
            Log = log;
        }

        private ServerList ServerList { get; set; }
        private ILog Log { get; set; }

        public int GetInterval()
        {
            return Settings.Default.IntervalSecs*1000;
        }

        private static string GetBody(Uri url)
        {
            var request = WebRequest.CreateHttp(url);
            request.UserAgent = "Mozilla/5.0";
            return GetResponseBody(request);
        }

        private static string GetHeader(string header, Uri url)
        {
            var request = WebRequest.CreateHttp(url);
            request.UserAgent = "Mozilla/5.0";
            return GetResponseHeader(header, request);
        }

        private static string GetResponseHeader(string header, HttpWebRequest request)
        {
            request.Method = WebRequestMethods.Http.Head;
            using (var response = request.GetResponse())
            {
                return response.Headers[header];
            }
        }

        private static DateTime ParseHttpDateString(string inputDate)
        {
            var time = DateTime.ParseExact(inputDate,
                "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                CultureInfo.InvariantCulture.DateTimeFormat,
                DateTimeStyles.AdjustToUniversal);
            return time;
        }

        private static string GetResponseBody(WebRequest request)
        {
            string documentContents;
            using (var response = request.GetResponse())
            {
                using (var readStream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    documentContents = readStream.ReadToEnd();
                }
            }
            return documentContents;
        }

        public void Update(int interval)
        {
            var list = ServerList;

            Log.Info("Starting update");

            // do http requests and look at the date header
            foreach (var server in list.Randomize())
            {
                try
                {
                    Dns.GetHostEntry(server.Url.DnsSafeHost);
                    server.IsOutlier = false;
                    server.RequestStartDateTime = DateTime.UtcNow;
                    var header = GetHeader("Date", server.Url);
                    server.RequestFinishDateTime = DateTime.UtcNow;
                    server.ResponseDateTime = ParseHttpDateString(header);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    server.IsOutlier = true;
                }
            }
            // filter out the worst responses
            list.MarkOutliers();

            foreach (var server in list)
            {
                Log.Debug(server);
            }

            var average = list.FilteredAverageOffset();
            Log.InfoFormat("Average offset {0}", average);

            var invalidPercentage = (double) list.Count(s => s.IsOutlier)/list.Count;

            if (invalidPercentage > 0.8)
            {
                TimePInvokes.StopAdjustClockspeed();
                Log.DebugFormat("Too many servers responded with an unusable value {0}%", invalidPercentage);
            }
            else
            {
                if (average.TotalMinutes > GetJumpMinutes())
                {
                    var systemTime = TimePInvokes.GetTime();
                    var newTime = systemTime.Add(average);
                    TimePInvokes.SetTime(newTime);
                    Log.WarnFormat("Big offset, time jumped from {0} to {1}", systemTime.TimeOfDay, newTime.TimeOfDay);
                }
                else
                {
                    var result = TimePInvokes.AdjustClockspeed(average, interval);
                    Log.InfoFormat("Adjusting clock speed, {0} => {1} ({2} / {3}), disabled {4}", result.adjustment, result.newadjustment, (int) result.newadjustment - (int) result.adjustment, (int) result.newadjustment - (int) result.increment, result.disabled);
                }
            }
        }

        private int GetJumpMinutes()
        {
            return Settings.Default.JumpMinutes;
        }
    }
}