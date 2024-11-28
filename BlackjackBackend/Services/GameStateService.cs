using System.Collections.Concurrent;
using BlackjackBackend.Models;

namespace BlackjackBackend.Services;

public interface IGameStateService
{
    public GameState GetGameState();
    public bool PlayerSelectSeat(string playerId, string playerName, int seatNum);
    public bool PlayerLeaveAllSeats(string playerId);
    public bool SetCurrentAction(GameAction newAction);
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
    public GameState GetGameState()
    {
        SeatData?[] currentSeats = new SeatData?[6];
        for (int i = 0; i < 6; i++)
        {
            _seats.TryGetValue(i, out currentSeats[i]);
        }

        _currentGameState.UpdateState(currentSeats, _currentAction);
        return _currentGameState;
    }

    public bool PlayerSelectSeat(string playerId, string playerName, int seatNum)
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
                bool leftSeat = _seats.TryUpdate(seatNum, null, new SeatData(id: playerId, name: playerName));
                if (leftSeat && ArePlayersAtTable())
                {
                    //No players left at table; set GamePhase to standy
                    SetCurrentAction(GameAction.Standby);
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
            SetCurrentAction(GameAction.Betting);
        }
        return satDown;
    }

    //Called on player disconnect
    public bool PlayerLeaveAllSeats(string playerId)
    {
        bool changesMade = false;
        for (int i = 0; i < 6; i++)
        {
            bool updated = _seats.TryUpdate(i, null, new SeatData(id: playerId, name: ""));
            if (updated)
            {
                changesMade = true;
                if (!ArePlayersAtTable())
                {
                    //Logic for canceling game here; No players left at table
                    SetCurrentAction(GameAction.Standby);
                }
            }
        }

        return changesMade;
    }

    public bool SetCurrentAction(GameAction newAction)
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
    }

    //returns true if more than 1 player at table
    private bool ArePlayersAtTable()
    {
        foreach (SeatData? seat in _seats.Values)
        {
            if (seat != null)
            {
                return true;
            }
        }

        return false;
    }

}