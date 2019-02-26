using System.Threading.Tasks;

namespace Com.CI.Infrastructure
{
    public interface IDeploymentAgentClient
    {
        Task<DeployCommandResponse> DeployAsync(DeploymentParameters deploymentAgentParameters);
    }
}
