using System.Threading.Tasks;

namespace Com.CI.Infrastructure.Impls
{
    public class SignalRClient : ISignalRClient
    {
        private readonly IEventBus eventBus;

        public SignalRClient(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public Task PublishAsync(CIMessage message)
        {
            return eventBus.PublishAsync(QueueNames.SignalRQueueName, message);
        }
    }
}
