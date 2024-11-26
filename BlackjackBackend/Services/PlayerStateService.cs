using System.Collections.Concurrent;
using BlackjackBackend.Models;

namespace BlackjackBackend.Services
{
    public interface IPlayerStateService
    {
        public Task<bool> AddPlayerAsync(string playerId, Player data);
        public Task<bool> RemovePlayerAsync(string playerId);
        public Task<Player?> GetPlayerDataAsync(string playerId);
        public Task<Player[]> GetAllPlayerDataAsync();
    }

    //This service holds the state of all current players in memory
    public class PlayerStateService : IPlayerStateService
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, Player> _connections = new();

        public PlayerStateService(ILogger<PlayerStateService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> AddPlayerAsync(string playerId, Player data)
        {
            return await Task.Run(() => _connections.TryAdd(playerId, data));
        }

        public async Task<bool> RemovePlayerAsync(string playerId)
        {
            return await Task.Run(() => _connections.TryRemove(playerId, out _));
        }

        public async Task<Player?> GetPlayerDataAsync(string playerId)
        {
            return await Task.Run(() =>
            {
                bool success = _connections.TryGetValue(playerId, out var data);
                return success ? data : null;
            });
        }

        public async Task<Player[]> GetAllPlayerDataAsync()
        {
            return await Task.Run(() => _connections.Values.ToArray<Player>());
        }
    }
}