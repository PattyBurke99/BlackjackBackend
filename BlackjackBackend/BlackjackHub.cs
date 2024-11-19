using Microsoft.AspNetCore.SignalR;

namespace BlackjackBackend
{
    public class BlackjackHub : Hub
    {

        private readonly ILogger<BlackjackHub> _logger;
        private readonly IPlayerManager _playerManager;

        public BlackjackHub(ILogger<BlackjackHub> logger, IPlayerManager playerManager) 
        {
            _logger = logger;
            _playerManager = playerManager;
        }

        public override Task OnConnectedAsync()
        {
            _playerManager.AddPlayer(Context.ConnectionId, new Models.Player("Test Name"));
            _logger.LogInformation("Connection established!");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _playerManager.RemovePlayer(Context.ConnectionId);
            _logger.LogInformation("Connection closed!");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
