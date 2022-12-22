using App.Actor;
using App.Core.Ioc;
using App.Framework.MQ;
using App.Handlers;
using App.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace App.Host
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            RepositoryManager.Init();
            IocManage.Build();//初始化Ioc容器
            HandlerManager.Init();
            await SubscribeManage.Start("Core");
            var host = CreateHost();
            await host.RunAsync();
        }
        private static IHost CreateHost()
        {
#pragma warning disable CS0219 
            var invariant = "Npgsql";
            const string connectionString = "Server=47.242.197.74;Port=5432;Database=Orleans;User Id=postgres;Password=123456;Pooling=false;";
#pragma warning restore CS0219 
            return new HostBuilder()
                .UseOrleans(
                    siloBuilder => siloBuilder
                        //.UseLocalhostClustering()                      
                        .Configure<ClusterOptions>(options =>
                         {
                             options.ClusterId = "DemoCluster";
                             options.ServiceId = "Demo";
                         })
                         .UseAdoNetClustering(options =>
                          {
                              options.ConnectionString = connectionString;
                              options.Invariant = invariant;
                          })
                        .ConfigureEndpoints(11111, 30000)
                        //.Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                        .ConfigureApplicationParts(parts => parts
                            .AddApplicationPart(typeof(Hello).Assembly).WithReferences())
                        .UseDashboard())
                .ConfigureLogging(
                    logging => logging
                        .SetMinimumLevel(LogLevel.Information)
                        .AddConsole())
                .ConfigureServices(services =>
                {
                    //services.AddSingleton(serviceProvider =>
                    //new DatadogMetricTelemetryConsumer(
                    //    new[] { "App.Requests.Total.Requests.Current" },
                    //    new StatsdConfig
                    //    {
                    //        StatsdServerName = "127.0.0.1",
                    //        StatsdPort = 8125
                    //    }));
                    //services.Configure<TelemetryOptions>(
                    //    telemetryOptions => telemetryOptions.AddConsumer<DatadogMetricTelemetryConsumer>());
                    //using (var dogStatsdService = new DogStatsdService())
                    //{                        
                    //    dogStatsdService.ServiceCheck("Service.check.name", 0, message: "Application is OK.", tags: new[] { "env:dev" });
                    //}
                })
                .Build();
        }
    }
}