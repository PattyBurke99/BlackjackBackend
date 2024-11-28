using System.Collections.Concurrent;
using BlackjackBackend.Models;

namespace BlackjackBackend.Services;

public interface IGameStateService
{
    public Task<GameState> GetGameStateAsync();
    public Task<bool> PlayerSelectSeatAsync(string playerId, string playerName, int seatNum);
    public Task<bool> PlayerLeaveAllSeatsAsync(string playerId);
    public Task<bool> SetCurrentActionAsync(GameAction newAction);
}

public class GameStateService : IGameStateService
{
    private readonly ILogger _logger;

    //Probably need lock for this
    private GameState _currentGameState = new();

    private GameAction _currentAction = new();
    private readonly object _currentActionLock = new object();

    private readonly ConcurrentDictionary<int, SeatData?> _seats = new ConcurrentDictionary<int, SeatData?>();

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
        return await Task.Run(async () => {
            SeatData?[] currentSeats = new SeatData?[6];
            for (int i = 0; i < 6; i++)
            {
                _seats.TryGetValue(i, out currentSeats[i]);
            }

            await _currentGameState.UpdateStateAsync(currentSeats, _currentAction);
            return _currentGameState;
        }); 
    }

    public async Task<bool> PlayerSelectSeatAsync(string playerId, string playerName, int seatNum)
    {
        return await Task.Run(async () => { 
            if (seatNum < 0 || seatNum > 5)
            {
                return false;
            }

            if (_seats[seatNum] != null)
            {
                if (_seats[seatNum]?.Id == playerId)
                {
                    //This is the player already in the seat; than leave
                    bool leftSeat = _seats.TryUpdate(seatNum, null, new SeatData(id: playerId, name: playerName));
                    if (leftSeat && !(await ArePlayersAtTable()))
                    {
                        //No players left at table; set GamePhase to standy
                        await SetCurrentActionAsync(GameAction.Standby);
                    }
                    return leftSeat;
                }
                return false;
            }

            //sit down
            bool satDown = _seats.TryUpdate(seatNum, new SeatData(id: playerId, name: playerName), null);
            if (satDown && _currentAction == GameAction.Standby)
            {
                //Logic for starting game if it is not in progress goes here
                await SetCurrentActionAsync(GameAction.Betting);
            }
            return satDown;
        });
    }

    //Called on player disconnect
    public async Task<bool> PlayerLeaveAllSeatsAsync(string playerId)
    {
        return await Task.Run(async () =>
        {
            bool changesMade = false;
            for (int i = 0; i < 6; i++)
            {
                bool updated = _seats.TryUpdate(i, null, new SeatData(id: playerId, name: ""));
                if (updated)
                {
                    changesMade = true;
                    if (!(await ArePlayersAtTable()))
                    {
                        //Logic for canceling game here; No players left at table
                        await SetCurrentActionAsync(GameAction.Standby);
                    }
                }
            }

            return changesMade;
        });
    }

    public async Task<bool> SetCurrentActionAsync(GameAction newAction)
    {
        return await Task.Run(() =>
        {

            if (Monitor.TryEnter(_currentActionLock))
            {
                try
                {
                    _currentAction = newAction;
                    return true;
                }
                finally
                {
                    Monitor.Exit(_currentActionLock);
                }
            }
            else
            {
                return false;
            }
        });
    }

    //returns true if more than 1 player at table
    private async Task<bool> ArePlayersAtTable()
    {
        return await Task.Run(() =>
        {
            foreach (SeatData? seat in _seats.Values)
            {
                if (seat != null)
                {
                    return true;
                }
            }

            return false;
        });
    }

}