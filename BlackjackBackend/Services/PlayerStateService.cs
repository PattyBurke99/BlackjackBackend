using System.Collections.Concurrent;
using BlackjackBackend.Models;

namespace BlackjackBackend.Services
{
    public interface IPlayerStateService
    {
        public Task<object> AddPlayer(string playerId, Player data);
        public Task<bool> RemovePlayer(string playerId);
        public Task<Player?> GetPlayerData(string playerId);
        public Task<Player[]> GetAllPlayerData();
        public Task<object> TogglePlayerReady(string connectionId);
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

        public async Task<object> AddPlayer(string playerId, Player data)
        {
            return await Task.Run(() =>
            {
                _connections.TryAdd(playerId, data);
                return (object)null!;
            });
        }

        public async Task<bool> RemovePlayer(string playerId)
        {
            return await Task.Run(() =>
            {
                return _connections.TryRemove(playerId, out _);
            });
        }

        public async Task<Player?> GetPlayerData(string playerId)
        {
            return await Task.Run(() =>
            {
                _connections.TryGetValue(playerId, out var data);
                return data;
            });
        }

        public async Task<Player[]> GetAllPlayerData()
        {
            return await Task.Run(() =>
            {
                return _connections.Values.ToArray<Player>();
            });
        }

        public async Task<object> TogglePlayerReady(string connectionId)
        {
            return await Task.Run(() =>
            {
                _connections[connectionId].ToggleReady();
                return (object)null!;
            });
        }
    }
}
