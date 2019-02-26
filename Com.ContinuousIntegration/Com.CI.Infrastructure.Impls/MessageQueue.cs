using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.CI.Infrastructure.Impls
{
    public class MessageQueue : IMessageQueue
    {
        private List<IModel> channels;
        private IConnection connection;
        private readonly Container container;


        private readonly string endPointName;
        private readonly ICILogger vorwerkLogger;
        private readonly string rabbitMqConnectionString;

        public MessageQueue(Container container, ICILogger cilogger, string endPointName, string rabbitMqConnectionString)
        {
            this.container = container;
            channels = new List<IModel>();
            vorwerkLogger = cilogger;
            this.endPointName = endPointName;
            this.rabbitMqConnectionString = rabbitMqConnectionString;

            Connect();
        }

        public void Publish(string queueName, object message)
        {
            var mqMessage = new Message
            {
                EndPoint = endPointName,
                Payload = JsonConvert.SerializeObject(message),
                Type = message.GetType().Name
            };

            var serilizedMessage = mqMessage.Serialize();

            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                var body = Encoding.UTF8.GetBytes(serilizedMessage);

                channel.BasicPublish(exchange: string.Empty, routingKey: queueName, basicProperties: null, body: body);

            }
            var publishLogMessage = $"Message of type {mqMessage.Type} with payload\n {mqMessage.Payload} published from endpoint: {mqMessage.EndPoint}";

            vorwerkLogger.WriteInfo(publishLogMessage);
        }




        private async void GetInstenceAndExecute(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body;

            var messageBody = Encoding.UTF8.GetString(body);

            var message = Message.InitializeFromBase64EncodedString(messageBody);

            var eventInstance = container.GetAllInstances<IEvent>().SingleOrDefault(evnt => evnt.GetType().Name.Equals(message.Type, StringComparison.InvariantCultureIgnoreCase));

            if (eventInstance != null)
            {
                var eventType = eventInstance.GetType();

                var genericEventHandlerTypeType = typeof(IEventHandler<>).MakeGenericType(new[] { eventType });

                dynamic commandHandler = container.GetInstance(genericEventHandlerTypeType);

                var messageEvent = JsonConvert.DeserializeObject(message.Payload, eventType);

                await commandHandler.HandleAsync(messageEvent as dynamic);

                vorwerkLogger.WriteInfo($"Event of type: {message.Type} with payload\n {message.Payload} \nhandled at endpoint {endPointName}");
            }
            else
            {
                vorwerkLogger.WriteInfo($"Event of type: {message.Type} ignored at endpoint {endPointName}");
            }
        }

        private void Connect()
        {
            var factory = new ConnectionFactory()
            {
                Uri = rabbitMqConnectionString,
                RequestedHeartbeat = 15,
                AutomaticRecoveryEnabled = true
            };

            connection = factory.CreateConnection();
        }

        public void Dispose()
        {
            foreach (var channel in channels)
            {
                channel.Dispose();
            }

            if (connection != null)
                connection.Dispose();
        }

        public void Subscribe(IEnumerable<string> queueNames)
        {
            foreach (var queueName in queueNames)
            {
                var channel = connection.CreateModel();

                channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                var basicConsumer = new EventingBasicConsumer(channel);

                basicConsumer.Received += GetInstenceAndExecute;

                channel.BasicConsume(queueName, true, basicConsumer);

                channels.Add(channel);
            }
        }


        public void Subscribe(string queueName, EventHandler<CIMessage> handler)
        {
            var channel = connection.CreateModel();

            channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var basicConsumer = new EventingBasicConsumer(channel);

            basicConsumer.Received += (object o, BasicDeliverEventArgs e) =>
            {
                var body = e.Body;

                var messageBody = Encoding.UTF8.GetString(body);

                var message = Message.InitializeFromBase64EncodedString(messageBody);

                var ciMessage = JsonConvert.DeserializeObject<CIMessage>(message.Payload);

                handler(this, ciMessage);

            };

            channel.BasicConsume(queueName, true, basicConsumer);

            channels.Add(channel);

        }

    }
}
