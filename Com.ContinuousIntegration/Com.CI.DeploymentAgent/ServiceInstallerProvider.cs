using Com.CI.Infrastructure;
using SimpleInjector;

namespace Com.CI.DeploymentAgent
{
    public class ServiceInstallerProvider: IServiceInstallerProvider
    {
        private readonly Container container;

        public ServiceInstallerProvider(Container container)
        {
            this.container = container;
        }

        public IInstaller GetInstaller(ServiceTypes serviceTypes)
        {
            switch (serviceTypes)
            {
                case ServiceTypes.WebService: return container.GetInstance<WebServiceInstaller>();
                case ServiceTypes.WindowsService: return container.GetInstance<WindowsServiceInstaller>();
                default: return null;
            }
        }
    }
}