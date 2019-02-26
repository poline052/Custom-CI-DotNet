namespace Com.CI.GitPushEventListener
{
    public class GitBranchPushedEventHandlerResponse
    {
        public int StatusCode { get; set; }

        public static GitBranchPushedEventHandlerResponse CreateSuccessResponse()
        {
            return new GitBranchPushedEventHandlerResponse { StatusCode = 200 };
        }

        public static GitBranchPushedEventHandlerResponse CreateBadEventResponse()
        {
            return new GitBranchPushedEventHandlerResponse { StatusCode = 500 };
        }
    }
}