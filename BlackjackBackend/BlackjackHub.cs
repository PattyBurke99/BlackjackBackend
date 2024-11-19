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
            string playerName = Context.GetHttpContext()?.Request.Query["name"]!;

            if (playerName == null)
            {
                _logger.LogInformation($"Connection {Context.ConnectionId} aborted! No name provided!");
                Clients.Caller.SendAsync("ReceiveMessage", "Connection Closed! No name provided!");

                Context.Abort();
                return Task.CompletedTask;
            }

            _playerService.AddPlayer(Context.ConnectionId, new Models.Player(playerName));
            _logger.LogInformation($"Connection {Context.ConnectionId} ({playerName}) established!");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _playerService.RemovePlayer(Context.ConnectionId);
            _logger.LogInformation($"Connection {Context.ConnectionId} closed!");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
