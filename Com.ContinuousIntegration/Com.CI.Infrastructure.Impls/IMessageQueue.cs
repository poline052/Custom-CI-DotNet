using System;
using System.Collections.Generic;

namespace Com.CI.Infrastructure.Impls
{
    public interface IMessageQueue : IDisposable
    {
        void Publish(string queueName, object message);
        void Subscribe(IEnumerable<string> queueNames);
        void Subscribe(string queueName, EventHandler<CIMessage> handler);
    }


   

}
