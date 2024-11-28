using System.Collections.Concurrent;
using BlackjackBackend.Models;

namespace BlackjackBackend.Services
{
    public interface IPlayerStateService
    {
        public bool AddPlayer(string playerId, Player data);
        public bool RemovePlayer(string playerId);
        public Player? GetPlayerData(string playerId);
        public Player[] GetAllPlayerData();
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

        public bool AddPlayer(string playerId, Player data)
        {
            return _connections.TryAdd(playerId, data);
        }

        public bool RemovePlayer(string playerId)
        {
            return _connections.TryRemove(playerId, out _);
        }

        public Player? GetPlayerData(string playerId)
        {
            bool success = _connections.TryGetValue(playerId, out var data);
            return success ? data : null;
        }

        public Player[] GetAllPlayerData()
        {
            return _connections.Values.ToArray<Player>();
        }
    }
}