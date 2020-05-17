using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Chat.Shared.SignalR;
using ConcurrentCollections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

namespace Chat.Server.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentHashSet<string> ExistingConnection = new ConcurrentHashSet<string>();

        private static readonly ConcurrentDictionary<string, UserAgrs> Communication =
            new ConcurrentDictionary<string, UserAgrs>();

        public async Task SendMessage(string message)
        {
            if (!Communication.ContainsKey(Context.ConnectionId))
            {
                return;
            }

            var receiver = Communication[Context.ConnectionId].ConnectionId;

            if (!Communication.ContainsKey(receiver))
            {
                return;
            }

            var receiverSender = Communication[receiver].ConnectionId;
            if (receiverSender == Context.ConnectionId)
            {
                await Clients.Client(receiver).SendAsync("ReceiveMessage", Context.ConnectionId, message);
            }
        }


        // public Task SetKey(string message)
        // {
        //     ExistingConnection.AddOrUpdate(Context.ConnectionId, message, (s, s1) => message);
        //     return Task.CompletedTask;
        // }

        public async Task Disconnect(string connectionId)
        {
            Communication.TryRemove(connectionId, out _);
            Communication.TryRemove(Context.ConnectionId, out _);

            await Clients.Client(connectionId).SendAsync("Disconnected", Context.ConnectionId);
        }

        public async Task StartCommunication(string connectionId, string publicKey)
        {
            if (!ExistingConnection.Contains(connectionId))
            {
                await Clients.Caller.SendAsync("ErrorCommunication", $"Unknown connection id {connectionId}");
            }
            else
            {
                Communication[Context.ConnectionId] = new UserAgrs(connectionId, publicKey);

                if (Communication.ContainsKey(connectionId))
                {
                    if (Communication[connectionId].ConnectionId == Context.ConnectionId)
                    {
                        await Clients.Caller.SendAsync("SuccessCommunication",
                            Communication[connectionId].Key);

                        await Clients.Client(connectionId).SendAsync("SuccessCommunication",
                            publicKey);
                    }
                }
                else
                {
                    await Clients.Client(connectionId).SendAsync("RequestStart", Context.ConnectionId);
                }
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (Communication.ContainsKey(Context.ConnectionId))
            {
                var connectionId = Communication[Context.ConnectionId].ConnectionId;

                Clients.Client(connectionId).SendAsync("Disconnected", Context.ConnectionId);

                Communication.TryRemove(Context.ConnectionId, out _);
            }

            ExistingConnection.TryRemove(Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }

        public override Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            ExistingConnection.Add(connectionId);

            return Clients.Caller.SendAsync("Handshake", connectionId);
        }
    }

    public class HubBackgroundWorker : BackgroundService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public HubBackgroundWorker(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _hubContext.Clients.All.SendCoreAsync("ReceiveMessage",
                    new object[] {"System", "Hello from blazor"},
                    stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}