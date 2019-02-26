namespace Com.CI.Infrastructure
{
    public class BuildParameters
    {
        public string RepositoryId { get; set; }
        public string BranchId { get; set; }
        public bool RestoreNuget { get; set; }
        public string SolutionPath { get; set; }
        public ServiceTypes ServiceType { get; set; }
    }
}
