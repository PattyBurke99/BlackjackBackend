using BlackjackBackend.Services;
using Microsoft.AspNetCore.SignalR;

namespace BlackjackBackend
{
    public class BlackjackHub : Hub
    {

        private readonly ILogger<BlackjackHub> _logger;
        private readonly IPlayerStateService _playerStateService;

        public BlackjackHub(ILogger<BlackjackHub> logger, IPlayerStateService playerStateService) 
        {
            _logger = logger;
            _playerStateService = playerStateService;
        }

        public override Task OnConnectedAsync()
        {
            string playerName = Context.GetHttpContext()?.Request.Query["name"]!;

            if (playerName == null)
            {
                _logger.LogInformation($"Connection {Context.ConnectionId} aborted! No name provided!");
                Clients.Caller.SendAsync("ReceiveMessage", "Connection Closed! No name provided!");

                Context.Abort();
                return Task.CompletedTask;
            }

            _playerStateService.AddPlayer(Context.ConnectionId, new Models.Player(playerName));
            _logger.LogInformation($"Connection {Context.ConnectionId} ({playerName}) established!");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _playerStateService.RemovePlayer(Context.ConnectionId);
            _logger.LogInformation($"Connection {Context.ConnectionId} closed!");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
