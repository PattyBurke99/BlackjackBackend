using System.Collections.Concurrent;
using BlackjackBackend.Models;

namespace BlackjackBackend.Services
{
    public interface IPlayerStateService
    {
        public Task AddPlayerAsync(string playerId, Player data);
        public Task<bool> RemovePlayerAsync(string playerId);
        public Task<Player?> GetPlayerDataAsync(string playerId);
        public Task<Player[]> GetAllPlayerDataAsync();
        public Task TogglePlayerReadyAsync(string connectionId);
    }

    //This service holds the state of all current players in memory (I'm too poor for a database :( )
    public class PlayerStateService : IPlayerStateService
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, Player> _connections = new();

        public PlayerStateService(ILogger<PlayerStateService> logger)
        {
            _logger = logger;
        }

        public async Task AddPlayerAsync(string playerId, Player data)
        {
            _connections.TryAdd(playerId, data);
            return;
        }

        public async Task<bool> RemovePlayerAsync(string playerId)
        {
            return _connections.TryRemove(playerId, out _);
        }

        public async Task<Player?> GetPlayerDataAsync(string playerId)
        {
            bool success = _connections.TryGetValue(playerId, out var data);
            if (success)
            {
                return data;
            }
            else
            {
                return null;
            }
        }

        public async Task<Player[]> GetAllPlayerDataAsync()
        {
            return _connections.Values.ToArray<Player>();
        }

        public async Task TogglePlayerReadyAsync(string connectionId)
        {
            _connections[connectionId].ToggleReady();
            return;
        }
    }
}
