using System;
using System.Net.Http;
using Chat.Shared;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Client
{
    public static class Extensions
    {
        public static string GetOutputString(this EmailValidateReply reply, string email)
        {
            return reply.IsSuccess ? $"'{email}' is valid email." : $"'{email}' is invalid email. {reply.ErrorMessage}";
        }

        public static void AddGrpcClient<T>(this IServiceCollection collection, Func<ChannelBase, ClientBase<T>> func)
            where T : ClientBase<T>
        {
            Console.WriteLine($"Create grpc client {typeof(T)}");
            collection.AddSingleton(services =>
            {
                // Create a gRPC-Web channel pointing to the backend server
                var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
                var baseUri = services.GetRequiredService<NavigationManager>().BaseUri;
                var channel = GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions {HttpClient = httpClient});

                // Now we can instantiate gRPC clients for this channel
                return (T) func(channel);
            });
        }
    }

    enum CommunicationState
    {
        Unknown,
        Unconnected,
        WaitConnect,
        Connected,
        Disconnected,
        FailedToConnect
    }

    public class Message
    {
        public string Css => IsMy ? "sent" : "received";

        public Message(string connectionId, string text, bool isMy)
        {
            ConnectionId = connectionId;
            Text = text;
            IsMy = isMy;
        }

        public string ConnectionId { get; }
        public string Text { get; }
        public bool IsMy { get; }
    }
}