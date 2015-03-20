using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace HttpTimeSyncService
{

    [RunInstaller(true)]
    public class NtpServiceInstaller : Installer
    {
        public NtpServiceInstaller()
        {
            var processInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

            //set the privileges
            processInstaller.Account = ServiceAccount.LocalSystem;

            serviceInstaller.DisplayName = NtpService.Name;
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            //must be the same as what was set in Program's constructor
            serviceInstaller.ServiceName = NtpService.Name;
            this.Installers.Add(processInstaller);
            this.Installers.Add(serviceInstaller);

        }
    }
}