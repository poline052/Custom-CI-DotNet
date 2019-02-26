namespace Com.CI.Infrastructure
{
    public interface ICIProcess
    {
        int Execute(string executablePath, string arguments, string repositoryId, string branchId);
    }
}
