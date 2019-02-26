using Com.CI.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Com.CI.GitPushEventListener
{
    public class GitBranchPushedEventHandler : IEventHandler<BranchPushNotifiedEvent, GitBranchPushedEventHandlerResponse>
    {
        private readonly ICILogger ciLogger;
        private readonly IEventBus eventBus;
        private readonly IDataMapper dataMapper;

        public GitBranchPushedEventHandler(IEventBus eventBus, ICILogger ciLogger, IDataMapper dataMapper)
        {
            this.eventBus = eventBus;
            this.ciLogger = ciLogger;
            this.dataMapper = dataMapper;
        }

        public async Task<GitBranchPushedEventHandlerResponse> HandleAsync(BranchPushNotifiedEvent branchPushNotifiedEvent)
        {
            var pushEventMessage = PushEventMessageFactory.Create(branchPushNotifiedEvent.PushEventMessageJson);

            if (!pushEventMessage.Valid)
            {
                ciLogger.WriteWarning($"Message {pushEventMessage} is discarded since it is invalid", new InvalidOperationException("Unable to handle message"));
                return GitBranchPushedEventHandlerResponse.CreateBadEventResponse();
            }

            ciLogger.WriteInfo($"Push event received from Bitbucket.org for Repository: {pushEventMessage.RepositoryId} Branch: {pushEventMessage.BranchId}");

            var branchPushedEvent = dataMapper.Map<BranchPushedEvent, GitBranchPushedEvent>(pushEventMessage);

            await eventBus.PublishAsync(QueueNames.BuildAgentQueueName, branchPushedEvent);

            return GitBranchPushedEventHandlerResponse.CreateSuccessResponse();
        }
    }



}