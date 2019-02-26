using System;

namespace Com.CI.Infrastructure
{
    public class GitBranchPushedEvent : IEvent
    {
      
        public string RepositoryId { get; set; }
        public bool Valid { get; set; }
        public string BranchId { get; set; }
        public string PushType { get; set; }
        public string CommitHash { get; set; }
        public string Author { get; set; }
        public string Message { get; set; }
        public DateTime PushDate { get; set; }
    }
}
