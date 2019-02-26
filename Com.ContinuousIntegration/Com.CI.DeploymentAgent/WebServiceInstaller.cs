using Microsoft.Web.Administration;
using Com.CI.Infrastructure;
using System;
using System.Linq;

namespace Com.CI.DeploymentAgent
{
    public class WebServiceInstaller : IInstaller
    {
        private readonly ICILogger ciLogger;

        public WebServiceInstaller(ICILogger ciLogger)
        {
            this.ciLogger = ciLogger;
        }

        public void Install(InstallSetting installSetting)
        {
            using (var serverManager = new ServerManager())
            {
                var existingSite = GetSite(installSetting.ServiceName, serverManager);

                if (existingSite != null)
                {
                    ciLogger.WriteInfo($"Service: {installSetting.ServiceName} already exists so cleaning up current deployment with site registration and application pool");
                    DeleteSite(existingSite, serverManager);
                }

                CreateApplicationPool(installSetting, serverManager);

                CreateWebSite(installSetting, serverManager);
            }
        }


        private Site GetSite(string serviceName, ServerManager server)
        {
            var site = server.Sites.SingleOrDefault(s => s.Name.Equals(serviceName, StringComparison.InvariantCultureIgnoreCase));
            return site;
        }


        private bool CreateWebSite(InstallSetting installSetting, ServerManager server)
        {
            string bindingInfo = $"*:{installSetting.Port}:{installSetting.WebServiceHostName}";

            ciLogger.WriteInfo($"Creating service: {installSetting.ServiceName} with binding {bindingInfo}");

            Site site = server.Sites.Add(installSetting.ServiceName, "http", bindingInfo, installSetting.PhysicalPath);

            site.ApplicationDefaults.ApplicationPoolName = installSetting.ServiceName;

            server.CommitChanges();

            return true;
        }

        private bool CreateApplicationPool(InstallSetting installSetting, ServerManager server)
        {
            ciLogger.WriteInfo($"Creating application pool (.NET v4.0) for service {installSetting.ServiceName}");

            var applicationPool = server.ApplicationPools.Add(installSetting.ServiceName);

            applicationPool.ProcessModel.IdentityType = ProcessModelIdentityType.NetworkService;

            applicationPool.ManagedRuntimeVersion = "v4.0";

            server.CommitChanges();

            return true;
        }



        private bool DeleteSite(Site site, ServerManager server)
        {
            var result = site.Stop();

            server.Sites.Remove(site);

            server.CommitChanges();

            var appPool = server.ApplicationPools.SingleOrDefault(ap => ap.Name.Equals(site.Name, StringComparison.InvariantCultureIgnoreCase));

            if (appPool != null)
            {
                appPool.Stop();

                server.ApplicationPools.Remove(appPool);

                server.CommitChanges();
            }

            return true;
        }

        public void VerifyInstallation()
        {

        }


    }
}