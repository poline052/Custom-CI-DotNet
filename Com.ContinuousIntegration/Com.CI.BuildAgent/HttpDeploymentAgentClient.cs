using Com.CI.Infrastructure;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Com.CI.BuildAgent
{
    public class HttpDeploymentAgentClient : IDeploymentAgentClient
    {
        private readonly ICILogger ciLogger;
        private readonly IHttpClient httpClient;
        private readonly ISignalRClient signalRClient;

        public HttpDeploymentAgentClient(IHttpClient httpClient, ISignalRClient signalRClient, ICILogger ciLogger)
        {
            this.ciLogger = ciLogger;
            this.httpClient = httpClient;
            this.signalRClient = signalRClient;
        }

        public async Task<DeployCommandResponse> DeployAsync(DeploymentParameters deploymentParameters)
        {
            var multipartFormValues = new[]
            {
                 new KeyValuePair<string, string>("ServiceName", deploymentParameters.ServiceName),
                 new KeyValuePair<string, string>("PhysicalPath", deploymentParameters.PhysicalPath),
                 new KeyValuePair<string, string>("ServiceType", deploymentParameters.ServiceType.ToString()),
                 new KeyValuePair<string, string>("ServiceDescription", deploymentParameters.ServiceDescription),
                 new KeyValuePair<string, string>("WebServiceHostName", deploymentParameters.WebServiceHostName),
                 new KeyValuePair<string, string>("BindingType", deploymentParameters.BindingType),
                 new KeyValuePair<string, string>("Port", deploymentParameters.Port.ToString())
            };

            var packageName = $"{deploymentParameters.ServiceName}.zip";

            var temporaryDeploymentArrchievePath = Path.Combine(GetTemporaryDirectory(), packageName);

            ciLogger.WriteInfo($"Packing build output..");

            ZipFile.CreateFromDirectory(deploymentParameters.BuildOutputPath, temporaryDeploymentArrchievePath);

            using (var deploymentPackageStream = File.OpenRead(temporaryDeploymentArrchievePath))
            {
                ciLogger.WriteInfo($"Sending build output to deployment agent {deploymentParameters.AgentUrl}");
                return await httpClient.PostAsync<DeployCommandResponse>(deploymentParameters.AgentUrl, multipartFormValues, deploymentPackageStream);
            }

        }
        private static string GetTemporaryDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}
