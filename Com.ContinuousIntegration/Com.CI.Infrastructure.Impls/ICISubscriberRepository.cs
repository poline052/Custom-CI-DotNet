using System.Collections.Generic;

namespace Com.CI.Infrastructure.Impls
{
    public interface ICISubscriberRepository
    {
        void AddSubscriber(CISubscriber subscriber);
        IEnumerable<CISubscriber> GetSubscribers(string repositoryId, string branchId);
        void RemoveSubscriber(string connectionId);
        bool SubscriberExists(string connectionId, string repositoryId, string branchId);
       
    }
}