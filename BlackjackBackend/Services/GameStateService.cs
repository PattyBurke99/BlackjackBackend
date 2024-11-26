using System.Collections.Concurrent;
using BlackjackBackend.Models;

namespace BlackjackBackend.Services;

public interface IGameStateService
{
    public Task<GameState> GetGameStateAsync();
    public Task<bool> PlayerSelectSeat(string playerId, string playerName, int seatNum);
    public Task<bool> PlayerLeaveAllSeats(string playerId);
}

public class GameStateService : IGameStateService
{
    private readonly ILogger _logger;
    public readonly ConcurrentDictionary<int, SeatData?> _seats = new ConcurrentDictionary<int, SeatData?>();
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
            SeatData?[] currentSeats = new SeatData?[6];
            for (int i = 0; i < 6; i++)
            {
                _seats.TryGetValue(i, out currentSeats[i]);
            }

            _currentGameState.UpdateState(currentSeats);

            return _currentGameState;
        });
    }

    public async Task<bool> PlayerSelectSeat(string playerId, string playerName, int seatNum)
    {
        if (seatNum < 0 || seatNum > 5)
        {
            return false;
        }

        if (_seats[seatNum] != null)
        {
            if (_seats[seatNum]?.Id == playerId)
            {
                //This is the player already in the seat; than leave
                _logger.LogInformation($"{playerName} Trying to leave!");
                return await Task.Run(() => _seats.TryUpdate(seatNum, null, new SeatData(id: playerId, name: playerName)));
            }
            return false;
        }

        //sit down
        return await Task.Run(() => _seats.TryUpdate(seatNum, new SeatData(id: playerId, name: playerName), null));
    }

    public async Task<bool> PlayerLeaveAllSeats(string playerId)
    {
        return await Task.Run(() =>
        {
            bool changesMade = false;
            for (int i = 0; i < 6; i++)
            {
                bool updated = _seats.TryUpdate(i, null, new SeatData(id: playerId, name: ""));
                if (updated)
                {
                    changesMade = true;
                }
            }

            return changesMade;
        });
    }
}