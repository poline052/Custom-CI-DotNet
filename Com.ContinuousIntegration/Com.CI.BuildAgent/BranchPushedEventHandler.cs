using Com.CI.Domain;
using Com.CI.Infrastructure;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Com.CI.BuildAgent
{
    public class BranchPushedEventHandler : IEventHandler<BranchPushedEvent>
    {
        private readonly IDataMapper dataMapper;
        private readonly ICIRepository ciRepository;
        private readonly IOutputBuilder outputBuilder;
        private readonly ISignalRClient signalRClient;
        private readonly ISourceProvider sourceProvider;
        private readonly IDeploymentAgentClient agentClient;

        public BranchPushedEventHandler(
            ISignalRClient signalRClient,
            ISourceProvider sourceProvider,
            IOutputBuilder outputBuilder,
            IDeploymentAgentClient agentClient,
            ICIRepository ciRepository,
            IDataMapper dataMapper)
        {
            this.dataMapper = dataMapper;
            this.agentClient = agentClient;
            this.signalRClient = signalRClient;
            this.outputBuilder = outputBuilder;
            this.sourceProvider = sourceProvider;
            this.ciRepository = ciRepository;
        }

        public async Task HandleAsync(BranchPushedEvent branchPushedEvent)
        {
            //jaforkhan.selisestaging.com
            var buildConfigs = ciRepository.GetBuildConfigs(branchPushedEvent.RepositoryId, branchPushedEvent.BranchId);

            foreach (var buildConfig in buildConfigs)
            {
                if (buildConfig == null)
                {
                    var ciMessage = CIMessage.Create(branchPushedEvent.RepositoryId, branchPushedEvent.BranchId, $"No build config found for RepositoryId: {branchPushedEvent.RepositoryId} and BranchId: {branchPushedEvent.BranchId}", true);

                    await signalRClient.PublishAsync(ciMessage);

                    return;
                }

                if (!buildConfig.Active)
                {
                    var ciMessage = CIMessage.Create(branchPushedEvent.RepositoryId, branchPushedEvent.BranchId, $"Build config with Id {buildConfig.BuildConfigId} is not active for RepositoryId: {branchPushedEvent.RepositoryId} and BranchId: {branchPushedEvent.BranchId}", true);

                    await signalRClient.PublishAsync(ciMessage);

                    return;
                }

                var sourceDownloadParameters = dataMapper.Map<SourceDownloadParameters, BuildConfig>(buildConfig);

                var downloadSourceResult = sourceProvider.DownloadSource(sourceDownloadParameters);

                if (!downloadSourceResult.Downloaded)
                {
                    var ciMessage = CIMessage.Create(branchPushedEvent.RepositoryId, branchPushedEvent.BranchId, "Error in downloading source", true);

                    await signalRClient.PublishAsync(ciMessage);
                }

                var buildParameters = dataMapper.Map<BuildParameters, BuildConfig>(buildConfig);

                var buildOutputResult = outputBuilder.Build(buildParameters, downloadSourceResult.TemporarySourceDownloadDirectory);

                if (!buildOutputResult.Built)
                {
                    var ciMessage = CIMessage.Create(branchPushedEvent.RepositoryId, branchPushedEvent.BranchId, "Error in building output", true);

                    await signalRClient.PublishAsync(ciMessage);
                }

                var serviceConfigs = ciRepository.GetServiceConfigsForCurrentBuild(buildConfig.BuildConfigId);

                foreach (var serviceConfig in serviceConfigs)
                {

                    if (serviceConfig.Active == false)
                    {
                        continue;
                    }

                    var deploymentAgent = ciRepository.GetDeploymentAgent(serviceConfig.DeploymentAgentId);

                    var deploymentParameters = CreateDeploymentParameters(buildConfig, serviceConfig, deploymentAgent, buildOutputResult);

                    var deploymentCommandResponse = await agentClient.DeployAsync(deploymentParameters);

                    if (!deploymentCommandResponse.StatusCode.Equals(200))
                    {
                        var ciMessage = CIMessage.Create(branchPushedEvent.RepositoryId, branchPushedEvent.BranchId, "Deployment failed", true);

                        await signalRClient.PublishAsync(ciMessage);
                    }
                    else
                    {
                        var ciMessage = CIMessage.Create(branchPushedEvent.RepositoryId, branchPushedEvent.BranchId, "Deployment successeded");

                        await signalRClient.PublishAsync(ciMessage);
                    }
                }
            }
        }

        private DeploymentParameters CreateDeploymentParameters(BuildConfig buildConfig, ServiceConfig serviceConfig, DeploymentAgent deploymentAgent, BuildOutputResult buildOutputResult)
        {
            var directoryForCurrentBuild = $"{buildConfig.RepositoryId}-{buildConfig.BranchId}-{DateTime.UtcNow.ToString("s").Replace(":", ".")}";

            var physicalPath = buildConfig.ServiceType == ServiceTypes.WebService ?
                Path.Combine(deploymentAgent.WebServiceRoot, serviceConfig.ServiceName, directoryForCurrentBuild)
                : Path.Combine(deploymentAgent.WindowsServiceRoot, serviceConfig.ServiceName, directoryForCurrentBuild);

            var deploymentAgentParameters = new DeploymentParameters
            {
                AgentUrl = deploymentAgent.AgentUrl,
                BuildOutputPath = buildOutputResult.BuildOutputDirectory,
                PhysicalPath = physicalPath,
                ServiceType = buildConfig.ServiceType,
                BindingType = serviceConfig.BindingType,
                Port = serviceConfig.Port,
                ServiceDescription = serviceConfig.ServiceDescription,
                ServiceName = serviceConfig.ServiceName,
                WebServiceHostName = serviceConfig.WebServiceHostName
            };

            return deploymentAgentParameters;
        }

    }
}
