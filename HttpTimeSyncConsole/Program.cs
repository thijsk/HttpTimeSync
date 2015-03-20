using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HttpTimeSync;
using HttpTimeSyncService;
using log4net;
using log4net.Config;

namespace HttpTimeSyncConsole
{
    class Program
    {
        private static void Main(string[] args)
        {

            AdjustTokenPrivilegesPInvokes.EnableSetTimePrivileges();
            TimePInvokes.StopAdjustClockspeed();

            var list = ServerList.GetServerListFromConfig();
            XmlConfigurator.Configure();
            ILog log = LogManager.GetLogger(typeof (Program));
            
            var service = new TimeService(list, log);
            int interval = service.GetInterval();
            while (true)
            {
                var start = DateTime.UtcNow;
                service.Update(interval);
                var spent = DateTime.UtcNow.Subtract(start).TotalMilliseconds;
                Thread.Sleep(interval - Convert.ToInt32(spent));
            }
        }


    }
}
