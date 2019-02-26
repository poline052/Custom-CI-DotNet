using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Owin;
using Com.CI.Infrastructure;
using Com.CI.Infrastructure.Impls;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using SimpleInjector.Lifestyles;
using System;
using System.Configuration;
using System.Net.Http.Formatting;
using System.Threading;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

[assembly: OwinStartup(typeof(Com.CI.GitPushEventListener.Startup))]
namespace Com.CI.GitPushEventListener
{

    public class Startup
    {
        private const string ServiceEndPointName = "Com.CI.GitPushEventListener";

        public void Configuration(IAppBuilder app)
        {
            var logRootPath = ConfigurationManager.AppSettings["LogRootPath"];

            CILogger.Setup(logRootPath, ServiceEndPointName, "INFO");

            var httpConfiguration = new HttpConfiguration();

            RegisterRoutes(httpConfiguration);

            var container = SetupContainer(httpConfiguration);

            var messageQueue = container.GetInstance<IMessageQueue>();

            AttachTraceExceptionLogger(httpConfiguration);

            app.UseWebApi(httpConfiguration);

            EnsureDisposalOfMessageQueueOnAppShutdown(app, messageQueue);

            InitCISignalR(app, container);

            SetupStaticFileSupport(app);



        }

        private static void InitCISignalR(IAppBuilder app, Container container)
        {
            GlobalHost.DependencyResolver.Register(typeof(CISignalRHub), container.GetInstance<CISignalRHub>);

            app.Map(
               "/echo",
               map =>
               {
                   map.UseCors(CorsOptions.AllowAll);
                   var hubConfiguration = new HubConfiguration
                   {
                       EnableDetailedErrors = true
                   };
                   map.RunSignalR<CISignalRHub>(hubConfiguration);
               });

            app.MapSignalR();
        }

        private void SetupStaticFileSupport(IAppBuilder app)
        {
            var physicalFileSystem = new PhysicalFileSystem(@"E:\Com.ContinuousIntegration\Com.CI.GitPushEventListener\www");
            var options = new FileServerOptions
            {
                EnableDefaultFiles = true,
                FileSystem = physicalFileSystem
            };
            options.StaticFileOptions.FileSystem = physicalFileSystem;
            options.StaticFileOptions.ServeUnknownFileTypes = true;
            options.DefaultFilesOptions.DefaultFileNames = new[]
            {
                "CILiveEventsViwer.html"
            };

            app.UseFileServer(options);
        }

        public void AttachTraceExceptionLogger(HttpConfiguration httpConfiguration)
        {
            httpConfiguration.Services.Add(typeof(IExceptionLogger), new TraceExceptionLogger());
        }
        private static void EnsureDisposalOfMessageQueueOnAppShutdown(IAppBuilder app, IMessageQueue messageQueue)
        {
            var context = new OwinContext(app.Properties);

            var token = context.Get<CancellationToken>("host.OnAppDisposing");

            if (token != CancellationToken.None)
            {
                token.Register(() =>
                {
                    messageQueue.Dispose();
                });
            }
        }
        public static void SetJsonAsDefaultResponseSerilizer(HttpConfiguration httpConfiguration)
        {
            httpConfiguration.Formatters.Clear();
            httpConfiguration.Formatters.Add(new JsonMediaTypeFormatter());
        }
        private static Container SetupContainer(HttpConfiguration httpConfiguration)
        {
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            InitializeContainer(container);

            container.Verify();

            httpConfiguration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);

            return container;
        }
        private static void RegisterRoutes(HttpConfiguration httpConfiguration)
        {

            httpConfiguration.Routes.IgnoreRoute("js", "{file}.js");
            httpConfiguration.Routes.IgnoreRoute("html", "{file}.html");
            httpConfiguration.MapHttpAttributeRoutes();
            httpConfiguration.Routes.MapHttpRoute("api", "api/{controller}/{id}", new { id = RouteParameter.Optional });
        }
        private static void InitializeContainer(Container container)
        {
            container.Register<ICILogger, CILogger>();

            container.Register<IEventBus, EventBus>();

            container.RegisterSingleton<IMessageQueue>(() =>
            {
                var ciLogger = container.GetInstance<ICILogger>();

                var rabbitMqConnectionString = ConfigurationManager.AppSettings["RabbitMqConnectionString"];

                return new MessageQueue(container, ciLogger, ServiceEndPointName, rabbitMqConnectionString);
            });

            container.Register<IDataMapper, DataMapper>();

            container.Register(typeof(IEventHandler<,>), new[] { typeof(GitBranchPushedEventHandler).Assembly });

            container.RegisterSingleton<CISignalRHub>();

            container.Register<CIMessage>();

        }

        static void Main(string[] args)
        {
            var apiHost = ConfigurationManager.AppSettings["ApiHost"];

            using (WebApp.Start<Startup>(url: apiHost))
            {
                Console.WriteLine("{0} service running on {1}.", ServiceEndPointName, apiHost);

                Console.ReadLine();
            }
        }
    }
}