using System;
using System.Collections.Generic;

namespace Com.CI.Domain
{
    public interface ICIRepository
    {
        IEnumerable<BuildConfig> GetBuildConfigs(string repositoryId, string branchId);

        IEnumerable<ServiceConfig> GetServiceConfigsForCurrentBuild(Guid buildConfigId);

        DeploymentAgent GetDeploymentAgent(Guid deploymentAgentId);
    }
}
