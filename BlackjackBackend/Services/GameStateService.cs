using System.Collections.Concurrent;
using BlackjackBackend.Models;

namespace BlackjackBackend.Services;

public interface IGameStateService
{
    public Task<GameState> GetGameStateAsync();
    public Task<bool> PlayerTakeSeat(string playerId, int seatNum);
    public Task<bool> PlayerLeaveSeat(string platerId, int seatNum);
}

public class GameStateService : IGameStateService
{
    private readonly ILogger _logger;
    public readonly ConcurrentDictionary<int, string?> _seats = new ConcurrentDictionary<int, string?>();
    private GameState _currentGameState = new();

    public GameStateService(ILogger<GameStateService> logger)
    {
        _logger = logger;

        // Initialize empty seats
        for (int i = 0; i < 6; i++)
        {
            _seats[i] = null;
        }
    }

    //Convert current state held within class into object for return
    // !! Does this need additional thread safety?
    public async Task<GameState> GetGameStateAsync()
    {
        return await Task.Run(() => {
            string?[] currentSeats = new string[6];
            for (int i = 0; i < 6; i++)
            {
                _seats.TryGetValue(i, out currentSeats[i]);
            }

            _currentGameState.UpdateState(currentSeats);

            return _currentGameState;
        });
    }

    //Returns true if seat can be taken, false if not
    public async Task<bool> PlayerTakeSeat(string playerId, int seatNum)
    {
        if (seatNum < 0 || seatNum > 5)
        {
            return false;
        }

        if (_seats[seatNum] != null)
        {
            return false;
        }

        return await Task.Run(() => _seats.TryUpdate(seatNum, playerId, null));
    }

    public async Task<bool> PlayerLeaveSeat(string playerId, int seatNum)
    {
        if (seatNum < 0 || seatNum > 5)
        {
            return false;
        }

        if (_seats[seatNum] == null)
        {
            return false;
        }

        return await Task.Run(() => _seats.TryUpdate(seatNum, null, playerId));
    }
}
