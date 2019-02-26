using Com.CI.Infrastructure;
using System.Collections.Specialized;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.ServiceProcess;

namespace Com.CI.DeploymentAgent
{
    public class WindowsServiceInstaller : IInstaller
    {

        private readonly ICILogger ciLogger;
        
        public WindowsServiceInstaller(ICILogger ciLogger)
        {
            this.ciLogger = ciLogger;
            
        }


        public void Install(InstallSetting installSetting)
        {
            ciLogger.WriteInfo("Checking if the service is already Installed or not");

            var serviceAlreadyInstalled = CheckIfServiceInstalled(installSetting);

            if (serviceAlreadyInstalled)
            {
                ciLogger.WriteInfo("Service already installed. Uninstalling it");

                StopService(installSetting);

                UnInstallWindowsService(installSetting);
                ciLogger.WriteInfo("Service uninstalled");
            }

            ciLogger.WriteInfo("Installing service");
            InstallWindowsService(installSetting);
            StartService(installSetting);
            ciLogger.WriteInfo("Service installed");
        }

        private static void InstallWindowsService(InstallSetting installSetting)
        {
            using (var serviceProcessInstaller = new ServiceProcessInstaller() { Account = ServiceAccount.NetworkService })
            {
                using (ServiceInstaller serviceInstaller = new ServiceInstaller())
                {
                    var serviceAssemblyPath = Path.Combine(installSetting.PhysicalPath, $"{installSetting.ServiceName}.exe");

                    string assemblyPathContextParameter = string.Format("/assemblypath={0}", serviceAssemblyPath);

                    string[] cmdline = { assemblyPathContextParameter };

                    var installContext = new InstallContext(string.Empty, cmdline);

                    serviceInstaller.Context = installContext;
                    serviceInstaller.DisplayName = installSetting.ServiceName;
                    serviceInstaller.Description = installSetting.ServiceDescription;
                    serviceInstaller.ServiceName = installSetting.ServiceName;
                    serviceInstaller.StartType = ServiceStartMode.Automatic;
                    serviceInstaller.Parent = serviceProcessInstaller;

                    ListDictionary state = new ListDictionary();

                    serviceInstaller.Install(state);
                }
            }
        }


        private static void UnInstallWindowsService(InstallSetting installSetting)
        {
            using (ServiceInstaller serviceInstaller = new ServiceInstaller())
            {
                InstallContext Context = new InstallContext(string.Empty, null);
                serviceInstaller.Context = Context;
                serviceInstaller.ServiceName = installSetting.ServiceName;
                serviceInstaller.Uninstall(null);
            }
        }


        private static bool CheckIfServiceInstalled(InstallSetting installSetting)
        {
            var serviceController = ServiceController.GetServices().SingleOrDefault(s => s.ServiceName == installSetting.ServiceName);

            var serviceExists = serviceController != null;

            return serviceExists;
        }

        private static void StopService(InstallSetting installSetting)
        {
            var serviceController = ServiceController.GetServices().SingleOrDefault(s => s.ServiceName == installSetting.ServiceName);

            if (serviceController.Status != ServiceControllerStatus.Stopped)
            {
                serviceController.Stop();
            }
        }

        private static void StartService(InstallSetting installSetting)
        {
            var serviceController = ServiceController.GetServices().SingleOrDefault(s => s.ServiceName == installSetting.ServiceName);

            serviceController.Start();
        }


        public void VerifyInstallation()
        {
        }
    }
}