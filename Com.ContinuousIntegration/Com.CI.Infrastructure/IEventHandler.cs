using System.Threading.Tasks;

namespace Com.CI.Infrastructure
{
    public interface IEventHandler<Event, Response> where Event : IEvent
    {
        Task<Response> HandleAsync(Event @event);
    }

    public interface IEventHandler<IEvent> 
    {
        Task HandleAsync(IEvent @event);
    }
}
