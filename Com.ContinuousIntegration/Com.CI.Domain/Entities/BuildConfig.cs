using Com.CI.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.CI.Domain
{
    public class BuildConfig
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid BuildConfigId { get; set; }
        public bool Active { get; set; }
        public string RepositoryId { get; set; }
        public string BranchId { get; set; }
        public ServiceTypes ServiceType { get; set; }
        public bool RestoreNuget { get; set; }
     
        public string SolutionPath { get; set; }
        public string GitUri { get; set; }
        public string GitUriWithCredentials { get; set; }

        public virtual ICollection<ServiceConfig> ServiceConfigs { get; set; }
    }
}
