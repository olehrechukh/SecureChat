namespace Chat.Shared.SignalR
{
    public class MessageReceived
    {
        public string User { get; }
        public string Message { get; }

        public MessageReceived(string user, string message)
        {
            User = user;
            Message = message;
        }

        public void Deconstruct(out string user, out string message)
        {
            user = User;
            message = Message;
        }
    }
}