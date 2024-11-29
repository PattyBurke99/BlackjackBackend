using System.Collections.Concurrent;
using BlackjackBackend.Models;

namespace BlackjackBackend.Services
{
    public interface IPlayerStateService
    {
        public bool AddPlayer(string playerId, Player data);
        public bool RemovePlayer(string playerId);
        public Player? GetPlayer(string playerId);
        public Player[] GetAllPlayers();
        public bool PlayerUpdateMoney(string playerId, int change);
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

        public Player? GetPlayer(string playerId)
        {
            bool success = _connections.TryGetValue(playerId, out var data);
            return success ? data : null;
        }

        public Player[] GetAllPlayers()
        {
            return _connections.Values.ToArray<Player>();
        }

        public bool PlayerUpdateMoney(string playerId, int change)
        {
            Player? player = GetPlayer(playerId);
            if (player == null)
            {
                return false;
            }

            Player newValue = new Player(playerId, player.Name, player.Money + change);
            return _connections.TryUpdate(playerId, newValue, player);
        }
    }
}