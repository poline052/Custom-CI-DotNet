using Com.CI.Infrastructure;

namespace Com.CI.GitPushEventListener
{
    public class BranchPushNotifiedEvent : IEvent
    {
        public string PushEventMessageJson { get; set; }
    }
}   