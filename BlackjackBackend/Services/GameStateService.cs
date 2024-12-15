using System.Collections.Concurrent;
using BlackjackBackend.Models;

namespace BlackjackBackend.Services;

public interface IGameStateService
{
    public GameState GetGameState();
    public bool PlayerSelectSeat(string playerId, string playerName, int seatNum);
    public bool PlayerLeaveAllSeats(string playerId);
    public bool ChangeBet(string playerId, int change, int seatNum);
}

public class GameStateService : IGameStateService
{
    private readonly ILogger _logger;
    private readonly IPlayerStateService _playerStateService;

    //Probably need lock for this
    private GameState _currentGameState = new();

    private GameAction _currentAction = new();
    private readonly object _currentActionLock = new object();

    private readonly ConcurrentDictionary<int, SeatData?> _seats = new ConcurrentDictionary<int, SeatData?>();

    public GameStateService(ILogger<GameStateService> logger, IPlayerStateService playerStateService)
    {
        _logger = logger;
        _playerStateService = playerStateService;

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

        _seats.TryGetValue(seatNum, out SeatData? seatData);

        if (seatData != null)
        {
            if (seatData.Id == playerId)
            {
                _logger.LogInformation($"Player leaving seat... Bet: {seatData.Bet}");
                if (seatData.Bet > 0)
                {
                    _playerStateService.PlayerUpdateMoney(playerId, seatData.Bet);
                }

                //This is the player already in the seat; than leave
                bool leftSeat = _seats.TryUpdate(seatNum, null, new SeatData(id: playerId, name: playerName));
                if (leftSeat && !ArePlayersAtTable())
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

    public bool ChangeBet(string playerId, int change, int seatNum)
    {
        if (GetCurrentAction() != GameAction.Betting)
        {
            return false;
        }

        _seats.TryGetValue(seatNum, out SeatData? playerSeatData);

        if (playerSeatData == null || playerSeatData.Id != playerId)
        {
            return false;
        }

        Player? player = _playerStateService.GetPlayer(playerId);
        if (player == null)
        {
            return false;
        }

        if ((change > 0 && player.Money - change < 0) || (change < 0 && playerSeatData.Bet + change < 0))
        {
            return false;
        }

        bool updatedMoney = _playerStateService.PlayerUpdateMoney(playerId, -change);
        if (!updatedMoney)
        {
            _logger.LogError("GameStateServices/ChangeBet(): Update money failed! (Race condition?)");
            return false;
        }

        SeatData newValue = new SeatData(playerSeatData.Id, playerSeatData.Name, playerSeatData.Bet + change);
        return _seats.TryUpdate(seatNum, newValue, playerSeatData);
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

    private GameAction? GetCurrentAction()
    {
        if (Monitor.TryEnter(_currentActionLock))
        {
            try
            {
                return _currentAction;
            }
            finally
            {
                Monitor.Exit(_currentActionLock);
            }
        }
        else
        {
            return null;
        }
    }

    private bool SetCurrentAction(GameAction newAction)
    {
        //Possible error case; should probably retry to obtain lock if it fails (?)
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
}