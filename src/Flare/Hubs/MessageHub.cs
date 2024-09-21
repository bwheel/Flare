using Microsoft.AspNetCore.SignalR;

namespace Flare.Hubs
{
    public class MessageHub : Hub
    {
        private readonly ILogger<MessageHub> m_logger;

        public MessageHub(ILogger<MessageHub> logger)
        {
            m_logger = logger;
            
        }
        public async Task SendMessage(string message)
        {
            m_logger.LogInformation("Send message: {message}", message);
            await Clients.All.SendAsync("SendMessage", message);
        }
    }
}
