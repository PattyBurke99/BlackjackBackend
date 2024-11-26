using System.Collections.Concurrent;
using BlackjackBackend.Models;

namespace BlackjackBackend.Services;

public interface IGameStateService
{
    public Task<GameState> GetGameStateAsync();
    public Task<bool> PlayerTakeSeat(string playerId, string playerName, int seatNum);
    //public Task<bool> PlayerLeaveSeat(string playerId, string playerName, int seatNum);
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

            //_logger.LogInformation($"currentSeats[0]: ({currentSeats[0].id}, {currentSeats[0].name}) ");

            _currentGameState.UpdateState(currentSeats);

            //_logger.LogInformation($"_currentGameState.Seats[0]: ({_currentGameState.Seats[0].id}, {_currentGameState.Seats[0].name}) ");

            return _currentGameState;
        });
    }

    //Returns true if seat can be taken, false if not
    public async Task<bool> PlayerTakeSeat(string playerId, string playerName, int seatNum)
    {
        if (seatNum < 0 || seatNum > 5)
        {
            return false;
        }

        if (_seats[seatNum] != null)
        {
            return false;
        }

        return await Task.Run(() => _seats.TryUpdate(seatNum, new SeatData(id: playerId, name: playerName), null));
    }

    //public async Task<bool> PlayerLeaveSeat(string playerId, string playerName, int seatNum)
    //{
    //    if (seatNum < 0 || seatNum > 5)
    //    {
    //        return false;
    //    }

    //    if (_seats[seatNum].id == null)
    //    {
    //        return false;
    //    }

    //    return await Task.Run(() => _seats.TryUpdate(seatNum, (null, null), (id: playerId, name: playerName)));
    //}
}