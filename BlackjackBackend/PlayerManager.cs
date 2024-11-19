using System.Collections.Concurrent;
using BlackjackBackend.Models;

namespace BlackjackBackend
{
    public interface IPlayerManager 
    {
        public void AddPlayer(string playerId, Player data);
        public bool RemovePlayer(string playerId);
        public Player? GetPlayerData(string playerId);
    }

    public class PlayerManager : IPlayerManager
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, Player> _connections = new();

        public PlayerManager(ILogger<PlayerManager> logger)
        {
            _logger = logger;
        }

        public void AddPlayer(string playerId, Player data)
        {
            _connections.TryAdd(playerId, data);
            _logger.LogInformation($"playerId {playerId} connected!");
            return;
        }

        public bool RemovePlayer(string playerId)
        {
            bool success = _connections.TryRemove(playerId, out _);
            _logger.LogInformation($"playerId {playerId} removed: {success}");
            return success;
        }

        public Player? GetPlayerData(string playerId)
        {
            _connections.TryGetValue(playerId, out var data);
            return data;
        }
    }
}
