using Com.CI.Infrastructure;
using System;
using System.IO;
using System.IO.Compression;

namespace Com.CI.DeploymentAgent
{
    public class DeployCommandHandler : ICommandHandler<DeployCommand, DeployCommandResponse>
    {
        private readonly ICILogger ciLogger;
        private readonly IDataMapper dataMapper;
        private readonly IServiceInstallerProvider serviceInstallerProvider;

        public DeployCommandHandler(IDataMapper dataMapper, IServiceInstallerProvider serviceInstallerProvider, ICILogger ciLogger)
        {
            this.ciLogger = ciLogger;
            this.dataMapper = dataMapper;
            this.serviceInstallerProvider = serviceInstallerProvider;
        }

        public DeployCommandResponse Handle(DeployCommand deployCommand)
        {
            ciLogger.WriteInfo($"Extracting build output to {deployCommand.PhysicalPath}");

            ExtractZipArchiveStreamTo(deployCommand.DeploymentPackageStream, deployCommand.PhysicalPath);

            var installer = serviceInstallerProvider.GetInstaller(deployCommand.ServiceType);

            var installSetting = dataMapper.Map<InstallSetting, DeployCommand>(deployCommand);

            ciLogger.WriteInfo($"Installing service: {deployCommand.ServiceName} ({deployCommand.ServiceType}) with setup parameters: {deployCommand.WebServiceHostName}");

            installer.Install(installSetting);

            WriteDone(installSetting);

            return new DeployCommandResponse { ErrorMessage = string.Empty, StatusCode = 200 };
        }


        private static void WriteDone(InstallSetting installSetting)
        {
            var consoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Deployment for {installSetting.ServiceType}: {installSetting.ServiceName} finished");
            Console.ForegroundColor = consoleColor;
        }


        private static void ExtractZipArchiveStreamTo(Stream zipArchiveStream, string extractionDirectory)
        {
            Directory.CreateDirectory(extractionDirectory);

            using (var zipArchive = new ZipArchive(zipArchiveStream, ZipArchiveMode.Read))
            {
                foreach (var zipArchiveEntry in zipArchive.Entries)
                {
                    if (string.IsNullOrWhiteSpace(zipArchiveEntry.Name))
                    {
                        continue;
                    }

                    var unzipFilePath = Path.Combine(extractionDirectory, zipArchiveEntry.FullName);

                    var directory = Path.GetDirectoryName(unzipFilePath);

                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    zipArchiveEntry.ExtractToFile(unzipFilePath);
                }
            }
        }
    }
}