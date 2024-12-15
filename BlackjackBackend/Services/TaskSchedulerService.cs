using Microsoft.AspNetCore.SignalR;

namespace BlackjackBackend.Services
{
    public enum SchedulableTask
    {
        StartDealing,

    }

    public interface ITaskSchedulerService
    {
        public void ScheduleTask(SchedulableTask task);
    }

    public class TaskSchedulerService : ITaskSchedulerService
    {
        //All times in seconds
        static int BettingActionTime = 20;


        private readonly IHubContext<BlackjackHub> _hubContext;
        private readonly ILogger<ITaskSchedulerService> _logger;
        private readonly IGameStateService _gameStateService;

        public TaskSchedulerService(IHubContext<BlackjackHub> hubContext,ILogger<ITaskSchedulerService> logger, IGameStateService gameStateService) 
        {
            _hubContext = hubContext;
            _logger = logger;
            _gameStateService = gameStateService;
        }

        public void ScheduleTask(SchedulableTask task)
        {
            switch (task)
            {
                case SchedulableTask.StartDealing:
                    ScheduleDealingStart();
                    break;
                default:
                    break;
            }
        }

        private void ScheduleDealingStart()
        {
            _ = Task.Run(async () =>
            {
                DateTime? triggeredActionTime = _gameStateService.GetGameState().NextActionTime;
                if (triggeredActionTime == null)
                {
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(BettingActionTime));

                if (_gameStateService.GetGameState().NextActionTime == triggeredActionTime)
                {
                    _gameStateService.DealCards();
                    Models.GameState gameState = _gameStateService.GetGameState();
                    await _hubContext.Clients.All.SendAsync("gameState", gameState);
                }

                return;
            });
        }
    }
}
