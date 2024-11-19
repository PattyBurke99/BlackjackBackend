using BlackjackBackend.Services;
using Microsoft.AspNetCore.SignalR;

namespace BlackjackBackend
{
    public class BlackjackHub : Hub
    {

        private readonly ILogger<BlackjackHub> _logger;
        private readonly IPlayerService _playerService;

        public BlackjackHub(ILogger<BlackjackHub> logger, IPlayerService playerService) 
        {
            _logger = logger;
            _playerService = playerService;
        }

        public override Task OnConnectedAsync()
        {
            _playerService.AddPlayer(Context.ConnectionId, new Models.Player("Test Name"));
            _logger.LogInformation("Connection established!");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _playerService.RemovePlayer(Context.ConnectionId);
            _logger.LogInformation("Connection closed!");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
