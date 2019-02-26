using Com.CI.Infrastructure;
using Com.CI.Infrastructure.Impls;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Com.CI.DeploymentAgent
{
    public class DeploymentController : ApiController
    {
        private readonly ICommandHandler<DeployCommand, DeployCommandResponse> deployCommandHandler;
        public DeploymentController(ICommandHandler<DeployCommand, DeployCommandResponse> deployCommandHandler)
        {
            this.deployCommandHandler = deployCommandHandler;
        }

        [HttpPost]
        public async Task<DeployCommandResponse> Deploy()
        {
            var provider = await Request.Content.ReadAsMultipartAsync(new CIMultipartStreamProvider());

            var deployCommand = new DeployCommand
            {
                DeploymentPackageStream = await provider.Files.First().ReadAsStreamAsync(),

                BindingType = provider.FormData["BindingType"] ?? string.Empty,
                PhysicalPath = provider.FormData["PhysicalPath"] ?? string.Empty,
                Port = int.Parse(provider.FormData["Port"] ?? "80"),
                ServiceDescription = provider.FormData["ServiceDescription"] ?? string.Empty,
                ServiceName = provider.FormData["ServiceName"] ?? string.Empty,
                ServiceType = (ServiceTypes)Enum.Parse(typeof(ServiceTypes), provider.FormData["ServiceType"] ?? string.Empty),
                WebServiceHostName = provider.FormData["WebServiceHostName"] ?? string.Empty

            };

            return deployCommandHandler.Handle(deployCommand);
        }
    }
}