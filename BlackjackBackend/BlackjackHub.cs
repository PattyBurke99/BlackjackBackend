using BlackjackBackend.Services;
using Microsoft.AspNetCore.SignalR;

namespace BlackjackBackend
{

    public class BlackjackHub : Hub
    {

        private readonly ILogger<BlackjackHub> _logger;
        private readonly IPlayerStateService _playerStateService;
        private readonly IGameStateService _gameStateService;

        public BlackjackHub(ILogger<BlackjackHub> logger, IPlayerStateService playerStateService, IGameStateService gameStateService)
        {
            _logger = logger;
            _playerStateService = playerStateService;
            _gameStateService = gameStateService;
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

            bool success = await _playerStateService.AddPlayerAsync(Context.ConnectionId, new Models.Player(Context.ConnectionId, playerName));
            if (success)
            {
                _logger.LogInformation($"Connection {Context.ConnectionId} ({playerName}) established!");
                await Clients.Caller.SendAsync("localId", Context.ConnectionId);
                await BroadcastPlayerDataAsync();
            } else
            {
                _logger.LogInformation($"Error adding new player {playerName} ({Context.ConnectionId})! Aborting connection");
                Context.Abort();
                return Task.CompletedTask;
            }

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

        public async Task BroadcastGameDataAsync()
        {
            var gameState = await _gameStateService.GetGameStateAsync();
            await Clients.All.SendAsync("gameState", gameState);
            return;
        }

        public async Task<bool> TakeSeat(int seatNum)
        {
            bool success = await _gameStateService.PlayerTakeSeat(Context.ConnectionId, seatNum);
            if (success)
            {
                await BroadcastGameDataAsync();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}