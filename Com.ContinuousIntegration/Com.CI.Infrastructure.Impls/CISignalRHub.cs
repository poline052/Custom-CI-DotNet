using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace Com.CI.Infrastructure.Impls
{
    public class CISignalRHub : PersistentConnection
    {
        private readonly IMessageQueue messageQueue;

        public CISignalRHub(IMessageQueue messageQueue)
        {
            this.messageQueue = messageQueue;
            this.messageQueue.Subscribe(QueueNames.SignalRQueueName, SignalREventReceived);
        }

        protected override Task OnDisconnected(IRequest request, string connectionId, bool stopCalled)
        {
            CISubscriberRepository.RemoveSubscriber(connectionId);
            return base.OnDisconnected(request, connectionId, stopCalled);
        }

        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            return base.OnReceived(request, connectionId, data);
        }

        protected override Task OnReconnected(IRequest request, string connectionId)
        {
            var ciSubscriber = CISubscriber.CreateFromRequestQueryString(connectionId, request.QueryString);

            CISubscriberRepository.AddSubscriber(ciSubscriber);

            return base.OnReconnected(request, connectionId);
        }

        protected override Task OnConnected(IRequest request, string connectionId)
        {
            var ciSubscriber = CISubscriber.CreateFromRequestQueryString(connectionId, request.QueryString);

            CISubscriberRepository.AddSubscriber(ciSubscriber);

            return base.OnConnected(request, connectionId);
        }

        private async void SignalREventReceived(object sender, CIMessage ciMessage)
        {
            if (ciMessage != null && !string.IsNullOrWhiteSpace(ciMessage.MessageBody))
            {
                var ciSubscribers = CISubscriberRepository.GetSubscribers(ciMessage.RepositoryId, ciMessage.BranchId);

                var sendTasks = ciSubscribers.Select(cs => Connection.Send(cs.ConnectionId, ciMessage));

                await Task.WhenAll(sendTasks);
            }
        }
    }





}
