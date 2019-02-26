using System.Threading.Tasks;

namespace Com.CI.Infrastructure
{
    public interface IEventBus
    {
        Task PublishAsync(string queueName, IEvent @event);
    }
}
