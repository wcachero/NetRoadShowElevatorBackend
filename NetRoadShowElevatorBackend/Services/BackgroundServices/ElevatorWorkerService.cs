namespace ElevatorMovement.Services.BackgroundServices
{
    public class ElevatorWorkerService: BackgroundService
    {
        private readonly ElevatorSystemService _elevatorSystemService;

        public ElevatorWorkerService(ElevatorSystemService elevatorSystemService)
        {
            _elevatorSystemService = elevatorSystemService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _elevatorSystemService.Step();
                await Task.Delay(10000, stoppingToken); // 10s delay per floor
            }
        }
      

    }
}
