namespace Com.CI.Infrastructure
{
    public class SourceDownloadParameters
    {
        public string RepositoryId { get; set; }
        public string BranchId { get; set; }
        public string GitUri { get; set; }
        public string GitUriWithCredentials { get; set; }

    }
}
