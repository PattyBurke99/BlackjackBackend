using System.Collections.Concurrent;
using BlackjackBackend.Models;

namespace BlackjackBackend.Services
{
    public interface IPlayerStateService
    {
        public void AddPlayer(string playerId, Player data);
        public bool RemovePlayer(string playerId);
        public Player? GetPlayerData(string playerId);
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

        public void AddPlayer(string playerId, Player data)
        {
            _connections.TryAdd(playerId, data);
            //_logger.LogInformation($"playerId {playerId} connected!");
            return;
        }

        public bool RemovePlayer(string playerId)
        {
            bool success = _connections.TryRemove(playerId, out _);
            //_logger.LogInformation($"playerId {playerId} removed: {success}");
            return success;
        }

        public Player? GetPlayerData(string playerId)
        {
            _connections.TryGetValue(playerId, out var data);
            return data;
        }
    }
}
