namespace HandyHero.Hub
{
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        public async Task SendMessage(int chatId, string user, string message)
        {
            _logger.LogInformation($"SendMessage called by {user} in chat {chatId} with message: {message}");

            try
            {
                // Send the message to clients within the specified chat
                await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", user, message);
                _logger.LogInformation("Message sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                throw; // Rethrow the exception to notify the client
            }
        }

    }
}
