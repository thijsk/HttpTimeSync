using System.ServiceProcess;
using NtpServer;

namespace HttpTimeSyncService
{
    public class NtpService : ServiceBase
    {
        public static string Name = "NtpService";

        public NtpService()
        {
            ServiceName = Name;
        }

        private NtpServerImpl _server;

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            _server = new NtpServerImpl();
            _server.Start();
        }

        protected override void OnStop()
        {
            _server.Stop();
            base.OnStop();
        }
    }
}