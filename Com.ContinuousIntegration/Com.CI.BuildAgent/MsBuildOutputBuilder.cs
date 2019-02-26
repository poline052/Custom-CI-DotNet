using Com.CI.Infrastructure;
using System.Configuration;
using System.IO;
using System.Text;

namespace Com.CI.BuildAgent
{
    public class MsBuildOutputBuilder : IOutputBuilder
    {

        private readonly string nugetPath;
        private readonly string msBuildPath;
        private readonly string publishProfileFilePath;

        private readonly ICILogger ciLogger;
        private readonly ICIProcess ciProcess;
        private readonly ISignalRClient signalRClient;

        public MsBuildOutputBuilder(ICIProcess ciProcess, ISignalRClient signalRClient, ICILogger ciLogger)
        {
            this.ciLogger = ciLogger;
            this.ciProcess = ciProcess;
            this.signalRClient = signalRClient;

            nugetPath = ConfigurationManager.AppSettings["NuGetPath"];
            msBuildPath = ConfigurationManager.AppSettings["MsBuildPath"];

            publishProfileFilePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Com.CI.WebService.pubxml");

        }

        public BuildOutputResult Build(BuildParameters buildParameters, string temporarySourceDownloadDirectory)
        {
            var temporaryBuildOutputDirectory = GetTemporaryDirectory();

            var solutionFilePath = Path.Combine(temporarySourceDownloadDirectory, buildParameters.SolutionPath);

            ciLogger.WriteInfo($"Restoring nuget packages for solution {buildParameters.SolutionPath} for Repository: {buildParameters.RepositoryId} Branch: {buildParameters.BranchId}");


            if (buildParameters.RestoreNuget)
            {
                var nugetExitCode = ciProcess.Execute(nugetPath, $"restore {solutionFilePath}", buildParameters.RepositoryId, buildParameters.BranchId);

                if (!nugetExitCode.Equals(0))
                {
                    return new BuildOutputResult { Built = false, BuildOutputDirectory = string.Empty };
                }
            }

            var profileFilePath = BuildPublishProfile(temporaryBuildOutputDirectory, buildParameters);

            var buildArguments = buildParameters.ServiceType == ServiceTypes.WebService ?
                $"{solutionFilePath} /p:DeployOnBuild=true /p:PublishProfile={profileFilePath}"
                : $"{solutionFilePath} /t:rebuild /p:OutputPath={temporaryBuildOutputDirectory}";

            ciLogger.WriteInfo($"Building output for solution {buildParameters.SolutionPath} for Repository: {buildParameters.RepositoryId} Branch: {buildParameters.BranchId} where solution type is: {buildParameters.ServiceType}");

            var msBuildExitCode = ciProcess.Execute(msBuildPath, buildArguments, buildParameters.RepositoryId, buildParameters.BranchId);

            if (!msBuildExitCode.Equals(0))
            {
                return new BuildOutputResult { Built = false, BuildOutputDirectory = string.Empty };
            }
            else
            {
                return new BuildOutputResult { Built = true, BuildOutputDirectory = temporaryBuildOutputDirectory };
            }
        }

        private string BuildPublishProfile(string temporaryBuildOutputDirectory, BuildParameters buildParameters)
        {
            var temporaryProfileFilePath = Path.Combine(GetTemporaryDirectory(), "Com.CI.WebService.pubxml");

            var publishXml = $"<?xml version=\"1.0\" encoding=\"utf-8\"?><Project ToolsVersion=\"4.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><PropertyGroup><WebPublishMethod>FileSystem</WebPublishMethod><LastUsedBuildConfiguration>Release</LastUsedBuildConfiguration><LastUsedPlatform>Any CPU</LastUsedPlatform><SiteUrlToLaunchAfterPublish /><LaunchSiteAfterPublish>True</LaunchSiteAfterPublish><ExcludeApp_Data>False</ExcludeApp_Data><publishUrl>{temporaryBuildOutputDirectory}</publishUrl><DeleteExistingFiles>False</DeleteExistingFiles></PropertyGroup></Project>";

            File.WriteAllText(temporaryProfileFilePath, publishXml, Encoding.UTF8);

            return temporaryProfileFilePath;
        }



        private static string GetTemporaryDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}
