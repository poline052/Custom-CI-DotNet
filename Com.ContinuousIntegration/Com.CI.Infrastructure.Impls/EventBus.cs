using System.Threading.Tasks;

namespace Com.CI.Infrastructure.Impls
{
    public class EventBus : IEventBus
    {
        private readonly IMessageQueue messageQueue;

        public EventBus(IMessageQueue messageQueue)
        {
            this.messageQueue = messageQueue;
        }

        public async Task PublishAsync(string queueName, IEvent @event)
        {
            await Task.Run(() => { messageQueue.Publish(queueName, @event); });
        }
    }
}
