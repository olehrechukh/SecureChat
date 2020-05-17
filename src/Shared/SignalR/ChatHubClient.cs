using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chat.Shared.SignalR
{
    public class UserAgrs
    {
        public UserAgrs(string connectionId, string key)
        {
            ConnectionId = connectionId;
            Key = key;
        }

        public string Key { get; }
        public string ConnectionId { get; }
    }

    public class ChatHubClient : IAsyncDisposable
    {
        public event EventHandler<HubConnectionState> StateWasChanged;
        public event EventHandler<MessageReceived> MessageReceived;
        public event EventHandler<string> HandshakeReceived;
        public event EventHandler<string> DisconnectReceived;
        public event EventHandler<string> ErrorCommunicationReceived;
        public event EventHandler<string> SuccessCommunicationReceived;
        public event EventHandler<string> RequestStartReceived;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
        public HubConnectionState State => _hubConnection.State;

        private readonly HubConnection _hubConnection;

        public ChatHubClient(HubConnection hubConnection)
        {
            _hubConnection = hubConnection;
        }

        public Task StartAsync()
        {
            _hubConnection.On<string, string>("ReceiveMessage", HandleMessage);
            _hubConnection.On<string>("Handshake", HandleHandshake);
            _hubConnection.On<string>("Disconnected", HandleDisconnected);
            _hubConnection.On<string>("ErrorCommunication", s => ErrorCommunicationReceived?.Invoke(this, s));
            _hubConnection.On<string>("SuccessCommunication", s => SuccessCommunicationReceived?.Invoke(this, s));
            _hubConnection.On<string>("RequestStart", s => RequestStartReceived?.Invoke(this, s));
            _hubConnection.Closed += HubConnectionOnClosed;
            _hubConnection.Reconnected += HubConnectionOnReconnected;
            _hubConnection.Reconnecting += HubConnectionOnReconnecting;

            return _hubConnection.StartAsync();
        }

        public Task SendMessage(string message) => _hubConnection.SendAsync("SendMessage", message);

        public Task StartCommunication(string connectionId, string publicKey) =>
            _hubConnection.SendAsync("StartCommunication", connectionId, publicKey);

        public Task Disconnect(string connectionId) => _hubConnection.SendAsync("Disconnect", connectionId);

        private Task HubConnectionOnReconnecting(Exception arg) => ChangeStatus();
        private Task HubConnectionOnReconnected(string arg) => ChangeStatus();
        private Task HubConnectionOnClosed(Exception arg) => ChangeStatus();
        private void HandleDisconnected(string obj) => DisconnectReceived?.Invoke(this, obj);

        private Task ChangeStatus()
        {
            StateWasChanged?.Invoke(this, State);
            return Task.CompletedTask;
        }

        private void HandleHandshake(string connectionId) => HandshakeReceived?.Invoke(this, connectionId);

        private void HandleMessage(string user, string message) =>
            MessageReceived?.Invoke(this, new MessageReceived(user, message));


        public async ValueTask DisposeAsync()
        {
            _hubConnection.Closed -= HubConnectionOnClosed;
            _hubConnection.Reconnected -= HubConnectionOnReconnected;
            _hubConnection.Reconnecting -= HubConnectionOnReconnecting;
            
            await _hubConnection.DisposeAsync();
        }
    }
}