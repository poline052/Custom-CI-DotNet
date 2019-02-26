using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.CI.Domain
{
    public class CIRepository : ICIRepository
    {
        private readonly ComCIContext seliseCiContext;
        public CIRepository()
        {
            seliseCiContext = new ComCIContext();
        }

        public IEnumerable<BuildConfig> GetBuildConfigs(string repositoryId, string branchId)
        {
            return seliseCiContext.BuildConfigs.Where(bc => bc.BranchId == branchId && bc.RepositoryId == repositoryId);
        }

        public DeploymentAgent GetDeploymentAgent(Guid deploymentAgentId)
        {
            return seliseCiContext.DeploymentAgents.SingleOrDefault(de => de.DeploymentAgentId == deploymentAgentId);
        }

        public IEnumerable<ServiceConfig> GetServiceConfigsForCurrentBuild(Guid buildConfigId)
        {
            return seliseCiContext.ServiceConfigs.Where(sc => sc.BuildConfigId == buildConfigId);
        }
    }
}
