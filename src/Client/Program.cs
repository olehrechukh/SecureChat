using System;
using System.Net.Http;
using System.Threading.Tasks;
using EmailChecker.Shared;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace EmailChecker.Client
{
    public class Program
    {
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
            collection.AddSingleton(services =>
            {
                // Create a gRPC-Web channel pointing to the backend server
                var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
                var baseUri = services.GetRequiredService<NavigationManager>().BaseUri;
                var channel = GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions {HttpClient = httpClient});

                // Now we can instantiate gRPC clients for this channel
                return new EmailValidator.EmailValidatorClient(channel);
            });
        }
    }
}