using Microsoft.Owin.Hosting;
using Owin;
using Com.CI.Infrastructure;
using Com.CI.Infrastructure.Impls;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using SimpleInjector.Lifestyles;
using System;
using System.Configuration;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace Com.CI.DeploymentAgent
{
    public class Startup
    {
        private const string ServiceEndPointName = "Com.CI.DeploymentAgent";

        public void Configuration(IAppBuilder app)
        {
            var logRootPath = ConfigurationManager.AppSettings["LogRootPath"];

            CILogger.Setup(logRootPath, ServiceEndPointName, "INFO");

            var httpConfiguration = new HttpConfiguration();

            RegisterRoutes(httpConfiguration);

            var container = SetupContainer(httpConfiguration);


            AttachTraceExceptionLogger(httpConfiguration);

            app.UseWebApi(httpConfiguration);

            

        }
        public void AttachTraceExceptionLogger(HttpConfiguration httpConfiguration)
        {
            httpConfiguration.Services.Add(typeof(IExceptionLogger), new TraceExceptionLogger());
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
            httpConfiguration.MapHttpAttributeRoutes();
            httpConfiguration.Routes.MapHttpRoute("api", "{controller}/{id}", new { id = RouteParameter.Optional });
        }
        private static void InitializeContainer(Container container)
        {
            container.Register<ICILogger, CILogger>();
            container.Register<IDataMapper, DataMapper>();
            container.Register<IServiceInstallerProvider, ServiceInstallerProvider>();


            container.Register(typeof(ICommandHandler<,>), new[] { typeof(DeployCommandHandler).Assembly });
            container.RegisterCollection(typeof(IInstaller), new[] { typeof(WebServiceInstaller).Assembly });
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