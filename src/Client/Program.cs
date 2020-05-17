using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazor.Extensions.Logging;
using Chat.Shared;
using Chat.Shared.SignalR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Chat.Client
{
    public class Program
    {
        private static readonly TimeSpan[] ReconnectDelays =
            {TimeSpan.Zero, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)};

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            ConfigureServices(builder.Services, builder.HostEnvironment.BaseAddress);
            await builder.Build().RunAsync();
        }

        private static void ConfigureServices(IServiceCollection collection, string baseAdders)
        {
            collection.AddTransient(sp => new HttpClient {BaseAddress = new Uri(baseAdders)});
            collection.AddGrpcClient(channel => new EmailValidator.EmailValidatorClient(channel));
            collection.AddGrpcClient(channel => new RsaGenerator.RsaGeneratorClient(channel));

            collection.AddScoped(provider =>
            {
                var navigationManager = provider.GetService<NavigationManager>();
                var hubConnection = new HubConnectionBuilder()
                    .WithUrl(navigationManager.ToAbsoluteUri("/chatHub"))
                    .WithAutomaticReconnect(ReconnectDelays)
                    .Build();
                return new ChatHubClient(hubConnection);
            });

            collection.AddLogging(builder => builder.AddBrowserConsole().SetMinimumLevel(LogLevel.Information));
        }
    }
}