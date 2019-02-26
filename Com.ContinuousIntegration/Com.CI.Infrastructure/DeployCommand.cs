using System.IO;

namespace Com.CI.Infrastructure
{
    public class DeployCommand
    {
        public string ServiceName { get; set; }
        public string PhysicalPath { get; set; }
        public ServiceTypes ServiceType { get; set; }
        public Stream DeploymentPackageStream { get; set; }
        public string ServiceDescription { get; set; }
        public string WebServiceHostName { get; set; }
        public string BindingType { get; set; }
        public int Port { get; set; }
    }
}
