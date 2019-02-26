using Com.CI.Domain;
using Com.CI.Infrastructure;
using Com.CI.Infrastructure.Impls;
using SimpleInjector;
using System;
using System.Configuration;

namespace Com.CI.BuildAgent
{
    class Program
    {
        private const string ServiceEndPointName = "Com.CI.BuildAgent";

        static void Main(string[] args)
        {
            var logRootPath = ConfigurationManager.AppSettings["LogRootPath"];

            CILogger.Setup(logRootPath, ServiceEndPointName, "INFO");

            var container = SetupContainer();

            var messageQueue = container.GetInstance<IMessageQueue>();

            messageQueue.Subscribe(new[] { QueueNames.BuildAgentQueueName});

            Console.WriteLine("Press enter to exit");

            Console.ReadLine();
            messageQueue.Dispose();
        }

        


        private static Container SetupContainer()
        {
            var container = new Container();

            InitializeContainer(container);

            container.Verify();

            return container;
        }

        private static void InitializeContainer(Container container)
        {
            container.Register<ISignalRClient, SignalRClient>();
            container.Register<ISourceProvider, GitSourceProvider>();
            container.Register<IOutputBuilder, MsBuildOutputBuilder>();
            container.Register<IDeploymentAgentClient, HttpDeploymentAgentClient>();

            container.Register<ICILogger, CILogger>();
            container.Register<ICIProcess, CIProcess>();
            container.Register<IHttpClient, CIHttpClient>();
            container.Register<ICIRepository, CIRepository>();

            container.RegisterCollection(typeof(IEvent), new[] { typeof(BranchPushedEvent).Assembly });

            container.Register<IEventBus, EventBus>();

            container.RegisterSingleton<IMessageQueue>(() =>
            {
                var ciLogger = container.GetInstance<ICILogger>();

                var rabbitMqConnectionString = ConfigurationManager.AppSettings["RabbitMqConnectionString"];

                return new MessageQueue(container, ciLogger, ServiceEndPointName, rabbitMqConnectionString);
            });

            container.Register<IDataMapper, DataMapper>();

            container.Register(typeof(IEventHandler<>), new[] { typeof(BranchPushedEventHandler).Assembly });
        }
    }
}
