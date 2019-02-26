namespace Com.CI.Infrastructure
{
    public interface ISourceProvider
    {
        SourceDownloadResult DownloadSource(SourceDownloadParameters buildConfig);
    }

   

}
