namespace Com.CI.Infrastructure
{
    public interface IInstaller
    {
        void Install(InstallSetting installSetting);
        void VerifyInstallation();
    }
}
