using BlackjackBackend.Services;
using Microsoft.AspNetCore.SignalR;

namespace BlackjackBackend
{

    //Double check async code in this class (really necessary?)
    public class BlackjackHub : Hub
    {

        private readonly ILogger<BlackjackHub> _logger;
        private readonly IPlayerStateService _playerStateService;

        public BlackjackHub(ILogger<BlackjackHub> logger, IPlayerStateService playerStateService) 
        {
            _logger = logger;
            _playerStateService = playerStateService;
        }

        public override async Task<Task> OnConnectedAsync()
        {
            string playerName = Context.GetHttpContext()?.Request.Query["name"]!;

            if (playerName == null)
            {
                _logger.LogInformation($"Connection {Context.ConnectionId} aborted! No name provided!");
                await Clients.Caller.SendAsync("info", "Connection Closed! No name provided!");

                Context.Abort();
                return Task.CompletedTask;
            }

            await _playerStateService.AddPlayerAsync(Context.ConnectionId, new Models.Player(Context.ConnectionId, playerName));
            _logger.LogInformation($"Connection {Context.ConnectionId} ({playerName}) established!");

            //Send the client it's connectionId so it can identify itself
            await Clients.Caller.SendAsync("localId", Context.ConnectionId);

            await BroadcastPlayerDataAsync();

            return base.OnConnectedAsync();
        }

        public override async Task<Task> OnDisconnectedAsync(Exception? exception)
        {
            await _playerStateService.RemovePlayerAsync(Context.ConnectionId);
            _logger.LogInformation($"Connection {Context.ConnectionId} closed!");

            await BroadcastPlayerDataAsync();

            return base.OnDisconnectedAsync(exception);
        }

        public async Task BroadcastPlayerDataAsync()
        {
            Models.Player[] playerData = await _playerStateService.GetAllPlayerDataAsync();
            await Clients.All.SendAsync("playerData", playerData);
            return;
        }

        public async Task ToggleReady()
        {

            await _playerStateService.TogglePlayerReadyAsync(Context.ConnectionId);
            await BroadcastPlayerDataAsync();
            return;
        }
    }
}
