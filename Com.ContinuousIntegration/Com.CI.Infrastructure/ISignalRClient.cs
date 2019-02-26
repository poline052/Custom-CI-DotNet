using System.Threading.Tasks;

namespace Com.CI.Infrastructure
{
    public interface ISignalRClient
    {
        Task PublishAsync(CIMessage message);
    }
}
