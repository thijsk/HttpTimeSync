using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpTimeSync.Properties;

namespace HttpTimeSync
{
    public class ServerList : List<Server>
    {

        public static ServerList GetServerListFromConfig()
        {
            var stringList = Settings.Default.ServerList.Split(',');
            var serverList = new ServerList();
            foreach (var str in stringList)
            {
                var server = new Server();
                server.Url = new Uri("http://" + str.Trim());
                serverList.Add(server);
            }
            return serverList;
        }


        public void MarkOutliers()
        {
            var offsetInTicks = this.Where(s => !s.IsOutlier).Select(s => (double)s.Offset.Ticks).ToList();

            double average = offsetInTicks.Average();
            double stddev = offsetInTicks.StdDev();
            var upper = (average + stddev);
            var lower = (average - stddev);

            var outliers = this.Where(o => !(lower < o.Offset.Ticks && o.Offset.Ticks < upper));

            foreach (var server in outliers)
            {
                server.IsOutlier = true;
            }
        }

        public TimeSpan FilteredAverageOffset()
        {
            var filteredAverage = this.Where(s => !s.IsOutlier).Select(s => s.Offset.Ticks).Average();
            return new TimeSpan(Convert.ToInt64(filteredAverage));
        }

    }
}
