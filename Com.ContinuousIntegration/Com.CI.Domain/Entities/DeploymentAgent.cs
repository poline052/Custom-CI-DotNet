using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.CI.Domain
{
    public class DeploymentAgent
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid DeploymentAgentId { get; set; }
        public bool Active { get; set; }
        public string WebServiceRoot { get; set; }
        public string WindowsServiceRoot { get; set; }
        public string AgentUrl { get; set; }
        public virtual ICollection<ServiceConfig> ServiceConfigs { get; set; }
    }
}
