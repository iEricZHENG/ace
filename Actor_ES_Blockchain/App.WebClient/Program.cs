using Lib;
using Orleans.Runtime;
using Orleans;
using App.IActor;
using App.Core.Ioc;
using Orleans.Hosting;
using Orleans.Configuration;

namespace App.WebClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.UseUrls("http://0.0.0.0:5001/");
            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<IClientFactory, ClientFactory>();

            StartClientWithRetries().GetAwaiter().GetResult();
            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
        private static async Task<IClusterClient> StartClientWithRetries(int initializeAttemptsBeforeFailing = 5)
        {
            int attempt = 0;
            IClusterClient client;
            var invariant = "Npgsql";
            const string connectionString = "Server=47.242.197.74;Port=5432;Database=Orleans;User Id=postgres;Password=123456;Pooling=false;";
            while (true)
            {
                try
                {
                    client = await ClientFactory.Build(() =>
                    {
                        var builder = new ClientBuilder()
                               //.UseLocalhostClustering()
                               .UseAdoNetClustering(options =>
                               {
                                   options.ConnectionString = connectionString;
                                   options.Invariant = invariant;
                               })
                               .Configure<ClusterOptions>(options =>
                               {
                                       options.ClusterId = "DemoCluster";
                                       options.ServiceId = "Demo";                                   
                               })                             
                            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IHello).Assembly).WithReferences())
                            .ConfigureLogging(logging => logging.AddConsole());
                        return builder;
                    });
                    Console.WriteLine("Client successfully connect to silo host");
                    break;
                }
                catch (SiloUnavailableException)
                {
                    attempt++;
                    Console.WriteLine($"Attempt {attempt} of {initializeAttemptsBeforeFailing} failed to initialize the Orleans client.");
                    if (attempt > initializeAttemptsBeforeFailing)
                    {
                        throw;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(4));
                }
            }

            return client;
        }
    }
}