namespace Com.CI.Infrastructure
{
    public interface IServiceInstallerProvider
    {
        IInstaller GetInstaller(ServiceTypes serviceTypes);
    }
}
