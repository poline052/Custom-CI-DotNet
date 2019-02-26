namespace Com.CI.Infrastructure
{
    public interface IOutputBuilder
    {
        BuildOutputResult Build(BuildParameters buildConfig, string temporarySourceDownloadDirectory);
    }


   

}
