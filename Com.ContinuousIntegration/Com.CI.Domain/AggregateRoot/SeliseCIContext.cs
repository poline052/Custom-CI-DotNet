using Com.CI.Infrastructure;
using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Com.CI.Domain
{
    public class ComCIContext : DbContext
    {
        public ComCIContext() : base("name=ComCIDatabaseConnectionString")
        {
            Database.SetInitializer(new ComCIContextInitializer());
        }

        public DbSet<BuildConfig> BuildConfigs { get; set; }
        public DbSet<ServiceConfig> ServiceConfigs { get; set; }
        public DbSet<DeploymentAgent> DeploymentAgents { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }


    public class ComCIContextInitializer : DropCreateDatabaseIfModelChanges<ComCIContext>
    {
        protected override void Seed(ComCIContext context)
        {
            var buildConfigForWebService = new BuildConfig
            {
                Active = true,
                BranchId = "dev",
                BuildConfigId = Guid.NewGuid(),
                GitUri = "https://*****@bitbucket.org/***/***.git",
                GitUriWithCredentials = "https://***:***@bitbucket.org/****/***.git",
                RepositoryId = "****",
                RestoreNuget = true,
                ServiceType = ServiceTypes.WebService,
                SolutionPath = @"***\***.sln"
            };

            var buildConfigForWindowsService = new BuildConfig
            {
                Active = true,
                BranchId = "test",
                BuildConfigId = Guid.NewGuid(),
                GitUri = "****@bitbucket.org/****/****.git",
                GitUriWithCredentials = "https://*****:****@bitbucket.org/***/***.git",
                RepositoryId = "******",
                RestoreNuget = true,
                ServiceType = ServiceTypes.WindowsService,
                SolutionPath = @"***\****.sln"
            };


            context.BuildConfigs.Add(buildConfigForWebService);
            context.BuildConfigs.Add(buildConfigForWindowsService);

            context.SaveChanges();

            var deploymentAgent = new DeploymentAgent
            {
                Active = true,
                AgentUrl = "http://localhost:5000/Deployment/Deploy",
                DeploymentAgentId = Guid.NewGuid(),
                WebServiceRoot = @"C:\WebServices",
                WindowsServiceRoot = @"C:\WindowsServices"
            };

            context.DeploymentAgents.Add(deploymentAgent);

            context.SaveChanges();

            var serviceConfigForWebService = new ServiceConfig
            {
                Active = true,
                BindingType = "http",
                BuildConfigId = buildConfigForWebService.BuildConfigId,
                DeploymentAgentId = deploymentAgent.DeploymentAgentId,
                Port = 80,
                ServiceConfigId = Guid.NewGuid(),
                ServiceDescription = "Test Web Service",
                ServiceName = "TestWebService",
                WebServiceHostName = "testservice.testdomain.com"
            };

            var serviceConfigForWindowsService = new ServiceConfig
            {
                Active = true,
                BindingType = string.Empty,
                BuildConfigId = buildConfigForWindowsService.BuildConfigId,
                DeploymentAgentId = deploymentAgent.DeploymentAgentId,
                Port = 0,
                ServiceConfigId = Guid.NewGuid(),
                ServiceDescription = "Test Windows Service",
                ServiceName = "TestWindowsService",
                WebServiceHostName = string.Empty
            };

            context.ServiceConfigs.Add(serviceConfigForWebService);
            context.ServiceConfigs.Add(serviceConfigForWindowsService);

            context.SaveChanges();

        }
    }

}
