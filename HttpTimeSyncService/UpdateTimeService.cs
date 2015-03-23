using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HttpTimeSync;
using log4net;
using log4net.Config;

namespace HttpTimeSyncService
{
    public class UpdateTimeService : ServiceBase
    {
        public static string Name = "HttpTimeSyncService";

        public UpdateTimeService()
        {
            ServiceName = Name;
        }

        private CancellationTokenSource _tokenSource;

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            AdjustTokenPrivilegesPInvokes.EnableSetTimePrivileges();
            TimePInvokes.StopAdjustClockspeed();

            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            var list = ServerList.GetServerListFromConfig();

            ILog log = LogManager.GetLogger(typeof (UpdateTimeService));
            log.Info("Starting");
            var service = new TimeService(list, log);
            int interval = service.GetInterval();


            var updateTimeTask = Task.Run(async () =>
            {
                await Task.Delay(1000*10, token);
                while (!token.IsCancellationRequested)
                {
                    var start = DateTime.UtcNow;
                    service.Update(interval);
                    var spent = DateTime.UtcNow.Subtract(start).TotalMilliseconds;
                    var delay = interval - Convert.ToInt32(spent);
                    await Task.Delay(Math.Max(delay, 10000), token);
                }
            }, token);
        }

        protected override void OnStop()
        {
            _tokenSource.Cancel();
            TimePInvokes.StopAdjustClockspeed();
            base.OnStop();
        }
    }
}
