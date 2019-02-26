using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.CI.Infrastructure.Impls
{
    public static class CISubscriberRepository
    {
        private static IList<CISubscriber> _subscribers;

        static CISubscriberRepository()
        {
            _subscribers = new List<CISubscriber>();
        }

        public static bool SubscriberExists(string connectionId, string repositoryId, string branchId)
        {
            var subscriberExists = _subscribers.Any(s => s.ConnectionId.Equals(connectionId, StringComparison.InvariantCultureIgnoreCase) && s.RepositoryId.Equals(repositoryId, StringComparison.InvariantCultureIgnoreCase) && s.BranchId.Equals(branchId, System.StringComparison.InvariantCultureIgnoreCase));
            return subscriberExists;
        }

        public static void AddSubscriber(CISubscriber subscriber)
        {
            var subscriberExists = SubscriberExists(subscriber.ConnectionId, subscriber.RepositoryId, subscriber.BranchId);

            if (!subscriberExists)
            {
                _subscribers.Add(subscriber);
            }
        }

        public static void RemoveSubscriber(string connectionId)
        {
            var subscriber = _subscribers.SingleOrDefault(s => s.ConnectionId.Equals(connectionId, StringComparison.InvariantCultureIgnoreCase));

            if (subscriber != null)
                _subscribers.Remove(subscriber);
        }

        public static IEnumerable<CISubscriber> GetSubscribers(string repositoryId, string branchId)
        {
            return _subscribers.Where(s => s.RepositoryId.Equals(repositoryId, StringComparison.InvariantCultureIgnoreCase) && s.BranchId.Equals(branchId, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
