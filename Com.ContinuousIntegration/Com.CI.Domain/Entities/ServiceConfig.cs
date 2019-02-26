using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.CI.Domain
{
    public class ServiceConfig
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ServiceConfigId { get; set; }
        public Guid BuildConfigId { get; set; }
        public Guid DeploymentAgentId { get; set; }
        public bool Active { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }
        public string WebServiceHostName { get; set; }
        public string BindingType { get; set; }
        public int Port { get; set; }


        public virtual BuildConfig BuildConfig { get; set; }
        public virtual DeploymentAgent DeploymentAgent { get; set; }
    }
}
