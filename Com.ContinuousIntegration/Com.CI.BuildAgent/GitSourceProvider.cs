using Com.CI.Infrastructure;
using System.Configuration;
using System.IO;

namespace Com.CI.BuildAgent
{
    public class GitSourceProvider : ISourceProvider
    {
        private readonly ICILogger ciLogger;
        private readonly string gitPath;
        private readonly ICIProcess ciProcess;
        private readonly ISignalRClient signalRClient;


        public GitSourceProvider(ICIProcess ciProcess, ISignalRClient signalRClient, ICILogger ciLogger)
        {
            this.ciProcess = ciProcess;
            this.ciLogger = ciLogger;
            this.signalRClient = signalRClient;
            gitPath = ConfigurationManager.AppSettings["GitPath"];
        }

        public SourceDownloadResult DownloadSource(SourceDownloadParameters sourceDownloadParameters)
        {
            var temporarySourceDownloadDirectory = GetTemporarySourceDownloadDirectory();

            var downloadSourceResult = new SourceDownloadResult { Downloaded = false, TemporarySourceDownloadDirectory = temporarySourceDownloadDirectory };

            ciLogger.WriteInfo($"Downloading source from {sourceDownloadParameters.GitUri} for Repository: {sourceDownloadParameters.RepositoryId} Branch: {sourceDownloadParameters.BranchId}");

            // git.exe clone --branch test --depth 1 https://************@bitbucket.org/***/****.git Path
            var exitCode = ciProcess.Execute(gitPath, $"clone --branch test --depth 1 {sourceDownloadParameters.GitUriWithCredentials} {temporarySourceDownloadDirectory}", sourceDownloadParameters.RepositoryId, sourceDownloadParameters.BranchId);

            if (!exitCode.Equals(0))
            {
                return downloadSourceResult;
            }
            else
            {
                return new SourceDownloadResult { Downloaded = true, TemporarySourceDownloadDirectory = temporarySourceDownloadDirectory };
            }
        }

        private static string GetTemporarySourceDownloadDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }


}
