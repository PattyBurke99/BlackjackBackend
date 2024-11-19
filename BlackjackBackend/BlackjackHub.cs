using Microsoft.AspNetCore.SignalR;

namespace BlackjackBackend
{
    public class BlackjackHub : Hub
    {

        private readonly ILogger<BlackjackHub> _logger;
        public BlackjackHub(ILogger<BlackjackHub> logger) 
        {
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation("Connection established!");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Connection closed!");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
